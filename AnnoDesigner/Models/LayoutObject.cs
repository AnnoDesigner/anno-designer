using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Media;
using AnnoDesigner.Core.Helper;
using AnnoDesigner.Core.Models;

namespace AnnoDesigner.Models
{
    /// <summary>
    /// This class is mainly for performance and a wrapper for <see cref="AnnoObject"/>.
    /// It caches all kinds of Visuals (e.g. Brushes, Pens) and calculations (e.g. CollisionRect).
    /// </summary>
    [DebuggerDisplay("{" + nameof(Identifier) + "}")]
    public class LayoutObject : IBounded
    {
        private AnnoObject _wrappedAnnoObject;
        private readonly ICoordinateHelper _coordinateHelper;
        private readonly IBrushCache _brushCache;
        private readonly IPenCache _penCache;

        private Color? _transparentColor;
        private SolidColorBrush _transparentBrush;
        private Color? _renderColor;
        private SolidColorBrush _renderBrush;
        private Color? _blockedAreaColor;
        private SolidColorBrush _blockedAreaBrush;
        private Pen _borderlessPen;
        private int _gridSizeScreenRect;
        private Point _position;
        private Rect _screenRect;
        private Rect? _blockedAreaScreenRect;
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
        private int _gridSizeScreenRectForCenterPoint;
        private double _screenRadius;
        private int _lastGridSizeForScreenRadius;
        private SerializableColor _color;
        private Rect _gridRect;
        private Rect _gridInfluenceRadiusRect;
        private Rect _gridInfluenceRangeRect;

        /// <summary>
        /// Creates a new instance of a wrapper for <see cref="AnnoObject"/>.
        /// </summary>
        /// <param name="annoObjectToWrap">The <see cref="AnnoObject"/> to wrap. Reference will be kept.</param>
        /// <param name="coordinateHelperToUse">The <see cref="ICoordinateHelper"/> to use in calculations.</param>
        /// <param name="brushCacheToUse">The <see cref="IBrushCache"/> used as a cache.</param>
        public LayoutObject(AnnoObject annoObjectToWrap, ICoordinateHelper coordinateHelperToUse, IBrushCache brushCacheToUse, IPenCache penCacheToUse)
        {
            WrappedAnnoObject = annoObjectToWrap;
            _coordinateHelper = coordinateHelperToUse;
            _brushCache = brushCacheToUse;
            _penCache = penCacheToUse;
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
                    _transparentBrush = _brushCache.GetSolidBrush(TransparentColor);
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
                    _renderBrush = _brushCache.GetSolidBrush(RenderColor);
                }

