using AnnoDesigner.Core.Layout.Helper;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Core.Presets.Models;
using AnnoDesigner.Extensions;
using AnnoDesigner.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace AnnoDesigner.ViewModels;

public class StatisticsViewModel : Notify
{
    private bool _isVisible;
    private string _usedArea;
    private double _usedTiles;
    private double _minTiles;
    private string _efficiency;
    private bool _areStatisticsAvailable;
    private bool _showBuildingList;
    private bool _showStatisticsBuildingCount;
    //private bool _showSelectedBuildingList;
    private ObservableCollection<StatisticsBuilding> _buildings;
    private ObservableCollection<StatisticsBuilding> _selectedBuildings;
    private readonly StatisticsCalculationHelper _statisticsCalculationHelper;
    private readonly ConcurrentDictionary<string, BuildingInfo> _cachedPresetsBuilding;
    private readonly ILocalizationHelper _localizationHelper;
    private readonly ICommons _commons;
    private readonly IAppSettings _appSettings;

    public StatisticsViewModel(ILocalizationHelper localizationHelperToUse, ICommons commonsToUse, IAppSettings appSettingsToUse)
    {
        _localizationHelper = localizationHelperToUse;
        _commons = commonsToUse;
        _appSettings = appSettingsToUse;

        UsedArea = "12x4";
        UsedTiles = 308;
        MinTiles = 48;
        Efficiency = "16%";
        AreStatisticsAvailable = true;

        ShowBuildingList = true;
        Buildings = [];
        SelectedBuildings = [];
        _statisticsCalculationHelper = new StatisticsCalculationHelper();
        _cachedPresetsBuilding = new ConcurrentDictionary<string, BuildingInfo>(Environment.ProcessorCount, 50);
    }

    public bool IsVisible
    {
        get => _isVisible;
        set => UpdateProperty(ref _isVisible, value);
    }

    public string UsedArea
    {
        get => _usedArea;
        set => UpdateProperty(ref _usedArea, value);
    }

    public double UsedTiles
    {
        get => _usedTiles;
        set => UpdateProperty(ref _usedTiles, value);
    }

    public double MinTiles
    {
        get => _minTiles;
        set => UpdateProperty(ref _minTiles, value);
    }

    public string Efficiency
    {
        get => _efficiency;
        set => UpdateProperty(ref _efficiency, value);
    }

    public bool AreStatisticsAvailable
    {
        get => _areStatisticsAvailable;
        set => UpdateProperty(ref _areStatisticsAvailable, value);
    }

    public bool ShowBuildingList
    {
        get => _showBuildingList;
        set
        {
            _ = UpdateProperty(ref _showBuildingList, value);
            OnPropertyChanged(nameof(ShowSelectedBuildingList));
        }
    }

    public bool ShowStatisticsBuildingCount
    {
        get => _showStatisticsBuildingCount;
        set => UpdateProperty(ref _showStatisticsBuildingCount, value);
    }

    public bool ShowSelectedBuildingList => ShowBuildingList && SelectedBuildings.Any();

    public ObservableCollection<StatisticsBuilding> Buildings
    {
        get => _buildings;
        set => UpdateProperty(ref _buildings, value);
    }

    public ObservableCollection<StatisticsBuilding> SelectedBuildings
    {
        get => _selectedBuildings;
        set
        {
            _ = UpdateProperty(ref _selectedBuildings, value);
            OnPropertyChanged(nameof(ShowSelectedBuildingList));
        }
    }

    public void ToggleBuildingList(bool showBuildingList, IList<LayoutObject> placedObjects, ICollection<LayoutObject> selectedObjects, BuildingPresets buildingPresets)
    {
        ShowBuildingList = showBuildingList;
        if (showBuildingList)
        {
            _ = UpdateStatisticsAsync(UpdateMode.All, placedObjects, selectedObjects, buildingPresets);
        }
    }

