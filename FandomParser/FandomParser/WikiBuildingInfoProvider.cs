using FandomParser.WikiText;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FandomParser
{
    public class WikiBuildingInfoProvider
    {
        public WikiBuildingInfoList GetWikiBuildingInfos(TableEntryList list)
        {
            var wikibuildingList = new WikiBuildingInfoList();
            wikibuildingList.Version = new Version(0, 2, 0, 0);
            foreach (var curentry in list.Entries)
            {
                wikibuildingList.Infos.Add(parseWikiBuildingInfo(curentry));
            }

            return wikibuildingList;
        }

        private static WikiBuildingInfo parseWikiBuildingInfo(TableEntry entry)
        {
            var result = new WikiBuildingInfo();

            try
            {
                result.Region = entry.Region;
                result.Tier = entry.Tier;
                result.Name = entry.Name.Replace("[[", string.Empty).Replace("]]", string.Empty);
                result.Icon = entry.Icon.Replace("[[File:", string.Empty).Replace("|40px]]", string.Empty);

                if (entry.Size.Contains(Environment.NewLine))
                {
                    var splitted = entry.Size.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    result.Radius = splitted[1].Replace("(", string.Empty).Replace(")", string.Empty);
                    var splittedSize = splitted[0].Split('x');
                    result.BuildingSize = new Size(double.Parse(splittedSize[0]), (double.Parse(splittedSize[1])));
                }
                else if (entry.Size.Contains("<br />"))
                {
                    var splitted = entry.Size.Split(new[] { "<br />" }, StringSplitOptions.RemoveEmptyEntries);
                    result.Radius = splitted[1].Replace("(", string.Empty).Replace(")", string.Empty);
                    var splittedSize = splitted[0].Split('x');
                    if (!double.TryParse(splittedSize[0], out double x))
                    {

                    }

                    if (!double.TryParse(splittedSize[1], out double y))
                    {

                    }
                    result.BuildingSize = new Size(double.Parse(splittedSize[0]), (double.Parse(splittedSize[1])));
                }
                else if (!string.IsNullOrWhiteSpace(entry.Size))
                {
                    var splittedSize = entry.Size.Split('x');
                    if (!double.TryParse(splittedSize[0], out double x))
                    {

                    }

                    if (!double.TryParse(splittedSize[1], out double y))
                    {

                    }

                    result.BuildingSize = new Size(double.Parse(splittedSize[0]), (double.Parse(splittedSize[1])));
                }

                if (entry.ConstructionCost.Contains("<br />"))
                {
                    var splittedInfos = entry.ConstructionCost.Split(new[] { "<br />" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var curSplittedInfo in splittedInfos)
                    {
                        var info = parseConstructionInfo(curSplittedInfo);
                        if (info != null)
                        {
                            result.ConstructionInfos.Add(info);
                        }
                    }
                }
                else if (!string.IsNullOrWhiteSpace(entry.ConstructionCost))
                {
                    var info = parseConstructionInfo(entry.ConstructionCost);
                    if (info != null)
                    {
                        result.ConstructionInfos.Add(info);
                    }
                }

                if (entry.MaintenanceCost.Contains("<br />"))
                {
                    var splittedInfos = entry.MaintenanceCost.Split(new[] { "<br />" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var curSplittedInfo in splittedInfos)
                    {
                        var info = parseMaintenanceInfo(curSplittedInfo);
                        if (info != null)
                        {
                            result.MaintenanceInfos.Add(info);
                        }
                    }
                }
                else if (!string.IsNullOrWhiteSpace(entry.MaintenanceCost))
                {
                    var info = parseMaintenanceInfo(entry.MaintenanceCost);
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

            var unit = new WikiCostUnit();

            if (splittedInfo[1].Contains("Infoicon"))
            {
                unit.Type = WikiCostUnitType.InfoIcon;
            }
            else
            {
                unit.Type = WikiCostUnitType.Unknown;
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

            var unit = new WikiCostUnit
            {
                Type = WikiCostUnitType.File,
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

            var unit = new WikiCostUnit();

            if (splittedInfo[1].Contains("Infoicon"))
            {
                unit.Type = WikiCostUnitType.InfoIcon;
            }
            else
            {
                unit.Type = WikiCostUnitType.Unknown;
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

            var unit = new WikiCostUnit
            {
                Type = WikiCostUnitType.File,
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
