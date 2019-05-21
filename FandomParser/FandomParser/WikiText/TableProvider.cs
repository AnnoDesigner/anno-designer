﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FandomParser.WikiText
{
    public class TableProvider
    {
        private static Dictionary<WorldRegion, Dictionary<string, string>> RegionTables { get; set; }

        public TableEntryList GetTables(string wikiText)
        {
            var cleanedTables = getTablesFromWikiText(wikiText);
            return parseTables(cleanedTables);
        }

        private static List<string> getTablesFromWikiText(string wikiTextToParse)
        {
            RegionTables = splitPageByRegions(wikiTextToParse);

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

        private static Dictionary<WorldRegion, Dictionary<string, string>> splitPageByRegions(string wikiTextToParse)
        {
            var groupedTables = groupTablesByRegion(wikiTextToParse);
            var parsedAndGroupedTables = parseRegionAndTableHeaders(groupedTables);

            return parsedAndGroupedTables;
        }

        private static Dictionary<string, List<string>> groupTablesByRegion(string wikiTextToParse)
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

        private static Dictionary<WorldRegion, Dictionary<string, string>> parseRegionAndTableHeaders(Dictionary<string, List<string>> groupedTables)
        {
            var result = new Dictionary<WorldRegion, Dictionary<string, string>>();

            foreach (var curPair in groupedTables)
            {
                var regionHeader = parseWorldRegion(curPair.Key);
                var parsedTableHeaders = parseTableHeaders(curPair.Value);

                result.Add(regionHeader, parsedTableHeaders);
            }

            return result;
        }

        private static WorldRegion parseWorldRegion(string worldRegionHeader)
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

        private static Dictionary<string, string> parseTableHeaders(List<string> tables)
        {
            var result = new Dictionary<string, string>();

            foreach (var curTable in tables)
            {
                var splitted = curTable.Split(new string[] { " ===" }, StringSplitOptions.RemoveEmptyEntries);
                var parsedTableHeader = splitted[0].Trim().Replace(" buildings", string.Empty);

                //align linebreaks for each table
                var x = alignLineBreaksInTables(new List<string> { splitted[1] });

                //table headers: Icon|Name|Description|Construction cost|Maintenance cost|Size
                //remove start of each table
                x = removeTableHeaders(x);

                result.Add(parsedTableHeader, x.First());
            }

            return result;
        }

        private static List<string> splitPageByTables(string wikiTextToParse)
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

        private static List<string> alignLineBreaksInTables(List<string> tableList)
        {
            var tablesWithLineBreaks = new List<string>();

            foreach (var curTable in tableList)
            {
                var splittedTable = curTable.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                var tableWithLineBreaks = String.Join(Environment.NewLine, splittedTable);

                tablesWithLineBreaks.Add(tableWithLineBreaks);
            }

            return tablesWithLineBreaks;
        }

        private static List<string> removeTableHeaders(List<string> tablesWithLineBreaks)
        {
            var cleanedTables = new List<string>();

            foreach (var curTable in tablesWithLineBreaks)
            {
                var split = curTable.Split(new[] { $"|Size{Environment.NewLine}" }, StringSplitOptions.RemoveEmptyEntries);
                cleanedTables.Add(split[1]);
            }

            return cleanedTables;
        }

        private static TableEntryList parseTables(List<string> cleanedTables)
        {
            var result = new TableEntryList();

            var allTableEntries = new List<TableEntry>();

            foreach (var curTable in cleanedTables)
            {
                var parsedTable = parseTableEntry(curTable);

                allTableEntries.AddRange(parsedTable);
            }

            allTableEntries = allTableEntries.Distinct().ToList();

            result = new TableEntryList
            {
                Version = new Version(0, 0, 0, 1),

                Entries = allTableEntries
            };

            return result;
        }

        private static List<TableEntry> parseTableEntry(string curTable)
        {
            var allEntries = new List<TableEntry>();

            var regionInfo = getRegionAndTierInfo(curTable);

            TableEntry curEntry = null;
            var entryCounter = 0;
            //read string line by line
            foreach (var curLine in curTable.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
            {
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
                    curEntry = new TableEntry
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

            return allEntries;
        }

        private static Tuple<WorldRegion, string> getRegionAndTierInfo(string wikiTextTable)
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
