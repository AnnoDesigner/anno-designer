using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AnnoDesigner.Core;
using AnnoDesigner.Core.Layout;
using AnnoDesigner.Core.Layout.Exceptions;
using AnnoDesigner.Core.Layout.Models;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Core.Presets.Loader;
using AnnoDesigner.Core.Presets.Models;
using AnnoDesigner.CustomEventArgs;
using AnnoDesigner.Helper;
using AnnoDesigner.Models;
using Microsoft.Win32;
using NLog;
using MessageBox = Xceed.Wpf.Toolkit.MessageBox;

namespace AnnoDesigner
{
    /// <summary>
    /// Interaction logic for AnnoCanvas.xaml
    /// </summary>
    public partial class AnnoCanvas : UserControl, IAnnoCanvas
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public event EventHandler<UpdateStatisticsEventArgs> StatisticsUpdated;
        public event EventHandler<EventArgs> ColorsInLayoutUpdated;

        #region Properties
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
            get
            {
                return _renderGrid;
            }
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
            get
            {
                return _renderInfluences;
            }
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
            get
            {
                return _renderLabel;
            }
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
            get
            {
                return _renderIcon;
            }
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
            get
            {
                return _renderTrueInfluenceRange;
            }
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
        /// Backing field of the CurrentObject property
        /// </summary>
        private List<LayoutObject> _currentObjects = new List<LayoutObject>();