                return _renderBrush;
            }
        }

        public Color BlockedAreaColor
        {
            get
            {
                if (_blockedAreaColor == null)
                {
                    var tmp = WrappedAnnoObject.Color.MediaColor;
                    tmp.A = 60;

                    _blockedAreaColor = tmp;
                }

                return _blockedAreaColor.Value;
            }
        }

        public SolidColorBrush BlockedAreaBrush
        {
            get
            {
                if (_blockedAreaBrush == null)
                {
                    _blockedAreaBrush = _brushCache.GetSolidBrush(BlockedAreaColor);
                }

                return _blockedAreaBrush;
            }
        }

        public Pen GetBorderlessPen(Brush brush, double thickness)
        {
            if (_borderlessPen == null || _borderlessPen.Thickness != thickness || _borderlessPen.Brush != brush)
            {
                _borderlessPen = _penCache.GetPen(brush, thickness);
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
                _iconRect = default;
                _screenRectCenterPoint = default;
                _lastScreenRectForIcon = default;
                _lastScreenRectForCenterPoint = default;
                _gridRect = default;
                _gridInfluenceRadiusRect = default;
                _gridInfluenceRangeRect = default;
            }
        }

        public double BlockedAreaLength => WrappedAnnoObject.BlockedAreaLength;

        public double BlockedAreaWidth
        {
            get
            {
                if (WrappedAnnoObject.BlockedAreaWidth > 0)
                {
                    return WrappedAnnoObject.BlockedAreaWidth;
                }
                switch (Direction)
                {
                    case GridDirection.Up:
                    case GridDirection.Down: return Size.Width - 0.5;
                    case GridDirection.Right:
                    case GridDirection.Left: return Size.Height - 0.5;
                }
                return 0;
            }
        }

        public GridDirection Direction
        {
            get => WrappedAnnoObject.Direction;
            set => WrappedAnnoObject.Direction = value;
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
                    _blockedAreaScreenRect = null;
                }

                return _screenRect;
            }
        }

        public Rect? CalculateBlockedScreenRect(int gridSize)
        {
            if (_gridSizeScreenRect != gridSize)
            {
                _gridSizeScreenRect = gridSize;
                _blockedAreaScreenRect = null;
            }

            return BlockedAreaScreenRect;
        }

        private Rect? BlockedAreaScreenRect
        {
            get
            {
                if (_blockedAreaScreenRect == null)
                {
                    if (BlockedAreaLength > 0)
                    {
                        var blockedAreaScreenWidth = _coordinateHelper.GridToScreen(BlockedAreaWidth, _gridSizeScreenRect);
                        var blockedAreaScreenLength = _coordinateHelper.GridToScreen(BlockedAreaLength, _gridSizeScreenRect);

                        switch (Direction)
                        {
                            case GridDirection.Up:
                                return _blockedAreaScreenRect = new Rect(
                                    ScreenRect.Left + (ScreenRect.Width - blockedAreaScreenWidth) / 2,
                                    ScreenRect.Top - blockedAreaScreenLength,
                                    blockedAreaScreenWidth,
                                    blockedAreaScreenLength);
                            case GridDirection.Right:
                                return _blockedAreaScreenRect = new Rect(
                                    ScreenRect.Right,
                                    ScreenRect.Top + (ScreenRect.Height - blockedAreaScreenWidth) / 2,
                                    blockedAreaScreenLength,
                                    blockedAreaScreenWidth);
                            case GridDirection.Down:
                                return _blockedAreaScreenRect = new Rect(ScreenRect.Left + (ScreenRect.Width - blockedAreaScreenWidth) / 2,
                                    ScreenRect.Bottom,
                                    blockedAreaScreenWidth,
                                    blockedAreaScreenLength);
                            case GridDirection.Left:
                                return _blockedAreaScreenRect = new Rect(ScreenRect.TopLeft.X - blockedAreaScreenLength,
                                    ScreenRect.TopLeft.Y + (ScreenRect.Height - blockedAreaScreenWidth) / 2,
                                    blockedAreaScreenLength,
                                    blockedAreaScreenWidth);
                        }
                    }
                }

                return _blockedAreaScreenRect;
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

        public IconImage Icon { get; set; }

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
                _gridRect = default;
                _gridInfluenceRadiusRect = default;
                _gridInfluenceRangeRect = default;
            }
        }

        public Rect Bounds
        {
            get => new Rect(Position, Size);
            set
            {
                Position = value.TopLeft;
                Size = value.Size;
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
            if (_iconRect == default || _gridSizeIconRect != gridSize)
            {
                var objRect = CalculateScreenRect(gridSize);
                if (_lastScreenRectForIcon != objRect)
                {
                    // draw icon 2x2 grid cells large
                    var minSize = Math.Min(Size.Width, Size.Height);
                    //minSize = minSize == 1 ? minSize : Math.Floor(NthRoot(minSize, Constants.IconSizeFactor) + 1);
                    var iconSize = _coordinateHelper.GridToScreen(new Size(minSize, minSize), gridSize);
                    iconSize = minSize == 1 ? iconSize : new Size(MathHelper.NthRoot(iconSize.Width, Constants.IconSizeFactor), MathHelper.NthRoot(iconSize.Height, Constants.IconSizeFactor));

                    // center icon within the object
                    var iconPos = objRect.TopLeft;
                    iconPos.X += (objRect.Width / 2) - (iconSize.Width / 2);
                    iconPos.Y += (objRect.Height / 2) - (iconSize.Height / 2);

                    _iconRect = new Rect(iconPos, iconSize);

                    _gridSizeIconRect = gridSize;
                    _lastScreenRectForIcon = objRect;
                }
            }

            return _iconRect;
        }


        public Point GetScreenRectCenterPoint(int gridSize)
        {
            if (_screenRectCenterPoint == default || _gridSizeScreenRectForCenterPoint != gridSize)
            {
                var objRect = CalculateScreenRect(gridSize);
                if (_lastScreenRectForCenterPoint != objRect)
                {
                    _screenRectCenterPoint = _coordinateHelper.GetCenterPoint(objRect);

                    _gridSizeScreenRectForCenterPoint = gridSize;
                    _lastScreenRectForCenterPoint = objRect;
                }
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
                // Buildings of an odd-numbered size (3x3/3x5 etc) draw their circular influence range with an additional +0.5,
                // this is not correct and needs to be adjusted to produce the correct radius for those buildings.
                // See https://github.com/AnnoDesigner/anno-designer/issues/299 for an explanation of the issue.

                // check if Object Width and Height are odd numbers or not, if both are, adjust the circle size with -0.1
                if ((WrappedAnnoObject.Size.Width % 2 != 0) && (WrappedAnnoObject.Size.Height % 2 != 0))
                {
                    _screenRadius = _coordinateHelper.GridToScreen(WrappedAnnoObject.Radius - 0.1, gridSize);
                }
                else
                {
                    _screenRadius = _coordinateHelper.GridToScreen(WrappedAnnoObject.Radius, gridSize);
                }
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

        public Rect GridRect
        {
            get
            {
                if (_gridRect == default)
                {
                    _gridRect = new Rect(Position, Size);
                }
                return _gridRect;
            }
        }

        public Rect GridInfluenceRadiusRect
        {
            get
            {
                if (_gridInfluenceRadiusRect == default)
                {
                    var centerPoint = _coordinateHelper.GetCenterPoint(GridRect);
                    _gridInfluenceRadiusRect = new Rect(centerPoint.X - WrappedAnnoObject.Radius, centerPoint.Y - WrappedAnnoObject.Radius, WrappedAnnoObject.Radius * 2, WrappedAnnoObject.Radius * 2);
                }
                return _gridInfluenceRadiusRect;
            }
        }

        public Rect GridInfluenceRangeRect
        {
            get
            {
                if (_gridInfluenceRangeRect == default)
                {
                    if (WrappedAnnoObject.InfluenceRange <= 0)
                    {
                        _gridInfluenceRangeRect = new Rect(Position, default(Size));
                    }
                    else
                    {
                        //influence range is computed from the edge of the building, not the center
                        _gridInfluenceRangeRect = new Rect(Position.X - WrappedAnnoObject.InfluenceRange, Position.Y - WrappedAnnoObject.InfluenceRange, WrappedAnnoObject.InfluenceRange + Size.Width, WrappedAnnoObject.InfluenceRange + Size.Height);
                    }
                }
                return _gridInfluenceRangeRect;
            }
        }
    }
}
