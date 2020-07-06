using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PresetParser.Anno1800;
using Xunit;

namespace PresetParser.Tests
{
    public class NewOrnamentsGroup1800Tests
    {
        #region test data

        public static TheoryData<string> ParkPathsTestdata
        {
            get
            {
                return new TheoryData<string>
                {
                    "Park_1x1_pathstraight",
                    "Park_1x1_pathend",
                    "Park_1x1_pathangle",
                    "Park_1x1_pathcrossing",
                    "Park_1x1_pathwall"
                };
            }
        }

        public static TheoryData<string> ParkFencesTestdata
        {
            get
            {
                return new TheoryData<string>
                {
                    "Park_1x1_hedgestraight",
                    "Park_1x1_hedgeend",
                    "Park_1x1_hedgeangle",
                    "Park_1x1_hedgecrossing",
                    "Park_1x1_hedgewall",
                    "Culture_1x1_fencestraight",
                    "Culture_1x1_fenceend",
                    "Culture_1x1_fenceangle",
                    "Culture_1x1_fencecrossing",
                    "Culture_1x1_fencewall",
                    "Park_1x1_hedgegate",
                    "Culture_1x1_fencegate"
                };
            }
        }

        public static TheoryData<string> ParkVegetationTestdata
        {
            get
            {
                return new TheoryData<string>
                {
                    "Park_1x1_grass",
                    "Park_1x1_bush",
                    "Park_1x1_smalltree",
                    "Park_1x1_pine",
                    "Park_1x1_poplar" ,
                    "Park_1x1_bigtree",
                    "Park_1x1_poplarforest",
                    "Park_1x1_tropicalforest",
                    "Park_1x1_philodendron",
                    "Park_1x1_ferns",
                    "Park_1x1_floweringshrub",
                    "Park_1x1_smallpalmtree",
                    "Park_1x1_palmtree",
                    "Park_1x1_shrub",
                    "Park_1x1_growncypress"
                };
            }
        }

        public static TheoryData<string> ParkFountainsTestdata
        {
            get
            {
                return new TheoryData<string>
                {
                    "Uplay_ornament_3x2_large_fountain",
                    "Park_2x2_fountain",
                    "Park_3x3_fountain"
                };
            }
        }

        public static TheoryData<string> ParkStatuesTestdata
        {
            get
            {
                return new TheoryData<string>
                {
                    "Sunken Treasure Ornament 01",
                    "Sunken Treasure Ornament 02",
                    "Sunken Treasure Ornament 03",
                    "Uplay_ornament_2x1_lion_statue",
                    "Culture_preorder_statue",
                    "Park_2x2_statue",
                    "Park_2x2_horsestatue"
                };
            }
        }

        public static TheoryData<string> ParkDecorationsTestdata
        {
            get
            {
                return new TheoryData<string>
                {
                    "Park_1x1_benches",
                    "Uplay_ornament_2x2_pillar_chess_park",
                    "Park_2x2_garden",
                    "Park_1x1_stand",
                    "Park_2x2_gazebo",
                    "Park_3x3_gazebo"
                };
            }
        }

        public static TheoryData<string> CityPathsTestdata
        {
            get
            {
                return new TheoryData<string>
                {
                    "Palace Ornament02 Set01 hedge pointy",
                    "Palace Ornament01 Set01 banner",
                    "Palace Ornament03 Set01 hedge round",
                    "Palace Ornament05 Set01 fountain big",
                    "Palace Ornament04 Set01 fountain small",
                    "Palace Ornament03 Set02 angle",
                    "Palace Ornament04 Set02 crossing",
                    "Palace Ornament02 Set02 end",
                    "Palace Ornament05 Set02 junction",
                    "Palace Ornament01 Set02 straight",
                    "Palace Ornament06 Set02 straight variation"
                };
            }
        }

        public static TheoryData<string> CityFencesTestdata
        {
            get
            {
                return new TheoryData<string>
                {
                    "Culture_prop_system_1x1_03",
                    "Culture_prop_system_1x1_04",
                    "Culture_prop_system_1x1_05",
                    "Culture_prop_system_1x1_06",
                    "Culture_prop_system_1x1_07",
                    "Culture_prop_system_1x1_08",
                    "Culture_prop_system_1x1_09",
                    "Culture_prop_system_1x1_11",
                    "Culture_prop_system_1x1_12",
                    "Culture_prop_system_1x1_13",
                    "Culture_prop_system_1x1_14",
                    "Culture_1x1_hedgegate",
                    "Culture_1x1_hedgestraight"
                };
            }
        }

