using System.Diagnostics;
using System.Runtime.Serialization;

namespace AnnoDesigner.Presets
{
    [DataContract]
    [DebuggerDisplay("{IconFilename}")]
    public class IconNameMap
    {
        [DataMember(Order = 0)]
        public string IconFilename { get; set; }

        [DataMember(Order = 1)]
        public SerializableDictionary<string> Localizations { get; set; }
    }
}