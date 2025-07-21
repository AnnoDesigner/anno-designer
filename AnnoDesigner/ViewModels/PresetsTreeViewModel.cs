using AnnoDesigner.Core;
using AnnoDesigner.Core.Extensions;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Core.Presets.Comparer;
using AnnoDesigner.Core.Presets.Models;
using AnnoDesigner.Extensions;
using AnnoDesigner.Models;
using AnnoDesigner.Models.PresetsTree;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace AnnoDesigner.ViewModels;

public class PresetsTreeViewModel : Notify
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    public event EventHandler<EventArgs> ApplySelectedItem;

    private readonly ILocalizationHelper _localizationHelper;
    private readonly ICommons _commons;
    private ObservableCollection<GenericTreeItem> _items;
    private ICollectionView _filteredItems;
    private GenericTreeItem _selectedItem;
    private string _buildingPresetsVersion;
    private string _filterText;
    private CoreConstants.GameVersion _filterGameVersion;

    public PresetsTreeViewModel(ILocalizationHelper localizationHelperToUse, ICommons commonsToUse)
    {
        _localizationHelper = localizationHelperToUse;
        _commons = commonsToUse;

        Items = [];
        FilteredItems = CollectionViewSource.GetDefaultView(Items);

        DoubleClickCommand = new RelayCommand(DoubleClick, null);
        //SelectedItemChangedCommand = new RelayCommand(SelectedItemChanged, null);
        ReturnKeyPressedCommand = new RelayCommand(ReturnKeyPressed, null);
        BuildingPresetsVersion = string.Empty;
        FilterGameVersion = CoreConstants.GameVersion.All;
        FilterText = string.Empty;
    }

    public ObservableCollection<GenericTreeItem> Items
    {
        get => _items;
        set => UpdateProperty(ref _items, value);
    }

    public ICollectionView FilteredItems
    {
        get => _filteredItems;
        set => UpdateProperty(ref _filteredItems, value);
    }

    public GenericTreeItem SelectedItem
    {
        get => _selectedItem;
        private set => UpdateProperty(ref _selectedItem, value);
    }

    public string BuildingPresetsVersion
    {
        get => _buildingPresetsVersion;
        private set => UpdateProperty(ref _buildingPresetsVersion, value);
    }

    public string FilterText
    {
        get => _filterText;
        set
        {
            if (UpdateProperty(ref _filterText, value))
            {
                Filter();
            }
        }
    }

    public CoreConstants.GameVersion FilterGameVersion
    {
        get => _filterGameVersion;
        set
        {
            if (UpdateProperty(ref _filterGameVersion, value))
            {
                Filter();
            }
        }
    }

    #region commands

    public ICommand DoubleClickCommand { get; private set; }

    private void DoubleClick(object param)
    {
        GenericTreeItem selectedItem = param as GenericTreeItem;
        if (selectedItem?.AnnoObject != null)
        {
            SelectedItem = selectedItem;

            //call ApplyPreset();
            ApplySelectedItem?.Invoke(this, EventArgs.Empty);
        }
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
        GenericTreeItem selectedItem = param as GenericTreeItem;
        if (selectedItem?.AnnoObject != null)
        {
            SelectedItem = selectedItem;

            //call ApplyPreset();
            ApplySelectedItem?.Invoke(this, EventArgs.Empty);
        }
    }

    #endregion

    public void LoadItems(BuildingPresets buildingPresets)
    {
        if (buildingPresets == null)
        {
            throw new ArgumentNullException(nameof(buildingPresets));
        }

        BuildingPresetsVersion = buildingPresets.Version;

#if DEBUG
        Stopwatch sw = new();
        sw.Start();
#endif

        Items.Clear();

        //manually add roads
        List<GenericTreeItem> roadTiles = GetRoadTiles();
        foreach (GenericTreeItem curRoad in roadTiles)
        {
            Items.Add(curRoad);
        }

        if (!buildingPresets.Buildings.Any())
        {
            return;
        }

        //Each item needs an id for save/restore of tree state. We don't know how many roads were added.
        int itemId = Items.Count;

        //prepare data
        string headerAnno2205 = "(A6) Anno 2205";
        string[] excludedTemplates = new[] { "Ark", "DefColFace", "RoofColDef", "RoofColFace" }; //Removed "Harbour" and "OrnamentBuilding" missing buidlings in those 2 Template Groups (18-06-2022)
        string[] excludedFactions = new[] { "third party", "Facility Modules" };

        IEnumerable<BuildingInfo> filteredBuildingList = buildingPresets.Buildings
            .Where(x => !excludedTemplates.Contains(x.Template) &&
            !excludedFactions.Contains(x.Faction));

        //For Anno 2205 only
        List<BuildingInfo> modulesList = buildingPresets.Buildings
            .Where(x => string.Equals(x.Header, headerAnno2205, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(x.Faction, "Facility Modules", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(x.Faction, "Facilities", StringComparison.OrdinalIgnoreCase))
            .ToList();

        #region some checks
#if DEBUG
        List<BuildingInfo> facilityList = filteredBuildingList.Where(x => string.Equals(x.Faction, "Facilities", StringComparison.OrdinalIgnoreCase)).ToList();

        //Get a list of nonMatchedModules;
        List<IBuildingInfo> nonMatchedModulesList = modulesList.Except(facilityList, new BuildingInfoComparer()).ToList();
        //These appear to all match. The below statement should notify the progammer if we need to add handling for non matching lists
        System.Diagnostics.Debug.Assert(nonMatchedModulesList.Count == 0, "Module lists do not match, implement handling for this");
#endif
        #endregion

        //add data to tree
        IOrderedEnumerable<IGrouping<string, BuildingInfo>> groupedGames = filteredBuildingList.GroupBy(x => x.Header).OrderBy(x => x.Key);
        foreach (IGrouping<string, BuildingInfo> curGame in groupedGames)
        {
            string gameHeader = curGame.Key;
            CoreConstants.GameVersion gameVersion = GetGameVersion(curGame.Key);
            if (gameVersion == CoreConstants.GameVersion.Unknown)
            {
                gameHeader = _localizationHelper.GetLocalization(curGame.Key);
            }

            GameHeaderTreeItem gameItem = new()
            {
                Header = gameHeader,
                GameVersion = gameVersion,
                Id = ++itemId
            };

            IOrderedEnumerable<IGrouping<string, BuildingInfo>> groupedFactions = curGame.Where(x => x.Faction != null).GroupBy(x => x.Faction).OrderBy(x => x.Key);
            foreach (IGrouping<string, BuildingInfo> curFaction in groupedFactions)
            {
                GenericTreeItem factionItem = new(gameItem)
                {
                    Header = _localizationHelper.GetLocalization(curFaction.Key),
                    Id = ++itemId
                };

                IOrderedEnumerable<IGrouping<string, BuildingInfo>> groupedGroups = curFaction.Where(x => x.Group != null).GroupBy(x => x.Group).OrderBy(x => x.Key);
                foreach (IGrouping<string, BuildingInfo> curGroup in groupedGroups)
                {
                    GenericTreeItem groupItem = new(factionItem)
                    {
                        Header = _localizationHelper.GetLocalization(curGroup.Key),
                        Id = ++itemId
                    };

                    foreach (BuildingInfo curBuildingInfo in curGroup.OrderBy(x => x.GetOrderParameter(_commons.CurrentLanguageCode)))
                    {
                        groupItem.Children.Add(new GenericTreeItem(groupItem)
                        {
                            Header = curBuildingInfo.ToAnnoObject(_commons.CurrentLanguageCode).Label,
                            AnnoObject = curBuildingInfo.ToAnnoObject(_commons.CurrentLanguageCode),
                            Id = ++itemId
                        });
                    }

                    //For 2205 only
                    //Add building modules to element list.
                    //Group will be the same for elements in the list.
                    if (string.Equals(curGame.Key, headerAnno2205, StringComparison.OrdinalIgnoreCase))
                    {
                        GenericTreeItem moduleItem = new(groupItem)
                        {
                            Header = _localizationHelper.GetLocalization(curGroup.ElementAt(0).Group) + " " + _localizationHelper.GetLocalization("Modules"),
                            Id = ++itemId
                        };

                        foreach (BuildingInfo fourthLevel in modulesList.Where(x => x.Group == curGroup.ElementAt(0).Group))
                        {
                            moduleItem.Children.Add(new GenericTreeItem(moduleItem)
                            {
                                Header = fourthLevel.ToAnnoObject(_commons.CurrentLanguageCode).Label,
                                AnnoObject = fourthLevel.ToAnnoObject(_commons.CurrentLanguageCode),
                                Id = ++itemId
                            });
                        }

                        if (moduleItem.Children.Count > 0)
                        {
                            groupItem.Children.Add(moduleItem);
                        }
                    }

                    factionItem.Children.Add(groupItem);
                }

                IOrderedEnumerable<BuildingInfo> groupedFactionBuildings = curFaction.Where(x => x.Group == null).OrderBy(x => x.GetOrderParameter(_commons.CurrentLanguageCode));
                foreach (BuildingInfo curGroup in groupedFactionBuildings)
                {
                    factionItem.Children.Add(new GenericTreeItem(factionItem)
                    {
                        Header = curGroup.ToAnnoObject(_commons.CurrentLanguageCode).Label,
                        AnnoObject = curGroup.ToAnnoObject(_commons.CurrentLanguageCode),
                        Id = ++itemId
                    });
                }

                gameItem.Children.Add(factionItem);
            }

            Items.Add(gameItem);
        }

#if DEBUG
        sw.Stop();
        logger.Trace($"loading items for PresetsTree took: {sw.ElapsedMilliseconds}ms");
#endif
    }

    private List<GenericTreeItem> GetRoadTiles()
    {
        List<GenericTreeItem> result = [];

        //Each item needs an id for save/restore of tree state.
        int itemId = -1;

        result.Add(new GenericTreeItem(null)
        {
            Header = _localizationHelper.GetLocalization("RoadTile"),
            AnnoObject = new AnnoObject
            {
                Label = _localizationHelper.GetLocalization("RoadTile"),
                Size = new Size(1, 1),
                Radius = 0,
                Road = true,
                Identifier = "Road",
                Template = "Road"
            },
            Id = ++itemId
        });

        result.Add(new GenericTreeItem(null)
        {
            Header = _localizationHelper.GetLocalization("BorderlessRoadTile"),
            AnnoObject = new AnnoObject
            {
                Label = _localizationHelper.GetLocalization("BorderlessRoadTile"),
                Size = new Size(1, 1),
                Radius = 0,
                Borderless = true,
                Road = true,
                Identifier = "Road",
                Template = "Road"
            },
            Id = ++itemId
        });

        return result;
    }

    private CoreConstants.GameVersion GetGameVersion(string gameHeader)
    {
        CoreConstants.GameVersion result = CoreConstants.GameVersion.Unknown;

        switch (gameHeader)
        {
            case "(A4) Anno 1404":
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
    public Dictionary<int, bool> GetCondensedTreeState()
    {
        Dictionary<int, bool> result = [];

        foreach (GenericTreeItem curNode in Items)
        {
            _ = GetCondensedTreeState(curNode, result);
        }

        return result;
    }

    private Dictionary<int, bool> GetCondensedTreeState(GenericTreeItem node, Dictionary<int, bool> result)
    {
        if (!node.Children.Any())
        {
            return result;
        }

        if (!node.IsExpanded)
        {
            result.Add(node.Id, false);

            return result;
        }

        result.Add(node.Id, true);

        foreach (GenericTreeItem curChildNode in node.Children)
        {
            _ = GetCondensedTreeState(curChildNode, result);
        }

        return result;
    }

    /// <summary>
    /// Sets the expanded state of all node in the tree.
    /// </summary>
    /// <param name="savedTreeState">The saved state of all expanded nodes.</param>
    /// <param name="lastBuildingPresetsVersion">The last presets version used to save the state.</param>
    public void SetCondensedTreeState(Dictionary<int, bool> savedTreeState, string lastBuildingPresetsVersion)
    {
        //presets version does not match
        if (string.IsNullOrWhiteSpace(lastBuildingPresetsVersion) || !string.Equals(BuildingPresetsVersion, lastBuildingPresetsVersion, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        //no saved tree state present
        if (savedTreeState == null || savedTreeState.Count <= 0)
        {
            return;
        }


        foreach (GenericTreeItem curNode in Items)
        {
            SetCondensedTreeState(curNode, savedTreeState);
        }
    }

    private void SetCondensedTreeState(GenericTreeItem node, Dictionary<int, bool> savedTreeState)
    {
        if (!node.Children.Any())
        {
            return;
        }

        if (!savedTreeState.ContainsKey(node.Id))
        {
            return;
        }

        node.IsExpanded = savedTreeState[node.Id];

        foreach (GenericTreeItem curChildNode in node.Children)
        {
            SetCondensedTreeState(curChildNode, savedTreeState);
        }
    }

    private void Filter()
    {
        FilterByGameVersion(FilterGameVersion);

        foreach (GenericTreeItem curItem in FilteredItems)
        {
            //short circuit hidden game items
            if (curItem is GameHeaderTreeItem curGameItem && !curGameItem.IsVisible)
            {
                continue;
            }

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
                    bool matches = curItem.Header.Contains(FilterText, StringComparison.OrdinalIgnoreCase);
                    curItem.IsVisible = matches;
                    curItem.IsExpanded = false;
                }

                continue;
            }

            //check child items
            bool anyChildMatches = false;
            foreach (GenericTreeItem curChild in curItem.Children)
            {
                bool childMatches = Filter(curChild);
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

    private bool Filter(GenericTreeItem curItem)
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
                bool matches = curItem.Header.Contains(FilterText, StringComparison.OrdinalIgnoreCase);
                curItem.IsVisible = matches;
                curItem.IsExpanded = false;
            }

            return curItem.IsVisible;
        }

        //check child items
        bool anyChildMatches = false;
        foreach (GenericTreeItem curChild in curItem.Children)
        {
            bool childMatches = Filter(curChild);
            if (childMatches)
            {
                anyChildMatches = true;
            }
        }

        //no child matches -> check if header of current node is matching
        if (!anyChildMatches)
        {
            bool currentHeaderMatches = curItem.Header.Contains(FilterText, StringComparison.OrdinalIgnoreCase);
            if (currentHeaderMatches)
            {
                foreach (GenericTreeItem curChild in curItem.Children)
                {
                    curChild.IsVisible = true;
                }

                curItem.IsVisible = true;
                //do not expand to avoid clutter in the view
                curItem.IsExpanded = false;
            }
            //no child matches -> hide item
            else
            {
                curItem.IsVisible = false;
                curItem.IsExpanded = false;
            }
        }
        else
        {
            curItem.IsVisible = true;
            //only expand if there is a search, else it is a "reset" of the tree
            curItem.IsExpanded = !string.IsNullOrWhiteSpace(FilterText);
        }

        return curItem.IsVisible;
    }

    private void FilterByGameVersion(CoreConstants.GameVersion versionsToUse)
    {
        foreach (GameHeaderTreeItem curGameItem in FilteredItems.OfType<GameHeaderTreeItem>())
        {
            if (versionsToUse == CoreConstants.GameVersion.All)
            {
                curGameItem.IsVisible = true;

                continue;
            }

            curGameItem.IsVisible = versionsToUse.HasFlag(curGameItem.GameVersion);
        }
    }
}