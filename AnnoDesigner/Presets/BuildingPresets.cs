using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Controls;
using AnnoDesigner;

namespace AnnoDesigner.Presets
{
    /// <summary>
    /// Notes:
    /// some radii are curiously missing, e.g. coffee plantation
    /// </summary>
    [DataContract]
    public class BuildingPresets
    {
        [DataMember(Order = 0)]
        public string Version { get; set; }

        [DataMember(Order = 1)]
        public List<BuildingInfo> Buildings { get; set; }

        public void AddToTree(TreeView treeView)
        {
            var excludedTemplates = new[] { "Ark", "Harbour", "OrnamentBuilding" };
            var excludedFactions = new[] { "third party", "Facility Modules" };
            var list = Buildings
                .Where(_ => !excludedTemplates.Contains(_.Template))
                .Where(_ => !excludedFactions.Contains(_.Faction));

            //For Anno 2205 only
            var modulesList = Buildings
                            .Where(_ => _.Header == "(A6) Anno 2205")
                            .Where(_ => _.Faction == "Facility Modules")
                            .Where(_ => _.Faction != "Facilities")
                            .ToList();
            var facilityList = list.Where(_ => _.Faction == "Facilities").ToList();
            //Get a list of nonMatchedModules;
            var nonMatchedModulesList = modulesList.Except(facilityList, new BuildingInfoModuleComparer()).ToList();
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
                    foreach (var thirdLevel in secondLevel.Where(_ => _.Group == null).OrderBy(_ => _.Group))
                    {
                        secondLevelItem.Items.Add(thirdLevel.ToAnnoObject());
                    }

                    headerItem.Items.Add(secondLevelItem);
                }
                treeView.Items.Add(headerItem);
            }
        }
    }

    /// <summary>
    /// Comparer used to check if two BuildingInfo groups match
    /// </summary>
    public class BuildingInfoModuleComparer : IEqualityComparer<BuildingInfo>
    {
        public bool Equals(BuildingInfo x, BuildingInfo y)
        {

            //Check whether the compared objects reference the same data.
            if (Object.ReferenceEquals(x, y)) return true;

            //Check whether any of the compared objects is null.
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            //Check whether the buildingInfo group properties are equal
            return x.Group == y.Group;
        }
        public int GetHashCode(BuildingInfo obj)
        {
            return base.GetHashCode();
        }
    }
}