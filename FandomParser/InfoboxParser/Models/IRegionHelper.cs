using System;
using System.Collections.Generic;
using System.Text;
using FandomParser.Core.Presets.Models;

namespace InfoboxParser.Models
{
    public interface IRegionHelper
    {
        WorldRegion GetRegion(string regionName);
    }
}
