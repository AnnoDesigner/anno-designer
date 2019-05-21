using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FandomParser
{

    [DebuggerDisplay("{" + nameof(Version) + "}")]
    [DataContract]
    public class WikiBuildingInfoList
    {
        public WikiBuildingInfoList()
        {
            Infos = new List<WikiBuildingInfo>();
        }

        [DataMember(Order = 0)]
        public Version Version { get; set; }

        [DataMember(Order = 1)]
        public List<WikiBuildingInfo> Infos { get; set; }
    }
}
