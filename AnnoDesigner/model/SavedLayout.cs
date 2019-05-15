using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AnnoDesigner.model
{
    /// <summary>
    /// Container with file version and all objects
    /// </summary>
    [DataContract]
    public class SavedLayout : LayoutVersionContainer
    {
        [DataMember(Order = 1)]
        public List<AnnoObject> Objects { get; set; }

        public SavedLayout(List<AnnoObject> objects)
        {
            FileVersion = Constants.FileVersion;
            Objects = objects;
        }
    }
}