using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AnnoDesigner.Presets
{
    [DataContract]
    public class ColorScheme
    {
        [DataMember]
        public string Name;

        [DataMember]
        public List<ColorInfo> ColorInfos;
    }
}
