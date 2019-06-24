using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using AnnoDesigner.Core;
using AnnoDesigner.Core.Extensions;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Core.Presets.Comparer;
using AnnoDesigner.Core.Presets.Models;
using AnnoDesigner.model;
using AnnoDesigner.model.PresetsTree;

namespace AnnoDesigner.viewmodel
{
    public class PresetsTreeViewModel : Notify
    {
        public event EventHandler ApplySelectedItem;

        private ObservableCollection<GenericTreeItem> _items;
        private ICollectionView _filteredItems;
        private GenericTreeItem _selectedItem;
        private string _buildingPresetsVersion;
        private string _filterText;

        public PresetsTreeViewModel()
        {
            Items = new ObservableCollection<GenericTreeItem>();
            FilteredItems = CollectionViewSource.GetDefaultView(Items);

            DoubleClickCommand = new RelayCommand(DoubleClick, null);
            //SelectedItemChangedCommand = new RelayCommand(SelectedItemChanged, null);
            ReturnKeyPressedCommand = new RelayCommand(ReturnKeyPressed, null);
            BuildingPresetsVersion = string.Empty;
            FilterText = string.Empty;
        }

        public ObservableCollection<GenericTreeItem> Items
        {
            get { return _items; }
            set { UpdateProperty(ref _items, value); }
        }

        public ICollectionView FilteredItems
        {
            get { return _filteredItems; }
            set { UpdateProperty(ref _filteredItems, value); }
        }

        public GenericTreeItem SelectedItem
        {
            get { return _selectedItem; }
            private set { UpdateProperty(ref _selectedItem, value); }
        }

        public string BuildingPresetsVersion
        {
            get { return _buildingPresetsVersion; }
            private set { UpdateProperty(ref _buildingPresetsVersion, value); }
        }

        public string FilterText
        {
            get { return _filterText; }
            set
            {
                if (UpdateProperty(ref _filterText, value))
                {
                    MyFilter();
                }
            }
        }

        #region commands

        public ICommand DoubleClickCommand { get; private set; }

        private void DoubleClick(object param)
        {
            var selectedItem = param as GenericTreeItem;
            if (selectedItem?.AnnoObject != null)
            {
                SelectedItem = selectedItem;
            }

            //call ApplyPreset();
            ApplySelectedItem?.Invoke(this, EventArgs.Empty);
        }

        //public ICommand SelectedItemChangedCommand { get; private set; }

        //private void SelectedItemChanged(object param)
        //{
        //    var selectedItem = param as GenericTreeItem;
        //    if (selectedItem != null)
        //    {
        //        SelectedItem = selectedItem;
        //    }
        //}

        public ICommand ReturnKeyPressedCommand { get; private set; }

        private void ReturnKeyPressed(object param)
        {
            var selectedItem = param as GenericTreeItem;
            if (selectedItem?.AnnoObject != null)
            {
                SelectedItem = selectedItem;
            }

            //call ApplyPreset();
            ApplySelectedItem?.Invoke(this, EventArgs.Empty);
        }

        #endregion

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
                    var factionItem = new GenericTreeItem(gameItem)
                    {
                        Header = TreeLocalization.TreeLocalization.GetTreeLocalization(curFaction.Key)
                    };