        public static TheoryData<string> CityStatuesTestdata
        {
            get
            {
                return new TheoryData<string>
                {
                    "Park_1x1_statue",
                    "City_prop_system_2x2_03",
                    "Culture_prop_system_1x1_10"
                };
            }
        }

        public static TheoryData<string> CityDecorationsTestdata
        {
            get
            {
                return new TheoryData<string>
                {
                    "PropagandaTower Players Version",
                    "PropagandaFlag Players Version",
                    "Botanica Ornament 01",
                    "Botanica Ornament 02",
                    "Botanica Ornament 03",
                    "Culture_1x1_benches",
                    "Culture_1x1_stand"
                };
            }
        }

        public static TheoryData<string> SpecialOrnamentsTestdata
        {
            get
            {
                return new TheoryData<string>
                {
                    "Event_ornament_halloween_2019",
                    "Event_ornament_christmas_2019",
                    "Event_ornament_onemio",
                    "Twitchdrops_ornament_billboard_annoholic",
                    "Twitchdrops_ornament_billboard_anno_union",
                    "Twitchdrops_ornament_billboard_anarchy",
                    "Twitchdrops_ornament_billboard_sunken_treasures",
                    "Twitchdrops_ornament_botanical_garden",
                    "Twitchdrops_ornament_flag_banner_annoholic",
                    "Twitchdrops_ornament_flag_banner_anno_union",
                    "Twitchdrops_ornament_billboard_the_passage",
                    "Twitchdrops_ornament_morris_column_annoholic",
                    "Twitchdrops_ornament_flag_seat_of_power",
                    "Twitchdrops_ornament_billboard_seat_of_power",
                    "Twitchdrops_ornament_flag_bright_harvest",
                    "Twitchdrops_ornament_billboard_bright_harvest",
                    "Twitchdrops_ornament_flag_land_of_lions",
                    "Twitchdrops_ornament_billboard_land_of_lions",
                    "Season 2 - Fountain Elephant",
                    "Season 2 - Statue Tractor",
                    "Season 2 - Pillar"
                };
            }
        }

        public static TheoryData<string> ChristmasDecorationsTestdata
        {
            get
            {
                return new TheoryData<string>
                {
                    "Xmas City Tree Small 01",
                    "Xmas City Tree Big 01",
                    "Xmas Parksystem Ornament Straight",
                    "Xmas Parksystem Ornament Corner",
                    "Xmas Parksystem Ornament End",
                    "Xmas City Snowman Ornament 01",
                    "Xmas City Lightpost Ornament 01",
                    "Xmas Citysystem Ornament T",
                    "Xmas Citysystem Ornament Straight",
                    "Xmas Citysystem Ornament Gap",
                    "Xmas Citysystem Ornament Cross",
                    "Xmas Citysystem Ornament End",
                    "Xmas Citysystem Ornament Corner",
                    "Xmas Parksystem Ornament Gap",
                    "Xmas Market 1",
                    "Xmas Parksystem Ornament Cross",
                    "Xmas Parksystem Ornament T",
                    "Xmas Lightpost Ornament 02",
                    "Xmas Market 2",
                    "XMas Market 3",
                    "Xmas Carousel",
                    "Xmas presents",
                    "Xmas Santa Chair"
                };
            }
        }

        public static TheoryData<string> WorldsFairRewardsTestdata
        {
            get
            {
                return new TheoryData<string>
                {
                    "City_prop_system_1x1_02",
                    "City_prop_system_1x1_03",
                    "City_prop_system_2x2_02",
                    "City_prop_system_2x2_04",
                    "City_prop_system_3x3_02",
                    "City_prop_system_3x3_03",
                    "Culture_prop_system_1x1_02",
                    "Culture_1x1_basinbridge",
                    "Culture_1x1_secondground"
                };
            }
        }

        public static TheoryData<string> GardensTestdata
        {
            get
            {
                return new TheoryData<string>
                {
                    "Light pink flower field - roses",
                    "Blue flower field - gentian",
                    "Labyrinth",
                    "Pink flower field - hibiscus",
                    "Purple blue flower field - iris",
                    "White flower field - blue heart lily",
                    "Orange flower field - plumeria aussi orange",
                    "Red white flower field - red white petunia",
                    "Trees alley",
                    "Sculpted trees",
                    "Yellow flower field - miracle daisy"
                };
            }
        }

        public static TheoryData<string> AgriculturalOrnamentsTestdata
        {
            get
            {
                return new TheoryData<string>
                {
                    "BH Ornament04 Flatbed Wagon",
                    "BH Ornament05 Scarecrow",
                    "BH Ornament06 LogPile",
                    "BH Ornament07 Outhouse",
                    "BH Ornament08 Signpost",
                    "BH Ornament09 HayBalePile",
                    "BH Ornament10 Swing",
                    "BH Ornament23 Clothes Line"
                };
            }
        }

