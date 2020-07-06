using System;
using System.Collections.Generic;
using System.Text;

namespace InfoboxParser.Models
{
    public interface ITitleParserSingle
    {
        string GetBuildingTitle(string wikiText);
    }
}
