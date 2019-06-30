using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using AnnoDesigner.Core;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Core.Presets.Loader;
using AnnoDesigner.Core.Presets.Models;
using AnnoDesigner.model;
using AnnoDesigner.model.PresetsTree;
using AnnoDesigner.viewmodel;
using Moq;
using Xunit;
using Xunit.Sdk;

namespace AnnoDesigner.Tests
{
    public class PresetsTreeViewModelTests
    {
        private static readonly BuildingPresets _subsetFromPresetsFile;
        private static readonly BuildingPresets _subsetForFiltering;
        private static ILocalizationHelper _mockedTreeLocalization;

        static PresetsTreeViewModelTests()
        {
            _subsetFromPresetsFile = InitSubsetFromPresetsFile();
            _subsetForFiltering = InitSubsetForFiltering();

            var mockedLocalizationHelper = new Mock<ILocalizationHelper>();
            mockedLocalizationHelper.Setup(x => x.GetLocalization(It.IsAny<string>())).Returns<string>(x => x);
            mockedLocalizationHelper.Setup(x => x.GetLocalization(It.IsAny<string>(), It.IsAny<string>())).Returns((string value, string langauge) => value);
            _mockedTreeLocalization = mockedLocalizationHelper.Object;
        }

        #region test data        

        private (List<GenericTreeItem> items, Dictionary<int, bool> expectedState) GetTreeAndState(bool expandLastMainNode = true)
        {
            //tree state:
            //-item 1       //is expanded, but has no children -> test unexpected behaviour [do not add to list]
            //-item 2       //is not expanded and has no children -> test normal behaviour [do not add to list]
            //-item 3       //is not expanded and has empty children list -> test default behaviour [do not add to list]
            //-item 4       //is expanded and has children -> test condensed behaviour [add true to list]
            // -item 41     //is expanded, but has no children -> test unexpected behaviour [do not add to list]
            //-item 5       //is expanded and has children -> test condensed behaviour [add true to list]
            // -item 51     //is not expanded, but has children -> test normal behaviour [add false to list]
            // -item 52     //is expanded and has children -> test condensed behaviour [add true to list]
            //  -item 521   //is not expanded and has no children -> test normal behaviour [do not add to list]

            var expectedState = new Dictionary<int, bool>();
            if (expandLastMainNode)
            {
                expectedState = new Dictionary<int, bool>
                {
                    { 4, true},
                    { 5, true},
                    { 51, false},
                    { 52, true},
                };
            }
            else
            {
                expectedState = new Dictionary<int, bool>
                {
                    { 4, true},
                    { 5, false},
                };
            }

            var item4 = new GenericTreeItem(null) { Id = 4, Header = "item 4", IsExpanded = true };
            var item41 = new GenericTreeItem(item4) { Id = 41, Header = "item 41", IsExpanded = true };
            item4.Children.Add(item41);

            var item5 = new GenericTreeItem(null) { Id = 5, Header = "item 5", IsExpanded = expandLastMainNode };
            var item51 = new GenericTreeItem(item5) { Id = 51, Header = "item 51", IsExpanded = false };
            var item511 = new GenericTreeItem(item51) { Id = 511, Header = "item 511", IsExpanded = false };
            item51.Children.Add(item511);
            var item52 = new GenericTreeItem(item5) { Id = 52, Header = "item 52", IsExpanded = expandLastMainNode };
            var item521 = new GenericTreeItem(item52) { Id = 521, Header = "item 521", IsExpanded = false };
            item52.Children.Add(item521);
            item5.Children.Add(item51);
            item5.Children.Add(item52);

            var items = new List<GenericTreeItem>
            {
                new GenericTreeItem(null) { Id = 1, Header = "item 1", IsExpanded = true },
                new GenericTreeItem(null) { Id = 2, Header = "item 2", IsExpanded = false },
                new GenericTreeItem(null) { Id = 3, Header = "item 3", Children = new ObservableCollection<GenericTreeItem>() },
                item4,
                item5
            };

            return (items, expectedState);
        }

        /// <summary>
        /// Load a subset of current presets. Is only called once.
        /// </summary>
        /// <returns>A subset of the current presets.</returns>
        private static BuildingPresets InitSubsetFromPresetsFile()
        {
            var loader = new BuildingPresetsLoader();
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            var buildingPresets = loader.Load(Path.Combine(basePath, CoreConstants.PresetsFiles.BuildingPresetsFile));

            var buildings_1404 = buildingPresets.Buildings.Where(x => x.Header.StartsWith("(A4")).OrderByDescending(x => x.GetOrderParameter()).Take(10).ToList();
            var buildings_2070 = buildingPresets.Buildings.Where(x => x.Header.StartsWith("(A5")).OrderByDescending(x => x.GetOrderParameter()).Take(10).ToList();
            var buildings_2205 = buildingPresets.Buildings.Where(x => x.Header.StartsWith("(A6")).OrderByDescending(x => x.GetOrderParameter()).Take(10).ToList();
            var buildings_1800 = buildingPresets.Buildings.Where(x => x.Header.StartsWith("(A7")).OrderByDescending(x => x.GetOrderParameter()).Take(10).ToList();

            var filteredBuildings = new List<BuildingInfo>();
            filteredBuildings.AddRange(buildings_1404);
            filteredBuildings.AddRange(buildings_2070);
            filteredBuildings.AddRange(buildings_2205);
            filteredBuildings.AddRange(buildings_1800);

            var presets = new BuildingPresets
            {
                Version = buildingPresets.Version,
                Buildings = filteredBuildings
            };

            return presets;
        }