        public static TheoryData<string> AgriculturalFencesTestdata
        {
            get
            {
                return new TheoryData<string>
                {
                    "BH Ornament03 Fence Straight",
                    "BH Ornament03 Fence End",
                    "BH Ornament03 Fence Cross",
                    "BH Ornament03 Fence T-Cross",
                    "BH Ornament03 Fence Corner",
                    "BH Ornament03 Fence Gate"
                };
            }
        }

        public static TheoryData<string> IndustrialOrnamentsTestdata
        {
            get
            {
                return new TheoryData<string>
                {
                    "BH Ornament24 Empty Groundplane",
                    "BH Ornament11 Pipes",
                    "BH Ornament12 Barrel Pile",
                    "BH Ornament13 WoddenBoxes",
                    "BH Ornament14 Tanks",
                    "BH Ornament15 Water Tower",
                    "BH Ornament17 Shed",
                    "BH Ornament18 Pile Iron Bars",
                    "BH Ornament19 Pile Boxes and Barrels",
                    "BH Ornament20 Heap",
                    "BH Ornament21 Large Boxes",
                    "BH Ornament22 Gangway"
                };
            }
        }

        public static TheoryData<string> IndustrialFencesTestdata
        {
            get
            {
                return new TheoryData<string>
                {
                    "BH Ornament01 Wall Straight",
                    "BH Ornament01 Wall End",
                    "BH Ornament01 Wall Cross",
                    "BH Ornament01 Wall T-Cross",
                    "BH Ornament01 Wall Corner",
                    "BH Ornament01 Wall Gate",
                    "BH Ornament01 Wall Gate 02",
                    "BH Ornament02 Wall Straight Large",
                    "BH Ornament02 Wall End Large",
                    "BH Ornament02 Wall Cross Large",
                    "BH Ornament02 Wall T-Cross Large",
                    "BH Ornament02 Wall Corner Large",
                    "BH Ornament02 Wall Gate Large"
                };
            }
        }

        public static TheoryData<string> AmusementParkTestdata
        {
            get
            {
                return new TheoryData<string>
                {
                    "",
                };
            }
        }

        #endregion

        [Theory]
        [InlineData(null)]
        [InlineData(" ")]
        [InlineData("")]
        public void GetNewOrnamentsGroup1800_IdentifierIsNullOrWhiteSpace_ShouldThrow(string identifier)
        {
            // Arrange/Act
            var ex = Assert.Throws<ArgumentNullException>(() => NewOrnamentsGroup1800.GetNewOrnamentsGroup1800(identifier, null, null, null));

            // Assert
            Assert.NotNull(ex);
            Assert.Contains("No identifier was given.", ex.Message);
        }

        [Theory]
        [MemberData(nameof(ParkPathsTestdata))]
        public void GetNewOrnamentsGroup1800_IdentifierBelongsToParkPaths_ShouldReturnCorrectGroup(string identifier)
        {
            // Arrange/Act
            var result = NewOrnamentsGroup1800.GetNewOrnamentsGroup1800(identifier, null, null, null);

            // Assert
            Assert.Equal("Ornaments", result.Faction);
            Assert.Equal("01 Park Paths", result.Group);
            Assert.Equal("OrnamentalBuilding_Park", result.Template);
        }

        [Theory]
        [MemberData(nameof(ParkFencesTestdata))]
        public void GetNewOrnamentsGroup1800_IdentifierBelongsToParkFences_ShouldReturnCorrectGroup(string identifier)
        {
            // Arrange/Act
            var result = NewOrnamentsGroup1800.GetNewOrnamentsGroup1800(identifier, null, null, null);

            // Assert
            Assert.Equal("Ornaments", result.Faction);
            Assert.Equal("02 Park Fences", result.Group);
            Assert.Equal("OrnamentalBuilding_Park", result.Template);
        }

        [Theory]
        [MemberData(nameof(ParkVegetationTestdata))]
        public void GetNewOrnamentsGroup1800_IdentifierBelongsToParkVegetation_ShouldReturnCorrectGroup(string identifier)
        {
            // Arrange/Act
            var result = NewOrnamentsGroup1800.GetNewOrnamentsGroup1800(identifier, null, null, null);

            // Assert
            Assert.Equal("Ornaments", result.Faction);
            Assert.Equal("03 Park Vegetation", result.Group);
            Assert.Equal("OrnamentalBuilding_Park", result.Template);
        }

