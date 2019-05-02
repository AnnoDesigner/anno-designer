using System.Runtime.Serialization;

namespace AnnoDesigner.Presets
{
    [DataContract]
    public class IconNameMap
    {
        [DataMember]
        public string IconFilename { get; set; }

        [DataMember]
        public SerializableDictionary<string> Localizations { get; set; }
    }
}