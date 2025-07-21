using AnnoDesigner.Core;
using AnnoDesigner.Core.DataStructures;
using AnnoDesigner.Core.Extensions;
using AnnoDesigner.Core.Helper;
using AnnoDesigner.Core.Layout;
using AnnoDesigner.Core.Layout.Helper;
using AnnoDesigner.Core.Layout.Models;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Core.Presets.Loader;
using AnnoDesigner.Core.Presets.Models;
using AnnoDesigner.Core.Services;
using AnnoDesigner.CustomEventArgs;
using AnnoDesigner.Extensions;
using AnnoDesigner.Helper;
using AnnoDesigner.Models;
using AnnoDesigner.Services;
using AnnoDesigner.Undo;
using AnnoDesigner.Undo.Operations;
using AnnoDesigner.ViewModels;
using Microsoft.Win32;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace AnnoDesigner;

/// <summary>
/// Interaction logic for AnnoCanvas.xaml
/// </summary>
public partial class AnnoCanvas : UserControl, IAnnoCanvas, IHotkeySource, IScrollInfo
{
    private readonly IFileSystem _fileSystem;
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    //Important: These match values in the translations dictionary (e.g "Rotate" matches "Rotate" in the localization dictionary)
    public const string ROTATE_LOCALIZATION_KEY = "Rotate";
    public const string ROTATE_ALL_LOCALIZATION_KEY = "RotateAll";
    public const string COPY_LOCALIZATION_KEY = "Copy";
    public const string PASTE_LOCALIZATION_KEY = "Paste";
    public const string DELETE_LOCALIZATION_KEY = "Delete";
    public const string DUPLICATE_LOCALIZATION_KEY = "Duplicate";
    public const string DELETE_OBJECT_UNDER_CURSOR_LOCALIZATION_KEY = "DeleteObjectUnderCursor";
    public const string UNDO_LOCALIZATION_KEY = "Undo";
    public const string REDO_LOCALIZATION_KEY = "Redo";
    public const string ENABLE_DEBUG_MODE_LOCALIZATION_KEY = "EnableDebugMode";
    public const string SELECT_ALL_SAME_IDENTIFIER_LOCALIZATION_KEY = "SelectAllSameIdentifier";

    public event EventHandler<UpdateStatisticsEventArgs> StatisticsUpdated;
    public event EventHandler<EventArgs> ColorsInLayoutUpdated;
    /// <summary>
    /// Event which is fired when the status message should be changed.
    /// </summary>
    public event EventHandler<FileLoadedEventArgs> OnLoadedFileChanged;
    public event EventHandler<OpenFileEventArgs> OpenFileRequested;
    public event EventHandler<SaveFileEventArgs> SaveFileRequested;

    #region Properties

    public IUndoManager UndoManager { get; private set; }

    public IClipboardService ClipboardService { get; set; }

    /// <summary>
    /// Contains all loaded icons as a mapping of name (the filename without extension) to loaded BitmapImage.
    /// </summary>
    public Dictionary<string, IconImage> Icons { get; }

    public BuildingPresets BuildingPresets { get; }

    /// <summary>
    /// Backing field of the GridSize property.
    /// </summary>
    private int _gridSize = Constants.GridStepDefault;

    /// <summary>
    /// Gets or sets the width of the grid cells.
    /// Increasing the grid size results in zooming in and vice versa.
    /// </summary>
    public int GridSize
    {
        get => _gridSize;
        set
        {
            int tmp = value;

            if (tmp < Constants.GridStepMin)
            {
                tmp = Constants.GridStepMin;
            }
            else if (tmp > Constants.GridStepMax)
            {
                tmp = Constants.GridStepMax;
            }

            if (_gridSize != tmp)
            {
                _gridSize = tmp;
                InvalidateVisual();
            }
        }
    }

    /// <summary>
    /// Backing field of the RenderGrid property.
    /// </summary>
    private bool _renderGrid;

    /// <summary>
    /// Gets or sets a value indicating whether the grid should be rendered.
    /// </summary>
    public bool RenderGrid
    {
        get => _renderGrid;
        set
        {
            if (_renderGrid != value)
            {
                _renderGrid = value;
                _isRenderingForced = true;
                InvalidateVisual();
            }
        }
    }

    /// <summary>
    /// Backing field of the RenderInfluences property.
    /// </summary>
    private bool _renderInfluences;

    /// <summary>
    /// Gets or sets a value indicating whether the influences should be rendered.
    /// </summary>
    public bool RenderInfluences
    {
        get => _renderInfluences;
        set
        {
            if (_renderInfluences != value)
            {
                _renderInfluences = value;
                _isRenderingForced = true;
                InvalidateVisual();
            }
        }
    }

    /// <summary>
    /// Backing field of the RenderLabel property.
    /// </summary>
    private bool _renderLabel;

    /// <summary>
    /// Gets or sets a value indicating whether the labels of objects should be rendered.
    /// </summary>
    public bool RenderLabel
    {
        get => _renderLabel;
        set
        {
            if (_renderLabel != value)
            {
                _renderLabel = value;
                _isRenderingForced = true;
                InvalidateVisual();
            }
        }
    }

    /// <summary>
    /// Backing field of the RenderIcon property.
    /// </summary>
    private bool _renderIcon;

    /// <summary>
    /// Gets or sets a value indicating whether the icons of objects should be rendered.
    /// </summary>
    public bool RenderIcon
    {
        get => _renderIcon;
        set
        {
            if (_renderIcon != value)
            {
                _renderIcon = value;
                _isRenderingForced = true;
                InvalidateVisual();
            }
        }
    }

    /// <summary>
    /// Backing field of the RenderTrueInfluenceRange property.
    /// </summary>
    private bool _renderTrueInfluenceRange;

    /// <summary>
    /// Gets or sets a value indicating whether the influence range should be calculated from roads present in the grid.
    /// </summary>
    public bool RenderTrueInfluenceRange
    {
        get => _renderTrueInfluenceRange;
        set
        {
            if (_renderTrueInfluenceRange != value)
            {
                _renderTrueInfluenceRange = value;
                _isRenderingForced = true;
                InvalidateVisual();
            }
        }
    }

    /// <summary>
    /// Backing field of the RenderHarborBlockedArea property.
    /// </summary>
    private bool _renderHarborBlockedArea;

    /// <summary>
    /// Gets or sets value indication whether the blocked harbor aread should be rendered.
    /// </summary>
    public bool RenderHarborBlockedArea
    {
        get => _renderHarborBlockedArea;
        set
        {
            if (_renderHarborBlockedArea != value)
            {
                _renderHarborBlockedArea = value;
                _isRenderingForced = true;
                InvalidateVisual();
            }
        }
    }

    /// <summary>
    /// Backing field of the RenderPanorama property.
    /// </summary>
    private bool _renderPanorama;

    /// <summary>
    /// Gets or sets a value indicating whether the skyscraper panorama should be visible.
    /// </summary>
    public bool RenderPanorama
    {
        get => _renderPanorama;
        set
        {
            if (_renderPanorama != value)
            {
                _renderPanorama = value;
                _isRenderingForced = true;
                InvalidateVisual();
            }
        }
    }

    /// <summary>
    /// Backing field of the CurrentObject property
    /// </summary>
    private List<LayoutObject> _currentObjects = [];

    /// <summary>
    /// Current object to be placed. Fires an event when changed.
    /// </summary>
    public List<LayoutObject> CurrentObjects
    {
        get => _currentObjects;
        private set
        {
            if (_currentObjects != value)
            {
                _currentObjects = value;
                if (value.Count != 0)
                {
                    OnCurrentObjectChanged?.Invoke(value[0]);
                }
            }
        }
    }

    /// <summary>
    /// List of all currently placed objects.
    /// </summary>
    public QuadTree<LayoutObject> PlacedObjects { get; set; }

    /// <summary>
    /// List of all currently selected objects.
    /// All of them must also be contained in the _placedObjects list.
    /// </summary>
    public HashSet<LayoutObject> SelectedObjects { get; set; }

    /// <summary>
    /// Event which is fired when the current object is changed
    /// </summary>
    public event Action<LayoutObject> OnCurrentObjectChanged;

    /// <summary>
    /// Backing field of the StatusMessage property.
    /// </summary>
    private string _statusMessage;

    /// <summary>
    /// Current status message.
    /// </summary>
    public string StatusMessage
    {
        get => _statusMessage;
        private set
        {
            if (_statusMessage != value)
            {
                _statusMessage = value;
                OnStatusMessageChanged?.Invoke(value);
            }
        }
    }

    /// <summary>
    /// Event which is fired when the status message has been changed.
    /// </summary>
    public event Action<string> OnStatusMessageChanged;

    /// <summary>
    /// Backing field of the LoadedFile property.
    /// </summary>
    private string _loadedFile;

    /// <summary>
    /// Last loaded file, i.e. the currently active file. Fire an event when changed.
    /// </summary>
    public string LoadedFile
    {
        get => _loadedFile;
        set
        {
            if (_loadedFile != value)
            {
                _loadedFile = value;
                OnLoadedFileChanged?.Invoke(this, new FileLoadedEventArgs(value));
            }
        }
    }

    #endregion

    #region Privates and constructor

    #region private members
    private const int DPI_FACTOR = 1;

    private readonly ILayoutLoader _layoutLoader;
    private readonly ICoordinateHelper _coordinateHelper;
    private readonly IAppSettings _appSettings;
    private readonly IBrushCache _brushCache;
    private readonly IPenCache _penCache;
    private readonly IMessageBoxService _messageBoxService;
    private readonly ILocalizationHelper _localizationHelper;

    private const string IDENTIFIER_SKYSCRAPER = "A7_residence_SkyScraper_";
    private readonly Regex _regex_panorama = GenerateSkyScraperRegex();//RegexOptions.IgnoreCase -> slow in < .NET 5 (triggers several calls to ToLower)

    /// <summary>
    /// States the mode of mouse interaction.
    /// </summary>
    private enum MouseMode
    {
        // used if not dragging
        Standard,
        // used to drag the selection rect
        SelectionRectStart,
        SelectionRect,
        // used to drag objects around
        DragSelectionStart,
        DragSingleStart,
        DragSelection,
        DragAllStart,
        DragAll,
        PlaceObjects,
        DeleteObject,
        SelectSameIdentifier
    }

    /// <summary>
    /// Backing field of the CurrentMode property.
    /// </summary>
    private MouseMode _currentMode;

    /// <summary>
    /// Indicates the current mouse mode.
    /// </summary>
    private MouseMode CurrentMode
    {
        get => _currentMode;
        set
        {
            _currentMode = value;
            StatusMessage = "Mode: " + _currentMode;
        }
    }

    /// <summary>
    /// Indicates whether the mouse is within this control.
    /// </summary>
    private bool _mouseWithinControl;

    /// <summary>
    /// The current mouse position.
    /// </summary>
    private Point _mousePosition = new(double.NaN, double.NaN);

    /// <summary>
    /// The position where the mouse button was pressed.
    /// </summary>
    private Point _mouseDragStart;

    /// <summary>
    /// The rectangle used for selection.
    /// </summary>
    private Rect _selectionRect;

    /// <summary>
    /// A list of object position <see cref="Rect"/>s. Used when dragging selected objects (when MouseMode is <see cref="MouseMode.DragSelection"/>).
    /// Holds a Rect that represents the object's previous position prior to dragging.
    /// </summary>
    private readonly List<(LayoutObject Item, Rect OldGridRect)> _oldObjectPositions;

    /// <summary>
    /// The collision rect derived from the current selection.
    /// </summary>
    private Rect _collisionRect;

    /// <summary>
    /// Calculation helper used when computing the <see cref="_collisionRect"/>.
    /// </summary>
    private readonly StatisticsCalculationHelper _statisticsCalculationHelper;

    /// <summary>
    /// The current viewport.
    /// </summary>
    private readonly Viewport _viewport;

    /// <summary>
    /// A transform used to translate items within the viewport.
    /// </summary>
    private readonly TranslateTransform _viewportTransform;

    /// <summary>
    /// A guideline set used for pixel-aligned drawing.
    /// </summary>
    private GuidelineSet _guidelineSet;

    /// <summary>
    /// A flag representing if <see cref="ScrollViewer.InvalidateScrollInfo"/> needs to be called on the next render.
    /// </summary>
    private bool _invalidateScrollInfo;

    /// <summary>
    /// A Rect representing the true space the current layout takes up.
    /// </summary>
    private Rect _layoutBounds;

    /// <summary>
    /// A Rect representing the scrollable area of the canvas.
    /// </summary>
    private Rect _scrollableBounds;

    /// <summary>
    /// A Size representing the area the AnnoCanvas control is currently allowed to take up.
    /// </summary>
    private Size _oldArrangeBounds;

    /// <summary>
    /// The typeface used when rendering text on the canvas.
    /// </summary>
    private readonly Typeface TYPEFACE = new("Verdana");

    /// <summary>
    /// Does currently selected objects contain object which is not ignored from rendering?
    /// </summary>
    private bool selectionContainsNotIgnoredObject;

    #endregion

    #region Pens and Brushes

    /// <summary>
    /// Used for object borders.
    /// </summary>
    private Pen _linePen;

    /// <summary>
    /// Used for grid lines.
    /// </summary>
    private Pen _gridLinePen;

    public double LinePenThickness => _linePen.Thickness;

    /// <summary>
    /// Used for selection and hover highlights and selection rect.
    /// </summary>
    private readonly Pen _highlightPen;

    /// <summary>
    /// Used for the radius circle.
    /// </summary>
    private readonly Pen _radiusPen;

    /// <summary>
    /// Used to highlight objects within influence.
    /// </summary>
    private readonly Pen _influencedPen;

    /// <summary>
    /// Used to fill the selection rect and influence circle.
    /// </summary>
    private readonly Brush _lightBrush;

    /// <summary>
    /// Used to fill objects within influence.
    /// </summary>
    private readonly Brush _influencedBrush;

    #endregion

    #region Debug options

    /// <summary>
    /// Brush used for filling and drawing debug-related information.
    /// </summary>
    private readonly SolidColorBrush _debugBrushDark;
    /// <summary>
    /// Brush used for filling and drawing debug-related information.
    /// </summary>
    private readonly SolidColorBrush _debugBrushLight;

