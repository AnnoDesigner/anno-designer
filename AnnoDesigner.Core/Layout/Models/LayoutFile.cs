﻿using System.Collections.Generic;
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

        public LayoutFile() : this(null) { }

        public LayoutFile(List<AnnoObject> objects)
        {
            FileVersion = CoreConstants.LayoutFileVersion;
            Objects = objects;
        }
    }
}