        private static BuildingPresets InitSubsetForFiltering()
        {
            var locFireStation = new SerializableDictionary<string>();
            locFireStation.Dict.Add("eng", "Fire Station");

            var locPoliceStation = new SerializableDictionary<string>();
            locPoliceStation.Dict.Add("eng", "Police Station");

            var locBakery = new SerializableDictionary<string>();
            locBakery.Dict.Add("eng", "Bakery");

            var locRiceFarm = new SerializableDictionary<string>();
            locRiceFarm.Dict.Add("eng", "Rice Farm");

            var locRiceField = new SerializableDictionary<string>();
            locRiceField.Dict.Add("eng", "Rice Field");

            var buildings = new List<BuildingInfo>
            {
                //Fire Station
                new BuildingInfo
                {
                    Header = "(A4) Anno 1404",
                    Faction = "Public",
                    Group = "Special",
                    Identifier = "FireStation",
                    Template = "SimpleBuilding",
                    Localization = locFireStation
                },
                //Police Station
                new BuildingInfo
                {
                    Header = "(A5) Anno 2070",
                    Faction = "Others",
                    Group = "Special",
                    Identifier = "police_station",
                    Template = "SupportBuilding",
                    Localization = locPoliceStation
                },
                new BuildingInfo
                {
                    Header = "(A6) Anno 2205",
                    Faction = "(1) Earth",
                    Group = "Public Buildings",
                    Identifier = "metro police",
                    Template = "CityInstitutionBuilding",
                    Localization = locPoliceStation
                },
                new BuildingInfo
                {
                    Header = "(A7) Anno 1800",
                    Faction = "(2) Workers",
                    Group = "Public Buildings",
                    Identifier = "Institution_01 (Police)",
                    Template = "CityInstitutionBuilding",
                    Localization = locPoliceStation
                },
                //Bakery
                new BuildingInfo
                {
                    Header = "(A4) Anno 1404",
                    Faction = "Production",
                    Group = "Factory",
                    Identifier = "Bakery",
                    Template = "FactoryBuilding",
                    Localization = locBakery
                },
                new BuildingInfo
                {
                    Header = "(A7) Anno 1800",
                    Faction = "(2) Workers",
                    Group = "Production Buildings",
                    Identifier = "Food_01 (Bread Maker)",
                    Template = "FactoryBuilding7",
                    Localization = locBakery
                },
                //Rice                
                new BuildingInfo
                {
                    Header = "(A6) Anno 2205",
                    Faction = "Facilities",
                    Group = "Agriculture",
                    Identifier = "production agriculture earth facility 01",
                    Template = "FactoryBuilding",
                    Localization = locRiceFarm
                },
                new BuildingInfo
                {
                    Header = "(A6) Anno 2205",
                    Faction = "Facility Modules",
                    Group = "Agriculture",
                    Identifier = "production agriculture earth facility module 01 tier 01",
                    Template = "BuildingModule",
                    Localization = locRiceField
                }
            };

            var presets = new BuildingPresets
            {
                Version = "0.1",
                Buildings = buildings
            };

            return presets;
        }

        #endregion

        #region ctor tests

        [Fact]
        public void Ctor_ShouldSetDefaultValues()
        {
            // Arrange/Act
            var viewModel = new PresetsTreeViewModel(_mockedTreeLocalization);

            // Assert
            Assert.NotNull(viewModel.Items);
            Assert.NotNull(viewModel.FilteredItems);
            Assert.NotNull(viewModel.DoubleClickCommand);
            Assert.NotNull(viewModel.ReturnKeyPressedCommand);
            Assert.Equal(string.Empty, viewModel.BuildingPresetsVersion);
            Assert.Equal(CoreConstants.GameVersion.All, viewModel.FilterGameVersion);
            Assert.Equal(string.Empty, viewModel.FilterText);
        }

        #endregion

        #region LoadItems tests

        [Fact]
        public void LoadItems_BuildingPresetsIsNull_ShouldThrow()
        {
            // Arrange
            var viewModel = new PresetsTreeViewModel(_mockedTreeLocalization);

            // Act/Assert
            Assert.Throws<ArgumentNullException>(() => viewModel.LoadItems(null));
        }

        [Fact]
        public void LoadItems_BuildingPresetsContainsNoBuildings_ShouldLoadTwoRoadItems()
        {
            // Arrange
            var viewModel = new PresetsTreeViewModel(_mockedTreeLocalization);

            var buildingPresets = new BuildingPresets();
            buildingPresets.Buildings = new List<BuildingInfo>();

            // Act
            viewModel.LoadItems(buildingPresets);

            // Assert
            Assert.Equal(2, viewModel.Items.Count);
            Assert.True(viewModel.Items[0].AnnoObject.Road);
            Assert.True(viewModel.Items[1].AnnoObject.Road);
        }

        [Fact]
        public void LoadItems_ViewModelHasItems_ShouldClearItemsBeforeLoad()
        {
            // Arrange
            var viewModel = new PresetsTreeViewModel(_mockedTreeLocalization);

            var itemsToAdd = Enumerable.Repeat(new GenericTreeItem(null), 10);
            foreach (var curItem in itemsToAdd)
            {
                viewModel.Items.Add(curItem);
            }

            var buildingPresets = new BuildingPresets();
            buildingPresets.Buildings = new List<BuildingInfo>();

            // Act
            viewModel.LoadItems(buildingPresets);

            // Assert
            Assert.Equal(2, viewModel.Items.Count);
        }

