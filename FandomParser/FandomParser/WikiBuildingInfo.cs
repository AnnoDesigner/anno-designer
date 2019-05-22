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

    [DebuggerDisplay("{" + nameof(Name) + "}")]
    [DataContract]
    public class WikiBuildingInfo
    {
        public WikiBuildingInfo()
        {
            ConstructionInfos = new List<ConstructionInfo>();
            MaintenanceInfos = new List<MaintenanceInfo>();
            Region = WorldRegion.Unknown;
            Type = BuildingType.Unknown;
        }

        [DataMember(Order = 0)]
        public string Name { get; set; }

        [DataMember(Order = 1)]
        public string Icon { get; set; }

        [DataMember(Order = 2)]
        public WorldRegion Region { get; set; }

        [DataMember(Order = 3)]
        public string Tier { get; set; }

        [DataMember(Order = 4)]
        public BuildingType Type { get; set; }

        [DataMember(Order = 5)]
        public Size BuildingSize { get; set; }

        [DataMember(Order = 6)]
        public string Radius { get; set; }

        [DataMember(Order = 7)]
        public List<ConstructionInfo> ConstructionInfos { get; set; }

        [DataMember(Order = 8)]
        public List<MaintenanceInfo> MaintenanceInfos { get; set; }

        [DataMember(Order = 9)]
        public ProductionInfo ProductionInfos { get; set; }
    }
}
