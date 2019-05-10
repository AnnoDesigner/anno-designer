using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AnnoDesigner.Presets
{
    [DataContract]
    public class ColorPresets
    {
        [DataMember(Order = 0)]
        public string Version { get; set; }

        [DataMember(Order = 1, Name = "AvailableSchemes")]
        public List<ColorScheme> AvailableSchemes { get; set; }
    }
}