    private bool _debugModeIsEnabled = false;
    private readonly bool _debugShowObjectPositions = true;
    private readonly bool _debugShowQuadTreeViz = true;
    private readonly bool _debugShowSelectionRectCoordinates = true;
    private readonly bool _debugShowSelectionCollisionRect = true;
    private readonly bool _debugShowViewportRectCoordinates = true;
    private readonly bool _debugShowScrollableRectCoordinates = true;
    private readonly bool _debugShowLayoutRectCoordinates = true;
    private readonly bool _debugShowMouseGridCoordinates = true;
    private readonly bool _debugShowObjectCount = true;
    #endregion
    #region Constructor
    /// <summary>
    /// Constructor
    /// </summary>
    public AnnoCanvas() : this(null, null)
    {
        _fileSystem = new FileSystem();
    }
    public AnnoCanvas(BuildingPresets presetsToUse,
        Dictionary<string, IconImage> iconsToUse,
        IAppSettings appSettingsToUse = null,
        ICoordinateHelper coordinateHelperToUse = null,
        IBrushCache brushCacheToUse = null,
        IPenCache penCacheToUse = null,
        IMessageBoxService messageBoxServiceToUse = null,
        ILocalizationHelper localizationHelperToUse = null,
        IUndoManager undoManager = null,
        IClipboardService clipboardService = null)
    {
        InitializeComponent();

        _appSettings = appSettingsToUse ?? AppSettings.Instance;
        _appSettings.SettingsChanged += AppSettings_SettingsChanged;
        _coordinateHelper = coordinateHelperToUse ?? new CoordinateHelper();
        _brushCache = brushCacheToUse ?? new BrushCache();
        _penCache = penCacheToUse ?? new PenCache();
        _messageBoxService = messageBoxServiceToUse ?? new MessageBoxService();
        _localizationHelper = localizationHelperToUse ?? Localization.Localization.Instance;
        _layoutLoader = new LayoutLoader();
        UndoManager = undoManager ?? new UndoManager();
        IClipboard clipboard = new WindowsClipboard();
        ClipboardService = clipboardService ?? new ClipboardService(_layoutLoader, clipboard);

        _showScrollBars = _appSettings.ShowScrollbars;
        _hideInfluenceOnSelection = _appSettings.HideInfluenceOnSelection;

        UpdateScrollBarVisibility();

        Stopwatch sw = new();
        sw.Start();

        //initialize
        CurrentMode = MouseMode.Standard;
        PlacedObjects = new QuadTree<LayoutObject>(new Rect(-128, -128, 256, 256));
        SelectedObjects = [];
        _oldObjectPositions = [];
        _statisticsCalculationHelper = new StatisticsCalculationHelper();
        _viewport = new Viewport();
        _viewportTransform = new TranslateTransform(0d, 0d);

        #region Hotkeys/Commands
        //Commands
        RotateCommand = new RelayCommand(ExecuteRotate);
        rotateAllCommand = new RelayCommand(ExecuteRotateAll);
        copyCommand = new RelayCommand(ExecuteCopy);
        pasteCommand = new RelayCommand(ExecutePaste);
        deleteCommand = new RelayCommand(ExecuteDelete);
        duplicateCommand = new RelayCommand(ExecuteDuplicate);
        deleteObjectUnderCursorCommand = new RelayCommand(ExecuteDeleteObjectUnderCursor);
        undoCommand = new RelayCommand(ExecuteUndo);
        redoCommand = new RelayCommand(ExecuteRedo);
        enableDebugModeCommand = new RelayCommand(ExecuteEnableDebugMode);
        selectAllSameIdentifierCommand = new RelayCommand(ExecuteSelectAllSameIdentifier);

        //Set up default keybindings

        //for rotation with the r key.
        InputBinding rotateBinding1 = new(RotateCommand, new PolyGesture(Key.R, ModifierKeys.None));
        rotateHotkey1 = new Hotkey("Rotate_1", rotateBinding1, ROTATE_LOCALIZATION_KEY);

        //for rotation with middle click
        InputBinding rotateBinding2 = new(RotateCommand, new PolyGesture(ExtendedMouseAction.MiddleClick));
        rotateHotkey2 = new Hotkey("Rotate_2", rotateBinding2, ROTATE_LOCALIZATION_KEY);

        InputBinding rotateAllBinding = new(rotateAllCommand, new PolyGesture(Key.R, ModifierKeys.Shift));
        rotateAllHotkey = new Hotkey(ROTATE_ALL_LOCALIZATION_KEY, rotateAllBinding, ROTATE_ALL_LOCALIZATION_KEY);

        InputBinding copyBinding = new(copyCommand, new PolyGesture(Key.C, ModifierKeys.Control));
        copyHotkey = new Hotkey(COPY_LOCALIZATION_KEY, copyBinding, COPY_LOCALIZATION_KEY);

        InputBinding pasteBinding = new(pasteCommand, new PolyGesture(Key.V, ModifierKeys.Control));
        pasteHotkey = new Hotkey(PASTE_LOCALIZATION_KEY, pasteBinding, PASTE_LOCALIZATION_KEY);

        InputBinding deleteBinding = new(deleteCommand, new PolyGesture(Key.Delete, ModifierKeys.None));
        deleteHotkey = new Hotkey(DELETE_LOCALIZATION_KEY, deleteBinding, DELETE_LOCALIZATION_KEY);

        InputBinding duplicateBinding = new(duplicateCommand, new PolyGesture(ExtendedMouseAction.LeftDoubleClick, ModifierKeys.None));
        duplicateHotkey = new Hotkey(DUPLICATE_LOCALIZATION_KEY, duplicateBinding, DUPLICATE_LOCALIZATION_KEY);

        InputBinding deleteHoveredOjectBinding = new(deleteObjectUnderCursorCommand, new PolyGesture(ExtendedMouseAction.RightClick, ModifierKeys.None));
        deleteObjectUnderCursorHotkey = new Hotkey(DELETE_OBJECT_UNDER_CURSOR_LOCALIZATION_KEY, deleteHoveredOjectBinding, DELETE_OBJECT_UNDER_CURSOR_LOCALIZATION_KEY);

        InputBinding undoBinding = new(undoCommand, new PolyGesture(Key.Z, ModifierKeys.Control));
        undoHotkey = new Hotkey(UNDO_LOCALIZATION_KEY, undoBinding, UNDO_LOCALIZATION_KEY);

        InputBinding redoBinding = new(redoCommand, new PolyGesture(Key.Y, ModifierKeys.Control));
        redoHotkey = new Hotkey(REDO_LOCALIZATION_KEY, redoBinding, REDO_LOCALIZATION_KEY);

        InputBinding enableDebugModeBinding = new(enableDebugModeCommand, new PolyGesture(Key.D, ModifierKeys.Control | ModifierKeys.Shift));
        enableDebugModeHotkey = new Hotkey(ENABLE_DEBUG_MODE_LOCALIZATION_KEY, enableDebugModeBinding, ENABLE_DEBUG_MODE_LOCALIZATION_KEY);

        InputBinding selectAllSameIdentifierBinding = new(selectAllSameIdentifierCommand, new PolyGesture(ExtendedMouseAction.LeftClick, ModifierKeys.Control | ModifierKeys.Shift));
        selectAllSameIdentifierHotkey = new Hotkey(SELECT_ALL_SAME_IDENTIFIER_LOCALIZATION_KEY, selectAllSameIdentifierBinding, SELECT_ALL_SAME_IDENTIFIER_LOCALIZATION_KEY);

        //We specifically do not add the `InputBinding`s to the `InputBindingCollection` of `AnnoCanvas`, as if we did that,
        //`InputBinding.Gesture.Matches()` would be fired for *every* event - MouseWheel, MouseDown, KeyUp, KeyDown, MouseMove etc
        //which we don't want, as it produces a noticeable performance impact.
        #endregion

        LoadGridLineColor();
        LoadObjectBorderLineColor();

        _highlightPen = _penCache.GetPen(Brushes.Yellow, DPI_FACTOR * 2);
        _radiusPen = _penCache.GetPen(Brushes.Black, DPI_FACTOR * 2);
        _influencedPen = _penCache.GetPen(Brushes.LawnGreen, DPI_FACTOR * 2);

        Color color = Colors.LightYellow;
        color.A = 32;
        _lightBrush = _brushCache.GetSolidBrush(color);
        color = Colors.LawnGreen;
        color.A = 32;
        _influencedBrush = _brushCache.GetSolidBrush(color);
        _debugBrushLight = Brushes.Blue;
        _debugBrushDark = Brushes.DarkBlue;

        sw.Stop();
        logger.Trace($"init variables took: {sw.ElapsedMilliseconds}ms");

        // load presets and icons if not in design time
        if (!DesignerProperties.GetIsInDesignMode(this))
        {
            sw.Start();
            // load presets
            try
            {
                if (presetsToUse == null)
                {
                    BuildingPresetsLoader loader = new();
                    BuildingPresets = loader.Load(Path.Combine(App.ApplicationPath, CoreConstants.PresetsFiles.BuildingPresetsFile));
                }
                else
                {
                    BuildingPresets = presetsToUse;
                }
            }
            catch (Exception ex)
            {
                _messageBoxService.ShowError(ex.Message,
                      _localizationHelper.GetLocalization("LoadingPresetsFailed"));
            }

            sw.Stop();
            logger.Trace($"loading presets took: {sw.ElapsedMilliseconds}ms");

            if (iconsToUse == null)
            {
                sw.Start();

                // load icon name mapping
                IconMappingPresets iconNameMapping = null;
                try
                {
                    IconMappingPresetsLoader loader = new();
                    iconNameMapping = loader.LoadFromFile(Path.Combine(App.ApplicationPath, CoreConstants.PresetsFiles.IconNameFile));
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Loading of the icon names failed.");

                    _messageBoxService.ShowError(_localizationHelper.GetLocalization("LoadingIconNamesFailed"),
                        _localizationHelper.GetLocalization("Error"));
                }

                sw.Stop();
                logger.Trace($"loading icon mapping took: {sw.ElapsedMilliseconds}ms");

                sw.Start();

                // load icons
                IconLoader iconLoader = new();
                Icons = iconLoader.Load(Path.Combine(App.ApplicationPath, Constants.IconFolder), iconNameMapping);

                sw.Stop();
                logger.Trace($"loading icons took: {sw.ElapsedMilliseconds}ms");
            }
            else
            {
                Icons = iconsToUse;
            }
        }

        StatisticsUpdated?.Invoke(this, UpdateStatisticsEventArgs.All);
    }

    #endregion

    private bool _showScrollBars;
    private bool _hideInfluenceOnSelection;

    private void AppSettings_SettingsChanged(object sender, EventArgs e)
    {
        LoadGridLineColor();
        LoadObjectBorderLineColor();
        _needsRefreshAfterSettingsChanged = true;

        _showScrollBars = _appSettings.ShowScrollbars;
        _hideInfluenceOnSelection = _appSettings.HideInfluenceOnSelection;

        UpdateScrollBarVisibility();
    }

    #endregion

    #region Rendering

    protected override Size ArrangeOverride(Size arrangeBounds)
    {
        //force scroll bars to update when we resize the window
        if (_oldArrangeBounds != arrangeBounds)
        {
            _oldArrangeBounds = arrangeBounds;
            InvalidateScroll();
        }
        return base.ArrangeOverride(arrangeBounds);
    }

    private Rect _lastViewPortAbsolute = default;
    private List<LayoutObject> _lastObjectsToDraw = [];
    private List<LayoutObject> _lastBorderlessObjectsToDraw = [];
    private List<LayoutObject> _lastBorderedObjectsToDraw = [];
    private QuadTree<LayoutObject> _lastPlacedObjects = null;

    private DrawingGroup _drawingGroupGridLines = new();
    private DrawingGroup _drawingGroupObjects = new();
    private DrawingGroup _drawingGroupSelectedObjectsInfluence = new();
    private DrawingGroup _drawingGroupInfluence = new();
    private int _lastGridSize = -1;
    private double _lastWidth = -1;
    private double _lastHeight = -1;
    private bool _needsRefreshAfterSettingsChanged;
    private bool _isRenderingForced;

