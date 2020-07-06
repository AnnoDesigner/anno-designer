using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FandomParser.Core;
using InfoboxParser.Models;

namespace InfoboxParser.Parser
{
    public class TitleParserSingle : ITitleParserSingle
    {
        private readonly ICommons _commons;
        private readonly ISpecialBuildingNameHelper _specialBuildingNameHelper;

        public TitleParserSingle(ICommons commons, ISpecialBuildingNameHelper specialBuildingNameHelperToUse)
        {
            _commons = commons;
            _specialBuildingNameHelper = specialBuildingNameHelperToUse;
        }

        public string GetBuildingTitle(string wikiText)
        {
            var result = string.Empty;

            if (!wikiText.Contains("|Title"))
            {
                return result;
            }

            using (var reader = new StringReader(wikiText))
            {
                string curLine;
                while ((curLine = reader.ReadLine()) != null)
                {
                    curLine = curLine.Replace(_commons.InfoboxTemplateEnd, string.Empty);

                    if (curLine.StartsWith("|Title", StringComparison.OrdinalIgnoreCase))
                    {
                        result = curLine.Replace("|Title", string.Empty)
                            .Replace("=", string.Empty)
                            .Trim();
                        break;
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(result))
            {
                result = _specialBuildingNameHelper.CheckSpecialBuildingName(result);
            }

            return result;
        }
    }
}
