using System.Runtime.Serialization;

namespace AnnoDesigner.Presets
{
    [DataContract]
    public class IconNameMap
    {
        [DataMember]
        public string IconFilename;

        [DataMember]
        public SerializableDictionary<string> Localizations;
    }
}