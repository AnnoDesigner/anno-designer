using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using FandomParser.Core;
using FandomParser.Core.Models;
using FandomParser.Core.Presets.Models;
using InfoboxParser.Models;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("InfoboxParser.Tests")]

namespace InfoboxParser
{
    public class InfoboxParser
    {
        private readonly ICommons _commons;
        private readonly ISpecialBuildingNameHelper _specialBuildingNameHelper;

        private readonly IParser parser;
        private readonly IParser parserBothWorlds;
        private readonly IParser parser2Regions;

        public InfoboxParser(ICommons commons, ISpecialBuildingNameHelper specialBuildingNameHelper)
        {
            _commons = commons;
            _specialBuildingNameHelper = specialBuildingNameHelper;

            parser = new Parser(_commons);
            parserBothWorlds = new ParserBothWorlds(_commons);
            parser2Regions = new Parser2Regions(_commons, _specialBuildingNameHelper);
        }

        public List<IInfobox> GetInfobox(string wikiText)
        {
            if (string.IsNullOrWhiteSpace(wikiText))
            {
                return null;
            }

            var result = new List<IInfobox>();


            if (wikiText.StartsWith(_commons.InfoboxTemplateStartBothWorlds))
            {
                var infoboxes = parserBothWorlds.GetInfobox(wikiText);

                result.AddRange(infoboxes);
            }
            else if (wikiText.StartsWith(_commons.InfoboxTemplateStart2Regions))
            {
                var infoboxes = parser2Regions.GetInfobox(wikiText);

                result.AddRange(infoboxes);
            }
            else if (wikiText.StartsWith(_commons.InfoboxTemplateStart3Regions))
            {
                //var infoboxes = parser.GetInfobox(wikiText);

                //result.AddRange(infoboxes);
            }
            else if (wikiText.StartsWith(_commons.InfoboxTemplateStart))
            {
                var infoboxes = parser.GetInfobox(wikiText);

                result.AddRange(infoboxes);
            }

            return result;
        }
    }
}