        [Theory]
        [InlineData("1.2.5")]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        [InlineData("dummy")]
        public void LoadItems_BuildingPresetsPassed_ShouldSetVersionOfBuildingPresets(string versionToSet)
        {
            // Arrange
            var viewModel = new PresetsTreeViewModel(_mockedTreeLocalization);

            var buildingPresets = new BuildingPresets();
            buildingPresets.Buildings = new List<BuildingInfo>();
            buildingPresets.Version = versionToSet;

            // Act
            viewModel.LoadItems(buildingPresets);

            // Assert
            Assert.Equal(versionToSet, viewModel.BuildingPresetsVersion);
        }

        [Fact]
        public void LoadItems_VersionOfBuildingPresetsIsSet_ShouldRaisePropertyChangedEvent()
        {
            // Arrange
            var viewModel = new PresetsTreeViewModel(_mockedTreeLocalization);

            var buildingPresets = new BuildingPresets();
            buildingPresets.Buildings = new List<BuildingInfo>();

            // Act/Assert            
            Assert.PropertyChanged(viewModel, nameof(viewModel.BuildingPresetsVersion), () => viewModel.LoadItems(buildingPresets));
        }

        [Theory]
        [InlineData("", CoreConstants.GameVersion.Unknown)]
        [InlineData(" ", CoreConstants.GameVersion.Unknown)]
        [InlineData(null, CoreConstants.GameVersion.Unknown)]
        [InlineData("dummy", CoreConstants.GameVersion.Unknown)]
        [InlineData("(A4) Anno 1404", CoreConstants.GameVersion.Anno1404)]
        [InlineData("(A5) Anno 2070", CoreConstants.GameVersion.Anno2070)]
        [InlineData("(A6) Anno 2205", CoreConstants.GameVersion.Anno2205)]
        [InlineData("(A7) Anno 1800", CoreConstants.GameVersion.Anno1800)]
        public void LoadItems_BuildingsHaveHeader_ShouldSetCorrectGameVersion(string headerToSet, CoreConstants.GameVersion expectedGameVersion)
        {
            // Arrange
            var viewModel = new PresetsTreeViewModel(_mockedTreeLocalization);

            var buildings = new List<BuildingInfo>
            {
                new BuildingInfo
                {
                    Header = headerToSet
                }
            };

            var buildingPresets = new BuildingPresets();
            buildingPresets.Buildings = buildings;

            // Act
            viewModel.LoadItems(buildingPresets);

            // Assert
            //first 2 items always the road items
            Assert.Equal(expectedGameVersion, (viewModel.Items[2] as GameHeaderTreeItem).GameVersion);
        }

        [Theory]
        [InlineData("Ark")]
        [InlineData("Harbour")]
        [InlineData("OrnamentBuilding")]
        public void LoadItems_BuildingsHaveSpecialTemplate_ShouldNotLoadBuildings(string templateToSet)
        {
            // Arrange
            var viewModel = new PresetsTreeViewModel(_mockedTreeLocalization);

            var buildings = new List<BuildingInfo>
            {
                new BuildingInfo
                {
                    Header = "(A4) Anno 1404",
                    Template = templateToSet
                }
            };

            var buildingPresets = new BuildingPresets();
            buildingPresets.Buildings = buildings;

            // Act
            viewModel.LoadItems(buildingPresets);

            // Assert
            //there are always 2 road items
            Assert.Equal(2, viewModel.Items.Count);
        }

        [Theory]
        [InlineData("third party")]
        [InlineData("Facility Modules")]
        public void LoadItems_BuildingsHaveSpecialFaction_ShouldNotLoadBuildings(string factionToSet)
        {
            // Arrange
            var viewModel = new PresetsTreeViewModel(_mockedTreeLocalization);

            var buildings = new List<BuildingInfo>
            {
                new BuildingInfo
                {
                    Header = "(A4) Anno 1404",
                    Faction = factionToSet
                }
            };

            var buildingPresets = new BuildingPresets();
            buildingPresets.Buildings = buildings;

            // Act
            viewModel.LoadItems(buildingPresets);

            // Assert
            //there are always 2 road items
            Assert.Equal(2, viewModel.Items.Count);
        }

        [Fact]
        public void LoadItems_BuildingsHaveFaction_ShouldLoadBuildings()
        {
            // Arrange
            var viewModel = new PresetsTreeViewModel(_mockedTreeLocalization);

            var buildings = new List<BuildingInfo>
            {
                new BuildingInfo
                {
                    Header = "(A4) Anno 1404",
                    Faction = "Workers",
                    Identifier = "A4_house"//required for GetOrderParameter
                }
            };

            var buildingPresets = new BuildingPresets();
            buildingPresets.Buildings = buildings;

            // Act
            viewModel.LoadItems(buildingPresets);

            // Assert
            //first 2 items always the road items
            Assert.Single((viewModel.Items[2] as GameHeaderTreeItem).Children);
            Assert.Equal(buildings[0].Faction, (viewModel.Items[2] as GameHeaderTreeItem).Children[0].Header);
        }

        [Fact]
        public void LoadItems_Subset_ShouldLoadBuildings()
        {
            // Arrange
            var viewModel = new PresetsTreeViewModel(_mockedTreeLocalization);

            // Act
            viewModel.LoadItems(_subsetFromPresetsFile);

            // Assert
            //2 road buildings + 4 game versions
            Assert.Equal(6, viewModel.Items.Count);
        }

