using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using AnnoDesigner.Core.Presets.Comparer;
using AnnoDesigner.Core.Presets.Models;

namespace AnnoDesigner
{
    public static class BuildingPresetsExtensions
    {
        public static void AddToTree(this BuildingPresets buildingPresets, TreeView treeView)
        {
            var excludedTemplates = new[] { "Ark", "Harbour", "OrnamentBuilding" };
            var excludedFactions = new[] { "third party", "Facility Modules" };
            var list = buildingPresets.Buildings
                .Where(_ => !excludedTemplates.Contains(_.Template))
                .Where(_ => !excludedFactions.Contains(_.Faction));

            //For Anno 2205 only
            var modulesList = buildingPresets.Buildings
                            .Where(_ => _.Header == "(A6) Anno 2205")
                            .Where(_ => _.Faction == "Facility Modules")
                            .Where(_ => _.Faction != "Facilities")
                            .ToList();
            var facilityList = list.Where(_ => _.Faction == "Facilities").ToList();
            //Get a list of nonMatchedModules;
            var nonMatchedModulesList = modulesList.Except(facilityList, new BuildingInfoComparer()).ToList();
            //These appear to all match. The below statement should notify the progammer if we need to add handling for non matching lists
            System.Diagnostics.Debug.Assert(nonMatchedModulesList.Count == 0, "Module lists do not match, implement handling for this");

            foreach (var header in list.GroupBy(_ => _.Header).OrderBy(_ => _.Key))
            {
                var headerItem = new TreeViewItem { Header = header.Key };
                foreach (var secondLevel in header.GroupBy(_ => _.Faction).OrderBy(_ => _.Key))
                {
                    var secondLevelItem = new TreeViewItem { Header = TreeLocalization.TreeLocalization.GetTreeLocalization(secondLevel.Key) };

                    foreach (var thirdLevel in secondLevel.Where(_ => _.Group != null).GroupBy(_ => _.Group).OrderBy(_ => _.Key))
                    {
                        var thirdLevelItem = new TreeViewItem { Header = TreeLocalization.TreeLocalization.GetTreeLocalization(thirdLevel.Key) };
                        foreach (var buildingInfo in thirdLevel.OrderBy(_ => _.GetOrderParameter()))
                        {
                            thirdLevelItem.Items.Add(buildingInfo.ToAnnoObject());
                        }
                        //For 2205 only
                        //Add building modules to element list.
                        //Group will be the same for elements in the list.
                        if (header.Key == "(A6) Anno 2205")
                        {
                            var fourthLevelItem = new TreeViewItem { Header = TreeLocalization.TreeLocalization.GetTreeLocalization(thirdLevel.ElementAt(0).Group) + " " + TreeLocalization.TreeLocalization.GetTreeLocalization("Modules") };
                            foreach (var fourthLevel in modulesList
                                .Where(_ => _.Group == thirdLevel.ElementAt(0).Group))
                            {
                                fourthLevelItem.Items.Add(fourthLevel.ToAnnoObject());
                            }
                            if (fourthLevelItem.Items.Count > 0)
                            {
                                thirdLevelItem.Items.Add(fourthLevelItem);
                            }
                        }
                        secondLevelItem.Items.Add(thirdLevelItem);
                    }
                    foreach (var thirdLevel in secondLevel.Where(_ => _.Group == null).OrderBy(_ => _.GetOrderParameter()))
                    {
                        secondLevelItem.Items.Add(thirdLevel.ToAnnoObject());
                    }

                    headerItem.Items.Add(secondLevelItem);
                }
                treeView.Items.Add(headerItem);
            }
        }
    }
}
