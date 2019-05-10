using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AnnoDesigner.Presets
{
    [DataContract]
    public class PredefinedColor
    {
        public PredefinedColor()
        {
            TargetTemplate = string.Empty;
            TargetIdentifiers = new List<string>();
            Color = new SerializableColor();
        }

        [DataMember(Order = 0, Name = "TargetTemplate")]
        public string TargetTemplate { get; set; }

        [DataMember(Order = 1, Name = "TargetIdentifiers")]
        public List<string> TargetIdentifiers { get; set; }

        [DataMember(Order = 2, Name = "Color")]
        public SerializableColor Color { get; set; }
    }
}