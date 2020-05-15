using FandomParser.Core.Presets.Models;
using FandomParser.WikiText;
using NLog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FandomParser
{
    public class WikiBuildingInfoProvider
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public WikiBuildingInfoPresets GetWikiBuildingInfos(WikiTextTableContainer list)
        {
            var wikibuildingList = new WikiBuildingInfoPresets();
            foreach (var curentry in list.Entries)
            {
                wikibuildingList.Infos.Add(ParseWikiBuildingInfo(curentry));
            }

            //order buildings by name
            wikibuildingList.Infos = wikibuildingList.Infos.OrderBy(x => x.Name).ToList();

            return wikibuildingList;
        }

        private static WikiBuildingInfo ParseWikiBuildingInfo(WikiTextTableEntry table)
        {
            var result = new WikiBuildingInfo();

            try
            {
                result.Region = table.Region;
                result.Tier = table.Tier;
                result.Name = table.Name.Replace("[[", string.Empty).Replace("]]", string.Empty);
                result.Icon = table.Icon.Replace("[[File:", string.Empty).Replace("|40px]]", string.Empty);

                if (table.Size.Contains(Environment.NewLine))
                {
                    var splitted = table.Size.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    result.Radius = splitted[1].Replace("(", string.Empty).Replace(")", string.Empty);
                    var splittedSize = splitted[0].Split('x');
                    result.BuildingSize = new Size(int.Parse(splittedSize[0]), int.Parse(splittedSize[1]));
                }
                else if (table.Size.Contains("<br />"))
                {
                    var splitted = table.Size.Split(new[] { "<br />" }, StringSplitOptions.RemoveEmptyEntries);
                    result.Radius = splitted[1].Replace("(", string.Empty).Replace(")", string.Empty);
                    var splittedSize = splitted[0].Split('x');
                    if (!int.TryParse(splittedSize[0], out int x))
                    {

                    }

                    if (!int.TryParse(splittedSize[1], out int y))
                    {

                    }
                    result.BuildingSize = new Size(int.Parse(splittedSize[0]), int.Parse(splittedSize[1]));
                }
                else if (!string.IsNullOrWhiteSpace(table.Size))
                {
                    var splittedSize = table.Size.Split('x');

                    var couldParseX = int.TryParse(splittedSize[0], out int x);
                    var couldParseY = int.TryParse(splittedSize[1], out int y);
                    if (!couldParseX || !couldParseY)
                    {
                        logger.Warn($"could not parse Size for \"{result.Name}\": \"{table.Size}\"");
                    }

                    result.BuildingSize = new Size(x, y);
                }

                if (table.ConstructionCost.Contains("<br />"))
                {
                    var splittedInfos = table.ConstructionCost.Split(new[] { "<br />" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var curSplittedInfo in splittedInfos)
                    {
                        var info = ParseConstructionInfo(curSplittedInfo);
                        if (info != null)
                        {
                            result.ConstructionInfos.Add(info);
                        }
                    }
                }
                else if (!string.IsNullOrWhiteSpace(table.ConstructionCost))
                {
                    var info = ParseConstructionInfo(table.ConstructionCost);
                    if (info != null)
                    {
                        result.ConstructionInfos.Add(info);
                    }
                }

                if (table.MaintenanceCost.Contains("<br />"))
                {
                    var splittedInfos = table.MaintenanceCost.Split(new[] { "<br />" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var curSplittedInfo in splittedInfos)
                    {
                        var info = ParseMaintenanceInfo(curSplittedInfo);
                        if (info != null)
                        {
                            result.MaintenanceInfos.Add(info);
                        }
                    }
                }
                else if (!string.IsNullOrWhiteSpace(table.MaintenanceCost))
                {
                    var info = ParseMaintenanceInfo(table.MaintenanceCost);
                    if (info != null)
                    {
                        result.MaintenanceInfos.Add(info);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"error parsing basic info for building: {table.Name}");
                Console.WriteLine(ex);
            }

            return result;
        }

        private static ConstructionInfo ParseConstructionInfo(string constructionCost)
        {
            ConstructionInfo result = null;

            if (constructionCost.Contains("[[File:"))
            {
                result = ParseContainingFile(constructionCost);
            }
            else
            {
                result = ParseContainingInfoIcon(constructionCost);
            }

            return result;
        }

        private static ConstructionInfo ParseContainingInfoIcon(string constructionCost)
        {
            ConstructionInfo result = null;

            var splittedInfo = constructionCost.Split(new[] { "{{" }, StringSplitOptions.RemoveEmptyEntries);

            if (!double.TryParse(splittedInfo[0], out double parsedValue))
            {
                logger.Warn($"could not parse construction cost: \"{constructionCost}\"");
            }

            var unit = new CostUnit();

            if (splittedInfo[1].Contains("Infoicon"))
            {
                unit.Type = CostUnitType.InfoIcon;
            }
            else
            {
                unit.Type = CostUnitType.Unknown;
            }

            unit.Name = splittedInfo[1].Replace("}}", string.Empty).Replace("Infoicon", string.Empty).Trim();

            result = new ConstructionInfo
            {
                Value = parsedValue,
                Unit = unit,
            };


            return result;
        }

        private static ConstructionInfo ParseContainingFile(string constructionCost)
        {
            ConstructionInfo result = null;

            var indexOfFileHeader = constructionCost.IndexOf("[[", StringComparison.OrdinalIgnoreCase);
            var valueString = constructionCost.Substring(0, indexOfFileHeader).Trim();
            var temp = constructionCost.Remove(0, indexOfFileHeader);
            if (!double.TryParse(valueString, out double value))
            {

            }

            temp = temp.Replace("[[File:", string.Empty); //filename with space? .Replace(" ", string.Empty)
            var splitted = temp.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
            var tempName = splitted[0];

            var unit = new CostUnit
            {
                Type = CostUnitType.File,
                Name = tempName.Trim()
            };

            result = new ConstructionInfo
            {
                Value = value,
                Unit = unit
            };

            return result;
        }

        private static MaintenanceInfo ParseMaintenanceInfo(string maintenanceCost)
        {
            MaintenanceInfo result = null;

            if (maintenanceCost.Contains("[[File:"))
            {
                result = ParseMaintenanceContainingFile(maintenanceCost);
            }
            else
            {
                result = ParseMaintenanceContainingInfoIcon(maintenanceCost);
            }

            return result;
        }

        private static MaintenanceInfo ParseMaintenanceContainingInfoIcon(string maintenanceCost)
        {
            MaintenanceInfo result = null;

            var splittedInfo = maintenanceCost.Split(new[] { "{{" }, StringSplitOptions.RemoveEmptyEntries);

            if (!double.TryParse(splittedInfo[0], out double parsedValue))
            {
                logger.Warn($"could not parse maintenance cost: \"{maintenanceCost}\"");
            }

            var unit = new CostUnit();

            if (splittedInfo[1].Contains("Infoicon"))
            {
                unit.Type = CostUnitType.InfoIcon;
            }
            else
            {
                unit.Type = CostUnitType.Unknown;
            }

            unit.Name = splittedInfo[1].Replace("}}", string.Empty).Replace("Infoicon", string.Empty).Trim();

            result = new MaintenanceInfo
            {
                Value = parsedValue,
                Unit = unit,
            };


            return result;
        }

        private static MaintenanceInfo ParseMaintenanceContainingFile(string maintenanceCost)
        {
            MaintenanceInfo result = null;

            var indexOfFileHeader = maintenanceCost.IndexOf("[[", StringComparison.OrdinalIgnoreCase);
            var valueString = maintenanceCost.Substring(0, indexOfFileHeader).Trim();
            var temp = maintenanceCost.Remove(0, indexOfFileHeader);
            if (!double.TryParse(valueString, out double value))
            {

            }

            temp = temp.Replace("[[File:", string.Empty); //filename with space? .Replace(" ", string.Empty)
            var splitted = temp.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
            var tempName = splitted[0];

            var unit = new CostUnit
            {
                Type = CostUnitType.File,
                Name = tempName.Trim()
            };

            result = new MaintenanceInfo
            {
                Value = value,
                Unit = unit
            };

            return result;
        }
    }
}
