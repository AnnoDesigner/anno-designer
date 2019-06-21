using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using AnnoDesigner.Core;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Core.Presets.Comparer;
using AnnoDesigner.Core.Presets.Models;
using AnnoDesigner.model;

namespace AnnoDesigner.viewmodel
{
    public class PresetsTreeViewModel : Notify
    {
        private ObservableCollection<GenericTreeItem> _items;
        private GenericTreeItem _selectedItem;

        public PresetsTreeViewModel()
        {
            Items = new ObservableCollection<GenericTreeItem>();
        }

        public ObservableCollection<GenericTreeItem> Items
        {
            get { return _items; }
            set { UpdateProperty(ref _items, value); }
        }

        public GenericTreeItem SelectedItem
        {
            get { return _selectedItem; }
            set { UpdateProperty(ref _selectedItem, value); }
        }

        public void LoadItems(BuildingPresets buildingPresets)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            //manually add roads
            var roadTiles = GetRoadTiles();
            foreach (var curRoad in roadTiles)
            {
                Items.Add(curRoad);
            }

            //prepare data
            var headerAnno2205 = "(A6) Anno 2205";

            var excludedTemplates = new[] { "Ark", "Harbour", "OrnamentBuilding" };
            var excludedFactions = new[] { "third party", "Facility Modules" };

            var filteredBuildingList = buildingPresets.Buildings
                .Where(x => !excludedTemplates.Contains(x.Template) &&
                !excludedFactions.Contains(x.Faction));

            //For Anno 2205 only
            var modulesList = buildingPresets.Buildings
                .Where(x => string.Equals(x.Header, headerAnno2205, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(x.Faction, "Facility Modules", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(x.Faction, "Facilities", StringComparison.OrdinalIgnoreCase))
                .ToList();

            #region some checks

            var facilityList = filteredBuildingList.Where(x => string.Equals(x.Faction, "Facilities", StringComparison.OrdinalIgnoreCase)).ToList();

            //Get a list of nonMatchedModules;
            var nonMatchedModulesList = modulesList.Except(facilityList, new BuildingInfoComparer()).ToList();
            //These appear to all match. The below statement should notify the progammer if we need to add handling for non matching lists
            System.Diagnostics.Debug.Assert(nonMatchedModulesList.Count == 0, "Module lists do not match, implement handling for this");

            #endregion

            //add data to tree
            var groupedGames = filteredBuildingList.GroupBy(x => x.Header).OrderBy(x => x.Key);
            foreach (var curGame in groupedGames)
            {
                var gameItem = new GameHeaderTreeItem
                {
                    Header = curGame.Key,
                    GameVersion = GetGameVersion(curGame.Key)
                };

                var groupedFactions = curGame.GroupBy(x => x.Faction).OrderBy(x => x.Key);
                foreach (var curFaction in groupedFactions)
                {
                    var factionItem = new GenericTreeItem
                    {
                        Header = TreeLocalization.TreeLocalization.GetTreeLocalization(curFaction.Key)
                    };

                    var groupedGroups = curFaction.Where(x => x.Group != null).GroupBy(x => x.Group).OrderBy(x => x.Key);
                    foreach (var curGroup in groupedGroups)
                    {
                        var groupItem = new GenericTreeItem
                        {
                            Header = TreeLocalization.TreeLocalization.GetTreeLocalization(curGroup.Key)
                        };

                        foreach (var curBuildingInfo in curGroup.OrderBy(x => x.GetOrderParameter()))
                        {
                            groupItem.Children.Add(new GenericTreeItem
                            {
                                Header = curBuildingInfo.ToAnnoObject().Label,
                                AnnoObject = curBuildingInfo.ToAnnoObject()
                            });
                        }

                        //For 2205 only
                        //Add building modules to element list.
                        //Group will be the same for elements in the list.
                        if (string.Equals(curGame.Key, headerAnno2205, StringComparison.OrdinalIgnoreCase))
                        {
                            var moduleItem = new GenericTreeItem
                            {
                                Header = TreeLocalization.TreeLocalization.GetTreeLocalization(curGroup.ElementAt(0).Group) + " " + TreeLocalization.TreeLocalization.GetTreeLocalization("Modules")
                            };

                            foreach (var fourthLevel in modulesList.Where(x => x.Group == curGroup.ElementAt(0).Group))
                            {
                                moduleItem.Children.Add(new GenericTreeItem
                                {
                                    Header = fourthLevel.ToAnnoObject().Label,
                                    AnnoObject = fourthLevel.ToAnnoObject()
                                });
                            }

                            if (moduleItem.Children.Count > 0)
                            {
                                groupItem.Children.Add(moduleItem);
                            }
                        }

                        factionItem.Children.Add(groupItem);
                    }

                    var groupedFactionBuildings = curFaction.Where(x => x.Group == null).OrderBy(x => x.GetOrderParameter());
                    foreach (var curGroup in groupedFactionBuildings)
                    {
                        factionItem.Children.Add(new GenericTreeItem
                        {
                            Header = curGroup.ToAnnoObject().Label,
                            AnnoObject = curGroup.ToAnnoObject()
                        });
                    }

                    gameItem.Children.Add(factionItem);
                }

                Items.Add(gameItem);
            }

            sw.Stop();
            Debug.WriteLine($"<< New >> loading TreeItems took: {sw.ElapsedMilliseconds}ms");
        }

        private List<GenericTreeItem> GetRoadTiles()
        {
            var result = new List<GenericTreeItem>();

            result.Add(new GenericTreeItem
            {
                Header = TreeLocalization.TreeLocalization.GetTreeLocalization("RoadTile"),
                AnnoObject = new AnnoObject
                {
                    Label = TreeLocalization.TreeLocalization.GetTreeLocalization("RoadTile"),
                    Size = new Size(1, 1),
                    Radius = 0,
                    Road = true,
                    Identifier = "Road",
                    Template = "Road"
                }
            });

            result.Add(new GenericTreeItem
            {
                Header = TreeLocalization.TreeLocalization.GetTreeLocalization("BorderlessRoadTile"),
                AnnoObject = new AnnoObject
                {
                    Label = TreeLocalization.TreeLocalization.GetTreeLocalization("BorderlessRoadTile"),
                    Size = new Size(1, 1),
                    Radius = 0,
                    Borderless = true,
                    Road = true,
                    Identifier = "Road",
                    Template = "Road"
                }
            });

            return result;
        }

        private CoreConstants.GameVersion GetGameVersion(string gameHeader)
        {
            var result = CoreConstants.GameVersion.Unknown;

            switch (gameHeader)
            {
                case "(A4) Anno 1400":
                    result = CoreConstants.GameVersion.Anno1404;
                    break;
                case "(A5) Anno 2070":
                    result = CoreConstants.GameVersion.Anno2070;
                    break;
                case "(A6) Anno 2205":
                    result = CoreConstants.GameVersion.Anno2205;
                    break;
                case "(A7) Anno 1800":
                    result = CoreConstants.GameVersion.Anno1800;
                    break;
            }

            return result;
        }
    }
}