        [Fact]
        public void LoadItems_SubsetForFiltering_ShouldLoadAllItemsAndAddExtraNodeForModulesIn2205()
        {
            // Arrange
            var viewModel = new PresetsTreeViewModel(_mockedTreeLocalization);

            // Act
            viewModel.LoadItems(_subsetForFiltering);

            // Assert
            Assert.True(viewModel.Items[0].IsVisible);//first road tile
            Assert.True(viewModel.Items[1].IsVisible);//second road tile

            var anno1404Node = viewModel.Items[2];
            Assert.Equal("(A4) Anno 1404", anno1404Node.Header);
            Assert.True(anno1404Node.IsVisible);
            var productionNode = anno1404Node.Children.Single(x => x.Header.StartsWith("Production"));
            Assert.True(productionNode.IsVisible);
            var factoryNode = productionNode.Children.Single(x => x.Header.StartsWith("Factory"));
            Assert.True(factoryNode.IsVisible);
            var bakeryNode = factoryNode.Children.Single(x => x.Header.StartsWith("Bakery"));
            Assert.True(bakeryNode.IsVisible);
            var publicNode = anno1404Node.Children.Single(x => x.Header.StartsWith("Public"));
            Assert.True(publicNode.IsVisible);
            var specialNode = publicNode.Children.Single(x => x.Header.StartsWith("Special"));
            Assert.True(specialNode.IsVisible);
            var fireStationNode = specialNode.Children.First(x => x.Header.StartsWith("Fire"));
            Assert.True(fireStationNode.IsVisible);

            var anno2070Node = viewModel.Items[3];
            Assert.Equal("(A5) Anno 2070", anno2070Node.Header);
            Assert.True(anno2070Node.IsVisible);
            var othersNode = anno2070Node.Children.Single(x => x.Header.StartsWith("Others"));
            Assert.True(othersNode.IsVisible);
            specialNode = othersNode.Children.Single(x => x.Header.StartsWith("Special"));
            Assert.True(specialNode.IsVisible);
            var policeStationNode = specialNode.Children.First(x => x.Header.StartsWith("Police"));
            Assert.True(policeStationNode.IsVisible);

            var anno2205Node = viewModel.Items[4];
            Assert.Equal("(A6) Anno 2205", anno2205Node.Header);
            Assert.True(anno2205Node.IsVisible);
            var facilityNode = anno2205Node.Children.Single(x => x.Header.StartsWith("Facilities"));
            Assert.True(facilityNode.IsVisible);
            var agricutureNode = facilityNode.Children.Single(x => x.Header.StartsWith("Agriculture"));
            Assert.True(agricutureNode.IsVisible);
            var riceFarmNode = agricutureNode.Children.Single(x => x.Header.StartsWith("Rice Farm"));
            Assert.True(riceFarmNode.IsVisible);
            var moduleNode = agricutureNode.Children.Single(x => x.Header.StartsWith("Agriculture Modules"));
            Assert.True(moduleNode.IsVisible);
            var riceFieldNode = moduleNode.Children.Single(x => x.Header.StartsWith("Rice Field"));
            Assert.True(riceFieldNode.IsVisible);
            var earthNode = anno2205Node.Children.Single(x => x.Header.StartsWith("(1) Earth"));
            Assert.True(earthNode.IsVisible);
            publicNode = earthNode.Children.Single(x => x.Header.StartsWith("Public"));
            Assert.True(publicNode.IsVisible);
            policeStationNode = publicNode.Children.First(x => x.Header.StartsWith("Police"));
            Assert.True(policeStationNode.IsVisible);

            var anno1800Node = viewModel.Items[5];
            Assert.Equal("(A7) Anno 1800", anno1800Node.Header);
            Assert.True(anno1800Node.IsVisible);
            var workersNode = anno1800Node.Children.Single(x => x.Header.StartsWith("(2) Workers"));
            Assert.True(workersNode.IsVisible);
            productionNode = workersNode.Children.Single(x => x.Header.StartsWith("Production"));
            Assert.True(productionNode.IsVisible);
            bakeryNode = productionNode.Children.Single(x => x.Header.StartsWith("Bakery"));
            Assert.True(bakeryNode.IsVisible);
            publicNode = workersNode.Children.Single(x => x.Header.StartsWith("Public"));
            Assert.True(publicNode.IsVisible);
            policeStationNode = publicNode.Children.First(x => x.Header.StartsWith("Police"));
            Assert.True(policeStationNode.IsVisible);
        }

        #endregion

        #region DoubleClick tests

        [Fact]
        public void DoubleClick_CommandParameterIsNull_ShouldNotSetSelectedItem()
        {
            // Arrange
            var viewModel = new PresetsTreeViewModel(_mockedTreeLocalization);

            // Act
            viewModel.DoubleClickCommand.Execute(null);

            // Assert
            Assert.Null(viewModel.SelectedItem);
        }

        [Fact]
        public void DoubleClick_CommandParameterIsNull_ShouldNotRaiseApplySelectedItemEvent()
        {
            // Arrange
            var viewModel = new PresetsTreeViewModel(_mockedTreeLocalization);

            // Act
            var ex = Record.Exception(() => Assert.Raises<EventArgs>(
                  x => viewModel.ApplySelectedItem += x,
                  x => viewModel.ApplySelectedItem -= x,
                  () => viewModel.DoubleClickCommand.Execute(null)));

            // Assert
            var exception = Assert.IsType<RaisesException>(ex);
            Assert.Equal("(No event was raised)", exception.Actual);
            Assert.Equal("EventArgs", exception.Expected);
        }

        [Fact]
        public void DoubleClick_CommandParameterHasNoAnnoObject_ShouldNotSetSelectedItem()
        {
            // Arrange
            var viewModel = new PresetsTreeViewModel(_mockedTreeLocalization);

            var commandParameter = new GenericTreeItem(null);

            // Act
            viewModel.DoubleClickCommand.Execute(commandParameter);

            // Assert
            Assert.Null(viewModel.SelectedItem);
        }