        [Theory]
        [MemberData(nameof(ParkFountainsTestdata))]
        public void GetNewOrnamentsGroup1800_IdentifierBelongsToParkFountains_ShouldReturnCorrectGroup(string identifier)
        {
            // Arrange/Act
            var result = NewOrnamentsGroup1800.GetNewOrnamentsGroup1800(identifier, null, null, null);

            // Assert
            Assert.Equal("Ornaments", result.Faction);
            Assert.Equal("04 Park Fountains", result.Group);
            Assert.Equal("OrnamentalBuilding_Park", result.Template);
        }

        [Theory]
        [MemberData(nameof(ParkStatuesTestdata))]
        public void GetNewOrnamentsGroup1800_IdentifierBelongsToParkStatues_ShouldReturnCorrectGroup(string identifier)
        {
            // Arrange/Act
            var result = NewOrnamentsGroup1800.GetNewOrnamentsGroup1800(identifier, null, null, null);

            // Assert
            Assert.Equal("Ornaments", result.Faction);
            Assert.Equal("05 Park Statues", result.Group);
            Assert.Equal("OrnamentalBuilding_Park", result.Template);
        }

        [Theory]
        [MemberData(nameof(ParkDecorationsTestdata))]
        public void GetNewOrnamentsGroup1800_IdentifierBelongsToParkDecorations_ShouldReturnCorrectGroup(string identifier)
        {
            // Arrange/Act
            var result = NewOrnamentsGroup1800.GetNewOrnamentsGroup1800(identifier, null, null, null);

            // Assert
            Assert.Equal("Ornaments", result.Faction);
            Assert.Equal("06 Park Decorations", result.Group);
            Assert.Equal("OrnamentalBuilding_Park", result.Template);
        }

        [Theory]
        [MemberData(nameof(CityPathsTestdata))]
        public void GetNewOrnamentsGroup1800_IdentifierBelongsToCityPaths_ShouldReturnCorrectGroup(string identifier)
        {
            // Arrange/Act
            var result = NewOrnamentsGroup1800.GetNewOrnamentsGroup1800(identifier, null, null, null);

            // Assert
            Assert.Equal("Ornaments", result.Faction);
            Assert.Equal("07 City Paths", result.Group);
            Assert.Null(result.Template);
        }

        [Theory]
        [MemberData(nameof(CityFencesTestdata))]
        public void GetNewOrnamentsGroup1800_IdentifierBelongsToCityFences_ShouldReturnCorrectGroup(string identifier)
        {
            // Arrange/Act
            var result = NewOrnamentsGroup1800.GetNewOrnamentsGroup1800(identifier, null, null, null);

            // Assert
            Assert.Equal("Ornaments", result.Faction);
            Assert.Equal("08 City Fences", result.Group);
            Assert.Null(result.Template);
        }

        [Theory]
        [MemberData(nameof(CityStatuesTestdata))]
        public void GetNewOrnamentsGroup1800_IdentifierBelongsToCityStatues_ShouldReturnCorrectGroup(string identifier)
        {
            // Arrange/Act
            var result = NewOrnamentsGroup1800.GetNewOrnamentsGroup1800(identifier, null, null, null);

            // Assert
            Assert.Equal("Ornaments", result.Faction);
            Assert.Equal("09 City Statues", result.Group);
            Assert.Null(result.Template);
        }

        [Theory]
        [MemberData(nameof(CityDecorationsTestdata))]
        public void GetNewOrnamentsGroup1800_IdentifierBelongsToCityDecorations_ShouldReturnCorrectGroup(string identifier)
        {
            // Arrange/Act
            var result = NewOrnamentsGroup1800.GetNewOrnamentsGroup1800(identifier, null, null, null);

            // Assert
            Assert.Equal("Ornaments", result.Faction);
            Assert.Equal("10 City Decorations", result.Group);
            Assert.Null(result.Template);
        }

        [Theory]
        [MemberData(nameof(SpecialOrnamentsTestdata))]
        public void GetNewOrnamentsGroup1800_IdentifierBelongsToSpecialOrnaments_ShouldReturnCorrectGroup(string identifier)
        {
            // Arrange/Act
            var result = NewOrnamentsGroup1800.GetNewOrnamentsGroup1800(identifier, null, null, null);

            // Assert
            Assert.Equal("Ornaments", result.Faction);
            Assert.Equal("11 Special Ornaments", result.Group);
            Assert.Null(result.Template);
        }

