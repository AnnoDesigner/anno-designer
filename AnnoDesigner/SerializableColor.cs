using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Windows.Media;

namespace AnnoDesigner
{
    /// <summary>
    /// Provides a serializable representation of the System.Windows.Media.Color class
    /// </summary>
    [Serializable]
    [DataContract]
    [DebuggerDisplay("{" + nameof(A) + "},{" + nameof(R) + "},{" + nameof(G) + "},{" + nameof(B) + "}")]
    public struct SerializableColor
    {
        [DataMember(Order = 0)]
        public byte A;

        [DataMember(Order = 1)]
        public byte R;

        [DataMember(Order = 2)]
        public byte G;

        [DataMember(Order = 3)]
        public byte B;

        //Needed for Databinding. Implicit converter is not called
        public Color MediaColor { get { return this; } }

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

        public override string ToString()
        {
            return ToArgbString();
        }

        public string ToArgbString()
        {
            return $"{A}, {R}, {G}, {B}";
        }

        public string ToHexString()
        {
            return $"#{A:X2}{R:X2}{G:X2}{B:X2}";
        }
    }
}