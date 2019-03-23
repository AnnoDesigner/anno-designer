using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AnnoDesigner.Presets
{
    [DataContract]
    public class ColorPresets
    {
        [DataMember]
        public List<ColorScheme> ColorSchemes;
    }
}