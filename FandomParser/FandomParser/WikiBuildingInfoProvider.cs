using FandomParser.Core.Presets.Models;
using FandomParser.WikiText;
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
        public WikiBuildingInfoPresets GetWikiBuildingInfos(WikiTextTableContainer list)
        {
            var wikibuildingList = new WikiBuildingInfoPresets();
            foreach (var curentry in list.Entries)
            {
                wikibuildingList.Infos.Add(parseWikiBuildingInfo(curentry));
            }

            //order buildings by name
            wikibuildingList.Infos = wikibuildingList.Infos.OrderBy(x => x.Name).ToList();

            return wikibuildingList;
        }

        private static WikiBuildingInfo parseWikiBuildingInfo(WikiTextTableEntry table)
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
                    if (!int.TryParse(splittedSize[0], out int x))
                    {

                    }

                    if (!int.TryParse(splittedSize[1], out int y))
                    {

                    }

                    result.BuildingSize = new Size(int.Parse(splittedSize[0]), int.Parse(splittedSize[1]));
                }

                if (table.ConstructionCost.Contains("<br />"))
                {
                    var splittedInfos = table.ConstructionCost.Split(new[] { "<br />" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var curSplittedInfo in splittedInfos)
                    {
                        var info = parseConstructionInfo(curSplittedInfo);
                        if (info != null)
                        {
                            result.ConstructionInfos.Add(info);
                        }
                    }
                }
                else if (!string.IsNullOrWhiteSpace(table.ConstructionCost))
                {
                    var info = parseConstructionInfo(table.ConstructionCost);
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
                        var info = parseMaintenanceInfo(curSplittedInfo);
                        if (info != null)
                        {
                            result.MaintenanceInfos.Add(info);
                        }
                    }
                }
                else if (!string.IsNullOrWhiteSpace(table.MaintenanceCost))
                {
                    var info = parseMaintenanceInfo(table.MaintenanceCost);
                    if (info != null)
                    {
                        result.MaintenanceInfos.Add(info);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return result;
        }

        private static ConstructionInfo parseConstructionInfo(string constructionCost)
        {
            ConstructionInfo result = null;

            if (constructionCost.Contains("[[File:"))
            {
                result = parseContainingFile(constructionCost);
            }
            else
            {
                result = parseContainingInfoIcon(constructionCost);
            }

            return result;
        }

        private static ConstructionInfo parseContainingInfoIcon(string constructionCost)
        {
            ConstructionInfo result = null;

            if (!double.TryParse(constructionCost.Split(new[] { "{{" }, StringSplitOptions.RemoveEmptyEntries)[0], out double value))
            {

            }

            var splittedInfo = constructionCost.Split(new[] { "{{" }, StringSplitOptions.RemoveEmptyEntries);

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
                Value = double.Parse(splittedInfo[0]),
                Unit = unit,
            };


            return result;
        }

        private static ConstructionInfo parseContainingFile(string constructionCost)
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

        private static MaintenanceInfo parseMaintenanceInfo(string maintenanceCost)
        {
            MaintenanceInfo result = null;

            if (maintenanceCost.Contains("[[File:"))
            {
                result = parseMaintenanceContainingFile(maintenanceCost);
            }
            else
            {
                result = parseMaintenanceContainingInfoIcon(maintenanceCost);
            }

            return result;
        }

        private static MaintenanceInfo parseMaintenanceContainingInfoIcon(string maintenanceCost)
        {
            MaintenanceInfo result = null;

            if (!double.TryParse(maintenanceCost.Split(new[] { "{{" }, StringSplitOptions.RemoveEmptyEntries)[0], out double value))
            {
                //−20
                //-20
            }

            var splittedInfo = maintenanceCost.Split(new[] { "{{" }, StringSplitOptions.RemoveEmptyEntries);

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
                Value = double.Parse(splittedInfo[0]),
                Unit = unit,
            };


            return result;
        }

        private static MaintenanceInfo parseMaintenanceContainingFile(string maintenanceCost)
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