        [Fact]
        public void DoubleClick_CommandParameterHasNoAnnoObject_ShouldNotRaiseApplySelectedItemEvent()
        {
            // Arrange
            var viewModel = new PresetsTreeViewModel(_mockedTreeLocalization);

            var commandParameter = new GenericTreeItem(null);

            // Act
            var ex = Record.Exception(() => Assert.Raises<EventArgs>(
                  x => viewModel.ApplySelectedItem += x,
                  x => viewModel.ApplySelectedItem -= x,
                  () => viewModel.DoubleClickCommand.Execute(commandParameter)));

            // Assert
            var exception = Assert.IsType<RaisesException>(ex);
            Assert.Equal("(No event was raised)", exception.Actual);
            Assert.Equal("EventArgs", exception.Expected);
        }

        [Fact]
        public void DoubleClick_CommandParameterIsUnknownObject_ShouldNotSetSelectedItem()
        {
            // Arrange
            var viewModel = new PresetsTreeViewModel(_mockedTreeLocalization);

            var commandParameter = new Object();

            // Act
            viewModel.DoubleClickCommand.Execute(commandParameter);

            // Assert
            Assert.Null(viewModel.SelectedItem);
        }

        [Fact]
        public void DoubleClick_CommandParameterIsUnknownObject_ShouldNotRaiseApplySelectedItemEvent()
        {
            // Arrange
            var viewModel = new PresetsTreeViewModel(_mockedTreeLocalization);

            var commandParameter = new Object();

            // Act
            var ex = Record.Exception(() => Assert.Raises<EventArgs>(
                  x => viewModel.ApplySelectedItem += x,
                  x => viewModel.ApplySelectedItem -= x,
                  () => viewModel.DoubleClickCommand.Execute(commandParameter)));

            // Assert
            var exception = Assert.IsType<RaisesException>(ex);
            Assert.Equal("(No event was raised)", exception.Actual);
            Assert.Equal("EventArgs", exception.Expected);
        }

        [Fact]
        public void DoubleClick_CommandParameterHasAnnoObject_ShouldSetSelectedItem()
        {
            // Arrange
            var viewModel = new PresetsTreeViewModel(_mockedTreeLocalization);
            var annoObjectToSet = new AnnoObject();

            var commandParameter = new GenericTreeItem(null);
            commandParameter.AnnoObject = annoObjectToSet;

            // Act
            viewModel.DoubleClickCommand.Execute(commandParameter);

            // Assert
            Assert.Equal(commandParameter, viewModel.SelectedItem);
        }

        [Fact]
        public void DoubleClick_CommandParameterHasAnnoObject_ShouldRaiseApplySelectedItemEvent()
        {
            // Arrange
            var viewModel = new PresetsTreeViewModel(_mockedTreeLocalization);

            var annoObjectToSet = new AnnoObject();

            var commandParameter = new GenericTreeItem(null);
            commandParameter.AnnoObject = annoObjectToSet;

            // Act/Assert
            Assert.Raises<EventArgs>(
                x => viewModel.ApplySelectedItem += x,
                x => viewModel.ApplySelectedItem -= x,
                () => viewModel.DoubleClickCommand.Execute(commandParameter));
        }

        #endregion

        #region ReturnKeyPressed tests

        [Fact]
        public void ReturnKeyPressed_CommandParameterIsNull_ShouldNotSetSelectedItem()
        {
            // Arrange
            var viewModel = new PresetsTreeViewModel(_mockedTreeLocalization);

            // Act
            viewModel.ReturnKeyPressedCommand.Execute(null);

            // Assert
            Assert.Null(viewModel.SelectedItem);
        }

        [Fact]
        public void ReturnKeyPressed_CommandParameterIsNull_ShouldNotRaiseApplySelectedItemEvent()
        {
            // Arrange
            var viewModel = new PresetsTreeViewModel(_mockedTreeLocalization);

            // Act
            var ex = Record.Exception(() => Assert.Raises<EventArgs>(
                  x => viewModel.ApplySelectedItem += x,
                  x => viewModel.ApplySelectedItem -= x,
                  () => viewModel.ReturnKeyPressedCommand.Execute(null)));

            // Assert
            var exception = Assert.IsType<RaisesException>(ex);
            Assert.Equal("(No event was raised)", exception.Actual);
            Assert.Equal("EventArgs", exception.Expected);
        }

        [Fact]
        public void ReturnKeyPressed_CommandParameterHasNoAnnoObject_ShouldNotSetSelectedItem()
        {
            // Arrange
            var viewModel = new PresetsTreeViewModel(_mockedTreeLocalization);

            var commandParameter = new GenericTreeItem(null);

            // Act
            viewModel.ReturnKeyPressedCommand.Execute(commandParameter);

            // Assert
            Assert.Null(viewModel.SelectedItem);
        }

        [Fact]
        public void ReturnKeyPressed_CommandParameterHasNoAnnoObject_ShouldNotRaiseApplySelectedItemEvent()
        {
            // Arrange
            var viewModel = new PresetsTreeViewModel(_mockedTreeLocalization);

            var commandParameter = new GenericTreeItem(null);

            // Act
            var ex = Record.Exception(() => Assert.Raises<EventArgs>(
                  x => viewModel.ApplySelectedItem += x,
                  x => viewModel.ApplySelectedItem -= x,
                  () => viewModel.ReturnKeyPressedCommand.Execute(commandParameter)));

            // Assert
            var exception = Assert.IsType<RaisesException>(ex);
            Assert.Equal("(No event was raised)", exception.Actual);
            Assert.Equal("EventArgs", exception.Expected);
        }

