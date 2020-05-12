using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using AnnoDesigner.Core.Models;

namespace AnnoDesigner.model
{
    /// <summary>
    /// This class is mainly for performance and a wrapper for <see cref="AnnoObject"/>.
    /// It caches all kinds of Visuals (e.g. Brushes, Pens) and calculations (e.g. CollisionRect).
    /// </summary>
    public class LayoutObject
    {
        private AnnoObject _wrappedAnnoObject;
        private readonly ICoordinateHelper _coordinateHelper;
        private readonly IBrushHelper _brushHelper;

        private Color? _transparentColor;
        private SolidColorBrush _transparentBrush;
        private Color? _renderColor;
        private SolidColorBrush _renderBrush;
        private Pen _borderlessPen;
        private int _gridSizeScreenRect;
        private Point _position;
        private Rect _screenRect;
        private Rect _collisionRect;
        private Size _collisionSize;
        private string _iconNameWithoutExtension;
        private string _identifier;
        private Size _size;
        private FormattedText _formattedText;
        private CultureInfo _usedTextCulture;
        private Typeface _usedTextTypeFace;
        private Rect _iconRect;
        private int _gridSizeIconRect;
        private Rect _lastScreenRectForIcon;
        private Point _screenRectCenterPoint;
        private EllipseGeometry _influenceCircle;
        private double _influenceCircleRadius;
        private Rect _lastScreenRectForCenterPoint;
        private double _screenRadius;
        private int _lastGridSizeForScreenRadius;
        private SerializableColor _color;

        /// <summary>
        /// Creates a new instance of a wrapper for <see cref="AnnoObject"/>.
        /// </summary>
        /// <param name="annoObjectToWrap">The <see cref="AnnoObject"/> to wrap. Reference will be kept.</param>
        /// <param name="coordinateHelperToUse">The <see cref="ICoordinateHelper"/> to use in calculations.</param>
        /// <param name="brushHelperToUse">The <see cref="IBrushHelper"/> used as a cache.</param>
        public LayoutObject(AnnoObject annoObjectToWrap, ICoordinateHelper coordinateHelperToUse, IBrushHelper brushHelperToUse)
        {
            WrappedAnnoObject = annoObjectToWrap;
            _coordinateHelper = coordinateHelperToUse;
            _brushHelper = brushHelperToUse;
        }

        public AnnoObject WrappedAnnoObject
        {
            get { return _wrappedAnnoObject; }
            private set { _wrappedAnnoObject = value; }
        }

        public Color TransparentColor
        {
            get
            {
                if (_transparentColor == null)
                {
                    var tmp = WrappedAnnoObject.Color.MediaColor;
                    tmp.A = 128;

                    _transparentColor = tmp;
                }

                return _transparentColor.Value;
            }
        }

        public SolidColorBrush TransparentBrush
        {
            get
            {
                if (_transparentBrush == null)
                {
                    _transparentBrush = _brushHelper.GetSolidBrush(TransparentColor);
                }

                return _transparentBrush;
            }
        }

        public Color RenderColor
        {
            get
            {
                if (_renderColor == null)
                {
                    _renderColor = WrappedAnnoObject.Color.MediaColor;
                }

                return _renderColor.Value;
            }
        }

        public SolidColorBrush RenderBrush
        {
            get
            {
                if (_renderBrush == null)
                {
                    _renderBrush = _brushHelper.GetSolidBrush(RenderColor);
                }

                return _renderBrush;
            }
        }

        public Pen GetBorderlessPen(Brush brush, double thickness)
        {
            if (_borderlessPen == null || _borderlessPen.Thickness != thickness || _borderlessPen.Brush != brush)
            {
                _borderlessPen = new Pen(brush, thickness);
                if (_borderlessPen.CanFreeze)
                {
                    _borderlessPen.Freeze();
                }
            }

            return _borderlessPen;
        }

        public Point Position
        {
            get
            {
                if (_position == default)
                {
                    _position = WrappedAnnoObject.Position;
                }

                return _position;
            }
            set
            {
                WrappedAnnoObject.Position = value;
                _position = value;
                _screenRect = default;
                _collisionRect = default;
                _influenceCircle = null;
            }
        }

        /// <summary>
        /// Generates the rect to which the given object is rendered.
        /// </summary>
        public Rect CalculateScreenRect(int gridSize)
        {
            if (_gridSizeScreenRect != gridSize)
            {
                _gridSizeScreenRect = gridSize;
                _screenRect = default;
            }

            return ScreenRect;
        }

        private Rect ScreenRect
        {
            get
            {
                if (_screenRect == default)
                {
                    _screenRect = new Rect(_coordinateHelper.GridToScreen(Position, _gridSizeScreenRect), _coordinateHelper.GridToScreen(Size, _gridSizeScreenRect));
                }

                return _screenRect;
            }
        }

        /// <summary>
        /// Gets the rect which is used for collision detection for the given object.
        /// Prevents undesired collisions which occur when using GetObjectScreenRect().
        /// </summary>        
        public Rect CollisionRect
        {
            get
            {
                if (_collisionRect == default)
                {
                    _collisionRect = new Rect(Position, CollisionSize);
                }

                return _collisionRect;
            }
        }