        [Theory]
        [MemberData(nameof(ChristmasDecorationsTestdata))]
        public void GetNewOrnamentsGroup1800_IdentifierBelongsToChristmasDecorations_ShouldReturnCorrectGroup(string identifier)
        {
            // Arrange/Act
            var result = NewOrnamentsGroup1800.GetNewOrnamentsGroup1800(identifier, null, null, null);

            // Assert
            Assert.Equal("Ornaments", result.Faction);
            Assert.Equal("12 Christmas Decorations", result.Group);
            Assert.Null(result.Template);
        }

        [Theory]
        [MemberData(nameof(WorldsFairRewardsTestdata))]
        public void GetNewOrnamentsGroup1800_IdentifierBelongsToWorldsFairRewards_ShouldReturnCorrectGroup(string identifier)
        {
            // Arrange/Act
            var result = NewOrnamentsGroup1800.GetNewOrnamentsGroup1800(identifier, null, null, null);

            // Assert
            Assert.Equal("Ornaments", result.Faction);
            Assert.Equal("13 World's Fair Rewards", result.Group);
            Assert.Null(result.Template);
        }

        [Theory]
        [MemberData(nameof(GardensTestdata))]
        public void GetNewOrnamentsGroup1800_IdentifierBelongsToGardens_ShouldReturnCorrectGroup(string identifier)
        {
            // Arrange/Act
            var result = NewOrnamentsGroup1800.GetNewOrnamentsGroup1800(identifier, null, null, null);

            // Assert
            Assert.Equal("Ornaments", result.Faction);
            Assert.Equal("14 Gardens", result.Group);
            Assert.Equal("OrnamentalBuilding_Park", result.Template);
        }

        [Theory]
        [MemberData(nameof(AgriculturalOrnamentsTestdata))]
        public void GetNewOrnamentsGroup1800_IdentifierBelongsToAgriculturalOrnaments_ShouldReturnCorrectGroup(string identifier)
        {
            // Arrange/Act
            var result = NewOrnamentsGroup1800.GetNewOrnamentsGroup1800(identifier, null, null, null);

            // Assert
            Assert.Equal("Ornaments", result.Faction);
            Assert.Equal("15 Agricultural Ornaments", result.Group);
            Assert.Equal("OrnamentalBuilding_Park", result.Template);
        }

        [Theory]
        [MemberData(nameof(AgriculturalFencesTestdata))]
        public void GetNewOrnamentsGroup1800_IdentifierBelongsToAgriculturalFences_ShouldReturnCorrectGroup(string identifier)
        {
            // Arrange/Act
            var result = NewOrnamentsGroup1800.GetNewOrnamentsGroup1800(identifier, null, null, null);

            // Assert
            Assert.Equal("Ornaments", result.Faction);
            Assert.Equal("16 Agricultural Fences", result.Group);
            Assert.Equal("OrnamentalBuilding_Park", result.Template);
        }

        [Theory]
        [MemberData(nameof(IndustrialOrnamentsTestdata))]
        public void GetNewOrnamentsGroup1800_IdentifierBelongsToIndustrialOrnaments_ShouldReturnCorrectGroup(string identifier)
        {
            // Arrange/Act
            var result = NewOrnamentsGroup1800.GetNewOrnamentsGroup1800(identifier, null, null, null);

            // Assert
            Assert.Equal("Ornaments", result.Faction);
            Assert.Equal("17 Industrial Ornaments", result.Group);
            Assert.Equal("OrnamentalBuilding_Industrial", result.Template);
        }

        [Theory]
        [MemberData(nameof(IndustrialFencesTestdata))]
        public void GetNewOrnamentsGroup1800_IdentifierBelongsToIndustrialFences_ShouldReturnCorrectGroup(string identifier)
        {
            // Arrange/Act
            var result = NewOrnamentsGroup1800.GetNewOrnamentsGroup1800(identifier, null, null, null);

            // Assert
            Assert.Equal("Ornaments", result.Faction);
            Assert.Equal("18 IndustrialFences", result.Group);
            Assert.Equal("OrnamentalBuilding_Industrial", result.Template);
        }

        [Theory(Skip = "not yet released")]
        [MemberData(nameof(AmusementParkTestdata))]
        public void GetNewOrnamentsGroup1800_IdentifierBelongsToAmusementPark_ShouldReturnCorrectGroup(string identifier)
        {
            // Arrange/Act
            var result = NewOrnamentsGroup1800.GetNewOrnamentsGroup1800(identifier, null, null, null);

            // Assert
            Assert.Equal("Ornaments", result.Faction);
            Assert.Equal("19 Amusement Park", result.Group);
            Assert.Null(result.Template);
        }
    }
}
