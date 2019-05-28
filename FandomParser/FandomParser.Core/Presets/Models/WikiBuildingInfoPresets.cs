using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

namespace FandomParser.Core.Presets.Models
{
    [DebuggerDisplay("{" + nameof(Version) + "}")]
    [DataContract]
    public class WikiBuildingInfoPresets
    {
        public WikiBuildingInfoPresets()
        {
            Infos = new List<WikiBuildingInfo>();
        }

        [DataMember(Order = 0)]
        public Version Version { get; set; }

        [DataMember(Order = 1)]
        public List<WikiBuildingInfo> Infos { get; set; }
    }
}
