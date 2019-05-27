using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace AnnoDesigner.Core.Presets.Models
{
    /// <summary>
    /// Notes:
    /// some radii are curiously missing, e.g. coffee plantation
    /// </summary>
    [DataContract]
    public class BuildingPresets
    {
        [DataMember(Order = 0)]
        public string Version { get; set; }

        [DataMember(Order = 1)]
        public List<BuildingInfo> Buildings { get; set; }
    }
}