                    var groupedGroups = curFaction.Where(x => x.Group != null).GroupBy(x => x.Group).OrderBy(x => x.Key);
                    foreach (var curGroup in groupedGroups)
                    {
                        var groupItem = new GenericTreeItem(factionItem)
                        {
                            Header = TreeLocalization.TreeLocalization.GetTreeLocalization(curGroup.Key)
                        };

                        foreach (var curBuildingInfo in curGroup.OrderBy(x => x.GetOrderParameter()))
                        {
                            groupItem.Children.Add(new GenericTreeItem(groupItem)
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
                            var moduleItem = new GenericTreeItem(groupItem)
                            {
                                Header = TreeLocalization.TreeLocalization.GetTreeLocalization(curGroup.ElementAt(0).Group) + " " + TreeLocalization.TreeLocalization.GetTreeLocalization("Modules")
                            };

                            foreach (var fourthLevel in modulesList.Where(x => x.Group == curGroup.ElementAt(0).Group))
                            {
                                moduleItem.Children.Add(new GenericTreeItem(moduleItem)
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
                        factionItem.Children.Add(new GenericTreeItem(factionItem)
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

            BuildingPresetsVersion = buildingPresets.Version;

            //to test things
            //Items[3].IsVisible = false;
            //Items[2].Children[1].Children[0].IsExpanded = true;
            //FilterText = "Fire";           
        }

        private List<GenericTreeItem> GetRoadTiles()
        {
            var result = new List<GenericTreeItem>();

            result.Add(new GenericTreeItem(null)
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

            result.Add(new GenericTreeItem(null)
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

        /// <summary>
        /// Returns the current state of all expanded nodes in the tree.
        /// </summary>
        /// <returns></returns>
        public List<bool> GetTreeState()
        {
            var result = new List<bool>();

            foreach (var curNode in Items)
            {
                GetTreeState(curNode, result);
            }

            return result;
        }

        private List<bool> GetTreeState(GenericTreeItem node, List<bool> result)
        {
            if (!node.Children.Any())
            {
                return result;
            }

            if (!node.IsExpanded)
            {
                result.Add(false);

                return result;
            }

            result.Add(true);

            foreach (var curChildNode in node.Children)
            {
                GetTreeState(curChildNode, result);
            }

            return result;
        }

        /// <summary>
        /// Sets the expanded state of all node in the tree.
        /// </summary>
        /// <param name="savedTreeState">The saved state of all expanded nodes.</param>
        /// <param name="lastBuildingPresetsVersion">The last presets version used to save the state.</param>
        public void SetTreeState(List<bool> savedTreeState, string lastBuildingPresetsVersion)
        {
            //presets version does not match
            if (!string.Equals(BuildingPresetsVersion, lastBuildingPresetsVersion, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            //no saved tree state present
            if (savedTreeState == null || savedTreeState.Count <= 0)
            {
                return;
            }

            var currentIndex = -1;
            foreach (var curNode in Items)
            {
                currentIndex = SetTreeState(curNode, savedTreeState, currentIndex);
            }
        }

        private int SetTreeState(GenericTreeItem node, List<bool> savedTreeState, int currentIndex)
        {
            if (!node.Children.Any())
            {
                return currentIndex;
            }

            if (!savedTreeState[currentIndex + 1])
            {
                return ++currentIndex;
            }

            ++currentIndex;

            node.IsExpanded = savedTreeState[currentIndex];

            foreach (var curChildNode in node.Children)
            {
                currentIndex = SetTreeState(curChildNode, savedTreeState, currentIndex);
            }

            return currentIndex;
        }

        private void MyFilter()
        {
            foreach (GenericTreeItem curItem in FilteredItems)
            {
                //short circuit items without children
                if (!curItem.Children.Any())
                {
                    if (string.IsNullOrWhiteSpace(FilterText))
                    {
                        curItem.IsVisible = true;
                        curItem.IsExpanded = false;
                    }
                    else
                    {
                        var matches = curItem.Header.Contains(FilterText, StringComparison.OrdinalIgnoreCase);
                        curItem.IsVisible = matches;
                        curItem.IsExpanded = false;
                    }

                    continue;
                }

                //check child items
                var anyChildMatches = false;
                foreach (var curChild in curItem.Children)
                {
                    var childMatches = MyFilter(curChild);
                    if (childMatches)
                    {
                        anyChildMatches = true;
                    }
                }

                //no child matches -> hide item
                if (!anyChildMatches)
                {
                    curItem.IsVisible = false;
                    curItem.IsExpanded = false;
                }
                else
                {
                    curItem.IsVisible = true;
                    //only expand if there is a search, else it is a "reset" of the tree
                    curItem.IsExpanded = !string.IsNullOrWhiteSpace(FilterText);
                }
            }
        }

        private bool MyFilter(GenericTreeItem curItem)
        {
            //short circuit items without children
            if (!curItem.Children.Any())
            {
                if (string.IsNullOrWhiteSpace(FilterText))
                {
                    curItem.IsVisible = true;
                    curItem.IsExpanded = false;
                }
                else
                {
                    var matches = curItem.Header.Contains(FilterText, StringComparison.OrdinalIgnoreCase);
                    curItem.IsVisible = matches;
                    curItem.IsExpanded = false;
                }

                return curItem.IsVisible;
            }

            //check child items
            var anyChildMatches = false;
            foreach (var curChild in curItem.Children)
            {
                var childMatches = MyFilter(curChild);
                if (childMatches)
                {
                    anyChildMatches = true;
                }
            }

            //no child matches -> hide item
            if (!anyChildMatches)
            {
                curItem.IsVisible = false;
                curItem.IsExpanded = false;
            }
            else
            {
                curItem.IsVisible = true;
                //only expand if there is a search, else it is a "reset" of the tree
                curItem.IsExpanded = !string.IsNullOrWhiteSpace(FilterText);
            }

            return curItem.IsVisible;
        }
    }
}