        [Fact]
        public void ReturnKeyPressed_CommandParameterIsUnknownObject_ShouldNotSetSelectedItem()
        {
            // Arrange
            var viewModel = new PresetsTreeViewModel(_mockedTreeLocalization);

            var commandParameter = new Object();

            // Act
            viewModel.ReturnKeyPressedCommand.Execute(commandParameter);

            // Assert
            Assert.Null(viewModel.SelectedItem);
        }

        [Fact]
        public void ReturnKeyPressed_CommandParameterIsUnknownObject_ShouldNotRaiseApplySelectedItemEvent()
        {
            // Arrange
            var viewModel = new PresetsTreeViewModel(_mockedTreeLocalization);

            var commandParameter = new Object();

            // Act
            var ex = Record.Exception(() => Assert.Raises<EventArgs>(
                  x => viewModel.ApplySelectedItem += x,
                  x => viewModel.ApplySelectedItem -= x,
                  () => viewModel.ReturnKeyPressedCommand.Execute(commandParameter)));

            // Assert
            var exception = Assert.IsType<RaisesException>(ex);
            Assert.Equal("(No event was raised)", exception.Actual);
            Assert.Equal("EventArgs", exception.Expected);
        }

        [Fact]
        public void ReturnKeyPressed_CommandParameterHasAnnoObject_ShouldSetSelectedItem()
        {
            // Arrange
            var viewModel = new PresetsTreeViewModel(_mockedTreeLocalization);
            var annoObjectToSet = new AnnoObject();

            var commandParameter = new GenericTreeItem(null);
            commandParameter.AnnoObject = annoObjectToSet;

            // Act
            viewModel.ReturnKeyPressedCommand.Execute(commandParameter);

            // Assert
            Assert.Equal(commandParameter, viewModel.SelectedItem);
        }

        [Fact]
        public void ReturnKeyPressed_CommandParameterHasAnnoObject_ShouldRaiseApplySelectedItemEvent()
        {
            // Arrange
            var viewModel = new PresetsTreeViewModel(_mockedTreeLocalization);

            var annoObjectToSet = new AnnoObject();

            var commandParameter = new GenericTreeItem(null);
            commandParameter.AnnoObject = annoObjectToSet;

            // Act/Assert
            Assert.Raises<EventArgs>(
                x => viewModel.ApplySelectedItem += x,
                x => viewModel.ApplySelectedItem -= x,
                () => viewModel.ReturnKeyPressedCommand.Execute(commandParameter));
        }

        #endregion

        #region GetCondensedTreeState tests

