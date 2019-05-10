using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AnnoDesigner.Presets
{
    [DataContract]
    public class ColorScheme
    {
        [DataMember(Order = 0, Name = "Name")]
        public string Name { get; set; }

        [DataMember(Order = 1, Name = "Colors")]
        public List<PredefinedColor> Colors { get; set; }
    }
}
