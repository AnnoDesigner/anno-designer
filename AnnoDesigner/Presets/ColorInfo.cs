using System;
using System.Runtime.Serialization;

namespace AnnoDesigner.Presets
{
    [DataContract]
    public class ColorInfo
    {
        [DataMember]
        private string Target
        {
            get { return ColorTarget.ToString(); }
            set { ColorTarget = (ColorTarget)Enum.Parse(typeof(ColorTarget), value); }
        }

        public ColorTarget ColorTarget { get; set; }

        [DataMember]
        public SerializableColor Color { get; set; }
    }
}