        /// <summary>
        /// Current object to be placed. Fires an event when changed.
        /// </summary>
        public List<LayoutObject> CurrentObjects
        {
            get
            {
                return _currentObjects;
            }
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
            get
            {
                return _clipboardObjects;
            }
            private set
            {
                if (value != null)
                {
                    _clipboardObjects = value;
                    StatusMessage = value.Count + " items copied";
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
            get
            {
                return _statusMessage;
            }
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
            get
            {
                return _loadedFile;
            }
            set
            {
                if (_loadedFile != value)
                {
                    _loadedFile = value;
                    OnLoadedFileChanged?.Invoke(value);
                }
            }
        }

        /// <summary>
        /// Event which is fired when the status message should be changed.
        /// </summary>
        public event Action<string> OnLoadedFileChanged;

        #endregion

        #region Privates and constructor

        private readonly ILayoutLoader _layoutLoader;
        private readonly ICoordinateHelper _coordinateHelper;
        private readonly IBrushCache _brushCache;
        private readonly IPenCache _penCache;

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
            DragAll
        }

        /// <summary>
        /// Backing field of the CurrentMode property.
        /// </summary>
        private MouseMode _currentMode;

        /// <summary>
        /// Indicates the current mouse mode.
        /// </summary>
        //private MouseMode CurrentMode
        private MouseMode CurrentMode
        {
            get
            {
                return _currentMode;
            }
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
        /// List of all currently placed objects.
        /// </summary>
        public List<LayoutObject> PlacedObjects { get; set; }

        /// <summary>
        /// List of all currently selected objects.
        /// All of them must also be contained in the _placedObjects list.
        /// </summary>
        public List<LayoutObject> SelectedObjects { get; set; }

        private readonly Typeface TYPEFACE = new Typeface("Verdana");

        #region Pens and Brushes

        /// <summary>
        /// Used for grid lines and object borders.
        /// </summary>
        private readonly Pen _linePen;

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

        /// <summary>
        /// Constructor
        /// </summary>
        public AnnoCanvas() : this(null, null)
        {
        }

        public AnnoCanvas(BuildingPresets presetsToUse,
            Dictionary<string, IconImage> iconsToUse,
            ICoordinateHelper coordinateHelperToUse = null,
            IBrushCache brushCacheToUse = null,
            IPenCache penCacheToUse = null)
        {
            InitializeComponent();

            _coordinateHelper = coordinateHelperToUse ?? new CoordinateHelper();
            _brushCache = brushCacheToUse ?? new BrushCache();
            _penCache = penCacheToUse ?? new PenCache();

            _layoutLoader = new LayoutLoader();

            Stopwatch sw = new Stopwatch();
            sw.Start();

            // initialize
            CurrentMode = MouseMode.Standard;

            PlacedObjects = new List<LayoutObject>();
            SelectedObjects = new List<LayoutObject>();

            const int dpiFactor = 1;
            _linePen = _penCache.GetPen(Brushes.Black, dpiFactor * 1);
            _highlightPen = _penCache.GetPen(Brushes.Yellow, dpiFactor * 2);
            _radiusPen = _penCache.GetPen(Brushes.Black, dpiFactor * 2);
            _influencedPen = _penCache.GetPen(Brushes.LawnGreen, dpiFactor * 2);

            var color = Colors.LightYellow;
            color.A = 32;
            _lightBrush = _brushCache.GetSolidBrush(color);
            color = Colors.LawnGreen;
            color.A = 32;
            _influencedBrush = _brushCache.GetSolidBrush(color);

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
                    MessageBox.Show(ex.Message, "Loading of the building presets failed");
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
                        IconMappingPresetsLoader loader = new IconMappingPresetsLoader();
                        iconNameMapping = loader.Load(Path.Combine(App.ApplicationPath, CoreConstants.PresetsFiles.IconNameFile));
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "Loading of the icon names failed.");

                        MessageBox.Show("Loading of the icon names failed",
                            Localization.Localization.Translations[Localization.Localization.GetLanguageCodeFromName(Properties.Settings.Default.SelectedLanguage)]["Error"],
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
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

        #region Rendering
        /// <summary>
        /// Renders the whole scene including grid, placed objects, current object, selection highlights, influence radii and selection rectangle.
        /// </summary>
        /// <param name="drawingContext">context used for rendering</param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            //needed?
            base.OnRender(drawingContext);

            //var m = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice;
            //var dpiFactor = 1 / m.M11;

            // assure pixel perfect drawing
            var halfPenWidth = _linePen.Thickness / 2;
            var guidelines = new GuidelineSet();
            guidelines.GuidelinesX.Add(halfPenWidth);
            guidelines.GuidelinesY.Add(halfPenWidth);
            guidelines.Freeze();
            drawingContext.PushGuidelineSet(guidelines);

            var width = RenderSize.Width;
            var height = RenderSize.Height;

            // draw background
            drawingContext.DrawRectangle(Brushes.Transparent, null, new Rect(new Point(), RenderSize));

            // draw grid
            if (RenderGrid)
            {
                for (var i = 0; i < width; i += _gridStep)
                {
                    drawingContext.DrawLine(_linePen, new Point(i, 0), new Point(i, height));
                }
                for (var i = 0; i < height; i += _gridStep)
                {
                    drawingContext.DrawLine(_linePen, new Point(0, i), new Point(width, i));
                }
            }

            // draw mouse grid position highlight
            //drawingContext.DrawRectangle(_lightBrush, _highlightPen, new Rect(GridToScreen(ScreenToGrid(_mousePosition)), new Size(_gridStep, _gridStep)));

            // draw placed objects            
            RenderObjectList(drawingContext, PlacedObjects, useTransparency: false);
            RenderObjectSelection(drawingContext, SelectedObjects);

            if (!RenderInfluences)
            {
                RenderObjectInfluenceRadius(drawingContext, SelectedObjects);
                RenderObjectInfluenceRange(drawingContext, SelectedObjects);
            }
            else
            {
                RenderObjectInfluenceRadius(drawingContext, PlacedObjects);
                RenderObjectInfluenceRange(drawingContext, PlacedObjects);
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
                    MoveCurrentObjectsToMouse();
                    // draw influence radius
                    RenderObjectInfluenceRadius(drawingContext, CurrentObjects);
                    // draw influence range
                    RenderObjectInfluenceRange(drawingContext, CurrentObjects);
                    // draw with transparency
                    RenderObjectList(drawingContext, CurrentObjects, useTransparency: true);
                }
            }

            // draw selection rect while dragging the mouse
            if (CurrentMode == MouseMode.SelectionRect)
            {
                drawingContext.DrawRectangle(_lightBrush, _highlightPen, _selectionRect);
            }

            // pop back guidlines set
            drawingContext.Pop();
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
                var r = CurrentObjects[0].CalculateScreenRect(GridSize);
                foreach (var obj in CurrentObjects.Skip(1))
                {
                    r.Union(obj.CalculateScreenRect(GridSize));
                }

                var center = _coordinateHelper.GetCenterPoint(r);
                var dx = _mousePosition.X - center.X;
                var dy = _mousePosition.Y - center.Y;

                //Ensure we move only in grid steps, to avoid rounding errors.
                dx = _coordinateHelper.GridToScreen(_coordinateHelper.RoundScreenToGrid(dx, GridSize), GridSize);
                dy = _coordinateHelper.GridToScreen(_coordinateHelper.RoundScreenToGrid(dy, GridSize), GridSize);

                for (var i = 0; i < CurrentObjects.Count; i++)
                {
                    var pos = _coordinateHelper.GridToScreen(CurrentObjects[i].Position, GridSize);
                    CurrentObjects[i].Position = _coordinateHelper.RoundScreenToGrid(new Point(pos.X + dx, pos.Y + dy), GridSize);
                }
            }
            else
            {
                var pos = _mousePosition;
                var size = _coordinateHelper.GridToScreen(CurrentObjects[0].Size, GridSize);
                pos.X -= size.Width / 2;
                pos.Y -= size.Height / 2;
                CurrentObjects[0].Position = _coordinateHelper.RoundScreenToGrid(pos, GridSize);
            }
        }

