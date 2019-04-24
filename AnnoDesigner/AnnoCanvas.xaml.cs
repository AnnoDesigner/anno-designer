using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AnnoDesigner.Presets;
using AnnoDesigner.UI;
using Microsoft.Win32;
using MessageBox = Microsoft.Windows.Controls.MessageBox;

namespace AnnoDesigner
{
    /// <summary>
    /// Interaction logic for AnnoCanvas.xaml
    /// </summary>
    public partial class AnnoCanvas
        : UserControl
    {
        #region Properties

        /// <summary>
        /// Contains all loaded icons as a mapping of name (the filename without extension) to loaded BitmapImage.
        /// </summary>
        public readonly Dictionary<string, IconImage> Icons;

        public readonly BuildingPresets BuildingPresets;

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
            get
            {
                return _gridStep;
            }
            set
            {
                var tmp = value;
                if (tmp < Constants.GridStepMin)
                    tmp = Constants.GridStepMin;
                if (tmp > Constants.GridStepMax)
                    tmp = Constants.GridStepMax;
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
        /// Backing field of the RenderStats property.
        /// </summary>
        private bool _renderStats;

        /// <summary>
        /// Gets or sets a value indicating whether the calculated statistics of the layout should be rendered.
        /// </summary>
        public bool RenderStats
        {
            get
            {
                return _renderStats;
            }
            set
            {
                if (_renderStats != value)
                {
                    InvalidateVisual();
                }
                _renderStats = value;
            }
        }

        private bool _renderBuildingCount = false;

        /// <summary>
        /// Gets or sets a value indicating whether the calculated building statistics of the layout should be rendered.
        /// </summary>
        public bool RenderBuildingCount
        {
            get
            {
                return _renderBuildingCount;
            }
            set
            {
                if (_renderBuildingCount != value)
                {
                    InvalidateVisual();
                }
                _renderBuildingCount = value;
            }
        }

        /// <summary>
        /// Backing field of the CurrentObject property
        /// </summary>
        private List<AnnoObject> _currentObjects = new List<AnnoObject>();

        /// <summary>
        /// Current object to be placed. Fires an event when changed.
        /// </summary>
        public List<AnnoObject> CurrentObjects
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
        public event Action<AnnoObject> OnCurrentObjectChanged;

        /// <summary>
        /// backing field of the ObjectClipboard property
        /// </summary>
        private List<AnnoObject> _objectClipboard = new List<AnnoObject>();

        /// <summary>
        /// Holds a list of objects that are currently on the clipboard.
        /// </summary>
        public List<AnnoObject> ObjectClipboard
        {
            get
            {
                return _objectClipboard;
            }
            private set
            {
                if (value != null)
                {
                    _objectClipboard = value;
                    StatusMessage = value.Count + " items copied";
                    OnClipboardChanged?.Invoke(value);
                }
            }
        }

        /// <summary>
        /// Event which is fired when the clipboard content is changed.
        /// </summary>
        public event Action<List<AnnoObject>> OnClipboardChanged;

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
            private set
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
        private List<AnnoObject> _placedObjects;

        /// <summary>
        /// List of all currently selected objects.
        /// All of them must also be contained in the _placedObjects list.
        /// </summary>
        private readonly List<AnnoObject> _selectedObjects;

        private readonly Typeface TYPEFACE = new Typeface("Verdana");

        #region Pens and Brushes

        /// <summary>
        /// Used for grid lines and object borders.
        /// </summary>
        private readonly Pen _linePen;

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
        public AnnoCanvas()
        {
            InitializeComponent();
            // control settings
            Focusable = true;
            ClipToBounds = true;
            // initialize
            CurrentMode = MouseMode.Standard;
            _placedObjects = new List<AnnoObject>();
            _selectedObjects = new List<AnnoObject>();
            _linePen = new Pen(Brushes.Black, 1);
            _highlightPen = new Pen(Brushes.Yellow, 1);
            _radiusPen = new Pen(Brushes.Black, 1);
            _influencedPen = new Pen(Brushes.LawnGreen, 1);
            var color = Colors.LightYellow;
            color.A = 32;
            _lightBrush = new SolidColorBrush(color);
            color = Colors.LawnGreen;
            color.A = 32;
            _influencedBrush = new SolidColorBrush(color);
            // load presets and icons if not in design time
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                // load presets
                try
                {
                    BuildingPresets = DataIO.LoadFromFile<BuildingPresets>(Path.Combine(App.ApplicationPath, Constants.BuildingPresetsFile));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Loading of the building presets failed");
                }
                // load icon name mapping
                List<IconNameMap> iconNameMap = null;
                try
                {
                    iconNameMap = DataIO.LoadFromFile<List<IconNameMap>>(Path.Combine(App.ApplicationPath, Constants.IconNameFile));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Loading of the icon names failed");
                }
                var icons = new Dictionary<string, IconImage>();
                foreach (var path in Directory.GetFiles(Path.Combine(App.ApplicationPath, Constants.IconFolder), Constants.IconFolderFilter))
                {
                    var filenameWithExt = Path.GetFileName(path);
                    var filenameWithoutExt = Path.GetFileNameWithoutExtension(path);
                    if (!string.IsNullOrEmpty(filenameWithoutExt))
                    {
                        // try mapping to the icon translations
                        Dictionary<string, string> localizations = null;
                        if (iconNameMap != null)
                        {
                            var map = iconNameMap.Find(_ => _.IconFilename == filenameWithExt);
                            if (map != null)
                            {
                                localizations = map.Localizations.Dict;
                            }
                        }
                        // add the current icon
                        icons.Add(filenameWithoutExt, new IconImage(filenameWithoutExt, localizations, new BitmapImage(new Uri(path))));
                    }
                }
                // sort icons by its DisplayName
                Icons = icons.OrderBy(_ => _.Value.DisplayName).ToDictionary(_ => _.Key, _ => _.Value);
            }

            const int dpiFactor = 1;
            _linePen.Thickness = dpiFactor * 1;
            _highlightPen.Thickness = dpiFactor * 2;
            _radiusPen.Thickness = dpiFactor * 2;
            _influencedPen.Thickness = dpiFactor * 2;

            _linePen.Freeze();
            _highlightPen.Freeze();
            _radiusPen.Freeze();
            _influencedPen.Freeze();
            _lightBrush.Freeze();
            _influencedBrush.Freeze();
        }

        #endregion

        #region Rendering
        /// <summary>
        /// Renders the whole scene including grid, placed objects, current object, selection highlights, influence radii and selection rectangle.
        /// </summary>
        /// <param name="drawingContext">context used for rendering</param>
        protected override void OnRender(DrawingContext drawingContext)
        {
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

            // apply offset when rendering statistics
            if (RenderStats)
            {
                width -= Constants.StatisticsMargin;
            }

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
            RenderObject(drawingContext, _placedObjects);
            RenderObjectInfluenceRadius(drawingContext, _selectedObjects.Where(_ => _.Radius > 0.5).ToList());
            _selectedObjects.ForEach(_ => RenderObjectSelection(drawingContext, _));

            if (CurrentObjects.Count == 0)
            {
                // highlight object which is currently hovered
                var hoveredObj = GetObjectAt(_mousePosition);
                if (hoveredObj != null)
                {
                    drawingContext.DrawRectangle(null, _highlightPen, GetObjectScreenRect(hoveredObj));
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
                    // draw with transparency
                    CurrentObjects.ForEach(_ => _.Color.A = 128);
                    RenderObject(drawingContext, CurrentObjects);
                    CurrentObjects.ForEach(_ => _.Color.A = 255);
                }
            }

            // draw selection rect while dragging the mouse
            if (CurrentMode == MouseMode.SelectionRect)
            {
                drawingContext.DrawRectangle(_lightBrush, _highlightPen, _selectionRect);
            }

            // draw additional information
            if (RenderStats)
            {
                RenderStatistics(drawingContext);
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
                var r = GetObjectScreenRect(CurrentObjects[0]);
                foreach (var obj in CurrentObjects.Skip(1))
                {
                    r.Union(GetObjectScreenRect(obj));
                }

                Point center = GetCenterPoint(r);

                var dx = _mousePosition.X - center.X;
                var dy = _mousePosition.Y - center.Y;

                //Ensure we move only in grid steps, to avoid rounding errors.
                dx = GridToScreen(RoundScreenToGrid(dx));
                dy = GridToScreen(RoundScreenToGrid(dy));


                for (int i = 0; i < CurrentObjects.Count; i++)
                {
                    var pos = GridToScreen(CurrentObjects[i].Position);
                    CurrentObjects[i].Position.X = pos.X + dx;
                    CurrentObjects[i].Position.Y = pos.Y + dy;
                    CurrentObjects[i].Position = RoundScreenToGrid(CurrentObjects[i].Position);
                }
            }
            else
            {

                var pos = _mousePosition;
                var size = GridToScreen(CurrentObjects[0].Size);
                pos.X -= size.Width / 2;
                pos.Y -= size.Height / 2;
                CurrentObjects[0].Position = RoundScreenToGrid(pos);
            }
        }

        /// <summary>
        /// Renders the given AnnoObject to the given DrawingContext.
        /// </summary>
        /// <param name="drawingContext">context used for rendering</param>
        /// <param name="obj">object to render</param>
        private void RenderObject(DrawingContext drawingContext, List<AnnoObject> objects)
        {

            foreach (var obj in objects)
            {

                // draw object rectangle
                var objRect = GetObjectScreenRect(obj);
                var brush = new SolidColorBrush(obj.Color);
                brush.Freeze();


                if (obj.Borderless)
                {
                    var borderlessPen = new Pen(brush, _linePen.Thickness);
                    borderlessPen.Freeze();
                    drawingContext.DrawRectangle(brush, borderlessPen, objRect);
                }
                else
                {
                    drawingContext.DrawRectangle(brush, _linePen, objRect);
                }

                // draw object icon if it is at least 2x2 cells
                var iconRendered = false;
                if (_renderIcon && !string.IsNullOrEmpty(obj.Icon))
                {
                    // draw icon 2x2 grid cells large
                    var minSize = Math.Min(obj.Size.Width, obj.Size.Height);
                    //minSize = minSize == 1 ? minSize : Math.Floor(NthRoot(minSize, Constants.IconSizeFactor) + 1);
                    var iconSize = GridToScreen(new Size(minSize, minSize));
                    iconSize = minSize == 1 ? iconSize : new Size(NthRoot(iconSize.Width, Constants.IconSizeFactor), NthRoot(iconSize.Height, Constants.IconSizeFactor));

                    // center icon within the object
                    var iconPos = objRect.TopLeft;
                    iconPos.X += objRect.Width / 2 - iconSize.Width / 2;
                    iconPos.Y += objRect.Height / 2 - iconSize.Height / 2;
                    var iconName = Path.GetFileNameWithoutExtension(obj.Icon); // for backwards compatibility to older layouts
                    if (iconName != null && Icons.ContainsKey(iconName))
                    {
                        drawingContext.DrawImage(Icons[iconName].Icon, new Rect(iconPos, iconSize));
                        iconRendered = true;
                    }
                    else
                    {
                        StatusMessage = string.Format("Icon file missing ({0}).", iconName);
                    }
                }
                // draw object label
                if (_renderLabel && obj.Label != "")
                {
                    var textPoint = objRect.TopLeft;
                    var text = new FormattedText(obj.Label, Thread.CurrentThread.CurrentCulture, FlowDirection.LeftToRight,
                                                 TYPEFACE, 12, Brushes.Black, null, TextFormattingMode.Display)
                    {
                        MaxTextWidth = objRect.Width,
                        MaxTextHeight = objRect.Height
                    };
                    if (iconRendered)
                    {
                        // place the text in the top left corner if a icon is present
                        text.TextAlignment = TextAlignment.Left;
                        textPoint.X += 3;
                        textPoint.Y += 2;
                    }
                    else
                    {
                        // center the text if no icon is present
                        text.TextAlignment = TextAlignment.Center;
                        textPoint.Y += (objRect.Height - text.Height) / 2;
                    }
                    drawingContext.DrawText(text, textPoint);
                }
            }
        }

        /// <summary>
        /// Renders a selection highlight on the specified object.
        /// </summary>
        /// <param name="drawingContext">context used for rendering</param>
        /// <param name="obj">object to render as selected</param>
        private void RenderObjectSelection(DrawingContext drawingContext, AnnoObject obj)
        {
            // draw object rectangle
            var objRect = GetObjectScreenRect(obj);
            drawingContext.DrawRectangle(null, _highlightPen, objRect);
        }

        /// <summary>
        /// Renders the influence radius of the given object and highlights other objects within range.
        /// </summary>
        /// <param name="drawingContext">context used for rendering</param>
        /// <param name="obj">object which's influence is rendered</param>
        private void RenderObjectInfluenceRadius(DrawingContext drawingContext, List<AnnoObject> objects)
        {
            foreach (var obj in objects)
            {

                //This if statement has been moved outside the method to calling code
                //if (obj.Radius >= 0.5)
                //{
                    // highlight buildings within influence
                    var radius = GridToScreen(obj.Radius);
                    var circle = new EllipseGeometry(GetCenterPoint(GetObjectScreenRect(obj)), radius, radius);
                    circle.Freeze();
                    foreach (var o in _placedObjects)
                    {
                        var oRect = GetObjectScreenRect(o);
                        var distance = GetCenterPoint(oRect);
                        distance.X -= circle.Center.X;
                        distance.Y -= circle.Center.Y;
                        // check if the center is within the influence circle
                        if (distance.X * distance.X + distance.Y * distance.Y <= radius * radius)
                        {
                            drawingContext.DrawRectangle(_influencedBrush, _influencedPen, oRect);
                        }
                        //o.Label = (Math.Sqrt(distance.X*distance.X + distance.Y*distance.Y) - Math.Sqrt(radius*radius)).ToString();
                    }
                    // draw circle
                    drawingContext.DrawGeometry(_lightBrush, _radiusPen, circle);
                //}
            }
        }

        /// <summary>
        /// Renders calculated statistics of the current layout like the bounding box and space efficiency
        /// </summary>
        /// <param name="drawingContext">context used for rendering</param>
        protected void RenderStatistics(DrawingContext drawingContext)
        {
            // clear background
            var offset = RenderSize.Width - Constants.StatisticsMargin;

            var informationLines = new List<string>(); 
            if (!_placedObjects.Any())
            {
                informationLines.Add("Nothing placed");
            }
            else
            {
                informationLines.Capacity = 9; //There will be at least 9 lines in this list 

                // calculate bouding box
                var boxX = _placedObjects.Max(_ => _.Position.X + _.Size.Width) - _placedObjects.Min(_ => _.Position.X);
                var boxY = _placedObjects.Max(_ => _.Position.Y + _.Size.Height) - _placedObjects.Min(_ => _.Position.Y);
                // calculate area of all buildings
                var minTiles = _placedObjects.Where(_ => !_.Road).Sum(_ => _.Size.Width * _.Size.Height);

                // format lines
                informationLines.Add("Bounding Box");
                informationLines.Add(string.Format(" {0}x{1}", boxX, boxY));
                informationLines.Add(string.Format(" {0} Tiles", boxX * boxY));
                informationLines.Add("");
                informationLines.Add("Minimum Area");
                informationLines.Add(string.Format(" {0} Tiles", minTiles));
                informationLines.Add("");
                informationLines.Add("Space efficiency");
                informationLines.Add(string.Format(" {0}%", Math.Round(minTiles / boxX / boxY * 100)));

                if (_renderBuildingCount)
                {
                    informationLines.Add("");

                    IEnumerable<IGrouping<string, AnnoObject>> groupedBuildings;
                    if (_selectedObjects.Count > 0)
                    {
                        informationLines.Add("Buildings Selected");
                        groupedBuildings = _selectedObjects.GroupBy(_ => _.Identifier);
                    }
                    else
                    {
                        informationLines.Add("Buildings");
                        groupedBuildings = _placedObjects.GroupBy(_ => _.Identifier);
                    }
                    foreach (var item in groupedBuildings
                        .Where(_ => _.ElementAt(0).Road == false)
                        .Where(_ => _.ElementAt(0).Identifier != null)
                        .OrderByDescending(_ => _.Count()))
                    {
                        if (item.ElementAt(0).Identifier != null && item.ElementAt(0).Identifier != "")
                        {
                            if (BuildingPresets.Buildings.FirstOrDefault(_ => _.Identifier == item.ElementAt(0).Identifier) != null)
                            {
                                var building = BuildingPresets.Buildings.First(_ => _.Identifier == item.ElementAt(0).Identifier);
                                informationLines.Add(string.Format("{0} x {1}", item.Count(), building.Localization[Localization.Localization.GetLanguageCodeFromName(MainWindow.SelectedLanguage)]));
                            }
                            else
                            {
                                item.ElementAt(0).Identifier = "";
                                informationLines.Add(string.Format("{0} x Building name not found", item.Count()));
                            }
                        }
                        else
                        {
                            informationLines.Add(string.Format("{0} x Building name not found", item.Count()));
                        }
                    }
                }
            }
            // render all the lines
            var text = String.Join("\n", informationLines);
            var f = new FormattedText(text, Thread.CurrentThread.CurrentCulture, FlowDirection.LeftToRight,
                                           TYPEFACE, 12, Brushes.Black, null, TextFormattingMode.Display)
            {
                MaxTextWidth = Constants.StatisticsMargin - 20,
                MaxTextHeight = RenderSize.Height,
                TextAlignment = TextAlignment.Left
            };
            drawingContext.DrawText(f, new Point(RenderSize.Width - Constants.StatisticsMargin + 10, 10));
        }

        //I was really just checking to see if there was a built in function, but this works
        //https://stackoverflow.com/questions/18657508/c-sharp-find-nth-root
        [Pure]
        static double NthRoot(double A, double N)
        {
            return Math.Pow(A, 1.0 / N);
        }

        static List<AnnoObject> CloneList(List<AnnoObject> list)
        {
            var newList = new List<AnnoObject>(list.Capacity);
            list.ForEach(_ => newList.Add(new AnnoObject(_)));
            return newList;
        }

        #endregion

        #region Coordinate and rectangle conversions

        /// <summary>
        /// Convert a screen coordinate to a grid coordinate by determining in which grid cell the point is contained.
        /// </summary>
        /// <param name="screenPoint"></param>
        /// <returns></returns>
        [Pure]
        private Point ScreenToGrid(Point screenPoint)
        {
            return new Point(Math.Floor(screenPoint.X / _gridStep), Math.Floor(screenPoint.Y / _gridStep));
        }

        /// <summary>
        /// Converts a screen coordinate to a grid coordinate by determining which grid cell is nearest.
        /// </summary>
        /// <param name="screenPoint"></param>
        /// <returns></returns>
        [Pure]
        private Point RoundScreenToGrid(Point screenPoint)
        {
            return new Point(Math.Round(screenPoint.X / _gridStep), Math.Round(screenPoint.Y / _gridStep));
        }


        /// <summary>
        /// Converts a length given in (pixel-)units to grid coordinate by determining which grid edge is nearest.
        /// </summary>
        /// <param name="screenLength"></param>
        /// <returns></returns>
        [Pure]
        private double RoundScreenToGrid(double screenLength)
        {
            return Math.Round(screenLength / _gridStep);
        }

        /// <summary>
        /// Converts a length given in (pixel-)units to a length given in grid cells.
        /// </summary>
        /// <param name="screenLength"></param>
        /// <returns></returns>
        [Pure]
        private double ScreenToGrid(double screenLength)
        {
            return screenLength / _gridStep;
        }

        /// <summary>
        /// Convert a grid coordinate to a screen coordinate.
        /// </summary>
        /// <param name="gridPoint"></param>
        /// <returns></returns>
        [Pure]
        private Point GridToScreen(Point gridPoint)
        {
            return new Point(gridPoint.X * _gridStep, gridPoint.Y * _gridStep);
        }

        /// <summary>
        /// Converts a size given in grid cells to a size given in (pixel-)units.
        /// </summary>
        /// <param name="gridSize"></param>
        /// <returns></returns>
        [Pure]
        private Size GridToScreen(Size gridSize)
        {
            return new Size(gridSize.Width * _gridStep, gridSize.Height * _gridStep);
        }

        /// <summary>
        /// Converts a length given in grid cells to a length given in (pixel-)units.
        /// </summary>
        /// <param name="gridLength"></param>
        /// <returns></returns>
        [Pure]
        private double GridToScreen(double gridLength)
        {
            return gridLength * _gridStep;
        }

        /// <summary>
        /// Calculates the exact center point of a given rect
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        [Pure]
        private static Point GetCenterPoint(Rect rect)
        {
            var pos = rect.Location;
            var size = rect.Size;
            pos.X += size.Width / 2;
            pos.Y += size.Height / 2;
            return pos;
        }

        /// <summary>
        /// Rotates the given Size object, i.e. switches width and height.
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        [Pure]
        private static Size Rotate(Size size)
        {
            return new Size(size.Height, size.Width);
        }

        /// <summary>
        /// Generates the rect to which the given object is rendered.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [Pure]
        private Rect GetObjectScreenRect(AnnoObject obj)
        {
            return new Rect(GridToScreen(obj.Position), GridToScreen(obj.Size));
        }

        /// <summary>
        /// Gets the rect which is used for collision detection for the given object.
        /// Prevents undesired collisions which occur when using GetObjectScreenRect().
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [Pure]
        private Rect GetObjectCollisionRect(AnnoObject obj)
        {
            return new Rect(obj.Position, new Size(obj.Size.Width - 0.5, obj.Size.Height - 0.5));
        }

        /// <summary>
        /// Rotates a group of objects.
        /// </summary>
        /// <param name="l"></param>
        private void Rotate(List<AnnoObject> l)
        {
            for (int i = 0; i < l.Count; i++)
            {
                l[i].Size = Rotate(l[i].Size);
                Point p = l[i].Position;
                //Full formula left in for explanation
                //var xPrime = x * Math.Cos(angle) - y * Math.Sin(angle);
                //var yPrime = x * Math.Sin(angle) - y * Math.Cos(angle);

                //Cos 90 = 0, sin 90 = 1
                //Therefore, the below is equivalent
                var xPrime = 0 - p.Y;
                var yPrime = p.X;

                //When the building is rotated, the xPrime and yPrime values no 
                //longer represent the top left corner, they will represent the 
                //top-right corner instead. We need to account for this, by 
                //moving the xPrime position (still in grid coordinates).
                xPrime -= l[i].Size.Width;

                l[i].Position.X = xPrime;
                l[i].Position.Y = yPrime;
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
                    CurrentObjects.Add(new AnnoObject(obj));
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
                TryPlaceCurrentObject();
            }
            else if (e.LeftButton == MouseButtonState.Pressed && CurrentObjects.Count == 0)
            {
                var obj = GetObjectAt(_mousePosition);
                if (obj == null)
                {
                    // user clicked nothing: start dragging the selection rect
                    CurrentMode = MouseMode.SelectionRectStart;
                }
                else if (!IsControlPressed())
                {
                    CurrentMode = _selectedObjects.Contains(obj) ? MouseMode.DragSelectionStart : MouseMode.DragSingleStart;
                }
            }
            InvalidateVisual();
        }

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
                        _selectedObjects.Clear();
                        _selectedObjects.Add(GetObjectAt(_mouseDragStart));
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
                var dx = (int)ScreenToGrid(_mousePosition.X - _mouseDragStart.X);
                var dy = (int)ScreenToGrid(_mousePosition.Y - _mouseDragStart.Y);
                // check if the mouse has moved at least one grid cell in any direction
                if (dx != 0 || dy != 0)
                {
                    foreach (var obj in _placedObjects)
                    {
                        obj.Position.X += dx;
                        obj.Position.Y += dy;
                    }
                    // adjust the drag start to compensate the amount we already moved
                    _mouseDragStart.X += GridToScreen(dx);
                    _mouseDragStart.Y += GridToScreen(dy);
                }
            }
            else if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (CurrentObjects.Count != 0)
                {
                    // place new object
                    TryPlaceCurrentObject();
                }
                else
                {
                    // selection of multiple objects
                    switch (CurrentMode)
                    {
                        case MouseMode.SelectionRect:
                            if (IsControlPressed())
                            {
                                // remove previously selected by the selection rect
                                _selectedObjects.RemoveAll(_ => GetObjectScreenRect(_).IntersectsWith(_selectionRect));
                            }
                            else
                            {
                                _selectedObjects.Clear();
                            }
                            // adjust rect
                            _selectionRect = new Rect(_mouseDragStart, _mousePosition);
                            // select intersecting objects
                            _selectedObjects.AddRange(_placedObjects.FindAll(_ => GetObjectScreenRect(_).IntersectsWith(_selectionRect)));
                            break;
                        case MouseMode.DragSelection:
                            // move all selected objects
                            var dx = (int)ScreenToGrid(_mousePosition.X - _mouseDragStart.X);
                            var dy = (int)ScreenToGrid(_mousePosition.Y - _mouseDragStart.Y);
                            // check if the mouse has moved at least one grid cell in any direction
                            if (dx == 0 && dy == 0)
                            {
                                break;
                            }
                            var unselected = _placedObjects.FindAll(_ => !_selectedObjects.Contains(_));
                            var collisionsExist = false;
                            // temporarily move each object and check if collisions with unselected objects exist
                            foreach (var obj in _selectedObjects)
                            {
                                var originalPosition = obj.Position;
                                // move object
                                obj.Position.X += dx;
                                obj.Position.Y += dy;
                                // check for collisions
                                var collides = unselected.Find(_ => ObjectIntersectionExists(obj, _)) != null;
                                obj.Position = originalPosition;
                                if (collides)
                                {
                                    collisionsExist = true;
                                    break;
                                }
                            }
                            // if no collisions were found, permanently move all selected objects
                            if (!collisionsExist)
                            {
                                foreach (var obj in _selectedObjects)
                                {
                                    obj.Position.X += dx;
                                    obj.Position.Y += dy;
                                }
                                // adjust the drag start to compensate the amount we already moved
                                _mouseDragStart.X += GridToScreen(dx);
                                _mouseDragStart.Y += GridToScreen(dy);
                            }
                            break;
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
                        // clear selection if no key is pressed
                        if (!IsControlPressed())
                        {
                            _selectedObjects.Clear();
                        }
                        var obj = GetObjectAt(_mousePosition);
                        if (obj != null)
                        {
                            // user clicked an object: select or deselect it
                            if (_selectedObjects.Contains(obj))
                            {
                                _selectedObjects.Remove(obj);
                            }
                            else
                            {
                                _selectedObjects.Add(obj);
                            }
                        }
                        // return to standard mode, i.e. clear any drag-start modes
                        CurrentMode = MouseMode.Standard;
                        break;
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
            if (e.ChangedButton == MouseButton.Right)
            {
                switch (CurrentMode)
                {
                    case MouseMode.Standard:
                        if (CurrentObjects.Count == 0)
                        {
                            var obj = GetObjectAt(_mousePosition);
                            if (obj == null)
                            {
                                if (!IsControlPressed())
                                {
                                    // clear selection
                                    _selectedObjects.Clear();
                                }
                            }
                            else
                            {
                                // remove clicked object
                                _placedObjects.Remove(obj);
                                _selectedObjects.Remove(obj);
                            }
                        }
                        else
                        {
                            // cancel placement of object
                            CurrentObjects.Clear();
                        }
                        break;
                }
            }
            // rotate current object
            if (e.ChangedButton == MouseButton.Middle)
            {
                if (CurrentObjects.Count == 0 && _selectedObjects.Count != 0)
                {
                    CurrentObjects = CloneList(_selectedObjects);
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
                    _selectedObjects.ForEach(_ => _placedObjects.Remove(_));
                    _selectedObjects.Clear();
                    break;
                case Key.C:
                    if (_selectedObjects.Count != 0)
                    {
                        ObjectClipboard = CloneList(_selectedObjects);
                    }
                    break;
                case Key.V:
                    if (ObjectClipboard.Count != 0)
                    {
                        CurrentObjects = CloneList(ObjectClipboard);
                        MoveCurrentObjectsToMouse();
                    }
                    break;
                case Key.R:
                    if (CurrentObjects.Count == 1)
                    {
                        CurrentObjects[0].Size = Rotate(CurrentObjects[0].Size);
                    }
                    else if (CurrentObjects.Count > 1)
                    {
                        Rotate(CurrentObjects);
                    }
                    else
                    {
                        //Count == 0;
                        //Rotate from selected objects
                        CurrentObjects = CloneList(_selectedObjects);
                        Rotate(CurrentObjects);
                    }
                    break;

            }
            InvalidateVisual();
        }

        /// <summary>
        /// Checks whether the user is pressing keys to signal that he wants to select multiple objects
        /// </summary>
        /// <returns></returns>
        private static bool IsControlPressed()
        {
            return Keyboard.Modifiers.HasFlag(ModifierKeys.Control) || Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);
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
        private bool ObjectIntersectionExists(AnnoObject a, AnnoObject b)
        {
            return GetObjectCollisionRect(a).IntersectsWith(GetObjectCollisionRect(b));
        }

        // <summary>
        /// Checks if there is a collision between a list of AnnoObjects a and object b.
        /// </summary>
        /// <param name="a">List of objects</param>
        /// <param name="b">second object</param>
        /// <returns>true if there is a collision, otherwise false</returns>
        private bool ObjectIntersectionExists(List<AnnoObject> a, AnnoObject b)
        {
            return a.Exists(_ => GetObjectCollisionRect(_).IntersectsWith(GetObjectCollisionRect(b)));
        }

        /// <summary>
        /// Tries to place the current object on the grid.
        /// Fails if there are any collisions.
        /// </summary>
        /// <returns>true if placement succeeded, otherwise false</returns>
        private bool TryPlaceCurrentObject()
        {
            if (CurrentObjects.Count != 0 && !_placedObjects.Exists(_ => ObjectIntersectionExists(CurrentObjects, _)))
            {
                _placedObjects.AddRange(CloneList(CurrentObjects));
                // sort the objects because borderless objects should be drawn first
                _placedObjects.Sort((a, b) => b.Borderless.CompareTo(a.Borderless));
                return true;
            }
            return false;
        }

        /// <summary>
        /// Retrieves the object at the given position given in screen coordinates.
        /// </summary>
        /// <param name="position">position given in screen coordinates</param>
        /// <returns>object at the position, if there is no object null</returns>
        private AnnoObject GetObjectAt(Point position)
        {
            return _placedObjects.FindLast(_ => GetObjectScreenRect(_).Contains(position));
        }

        #endregion

        #region API

        /// <summary>
        /// Sets the current object, i.e. the object which the user can place.
        /// </summary>
        /// <param name="obj">object to apply</param>
        public void SetCurrentObject(AnnoObject obj)
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
            if (_placedObjects.Count == 0)
            {
                return;
            }
            var dx = _placedObjects.Min(_ => _.Position.X) - border;
            var dy = _placedObjects.Min(_ => _.Position.Y) - border;
            _placedObjects.ForEach(_ => _.Position.X -= dx);
            _placedObjects.ForEach(_ => _.Position.Y -= dy);
            InvalidateVisual();
        }
        #endregion

        #region New/Save/Load/Export methods

        /// <summary>
        /// Removes all objects from the grid.
        /// </summary>
        public void NewFile()
        {
            _placedObjects.Clear();
            _selectedObjects.Clear();
            LoadedFile = "";
            InvalidateVisual();
        }

        /// <summary>
        /// Writes layout to file.
        /// </summary>
        private void SaveFile()
        {
            try
            {
                Normalize(1);
                DataIO.SaveLayout(_placedObjects, LoadedFile);
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
        public void OpenFile(string filename)
        {
            try
            {
                var layout = DataIO.LoadLayout(filename);
                if (layout != null)
                {
                    _selectedObjects.Clear();
                    _placedObjects = layout;
                    LoadedFile = filename;
                    Normalize(1);
                }
            }
            catch (Exception e)
            {
                IOErrorMessageBox(e);
            }
        }

        /// <summary>
        /// Renders the current layout to file.
        /// </summary>
        /// <param name="exportZoom">indicates whether the current zoom level should be applied, if false the default zoom is used</param>
        /// <param name="exportSelection">indicates whether selection and influence highlights should be rendered</param>
        public void ExportImage(bool exportZoom, bool exportSelection)
        {
            var dialog = new SaveFileDialog
            {
                DefaultExt = Constants.ExportedImageExtension,
                Filter = Constants.ExportDialogFilter
            };
            if (!string.IsNullOrEmpty(LoadedFile))
            {
                // default the filename to the same name as the saved layout
                dialog.FileName = Path.GetFileNameWithoutExtension(LoadedFile);
            }
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    RenderToFile(dialog.FileName, 1, exportZoom, exportSelection);
                }
                catch (Exception e)
                {
                    IOErrorMessageBox(e);
                }
            }
        }

        /// <summary>
        /// Asynchronously renders the current layout to file.
        /// </summary>
        /// <param name="filename">filename of the output image</param>
        /// <param name="border">normalization value used prior to exporting</param>
        /// <param name="exportZoom">indicates whether the current zoom level should be applied, if false the default zoom is used</param>
        /// <param name="exportSelection">indicates whether selection and influence highlights should be rendered</param>
        private void RenderToFile(string filename, int border, bool exportZoom, bool exportSelection)
        {
            // copy all objects
            var allObjects = _placedObjects.Select(_ => new AnnoObject(_)).ToList();
            // copy selected objects
            // note: should be references to the correct copied objects from allObjects
            var selectedObjects = _selectedObjects.Select(_ => new AnnoObject(_)).ToList();
            System.Diagnostics.Debug.WriteLine("UI thread: {0}", Thread.CurrentThread.ManagedThreadId);
            ThreadStart renderThread = delegate
            {
                System.Diagnostics.Debug.WriteLine("Render thread: {0}", Thread.CurrentThread.ManagedThreadId);
                // initialize output canvas
                var target = new AnnoCanvas
                {
                    _placedObjects = allObjects,
                    RenderGrid = RenderGrid,
                    RenderIcon = RenderIcon,
                    RenderLabel = RenderLabel,
                    RenderStats = RenderStats
                };
                // normalize layout
                target.Normalize(border);
                // set zoom level
                if (exportZoom)
                {
                    target.GridSize = GridSize;
                }
                // set selection
                if (exportSelection)
                {
                    target._selectedObjects.AddRange(selectedObjects);
                }
                // calculate output size
                var width = target.GridToScreen(_placedObjects.Max(_ => _.Position.X + _.Size.Width) + border) + 1;
                var height = target.GridToScreen(_placedObjects.Max(_ => _.Position.Y + _.Size.Height) + border) + 1;
                if (RenderStats)
                {
                    width += Constants.StatisticsMargin - 0.5;
                }
                target.Width = width;
                target.Height = height;
                // apply size
                var outputSize = new Size(width, height);
                target.Measure(outputSize);
                target.Arrange(new Rect(outputSize));
                // render canvas to file
                DataIO.RenderToFile(target, filename);
            };
            var thread = new Thread(renderThread);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
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
            var canvas = sender as AnnoCanvas;
            if (canvas != null && CommandExecuteMappings.ContainsKey(e.Command))
            {
                CommandExecuteMappings[e.Command].Invoke(canvas);
                e.Handled = true;
            }
        }

        #endregion
    }
}
