using System.Diagnostics;
using System.Runtime.Serialization;
using AnnoDesigner.Core.Models;

namespace AnnoDesigner.Core.Presets.Models
{
    [DataContract]
    [DebuggerDisplay("{" + nameof(IconFilename) + "}")]
    public class IconNameMap
    {
        [DataMember(Order = 0)]
        public string IconFilename { get; set; }

        [DataMember(Order = 1)]
        public SerializableDictionary<string> Localizations { get; set; }
    }
}