        /// <summary>
        /// Renders the given AnnoObject to the given DrawingContext.
        /// </summary>
        /// <param name="drawingContext">context used for rendering</param>
        /// <param name="obj">object to render</param>
        private void RenderObjectList(DrawingContext drawingContext, List<LayoutObject> objects, bool useTransparency)
        {
            foreach (var curLayoutObject in objects)
            {
                var obj = curLayoutObject.WrappedAnnoObject;

                // draw object rectangle
                var objRect = curLayoutObject.CalculateScreenRect(GridSize);

                var brush = useTransparency ? curLayoutObject.TransparentBrush : curLayoutObject.RenderBrush;

                var borderPen = obj.Borderless ? curLayoutObject.GetBorderlessPen(brush, _linePen.Thickness) : _linePen;
                drawingContext.DrawRectangle(brush, borderPen, objRect);

                // draw object icon if it is at least 2x2 cells
                var iconRendered = false;
                if (RenderIcon && !string.IsNullOrEmpty(obj.Icon))
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
                        var iconRect = curLayoutObject.GetIconRect(GridSize);

                        drawingContext.DrawImage(curLayoutObject.Icon.Icon, iconRect);
                        iconRendered = true;
                    }
                }

                // draw object label
                if (RenderLabel && !string.IsNullOrEmpty(obj.Label))
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

