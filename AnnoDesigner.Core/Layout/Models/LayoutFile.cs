using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using AnnoDesigner.Core.Models;

namespace AnnoDesigner.Core.Layout.Models
{
    /// <summary>
    /// Container with version information and all objects.
    /// </summary>
    [DataContract]
    public class LayoutFile : LayoutFileVersionContainer
    {
        [DataMember(Order = 99)]
        public List<AnnoObject> Objects { get; set; }

        public LayoutFile() { }

        public LayoutFile(IEnumerable<AnnoObject> objects)
        {
            FileVersion = CoreConstants.LayoutFileVersion;
            Objects = objects.ToList();
        }

        public LayoutFile(LayoutFile copy)
        {
            FileVersion = copy.FileVersion;
            LayoutVersion = (Version)copy.LayoutVersion.Clone();
            Modified = copy.Modified;
            Objects = copy.Objects.Select(x => new AnnoObject(x)).ToList();
        }
    }
}