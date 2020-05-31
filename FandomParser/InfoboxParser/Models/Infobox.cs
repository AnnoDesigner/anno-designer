using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FandomParser.Core.Models;
using FandomParser.Core.Presets.Models;

namespace InfoboxParser.Models
{
    [DebuggerDisplay("{" + nameof(Name) + "} ({" + nameof(Region) + "})")]
    public class Infobox : IInfobox
    {
        public Infobox()
        {
            Type = BuildingType.Unknown;
            Region = WorldRegion.Unknown;
        }

        public string Name { get; set; }

        public string Icon { get; set; }

        public Size BuildingSize { get; set; }

        public BuildingType Type { get; set; }

        public WorldRegion Region { get; set; }

        public ProductionInfo ProductionInfos { get; set; }

        public SupplyInfo SupplyInfos { get; set; }

        public UnlockInfo UnlockInfos { get; set; }

        public List<ConstructionInfo> ConstructionInfos { get; set; }
    }
}