    /// <summary>
    /// Renders the whole scene including grid, placed objects, current object, selection highlights, influence radii and selection rectangle.
    /// </summary>
    /// <param name="drawingContext">context used for rendering</param>
    protected override void OnRender(DrawingContext drawingContext)
    {
        double width = RenderSize.Width;
        double height = RenderSize.Height;
        _viewport.Width = _coordinateHelper.ScreenToGrid(width, _gridSize);
        _viewport.Height = _coordinateHelper.ScreenToGrid(height, _gridSize);

        if (ScrollOwner != null && _invalidateScrollInfo)
        {
            ScrollOwner?.InvalidateScrollInfo();
            _invalidateScrollInfo = false;
        }

        //use the negated value for the transform, as when we move the viewport (for example, if Top gets
        //increased by 1) we want the items to "shift" in the opposite direction to the movement of the viewport:
        /*
         |  +=+ = viewport
         |  [] = object
         |
         |  Object on edge of viewport.
         |
         |  1 +==[]=+
         |  2 |     |
         |  3 +=====+
         |  4
         |
         |  Viewport shifts down
         |
         |  1    []
         |  2 +=====+
         |  3 |     |
         |  4 +=====+
         |
         |  Relative to the viewport, the object has been shifted "up".
         */
        _viewportTransform.X = _coordinateHelper.GridToScreen(-_viewport.Left, _gridSize);
        _viewportTransform.Y = _coordinateHelper.GridToScreen(-_viewport.Top, _gridSize);

        // assure pixel perfect drawing using guidelines.
        // this value is cached and refreshed in LoadGridLineColor(), as it uses pen thickness in its calculation;
        drawingContext.PushGuidelineSet(_guidelineSet);

        // draw background
        drawingContext.DrawRectangle(Brushes.Transparent, null, new Rect(0, 0, width, height));

        // draw grid
        if (RenderGrid)
        {
            //check if redraw is necessary
            if (_isRenderingForced ||
                _gridSize != _lastGridSize ||
                height != _lastHeight ||
                width != _lastWidth ||
                _needsRefreshAfterSettingsChanged)
            {
                if (_drawingGroupGridLines.IsFrozen)
                {
                    _drawingGroupGridLines = new DrawingGroup();
                }

                DrawingContext context = _drawingGroupGridLines.Open();
                context.PushGuidelineSet(_guidelineSet);

                //vertical lines
                for (double i = _viewport.HorizontalAlignmentValue * _gridSize; i < width; i += _gridSize)
                {
                    context.DrawLine(_gridLinePen, new Point(i, 0), new Point(i, height));
                }

                //horizontal lines
                for (double i = _viewport.VerticalAlignmentValue * _gridSize; i < height; i += _gridSize)
                {
                    context.DrawLine(_gridLinePen, new Point(0, i), new Point(width, i));
                }

                context.Close();

                _lastGridSize = _gridSize;
                _lastHeight = height;
                _lastWidth = width;
                _needsRefreshAfterSettingsChanged = false;

                if (_drawingGroupGridLines.CanFreeze)
                {
                    _drawingGroupGridLines.Freeze();
                }
            }

            drawingContext.DrawDrawing(_drawingGroupGridLines);
        }

        //Push the transform after rendering everything that should not be translated.
        drawingContext.PushTransform(_viewportTransform);

        Rect viewPortAbsolute = _viewport.Absolute; //hot path optimization
        List<LayoutObject> objectsToDraw = _lastObjectsToDraw;
        List<LayoutObject> borderlessObjects = _lastBorderlessObjectsToDraw;
        List<LayoutObject> borderedObjects = _lastBorderedObjectsToDraw;
        bool objectsChanged = false;

        if (_isRenderingForced ||
            _lastViewPortAbsolute != viewPortAbsolute ||
            _lastPlacedObjects != PlacedObjects ||
            CurrentMode == MouseMode.PlaceObjects ||
            CurrentMode == MouseMode.DeleteObject)
        {
            objectsToDraw = PlacedObjects.GetItemsIntersecting(viewPortAbsolute).ToList();
            _lastObjectsToDraw = objectsToDraw;
            _lastPlacedObjects = PlacedObjects;
            _lastViewPortAbsolute = viewPortAbsolute;

            borderlessObjects = objectsToDraw.Where(_ => _.WrappedAnnoObject.Borderless).ToList();
            _lastBorderlessObjectsToDraw = borderlessObjects;
            borderedObjects = objectsToDraw.Where(_ => !_.WrappedAnnoObject.Borderless).ToList();
            _lastBorderedObjectsToDraw = borderedObjects;

            //quick fix deleting objects via keyboard instead of right click
            if (CurrentMode == MouseMode.DeleteObject)
            {
                CurrentMode = MouseMode.Standard;
            }

            objectsChanged = true;
        }

        // draw placed objects            
        if (_isRenderingForced || objectsChanged)
        {
            if (_drawingGroupObjects.IsFrozen)
            {
                _drawingGroupObjects = new DrawingGroup();
            }

            DrawingContext context = _drawingGroupObjects.Open();
            context.PushGuidelineSet(_guidelineSet);

            //borderless objects should be drawn first; selection afterwards
            RenderObjectList(context, borderlessObjects, useTransparency: false);
            RenderObjectList(context, borderedObjects, useTransparency: false);

            context.Close();

            if (_drawingGroupObjects.CanFreeze)
            {
                _drawingGroupObjects.Freeze();
            }
        }

        drawingContext.DrawDrawing(_drawingGroupObjects);

        bool selectionWasRedrawn;
        // draw object selection around not ignored selected objects
        if (selectionContainsNotIgnoredObject)
        {
            selectionWasRedrawn = RenderObjectSelection(drawingContext, SelectedObjects.WithoutIgnoredObjects());
        }
        else
        {
            // except when only ignored objects are selected, in which case render their selection
            selectionWasRedrawn = RenderObjectSelection(drawingContext, SelectedObjects);
        }

        if (RenderPanorama)
        {
            RenderPanoramaText(drawingContext, objectsToDraw, forceRedraw: _isRenderingForced || objectsChanged);
        }

        if (!RenderInfluences)
        {
            if (!_hideInfluenceOnSelection)
            {
                if (selectionWasRedrawn || _isRenderingForced)
                {
                    if (_drawingGroupSelectedObjectsInfluence.IsFrozen)
                    {
                        _drawingGroupSelectedObjectsInfluence = new DrawingGroup();
                    }

                    DrawingContext context = _drawingGroupSelectedObjectsInfluence.Open();
                    context.PushGuidelineSet(_guidelineSet);

                    RenderObjectInfluenceRadius(context, SelectedObjects);
                    RenderObjectInfluenceRange(context, SelectedObjects);

                    context.Close();

                    if (_drawingGroupSelectedObjectsInfluence.CanFreeze)
                    {
                        _drawingGroupSelectedObjectsInfluence.Freeze();
                    }
                }

                if (SelectedObjects.Count > 0)
                {
                    drawingContext.DrawDrawing(_drawingGroupSelectedObjectsInfluence);
                }
            }
        }
        else
        {
            if (objectsChanged || _isRenderingForced)
            {
                if (_drawingGroupInfluence.IsFrozen)
                {
                    _drawingGroupInfluence = new DrawingGroup();
                }

                DrawingContext context = _drawingGroupInfluence.Open();
                context.PushGuidelineSet(_guidelineSet);

                RenderObjectInfluenceRadius(context, objectsToDraw);
                RenderObjectInfluenceRange(context, objectsToDraw);
                //Retrieve objects outside the viewport that have an influence range which affects objects
                //within the viewport.
                List<LayoutObject> offscreenObjects = PlacedObjects
                .Where(_ => !viewPortAbsolute.Contains(_.GridRect) &&
                            (viewPortAbsolute.IntersectsWith(_.GridInfluenceRadiusRect) || viewPortAbsolute.IntersectsWith(_.GridInfluenceRangeRect))
                 ).ToList();
                RenderObjectInfluenceRadius(context, offscreenObjects);
                RenderObjectInfluenceRange(context, offscreenObjects);

                context.Close();

                if (_drawingGroupInfluence.CanFreeze)
                {
                    _drawingGroupInfluence.Freeze();
                }
            }

            drawingContext.DrawDrawing(_drawingGroupInfluence);
        }

        if (CurrentObjects.Count == 0)
        {
            // highlight object which is currently hovered, but not if some objects are being dragged
            if (CurrentMode != MouseMode.DragSelection)
            {
                LayoutObject hoveredObj = GetObjectAt(_mousePosition);
                if (hoveredObj != null)
                {
                    drawingContext.DrawRectangleRotated(null, _highlightPen, hoveredObj.CalculateScreenRect(_gridSize), hoveredObj.RotationDegrees);
                }
            }
        }
        else
        {
            // draw current object
            if (_mouseWithinControl)
            {
                //Push a tranform to reverse the effects, as objects should be positioned correctly
                //on the canvas with the included viewport offset, but we want them to render without the offset.
                //If we just did drawingContext.Pop() here, the items would appear offset compared to where the mouse is, 
                //as the Position of the objects have already been set to values relative to the viewport.
                drawingContext.PushTransform(_viewportTransform.Inverse as TranslateTransform);

                MoveCurrentObjectsToMouse();
                // draw influence radius
                RenderObjectInfluenceRadius(drawingContext, CurrentObjects);
                // draw influence range
                RenderObjectInfluenceRange(drawingContext, CurrentObjects);
                // draw with transparency
                RenderObjectList(drawingContext, CurrentObjects, useTransparency: true);

                drawingContext.Pop();
            }

        }
        //pop viewport transform
        drawingContext.Pop();

        // draw selection rect while dragging the mouse
        if (CurrentMode == MouseMode.SelectionRect)
        {
            drawingContext.DrawRectangle(_lightBrush, _highlightPen, _selectionRect);
        }

        #region Draw debug information

        if (_debugModeIsEnabled)
        {
            drawingContext.PushTransform(_viewportTransform);
            if (_debugShowQuadTreeViz)
            {
                SolidColorBrush brush = Brushes.Transparent;
                Pen pen = _penCache.GetPen(_debugBrushDark, 2);
                foreach (Rect rect in PlacedObjects.GetQuadrantRects())
                {
                    drawingContext.DrawRectangle(brush, pen, _coordinateHelper.GridToScreen(rect, _gridSize));
                }
            }

            if (_debugShowSelectionCollisionRect)
            {
                Color color = _debugBrushLight.Color;
                color.A = 0x08;
                SolidColorBrush brush = _brushCache.GetSolidBrush(color);
                Pen pen = _penCache.GetPen(_debugBrushLight, 1);
                Rect collisionRectScreen = _coordinateHelper.GridToScreen(_collisionRect, _gridSize);
                drawingContext.DrawRectangle(brush, pen, collisionRectScreen);
            }

            //pop viewport transform
            drawingContext.Pop();
            List<FormattedText> debugText = new(3);

            if (_debugShowViewportRectCoordinates)
            {
                //The first time this is called, App.DpiScale is still 0 which causes this code to throw an error
                if (App.DpiScale.PixelsPerDip != 0)
                {
                    double top = _viewport.Top;
                    double left = _viewport.Left;
                    double h = _viewport.Height;
                    double w = _viewport.Width;
                    FormattedText text = new($"Viewport: {left:F2}, {top:F2}, {w:F2}, {h:F2}", Thread.CurrentThread.CurrentCulture, FlowDirection.LeftToRight,
                                                 TYPEFACE, 12, _debugBrushLight, null, TextFormattingMode.Display, App.DpiScale.PixelsPerDip)
                    {
                        TextAlignment = TextAlignment.Left
                    };
                    debugText.Add(text);
                }
            }

            if (_debugShowScrollableRectCoordinates)
            {
                //The first time this is called, App.DpiScale is still 0 which causes this code to throw an error
                if (App.DpiScale.PixelsPerDip != 0)
                {
                    double top = _scrollableBounds.Top;
                    double left = _scrollableBounds.Left;
                    double h = _scrollableBounds.Height;
                    double w = _scrollableBounds.Width;
                    FormattedText text = new($"Scrolllable: {left:F2}, {top:F2}, {w:F2}, {h:F2}", Thread.CurrentThread.CurrentCulture, FlowDirection.LeftToRight,
                                                 TYPEFACE, 12, _debugBrushLight, null, TextFormattingMode.Display, App.DpiScale.PixelsPerDip)
                    {
                        TextAlignment = TextAlignment.Left
                    };
                    debugText.Add(text);
                }
            }

            if (_debugShowLayoutRectCoordinates)
            {
                //The first time this is called, App.DpiScale is still 0 which causes this code to throw an error
                if (App.DpiScale.PixelsPerDip != 0)
                {
                    double top = _layoutBounds.Top;
                    double left = _layoutBounds.Left;
                    double h = _layoutBounds.Height;
                    double w = _layoutBounds.Width;
                    FormattedText text = new($"Layout: {left:F2}, {top:F2}, {w:F2}, {h:F2}", Thread.CurrentThread.CurrentCulture, FlowDirection.LeftToRight,
                                                 TYPEFACE, 12, _debugBrushLight, null, TextFormattingMode.Display, App.DpiScale.PixelsPerDip)
                    {
                        TextAlignment = TextAlignment.Left
                    };
                    debugText.Add(text);
                }
            }

            if (_debugShowObjectCount)
            {
                //The first time this is called, App.DpiScale is still 0 which causes this code to throw an error
                if (App.DpiScale.PixelsPerDip != 0)
                {
                    FormattedText text = new($"{nameof(PlacedObjects)}: {PlacedObjects.Count}", Thread.CurrentThread.CurrentCulture, FlowDirection.LeftToRight,
                                                 TYPEFACE, 12, _debugBrushLight, null, TextFormattingMode.Display, App.DpiScale.PixelsPerDip)
                    {
                        TextAlignment = TextAlignment.Left
                    };
                    debugText.Add(text);
                }
            }

            for (int i = 0; i < debugText.Count; i++)
            {
                drawingContext.DrawText(debugText[i], new Point(5, (i * 15) + 5));
            }

            if (_debugShowMouseGridCoordinates)
            {
                //The first time this is called, App.DpiScale is still 0 which causes this code to throw an error
                if (App.DpiScale.PixelsPerDip != 0)
                {
                    Point gridPosition = _coordinateHelper.ScreenToFractionalGrid(_mousePosition, _gridSize);
                    gridPosition = _viewport.OriginToViewport(gridPosition);
                    double x = gridPosition.X;
                    double y = gridPosition.Y;
                    FormattedText text = new($"{x:F2}, {y:F2}", Thread.CurrentThread.CurrentCulture, FlowDirection.LeftToRight,
                                                 TYPEFACE, 12, _debugBrushLight, null, TextFormattingMode.Display, App.DpiScale.PixelsPerDip)
                    {
                        TextAlignment = TextAlignment.Left
                    };
                    Point pos = _mousePosition;
                    pos.X -= 5;
                    pos.Y += 15;
                    drawingContext.DrawText(text, pos);
                }
            }

            //draw selection rect coords last so they draw over the top of everything else
            if (CurrentMode == MouseMode.SelectionRect)
            {
                if (_debugShowSelectionRectCoordinates)
                {
                    Rect rect = _coordinateHelper.ScreenToGrid(_selectionRect, _gridSize);
                    double top = rect.Top;
                    double left = rect.Left;
                    double h = rect.Height;
                    double w = rect.Width;
                    FormattedText text = new($"{left:F2}, {top:F2}, {w:F2}, {h:F2}", Thread.CurrentThread.CurrentCulture, FlowDirection.LeftToRight,
                   TYPEFACE, 12, _debugBrushLight,
                   null, TextFormattingMode.Display, App.DpiScale.PixelsPerDip)
                    {
                        TextAlignment = TextAlignment.Left
                    };
                    Point location = _selectionRect.BottomRight;
                    location.X -= text.Width;
                    location.Y -= text.Height;
                    drawingContext.DrawText(text, location);
                }
            }
        }

        #endregion

        // pop back guidlines set
        drawingContext.Pop();

        _isRenderingForced = false;
    }

    private DrawingGroup _drawingGroupPanoramaText = new();

