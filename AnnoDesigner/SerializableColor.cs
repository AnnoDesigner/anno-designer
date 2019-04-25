using System;
using System.Diagnostics;
using System.Windows.Media;

namespace AnnoDesigner
{
    /// <summary>
    /// Provides a serializable representation of the System.Windows.Media.Color class
    /// </summary>
    [Serializable]
    [DebuggerDisplay("{A},{R},{G},{B}")]
    public struct SerializableColor
    {
        public byte A;
        public byte R;
        public byte G;
        public byte B;

        public SerializableColor(byte a, byte r, byte g, byte b)
        {
            A = a;
            R = r;
            G = g;
            B = b;
        }

        public SerializableColor(Color color)
            : this(color.A, color.R, color.G, color.B)
        {
        }

        public static implicit operator SerializableColor(Color color)
        {
            return new SerializableColor(color);
        }

        public static implicit operator Color(SerializableColor color)
        {
            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }
    }
}