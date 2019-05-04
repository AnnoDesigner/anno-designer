using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using AnnoDesigner.model;
using AnnoDesigner.Presets;

namespace AnnoDesigner.viewmodel
{
    public class StatisticsViewModel : Notify
    {
        private string _textNothingPlaced;
        private string _textBoundingBox;
        private string _textMinimumArea;
        private string _textSpaceEfficiency;
        private string _textBuildings;
        private string _textBuildingsSelected;
        private string _textTiles;
        private string _textNameNotFound;

        private string _usedArea;
        private double _usedTiles;
        private double _minTiles;
        private string _efficiency;
        private bool _areStatisticsAvailable;
        private bool _showBuildingList;
        private bool _showSelectedBuildingList;
        private ObservableCollection<StatisticsBuilding> _buildings;
        private ObservableCollection<StatisticsBuilding> _selectedBuildings;

        public StatisticsViewModel()
        {
            TextNothingPlaced = "Nothing Placed";
            TextBoundingBox = "Bounding Box";
            TextMinimumArea = "Minimum Area";
            TextSpaceEfficiency = "Space Efficiency";
            TextBuildings = "Buildings";
            TextBuildingsSelected = "Buildings Selected";
            TextTiles = "Tiles";
            TextNameNotFound = "Building name not found";

            UsedArea = "12x4";
            UsedTiles = 308;
            MinTiles = 48;
            Efficiency = "16%";
            AreStatisticsAvailable = true;

            ShowBuildingList = true;
            Buildings = new ObservableCollection<StatisticsBuilding>();
            SelectedBuildings = new ObservableCollection<StatisticsBuilding>();
        }

        #region localization

        public string TextNothingPlaced
        {
            get { return _textNothingPlaced; }
            set { UpdateProperty(ref _textNothingPlaced, value); }
        }

        public string TextBoundingBox
        {
            get { return _textBoundingBox; }
            set
            {
                UpdateProperty(ref _textBoundingBox, value);
            }
        }

        public string TextMinimumArea
        {
            get { return _textMinimumArea; }
            set
            {
                UpdateProperty(ref _textMinimumArea, value);
            }
        }

        public string TextSpaceEfficiency
        {
            get { return _textSpaceEfficiency; }
            set
            {
                UpdateProperty(ref _textSpaceEfficiency, value);
            }
        }

        public string TextBuildings
        {
            get { return _textBuildings; }
            set
            {
                UpdateProperty(ref _textBuildings, value);
            }
        }

        public string TextBuildingsSelected
        {
            get { return _textBuildingsSelected; }
            set
            {
                UpdateProperty(ref _textBuildingsSelected, value);
            }
        }

        public string TextTiles
        {
            get { return _textTiles; }
            set
            {
                UpdateProperty(ref _textTiles, value);
            }
        }

        public string TextNameNotFound
        {
            get { return _textNameNotFound; }
            set
            {
                UpdateProperty(ref _textNameNotFound, value);
            }
        }

        #endregion

        public string UsedArea
        {
            get { return _usedArea; }
            set { UpdateProperty(ref _usedArea, value); }
        }

        public double UsedTiles
        {
            get { return _usedTiles; }
            set { UpdateProperty(ref _usedTiles, value); }
        }

        public double MinTiles
        {
            get { return _minTiles; }
            set { UpdateProperty(ref _minTiles, value); }
        }

        public string Efficiency
        {
            get { return _efficiency; }
            set { UpdateProperty(ref _efficiency, value); }
        }

        public bool AreStatisticsAvailable
        {
            get { return _areStatisticsAvailable; }
            set { UpdateProperty(ref _areStatisticsAvailable, value); }
        }

        public bool ShowBuildingList
        {
            get { return _showBuildingList; }
            set
            {
                UpdateProperty(ref _showBuildingList, value);
                OnPropertyChanged(nameof(ShowSelectedBuildingList));
            }
        }