    private void RenderPanoramaText(DrawingContext drawingContext, List<LayoutObject> placedObjects, bool forceRedraw)
    {
        if (placedObjects.Count == 0)
        {
            return;
        }

        if (!forceRedraw)
        {
            drawingContext.DrawDrawing(_drawingGroupPanoramaText);
            return;
        }

        if (_drawingGroupPanoramaText.IsFrozen)
        {
            _drawingGroupPanoramaText = new DrawingGroup();
        }

        DrawingContext context = _drawingGroupPanoramaText.Open();
        context.PushGuidelineSet(_guidelineSet);

        foreach (LayoutObject curObject in placedObjects.FindAll(_ => _.Identifier.StartsWith(IDENTIFIER_SKYSCRAPER, StringComparison.OrdinalIgnoreCase)))
        {
            if (!_regex_panorama.TryMatch(curObject.Identifier, out Match match))
            {
                continue;
            }

            Point center = _coordinateHelper.GetCenterPoint(curObject.GridRect);

            int tier = int.Parse(match.Groups["tier"].Value);
            int level = int.Parse(match.Groups["level"].Value);
            double radiusSquared = curObject.WrappedAnnoObject.Radius * curObject.WrappedAnnoObject.Radius;
            int panorama = level;

            //find intersecting skyscrapers
            foreach (LayoutObject adjacentObject in PlacedObjects.GetItemsIntersecting(curObject.GridInfluenceRadiusRect)
                .Where(_ => _.Identifier.StartsWith(IDENTIFIER_SKYSCRAPER, StringComparison.OrdinalIgnoreCase)))
            {
                if (adjacentObject == curObject)
                {
                    continue;
                }

                if ((center - _coordinateHelper.GetCenterPoint(adjacentObject.GridRect)).LengthSquared <= radiusSquared)
                {
                    if (_regex_panorama.TryMatch(adjacentObject.Identifier, out Match match2))
                    {
                        int tier2 = int.Parse(match2.Groups["tier"].Value);
                        int level2 = int.Parse(match2.Groups["level"].Value);
                        if (tier != tier2)
                        {
                            // same levels increase panorama if different tiers
                            panorama += level >= level2 ? 1 : -1;
                        }
                        else
                        {
                            // only lower levels increase panorama for same tiers
                            panorama += level > level2 ? 1 : -1;
                        }
                    }
                }
            }

            if (curObject.LastPanorama != panorama || curObject.PanoramaText == null)
            {
                curObject.LastPanorama = panorama;

                // put the sign at the end of the string since it will be drawn from right to left
                string text = Math.Abs(panorama).ToString() + (panorama >= 0 ? "" : "-");

                curObject.PanoramaText = new FormattedText(text, Thread.CurrentThread.CurrentUICulture,
                    FlowDirection.RightToLeft, TYPEFACE, FontSize, Brushes.Black, App.DpiScale.PixelsPerDip);
            }

            context.DrawText(curObject.PanoramaText, curObject.CalculateScreenRect(GridSize).TopRight);
        }

        context.Close();

        if (_drawingGroupPanoramaText.CanFreeze)
        {
            _drawingGroupPanoramaText.Freeze();
        }

        drawingContext.DrawDrawing(_drawingGroupPanoramaText);
    }

    /// <summary>
    /// Moves the current object to the mouse position.
    /// </summary>
    private void MoveCurrentObjectsToMouse()
    {
        if (CurrentObjects.Count == 0)
        {
            return;
        }

        if (CurrentObjects.Count > 1)
        {
            //Get the center of the current selection
            Rect r = CurrentObjects[0].GridRect;
            foreach (LayoutObject obj in CurrentObjects.Skip(1))
            {
                r.Union(obj.GridRect);
            }

            Point center = _coordinateHelper.GetCenterPoint(r);
            Point mousePosition = _coordinateHelper.ScreenToFractionalGrid(_mousePosition, GridSize);
            double dx = mousePosition.X - center.X;
            double dy = mousePosition.Y - center.Y;
            foreach (LayoutObject obj in CurrentObjects)
            {
                Point pos = obj.Position;
                pos = _viewport.OriginToViewport(new Point(pos.X + dx, pos.Y + dy));
                pos = new Point(Math.Floor(pos.X), Math.Floor(pos.Y));
                obj.Position = pos;
            }
        }
        //Place singular building
        else
        {
            Point pos = _coordinateHelper.ScreenToFractionalGrid(_mousePosition, GridSize);
            var layoutObj = CurrentObjects[0];
            pos.X -= layoutObj.Size.Width / 2;
            pos.Y -= layoutObj.Size.Height / 2;

                    
            pos = _viewport.OriginToViewport(pos);

            if (layoutObj.Is45DegreeOriented)
            {
                pos = _coordinateHelper.GridClamp(pos, layoutObj.DiagonalTileSize.Width % 2 == 0, layoutObj.DiagonalTileSize.Height % 2 == 0);
            }
            else
            {
                pos = _coordinateHelper.GridClamp(pos, layoutObj.Size.Width % 2 == 0, layoutObj.Size.Height % 2 == 0);
            }
            
            CurrentObjects[0].Position = pos;
        }
    }

    /// <summary>
    /// Renders the given AnnoObject to the given DrawingContext.
    /// </summary>
    /// <param name="drawingContext">context used for rendering</param>
    /// <param name="objects">object to render</param>
    /// <param name="useTransparency">if true, the object will be rendered with transparency</param>"
    private void RenderObjectList(DrawingContext drawingContext, List<LayoutObject> objects, bool useTransparency)
    {
        if (objects.Count == 0)
        {
            return;
        }

        int gridSize = GridSize; //hot path optimization
        double linePenThickness = LinePenThickness; //hot path optimization (avoid access of DependencyProperty)
        bool renderHarborBlockedArea = RenderHarborBlockedArea; //hot path optimization
        bool renderIcon = RenderIcon; //hot path optimization
        bool renderLabel = RenderLabel; //hot path optimization

        foreach (LayoutObject curLayoutObject in objects)
        {
            AnnoObject obj = curLayoutObject.WrappedAnnoObject;
            Rect objRect = curLayoutObject.CalculateScreenRect(gridSize);



            SolidColorBrush brush = useTransparency ? curLayoutObject.TransparentBrush : curLayoutObject.RenderBrush;

            Pen borderPen = obj.Borderless ? curLayoutObject.GetBorderlessPen(brush, linePenThickness) : _linePen;

            drawingContext.DrawRectangleRotated(brush, borderPen, objRect, curLayoutObject.RotationDegrees);

            if (renderHarborBlockedArea)
            {
                Rect? objBlockedRect = curLayoutObject.CalculateBlockedScreenRect(gridSize);
                if (objBlockedRect.HasValue)
                {
                    drawingContext.DrawRectangleRotated(curLayoutObject.BlockedAreaBrush, borderPen, objBlockedRect.Value, curLayoutObject.RotationDegrees);
                }
            }

            // draw object icon if it is at least 2x2 cells
            bool iconRendered = false;
            if (renderIcon && !string.IsNullOrEmpty(obj.Icon))
            {
                bool iconFound = false;

                if (curLayoutObject.Icon is null)
                {
                    string iconName = curLayoutObject.IconNameWithoutExtension; // for backwards compatibility to older layouts

                    //a null check is not needed here, as IconNameWithoutExtension uses obj.Icon, and we already check if that 
                    //is null or empty, meaning the value that we feed into Path.GetFileNameWithoutExtension cannot be null, and
                    //Path.GetFileNameWithoutExtension will either throw (representing an invalid path) or return a string 
                    //(representing the file name)
                    if (Icons.TryGetValue(iconName, out IconImage iconImage))
                    {
                        curLayoutObject.Icon = iconImage;
                        iconFound = true;
                    }
                    else
                    {
                        string message = $"Icon file missing ({iconName}).";
                        logger.Warn(message);
                        StatusMessage = message;
                    }
                }
                else
                {
                    iconFound = true;
                }

                if (iconFound)
                {
                    Rect iconRect = curLayoutObject.GetIconRect(gridSize);

                    drawingContext.DrawImage(curLayoutObject.Icon.Icon, iconRect);
                    iconRendered = true;
                }
            }

            // draw object label
            if (renderLabel && !string.IsNullOrEmpty(obj.Label))
            {
                TextAlignment textAlignment = iconRendered ? TextAlignment.Left : TextAlignment.Center;
                FormattedText text = curLayoutObject.GetFormattedText(textAlignment, Thread.CurrentThread.CurrentCulture,
                    TYPEFACE, App.DpiScale.PixelsPerDip, objRect.Width, objRect.Height);

                Point textLocation = objRect.TopLeft;
                if (iconRendered)
                {
                    // place the text in the top left corner if a icon is present                        
                    textLocation.X += 3;
                    textLocation.Y += 2;
                }
                else
                {
                    // center the text if no icon is present                        
                    textLocation.Y += (objRect.Height - text.Height) / 2;
                }

                drawingContext.DrawText(text, textLocation);
            }

            if (_debugModeIsEnabled && _debugShowObjectPositions)
            {
                FormattedText text = new(obj.Position.ToString(), Thread.CurrentThread.CurrentCulture, FlowDirection.LeftToRight,
                TYPEFACE, 12, _debugBrushLight,
                null, TextFormattingMode.Display, App.DpiScale.PixelsPerDip)
                {
                    MaxTextWidth = objRect.Width,
                    MaxTextHeight = objRect.Width,
                    TextAlignment = TextAlignment.Left
                };
                Point textLocation = objRect.BottomRight;
                textLocation.X -= text.Width;
                textLocation.Y -= text.Height;

                drawingContext.DrawText(text, textLocation);
            }
        }
    }

    private DrawingGroup _drawingGroupObjectSelection = new();
    private ICollection<LayoutObject> _lastSelectedObjects = [];
    private int _lastObjectSelectionGridSize = -1;
    private Rect _lastSelectionRect;

    /// <summary>
    /// Renders a selection highlight on the specified object.
    /// </summary>
    /// <param name="drawingContext">context used for rendering</param>
    private bool RenderObjectSelection(DrawingContext drawingContext, ICollection<LayoutObject> objects)
    {
        bool wasRedrawn = false;
        if (_lastSelectionRect == _selectionRect && objects.Count == 0)
        {
            return wasRedrawn;
        }

        if (_lastSelectedObjects != objects || _lastObjectSelectionGridSize != GridSize || CurrentMode == MouseMode.DragSelection || _lastSelectionRect != _selectionRect)
        {
            if (_drawingGroupObjectSelection.IsFrozen)
            {
                _drawingGroupObjectSelection = new DrawingGroup();
            }

            DrawingContext context = _drawingGroupObjectSelection.Open();
            context.PushGuidelineSet(_guidelineSet);

            foreach (LayoutObject curLayoutObject in objects)
            {
                // draw object rectangle                
                context.DrawRectangleRotated(null, _highlightPen, curLayoutObject.CalculateScreenRect(GridSize), curLayoutObject.RotationDegrees);
            }

            context.Close();

            _lastObjectSelectionGridSize = GridSize;
            _lastSelectedObjects = objects;
            _lastSelectionRect = _selectionRect;
            wasRedrawn = true;

            if (_drawingGroupObjectSelection.CanFreeze)
            {
                _drawingGroupObjectSelection.Freeze();
            }
        }

        drawingContext.DrawDrawing(_drawingGroupObjectSelection);

        return wasRedrawn;
    }

    /// <summary>
    /// Renders the influence radius of the given object and highlights other objects within range.
    /// </summary>
    /// <param name="drawingContext">context used for rendering</param>
    private void RenderObjectInfluenceRadius(DrawingContext drawingContext, ICollection<LayoutObject> objects)
    {
        if (objects.Count == 0)
        {
            return;
        }

        foreach (LayoutObject curLayoutObject in objects)
        {
            if (curLayoutObject.WrappedAnnoObject.Radius >= 0.5)
            {
                // highlight buildings within influence
                double radius = curLayoutObject.GetScreenRadius(GridSize);
                EllipseGeometry circle = curLayoutObject.GetInfluenceCircle(GridSize, radius);

                double circleCenterX = circle.Center.X;
                double circleCenterY = circle.Center.Y;

                Rect influenceGridRect = curLayoutObject.GridInfluenceRadiusRect;

                foreach (LayoutObject curPlacedObject in PlacedObjects.GetItemsIntersecting(influenceGridRect).WithoutIgnoredObjects())
                {
                    Point distance = curPlacedObject.GetScreenRectCenterPoint(GridSize);
                    distance.X -= circleCenterX;
                    distance.Y -= circleCenterY;
                    // check if the center is within the influence circle
                    if ((distance.X * distance.X) + (distance.Y * distance.Y) <= radius * radius)
                    {
                        drawingContext.DrawRectangleRotated(_influencedBrush, _influencedPen, curPlacedObject.CalculateScreenRect(GridSize), curPlacedObject.RotationDegrees);
                    }
                }

                // draw circle
                drawingContext.DrawGeometry(_lightBrush, _radiusPen, circle);
            }
        }
    }

    /// <summary>
    /// Renders influence range of the given objects.
    /// If RenderTrueInfluenceRange is set to true, true influence range will be rendered and objects inside will be highlighted.
    /// Else maximum influence range will be rendered.
    /// </summary>
    private void RenderObjectInfluenceRange(DrawingContext drawingContext, ICollection<LayoutObject> objects)
    {
        if (objects.Count == 0 || !RenderInfluences)
        {
            //show influence for selected objects and for objects to be placed
            if (SelectedObjects.Count == 0 && CurrentObjects.Count == 0)
            {
                return;
            }
        }

        Moved2DArray<AnnoObject> gridDictionary = null;
        List<AnnoObject> placedAnnoObjects = null;

        if (RenderTrueInfluenceRange && PlacedObjects.Count > 0)
        {
            HashSet<LayoutObject> placedObjects = [.. PlacedObjects, .. objects];
            placedAnnoObjects = placedObjects.Select(o => o.WrappedAnnoObject).ToList();
            Dictionary<AnnoObject, LayoutObject> placedObjectDictionary = placedObjects.ToDictionaryWithCapacity(o => o.WrappedAnnoObject);

            void Highlight(AnnoObject objectInRange)
            {
                drawingContext.DrawRectangleRotated(_influencedBrush, _influencedPen, placedObjectDictionary[objectInRange].CalculateScreenRect(GridSize), placedObjectDictionary[objectInRange].RotationDegrees);
            }

            gridDictionary = RoadSearchHelper.PrepareGridDictionary(placedAnnoObjects);
            _ = RoadSearchHelper.BreadthFirstSearch(
                placedAnnoObjects,
                objects.Select(o => o.WrappedAnnoObject).Where(o => o.InfluenceRange > 0.5),
                o => (int)o.InfluenceRange + 1,// increase distance to get objects that are touching even the last road cell in influence range
                gridDictionary,
                Highlight);
        }

        ConcurrentBag<(long index, StreamGeometry geometry)> geometries = [];
        _ = Parallel.ForEach(objects, (curLayoutObject, _, index) =>
        {
            if (curLayoutObject.WrappedAnnoObject.InfluenceRange > 0.5)
            {
                StreamGeometry sg = new();

                using (StreamGeometryContext sgc = sg.Open())
                {
                    if (RenderTrueInfluenceRange)
                    {
                        DrawTrueInfluenceRangePolygon(curLayoutObject, sgc, gridDictionary, placedAnnoObjects);
                    }
                    else
                    {
                        DrawInfluenceRangePolygon(curLayoutObject, sgc);
                    }
                }

                if (sg.CanFreeze)
                {
                    sg.Freeze();
                }
                geometries.Add((index, sg));
            }
        });

        foreach ((_, StreamGeometry geometry) in geometries.OrderBy(p => p.index))
        {
            drawingContext.DrawGeometry(_lightBrush, _radiusPen, geometry);
        }
    }

