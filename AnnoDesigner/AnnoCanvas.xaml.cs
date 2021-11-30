using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
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
using Microsoft.Win32;
using NLog;

namespace AnnoDesigner
{
    /// <summary>
    /// Interaction logic for AnnoCanvas.xaml
    /// </summary>
    public partial class AnnoCanvas : UserControl, IAnnoCanvas, IHotkeySource, IScrollInfo
    {
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

        /// <summary>
        /// Contains all loaded icons as a mapping of name (the filename without extension) to loaded BitmapImage.
        /// </summary>
        public Dictionary<string, IconImage> Icons { get; }

        public BuildingPresets BuildingPresets { get; }

        /// <summary>
        /// Backing field of the GridSize property.
        /// </summary>
        private int _gridStep = Constants.GridStepDefault;

        /// <summary>
        /// Gets or sets the width of the grid cells.
        /// Increasing the grid size results in zooming in and vice versa.
        /// </summary>
        public int GridSize
        {
            get { return _gridStep; }
            set
            {
                var tmp = value;

                if (tmp < Constants.GridStepMin)
                {
                    tmp = Constants.GridStepMin;
                }
                else if (tmp > Constants.GridStepMax)
                {
                    tmp = Constants.GridStepMax;
                }

                if (_gridStep != tmp)
                {
                    InvalidateVisual();
                }

                _gridStep = tmp;
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
            get { return _renderGrid; }
            set
            {
                if (_renderGrid != value)
                {
                    InvalidateVisual();
                }
                _renderGrid = value;
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
            get { return _renderInfluences; }
            set
            {
                if (_renderInfluences != value)
                {
                    InvalidateVisual();
                }
                _renderInfluences = value;
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
            get { return _renderLabel; }
            set
            {
                if (_renderLabel != value)
                {
                    InvalidateVisual();
                }
                _renderLabel = value;
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
            get { return _renderIcon; }
            set
            {
                if (_renderIcon != value)
                {
                    InvalidateVisual();
                }
                _renderIcon = value;
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
            get { return _renderTrueInfluenceRange; }
            set
            {
                if (_renderTrueInfluenceRange != value)
                {
                    InvalidateVisual();
                }
                _renderTrueInfluenceRange = value;
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
            get { return _renderHarborBlockedArea; }
            set
            {
                if (_renderHarborBlockedArea != value)
                {
                    InvalidateVisual();
                }
                _renderHarborBlockedArea = value;
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
            get { return _renderPanorama; }
            set
            {
                if (_renderPanorama != value)
                {
                    InvalidateVisual();
                }
                _renderPanorama = value;
            }
        }

        /// <summary>
        /// Backing field of the CurrentObject property
        /// </summary>
        private List<LayoutObject> _currentObjects = new List<LayoutObject>();

        /// <summary>
        /// Current object to be placed. Fires an event when changed.
        /// </summary>
        public List<LayoutObject> CurrentObjects
        {
            get { return _currentObjects; }
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
        public List<LayoutObject> SelectedObjects { get; set; }

        /// <summary>
        /// Event which is fired when the current object is changed
        /// </summary>
        public event Action<LayoutObject> OnCurrentObjectChanged;

        /// <summary>
        /// backing field of the ObjectClipboard property
        /// </summary>
        private List<LayoutObject> _clipboardObjects = new List<LayoutObject>();

        /// <summary>
        /// Holds a list of objects that are currently on the clipboard.
        /// </summary>
        public List<LayoutObject> ClipboardObjects
        {
            get { return _clipboardObjects; }
            private set
            {
                if (value != null)
                {
                    _clipboardObjects = value;
                    var localizedMessage = value.Count == 1 ? _localizationHelper.GetLocalization("ItemCopied") : _localizationHelper.GetLocalization("ItemsCopied");
                    StatusMessage = $"{value.Count} {localizedMessage}";
                    OnClipboardChanged?.Invoke(value);
                }
            }
        }

        /// <summary>
        /// Event which is fired when the clipboard content is changed.
        /// </summary>
        public event Action<List<LayoutObject>> OnClipboardChanged;

        /// <summary>
        /// Backing field of the StatusMessage property.
        /// </summary>
        private string _statusMessage;

        /// <summary>
        /// Current status message.
        /// </summary>
        public string StatusMessage
        {
            get { return _statusMessage; }
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
            get { return _loadedFile; }
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
        private readonly Regex _regex_panorama = new Regex($"{IDENTIFIER_SKYSCRAPER}(?<tier>[45])lvl(?<level>[1-5])",
            RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant);//RegexOptions.IgnoreCase -> slow in < .NET 5 (triggers several calls to ToLower)

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
            DeleteObject
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
            get { return _currentMode; }
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
        private Point _mousePosition;

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
        private readonly Typeface TYPEFACE = new Typeface("Verdana");

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

        public double LinePenThickness
        {
            get { return _linePen.Thickness; }
        }

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

#if DEBUG
        #region Debug options

        /// <summary>
        /// Brush used for filling and drawing debug-related information.
        /// </summary>
        private readonly SolidColorBrush _debugBrushDark;
        /// <summary>
        /// Brush used for filling and drawing debug-related information.
        /// </summary>
        private readonly SolidColorBrush _debugBrushLight;

        private bool debugModeIsEnabled = false;
        private readonly bool debugShowObjectPositions = true;
        private readonly bool debugShowQuadTreeViz = true;
        private readonly bool debugShowSelectionRectCoordinates = true;
        private readonly bool debugShowSelectionCollisionRect = true;
        private readonly bool debugShowViewportRectCoordinates = true;
        private readonly bool debugShowScrollableRectCoordinates = true;
        private readonly bool debugShowLayoutRectCoordinates = true;
        private readonly bool debugShowMouseGridCoordinates = true;
        private readonly bool debugShowObjectCount = true;

        #endregion
#endif

        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        public AnnoCanvas() : this(null, null)
        {
        }

        public AnnoCanvas(BuildingPresets presetsToUse,
            Dictionary<string, IconImage> iconsToUse,
            IAppSettings appSettingsToUse = null,
            ICoordinateHelper coordinateHelperToUse = null,
            IBrushCache brushCacheToUse = null,
            IPenCache penCacheToUse = null,
            IMessageBoxService messageBoxServiceToUse = null,
            ILocalizationHelper localizationHelperToUse = null,
            IUndoManager undoManager = null)
        {
            InitializeComponent();

            _appSettings = appSettingsToUse ?? AppSettings.Instance;
            _appSettings.SettingsChanged += AppSettings_SettingsChanged;
            _coordinateHelper = coordinateHelperToUse ?? new CoordinateHelper();
            _brushCache = brushCacheToUse ?? new BrushCache();
            _penCache = penCacheToUse ?? new PenCache();
            _messageBoxService = messageBoxServiceToUse ?? new MessageBoxService();
            _localizationHelper = localizationHelperToUse ?? Localization.Localization.Instance;
            UndoManager = undoManager ?? new UndoManager();

            _layoutLoader = new LayoutLoader();

            var sw = new Stopwatch();
            sw.Start();

            //initialize
            CurrentMode = MouseMode.Standard;
            PlacedObjects = new QuadTree<LayoutObject>(new Rect(-128, -128, 256, 256));
            SelectedObjects = new List<LayoutObject>();
            _oldObjectPositions = new List<(LayoutObject Item, Rect OldBounds)>();
            _statisticsCalculationHelper = new StatisticsCalculationHelper();
            _viewport = new Viewport();
            _viewportTransform = new TranslateTransform(0d, 0d);

            #region Hotkeys/Commands
            //Commands
            rotateCommand = new RelayCommand(ExecuteRotate);
            rotateAllCommand = new RelayCommand(ExecuteRotateAll);
            copyCommand = new RelayCommand(ExecuteCopy);
            pasteCommand = new RelayCommand(ExecutePaste);
            deleteCommand = new RelayCommand(ExecuteDelete);
            duplicateCommand = new RelayCommand(ExecuteDuplicate);
            deleteObjectUnderCursorCommand = new RelayCommand(ExecuteDeleteObjectUnderCursor);
            undoCommand = new RelayCommand(ExecuteUndo);
            redoCommand = new RelayCommand(ExecuteRedo);

            //Set up default keybindings

            //for rotation with the r key.
            var rotateBinding1 = new InputBinding(rotateCommand, new PolyGesture(Key.R, ModifierKeys.None));
            rotateHotkey1 = new Hotkey("Rotate_1", rotateBinding1, ROTATE_LOCALIZATION_KEY);

            //for rotation with middle click
            var rotateBinding2 = new InputBinding(rotateCommand, new PolyGesture(ExtendedMouseAction.MiddleClick));
            rotateHotkey2 = new Hotkey("Rotate_2", rotateBinding2, ROTATE_LOCALIZATION_KEY);

            var rotateAllBinding = new InputBinding(rotateAllCommand, new PolyGesture(Key.R, ModifierKeys.Shift));
            rotateAllHotkey = new Hotkey(ROTATE_ALL_LOCALIZATION_KEY, rotateAllBinding, ROTATE_ALL_LOCALIZATION_KEY);

            var copyBinding = new InputBinding(copyCommand, new PolyGesture(Key.C, ModifierKeys.Control));
            copyHotkey = new Hotkey(COPY_LOCALIZATION_KEY, copyBinding, COPY_LOCALIZATION_KEY);

            var pasteBinding = new InputBinding(pasteCommand, new PolyGesture(Key.V, ModifierKeys.Control));
            pasteHotkey = new Hotkey(PASTE_LOCALIZATION_KEY, pasteBinding, PASTE_LOCALIZATION_KEY);

            var deleteBinding = new InputBinding(deleteCommand, new PolyGesture(Key.Delete, ModifierKeys.None));
            deleteHotkey = new Hotkey(DELETE_LOCALIZATION_KEY, deleteBinding, DELETE_LOCALIZATION_KEY);

            var duplicateBinding = new InputBinding(duplicateCommand, new PolyGesture(ExtendedMouseAction.LeftDoubleClick, ModifierKeys.None));
            duplicateHotkey = new Hotkey(DUPLICATE_LOCALIZATION_KEY, duplicateBinding, DUPLICATE_LOCALIZATION_KEY);

            var deleteHoveredOjectBinding = new InputBinding(deleteObjectUnderCursorCommand, new PolyGesture(ExtendedMouseAction.RightClick, ModifierKeys.None));
            deleteObjectUnderCursorHotkey = new Hotkey(DELETE_OBJECT_UNDER_CURSOR_LOCALIZATION_KEY, deleteHoveredOjectBinding, DELETE_OBJECT_UNDER_CURSOR_LOCALIZATION_KEY);

            var undoBinding = new InputBinding(undoCommand, new PolyGesture(Key.Z, ModifierKeys.Control));
            undoHotkey = new Hotkey(UNDO_LOCALIZATION_KEY, undoBinding, UNDO_LOCALIZATION_KEY);

            var redoBinding = new InputBinding(redoCommand, new PolyGesture(Key.Y, ModifierKeys.Control));
            redoHotkey = new Hotkey(REDO_LOCALIZATION_KEY, redoBinding, REDO_LOCALIZATION_KEY);

            //We specifically do not add the `InputBinding`s to the `InputBindingCollection` of `AnnoCanvas`, as if we did that,
            //`InputBinding.Gesture.Matches()` would be fired for *every* event - MouseWheel, MouseDown, KeyUp, KeyDown, MouseMove etc
            //which we don't want, as it produces a noticeable performance impact.
            #endregion

            LoadGridLineColor();
            LoadObjectBorderLineColor();

            _highlightPen = _penCache.GetPen(Brushes.Yellow, DPI_FACTOR * 2);
            _radiusPen = _penCache.GetPen(Brushes.Black, DPI_FACTOR * 2);
            _influencedPen = _penCache.GetPen(Brushes.LawnGreen, DPI_FACTOR * 2);

            var color = Colors.LightYellow;
            color.A = 32;
            _lightBrush = _brushCache.GetSolidBrush(color);
            color = Colors.LawnGreen;
            color.A = 32;
            _influencedBrush = _brushCache.GetSolidBrush(color);
#if DEBUG
            _debugBrushLight = Brushes.Blue;
            _debugBrushDark = Brushes.DarkBlue;
#endif

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
                        var loader = new BuildingPresetsLoader();
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
                        var loader = new IconMappingPresetsLoader();
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
                    var iconLoader = new IconLoader();
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

        private void AppSettings_SettingsChanged(object sender, EventArgs e)
        {
            LoadGridLineColor();
            LoadObjectBorderLineColor();
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
        private List<LayoutObject> _lastObjectsToDraw = new List<LayoutObject>();
        private List<LayoutObject> _lastBorderlessObjectsToDraw = new List<LayoutObject>();
        private List<LayoutObject> _lastBorderedObjectsToDraw = new List<LayoutObject>();
        private QuadTree<LayoutObject> _lastPlacedObjects = null;

        /// <summary>
        /// Renders the whole scene including grid, placed objects, current object, selection highlights, influence radii and selection rectangle.
        /// </summary>
        /// <param name="drawingContext">context used for rendering</param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            var width = RenderSize.Width;
            var height = RenderSize.Height;
            _viewport.Width = _coordinateHelper.ScreenToGrid(width, GridSize);
            _viewport.Height = _coordinateHelper.ScreenToGrid(height, GridSize);

            if (ScrollOwner != null)
            {
                //SCrollbar visibility should probably be managed by the owner of the the scrollviewer itself, not here...
                if (_appSettings.ShowScrollbars)
                {
                    ScrollOwner.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
                    ScrollOwner.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
                }
                else
                {
                    ScrollOwner.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                    ScrollOwner.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
                }

                if (_invalidateScrollInfo)
                {
                    ScrollOwner?.InvalidateScrollInfo();
                    _invalidateScrollInfo = false;
                }
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
            _viewportTransform.X = _coordinateHelper.GridToScreen(-_viewport.Left, GridSize);
            _viewportTransform.Y = _coordinateHelper.GridToScreen(-_viewport.Top, GridSize);

            // assure pixel perfect drawing using guidelines.
            // this value is cached and refreshed in LoadGridLineColor(), as it uses pen thickness in its calculation;
            drawingContext.PushGuidelineSet(_guidelineSet);

            // draw background
            drawingContext.DrawRectangle(Brushes.Transparent, null, new Rect(new Point(), RenderSize));
            // draw grid
            if (RenderGrid)
            {
                for (var i = _viewport.HorizontalAlignmentValue * GridSize; i < width; i += _gridStep)
                {
                    drawingContext.DrawLine(_gridLinePen, new Point(i, 0), new Point(i, height));
                }
                for (var i = _viewport.VerticalAlignmentValue * GridSize; i < height; i += _gridStep)
                {
                    drawingContext.DrawLine(_gridLinePen, new Point(0, i), new Point(width, i));
                }
            }

            //Push the transform after rendering everything that should not be translated.
            drawingContext.PushTransform(_viewportTransform);

            var objectsToDraw = _lastObjectsToDraw;
            var borderlessObjects = _lastBorderlessObjectsToDraw;
            var borderedObjects = _lastBorderedObjectsToDraw;

            if (_lastViewPortAbsolute != _viewport.Absolute || _lastPlacedObjects != PlacedObjects || CurrentMode == MouseMode.PlaceObjects || CurrentMode == MouseMode.DeleteObject)
            {
                objectsToDraw = PlacedObjects.GetItemsIntersecting(_viewport.Absolute).ToList();
                _lastObjectsToDraw = objectsToDraw;
                _lastPlacedObjects = PlacedObjects;
                _lastViewPortAbsolute = _viewport.Absolute;

                borderlessObjects = objectsToDraw.Where(_ => _.WrappedAnnoObject.Borderless).ToList();
                _lastBorderlessObjectsToDraw = borderlessObjects;
                borderedObjects = objectsToDraw.Where(_ => !_.WrappedAnnoObject.Borderless).ToList();
                _lastBorderedObjectsToDraw = borderedObjects;

                //quick fix deleting objects via keyboard instead of right click
                if (CurrentMode == MouseMode.DeleteObject)
                {
                    CurrentMode = MouseMode.Standard;
                }
            }

            // draw placed objects
            //borderless objects should be drawn first; selection afterwards
            RenderObjectList(drawingContext, borderlessObjects, useTransparency: false);
            RenderObjectList(drawingContext, borderedObjects, useTransparency: false);
            RenderObjectSelection(drawingContext, SelectedObjects.WithoutIgnoredObjects());

            if (RenderPanorama)
            {
                RenderPanoramaText(drawingContext, objectsToDraw);
            }

            if (!RenderInfluences)
            {
                if (!_appSettings.HideInfluenceOnSelection)
                {
                    RenderObjectInfluenceRadius(drawingContext, SelectedObjects);
                    RenderObjectInfluenceRange(drawingContext, SelectedObjects);
                }
            }
            else
            {
                RenderObjectInfluenceRadius(drawingContext, objectsToDraw);
                RenderObjectInfluenceRange(drawingContext, objectsToDraw);
                //Retrieve objects outside the viewport that have an influence range which affects objects
                //within the viewport.
                var offscreenObjects = PlacedObjects
                .Where(_ => !_viewport.Absolute.Contains(_.GridRect) &&
                            (_viewport.Absolute.IntersectsWith(_.GridInfluenceRadiusRect) || _viewport.Absolute.IntersectsWith(_.GridInfluenceRangeRect))
                 ).ToList();
                RenderObjectInfluenceRadius(drawingContext, offscreenObjects);
                RenderObjectInfluenceRange(drawingContext, offscreenObjects);

            }

            if (CurrentObjects.Count == 0)
            {
                // highlight object which is currently hovered
                var hoveredObj = GetObjectAt(_mousePosition);
                if (hoveredObj != null)
                {
                    drawingContext.DrawRectangle(null, _highlightPen, hoveredObj.CalculateScreenRect(GridSize));
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
#if DEBUG
            #region Draw debug information
            if (debugModeIsEnabled)
            {
                drawingContext.PushTransform(_viewportTransform);
                if (debugShowQuadTreeViz)
                {
                    var brush = Brushes.Transparent;
                    var pen = _penCache.GetPen(_debugBrushDark, 2);
                    foreach (var rect in PlacedObjects.GetQuadrantRects())
                    {
                        drawingContext.DrawRectangle(brush, pen, _coordinateHelper.GridToScreen(rect, GridSize));
                    }
                }

                if (debugShowSelectionCollisionRect)
                {
                    var color = _debugBrushLight.Color;
                    color.A = 0x08;
                    var brush = _brushCache.GetSolidBrush(color);
                    var pen = _penCache.GetPen(_debugBrushLight, 1);
                    var collisionRectScreen = _coordinateHelper.GridToScreen(_collisionRect, GridSize);
                    drawingContext.DrawRectangle(brush, pen, collisionRectScreen);
                }

                //pop viewport transform
                drawingContext.Pop();
                var debugText = new List<FormattedText>(3);

                if (debugShowViewportRectCoordinates)
                {
                    //The first time this is called, App.DpiScale is still 0 which causes this code to throw an error
                    if (App.DpiScale.PixelsPerDip != 0)
                    {
                        var top = _viewport.Top;
                        var left = _viewport.Left;
                        var h = _viewport.Height;
                        var w = _viewport.Width;
                        var text = new FormattedText($"Viewport: {left:F2}, {top:F2}, {w:F2}, {h:F2}", Thread.CurrentThread.CurrentCulture, FlowDirection.LeftToRight,
                                                     TYPEFACE, 12, _debugBrushLight, null, TextFormattingMode.Display, App.DpiScale.PixelsPerDip)
                        {
                            TextAlignment = TextAlignment.Left
                        };
                        debugText.Add(text);
                    }
                }

                if (debugShowScrollableRectCoordinates)
                {
                    //The first time this is called, App.DpiScale is still 0 which causes this code to throw an error
                    if (App.DpiScale.PixelsPerDip != 0)
                    {
                        var top = _scrollableBounds.Top;
                        var left = _scrollableBounds.Left;
                        var h = _scrollableBounds.Height;
                        var w = _scrollableBounds.Width;
                        var text = new FormattedText($"Scrolllable: {left:F2}, {top:F2}, {w:F2}, {h:F2}", Thread.CurrentThread.CurrentCulture, FlowDirection.LeftToRight,
                                                     TYPEFACE, 12, _debugBrushLight, null, TextFormattingMode.Display, App.DpiScale.PixelsPerDip)
                        {
                            TextAlignment = TextAlignment.Left
                        };
                        debugText.Add(text);
                    }
                }

                if (debugShowLayoutRectCoordinates)
                {
                    //The first time this is called, App.DpiScale is still 0 which causes this code to throw an error
                    if (App.DpiScale.PixelsPerDip != 0)
                    {
                        var top = _layoutBounds.Top;
                        var left = _layoutBounds.Left;
                        var h = _layoutBounds.Height;
                        var w = _layoutBounds.Width;
                        var text = new FormattedText($"Layout: {left:F2}, {top:F2}, {w:F2}, {h:F2}", Thread.CurrentThread.CurrentCulture, FlowDirection.LeftToRight,
                                                     TYPEFACE, 12, _debugBrushLight, null, TextFormattingMode.Display, App.DpiScale.PixelsPerDip)
                        {
                            TextAlignment = TextAlignment.Left
                        };
                        debugText.Add(text);
                    }
                }

                if (debugShowObjectCount)
                {
                    //The first time this is called, App.DpiScale is still 0 which causes this code to throw an error
                    if (App.DpiScale.PixelsPerDip != 0)
                    {
                        var text = new FormattedText($"{nameof(PlacedObjects)}: {PlacedObjects.Count}", Thread.CurrentThread.CurrentCulture, FlowDirection.LeftToRight,
                                                     TYPEFACE, 12, _debugBrushLight, null, TextFormattingMode.Display, App.DpiScale.PixelsPerDip)
                        {
                            TextAlignment = TextAlignment.Left
                        };
                        debugText.Add(text);
                    }
                }

                for (var i = 0; i < debugText.Count; i++)
                {
                    drawingContext.DrawText(debugText[i], new Point(5, (i * 15) + 5));
                }

                if (debugShowMouseGridCoordinates)
                {
                    //The first time this is called, App.DpiScale is still 0 which causes this code to throw an error
                    if (App.DpiScale.PixelsPerDip != 0)
                    {
                        var gridPosition = _coordinateHelper.ScreenToFractionalGrid(_mousePosition, GridSize);
                        gridPosition = _viewport.OriginToViewport(gridPosition);
                        var x = gridPosition.X;
                        var y = gridPosition.Y;
                        var text = new FormattedText($"{x:F2}, {y:F2}", Thread.CurrentThread.CurrentCulture, FlowDirection.LeftToRight,
                                                     TYPEFACE, 12, _debugBrushLight, null, TextFormattingMode.Display, App.DpiScale.PixelsPerDip)
                        {
                            TextAlignment = TextAlignment.Left
                        };
                        var pos = _mousePosition;
                        pos.X -= 5;
                        pos.Y += 15;
                        drawingContext.DrawText(text, pos);
                    }
                }

                //draw selection rect coords last so they draw over the top of everything else
                if (CurrentMode == MouseMode.SelectionRect)
                {
                    if (debugShowSelectionRectCoordinates)
                    {
                        var rect = _coordinateHelper.ScreenToGrid(_selectionRect, GridSize);
                        var top = rect.Top;
                        var left = rect.Left;
                        var h = rect.Height;
                        var w = rect.Width;
                        var text = new FormattedText($"{left:F2}, {top:F2}, {w:F2}, {h:F2}", Thread.CurrentThread.CurrentCulture, FlowDirection.LeftToRight,
                       TYPEFACE, 12, _debugBrushLight,
                       null, TextFormattingMode.Display, App.DpiScale.PixelsPerDip)
                        {
                            TextAlignment = TextAlignment.Left
                        };
                        var location = _selectionRect.BottomRight;
                        location.X -= text.Width;
                        location.Y -= text.Height;
                        drawingContext.DrawText(text, location);
                    }
                }
            }
            #endregion
#endif
            // pop back guidlines set
            drawingContext.Pop();
        }

        private void RenderPanoramaText(DrawingContext drawingContext, List<LayoutObject> placedObjects)
        {
            foreach (var curObject in placedObjects.FindAll(_ => _.Identifier.StartsWith(IDENTIFIER_SKYSCRAPER, StringComparison.OrdinalIgnoreCase)))
            {
                if (!_regex_panorama.TryMatch(curObject.Identifier, out var match))
                {
                    continue;
                }

                var center = _coordinateHelper.GetCenterPoint(curObject.GridRect);

                var level = int.Parse(match.Groups["level"].Value);
                var radiusSquared = curObject.WrappedAnnoObject.Radius * curObject.WrappedAnnoObject.Radius;
                var panorama = level;

                //find intersecting skyscrapers
                foreach (var adjacentObject in PlacedObjects.GetItemsIntersecting(curObject.GridInfluenceRadiusRect)
                    .Where(_ => _.Identifier.StartsWith(IDENTIFIER_SKYSCRAPER, StringComparison.OrdinalIgnoreCase)))
                {
                    if (adjacentObject == curObject)
                    {
                        continue;
                    }

                    if ((center - _coordinateHelper.GetCenterPoint(adjacentObject.GridRect)).LengthSquared <= radiusSquared)
                    {
                        if (_regex_panorama.TryMatch(adjacentObject.Identifier, out var match2))
                        {
                            var level2 = int.Parse(match2.Groups["level"].Value);
                            panorama += level > level2 ? 1 : -1;
                        }
                    }
                }

                if (curObject.LastPanorama != panorama || curObject.PanoramaText == null)
                {
                    // put the sign at the end of the string since it will be drawn from right to left
                    var text = Math.Abs(panorama).ToString() + (panorama >= 0 ? "" : "-");

                    curObject.PanoramaText = new FormattedText(text, Thread.CurrentThread.CurrentUICulture,
                        FlowDirection.RightToLeft, TYPEFACE, FontSize, Brushes.Black, App.DpiScale.PixelsPerDip);
                }

                drawingContext.DrawText(curObject.PanoramaText, curObject.CalculateScreenRect(GridSize).TopRight);
            }
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
                var r = CurrentObjects[0].GridRect;
                foreach (var obj in CurrentObjects.Skip(1))
                {
                    r.Union(obj.GridRect);
                }

                var center = _coordinateHelper.GetCenterPoint(r);
                var mousePosition = _coordinateHelper.ScreenToFractionalGrid(_mousePosition, GridSize);
                var dx = mousePosition.X - center.X;
                var dy = mousePosition.Y - center.Y;
                foreach (var obj in CurrentObjects)
                {
                    var pos = obj.Position;
                    pos = _viewport.OriginToViewport(new Point(pos.X + dx, pos.Y + dy));
                    pos = new Point(Math.Floor(pos.X), Math.Floor(pos.Y));
                    obj.Position = pos;
                }
            }
            else
            {
                var pos = _coordinateHelper.ScreenToFractionalGrid(_mousePosition, GridSize);
                var size = CurrentObjects[0].Size;
                pos.X -= size.Width / 2;
                pos.Y -= size.Height / 2;
                pos = _viewport.OriginToViewport(pos);
                pos = new Point(Math.Round(pos.X, MidpointRounding.AwayFromZero), Math.Round(pos.Y, MidpointRounding.AwayFromZero));
                CurrentObjects[0].Position = pos;
            }
        }

        /// <summary>
        /// Renders the given AnnoObject to the given DrawingContext.
        /// </summary>
        /// <param name="drawingContext">context used for rendering</param>
        /// <param name="obj">object to render</param>
        private void RenderObjectList(DrawingContext drawingContext, List<LayoutObject> objects, bool useTransparency)
        {
            var gridSize = GridSize; //hot path optimization
            var linePenThickness = _linePen.Thickness; //hot path optimization (avoid access of DependencyProperty)
            var renderHarborBlockedArea = RenderHarborBlockedArea; //hot path optimization
            var renderIcon = RenderIcon; //hot path optimization
            var renderLabel = RenderLabel; //hot path optimization

            foreach (var curLayoutObject in objects)
            {
                var obj = curLayoutObject.WrappedAnnoObject;

                // draw object rectangle
                var objRect = curLayoutObject.CalculateScreenRect(gridSize);

                var brush = useTransparency ? curLayoutObject.TransparentBrush : curLayoutObject.RenderBrush;

                var borderPen = obj.Borderless ? curLayoutObject.GetBorderlessPen(brush, linePenThickness) : _linePen;
                drawingContext.DrawRectangle(brush, borderPen, objRect);
                if (renderHarborBlockedArea)
                {
                    var objBlockedRect = curLayoutObject.CalculateBlockedScreenRect(gridSize);
                    if (objBlockedRect.HasValue)
                    {
                        drawingContext.DrawRectangle(curLayoutObject.BlockedAreaBrush, borderPen, objBlockedRect.Value);
                    }
                }

                // draw object icon if it is at least 2x2 cells
                var iconRendered = false;
                if (renderIcon && !string.IsNullOrEmpty(obj.Icon))
                {
                    var iconFound = false;

                    if (curLayoutObject.Icon is null)
                    {
                        var iconName = curLayoutObject.IconNameWithoutExtension; // for backwards compatibility to older layouts

                        //a null check is not needed here, as IconNameWithoutExtension uses obj.Icon, and we already check if that 
                        //is null or empty, meaning the value that we feed into Path.GetFileNameWithoutExtension cannot be null, and
                        //Path.GetFileNameWithoutExtension will either throw (representing an invalid path) or return a string 
                        //(representing the file name)
                        if (Icons.TryGetValue(iconName, out var iconImage))
                        {
                            curLayoutObject.Icon = iconImage;
                            iconFound = true;
                        }
                        else
                        {
                            var message = $"Icon file missing ({iconName}).";
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
                        var iconRect = curLayoutObject.GetIconRect(gridSize);

                        drawingContext.DrawImage(curLayoutObject.Icon.Icon, iconRect);
                        iconRendered = true;
                    }
                }

                // draw object label
                if (renderLabel && !string.IsNullOrEmpty(obj.Label))
                {
                    var textAlignment = iconRendered ? TextAlignment.Left : TextAlignment.Center;
                    var text = curLayoutObject.GetFormattedText(textAlignment, Thread.CurrentThread.CurrentCulture,
                        TYPEFACE, App.DpiScale.PixelsPerDip, objRect.Width, objRect.Height);

                    var textLocation = objRect.TopLeft;
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
#if DEBUG
                if (debugModeIsEnabled && debugShowObjectPositions)
                {
                    var text = new FormattedText(obj.Position.ToString(), Thread.CurrentThread.CurrentCulture, FlowDirection.LeftToRight,
                    TYPEFACE, 12, _debugBrushLight,
                    null, TextFormattingMode.Display, App.DpiScale.PixelsPerDip)
                    {
                        MaxTextWidth = objRect.Width,
                        MaxTextHeight = objRect.Width,
                        TextAlignment = TextAlignment.Left
                    };
                    var textLocation = objRect.BottomRight;
                    textLocation.X -= text.Width;
                    textLocation.Y -= text.Height;

                    drawingContext.DrawText(text, textLocation);
                }
#endif
            }
        }


        /// <summary>
        /// Renders a selection highlight on the specified object.
        /// </summary>
        /// <param name="drawingContext">context used for rendering</param>
        /// <param name="obj">object to render as selected</param>
        private void RenderObjectSelection(DrawingContext drawingContext, List<LayoutObject> objects)
        {
            foreach (var curLayoutObject in objects)
            {
                // draw object rectangle                
                drawingContext.DrawRectangle(null, _highlightPen, curLayoutObject.CalculateScreenRect(GridSize));
            }
        }

        /// <summary>
        /// Renders the influence radius of the given object and highlights other objects within range.
        /// </summary>
        /// <param name="drawingContext">context used for rendering</param>
        /// <param name="obj">object which's influence is rendered</param>
        private void RenderObjectInfluenceRadius(DrawingContext drawingContext, List<LayoutObject> objects)
        {
            foreach (var curLayoutObject in objects)
            {
                if (curLayoutObject.WrappedAnnoObject.Radius >= 0.5)
                {
                    // highlight buildings within influence
                    var radius = curLayoutObject.GetScreenRadius(GridSize);
                    var circle = curLayoutObject.GetInfluenceCircle(GridSize, radius);

                    var circleCenterX = circle.Center.X;
                    var circleCenterY = circle.Center.Y;

                    var influenceGridRect = curLayoutObject.GridInfluenceRadiusRect;

                    foreach (var curPlacedObject in PlacedObjects.GetItemsIntersecting(influenceGridRect).WithoutIgnoredObjects())
                    {
                        var distance = curPlacedObject.GetScreenRectCenterPoint(GridSize);
                        distance.X -= circleCenterX;
                        distance.Y -= circleCenterY;
                        // check if the center is within the influence circle
                        if ((distance.X * distance.X) + (distance.Y * distance.Y) <= radius * radius)
                        {
                            drawingContext.DrawRectangle(_influencedBrush, _influencedPen, curPlacedObject.CalculateScreenRect(GridSize));
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
        private void RenderObjectInfluenceRange(DrawingContext drawingContext, List<LayoutObject> objects)
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
                var placedObjects = PlacedObjects.Concat(objects).ToHashSet();
                placedAnnoObjects = placedObjects.Select(o => o.WrappedAnnoObject).ToList();
                var placedObjectDictionary = placedObjects.ToDictionaryWithCapacity(o => o.WrappedAnnoObject);

                void Highlight(AnnoObject objectInRange)
                {
                    drawingContext.DrawRectangle(_influencedBrush, _influencedPen, placedObjectDictionary[objectInRange].CalculateScreenRect(GridSize));
                }

                gridDictionary = RoadSearchHelper.PrepareGridDictionary(placedAnnoObjects);
                RoadSearchHelper.BreadthFirstSearch(
                    placedAnnoObjects,
                    objects.Select(o => o.WrappedAnnoObject).Where(o => o.InfluenceRange > 0.5),
                    o => (int)o.InfluenceRange + 1,// increase distance to get objects that are touching even the last road cell in influence range
                    gridDictionary,
                    Highlight);
            }

            var geometries = new ConcurrentBag<(long index, StreamGeometry geometry)>();
            Parallel.ForEach(objects, (curLayoutObject, _, index) =>
            {
                if (curLayoutObject.WrappedAnnoObject.InfluenceRange > 0.5)
                {
                    var sg = new StreamGeometry();

                    using (var sgc = sg.Open())
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

            foreach (var (_, geometry) in geometries.OrderBy(p => p.index))
            {
                drawingContext.DrawGeometry(_lightBrush, _radiusPen, geometry);
            }
        }

        private void DrawTrueInfluenceRangePolygon(LayoutObject curLayoutObject, StreamGeometryContext sgc, Moved2DArray<AnnoObject> gridDictionary, List<AnnoObject> placedAnnoObjects)
        {
            var stroked = true;
            var smoothJoin = true;

            var geometryFill = true;
            var geometryStroke = true;

            var startObjects = new AnnoObject[1]
            {
                curLayoutObject.WrappedAnnoObject
            };

            var cellsInInfluenceRange = RoadSearchHelper.BreadthFirstSearch(
                placedAnnoObjects,
                startObjects,
                o => (int)o.InfluenceRange,
                gridDictionary);

            var points = PolygonBoundaryFinderHelper.GetBoundaryPoints(cellsInInfluenceRange);
            if (points.Count < 1)
            {
                return;
            }

            sgc.BeginFigure(_coordinateHelper.GridToScreen(new Point(points[0].x + gridDictionary.Offset.x, points[0].y + gridDictionary.Offset.y), GridSize), geometryFill, geometryStroke);
            for (var i = 1; i < points.Count; i++)
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
            var topLeftCorner = curLayoutObject.Position;
            var topRightCorner = new Point(curLayoutObject.Position.X + curLayoutObject.Size.Width, curLayoutObject.Position.Y);
            var bottomLeftCorner = new Point(curLayoutObject.Position.X, curLayoutObject.Position.Y + curLayoutObject.Size.Height);
            var bottomRightCorner = new Point(curLayoutObject.Position.X + curLayoutObject.Size.Width, curLayoutObject.Position.Y + curLayoutObject.Size.Height);

            var influenceRange = curLayoutObject.WrappedAnnoObject.InfluenceRange;

            var startPoint = new Point(topLeftCorner.X, topLeftCorner.Y - influenceRange);
            var stroked = true;
            var smoothJoin = true;

            var geometryFill = true;
            var geometryStroke = true;

            sgc.BeginFigure(_coordinateHelper.GridToScreen(startPoint, GridSize), geometryFill, geometryStroke);

            ////////////////////////////////////////////////////////////////
            //Draw in width of object
            sgc.LineTo(_coordinateHelper.GridToScreen(new Point(topRightCorner.X, startPoint.Y), GridSize), stroked, smoothJoin);

            //Draw quadrant 2
            //Get end value to draw from top-right of 2nd quadrant to bottom-right of 2nd quadrant
            startPoint = new Point(topRightCorner.X, topRightCorner.Y - influenceRange);
            var endPoint = new Point(topRightCorner.X + influenceRange, topRightCorner.Y);

            //Following the rules for quadrant 2 - go right and down
            var currentPoint = new Point(startPoint.X, startPoint.Y);
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
        /// If <see langword="true"> then apply to objects whose identifier matches one of those in <see cref="objectsToAdd">.
        /// </param>
        private void AddSelectedObjects(List<LayoutObject> objectsToAdd, bool includeSameObjects)
        {
            if (includeSameObjects)
            {
                // Add all placed objects whose identifier matches any of those in the objectsToAdd.
                SelectedObjects.AddRange(PlacedObjects.Where(placed => objectsToAdd.Any(toAdd => toAdd.Identifier.Equals(placed.Identifier, StringComparison.OrdinalIgnoreCase))));
            }
            else
            {
                SelectedObjects.AddRange(objectsToAdd);
            }

            // This can lead to some objects being selected multiple times, so only keep distinct objects.
            SelectedObjects = SelectedObjects.Distinct().ToList();
        }

        /// <summary>
        /// Remove the objects from SelectedObjects, optionally also remove all objects which match one of their identifiers.
        /// </summary>
        /// <param name="includeSameObjects"> 
        /// If <see langword="true"> then apply to objects whose identifier matches one of those in <see cref="objectsToRemove">.
        /// </param>
        private void RemoveSelectedObjects(List<LayoutObject> objectsToRemove, bool includeSameObjects)
        {
            if (includeSameObjects)
            {
                // Exclude any selected objects whose identifier matches any of those in the objectsToRemove.
                SelectedObjects = SelectedObjects.Except(SelectedObjects.Where(placed => objectsToRemove.Any(toRemove => toRemove.Identifier.Equals(placed.Identifier, StringComparison.OrdinalIgnoreCase)))).ToList();
            }
            else
            {
                SelectedObjects = SelectedObjects.Except(objectsToRemove).ToList();
            }
        }

        /// <summary>
        /// Add a single object to SelectedObjects, optionally also add all objects with the same identifier.
        /// </summary>
        /// <param name="includeSameObjects"> 
        /// If <see langword="true"> then apply to objects whose identifier match that of <see cref="objectToAdd">.
        /// </param>
        private void AddSelectedObject(LayoutObject objectToAdd, bool includeSameObjects)
        {
            AddSelectedObjects(new List<LayoutObject>() { objectToAdd }, includeSameObjects);
        }

        /// <summary>
        /// Remove a single object from SelectedObjects, optionally also remove all objects with the same identifier.
        /// </summary>
        /// <param name="includeSameObjects"> 
        /// If <see langword="true"> then apply to objects whose identifier match that of <see cref="objectToRemove">.
        /// </param>
        private void RemoveSelectedObject(LayoutObject objectToRemove, bool includeSameObjects)
        {
            RemoveSelectedObjects(new List<LayoutObject>() { objectToRemove }, includeSameObjects);
        }

        /// <summary>
        /// Used to load current color for grid lines from settings.
        /// </summary>
        /// <remarks>As this method can be called when AppSettings are updated, we make sure to not call anything that relies on the UI thread from here.</remarks>
        private void LoadGridLineColor()
        {
            var colorFromJson = SerializationHelper.LoadFromJsonString<UserDefinedColor>(_appSettings.ColorGridLines);//explicit variable to make debugging easier
            _gridLinePen = _penCache.GetPen(_brushCache.GetSolidBrush(colorFromJson.Color), DPI_FACTOR * 1);
            var halfPenWidth = _gridLinePen.Thickness / 2;
            var guidelines = new GuidelineSet();
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
            var colorFromJson = SerializationHelper.LoadFromJsonString<UserDefinedColor>(_appSettings.ColorObjectBorderLines);//explicit variable to make debugging easier
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
            foreach (var (item, oldBounds) in _oldObjectPositions)
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
            var r = _viewport.Absolute;
            r.Union(_layoutBounds);
            _scrollableBounds = r;

            //update scroll viewer on next render
            _invalidateScrollInfo = true;
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
            foreach (var item in objects)
            {
                var newRect = _coordinateHelper.Rotate(item.Bounds);
                var oldRect = item.Bounds;
                item.Bounds = newRect;
                item.Direction = _coordinateHelper.Rotate(item.Direction);
                yield return (item, oldRect);
            }
        }

        #endregion

        #region Event handling

        #region Mouse

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            _mouseWithinControl = true;
            //  _mousePosition = e.GetPosition(this);
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
            var zoomFactor = (((Constants.ZoomSensitivitySliderMaximum + 1) - _appSettings.ZoomSensitivityPercentage) * Constants.ZoomSensitivityCoefficient) + Constants.ZoomSensitivityMinimum;
            var change = (int)(e.Delta / zoomFactor);
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
                var mousePosition = _mousePosition;
                var preZoomPosition = _coordinateHelper.ScreenToFractionalGrid(mousePosition, GridSize);
                GridSize += change;
                var postZoomPosition = _coordinateHelper.ScreenToFractionalGrid(mousePosition, GridSize);
                var diff = preZoomPosition - postZoomPosition;
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
            Focus();
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
                Focus();
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
                TryPlaceCurrentObjects(isContinuousDrawing: false);
            }
            else if (e.LeftButton == MouseButtonState.Pressed && CurrentObjects.Count == 0)
            {
                var obj = GetObjectAt(_mousePosition);
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
                        var obj = GetObjectAt(_mouseDragStart);
                        AddSelectedObject(obj, ShouldAffectObjectsWithIdentifier());
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
                var dx = (int)_coordinateHelper.ScreenToGrid(_mousePosition.X - _mouseDragStart.X, GridSize);
                var dy = (int)_coordinateHelper.ScreenToGrid(_mousePosition.Y - _mouseDragStart.Y, GridSize);

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
                    TryPlaceCurrentObjects(isContinuousDrawing: true);
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
                                    RemoveSelectedObjects(SelectedObjects.Where(_ => _.CalculateScreenRect(GridSize).IntersectsWith(_selectionRect)).ToList(),
                                                          ShouldAffectObjectsWithIdentifier());
                                }
                                else
                                {
                                    SelectedObjects.Clear();
                                }

                                // adjust rect
                                _selectionRect = new Rect(_mouseDragStart, _mousePosition);
                                // select intersecting objects
                                var selectionRectGrid = _coordinateHelper.ScreenToGrid(_selectionRect, GridSize);
                                selectionRectGrid = _viewport.OriginToViewport(selectionRectGrid);
                                AddSelectedObjects(PlacedObjects.GetItemsIntersecting(selectionRectGrid).ToList(),
                                                   ShouldAffectObjectsWithIdentifier());

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
                                var dx = (int)_coordinateHelper.ScreenToGrid(_mousePosition.X - _mouseDragStart.X, GridSize);
                                var dy = (int)_coordinateHelper.ScreenToGrid(_mousePosition.Y - _mouseDragStart.Y, GridSize);
                                // check if the mouse has moved at least one grid cell in any direction
                                if (dx == 0 && dy == 0)
                                {
                                    //no relevant mouse move -> no further action
                                    break;
                                }
                                //Recompute _unselectedObjects
                                var offsetCollisionRect = _collisionRect;
                                offsetCollisionRect.Offset(dx, dy);

                                //Its causing slowdowns when dragging large numbers of objects
                                _unselectedObjects = PlacedObjects.GetItemsIntersecting(offsetCollisionRect).Where(_ => !SelectedObjects.Contains(_)).ToList();
                                var collisionsExist = false;
                                // temporarily move each object and check if collisions with unselected objects exist
                                foreach (var curLayoutObject in SelectedObjects)
                                {
                                    var originalPosition = curLayoutObject.Position;
                                    // move object                                
                                    curLayoutObject.Position = new Point(curLayoutObject.Position.X + dx, curLayoutObject.Position.Y + dy);
                                    // check for collisions                                
                                    var collides = _unselectedObjects.Find(_ => ObjectIntersectionExists(curLayoutObject, _)) != null;
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
                                    foreach (var curLayoutObject in SelectedObjects)
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
                                    var oldLayoutBounds = _layoutBounds;
                                    InvalidateBounds();
                                    if (oldLayoutBounds != _layoutBounds)
                                    {
                                        InvalidateScroll();
                                    }
                                }

                                break;
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

                            var obj = GetObjectAt(_mousePosition);

                            if (obj != null)
                            {
                                // user clicked an object: select or deselect it
                                if (SelectedObjects.Contains(obj))
                                {
                                    RemoveSelectedObject(obj, ShouldAffectObjectsWithIdentifier());
                                }
                                else
                                {
                                    AddSelectedObject(obj, ShouldAffectObjectsWithIdentifier());
                                }
                            }

                            _collisionRect = ComputeBoundingRect(SelectedObjects);
                            StatisticsUpdated?.Invoke(this, UpdateStatisticsEventArgs.All);
                            // return to standard mode, i.e. clear any drag-start modes
                            CurrentMode = MouseMode.Standard;
                            break;
                        }
                    case MouseMode.SelectionRect:
                        _collisionRect = ComputeBoundingRect(SelectedObjects);
                        // cancel dragging of selection rect
                        CurrentMode = MouseMode.Standard;
                        break;
                    case MouseMode.DragSelection:
                        // stop dragging of selected objects
                        UndoManager.RegisterOperation(new MoveObjectsOperation<LayoutObject>()
                        {
                            ObjectPropertyValues = _oldObjectPositions.Select(pair => (pair.Item, pair.OldGridRect, pair.Item.Bounds)).ToList(),
                            QuadTree = PlacedObjects
                        });

                        ReindexMovedObjects();
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
#if DEBUG
            if (e.Key == Key.D && Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
            {
                debugModeIsEnabled = !debugModeIsEnabled;
                e.Handled = true;
            }
#endif
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
        /// <returns><see langword="true"> if all objects with same identifier should be affected, otherwise <see langword="false">.</returns>
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
        private bool ObjectIntersectionExists(LayoutObject a, LayoutObject b)
        {
            return a.CollisionRect.IntersectsWith(b.CollisionRect);
        }

        /// <summary>
        /// Checks if there is a collision between a list of AnnoObjects a and object b.
        /// </summary>
        /// <param name="a">List of objects</param>
        /// <param name="b">second object</param>
        /// <returns>true if there is a collision, otherwise false</returns>
        private bool ObjectIntersectionExists(List<LayoutObject> a, LayoutObject b)
        {
            return a.Exists(_ => _.CollisionRect.IntersectsWith(b.CollisionRect));
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

            var boundingRect = ComputeBoundingRect(CurrentObjects);
            var objects = PlacedObjects.GetItemsIntersecting(boundingRect);

            if (CurrentObjects.Count != 0 && !objects.Any(_ => ObjectIntersectionExists(CurrentObjects, _)))
            {
                var newObjects = CloneList(CurrentObjects);
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

        /// <summary>
        /// Retrieves the object at the given position given in screen coordinates.
        /// </summary>
        /// <param name="position">position given in screen coordinates</param>
        /// <returns>object at the position, <see langword="null"/> if no object could be found</returns>
        private LayoutObject GetObjectAt(Point position)
        {
            var gridPosition = _coordinateHelper.ScreenToFractionalGrid(position, GridSize);
            gridPosition = _viewport.OriginToViewport(gridPosition);
            var possibleItems = PlacedObjects.GetItemsIntersecting(new Rect(gridPosition, new Size(1, 1)));
            return possibleItems.FirstOrDefault(_ => _.GridRect.Contains(gridPosition));
        }

        /// <summary>
        /// Computes a <see cref="Rect"/> that encompasses the given objects
        /// </summary>
        /// <param name="objects"></param>
        /// <returns></returns>
        public Rect ComputeBoundingRect(IEnumerable<LayoutObject> objects)
        {
            //compute bouding box for given objects
            var result = _statisticsCalculationHelper.CalculateStatistics(objects.Select(_ => _.WrappedAnnoObject), includeRoads: true);
            return new Rect(result.MinX, result.MinY, result.UsedAreaWidth, result.UsedAreaHeight);
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
            _viewport.Left = 0;
            _viewport.Top = 0;

            if (PlacedObjects.Count == 0)
            {
                return;
            }

            var dx = PlacedObjects.Min(_ => _.Position.X) - border;
            var dy = PlacedObjects.Min(_ => _.Position.Y) - border;
            var diff = new Vector(dx, dy);

            UndoManager.RegisterOperation(new MoveObjectsOperation<LayoutObject>()
            {
                ObjectPropertyValues = PlacedObjects.Select(obj => (obj, obj.Bounds, new Rect(obj.Position - diff, obj.Size))).ToList(),
                QuadTree = PlacedObjects
            });

            // make a copy of a list to avoid altering collection during iteration
            var placedObjects = PlacedObjects.ToList();

            foreach (var item in placedObjects)
            {
                PlacedObjects.Move(item, -diff);
            }

            InvalidateVisual();
            InvalidateBounds();
            InvalidateScroll();
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
        }

        #endregion

        #region New/Save/Load/Export methods

        public void CheckUnsavedChangesBeforeCrash()
        {
            if (UndoManager.IsDirty)
            {
                var save = _messageBoxService.ShowQuestion(
                    _localizationHelper.GetLocalization("SaveUnsavedChanges"),
                    _localizationHelper.GetLocalization("UnsavedChangedBeforeCrash")
                );

                if (save)
                {
                    SaveAs();
                }
            }
        }

        /// <summary>
        /// Checks for unsaved changes. Shows Yes/No/Cancel dialog to let user decide what to do.
        /// </summary>
        /// <returns>True if changes were saved or discarded. False if operation should be cancelled.</returns>
        public bool CheckUnsavedChanges()
        {
            if (UndoManager.IsDirty)
            {
                var save = _messageBoxService.ShowQuestionWithCancel(
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
        public void NewFile()
        {
            if (!CheckUnsavedChanges())
            {
                return;
            }

            _viewport.Left = 0;
            _viewport.Top = 0;
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
                return true;
            }
        }

        /// <summary>
        /// Opens a dialog and saves the current layout to file.
        /// </summary>
        public bool SaveAs()
        {
            var dialog = new SaveFileDialog
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
        public void OpenFile()
        {
            if (!CheckUnsavedChanges())
            {
                return;
            }

            var dialog = new OpenFileDialog
            {
                DefaultExt = Constants.SavedLayoutExtension,
                Filter = Constants.SaveOpenDialogFilter
            };

            if (dialog.ShowDialog() == true)
            {
                OpenFileRequested?.Invoke(this, new OpenFileEventArgs(dialog.FileName));
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
                { ApplicationCommands.New, _ => _.NewFile() },
                { ApplicationCommands.Open, _ => _.OpenFile() },
                { ApplicationCommands.Save, _ => _.Save() },
                { ApplicationCommands.SaveAs, _ => _.SaveAs() }
            };

            // register event handlers for the specified commands
            foreach (var action in CommandExecuteMappings)
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
            if (sender is AnnoCanvas canvas && CommandExecuteMappings.ContainsKey(e.Command))
            {
                CommandExecuteMappings[e.Command].Invoke(canvas);
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
        private readonly ICommand rotateCommand;
        private void ExecuteRotate(object param)
        {
            if (CurrentObjects.Count == 1)
            {
                CurrentObjects[0].Size = _coordinateHelper.Rotate(CurrentObjects[0].Size);
                CurrentObjects[0].Direction = _coordinateHelper.Rotate(CurrentObjects[0].Direction);
            }
            else if (CurrentObjects.Count > 1)
            {
                Rotate(CurrentObjects).Consume();
            }
            else
            {
                //Count == 0;
                //Rotate from selected objects
                CurrentObjects = CloneList(SelectedObjects);
                Rotate(CurrentObjects).Consume();
            }
            InvalidateVisual();
        }

        private readonly Hotkey rotateAllHotkey;
        private readonly ICommand rotateAllCommand;
        private void ExecuteRotateAll(object param)
        {
            UndoManager.AsSingleUndoableOperation(() =>
            {
                var placedObjects = PlacedObjects.ToList();
                UndoManager.RegisterOperation(new MoveObjectsOperation<LayoutObject>()
                {
                    QuadTree = PlacedObjects,
                    ObjectPropertyValues = PlacedObjects.Select(obj => (obj, obj.Bounds, _coordinateHelper.Rotate(obj.Bounds))).ToList()
                });

                foreach (var (item, oldRect) in Rotate(placedObjects))
                {
                    PlacedObjects.ReIndex(item, oldRect);
                }
                Normalize(1);
            });

            InvalidateVisual();
        }

        private readonly Hotkey copyHotkey;
        private readonly ICommand copyCommand;
        private void ExecuteCopy(object param)
        {
            if (SelectedObjects.Count != 0)
            {
                ClipboardObjects = CloneList(SelectedObjects);
            }
        }

        private readonly Hotkey pasteHotkey;
        private readonly ICommand pasteCommand;
        private void ExecutePaste(object param)
        {
            if (ClipboardObjects.Count != 0)
            {
                CurrentObjects = CloneList(ClipboardObjects);
                MoveCurrentObjectsToMouse();
            }
        }

        private readonly Hotkey deleteHotkey;
        private readonly ICommand deleteCommand;
        private void ExecuteDelete(object param)
        {
            UndoManager.RegisterOperation(new RemoveObjectsOperation<LayoutObject>()
            {
                Objects = SelectedObjects.ToList(),
                Collection = PlacedObjects
            });

            // remove all currently selected objects from the grid and clear selection
            SelectedObjects.ForEach(item => PlacedObjects.Remove(item));
            SelectedObjects.Clear();
            StatisticsUpdated?.Invoke(this, UpdateStatisticsEventArgs.All);
            CurrentMode = MouseMode.DeleteObject;
        }

        private readonly Hotkey duplicateHotkey;
        private readonly ICommand duplicateCommand;
        private void ExecuteDuplicate(object param)
        {
            var obj = GetObjectAt(_mousePosition);
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
                var obj = GetObjectAt(_mousePosition);
                if (obj != null)
                {
                    // Remove object, only ever remove a single object this way.
                    UndoManager.RegisterOperation(new RemoveObjectsOperation<LayoutObject>()
                    {
                        Objects = new List<LayoutObject>()
                        {
                            obj
                        },
                        Collection = PlacedObjects
                    });

                    PlacedObjects.Remove(obj);
                    RemoveSelectedObject(obj, false);
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
        }

        private readonly Hotkey redoHotkey;
        private readonly ICommand redoCommand;
        private void ExecuteRedo(object param)
        {
            UndoManager.Redo();
        }

        #endregion

        #region Helper methods

        private List<LayoutObject> CloneList(List<LayoutObject> list)
        {
            var newList = new List<LayoutObject>(list.Capacity);
            list.ForEach(_ => newList.Add(new LayoutObject(new AnnoObject(_.WrappedAnnoObject), _coordinateHelper, _brushCache, _penCache)));
            return newList;
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

        #region IScrollInfo

        public double ExtentWidth => _scrollableBounds.Width;
        public double ExtentHeight => _scrollableBounds.Height;
        public double ViewportWidth => _viewport.Width;
        public double ViewportHeight => _viewport.Height;

        public double HorizontalOffset
        {
            get
            {
                if (_appSettings.InvertScrollingDirection)
                {
                    return (_scrollableBounds.Left - _viewport.Left) + (_scrollableBounds.Width - _viewport.Width);
                }
                else
                {
                    return _viewport.Left - _scrollableBounds.Left;
                }
            }
        }

        public double VerticalOffset
        {
            get
            {
                if (_appSettings.InvertScrollingDirection)
                {
                    return (_scrollableBounds.Top - _viewport.Top) + (_scrollableBounds.Height - _viewport.Height);
                }
                else
                {
                    return _viewport.Top - _scrollableBounds.Top;
                }
            }
        }

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
            if (_appSettings.InvertScrollingDirection)
            {
                _viewport.Left = (_scrollableBounds.Left - offset) + (_scrollableBounds.Width - _viewport.Width);
            }
            else
            {
                _viewport.Left = _scrollableBounds.Left + offset;
            }
            _viewport.Left = _scrollableBounds.Left + offset;
            InvalidateScroll();
            InvalidateVisual();
        }

        public void SetVerticalOffset(double offset)
        {
            //handle when offset is +/- infinity (when scrolling to top/bottom using the end and home keys)
            offset = Math.Max(offset, 0d);
            offset = Math.Min(offset, _scrollableBounds.Height);
            if (_appSettings.InvertScrollingDirection)
            {
                _viewport.Top = (_scrollableBounds.Top - offset) + (_scrollableBounds.Height - _viewport.Height);
            }
            else
            {
                _viewport.Top = _scrollableBounds.Top + offset;
            }
            InvalidateScroll();
            InvalidateVisual();
        }

        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            return _viewport.Absolute;
        }

        #endregion
    }
}
