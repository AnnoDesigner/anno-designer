using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FandomParser.Core.Presets.Models;

namespace FandomParser.WikiText
{
    public class WikiTextTableParser
    {
        private static Dictionary<WorldRegion, Dictionary<string, string>> RegionTables { get; set; }

        private static readonly Regex regexNormalizeLineEndings = new Regex(@"\r\n|\n|\r", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public WikiTextTableContainer GetTables(string wikiText)
        {
            var cleanedTables = GetTablesFromWikiText(wikiText);
            var result = ParseTables(cleanedTables);
            result.Version = new Version(1, 0, 0, 0);

            return result;
        }

        private static List<string> GetTablesFromWikiText(string wikiTextToParse)
        {
            RegionTables = SplitPageByRegions(wikiTextToParse);

            var cleanedTables = new List<string>();

            foreach (var region in RegionTables)
            {
                foreach (var tier in region.Value)
                {
                    cleanedTables.Add(tier.Value);
                }
            }

            return cleanedTables;
        }

        private static Dictionary<WorldRegion, Dictionary<string, string>> SplitPageByRegions(string wikiTextToParse)
        {
            var groupedTables = GroupTablesByRegion(wikiTextToParse);
            var parsedAndGroupedTables = ParseRegionAndTableHeaders(groupedTables);

            return parsedAndGroupedTables;
        }

        private static Dictionary<string, List<string>> GroupTablesByRegion(string wikiTextToParse)
        {
            var result = new Dictionary<string, List<string>>();
            var lastKey = string.Empty;

            var parsedArticle = wikiTextToParse.Split(new string[] { "== " }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var curEntry in parsedArticle)
            {
                if (curEntry.Contains(" ==="))
                {
                    if (!string.IsNullOrWhiteSpace(lastKey) && result.ContainsKey(lastKey))
                    {
                        result[lastKey].Add(curEntry);
                    }
                }
                else if (curEntry.Contains(" =="))
                {
                    if (!result.ContainsKey(curEntry))
                    {
                        result.Add(curEntry, new List<string>());
                        lastKey = curEntry;
                    }
                }
            }

            return result;
        }

        private static Dictionary<WorldRegion, Dictionary<string, string>> ParseRegionAndTableHeaders(Dictionary<string, List<string>> groupedTables)
        {
            var result = new Dictionary<WorldRegion, Dictionary<string, string>>();

            foreach (var curPair in groupedTables)
            {
                var regionHeader = ParseWorldRegion(curPair.Key);
                var parsedTableHeaders = ParseTableHeaders(curPair.Value);

                result.Add(regionHeader, parsedTableHeaders);
            }

            return result;
        }

        private static WorldRegion ParseWorldRegion(string worldRegionHeader)
        {
            WorldRegion result = WorldRegion.Unknown;

            var parsedRegionHeader = worldRegionHeader.Split(new string[] { " ==" }, StringSplitOptions.RemoveEmptyEntries)[0].Trim();
            if (parsedRegionHeader.Equals("Old World buildings", StringComparison.OrdinalIgnoreCase))
            {
                result = WorldRegion.OldWorld;
            }
            else if (parsedRegionHeader.Equals("New World buildings", StringComparison.OrdinalIgnoreCase))
            {
                result = WorldRegion.NewWorld;
            }

            return result;
        }

        private static Dictionary<string, string> ParseTableHeaders(List<string> tables)
        {
            var result = new Dictionary<string, string>();

            foreach (var curTable in tables)
            {
                var splitted = curTable.Split(new string[] { " ===" }, StringSplitOptions.RemoveEmptyEntries);
                var parsedTableHeader = splitted[0].Trim()
                    .Replace(" buildings", string.Empty)
                    .Replace(" Buildings", string.Empty);

                //align linebreaks for each table
                var x = AlignLineBreaksInTables(new List<string> { splitted[1] });

                //table headers: Icon|Name|Description|Construction cost|Maintenance cost|Size
                //remove start of each table
                x = RemoveTableHeaders(x);

                result.Add(parsedTableHeader, x.First());
            }

            return result;
        }

        private static List<string> SplitPageByTables(string wikiTextToParse)
        {
            var tables = new List<string>();

            var parsedArticle = wikiTextToParse.Split(new string[] { "===" }, StringSplitOptions.RemoveEmptyEntries)
               .Where(x => x.Contains("class=\"mw-collapsible") && x.Contains("article-table\""))
               .ToList();

            tables.AddRange(parsedArticle);

            //foreach (var curEntry in parsedArticle)
            //{
            //    if (curEntry.Contains("class=\"mw-collapsible") && curEntry.Contains("article-table\""))
            //    {
            //        tables.Add(curEntry);
            //    }
            //}

            return tables;
        }

        private static List<string> AlignLineBreaksInTables(List<string> tableList)
        {
            var tablesWithLineBreaks = new List<string>();

            foreach (var curTable in tableList)
            {
                var normalized = regexNormalizeLineEndings.Replace(curTable, Environment.NewLine);
                //var normalized = Regex.Replace(curTable, @"\r\n|\n|\r", "\r\n");
                tablesWithLineBreaks.Add(normalized);

                //var splittedTable = curTable.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                //var tableWithLineBreaks = string.Join(Environment.NewLine, splittedTable);

                //tablesWithLineBreaks.Add(tableWithLineBreaks);
            }

            return tablesWithLineBreaks;
        }

        private static List<string> RemoveTableHeaders(List<string> tablesWithLineBreaks)
        {
            var cleanedTables = new List<string>();

            foreach (var curTable in tablesWithLineBreaks)
            {
                var split = curTable.Split(new[] { $"! style=\"text-align:center;\" |Size" }, StringSplitOptions.RemoveEmptyEntries);
                cleanedTables.Add(split[1]);
            }

            return cleanedTables;
        }

        private static WikiTextTableContainer ParseTables(List<string> cleanedTables)
        {
            var result = new WikiTextTableContainer();

            var allTableEntries = new List<WikiTextTableEntry>();

            foreach (var curTable in cleanedTables)
            {
                var parsedTable = ParseTableEntry(curTable);

                allTableEntries.AddRange(parsedTable);
            }

            var comparer = new WikiTextTableEntryComparer();

            //285 vs 260 (Ornamentals?)
            //var distinctAllEntries = allTableEntries.Distinct(comparer).ToList();
            //var test = allTableEntries.Except(distinctAllEntries).ToList();

            allTableEntries = allTableEntries.Distinct(comparer).ToList();

            result.Entries = allTableEntries;

            return result;
        }

        private static List<WikiTextTableEntry> ParseTableEntry(string curTable)
        {
            var allEntries = new List<WikiTextTableEntry>();

            var regionInfo = GetRegionAndTierInfo(curTable);

            WikiTextTableEntry curEntry = null;
            var entryCounter = 0;
            var lastLineWasCollapsible = false;
            //read string line by line
            //use StringReader?
            foreach (var curLine in curTable.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
            {
                //line is a collapsible information
                if (lastLineWasCollapsible)
                {
                    lastLineWasCollapsible = !lastLineWasCollapsible;
                    continue;
                }

                if (curLine.StartsWith("!", StringComparison.OrdinalIgnoreCase) && curLine.Contains("colspan=\"6\""))
                {
                    lastLineWasCollapsible = true;
                    continue;
                }

                //line is empty
                if (string.IsNullOrWhiteSpace(curLine))
                {
                    continue;
                }

                //line is end of table
                if (curLine.Equals("=", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                //line contains description
                if (!curLine.StartsWith("|", StringComparison.OrdinalIgnoreCase) && !curLine.StartsWith("(", StringComparison.OrdinalIgnoreCase))
                {
                    curEntry.Description += $"{Environment.NewLine}{curLine}";
                    continue;
                }

                //line contains radius info
                if (curLine.StartsWith("(", StringComparison.OrdinalIgnoreCase))
                {
                    curEntry.Size += $"{Environment.NewLine}{curLine}";
                    continue;
                }

                //start new entry
                if (curLine.StartsWith("|-", StringComparison.OrdinalIgnoreCase))
                {
                    if (curEntry != null)
                    {
                        allEntries.Add(curEntry);
                    }

                    entryCounter = 0;
                    curEntry = new WikiTextTableEntry
                    {
                        Region = regionInfo.Item1,
                        Tier = regionInfo.Item2
                    };
                    continue;
                }

                switch (entryCounter)
                {
                    case 0:
                        {
                            curEntry.Icon = curLine.Remove(0, 1);
                            entryCounter++;
                            break;
                        }
                    case 1:
                        {
                            curEntry.Name = curLine.Remove(0, 1);
                            entryCounter++;
                            break;
                        }
                    case 2:
                        {
                            curEntry.Description = curLine.Remove(0, 1);
                            entryCounter++;
                            break;
                        }
                    case 3:
                        {
                            curEntry.ConstructionCost = curLine.Remove(0, 1)
                                .Replace(" style=\"text-align:center;\" |", string.Empty)
                                .Replace(" style=\"text-align: center;\" |", string.Empty);
                            entryCounter++;
                            break;
                        }
                    case 4:
                        {
                            curEntry.MaintenanceCost = curLine.Remove(0, 1)
                                .Replace(" style=\"text-align:center;\" |", string.Empty)
                                .Replace(" style=\"text-align: center;\" |", string.Empty)
                                .Replace("−", "-");//can't parse as negative number
                            entryCounter++;
                            break;
                        }
                    case 5:
                        {
                            curEntry.Size = curLine.Remove(0, 1)
                                .Replace(" style=\"text-align:center;\" |", string.Empty)
                                .Replace(" style=\"text-align: center;\" |", string.Empty);
                            entryCounter++;
                            break;
                        }
                    default:
                        break;
                }
            }

            //add last entry
            if (curEntry != null && !allEntries.Contains(curEntry))
            {
                allEntries.Add(curEntry);
            }

            return allEntries;
        }

        private static Tuple<WorldRegion, string> GetRegionAndTierInfo(string wikiTextTable)
        {
            var result = Tuple.Create(WorldRegion.Unknown, string.Empty);

            foreach (var regionPair in RegionTables)
            {
                foreach (var tierPair in regionPair.Value)
                {
                    if (tierPair.Value.Equals(wikiTextTable, StringComparison.OrdinalIgnoreCase))
                    {
                        return Tuple.Create(regionPair.Key, tierPair.Key);
                    }
                }
            }

            return result;
        }
    }
}