        private Size CollisionSize
        {
            get
            {
                if (_collisionSize == default)
                {
                    _collisionSize = new Size(Size.Width - 0.5, Size.Height - 0.5);
                }

                return _collisionSize;
            }
        }

        public string IconNameWithoutExtension
        {
            get
            {
                if (_iconNameWithoutExtension == null)
                {
                    _iconNameWithoutExtension = Path.GetFileNameWithoutExtension(WrappedAnnoObject.Icon);
                }

                return _iconNameWithoutExtension;
            }
        }

        public string Identifier
        {
            get
            {
                if (_identifier == null)
                {
                    _identifier = WrappedAnnoObject.Identifier;
                }

                return _identifier;
            }
            set
            {
                WrappedAnnoObject.Identifier = value;
                _identifier = value;
            }
        }

        public Size Size
        {
            get
            {
                if (_size == default)
                {
                    _size = WrappedAnnoObject.Size;
                }

                return _size;
            }
            set
            {
                WrappedAnnoObject.Size = value;
                _size = value;

                _collisionSize = default;
                _screenRect = default;
                _iconRect = default;
            }
        }

        public FormattedText GetFormattedText(TextAlignment textAlignment, CultureInfo culture, Typeface typeface, double pixelsPerDip, double width, double height)
        {
            if (_formattedText == null ||
                _formattedText.TextAlignment != textAlignment ||
                _usedTextCulture != culture ||
                _usedTextTypeFace != typeface ||
                _formattedText.PixelsPerDip != pixelsPerDip ||
                _formattedText.MaxTextWidth != width ||
                _formattedText.MaxTextHeight != height)
            {
                _formattedText = new FormattedText(WrappedAnnoObject.Label, culture, FlowDirection.LeftToRight,
                    typeface, 12, Brushes.Black,
                    null, TextFormattingMode.Display, pixelsPerDip)
                {
                    MaxTextWidth = width,
                    MaxTextHeight = height,
                    TextAlignment = textAlignment
                };

                _usedTextCulture = culture;
                _usedTextTypeFace = typeface;
            }

            return _formattedText;
        }

        public Rect GetIconRect(int gridSize)
        {
            var objRect = CalculateScreenRect(gridSize);

            if (_iconRect == default || _gridSizeIconRect != gridSize || _lastScreenRectForIcon != objRect)
            {
                // draw icon 2x2 grid cells large
                var minSize = Math.Min(Size.Width, Size.Height);
                //minSize = minSize == 1 ? minSize : Math.Floor(NthRoot(minSize, Constants.IconSizeFactor) + 1);
                var iconSize = _coordinateHelper.GridToScreen(new Size(minSize, minSize), gridSize);
                iconSize = minSize == 1 ? iconSize : new Size(NthRoot(iconSize.Width, Constants.IconSizeFactor), NthRoot(iconSize.Height, Constants.IconSizeFactor));

                // center icon within the object
                var iconPos = objRect.TopLeft;
                iconPos.X += (objRect.Width / 2) - (iconSize.Width / 2);
                iconPos.Y += (objRect.Height / 2) - (iconSize.Height / 2);

                _iconRect = new Rect(iconPos, iconSize);

                _gridSizeIconRect = gridSize;
                _lastScreenRectForIcon = objRect;
            }

            return _iconRect;

        }

        //I was really just checking to see if there was a built in function, but this works
        //https://stackoverflow.com/questions/18657508/c-sharp-find-nth-root
        private static double NthRoot(double A, double N)
        {
            return Math.Pow(A, 1.0 / N);
        }

        public Point GetScreenRectCenterPoint(int gridSize)
        {
            var objRect = CalculateScreenRect(gridSize);

            if (_screenRectCenterPoint == default || _lastScreenRectForCenterPoint != objRect)
            {
                _screenRectCenterPoint = _coordinateHelper.GetCenterPoint(objRect);

                _lastScreenRectForCenterPoint = objRect;
            }

            return _screenRectCenterPoint;
        }

        public EllipseGeometry GetInfluenceCircle(int gridSize, double radius)
        {
            if (_influenceCircle == null || _gridSizeScreenRect != gridSize || _influenceCircleRadius != radius)
            {
                _influenceCircle = new EllipseGeometry(GetScreenRectCenterPoint(gridSize), radius, radius);
                if (_influenceCircle.CanFreeze)
                {
                    _influenceCircle.Freeze();
                }

                _influenceCircleRadius = radius;
            }

            return _influenceCircle;
        }

        public double GetScreenRadius(int gridSize)
        {
            if (_screenRadius == default || _lastGridSizeForScreenRadius != gridSize)
            {
                _screenRadius = _coordinateHelper.GridToScreen(WrappedAnnoObject.Radius, gridSize);

                _lastGridSizeForScreenRadius = gridSize;
            }

            return _screenRadius;
        }

        public SerializableColor Color
        {
            get
            {
                if (_color == default)
                {
                    _color = WrappedAnnoObject.Color;
                }

                return _color;
            }
            set
            {
                WrappedAnnoObject.Color = value;
                _color = value;

                _transparentColor = null;
                _transparentBrush = null;
                _renderColor = null;
                _renderBrush = null;
            }
        }
    }
}