    private void DrawTrueInfluenceRangePolygon(LayoutObject curLayoutObject, StreamGeometryContext sgc, Moved2DArray<AnnoObject> gridDictionary, List<AnnoObject> placedAnnoObjects)
    {
        bool stroked = true;
        bool smoothJoin = true;

        bool geometryFill = true;
        bool geometryStroke = true;

        AnnoObject[] startObjects =
        [
            curLayoutObject.WrappedAnnoObject
        ];

        bool[][] cellsInInfluenceRange = RoadSearchHelper.BreadthFirstSearch(
            placedAnnoObjects,
            startObjects,
            o => (int)o.InfluenceRange,
            gridDictionary);

        IReadOnlyList<(int x, int y)> points = PolygonBoundaryFinderHelper.GetBoundaryPoints(cellsInInfluenceRange);
        if (points.Count < 1)
        {
            return;
        }

        sgc.BeginFigure(_coordinateHelper.GridToScreen(new Point(points[0].x + gridDictionary.Offset.x, points[0].y + gridDictionary.Offset.y), GridSize), geometryFill, geometryStroke);
        for (int i = 1; i < points.Count; i++)
        {
            sgc.LineTo(_coordinateHelper.GridToScreen(new Point(points[i].x + gridDictionary.Offset.x, points[i].y + gridDictionary.Offset.y), GridSize), stroked, smoothJoin);
        }
    }

    private void DrawInfluenceRangePolygon(LayoutObject curLayoutObject, StreamGeometryContext sgc)
    {
        //The below code looks very complex, but is surprisingly quick as most of its
        //calculations are done with Points, which being structs, are value types, and
        //consequently have much lower GC and memory copy costs.

        //I did try to cache these points, but as they are absolute values, I needed to
        //appy an offset to revert to the proper positions. The process of needing to
        //enumerate the points list to add the offset proved to be slower than just
        //recalculating all the points again.

        //You can see my attempts in some of the previous reverted commits for this branch.
        //An alternate caching could work in future, if one can be designed. 

        //Octagon is drawn in clockwise starting from the top-left corner
        //The arrows represent the direction, the inner square represents the influence area
        //In the normal working, this area is diagonal (hence the octagon drawn), but this 
        //cannot be easily displayed on the diagram below.

        //Start here: V
        //  +-------> --> -------+
        //  |                    |
        //  |    +---+--+---+    |
        //  |    |   |  |   |    |
        //  |    | 1 |  | 2 |    |
        //  |    |   |  |   |    v
        //  ^    +----------+    |
        //  |    |   |  |   |    |
        //  |    +----------+    v
        //  ^    |   |  |   |    |
        //  |    | 4 |  | 3 |    |
        //  |    |   |  |   |    |
        //  |    +---+--+---+    |
        //  |                    |
        //  +------- <--- <------+

        //Quadrant 1 = min(x), min(y)
        //Quadrant 2 = max(x), min(y)
        //Quadrant 3 = min(x), max(y)
        //Quadrant 4 = max(x), max(y)

        //In grid units
        Point topLeftCorner = curLayoutObject.Position;
        Point topRightCorner = new(curLayoutObject.Position.X + curLayoutObject.Size.Width, curLayoutObject.Position.Y);
        Point bottomLeftCorner = new(curLayoutObject.Position.X, curLayoutObject.Position.Y + curLayoutObject.Size.Height);
        Point bottomRightCorner = new(curLayoutObject.Position.X + curLayoutObject.Size.Width, curLayoutObject.Position.Y + curLayoutObject.Size.Height);

        double influenceRange = curLayoutObject.WrappedAnnoObject.InfluenceRange;

        Point startPoint = new(topLeftCorner.X, topLeftCorner.Y - influenceRange);
        bool stroked = true;
        bool smoothJoin = true;

        bool geometryFill = true;
        bool geometryStroke = true;

        sgc.BeginFigure(_coordinateHelper.GridToScreen(startPoint, GridSize), geometryFill, geometryStroke);

        ////////////////////////////////////////////////////////////////
        //Draw in width of object
        sgc.LineTo(_coordinateHelper.GridToScreen(new Point(topRightCorner.X, startPoint.Y), GridSize), stroked, smoothJoin);

        //Draw quadrant 2
        //Get end value to draw from top-right of 2nd quadrant to bottom-right of 2nd quadrant
        startPoint = new Point(topRightCorner.X, topRightCorner.Y - influenceRange);
        Point endPoint = new(topRightCorner.X + influenceRange, topRightCorner.Y);

        //Following the rules for quadrant 2 - go right and down
        Point currentPoint = new(startPoint.X, startPoint.Y);
        while (endPoint != currentPoint)
        {
            currentPoint = new Point(currentPoint.X, currentPoint.Y + 1);
            sgc.LineTo(_coordinateHelper.GridToScreen(currentPoint, GridSize), stroked, smoothJoin);
            currentPoint = new Point(currentPoint.X + 1, currentPoint.Y);
            sgc.LineTo(_coordinateHelper.GridToScreen(currentPoint, GridSize), stroked, smoothJoin);
        }

        ////////////////////////////////////////////////////////////////
        startPoint = endPoint;
        //Draw in height of object
        sgc.LineTo(_coordinateHelper.GridToScreen(new Point(startPoint.X, bottomRightCorner.Y), GridSize), stroked, smoothJoin);

        //Draw quadrant 3
        //Get end value to draw from top-left of 3rd quadrant to bottom-left of 3rd quadrant
        //Move startPoint to bottomLeftCorner (x value is already correct)
        startPoint = new Point(startPoint.X, bottomRightCorner.Y);
        endPoint = new Point(bottomRightCorner.X, bottomRightCorner.Y + influenceRange);

        //Following the rules for quadrant 3 - go left and down
        currentPoint = new Point(startPoint.X, startPoint.Y);
        while (endPoint != currentPoint)
        {
            currentPoint = new Point(currentPoint.X - 1, currentPoint.Y);
            sgc.LineTo(_coordinateHelper.GridToScreen(currentPoint, GridSize), stroked, smoothJoin);
            currentPoint = new Point(currentPoint.X, currentPoint.Y + 1);
            sgc.LineTo(_coordinateHelper.GridToScreen(currentPoint, GridSize), stroked, smoothJoin);
        }

        ////////////////////////////////////////////////////////////////
        startPoint = endPoint;
        //Draw in width of object
        sgc.LineTo(_coordinateHelper.GridToScreen(new Point(bottomLeftCorner.X, startPoint.Y), GridSize), stroked, smoothJoin);

        //Draw quadrant 4
        //Get end value to draw from bottom-right of 4th quadrant to top-left of 4th quadrant
        //Move startPoint to bottomRightCorner (y value is already correct)
        startPoint = new Point(bottomLeftCorner.X, startPoint.Y);
        endPoint = new Point(bottomLeftCorner.X - influenceRange, bottomRightCorner.Y);

        //Following the rules for quadrant 4 - go up and left
        currentPoint = new Point(startPoint.X, startPoint.Y);
        while (endPoint != currentPoint)
        {
            currentPoint = new Point(currentPoint.X, currentPoint.Y - 1);
            sgc.LineTo(_coordinateHelper.GridToScreen(currentPoint, GridSize), stroked, smoothJoin);
            currentPoint = new Point(currentPoint.X - 1, currentPoint.Y);
            sgc.LineTo(_coordinateHelper.GridToScreen(currentPoint, GridSize), stroked, smoothJoin);
        }

        ////////////////////////////////////////////////////////////////
        startPoint = endPoint;
        //Draw in height of object
        sgc.LineTo(_coordinateHelper.GridToScreen(new Point(startPoint.X, topLeftCorner.Y), GridSize), stroked, smoothJoin);

        //Draw quadrant 1
        //Get end value to draw from bottom-left of 1st quadrant to top-right of 1st quadrant
        //Move startPoint to topLeftCorner (x value is already correct)
        startPoint = new Point(startPoint.X, topLeftCorner.Y);
        endPoint = new Point(topLeftCorner.X, topLeftCorner.Y - influenceRange);

        //Following the rules for quadrant 1 - go up and right
        currentPoint = new Point(startPoint.X, startPoint.Y);
        while (endPoint != currentPoint)
        {
            currentPoint = new Point(currentPoint.X + 1, currentPoint.Y);
            sgc.LineTo(_coordinateHelper.GridToScreen(currentPoint, GridSize), stroked, smoothJoin);
            currentPoint = new Point(currentPoint.X, currentPoint.Y - 1);
            sgc.LineTo(_coordinateHelper.GridToScreen(currentPoint, GridSize), stroked, smoothJoin);
        }

        //Shape should be complete by this point.
    }

    /// <summary>
    /// Add the objects to SelectedObjects, optionally also add all objects which match one of their identifiers.
    /// </summary>
    /// <param name="includeSameObjects"> 
    /// If <see langword="true"/> then apply to objects whose identifier matches one of those in <see cref="objectsToAdd"/>.
    /// </param>
    private void AddSelectedObjects(IEnumerable<LayoutObject> objectsToAdd, bool includeSameObjects)
    {
        if (includeSameObjects)
        {
            // Add all placed objects whose identifier matches any of those in the objectsToAdd.
            SelectedObjects.UnionWith(PlacedObjects.Where(placed => objectsToAdd.Any(toAdd => toAdd.Identifier.Equals(placed.Identifier, StringComparison.OrdinalIgnoreCase))));
        }
        else
        {
            SelectedObjects.UnionWith(objectsToAdd);
        }
    }

    /// <summary>
    /// Remove the objects from SelectedObjects, optionally also remove all objects which match one of their identifiers.
    /// </summary>
    /// <param name="includeSameObjects"> 
    /// If <see langword="true"> then apply to objects whose identifier matches one of those in <see cref="objectsToRemove">.
    /// </param>
    private void RemoveSelectedObjects(IEnumerable<LayoutObject> objectsToRemove, bool includeSameObjects)
    {
        if (includeSameObjects)
        {
            // Exclude any selected objects whose identifier matches any of those in the objectsToRemove.
            _ = SelectedObjects.RemoveWhere(placed => objectsToRemove.Any(toRemove => toRemove.Identifier.Equals(placed.Identifier, StringComparison.OrdinalIgnoreCase)));
        }
        else
        {
            SelectedObjects.ExceptWith(objectsToRemove);
        }
    }

    /// <summary>
    /// Remove the objects from SelectedObjects which match specified predicate.
    /// </summary>
    private void RemoveSelectedObjects(Predicate<LayoutObject> predicate)
    {
        _ = SelectedObjects.RemoveWhere(predicate);
    }

    /// <summary>
    /// Add a single object to SelectedObjects, optionally also add all objects with the same identifier.
    /// </summary>
    /// <param name="includeSameObjects"> 
    /// If <see langword="true"> then apply to objects whose identifier match that of <see cref="objectToAdd">.
    /// </param>
    private void AddSelectedObject(LayoutObject objectToAdd, bool includeSameObjects = false)
    {
        AddSelectedObjects([objectToAdd], includeSameObjects);
    }

    /// <summary>
    /// Remove a single object from SelectedObjects, optionally also remove all objects with the same identifier.
    /// </summary>
    /// <param name="includeSameObjects"> 
    /// If <see langword="true"> then apply to objects whose identifier match that of <see cref="objectToRemove">.
    /// </param>
    private void RemoveSelectedObject(LayoutObject objectToRemove, bool includeSameObjects = false)
    {
        RemoveSelectedObjects([objectToRemove], includeSameObjects);
    }

    private void RecalculateSelectionContainsNotIgnoredObject()
    {
        selectionContainsNotIgnoredObject = SelectedObjects.Any(x => !x.IsIgnoredObject());
    }

    /// <summary>
    /// Used to load current color for grid lines from settings.
    /// </summary>
    /// <remarks>As this method can be called when AppSettings are updated, we make sure to not call anything that relies on the UI thread from here.</remarks>
    private void LoadGridLineColor()
    {
        UserDefinedColor colorFromJson = SerializationHelper.LoadFromJsonString<UserDefinedColor>(_appSettings.ColorGridLines);//explicit variable to make debugging easier
        _gridLinePen = _penCache.GetPen(_brushCache.GetSolidBrush(colorFromJson.Color), DPI_FACTOR * 1);
        double halfPenWidth = _gridLinePen.Thickness / 2;
        GuidelineSet guidelines = new();
        guidelines.GuidelinesX.Add(halfPenWidth);
        guidelines.GuidelinesY.Add(halfPenWidth);
        guidelines.Freeze();
        _guidelineSet = guidelines;
    }

    /// <summary>
    /// Used to load current color for object border lines from settings.
    /// </summary>
    /// <remarks>As this method can be called when AppSettings are updated, we make sure to not call anything that relies on the UI thread from here.</remarks>
    private void LoadObjectBorderLineColor()
    {
        UserDefinedColor colorFromJson = SerializationHelper.LoadFromJsonString<UserDefinedColor>(_appSettings.ColorObjectBorderLines);//explicit variable to make debugging easier
        _linePen = _penCache.GetPen(_brushCache.GetSolidBrush(colorFromJson.Color), DPI_FACTOR * 1);
    }

    /// <summary>
    /// Reindexes given objects in the <see cref="PlacedObjects"/>. This is potentially a very expensive operation.
    /// Calling this method when the LayoutObjects in <paramref name="newPositions"/> and <paramref name="oldPositions"/> do not
    /// match can cause object duplication.
    /// </summary>
    /// <remarks>
    /// When the parameter types were IEnumerable, sequences passed in sometimes got GC'd between calls when using MouseMode.DragAll, 
    /// as the objects were not referenced anywhere between the end of the foreach loop and the AddRange call (the variables
    /// themselves did not count as references due to IEnumerable lazy evaluation).
    /// By making sure the parameters are lists, we avoid this issues.
    /// </remarks>
    private void ReindexMovedObjects()
    {
        foreach ((LayoutObject item, Rect oldBounds) in _oldObjectPositions)
        {
            PlacedObjects.ReIndex(item, oldBounds);
        }
        _oldObjectPositions.Clear();
    }

