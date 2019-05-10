using System;
using System.Runtime.Serialization;

namespace AnnoDesigner.Presets
{
    [DataContract]
    public class PredefinedColor
    {
        [DataMember(Order = 0, Name = "TargetTemplate")]
        public string TargetTemplate { get; set; }

        [DataMember(Order = 1, Name = "Color")]
        public SerializableColor Color { get; set; }
    }
}