using System;
using System.Collections.Generic;
using System.Text;
using FandomParser.Core.Presets.Models;

namespace FandomParser.Core.Models
{
    public interface IInfobox
    {
        string Name { get; set; }

        string Icon { get; set; }

        BuildingType Type { get; set; }

        WorldRegion Region { get; set; }

        ProductionInfo ProductionInfos { get; set; }

        SupplyInfo SupplyInfos { get; set; }

        UnlockInfo UnlockInfos { get; set; }
    }
}
