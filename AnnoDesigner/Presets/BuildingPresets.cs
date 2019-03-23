using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Controls;

namespace AnnoDesigner.Presets
{
    /// <summary>
    /// Notes:
    /// some radii are curiously missing, e.g. coffee plantation
    /// </summary>
    [DataContract]
    public class BuildingPresets
    {
        [DataMember]
        public string Version;

        [DataMember]
        public List<BuildingInfo> Buildings;

        public void AddToTree(TreeView treeView)
        {
            var excludedTemplates = new[] { "Ark", "Harbour", "OrnamentBuilding" };
            var excludedFactions = new[] { "third party" };
            var list = Buildings.Where(_ => !excludedTemplates.Contains(_.Template)).Where(_ => !excludedFactions.Contains(_.Faction));
            foreach (var firstLevel in list.GroupBy(_ => _.Faction).OrderBy(_ => _.Key))
            {
                var firstLevelItem = new TreeViewItem { Header = firstLevel.Key };
                foreach (var secondLevel in firstLevel.GroupBy(_ => _.Group).OrderBy(_ => _.Key))
                {
                    var secondLevelItem = new TreeViewItem { Header = secondLevel.Key };
                    foreach (var buildingInfo in secondLevel.OrderBy(_ => _.GetOrderParameter()))
                    {
                        secondLevelItem.Items.Add(buildingInfo.ToAnnoObject());
                    }
                    firstLevelItem.Items.Add(secondLevelItem);
                }
                treeView.Items.Add(firstLevelItem);
            }
        }
    }
}