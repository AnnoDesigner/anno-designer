using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FandomParser.Core.Models;
using FandomParser.Core.Presets.Models;

namespace InfoboxParser.Models
{
    public class Infobox : IInfobox
    {
        public string Name { get; set; }

        public BuildingType Type { get; set; }

        public ProductionInfo ProductionInfos { get; set; }

        public SupplyInfo SupplyInfos { get; set; }

        public UnlockInfo UnlockInfos { get; set; }
    }
}