    public async Task UpdateStatisticsAsync(UpdateMode mode,
        IList<LayoutObject> placedObjects,
        ICollection<LayoutObject> selectedObjects,
        BuildingPresets buildingPresets)
    {
        if (placedObjects.Count == 0)
        {
            AreStatisticsAvailable = false;
            return;
        }

        AreStatisticsAvailable = true;

        Task<Core.Layout.Models.StatisticsCalculationResult> calculateStatisticsTask = Task.Run(() => _statisticsCalculationHelper.CalculateStatistics(placedObjects.Select(_ => _.WrappedAnnoObject), includeRoads: _appSettings.IncludeRoadsInStatisticCalculation));

        if (mode != UpdateMode.NoBuildingList && ShowBuildingList)
        {
            List<IGrouping<string, LayoutObject>> groupedPlacedBuildings = placedObjects.GroupBy(_ => _.Identifier).ToList();

            IEnumerable<IGrouping<string, LayoutObject>> groupedSelectedBuildings = null;
            if (selectedObjects != null && selectedObjects.Count > 0)
            {
                groupedSelectedBuildings = selectedObjects.Where(_ => _ != null).GroupBy(_ => _.Identifier).ToList();
            }

            Task<ObservableCollection<StatisticsBuilding>> buildingsTask = Task.Run(() => GetStatisticBuildings(groupedPlacedBuildings, buildingPresets));
            Task<ObservableCollection<StatisticsBuilding>> selectedBuildingsTask = Task.Run(() => GetStatisticBuildings(groupedSelectedBuildings, buildingPresets));
            SelectedBuildings = await selectedBuildingsTask;
            Buildings = await buildingsTask;
        }

        Core.Layout.Models.StatisticsCalculationResult calculatedStatistics = await calculateStatisticsTask;

        UsedArea = string.Format("{0}x{1}", calculatedStatistics.UsedAreaWidth, calculatedStatistics.UsedAreaHeight);
        UsedTiles = calculatedStatistics.UsedTiles;
        MinTiles = calculatedStatistics.MinTiles;
        Efficiency = string.Format("{0}%", calculatedStatistics.Efficiency);
    }

    private ObservableCollection<StatisticsBuilding> GetStatisticBuildings(IEnumerable<IGrouping<string, LayoutObject>> groupedBuildingsByIdentifier, BuildingPresets buildingPresets)
    {
        if (groupedBuildingsByIdentifier is null || !groupedBuildingsByIdentifier.Any())
        {
            return [];
        }

        List<StatisticsBuilding> tempList = [];

        IOrderedEnumerable<IGrouping<string, LayoutObject>> validBuildingsGrouped = groupedBuildingsByIdentifier
                    .Where(_ => (!_.ElementAt(0).WrappedAnnoObject.Road || _appSettings.IncludeRoadsInStatisticCalculation) && _.ElementAt(0).Identifier != null)
                    .Where(x => x.AsEnumerable().WithoutIgnoredObjects().Count() > 0)
                    .OrderByDescending(_ => _.Count());
        foreach (IGrouping<string, LayoutObject> item in validBuildingsGrouped)
        {
            StatisticsBuilding statisticBuilding = new();

            string identifierToCheck = item.ElementAt(0).Identifier;
            if (!string.IsNullOrWhiteSpace(identifierToCheck))
            {
                //try to find building in presets by identifier
                if (!_cachedPresetsBuilding.TryGetValue(identifierToCheck, out BuildingInfo building))
                {
                    building = buildingPresets.Buildings.Find(_ => string.Equals(_.Identifier, identifierToCheck, StringComparison.OrdinalIgnoreCase));
                    _ = _cachedPresetsBuilding.TryAdd(identifierToCheck, building);
                }

                bool isUnknownObject = string.Equals(identifierToCheck, "Unknown Object", StringComparison.OrdinalIgnoreCase);
                if (building != null || isUnknownObject)
                {
                    statisticBuilding.Count = item.Count();
                    statisticBuilding.Name = isUnknownObject ? _localizationHelper.GetLocalization("UnknownObject") : building.Localization[_commons.CurrentLanguageCode];
                }
                else
                {
                    /// Ruled those 2 out to keep Building Name Changes done through the Labeling of the building
                    /// and when the building is not in the preset. Those statisticBuildings.name will not translated to
                    /// other luangages anymore, as users can give there own names.
                    /// However i made it so, that if localizations get those translations, it will translated.
                    /// 06-02-2021, on request of user(s) on Discord read this on
                    /// https://discord.com/channels/571011757317947406/571011757317947410/800118895855665203
                    //item.ElementAt(0).Identifier = "";
                    //statisticBuilding.Name = _localizationHelper.GetLocalization("StatNameNotFound");

                    statisticBuilding.Count = item.Count();
                    statisticBuilding.Name = _localizationHelper.GetLocalization(item.ElementAt(0).Identifier);
                }
            }
            else
            {
                statisticBuilding.Count = item.Count();
                statisticBuilding.Name = _localizationHelper.GetLocalization("StatNameNotFound");
            }

            tempList.Add(statisticBuilding);
        }

        return new ObservableCollection<StatisticsBuilding>(tempList.OrderByDescending(x => x.Count).ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase));
    }
}
