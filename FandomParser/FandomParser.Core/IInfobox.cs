using System;
using System.Collections.Generic;
using System.Text;

namespace FandomParser.Core
{
    public interface IInfobox
    {
        string Name { get; set; }

        BuildingType Type { get; set; }

        ProductionInfo ProductionInfos { get; set; }

        SupplyInfo SupplyInfos { get; set; }

        UnlockInfo UnlockInfos { get; set; }
    }
}
