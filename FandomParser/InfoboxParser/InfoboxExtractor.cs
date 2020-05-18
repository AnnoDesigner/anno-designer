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

            if (string.IsNullOrWhiteSpace(content))
            {
                return result;
            }

            var indexInfoboxOldAndNewWorldStart = content.IndexOf(_commons.InfoboxTemplateStartOldAndNewWorld, StringComparison.OrdinalIgnoreCase);
            var indexInfobox2RegionsStart = content.IndexOf(_commons.InfoboxTemplateStart2Regions, StringComparison.OrdinalIgnoreCase);
            var indexInfobox3RegionsStart = content.IndexOf(_commons.InfoboxTemplateStart3Regions, StringComparison.OrdinalIgnoreCase);
            var indexInfoboxStart = content.IndexOf(_commons.InfoboxTemplateStart, StringComparison.OrdinalIgnoreCase);

            var startIndex = -1;
            if (indexInfoboxOldAndNewWorldStart != -1)
            {
                startIndex = indexInfoboxOldAndNewWorldStart;
            }
            else if (indexInfobox2RegionsStart != -1)
            {
                startIndex = indexInfobox2RegionsStart;
            }
            else if (indexInfobox3RegionsStart != -1)
            {
                startIndex = indexInfobox3RegionsStart;
            }
            else if (indexInfoboxStart != -1)
            {
                startIndex = indexInfoboxStart;
            }

            //handle files with no infobox
            if (startIndex == -1)
            {
                return result;
            }

            //find {{ before
            //if before = {{Goodsicon, search further for }}
            //repeat until false

            var endIndex = content.IndexOf(_commons.InfoboxTemplateEnd, startIndex, StringComparison.OrdinalIgnoreCase);
            var subStartIndex = startIndex;
            while (endIndex != -1)
            {
                var subContent = content.Substring(subStartIndex, content.Length - subStartIndex);
                if (subContent.Contains("{{Goodsicon"))
                {
                    subStartIndex = endIndex + _commons.InfoboxTemplateEnd.Length;
                    endIndex = content.IndexOf(_commons.InfoboxTemplateEnd, subStartIndex, StringComparison.OrdinalIgnoreCase);
                }
                else
                {
                    endIndex = content.IndexOf(_commons.InfoboxTemplateEnd, subStartIndex, StringComparison.OrdinalIgnoreCase);
                    break;
                }
            }

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
