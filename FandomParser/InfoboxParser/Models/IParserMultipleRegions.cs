using System;
using System.Collections.Generic;
using System.Text;
using FandomParser.Core.Models;

namespace InfoboxParser.Models
{
    interface IParserMultipleRegions
    {
        List<IInfobox> GetInfobox(string wikiText, List<string> possibleRegions);
    }
}