                    foreach (var curPlacedObject in PlacedObjects)
                    {
                        var distance = curPlacedObject.GetScreenRectCenterPoint(GridSize);
                        distance.X -= circleCenterX;
                        distance.Y -= circleCenterY;
                        // check if the center is within the influence circle
                        if ((distance.X * distance.X) + (distance.Y * distance.Y) <= radius * radius)
                        {
                            drawingContext.DrawRectangle(_influencedBrush, _influencedPen, curPlacedObject.CalculateScreenRect(GridSize));
                        }
                        //o.Label = (Math.Sqrt(distance.X*distance.X + distance.Y*distance.Y) - Math.Sqrt(radius*radius)).ToString();
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
            AnnoObject[][] gridDictionary = null;
            if (RenderTrueInfluenceRange && PlacedObjects.Count > 0)
            {
                var placedAnnoObjects = PlacedObjects.Concat(objects).Select(o => o.WrappedAnnoObject).ToList();
                var placedObjectDictionary = PlacedObjects.ToDictionary(o => o.WrappedAnnoObject);

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
                            DrawTrueInfluenceRangePolygon(curLayoutObject, sgc, gridDictionary);
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

        private void DrawTrueInfluenceRangePolygon(LayoutObject curLayoutObject, StreamGeometryContext sgc, AnnoObject[][] gridDictionary)
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
                PlacedObjects.Select(o => o.WrappedAnnoObject),
                startObjects,
                o => (int)o.InfluenceRange,
                gridDictionary);

            var points = PolygonBoundaryFinderHelper.GetBoundaryPoints(cellsInInfluenceRange);
            if (points.Count < 1)
            {
                return;
            }

            sgc.BeginFigure(_coordinateHelper.GridToScreen(new Point(points[0].x, points[0].y), GridSize), geometryFill, geometryStroke);
            for (var i = 1; i < points.Count; i++)
            {
                sgc.LineTo(_coordinateHelper.GridToScreen(new Point(points[i].x, points[i].y), GridSize), stroked, smoothJoin);
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

        private List<LayoutObject> CloneList(List<LayoutObject> list)
        {
            var newList = new List<LayoutObject>(list.Capacity);
            list.ForEach(_ => newList.Add(new LayoutObject(new AnnoObject(_.WrappedAnnoObject), _coordinateHelper, _brushCache, _penCache)));
            return newList;
        }

        #endregion

        #region Coordinate and rectangle conversions

        /// <summary>
        /// Rotates a group of objects.
        /// </summary>
        /// <param name="objects"></param>
        private void Rotate(List<LayoutObject> objects)
        {
            for (var i = 0; i < objects.Count; i++)
            {
                objects[i].Size = _coordinateHelper.Rotate(objects[i].Size);
                var position = objects[i].Position;
                //Full formula left in for explanation
                //var xPrime = x * Math.Cos(angle) - y * Math.Sin(angle);
                //var yPrime = x * Math.Sin(angle) - y * Math.Cos(angle);

                //Cos 90 = 0, sin 90 = 1
                //Therefore, the below is equivalent
                var xPrime = 0 - position.Y;
                var yPrime = position.X;

                //When the building is rotated, the xPrime and yPrime values no 
                //longer represent the top left corner, they will represent the 
                //top-right corner instead. We need to account for this, by 
                //moving the xPrime position (still in grid coordinates).
                xPrime -= objects[i].Size.Width;

                objects[i].Position = new Point(xPrime, yPrime);
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

            InvalidateVisual();
        }

        /// <summary>
        /// Handles the zoom level
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            GridSize += e.Delta / 100;
        }

        private void HandleMouse(MouseEventArgs e)
        {
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

            if (e.ClickCount > 1)
            {
                var obj = GetObjectAt(_mousePosition);
                if (obj != null)
                {
                    CurrentObjects.Clear();
                    CurrentObjects.Add(new LayoutObject(new AnnoObject(obj.WrappedAnnoObject), _coordinateHelper, _brushCache, _penCache));
                    OnCurrentObjectChanged(obj);
                }
                return;
            }

            _mouseDragStart = _mousePosition;

            if (e.LeftButton == MouseButtonState.Pressed && e.RightButton == MouseButtonState.Pressed)
            {
                CurrentMode = MouseMode.DragAllStart;
            }
            else if (e.LeftButton == MouseButtonState.Pressed && CurrentObjects.Count != 0)
            {
                // place new object
                TryPlaceCurrentObject(isContinuousDrawing: false);
            }
            else if (e.LeftButton == MouseButtonState.Pressed && CurrentObjects.Count == 0)
            {
                var obj = GetObjectAt(_mousePosition);
                if (obj == null)
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
            if (Math.Abs(_mouseDragStart.X - _mousePosition.X) > 1 || Math.Abs(_mouseDragStart.Y - _mousePosition.Y) > 1)
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
                        SelectedObjects.Add(GetObjectAt(_mouseDragStart));
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
                // check if the mouse has moved at least one grid cell in any direction
                if (dx != 0 || dy != 0)
                {
                    foreach (var curLayoutObject in PlacedObjects)
                    {
                        curLayoutObject.Position = new Point(curLayoutObject.Position.X + dx, curLayoutObject.Position.Y + dy);
                    }
                    // adjust the drag start to compensate the amount we already moved
                    _mouseDragStart.X += _coordinateHelper.GridToScreen(dx, GridSize);
                    _mouseDragStart.Y += _coordinateHelper.GridToScreen(dy, GridSize);

                    //all is moved -> no need to update statistics
                    //StatisticsUpdated?.Invoke(this, EventArgs.Empty);
                }
            }
            else if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (CurrentObjects.Count != 0)
                {
                    // place new object
                    TryPlaceCurrentObject(isContinuousDrawing: true);
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
                                    SelectedObjects.RemoveAll(_ => _.CalculateScreenRect(GridSize).IntersectsWith(_selectionRect));
                                }
                                else
                                {
                                    SelectedObjects.Clear();
                                }

                                // adjust rect
                                _selectionRect = new Rect(_mouseDragStart, _mousePosition);
                                // select intersecting objects
                                SelectedObjects.AddRange(PlacedObjects.FindAll(_ => _.CalculateScreenRect(GridSize).IntersectsWith(_selectionRect)));

                                StatisticsUpdated?.Invoke(this, UpdateStatisticsEventArgs.All);
                                break;
                            }
                        case MouseMode.DragSelection:
                            {
                                // move all selected objects
                                var dx = (int)_coordinateHelper.ScreenToGrid(_mousePosition.X - _mouseDragStart.X, GridSize);
                                var dy = (int)_coordinateHelper.ScreenToGrid(_mousePosition.Y - _mouseDragStart.Y, GridSize);
                                // check if the mouse has moved at least one grid cell in any direction
                                if (dx == 0 && dy == 0)
                                {
                                    //no relevant mouse move -> no further action                                    
                                    break;
                                }

                                if (_unselectedObjects == null)
                                {
                                    _unselectedObjects = PlacedObjects.FindAll(_ => !SelectedObjects.Contains(_));
                                }

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

                                    //position change -> update
                                    StatisticsUpdated?.Invoke(this, new UpdateStatisticsEventArgs(UpdateMode.NoBuildingList));
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
                                    SelectedObjects.Remove(obj);
                                }
                                else
                                {
                                    SelectedObjects.Add(obj);
                                }
                            }

                            StatisticsUpdated?.Invoke(this, UpdateStatisticsEventArgs.All);
                            // return to standard mode, i.e. clear any drag-start modes
                            CurrentMode = MouseMode.Standard;
                            break;
                        }
                    case MouseMode.SelectionRect:
                        // cancel dragging of selection rect
                        CurrentMode = MouseMode.Standard;
                        break;
                    case MouseMode.DragSelection:
                        // stop dragging of selected objects
                        CurrentMode = MouseMode.Standard;
                        break;
                }
            }
            else if (e.ChangedButton == MouseButton.Right)
            {
                switch (CurrentMode)
                {
                    case MouseMode.Standard:
                        {
                            if (CurrentObjects.Count == 0)
                            {
                                var obj = GetObjectAt(_mousePosition);
                                if (obj == null)
                                {
                                    if (!(IsControlPressed() || IsShiftPressed()))
                                    {
                                        // clear selection
                                        SelectedObjects.Clear();
                                    }
                                }
                                else
                                {
                                    // remove selected object
                                    PlacedObjects.Remove(obj);
                                    SelectedObjects.Remove(obj);
                                }
                            }
                            else
                            {
                                // cancel placement of object
                                CurrentObjects.Clear();
                            }

                            StatisticsUpdated?.Invoke(this, UpdateStatisticsEventArgs.All);
                            break;
                        }
                    case MouseMode.DragSelection:
                        {
                            //clear selection
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
            // rotate current object
            else if (e.ChangedButton == MouseButton.Middle)
            {
                if (CurrentObjects.Count == 0 && SelectedObjects.Count != 0)
                {
                    CurrentObjects = CloneList(SelectedObjects);
                }

                Rotate(CurrentObjects);
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
            switch (e.Key)
            {
                case Key.Delete:
                    // remove all currently selected objects from the grid and clear selection
                    SelectedObjects.ForEach(_ => PlacedObjects.Remove(_));
                    SelectedObjects.Clear();
                    StatisticsUpdated?.Invoke(this, UpdateStatisticsEventArgs.All);
                    break;
                case Key.C:
                    if (IsControlPressed())
                    {
                        if (SelectedObjects.Count != 0)
                        {
                            ClipboardObjects = CloneList(SelectedObjects);
                        }
                    }
                    break;
                case Key.V:
                    if (IsControlPressed())
                    {
                        if (ClipboardObjects.Count != 0)
                        {
                            CurrentObjects = CloneList(ClipboardObjects);
                            MoveCurrentObjectsToMouse();
                        }
                    }
                    break;
                case Key.R:
                    if (CurrentObjects.Count == 1)
                    {
                        CurrentObjects[0].Size = _coordinateHelper.Rotate(CurrentObjects[0].Size);
                    }
                    else if (CurrentObjects.Count > 1)
                    {
                        Rotate(CurrentObjects);
                    }
                    else
                    {
                        //Count == 0;
                        //Rotate from selected objects
                        CurrentObjects = CloneList(SelectedObjects);
                        Rotate(CurrentObjects);
                    }
                    break;

            }

            InvalidateVisual();
        }

        /// <summary>
        /// Checks whether the user is pressing the control key.
        /// </summary>
        /// <returns></returns>
        private static bool IsControlPressed()
        {
            return Keyboard.Modifiers.HasFlag(ModifierKeys.Control) || Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);
        }

        /// <summary>
        /// Checks whether the user is pressing the shift key.
        /// </summary>
        /// <returns></returns>
        private static bool IsShiftPressed()
        {
            return Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);
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
        /// Tries to place the current object on the grid.
        /// Fails if there are any collisions.
        /// </summary>
        /// <param name="isContinuousDrawing"><c>true</c> if drawing the same object(s) over and over</param>
        /// <returns>true if placement succeeded, otherwise false</returns>
        private bool TryPlaceCurrentObject(bool isContinuousDrawing)
        {
            if (CurrentObjects.Count != 0 && !PlacedObjects.Exists(_ => ObjectIntersectionExists(CurrentObjects, _)))
            {
                PlacedObjects.AddRange(CloneList(CurrentObjects));
                // sort the objects because borderless objects should be drawn first
                PlacedObjects.Sort((a, b) => b.WrappedAnnoObject.Borderless.CompareTo(a.WrappedAnnoObject.Borderless));

                StatisticsUpdated?.Invoke(this, UpdateStatisticsEventArgs.All);
                //no need to update colors if drawing the same object(s)
                if (!isContinuousDrawing)
                {
                    ColorsInLayoutUpdated?.Invoke(this, EventArgs.Empty);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Retrieves the object at the given position given in screen coordinates.
        /// </summary>
        /// <param name="position">position given in screen coordinates</param>
        /// <returns>object at the position, if there is no object null</returns>
        private LayoutObject GetObjectAt(Point position)
        {
            var gridPosition = _coordinateHelper.ScreenToGrid(position, GridSize);
            return PlacedObjects.Find(_ => _.CollisionRect.Contains(gridPosition));
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
            // note: setting of the backing field doens't fire the changed event
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

            var dx = PlacedObjects.Min(_ => _.Position.X) - border;
            var dy = PlacedObjects.Min(_ => _.Position.Y) - border;
            PlacedObjects.ForEach(_ => _.Position = new Point(_.Position.X - dx, _.Position.Y - dy));

            InvalidateVisual();
        }

        #endregion

        #region New/Save/Load/Export methods

        /// <summary>
        /// Removes all objects from the grid.
        /// </summary>
        public void NewFile()
        {
            PlacedObjects.Clear();
            SelectedObjects.Clear();
            LoadedFile = "";
            InvalidateVisual();

            StatisticsUpdated?.Invoke(this, UpdateStatisticsEventArgs.All);
            ColorsInLayoutUpdated?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Writes layout to file.
        /// </summary>
        private void SaveFile()
        {
            try
            {
                Normalize(1);
                _layoutLoader.SaveLayout(PlacedObjects.Select(x => x.WrappedAnnoObject).ToList(), LoadedFile);
            }
            catch (Exception e)
            {
                IOErrorMessageBox(e);
            }
        }

        /// <summary>
        /// Saves the current layout to file.
        /// </summary>
        public void Save()
        {
            if (string.IsNullOrEmpty(LoadedFile))
            {
                SaveAs();
            }
            else
            {
                SaveFile();
            }
        }

        /// <summary>
        /// Opens a dialog and saves the current layout to file.
        /// </summary>
        public void SaveAs()
        {
            var dialog = new SaveFileDialog
            {
                DefaultExt = Constants.SavedLayoutExtension,
                Filter = Constants.SaveOpenDialogFilter
            };
            if (dialog.ShowDialog() == true)
            {
                LoadedFile = dialog.FileName;
                SaveFile();
            }
        }

        /// <summary>
        /// Opens a dialog and loads the given file.
        /// </summary>
        public void OpenFile()
        {
            var dialog = new OpenFileDialog
            {
                DefaultExt = Constants.SavedLayoutExtension,
                Filter = Constants.SaveOpenDialogFilter
            };

            if (dialog.ShowDialog() == true)
            {
                OpenFile(dialog.FileName);
            }
        }

        /// <summary>
        /// Loads a new layout from file.
        /// </summary>
        public void OpenFile(string filename, bool forceLoad = false)
        {
            try
            {
                var layout = _layoutLoader.LoadLayout(filename, forceLoad);
                if (layout != null)
                {
                    SelectedObjects.Clear();

                    var layoutObjects = new List<LayoutObject>(layout.Count);
                    foreach (var curObj in layout)
                    {
                        layoutObjects.Add(new LayoutObject(curObj, _coordinateHelper, _brushCache, _penCache));
                    }

                    PlacedObjects = layoutObjects;
                    LoadedFile = filename;
                    Normalize(1);

                    StatisticsUpdated?.Invoke(this, UpdateStatisticsEventArgs.All);
                    ColorsInLayoutUpdated?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (LayoutFileVersionMismatchException layoutEx)
            {
                logger.Warn(layoutEx, "Version of layout does not match.");

                if (MessageBox.Show(
                        "Try loading anyway?\nThis is very likely to fail or result in strange things happening.",
                        "File version mismatch", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    OpenFile(filename, true);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error loading layout from JSON.");

                IOErrorMessageBox(ex);
            }
        }

        /// <summary>
        /// Displays a message box containing some error information.
        /// </summary>
        /// <param name="e">exception containing error information</param>
        private static void IOErrorMessageBox(Exception e)
        {
            MessageBox.Show(e.Message, "Something went wrong while saving/loading file.");
        }

        #endregion

        #region Commands

        /// <summary>
        /// Holds event handlers for command executions.
        /// </summary>
        private static readonly Dictionary<ICommand, Action<AnnoCanvas>> CommandExecuteMappings;

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

        #endregion
    }
}
