using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AnnoDesigner.Presets
{
    [DataContract]
    public class ColorScheme
    {
        public ColorScheme()
        {
            Name = string.Empty;
            Colors = new List<PredefinedColor>();
        }

        [DataMember(Order = 0, Name = "Name")]
        public string Name { get; set; }

        [DataMember(Order = 1, Name = "Colors")]
        public List<PredefinedColor> Colors { get; set; }
    }
}