    /// <summary>
    /// Forces an update to the parent <see cref="ScrollViewer"/> on the next render (if necessary), and
    /// recomputes <see cref="_scrollableBounds"/>, which represents the currently scrollable area.
    /// This computation relies on <see cref="_layoutBounds"/> being up to date.
    /// </summary>
    private void InvalidateScroll()
    {
        //make sure the scrollable area encompasses the current viewport plus the bounding rect of the current layout
        Rect r = _viewport.Absolute;
        r.Union(_layoutBounds);
        _scrollableBounds = r;

        //update scroll viewer on next render
        _invalidateScrollInfo = true;
        _isRenderingForced = true;
    }

    /// <summary>
    /// Computes the bounds of the current layout
    /// </summary>
    private void InvalidateBounds()
    {
        _layoutBounds = ComputeBoundingRect(PlacedObjects);

    }
    #endregion

    #region Coordinate and rectangle conversions

    /// <summary>
    /// Rotates a group of objects 90 degrees clockwise around point (0, 0).
    /// </summary>
    /// <param name="objects"></param>
    /// <returns>Lazily evaluated iterator which rotates each object and return tuple of the rotate item and its old rectangle.</returns>
    private IEnumerable<(LayoutObject item, Rect oldRect)> Rotate(IEnumerable<LayoutObject> objects)
    {
        foreach (LayoutObject item in objects)
        {
            Rect newRect = _coordinateHelper.Rotate(item.Bounds);
            Rect oldRect = item.Bounds;
            item.Bounds = newRect;
            item.Rotation = _coordinateHelper.Rotate(item.Rotation);
            yield return (item, oldRect);
        }
    }

    #endregion

    #region Event handling

    #region Mouse

    protected override void OnMouseEnter(MouseEventArgs e)
    {
        _mouseWithinControl = true;
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        _mouseWithinControl = false;

        //clear selection rectangle
        CurrentMode = MouseMode.Standard;
        _selectionRect = Rect.Empty;

        //update object positions if dragging
        ReindexMovedObjects();

        InvalidateVisual();
    }

    /// <summary>
    /// Handles the zoom level
    /// </summary>
    /// <param name="e"></param>
    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        //We subtract from ZoomSensitivitySliderMaximum here to get the inverse of the value (e.g 100(%) becomes 1(%), and 1(%) becomes 100(%))
        double zoomFactor = ((Constants.ZoomSensitivitySliderMaximum + 1 - _appSettings.ZoomSensitivityPercentage) * Constants.ZoomSensitivityCoefficient) + Constants.ZoomSensitivityMinimum;
        int change = (int)(e.Delta / zoomFactor);
        //change by at least 1
        if (change == 0)
        {
            change = e.Delta > 0 ? 1 : -1;
        }

        if (!_appSettings.UseZoomToPoint)
        {
            GridSize += change;
        }
        else
        {
            Point mousePosition = _mousePosition;
            Point preZoomPosition = _coordinateHelper.ScreenToFractionalGrid(mousePosition, GridSize);
            GridSize += change;
            Point postZoomPosition = _coordinateHelper.ScreenToFractionalGrid(mousePosition, GridSize);
            Vector diff = preZoomPosition - postZoomPosition;
            _viewport.Left += diff.X;
            _viewport.Top += diff.Y;
        }

        //if there are no objects placed down, then reset to viewport to 0,0, whilst maintaining any offsets to hide the change
        if (PlacedObjects.Count == 0)
        {
            _viewport.Left = _viewport.HorizontalAlignmentValue >= 0 ? 1 - _viewport.HorizontalAlignmentValue : Math.Abs(_viewport.HorizontalAlignmentValue);
            _viewport.Top = _viewport.VerticalAlignmentValue >= 0 ? 1 - _viewport.VerticalAlignmentValue : Math.Abs(_viewport.VerticalAlignmentValue);
        }

