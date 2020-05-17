using System;
using System.Collections.Generic;
using System.Text;
using FandomParser.Core;

namespace InfoboxParser
{
    public class InfoboxExtractor : IInfoboxExtractor
    {
        private readonly ICommons _commons;

        public InfoboxExtractor(ICommons commons)
        {
            _commons = commons;
        }

        public string ExtractInfobox(string content)
        {
            string result = null;

            var indexInfoboxBothWorldsStart = content.IndexOf(_commons.InfoboxTemplateStartBothWorlds, StringComparison.OrdinalIgnoreCase);
            var indexInfoboxStart = content.IndexOf(_commons.InfoboxTemplateStart, StringComparison.OrdinalIgnoreCase);

            var startIndex = indexInfoboxBothWorldsStart == -1 ? indexInfoboxStart : indexInfoboxBothWorldsStart;

            //handle files with no infobox
            if (startIndex == -1)
            {
                return result;
            }

            var endIndex = content.IndexOf(_commons.InfoboxTemplateEnd, startIndex, StringComparison.OrdinalIgnoreCase);
            //find {{ before
            //if before = {{Goodsicon, search further for }}
            //repeat until false
            int length = endIndex - startIndex + _commons.InfoboxTemplateEnd.Length;

            var infoBox = content.Substring(startIndex, length);
            if (!string.IsNullOrWhiteSpace(infoBox))
            {
                //format infobox with new entries on separate lines for later parsing
                var splittedInfobox = infoBox.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                var infoboxWithLineBreaks = string.Join(Environment.NewLine + '|', splittedInfobox);

                result = infoboxWithLineBreaks;
            }

            return result;
        }
    }
}