        public bool ShowSelectedBuildingList
        {
            get { return ShowBuildingList && SelectedBuildings.Any(); }
        }

        public ObservableCollection<StatisticsBuilding> Buildings
        {
            get { return _buildings; }
            set { UpdateProperty(ref _buildings, value); }
        }
        public ObservableCollection<StatisticsBuilding> SelectedBuildings
        {
            get { return _selectedBuildings; }
            set
            {
                UpdateProperty(ref _selectedBuildings, value);
                OnPropertyChanged(nameof(ShowSelectedBuildingList));
            }
        }

        public void UpdateStatistics(List<AnnoObject> placedObjects,
            List<AnnoObject> selectedObjects,
            BuildingPresets buildingPresets)
        {
            if (!placedObjects.Any())
            {
                AreStatisticsAvailable = false;
                return;
            }

            AreStatisticsAvailable = true;

            // calculate bouding box
            var boxX = placedObjects.Max(_ => _.Position.X + _.Size.Width) - placedObjects.Min(_ => _.Position.X);
            var boxY = placedObjects.Max(_ => _.Position.Y + _.Size.Height) - placedObjects.Min(_ => _.Position.Y);
            // calculate area of all buildings
            var minTiles = placedObjects.Where(_ => !_.Road).Sum(_ => _.Size.Width * _.Size.Height);

            UsedArea = string.Format("{0}x{1}", boxX, boxY);
            UsedTiles = boxX * boxY;

            MinTiles = minTiles;
            Efficiency = string.Format("{0}%", Math.Round(minTiles / boxX / boxY * 100));

            if (ShowBuildingList)
            {
                var groupedBuildings = placedObjects.GroupBy(_ => _.Identifier);
                var groupedSelectedBuildings = selectedObjects.Count > 0 ? selectedObjects.GroupBy(_ => _.Identifier) : null;

                Buildings = getStatisticBuildings(groupedBuildings, buildingPresets);
                SelectedBuildings = getStatisticBuildings(groupedSelectedBuildings, buildingPresets);
            }
            else
            {

            }
        }

        private ObservableCollection<StatisticsBuilding> getStatisticBuildings(IEnumerable<IGrouping<string, AnnoObject>> groupedBuildingsByIdentifier, BuildingPresets buildingPresets)
        {
            var result = new ObservableCollection<StatisticsBuilding>();

            if (groupedBuildingsByIdentifier == null)
            {
                return result;
            }

            var language = Localization.Localization.GetLanguageCodeFromName(MainWindow.SelectedLanguage);

            foreach (var item in groupedBuildingsByIdentifier
                        .Where(_ => !_.ElementAt(0).Road && _.ElementAt(0).Identifier != null)
                        .OrderByDescending(_ => _.Count()))
            {
                var statisticBuilding = new StatisticsBuilding();

                if (!string.IsNullOrWhiteSpace(item.ElementAt(0).Identifier))
                {
                    var building = buildingPresets.Buildings.FirstOrDefault(_ => _.Identifier == item.ElementAt(0).Identifier);
                    if (building != null || item.ElementAt(0).Identifier=="Unknown Object")
                    {
                        if (item.ElementAt(0).Identifier == "Unknown Object")
                        {
                            statisticBuilding.Count = item.Count();
                            statisticBuilding.Name = Localization.Localization.Translations[language]["UnknownObject"];
                        }
                        else
                        {
                            statisticBuilding.Count = item.Count();
                            statisticBuilding.Name = building.Localization[language];
                        }
                    }
                    else
                    {
                        item.ElementAt(0).Identifier = "";

                        statisticBuilding.Count = item.Count();
                        statisticBuilding.Name = TextNameNotFound;
                    }
                }
                else
                {
                    statisticBuilding.Count = item.Count();
                    statisticBuilding.Name = TextNameNotFound;
                }

                result.Add(statisticBuilding);
            }

            return result;
        }
    }
}
