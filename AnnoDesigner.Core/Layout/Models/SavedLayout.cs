using System.Collections.Generic;
using System.Runtime.Serialization;
using AnnoDesigner.Core.Models;

namespace AnnoDesigner.Core.Layout.Models
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
            FileVersion = CoreConstants.LayoutFileVersion;
            Objects = objects;
        }
    }
}