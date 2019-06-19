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

        private readonly Parser parser;
        private readonly ParserBothWorlds parserBothWorlds;

        public InfoboxParser(ICommons commons)
        {
            _commons = commons;

            parser = new Parser(_commons);
            parserBothWorlds = new ParserBothWorlds(_commons);
        }

        public List<IInfobox> GetInfobox(string wikiText)
        {
            if (string.IsNullOrWhiteSpace(wikiText))
            {
                return null;
            }

            var result = new List<IInfobox>();

            if (!wikiText.StartsWith(_commons.InfoboxTemplateStartBothWorlds))
            {
                var infoboxes = parser.GetInfobox(wikiText);

                result.AddRange(infoboxes);
            }
            else
            {
                var infoboxes = parserBothWorlds.GetInfobox(wikiText);

                result.AddRange(infoboxes);
            }

            return result;
        }
    }
}