        InvalidateScroll();
    }

    private void HandleMouse(MouseEventArgs e)
    {
        _ = Focus();
        // refresh retrieved mouse position
        _mousePosition = e.GetPosition(this);
        MoveCurrentObjectsToMouse();
    }

    /// <summary>
    /// Handles pressing of mouse buttons
    /// </summary>
    /// <param name="e"></param>
    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        if (!IsFocused)
        {
            _ = Focus();
        }

        HandleMouse(e);
        HotkeyCommandManager.HandleCommand(e);
        _mouseDragStart = _mousePosition;

        if (e.LeftButton == MouseButtonState.Pressed && e.RightButton == MouseButtonState.Pressed)
        {
            //If the previous mode was DragSelection, we may have moved an object
            //UpdateObjectPositions is usually called on MouseUp, which will not fire if the current MouseMode is
            //DragAll, so we fire it here instead, to prevent objects being incorrectly represented within the QuadTree.
            if (CurrentMode == MouseMode.DragSelection)
            {
                UndoManager.RegisterOperation(new MoveObjectsOperation<LayoutObject>()
                {
                    ObjectPropertyValues = _oldObjectPositions.Select(pair => (pair.Item, pair.OldGridRect, pair.Item.Bounds)).ToList(),
                    QuadTree = PlacedObjects
                });

                ReindexMovedObjects();
            }

            CurrentMode = MouseMode.DragAllStart;
        }
        else if (e.LeftButton == MouseButtonState.Pressed && CurrentObjects.Count != 0)
        {
            // place new object
            _ = TryPlaceCurrentObjects(isContinuousDrawing: false);
        }
        else if (e.LeftButton == MouseButtonState.Pressed && CurrentObjects.Count == 0)
        {
            LayoutObject obj = GetObjectAt(_mousePosition);
            if (obj is null)
            {
                // user clicked nothing: start dragging the selection rect
                CurrentMode = MouseMode.SelectionRectStart;
                _unselectedObjects = null;
            }
            else if (!(IsControlPressed() || IsShiftPressed()))
            {
                CurrentMode = SelectedObjects.Contains(obj) ? MouseMode.DragSelectionStart : MouseMode.DragSingleStart;
                _unselectedObjects = null;
            }
        }

        InvalidateVisual();
    }

    private List<LayoutObject> _unselectedObjects = null;

    /// <summary>
    /// Here be dragons.
    /// </summary>
    protected override void OnMouseMove(MouseEventArgs e)
    {
        HandleMouse(e);

        // check if user begins to drag
        if (Math.Abs(_mouseDragStart.X - _mousePosition.X) >= 1 || Math.Abs(_mouseDragStart.Y - _mousePosition.Y) >= 1)
        {
            switch (CurrentMode)
            {
                case MouseMode.SelectionRectStart:
                    CurrentMode = MouseMode.SelectionRect;
                    _selectionRect = new Rect();
                    break;
                case MouseMode.DragSelectionStart:
                    CurrentMode = MouseMode.DragSelection;
                    break;
                case MouseMode.DragSingleStart:
                    SelectedObjects.Clear();
                    LayoutObject obj = GetObjectAt(_mouseDragStart);
                    AddSelectedObject(obj, ShouldAffectObjectsWithIdentifier());
                    RecalculateSelectionContainsNotIgnoredObject();
                    //after adding the object, compute the collision rect
                    _collisionRect = obj.GridRect;
                    CurrentMode = MouseMode.DragSelection;
                    break;
                case MouseMode.DragAllStart:
                    CurrentMode = MouseMode.DragAll;
                    break;
            }
        }

        if (CurrentMode == MouseMode.DragAll)
        {
            // move all selected objects
            int dx = (int)_coordinateHelper.ScreenToGrid(_mousePosition.X - _mouseDragStart.X, GridSize);
            int dy = (int)_coordinateHelper.ScreenToGrid(_mousePosition.Y - _mouseDragStart.Y, GridSize);

            //shift the viewport;
            if (_appSettings.InvertPanningDirection)
            {
                _viewport.Left -= dx;
                _viewport.Top -= dy;
            }
            else
            {
                _viewport.Left += dx;
                _viewport.Top += dy;
            }

            // adjust the drag start to compensate the amount we already moved
            _mouseDragStart.X += _coordinateHelper.GridToScreen(dx, GridSize);
            _mouseDragStart.Y += _coordinateHelper.GridToScreen(dy, GridSize);

            //invalidate scroll info on next render;
            InvalidateScroll();
        }
        else if (e.LeftButton == MouseButtonState.Pressed)
        {
            if (CurrentObjects.Count != 0)
            {
                CurrentMode = MouseMode.PlaceObjects;
                // place new object
                _ = TryPlaceCurrentObjects(isContinuousDrawing: true);
            }
            else
            {
                // selection of multiple objects
                switch (CurrentMode)
                {
                    case MouseMode.SelectionRect:
                        {
                            if (IsControlPressed() || IsShiftPressed())
                            {
                                // remove previously selected by the selection rect
                                if (ShouldAffectObjectsWithIdentifier())
                                {
                                    RemoveSelectedObjects(
                                        SelectedObjects.Where(_ => _.CalculateScreenRect(GridSize).IntersectsWith(_selectionRect)).ToList(),
                                        true
                                    );
                                }
                                else
                                {
                                    RemoveSelectedObjects(x => x.CalculateScreenRect(GridSize).IntersectsWith(_selectionRect));
                                }
                            }
                            else
                            {
                                SelectedObjects.Clear();
                            }

                            // adjust rect
                            _selectionRect = new Rect(_mouseDragStart, _mousePosition);
                            // select intersecting objects
                            Rect selectionRectGrid = _coordinateHelper.ScreenToGrid(_selectionRect, GridSize);
                            selectionRectGrid = _viewport.OriginToViewport(selectionRectGrid);
                            AddSelectedObjects(PlacedObjects.GetItemsIntersecting(selectionRectGrid),
                                               ShouldAffectObjectsWithIdentifier());
                            RecalculateSelectionContainsNotIgnoredObject();

                            StatisticsUpdated?.Invoke(this, UpdateStatisticsEventArgs.All);
                            break;
                        }
                    case MouseMode.DragSelection:
                        {
                            if (_oldObjectPositions.Count == 0)
                            {
                                _oldObjectPositions.AddRange(SelectedObjects.Select(obj => (obj, obj.GridRect)));
                            }

                            // move all selected objects
                            int dx = (int)_coordinateHelper.ScreenToGrid(_mousePosition.X - _mouseDragStart.X, GridSize);
                            int dy = (int)_coordinateHelper.ScreenToGrid(_mousePosition.Y - _mouseDragStart.Y, GridSize);
                            // check if the mouse has moved at least one grid cell in any direction
                            if (dx == 0 && dy == 0)
                            {
                                //no relevant mouse move -> no further action
                                break;
                            }
                            //Recompute _unselectedObjects
                            Rect offsetCollisionRect = _collisionRect;
                            offsetCollisionRect.Offset(dx, dy);

                            //Its causing slowdowns when dragging large numbers of objects
                            _unselectedObjects = PlacedObjects.GetItemsIntersecting(offsetCollisionRect).Where(_ => !SelectedObjects.Contains(_)).ToList();
                            bool collisionsExist = false;
                            // temporarily move each object and check if collisions with unselected objects exist
                            foreach (LayoutObject curLayoutObject in SelectedObjects)
                            {
                                Point originalPosition = curLayoutObject.Position;
                                // move object                                
                                curLayoutObject.Position = new Point(curLayoutObject.Position.X + dx, curLayoutObject.Position.Y + dy);
                                // check for collisions                                
                                bool collides = _unselectedObjects.Find(_ => ObjectIntersectionExists(curLayoutObject, _)) != null;
                                curLayoutObject.Position = originalPosition;
                                if (collides)
                                {
                                    collisionsExist = true;
                                    break;
                                }
                            }

                            // if no collisions were found, permanently move all selected objects
                            if (!collisionsExist)
                            {
                                foreach (LayoutObject curLayoutObject in SelectedObjects)
                                {
                                    curLayoutObject.Position = new Point(curLayoutObject.Position.X + dx, curLayoutObject.Position.Y + dy);
                                }
                                // adjust the drag start to compensate the amount we already moved
                                _mouseDragStart.X += _coordinateHelper.GridToScreen(dx, GridSize);
                                _mouseDragStart.Y += _coordinateHelper.GridToScreen(dy, GridSize);

                                //update collision rect, so that collisions are correctly computed on next run
                                _collisionRect.X += dx;
                                _collisionRect.Y += dy;

                                //position change -> update
                                StatisticsUpdated?.Invoke(this, new UpdateStatisticsEventArgs(UpdateMode.NoBuildingList));
                                //always recompute bounds when moving, as we may be moving an item in from the edge of the layout
                                Rect oldLayoutBounds = _layoutBounds;
                                InvalidateBounds();
                                if (oldLayoutBounds != _layoutBounds)
                                {
                                    InvalidateScroll();
                                }
                            }

                            ForceRendering();
                            return;
                        }
                }
            }
        }

        InvalidateVisual();
    }

    /// <summary>
    /// Handles the release of mouse buttons.
    /// </summary>
    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        HandleMouse(e);

        if (CurrentMode == MouseMode.DragAll)
        {
            if (e.LeftButton == MouseButtonState.Released && e.RightButton == MouseButtonState.Released)
            {
                CurrentMode = MouseMode.Standard;
            }

            return;
        }

        if (e.ChangedButton == MouseButton.Left && CurrentObjects.Count == 0)
        {
            switch (CurrentMode)
            {
                default:
                    {
                        // clear selection if no key is pressed
                        if (!(IsControlPressed() || IsShiftPressed()))
                        {
                            SelectedObjects.Clear();
                        }

                        LayoutObject obj = GetObjectAt(_mousePosition);
                        if (obj != null)
                        {
                            // user clicked an object: select or deselect it
                            if (SelectedObjects.Contains(obj))
                            {
                                RemoveSelectedObject(obj);
                            }
                            else
                            {
                                AddSelectedObject(obj);
                            }
                            RecalculateSelectionContainsNotIgnoredObject();
                        }

                        _collisionRect = ComputeBoundingRect(SelectedObjects);
                        StatisticsUpdated?.Invoke(this, UpdateStatisticsEventArgs.All);
                        // return to standard mode, i.e. clear any drag-start modes
                        CurrentMode = MouseMode.Standard;
                        if (selectionContainsNotIgnoredObject)
                        {
                            RemoveSelectedObjects(Extensions.IEnumerableExtensions.IsIgnoredObject);
                        }
                        break;
                    }
                case MouseMode.SelectSameIdentifier:
                    {
                        CurrentMode = MouseMode.Standard;
                        break;
                    }
                case MouseMode.SelectionRect:
                    _collisionRect = ComputeBoundingRect(SelectedObjects);
                    // cancel dragging of selection rect
                    CurrentMode = MouseMode.Standard;
                    if (selectionContainsNotIgnoredObject)
                    {
                        RemoveSelectedObjects(Extensions.IEnumerableExtensions.IsIgnoredObject);
                    }
                    break;
                case MouseMode.DragSelection:
                    // stop dragging of selected objects
                    UndoManager.RegisterOperation(new MoveObjectsOperation<LayoutObject>()
                    {
                        ObjectPropertyValues = _oldObjectPositions.Select(pair => (pair.Item, pair.OldGridRect, pair.Item.Bounds)).ToList(),
                        QuadTree = PlacedObjects
                    });

                    ReindexMovedObjects();
                    if (SelectedObjects.Count == 1)
                    {
                        SelectedObjects.Clear();
                    }
                    CurrentMode = MouseMode.Standard;
                    break;
            }
        }
        else if (e.ChangedButton == MouseButton.Left && CurrentObjects.Count != 0)
        {
            CurrentMode = MouseMode.PlaceObjects;
        }
        else if (e.ChangedButton == MouseButton.Right)
        {
            switch (CurrentMode)
            {
                case MouseMode.PlaceObjects:
                case MouseMode.DeleteObject:
                case MouseMode.Standard:
                    {
                        if (CurrentObjects.Count != 0)
                        {
                            // cancel placement of object
                            CurrentObjects.Clear();
                        }

                        CurrentMode = MouseMode.Standard;
                        break;
                    }
                case MouseMode.DragSelection:
                    {
                        UndoManager.RegisterOperation(new MoveObjectsOperation<LayoutObject>()
                        {
                            ObjectPropertyValues = _oldObjectPositions.Select(pair => (pair.Item, pair.OldGridRect, pair.Item.Bounds)).ToList(),
                            QuadTree = PlacedObjects
                        });

                        ReindexMovedObjects();
                        //clear selection after potentially modifying QuadTree
                        SelectedObjects.Clear();

                        if (CurrentObjects.Count != 0)
                        {
                            // cancel placement of object
                            CurrentObjects.Clear();
                        }

                        CurrentMode = MouseMode.Standard;
                        break;
                    }
                case MouseMode.SelectSameIdentifier:
                    {
                        CurrentMode = MouseMode.Standard;
                        break;
                    }
            }
        }
        else if (e.ChangedButton == MouseButton.Right)
        {
            switch (CurrentMode)
            {
                case MouseMode.SelectSameIdentifier:
                    {
                        CurrentMode = MouseMode.Standard;
                        break;
                    }
            }
        }
        else if (e.ChangedButton == MouseButton.XButton1)
        {
            switch (CurrentMode)
            {
                case MouseMode.SelectSameIdentifier:
                    {
                        CurrentMode = MouseMode.Standard;
                        break;
                    }
            }
        }
        else if (e.ChangedButton == MouseButton.XButton2)
        {
            switch (CurrentMode)
            {
                case MouseMode.SelectSameIdentifier:
                    {
                        CurrentMode = MouseMode.Standard;
                        break;
                    }
            }
        }

        InvalidateVisual();
    }

    #endregion

    #region Keyboard

    /// <summary>
    /// Handles key presses
    /// </summary>
    /// <param name="e"></param>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        //Used here instead of adding to the InputBindingsCollection as we don't want run `Binding.Matches` on *every* event.
        //When an InputBinding is added to the InputBindingsCollection, the  `Matches` method is fired for every event - KeyUp,
        //KeyDown, MouseUp, MouseMove, MouseWheel etc.
        HotkeyCommandManager.HandleCommand(e);

        if (e.Handled)
        {
            InvalidateVisual();
        }

    }

    /// <summary>
    /// Checks whether the user is pressing the control key.
    /// </summary>
    /// <returns><see langword="true"> if the control key is pressed, otherwise <see langword="false">.</returns>
    private static bool IsControlPressed()
    {
        return (Keyboard.Modifiers & ModifierKeys.Control) != 0;
    }

    /// <summary>
    /// Checks whether the user is pressing the shift key.
    /// </summary>
    /// <returns><see langword="true"> if the shift key is pressed, otherwise <see langword="false">.</returns>
    private static bool IsShiftPressed()
    {
        return (Keyboard.Modifiers & ModifierKeys.Shift) != 0;
    }

    /// <summary>
    /// Checks whether actions should affect all objects with the same identifier.
    /// </summary>
    /// <returns><see langword="true"/> if all objects with same identifier should be affected, otherwise <see langword="false"/>.</returns>
    private static bool ShouldAffectObjectsWithIdentifier()
    {
        return IsShiftPressed() && IsControlPressed();
    }

    #endregion

    #endregion

    #region Collision handling

    /// <summary>
    /// Checks if there is a collision between given objects a and b.
    /// </summary>
    /// <param name="a">first object</param>
    /// <param name="b">second object</param>
    /// <returns>true if there is a collision, otherwise false</returns>
    private static bool ObjectIntersectionExists(LayoutObject a, LayoutObject b)
    {
        return a.CollisionRect.IntersectsWith(b.CollisionRect);
    }

    /// <summary>
    /// Checks if there is a collision between a list of AnnoObjects a and object b.
    /// </summary>
    /// <param name="a">List of objects</param>
    /// <param name="b">second object</param>
    /// <returns>true if there is a collision, otherwise false</returns>
    private static bool ObjectIntersectionExists(IEnumerable<LayoutObject> a, LayoutObject b)
    {
        return a.Any(_ => _.CollisionRect.IntersectsWith(b.CollisionRect));
    }

    /// <summary>
    /// Tries to place current objects on the grid.
    /// Fails if there are any collisions.
    /// </summary>
    /// <param name="isContinuousDrawing"><c>true</c> if drawing the same object(s) over and over</param>
    /// <returns>true if placement succeeded, otherwise false</returns>
    private bool TryPlaceCurrentObjects(bool isContinuousDrawing)
    {
        if (CurrentObjects.Count == 0)
        {
            return true;
        }

        Rect boundingRect = ComputeBoundingRect(CurrentObjects);
        IEnumerable<LayoutObject> relevantPlacedObjects = PlacedObjects.GetItemsIntersecting(boundingRect);
        IEnumerable<LayoutObject> intersectingCurrentObjects = CurrentObjects.Where(x => ObjectIntersectionExists(relevantPlacedObjects, x));

        if (IsShiftPressed() || !intersectingCurrentObjects.Any())
        {
            List<LayoutObject> newObjects = CloneLayoutObjects(CurrentObjects.Except(intersectingCurrentObjects), CurrentObjects.Count);
            UndoManager.RegisterOperation(new AddObjectsOperation<LayoutObject>()
            {
                Objects = newObjects,
                Collection = PlacedObjects
            });

            PlacedObjects.AddRange(newObjects);
            StatisticsUpdated?.Invoke(this, UpdateStatisticsEventArgs.All);

            //no need to update colors if drawing the same object(s)
            if (!isContinuousDrawing)
            {
                ColorsInLayoutUpdated?.Invoke(this, EventArgs.Empty);
            }

            if (!_layoutBounds.Contains(boundingRect))
            {
                InvalidateBounds();
            }

            if (!_scrollableBounds.Contains(boundingRect))
            {
                InvalidateScroll();
            }

            return true;
        }

        return false;
    }

    private Size _intersectingRectSize = new(1, 1);

    /// <summary>
    /// Retrieves the object at the given position given in screen coordinates.
    /// </summary>
    /// <param name="position">position given in screen coordinates</param>
    /// <returns>object at the position, <see langword="null"/> if no object could be found</returns>
    private LayoutObject GetObjectAt(Point position)
    {
        if (PlacedObjects.Count == 0)
        {
            return null;
        }

        Point gridPosition = _coordinateHelper.ScreenToFractionalGrid(position, GridSize);
        gridPosition = _viewport.OriginToViewport(gridPosition);
        IEnumerable<LayoutObject> possibleItems = PlacedObjects.GetItemsIntersecting(new Rect(gridPosition, _intersectingRectSize));
        foreach (LayoutObject curItem in possibleItems)
        {
            if (curItem.GridRect.Contains(gridPosition))
            {
                return curItem;
            }
        }

        return null;
    }

    /// <summary>
    /// Computes a <see cref="Rect"/> that encompasses the given objects.
    /// </summary>
    /// <param name="objects">The collection of <see cref="LayoutObject"/> to compute the bounding <see cref="Rect"/> for.</param>
    /// <returns>The <see cref="Rect"/> that encompasses all <paramref name="objects"/>.</returns>
    public Rect ComputeBoundingRect(IEnumerable<LayoutObject> objects)
    {
        //make sure to include ALL objects (e.g. roads and ignored objetcs)
        return (Rect)_statisticsCalculationHelper.CalculateStatistics(objects.Select(_ => _.WrappedAnnoObject), includeRoads: true, includeIgnoredObjects: true);
    }

    #endregion

    #region API

    /// <summary>
    /// Sets the current object, i.e. the object which the user can place.
    /// </summary>
    /// <param name="obj">object to apply</param>
    public void SetCurrentObject(LayoutObject obj)
    {
        obj.Position = _mousePosition;
        // note: setting of the backing field doesn't fire the changed event
        _currentObjects.Clear();
        _currentObjects.Add(obj);
        InvalidateVisual();
    }

    /// <summary>
    /// Resets the zoom to the default level.
    /// </summary>
    public void ResetZoom()
    {
        GridSize = Constants.GridStepDefault;
    }

    /// <summary>
    /// Normalizes the layout with border parameter set to zero.
    /// </summary>
    public void Normalize()
    {
        Normalize(0);
    }

    /// <summary>
    /// Normalizes the layout, i.e. moves all objects so that the top-most and left-most objects are exactly at the top and left coordinate zero if border is zero.
    /// Otherwise moves all objects further to the bottom-right by border in grid-units.
    /// </summary>
    /// <param name="border"></param>
    public void Normalize(int border)
    {
        if (PlacedObjects.Count == 0)
        {
            return;
        }

        double dx = PlacedObjects.Min(_ => _.Position.X) - border;
        double dy = PlacedObjects.Min(_ => _.Position.Y) - border;
        Vector diff = new(dx, dy);

        if (diff.LengthSquared > 0)
        {
            UndoManager.RegisterOperation(new MoveObjectsOperation<LayoutObject>()
            {
                ObjectPropertyValues = PlacedObjects.Select(obj => (obj, obj.Bounds, new Rect(obj.Position - diff, obj.Size))).ToList(),
                QuadTree = PlacedObjects
            });

            // make a copy of a list to avoid altering collection during iteration
            List<LayoutObject> placedObjects = [.. PlacedObjects];

            foreach (LayoutObject item in placedObjects)
            {
                PlacedObjects.Move(item, -diff);
            }

            InvalidateVisual();
            InvalidateBounds();
            InvalidateScroll();
        }
    }

    /// <summary>
    /// Resets viewport of the canvas to top left corner.
    /// </summary>
    public void ResetViewport()
    {
        _viewport.Top = 0;
        _viewport.Left = 0;
    }

    /// <summary>
    /// Registers hotkeys with the <see cref="HotkeyCommandManager"/>.
    /// </summary>
    /// <param name="manager"></param>
    public void RegisterHotkeys(HotkeyCommandManager manager)
    {
        HotkeyCommandManager = manager;
        manager.AddHotkey(rotateHotkey1);
        manager.AddHotkey(rotateHotkey2);
        manager.AddHotkey(rotateAllHotkey);
        manager.AddHotkey(copyHotkey);
        manager.AddHotkey(pasteHotkey);
        manager.AddHotkey(deleteHotkey);
        manager.AddHotkey(duplicateHotkey);
        manager.AddHotkey(deleteObjectUnderCursorHotkey);
        manager.AddHotkey(undoHotkey);
        manager.AddHotkey(redoHotkey);
        manager.AddHotkey(enableDebugModeHotkey);
        manager.AddHotkey(selectAllSameIdentifierHotkey);
    }

    #endregion

    #region New/Save/Load/Export methods

    public async void CheckUnsavedChangesBeforeCrash()
    {
        if (UndoManager.IsDirty)
        {
            bool save = await _messageBoxService.ShowQuestion(
                _localizationHelper.GetLocalization("SaveUnsavedChanges"),
                _localizationHelper.GetLocalization("UnsavedChangedBeforeCrash")
            );

            if (save)
            {
                _ = SaveAs();
            }
        }
    }

    /// <summary>
    /// Checks for unsaved changes. Shows Yes/No/Cancel dialog to let user decide what to do.
    /// </summary>
    /// <returns>True if changes were saved or discarded. False if operation should be cancelled.</returns>
    public async Task<bool> CheckUnsavedChanges()
    {
        if (UndoManager.IsDirty)
        {
            bool? save = await _messageBoxService.ShowQuestionWithCancel(
                _localizationHelper.GetLocalization("SaveUnsavedChanges"),
                _localizationHelper.GetLocalization("UnsavedChanged")
            );

            if (save == null)
            {
                return false;
            }
            if (save.Value)
            {
                return Save();
            }
        }

        return true;
    }

    /// <summary>
    /// Removes all objects from the grid.
    /// </summary>
    public async Task NewFileAsync()
    {
        if (!await CheckUnsavedChanges())
        {
            return;
        }

        ResetViewport();
        PlacedObjects.Clear();
        SelectedObjects.Clear();
        UndoManager.Clear();
        LoadedFile = "";
        InvalidateBounds();
        InvalidateScroll();
        InvalidateVisual();

        StatisticsUpdated?.Invoke(this, UpdateStatisticsEventArgs.All);
        ColorsInLayoutUpdated?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Saves the current layout to file.
    /// </summary>
    public bool Save()
    {
        if (string.IsNullOrEmpty(LoadedFile))
        {
            return SaveAs();
        }
        else
        {
            SaveFileRequested?.Invoke(this, new SaveFileEventArgs(LoadedFile));
            // Fire OnLoadedFileChanged event to trigger recalculation of MainWindowTitle
            OnLoadedFileChanged?.Invoke(this, new FileLoadedEventArgs(LoadedFile));
            return true;
        }
    }

    /// <summary>
    /// Opens a dialog and saves the current layout to file.
    /// </summary>
    public bool SaveAs()
    {
        SaveFileDialog dialog = new()
        {
            DefaultExt = Constants.SavedLayoutExtension,
            Filter = Constants.SaveOpenDialogFilter
        };

        if (dialog.ShowDialog() == true)
        {
            LoadedFile = dialog.FileName;
            SaveFileRequested?.Invoke(this, new SaveFileEventArgs(LoadedFile));
            return true;
        }
        return false;
    }

    /// <summary>
    /// Opens a dialog and loads the given file.
    /// </summary>
    public async Task OpenFileAsync()
    {
        if (!await CheckUnsavedChanges())
        {
            return;
        }

        OpenFileDialog dialog = new()
        {
            DefaultExt = Constants.SavedLayoutExtension,
            Filter = Constants.SaveOpenDialogFilter
        };

        if (dialog.ShowDialog() == true)
        {
            OpenFileRequested?.Invoke(this, new OpenFileEventArgs(dialog.FileName));
            InvalidateBounds();
        }
    }

    #endregion

    #region Commands

    /// <summary>
    /// Holds event handlers for command executions.
    /// </summary>
    private static readonly Dictionary<ICommand, Action<AnnoCanvas>> CommandExecuteMappings;

    public HotkeyCommandManager HotkeyCommandManager { get; set; }
    /// <summary>
    /// Creates event handlers for command executions and registers them at the CommandManager.
    /// </summary>
    static AnnoCanvas()
    {
        // create event handler mapping
        CommandExecuteMappings = new Dictionary<ICommand, Action<AnnoCanvas>>
        {
            { ApplicationCommands.New, _ => _.NewFileAsync() },
            { ApplicationCommands.Open, _ => _.OpenFileAsync() },
            { ApplicationCommands.Save, _ => _.Save() },
            { ApplicationCommands.SaveAs, _ => _.SaveAs() }
        };

        // register event handlers for the specified commands
        foreach (KeyValuePair<ICommand, Action<AnnoCanvas>> action in CommandExecuteMappings)
        {
            CommandManager.RegisterClassCommandBinding(typeof(AnnoCanvas), new CommandBinding(action.Key, ExecuteCommand));
        }
    }

    /// <summary>
    /// Handler for all executed command events.
    ///  </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private static void ExecuteCommand(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is AnnoCanvas canvas && CommandExecuteMappings.TryGetValue(e.Command, out Action<AnnoCanvas> value))
        {
            value.Invoke(canvas);
            e.Handled = true;
        }
    }

    /// <summary>
    /// R key rotate
    /// </summary>
    private readonly Hotkey rotateHotkey1;
    /// <summary>
    /// MiddleClick rotate
    /// </summary>
    private readonly Hotkey rotateHotkey2;
    public ICommand RotateCommand { get; private set; }
    private void ExecuteRotate(object param)
    {
        if (CurrentObjects.Count == 1)
        {
            CurrentObjects[0].Size = _coordinateHelper.Rotate(CurrentObjects[0].Size);
            CurrentObjects[0].Rotation = _coordinateHelper.Rotate(CurrentObjects[0].Rotation);
        }
        else if (CurrentObjects.Count > 1)
        {
            Rotate(CurrentObjects).Consume();
        }
        else
        {
            //Count == 0;
            //Rotate from selected objects
            CurrentObjects = CloneLayoutObjects(SelectedObjects);
            Rotate(CurrentObjects).Consume();
        }
    }

    private readonly Hotkey rotateAllHotkey;
    private readonly ICommand rotateAllCommand;
    private void ExecuteRotateAll(object param)
    {
        UndoManager.AsSingleUndoableOperation(() =>
        {
            List<LayoutObject> placedObjects = [.. PlacedObjects];
            UndoManager.RegisterOperation(new MoveObjectsOperation<LayoutObject>()
            {
                QuadTree = PlacedObjects,
                ObjectPropertyValues = PlacedObjects.Select(obj => (obj, obj.Bounds, _coordinateHelper.Rotate(obj.Bounds))).ToList()
            });

            foreach ((LayoutObject item, Rect oldRect) in Rotate(placedObjects))
            {
                PlacedObjects.ReIndex(item, oldRect);
            }
            Normalize(1);
        });
    }

    private readonly Hotkey copyHotkey;
    private readonly ICommand copyCommand;
    private void ExecuteCopy(object param)
    {
        if (SelectedObjects.Count != 0)
        {
            ClipboardService.Copy(SelectedObjects.Select(x => x.WrappedAnnoObject));

            string localizedMessage = SelectedObjects.Count == 1 ? _localizationHelper.GetLocalization("ItemCopied") : _localizationHelper.GetLocalization("ItemsCopied");
            StatusMessage = $"{SelectedObjects.Count} {localizedMessage}";
        }
    }

    private readonly Hotkey pasteHotkey;
    private readonly ICommand pasteCommand;
    private void ExecutePaste(object param)
    {
        ICollection<AnnoObject> objects = ClipboardService.Paste();
        if (objects.Count > 0)
        {
            CurrentObjects = objects.Select(x => new LayoutObject(x, _coordinateHelper, _brushCache, _penCache)).ToList();
        }
    }

    private readonly Hotkey deleteHotkey;
    private readonly ICommand deleteCommand;
    private void ExecuteDelete(object param)
    {
        UndoManager.RegisterOperation(new RemoveObjectsOperation<LayoutObject>()
        {
            Objects = [.. SelectedObjects],
            Collection = PlacedObjects
        });

        // remove all currently selected objects from the grid and clear selection    
        foreach (LayoutObject item in SelectedObjects)
        {
            _ = PlacedObjects.Remove(item);
        }
        SelectedObjects.Clear();
        StatisticsUpdated?.Invoke(this, UpdateStatisticsEventArgs.All);
        CurrentMode = MouseMode.DeleteObject;
    }

    private readonly Hotkey duplicateHotkey;
    private readonly ICommand duplicateCommand;
    private void ExecuteDuplicate(object param)
    {
        LayoutObject obj = GetObjectAt(_mousePosition);
        if (obj != null)
        {
            CurrentObjects.Clear();
            CurrentObjects.Add(new LayoutObject(new AnnoObject(obj.WrappedAnnoObject), _coordinateHelper, _brushCache, _penCache));
            OnCurrentObjectChanged(obj);
        }
    }

    private readonly Hotkey deleteObjectUnderCursorHotkey;
    private readonly ICommand deleteObjectUnderCursorCommand;
    private void ExecuteDeleteObjectUnderCursor(object param)
    {
        if (CurrentObjects.Count == 0)
        {
            LayoutObject obj = GetObjectAt(_mousePosition);
            if (obj != null)
            {
                // Remove object, only ever remove a single object this way.
                UndoManager.RegisterOperation(new RemoveObjectsOperation<LayoutObject>()
                {
                    Objects =
                    [
                        obj
                    ],
                    Collection = PlacedObjects
                });

                _ = PlacedObjects.Remove(obj);
                RemoveSelectedObject(obj);
                RecalculateSelectionContainsNotIgnoredObject();
                StatisticsUpdated?.Invoke(this, UpdateStatisticsEventArgs.All);
                CurrentMode = MouseMode.DeleteObject;

                InvalidateVisual();
            }
        }
    }

    private readonly Hotkey undoHotkey;
    private readonly ICommand undoCommand;
    private void ExecuteUndo(object param)
    {
        UndoManager.Undo();
        ForceRendering();
        StatisticsUpdated?.Invoke(this, UpdateStatisticsEventArgs.All);
    }

    private readonly Hotkey redoHotkey;
    private readonly ICommand redoCommand;
    private void ExecuteRedo(object param)
    {
        UndoManager.Redo();
        ForceRendering();
        StatisticsUpdated?.Invoke(this, UpdateStatisticsEventArgs.All);
    }

    private readonly Hotkey selectAllSameIdentifierHotkey;
    private readonly ICommand selectAllSameIdentifierCommand;
    private void ExecuteSelectAllSameIdentifier(object param)
    {
        //select all objects with same identifier as object under mouse cursor
        LayoutObject objectToCheck = GetObjectAt(_mousePosition);
        if (objectToCheck != null)
        {
            CurrentMode = MouseMode.SelectSameIdentifier;

            if (SelectedObjects.Contains(objectToCheck))
            {
                RemoveSelectedObject(objectToCheck, includeSameObjects: true);
            }
            else
            {
                AddSelectedObject(objectToCheck, includeSameObjects: true);
            }

            RecalculateSelectionContainsNotIgnoredObject();
            ForceRendering();
            StatisticsUpdated?.Invoke(this, UpdateStatisticsEventArgs.All);
        }
    }

    private readonly Hotkey enableDebugModeHotkey;
    private readonly ICommand enableDebugModeCommand;
    private void ExecuteEnableDebugMode(object param)
    {
        _debugModeIsEnabled = !_debugModeIsEnabled;
        ForceRendering();
    }

    #endregion

    #region Helper methods

    private List<LayoutObject> CloneLayoutObjects(HashSet<LayoutObject> list)
    {
        ArgumentNullException.ThrowIfNull(list);

        return list.Select(x => new LayoutObject(new AnnoObject(x.WrappedAnnoObject), _coordinateHelper, _brushCache, _penCache)).ToListWithCapacity(list.Count);
    }

    private List<LayoutObject> CloneLayoutObjects(IEnumerable<LayoutObject> list, int capacity)
    {
        return list.Select(x => new LayoutObject(new AnnoObject(x.WrappedAnnoObject), _coordinateHelper, _brushCache, _penCache)).ToListWithCapacity(capacity);
    }

    private void UpdateScrollBarVisibility()
    {
        if (ScrollOwner != null)
        {
            if (_showScrollBars)
            {
                ScrollOwner.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
                ScrollOwner.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
            }
            else
            {
                ScrollOwner.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                ScrollOwner.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            }
        }
    }

    #endregion

    public void RaiseStatisticsUpdated(UpdateStatisticsEventArgs args)
    {
        StatisticsUpdated?.Invoke(this, args);
    }

    public void RaiseColorsInLayoutUpdated()
    {
        ColorsInLayoutUpdated?.Invoke(this, EventArgs.Empty);
    }

    public void ForceRendering()
    {
        _isRenderingForced = true;
        InvalidateVisual();
    }

    #region IScrollInfo

    public double ExtentWidth => _scrollableBounds.Width;
    public double ExtentHeight => _scrollableBounds.Height;
    public double ViewportWidth => _viewport.Width;
    public double ViewportHeight => _viewport.Height;

    public double HorizontalOffset => _appSettings.InvertScrollingDirection
                ? _scrollableBounds.Left - _viewport.Left + (_scrollableBounds.Width - _viewport.Width)
                : _viewport.Left - _scrollableBounds.Left;

    public double VerticalOffset => _appSettings.InvertScrollingDirection
                ? _scrollableBounds.Top - _viewport.Top + (_scrollableBounds.Height - _viewport.Height)
                : _viewport.Top - _scrollableBounds.Top;

    public ScrollViewer ScrollOwner { get; set; }
    public bool CanVerticallyScroll { get; set; }
    public bool CanHorizontallyScroll { get; set; }

    public void LineUp()
    {
        _viewport.Top -= 1;
        if (!_scrollableBounds.Contains(_viewport.Absolute))
        {
            InvalidateScroll();
        }
        InvalidateVisual();
    }

    public void LineDown()
    {
        _viewport.Top += 1;
        if (!_scrollableBounds.Contains(_viewport.Absolute))
        {
            InvalidateScroll();
        }
        InvalidateVisual();
    }

    public void LineLeft()
    {
        _viewport.Left -= 1;
        if (!_scrollableBounds.Contains(_viewport.Absolute))
        {
            InvalidateScroll();
        }
        InvalidateVisual();
    }

    public void LineRight()
    {
        _viewport.Left += 1;
        if (!_scrollableBounds.Contains(_viewport.Absolute))
        {
            InvalidateScroll();
        }
        InvalidateVisual();
    }

    public void PageUp()
    {
        _viewport.Top -= _viewport.Height;
        if (!_scrollableBounds.Contains(_viewport.Absolute))
        {
            InvalidateScroll();
        }
        InvalidateVisual();
    }

    public void PageDown()
    {
        _viewport.Top += _viewport.Height;
        if (!_scrollableBounds.Contains(_viewport.Absolute))
        {
            InvalidateScroll();
        }
        InvalidateVisual();
    }

    public void PageLeft()
    {
        _viewport.Left -= _viewport.Width;
        if (!_scrollableBounds.Contains(_viewport.Absolute))
        {
            InvalidateScroll();
        }
        InvalidateVisual();
    }

    public void PageRight()
    {
        _viewport.Left += _viewport.Width;
        if (!_scrollableBounds.Contains(_viewport.Absolute))
        {
            InvalidateScroll();
        }
        InvalidateVisual();
    }

    public void MouseWheelUp()
    {
        //Will zoom the canvas, rather than scroll the canvas
        //throw new NotImplementedException();
    }

    public void MouseWheelDown()
    {
        //Will zoom the canvas, rather than scroll the canvas
        //throw new NotImplementedException();
    }

    public void MouseWheelLeft()
    {
        //throw new NotImplementedException();
    }

    public void MouseWheelRight()
    {
        //throw new NotImplementedException();
    }

    public void SetHorizontalOffset(double offset)
    {
        //handle when offset is +/- infinity (when scrolling to top/bottom using the end and home keys)
        offset = Math.Max(offset, 0d);
        offset = Math.Min(offset, _scrollableBounds.Width);
        _viewport.Left = _appSettings.InvertScrollingDirection
            ? _scrollableBounds.Left - offset + (_scrollableBounds.Width - _viewport.Width)
            : _scrollableBounds.Left + offset;
        _viewport.Left = _scrollableBounds.Left + offset;
        InvalidateScroll();
        InvalidateVisual();
    }

    public void SetVerticalOffset(double offset)
    {
        //handle when offset is +/- infinity (when scrolling to top/bottom using the end and home keys)
        offset = Math.Max(offset, 0d);
        offset = Math.Min(offset, _scrollableBounds.Height);
        _viewport.Top = _appSettings.InvertScrollingDirection
            ? _scrollableBounds.Top - offset + (_scrollableBounds.Height - _viewport.Height)
            : _scrollableBounds.Top + offset;
        InvalidateScroll();
        InvalidateVisual();
    }

    public Rect MakeVisible(Visual visual, Rect rectangle)
    {
        return _viewport.Absolute;
    }

    [GeneratedRegex("A7_residence_SkyScraper_(?<tier>[45])lvl(?<level>[1-5])", RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex GenerateSkyScraperRegex();

    #endregion
}
