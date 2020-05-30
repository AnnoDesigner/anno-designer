using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using FandomParser.Core;
using InfoboxParser.Models;

namespace InfoboxParser
{
    public class InfoboxExtractor : IInfoboxExtractor
    {
        private static readonly Regex regexReplaceEmpyLines = new Regex(@"^\s*$\n|\r", RegexOptions.Multiline | RegexOptions.Compiled);
        private readonly Regex regexMultipleInfoboxes;
        private readonly ICommons _commons;
        private readonly ITitleParserSingle _titleParserSingle;

        public InfoboxExtractor(ICommons commonsToUse, ITitleParserSingle titleParserSingleToUse)
        {
            _commons = commonsToUse;
            _titleParserSingle = titleParserSingleToUse;

            regexMultipleInfoboxes = new Regex(_commons.InfoboxTemplateStart, RegexOptions.Compiled);
        }

        public List<(string title, string infobox)> ExtractInfobox(string content)
        {
            var result = new List<(string title, string infobox)>();

            if (string.IsNullOrWhiteSpace(content))
            {
                return result;
            }

            var indexInfoboxOldAndNewWorldStart = content.IndexOf(_commons.InfoboxTemplateStartOldAndNewWorld, StringComparison.OrdinalIgnoreCase);
            var indexInfobox2RegionsStart = content.IndexOf(_commons.InfoboxTemplateStart2Regions, StringComparison.OrdinalIgnoreCase);
            var indexInfobox3RegionsStart = content.IndexOf(_commons.InfoboxTemplateStart3Regions, StringComparison.OrdinalIgnoreCase);
            var indexInfoboxStart = content.IndexOf(_commons.InfoboxTemplateStart, StringComparison.OrdinalIgnoreCase);
            var isInfoboxMultiple = false;

            var startIndex = -1;
            if (indexInfoboxOldAndNewWorldStart != -1)
            {
                startIndex = indexInfoboxOldAndNewWorldStart;
            }
            else if (indexInfobox2RegionsStart != -1)
            {
                startIndex = indexInfobox2RegionsStart;
                isInfoboxMultiple = true;
            }
            else if (indexInfobox3RegionsStart != -1)
            {
                startIndex = indexInfobox3RegionsStart;
                isInfoboxMultiple = true;
            }
            else if (indexInfoboxStart != -1)
            {
                //check for multiple infoboxes (only single region)
                var matches = regexMultipleInfoboxes.Matches(content);
                if (matches.Count > 1)
                {
                    foreach (Match curMatch in matches)
                    {
                        var localInfobox = GetInfobox(content, curMatch.Index);
                        var localTitle = _titleParserSingle.GetBuildingTitle(localInfobox);
                        result.Add((localTitle, localInfobox));
                    }

                    return result;
                }

                startIndex = indexInfoboxStart;
            }

            var infobox = GetInfobox(content, startIndex);
            string title = null;
            if (isInfoboxMultiple)
            {
                title = _titleParserSingle.GetBuildingTitle(infobox);
            }
            result.Add((title, infobox));
            return result;
        }

        private string GetInfobox(string content, int startIndex)
        {
            string result = null;

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
                result = FormatInfobox(infoBox);
            }

            return result;
        }

        private string FormatInfobox(string infobox)
        {
            //format infobox with new entries on separate lines for later parsing
            var splittedInfobox = infobox.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            var infoboxWithLineBreaks = string.Join(Environment.NewLine + '|', splittedInfobox);
            var removedEmptyLines = RemoveEmptyLines(infoboxWithLineBreaks);

            return removedEmptyLines;
        }

        private string RemoveEmptyLines(string lines)
        {
            return regexReplaceEmpyLines.Replace(lines, string.Empty).TrimEnd();
        }
    }
}
