using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;

namespace FandomParser.Core.Presets.Models
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
            RevisionDate = DateTime.MinValue;
            RevisionId = -1;
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
        public Uri Url { get; set; }

        [DataMember(Order = 8)]
        public int RevisionId { get; set; }

        [DataMember(Order = 9, Name = "RevisionDate")]
        public string FormattedRevisionDate { get; set; }

        [IgnoreDataMember]
        public DateTime RevisionDate
        {
            //"o" -> round-trippable format which is ISO-8601-compatible.
            get { return DateTime.ParseExact(FormattedRevisionDate, "o", CultureInfo.InvariantCulture); }
            set { FormattedRevisionDate = value.ToString("o"); }
        }

        [DataMember(Order = 10)]
        public List<ConstructionInfo> ConstructionInfos { get; set; }

        [DataMember(Order = 11)]
        public List<MaintenanceInfo> MaintenanceInfos { get; set; }

        [DataMember(Order = 12)]
        public ProductionInfo ProductionInfos { get; set; }

        [DataMember(Order = 13)]
        public SupplyInfo SupplyInfos { get; set; }

        [DataMember(Order = 14)]
        public UnlockInfo UnlockInfos { get; set; }
    }
}