        [Fact]
        public void GetCondensedTreeState_ItemsAreEmpty_ShouldReturnEmptyList()
        {
            // Arrange
            var viewModel = new PresetsTreeViewModel(_mockedTreeLocalization);

            // Act
            var result = viewModel.GetCondensedTreeState();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetCondensedTreeState_ItemsExpanded_ShouldReturnCorrectList()
        {
            // Arrange
            var viewModel = new PresetsTreeViewModel(_mockedTreeLocalization);

            var (items, expectedState) = GetTreeAndState();

            viewModel.Items = new ObservableCollection<GenericTreeItem>(items);

            // Act
            var result = viewModel.GetCondensedTreeState();

            // Assert
            Assert.Equal(expectedState, result);
        }

        #endregion

        #region SetCondensedTreeState tests

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void SetCondensedTreeState_LastPresetsVersionIsNullOrWhiteSpace_ShouldNotSetAnyStateAndNotThrow(string lastBuildingPresetsVersionToSet)
        {
            // Arrange
            var viewModel = new PresetsTreeViewModel(_mockedTreeLocalization);

            var (items, expectedState) = GetTreeAndState();
            viewModel.Items = new ObservableCollection<GenericTreeItem>(items);

            // Act
            viewModel.SetCondensedTreeState(new Dictionary<int, bool>(), lastBuildingPresetsVersionToSet);

            // Assert
            Assert.Equal(expectedState, viewModel.GetCondensedTreeState());
        }

        [Fact]
        public void SetCondensedTreeState_LastPresetsVersionIsDifferent_ShouldNotSetAnyStateAndNotThrow()
        {
            // Arrange
            var buildingPresetsVersion = "1.0";

            var buildingPresets = new BuildingPresets();
            buildingPresets.Buildings = new List<BuildingInfo>();
            buildingPresets.Version = buildingPresetsVersion;

            var viewModel = new PresetsTreeViewModel(_mockedTreeLocalization);
            viewModel.LoadItems(buildingPresets);

            var (items, expectedState) = GetTreeAndState();
            viewModel.Items = new ObservableCollection<GenericTreeItem>(items);

            // Act
            viewModel.SetCondensedTreeState(new Dictionary<int, bool>(), "2.0");

            // Assert
            Assert.Equal(expectedState, viewModel.GetCondensedTreeState());
        }

        [Fact]
        public void SetCondensedTreeState_SavedTreeStateIsNull_ShouldNotSetAnyStateAndNotThrow()
        {
            // Arrange
            var buildingPresetsVersion = "1.0";

            var buildingPresets = new BuildingPresets();
            buildingPresets.Buildings = new List<BuildingInfo>();
            buildingPresets.Version = buildingPresetsVersion;

            var viewModel = new PresetsTreeViewModel(_mockedTreeLocalization);
            viewModel.LoadItems(buildingPresets);

            var (items, expectedState) = GetTreeAndState();
            viewModel.Items = new ObservableCollection<GenericTreeItem>(items);

            // Act
            viewModel.SetCondensedTreeState(null, buildingPresetsVersion);

            // Assert
            Assert.Equal(expectedState, viewModel.GetCondensedTreeState());
        }

        [Fact]
        public void SetCondensedTreeState_SavedTreeStateIsEmpty_ShouldNotSetAnyStateAndNotThrow()
        {
            // Arrange
            var buildingPresetsVersion = "1.0";

            var buildingPresets = new BuildingPresets();
            buildingPresets.Buildings = new List<BuildingInfo>();
            buildingPresets.Version = buildingPresetsVersion;

            var viewModel = new PresetsTreeViewModel(_mockedTreeLocalization);
            viewModel.LoadItems(buildingPresets);

            var (items, expectedState) = GetTreeAndState();
            viewModel.Items = new ObservableCollection<GenericTreeItem>(items);

            // Act
            viewModel.SetCondensedTreeState(new Dictionary<int, bool>(), buildingPresetsVersion);

            // Assert
            Assert.Equal(expectedState, viewModel.GetCondensedTreeState());
        }

        [Fact]
        public void SetCondensedTreeState_SavedTreeStateIsCompatible_ShouldSetTreeState()
        {
            // Arrange
            var buildingPresetsVersion = "1.0";

            var buildingPresets = new BuildingPresets();
            buildingPresets.Buildings = new List<BuildingInfo>();
            buildingPresets.Version = buildingPresetsVersion;

            var viewModel = new PresetsTreeViewModel(_mockedTreeLocalization);
            viewModel.LoadItems(buildingPresets);

            var (items, expectedState) = GetTreeAndState(expandLastMainNode: false);
            viewModel.Items.Clear();
            foreach (var curItem in items)
            {
                viewModel.Items.Add(curItem);
            }

            var savedTreeState = new Dictionary<int, bool>
            {
                { 4, true },
                { 5, false }
            };

            // Act
            viewModel.SetCondensedTreeState(savedTreeState, buildingPresetsVersion);

            // Assert
            var currentState = viewModel.GetCondensedTreeState();
            Assert.Equal(savedTreeState, currentState);
        }

        #endregion

        #region FilterGameVersion tests

        [Theory]
        [InlineData(CoreConstants.GameVersion.Anno1404, 1)]
        [InlineData(CoreConstants.GameVersion.Anno2070, 1)]
        [InlineData(CoreConstants.GameVersion.Anno2205, 1)]
        [InlineData(CoreConstants.GameVersion.Anno1800, 1)]
        [InlineData(CoreConstants.GameVersion.Anno1404 | CoreConstants.GameVersion.Anno1800, 2)]
        [InlineData(CoreConstants.GameVersion.Anno1404 | CoreConstants.GameVersion.Anno1800 | CoreConstants.GameVersion.Anno2205, 3)]
        [InlineData(CoreConstants.GameVersion.All, 4)]
        [InlineData(CoreConstants.GameVersion.Unknown, 0)]
        public void FilterGameVersion_SubsetIsLoadedAndFilterTextIsEmpty_ShouldFilterByGameVersion(CoreConstants.GameVersion gameVersionsToFilter, int expectedMainNodeCount)
        {
            // Arrange
            var viewModel = new PresetsTreeViewModel(_mockedTreeLocalization);
            viewModel.LoadItems(_subsetFromPresetsFile);
            viewModel.FilterText = string.Empty;

            //make sure to really trigger the filter method
            if (viewModel.FilterGameVersion != CoreConstants.GameVersion.Unknown)
            {
                viewModel.FilterGameVersion = CoreConstants.GameVersion.Unknown;
            }
            else
            {
                viewModel.FilterGameVersion = CoreConstants.GameVersion.All;
            }

            // Act
            viewModel.FilterGameVersion = gameVersionsToFilter;

            // Assert
            //+ 2 road buildings
            Assert.Equal(2 + expectedMainNodeCount, viewModel.Items.Where(x => x.IsVisible).Count());
        }

        #endregion

        #region FilterText tests

        [Fact]
        public void FilterText_SearchForFireAndFilterGameVersionIsAll_ShouldOnlyShowFireStationAndExpandParents()
        {
            // Arrange
            var filterText = "Fire";

            var viewModel = new PresetsTreeViewModel(_mockedTreeLocalization);
            viewModel.LoadItems(_subsetForFiltering);
            viewModel.FilterText = string.Empty;
            viewModel.FilterGameVersion = CoreConstants.GameVersion.All;

            // Act
            viewModel.FilterText = filterText;

            // Assert
            Assert.False(viewModel.Items[0].IsVisible);//first road tile
            Assert.False(viewModel.Items[1].IsVisible);//second road tile

            var anno1404Node = viewModel.Items[2];
            Assert.Equal("(A4) Anno 1404", anno1404Node.Header);
            Assert.True(anno1404Node.IsVisible);
            Assert.True(anno1404Node.IsExpanded);
            Assert.False(anno1404Node.Children.Single(x => x.Header.StartsWith("Production")).IsVisible);
            var publicNode = anno1404Node.Children.Single(x => x.Header.StartsWith("Public"));
            Assert.True(publicNode.IsVisible);
            Assert.True(publicNode.IsExpanded);
            var specialNode = publicNode.Children.Single(x => x.Header.StartsWith("Special"));
            Assert.True(specialNode.IsVisible);
            Assert.True(specialNode.IsExpanded);
            var fireStationNode = specialNode.Children.First(x => x.Header.StartsWith("Fire"));
            Assert.True(fireStationNode.IsVisible);

            //all other nodes should not be visible
            var anno2070Node = viewModel.Items[3];
            Assert.Equal("(A5) Anno 2070", anno2070Node.Header);
            Assert.False(anno2070Node.IsVisible);
            var anno2205Node = viewModel.Items[4];
            Assert.Equal("(A6) Anno 2205", anno2205Node.Header);
            Assert.False(anno2205Node.IsVisible);
            var anno1800Node = viewModel.Items[5];
            Assert.Equal("(A7) Anno 1800", anno1800Node.Header);
            Assert.False(anno1800Node.IsVisible);
        }

        [Fact]
        public void FilterText_SearchForPoliceAndFilterGameVersionIsAll_ShouldOnlyShowPoliceStationAndExpandParents()
        {
            // Arrange
            var filterText = "Police";

            var viewModel = new PresetsTreeViewModel(_mockedTreeLocalization);
            viewModel.LoadItems(_subsetForFiltering);
            viewModel.FilterText = string.Empty;
            viewModel.FilterGameVersion = CoreConstants.GameVersion.All;

            // Act
            viewModel.FilterText = filterText;

            // Assert
            Assert.False(viewModel.Items[0].IsVisible);//first road tile
            Assert.False(viewModel.Items[1].IsVisible);//second road tile

            var anno1404Node = viewModel.Items[2];
            Assert.Equal("(A4) Anno 1404", anno1404Node.Header);
            Assert.False(anno1404Node.IsVisible);

            var anno2070Node = viewModel.Items[3];
            Assert.Equal("(A5) Anno 2070", anno2070Node.Header);
            Assert.True(anno2070Node.IsVisible);
            Assert.True(anno2070Node.IsExpanded);
            var othersNode = anno2070Node.Children.Single(x => x.Header.StartsWith("Others"));
            Assert.True(othersNode.IsVisible);
            Assert.True(othersNode.IsExpanded);
            var specialNode = othersNode.Children.Single(x => x.Header.StartsWith("Special"));
            Assert.True(specialNode.IsVisible);
            Assert.True(specialNode.IsExpanded);
            var policeStationNode = specialNode.Children.First(x => x.Header.StartsWith("Police"));
            Assert.True(policeStationNode.IsVisible);

            var anno2205Node = viewModel.Items[4];
            Assert.Equal("(A6) Anno 2205", anno2205Node.Header);
            Assert.True(anno2205Node.IsVisible);
            Assert.True(anno2205Node.IsExpanded);
            Assert.False(anno2205Node.Children.Single(x => x.Header.StartsWith("Facilities")).IsVisible);
            var earthNode = anno2205Node.Children.Single(x => x.Header.StartsWith("(1) Earth"));
            Assert.True(earthNode.IsVisible);
            Assert.True(earthNode.IsExpanded);
            var publicNode = earthNode.Children.Single(x => x.Header.StartsWith("Public"));
            Assert.True(publicNode.IsVisible);
            Assert.True(publicNode.IsExpanded);
            policeStationNode = publicNode.Children.First(x => x.Header.StartsWith("Police"));
            Assert.True(policeStationNode.IsVisible);

            var anno1800Node = viewModel.Items[5];
            Assert.Equal("(A7) Anno 1800", anno1800Node.Header);
            Assert.True(anno1800Node.IsVisible);
            Assert.True(anno1800Node.IsExpanded);
            var workersNode = anno1800Node.Children.Single(x => x.Header.StartsWith("(2) Workers"));
            Assert.True(workersNode.IsVisible);
            Assert.True(workersNode.IsExpanded);
            Assert.False(workersNode.Children.Single(x => x.Header.StartsWith("Production")).IsVisible);
            publicNode = workersNode.Children.Single(x => x.Header.StartsWith("Public"));
            Assert.True(publicNode.IsVisible);
            Assert.True(publicNode.IsExpanded);
            policeStationNode = publicNode.Children.First(x => x.Header.StartsWith("Police"));
            Assert.True(policeStationNode.IsVisible);
        }

        [Fact]
        public void FilterText_SearchForPoliceAndFilterGameVersionIs2070_ShouldOnlyShowPoliceStationAndExpandParentsForAnno2070()
        {
            // Arrange
            var filterText = "Police";

            var viewModel = new PresetsTreeViewModel(_mockedTreeLocalization);
            viewModel.LoadItems(_subsetForFiltering);
            viewModel.FilterText = string.Empty;
            viewModel.FilterGameVersion = CoreConstants.GameVersion.Anno2070;

            // Act
            viewModel.FilterText = filterText;

            // Assert
            Assert.False(viewModel.Items[0].IsVisible);//first road tile
            Assert.False(viewModel.Items[1].IsVisible);//second road tile

            var anno1404Node = viewModel.Items[2];
            Assert.Equal("(A4) Anno 1404", anno1404Node.Header);
            Assert.False(anno1404Node.IsVisible);

            var anno2070Node = viewModel.Items[3];
            Assert.Equal("(A5) Anno 2070", anno2070Node.Header);
            Assert.True(anno2070Node.IsVisible);
            Assert.True(anno2070Node.IsExpanded);
            var othersNode = anno2070Node.Children.Single(x => x.Header.StartsWith("Others"));
            Assert.True(othersNode.IsVisible);
            Assert.True(othersNode.IsExpanded);
            var specialNode = othersNode.Children.Single(x => x.Header.StartsWith("Special"));
            Assert.True(specialNode.IsVisible);
            Assert.True(specialNode.IsExpanded);
            var policeStationNode = specialNode.Children.First(x => x.Header.StartsWith("Police"));
            Assert.True(policeStationNode.IsVisible);

            //all other nodes should not be visible
            var anno2205Node = viewModel.Items[4];
            Assert.Equal("(A6) Anno 2205", anno2205Node.Header);
            Assert.False(anno2205Node.IsVisible);
            var anno1800Node = viewModel.Items[5];
            Assert.Equal("(A7) Anno 1800", anno1800Node.Header);
            Assert.False(anno1800Node.IsVisible);
        }

        #endregion
    }
}
