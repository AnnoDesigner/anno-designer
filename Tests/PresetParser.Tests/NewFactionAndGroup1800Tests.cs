using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Moq;

namespace PresetParser.Tests
{
    public class NewFactionAndGroup1800Tests
    {
        #region testdata

        public static TheoryData<string> FarmersPublicBuildingsTestdata
        {
            get
            {
                return new TheoryData<string>
                {
                    "Logistic_01 (Marketplace)",
                    "Service_01 (Pub)",
                    "Institution_02 (Fire Department)"
                };
            }
        }

        public static TheoryData<string> FarmersProductionBuildingsTestdata
        {
            get
            {
                return new TheoryData<string>
                {
                    "Coastal_01 (Fish Coast Building)",
                    "Processing_04 (Weavery)",
                    "Food_06 (Schnapps Maker)",
                    "Factory_03 (Timber Factory)",
                    "Agriculture_05 (Timber Yard)"
                };
            }
        }

        public static TheoryData<string> FarmersFarmBuildingsTestdata
        {
            get
            {
                return new TheoryData<string>
                {
                    "Agriculture_04 (Potato Farm)",
                    "Agriculture_06 (Sheep Farm)"
                };
            }
        }

        public static TheoryData<string> WorkersPublicBuildingsTestdata
        {
            get
            {
                return new TheoryData<string>
                {
                    "Institution_01 (Police)",
                    "Service_04 (Church)",
                    "Service_02 (School)"
                };
            }
        }

        public static TheoryData<string> WorkersProductionBuildingsTestdata
        {
            get
            {
                return new TheoryData<string>
                {
                    "Factory_09 (Sailcloth Factory)",
                    "Heavy_01 (Beams Heavy Industry)",
                    "Heavy_04 (Weapons Heavy Industry)",
                    "Heavy_02 (Steel Heavy Industry)",
                    "Heavy_03 (Coal Heavy Industry)",
                    "Processing_01 (Tallow Processing)",
                    "Food_07 (Sausage Maker)",
                    "Processing_02 (Flour Processing)",
                    "Factory_02 (Soap Factory)",
                    "Processing_03 (Malt Processing)",
                    "Food_02 (Beer Maker)",
                    "Factory_04 (Brick Factory)",
                    "Food_01 (Bread Maker)"
                };
            }
        }

        public static TheoryData<string> WorkersFarmBuildingsTestdata
        {
            get
            {
                return new TheoryData<string>
                {
                    "Agriculture_08 (Pig Farm)",
                    "Agriculture_03 (Hop Farm)",
                    "Agriculture_01 (Grain Farm)"
                };
            }
        }

        public static TheoryData<string> ArtisansPublicBuildingsTestdata
        {
            get
            {
                return new TheoryData<string>
                {
                    "Institution_03 (Hospital)",
                    "Service_05 (Cabaret)",
                    "Service_07 (University)"
                };
            }
        }

        public static TheoryData<string> ArtisansProductionBuildingsTestdata
        {
            get
            {
                return new TheoryData<string>
                {
                    "Food_03 (Goulash Factory)",
                    "Food_05 (Canned Food Factory)",
                    "Processing_06 (Glass Processing)",
                    "Factory_07 (Window Factory)",
                    "Agriculture_09 (Hunter's Cabin)",
                    "Factory_05 (Fur Coat Workshop)",
                    "Workshop_03 (Sewing Machines Factory)"
                };
            }
        }

        public static TheoryData<string> ArtisansFarmBuildingsTestdata
        {
            get
            {
                return new TheoryData<string>
                {
                    "Agriculture_11 (Bell Pepper Farm)",
                    "Agriculture_02 (Cattle Farm)"
                };
            }
        }

        public static TheoryData<string> EngineersPublicBuildingsTestdata
        {
            get
            {
                return new TheoryData<string>
                {
                    "Service_03 (Bank)",
                    "Electricity_02 (Oil Power Plant)"
                };
            }
        }

        public static TheoryData<string> EngineersProductionBuildingsTestdata
        {
            get
            {
                return new TheoryData<string>
                {
                    "Factory_06 (Light Bulb Factory)",
                    "Processing_08 (Carbon Filament Processing)",
                    "Workshop_02 (Pocket Watch Workshop)",
                    "Workshop_05 (Gold Workshop)",
                    "Heavy_07 (Steam Motors Heavy Industry)",
                    "Workshop_01 (High-Wheeler Workshop)",
                    "Heavy_06 (Advanced Weapons Heavy Industry)",
                    "Processing_05 (Dynamite Processing)",
                    "Coastal_02 (Niter Coast Building)",
                    "Workshop_07 (Glasses Workshop)",
                    "Heavy_09 (Brass Heavy Industry)",
                    "Heavy_10 (Oil Heavy Industry)",
                    "Factory_01 (Concrete Factory)",
                    "Heavy_10_field (Oil Pump)"
                };
            }
        }

        public static TheoryData<string> InvestorsPublicBuildingsTestdata
        {
            get
            {
                return new TheoryData<string>
                {
                    "Service_09 (Club House)"
                };
            }
        }

        public static TheoryData<string> InvestorsProductionBuildingsTestdata
        {
            get
            {
                return new TheoryData<string>
                {
                    "Heavy_08 (Steam Carriages Heavy Industry)",
                    "Factory_10 (Chassis Factory)",
                    "Workshop_04 (Phonographs Workshop)",
                    "Processing_07 (Inlay Processing)",
                    "Workshop_06 (Jewelry Workshop)",
                    "Food_08 (Champagne Maker)"
                };
            }
        }

        public static TheoryData<string> InvestorsFarmBuildingsTestdata
        {
            get
            {
                return new TheoryData<string>
                {
                    "Agriculture_10 (Vineyard)"
                };
            }
        }

        public static TheoryData<string> JornalerosPublicBuildingsTestdata
        {
            get
            {
                return new TheoryData<string>
                {
                    "Institution_colony01_02 (Fire Department)",
                    "Institution_colony01_01 (Police)",
                    "Service_colony01_01 (Marketplace)",
                    "Service_colony01_02 (Chapel)"
                };
            }
        }

        public static TheoryData<string> JornalerosProductionBuildingsTestdata
        {
            get
            {
                return new TheoryData<string>
                {
                    "Processing_colony01_02 (Poncho Maker)",
                    "Coastal_colony01_01 (Pearls Coast Building)",
                    "Food_colony01_04 (Fried Banana Maker)",
                    "Coastal_colony01_02 (Fish Coast Building)",
                    "Factory_colony01_02 (Sailcloth Factory)",
                    "Factory_colony01_01 (Timber Factory)",
                    "Agriculture_colony01_06 (Timber Yard)",
                    "Factory_colony01_03 (Cotton Cloth Processing)",
                    "Food_colony01_01 (Rum Maker)"
                };
            }
        }

        public static TheoryData<string> JornalerosFarmBuildingsTestdata
        {
            get
            {
                return new TheoryData<string>
                {
                    "Agriculture_colony01_11 (Alpaca Farm)",
                    "Agriculture_colony01_08 (Banana Farm)",
                    "Agriculture_colony01_03 (Cotton Farm)",
                    "Agriculture_colony01_01 (Sugar Cane Farm)",
                    "Agriculture_colony01_05 (Caoutchouc Farm)"
                };
            }
        }

        public static TheoryData<string> ObrerosPublicBuildingsTestdata
        {
            get
            {
                return new TheoryData<string>
                {
                    "Institution_colony01_03 (Hospital)",
                    "Service_colony01_03 (Boxing Arena)"
                };
            }
        }

        public static TheoryData<string> ObrerosProductionBuildingsTestdata
        {
            get
            {
                return new TheoryData<string>
                {
                    "Food_colony01_02 (Chocolate Maker)",
                    "Workshop_colony01_01 (Cigars Workshop)",
                    "Factory_colony01_07 (Bombin Maker)",
                    "Factory_colony01_06 (Felt Maker)",
                    "Food_colony01_03 (Coffee Maker)",
                    "Food_colony01_05 (Burrito Maker)",
                    "Processing_colony01_01 (Sugar Processing)",
                    "Processing_colony01_03 (Inlay Processing)",
                    "Heavy_colony01_01 (Oil Heavy Industry)",
                    "Heavy_colony01_01_field (Oil Pump)"
                };
            }
        }

        public static TheoryData<string> ObrerosFarmBuildingsTestdata
        {
            get
            {
                return new TheoryData<string>
                {
                    "Agriculture_colony01_09 (Cattle Farm)",
                    "Agriculture_colony01_04 (Cocoa Farm)",
                    "Agriculture_colony01_02 (Tobacco Farm)",
                    "Agriculture_colony01_07 (Coffee Beans Farm)",
                    "Agriculture_colony01_10 (Corn Farm)"
                };
            }
        }

        public static TheoryData<string> SpecialBuildingsTestdata
        {
            get
            {
                return new TheoryData<string>
                {
                    "Guild_house",
                    "Town hall"
                };
            }
        }

        public static TheoryData<string> OrnamentalsTestdata
        {
            get
            {
                return new TheoryData<string>
                {
                    "Culture_preorder_statue",
                    "Uplay_ornament_2x1_lion_statue",
                    "Uplay_ornament_2x2_pillar_chess_park",
                    "Uplay_ornament_3x2_large_fountain"
                };
            }
        }

        #endregion

        [Theory]
        [InlineData(null)]
        [InlineData(" ")]
        [InlineData("")]
        public void GetNewFactionAndGroup1800_IdentifierIsNullOrWhiteSpace_ShouldThrow(string identifier)
        {
            // Arrange/Act
            var ex = Assert.Throws<ArgumentNullException>(() => NewFactionAndGroup1800.GetNewFactionAndGroup1800(identifier, null, null));

            // Assert
            Assert.NotNull(ex);
            Assert.Contains("No identifier was given.", ex.Message);
        }

        #region Farmers tests

        [Theory]
        [MemberData(nameof(FarmersPublicBuildingsTestdata))]
        [MemberData(nameof(FarmersProductionBuildingsTestdata))]
        [MemberData(nameof(FarmersFarmBuildingsTestdata))]
        public void GetNewFactionAndGroup1800_IdentifierBelongsToFarmers_ShouldReturnCorrectFaction(string identifier)
        {
            // Arrange/Act
            var result = NewFactionAndGroup1800.GetNewFactionAndGroup1800(identifier, null, null);

            // Assert
            Assert.Contains("(1) Farmers", result[0]);
        }

        [Theory]
        [MemberData(nameof(FarmersPublicBuildingsTestdata))]
        public void GetNewFactionAndGroup1800_IdentifierBelongsToFarmersPublicBuildings_ShouldReturnCorrectGroup(string identifier)
        {
            // Arrange/Act
            var result = NewFactionAndGroup1800.GetNewFactionAndGroup1800(identifier, null, null);

            // Assert
            Assert.Contains("Public Buildings", result[1]);
            Assert.Contains("(1) Farmers", result[0]);
        }

        [Theory]
        [MemberData(nameof(FarmersProductionBuildingsTestdata))]
        public void GetNewFactionAndGroup1800_IdentifierBelongsToFarmersProductionBuildings_ShouldReturnCorrectGroup(string identifier)
        {
            // Arrange/Act
            var result = NewFactionAndGroup1800.GetNewFactionAndGroup1800(identifier, null, null);

            // Assert
            Assert.Contains("Production Buildings", result[1]);
            Assert.Contains("(1) Farmers", result[0]);
        }

        [Theory]
        [MemberData(nameof(FarmersFarmBuildingsTestdata))]
        public void GetNewFactionAndGroup1800_IdentifierBelongsToFarmersFarmBuildings_ShouldReturnCorrectGroup(string identifier)
        {
            // Arrange/Act
            var result = NewFactionAndGroup1800.GetNewFactionAndGroup1800(identifier, null, null);

            // Assert
            Assert.Contains("Farm Buildings", result[1]);
            Assert.Contains("(1) Farmers", result[0]);
        }

        #endregion

        #region Workers tests

        [Theory]
        [MemberData(nameof(WorkersPublicBuildingsTestdata))]
        [MemberData(nameof(WorkersProductionBuildingsTestdata))]
        [MemberData(nameof(WorkersFarmBuildingsTestdata))]
        public void GetNewFactionAndGroup1800_IdentifierBelongsToWorkers_ShouldReturnCorrectFaction(string identifier)
        {
            // Arrange/Act
            var result = NewFactionAndGroup1800.GetNewFactionAndGroup1800(identifier, null, null);

            // Assert
            Assert.Contains("(2) Workers", result[0]);
        }

        [Theory]
        [MemberData(nameof(WorkersPublicBuildingsTestdata))]
        public void GetNewFactionAndGroup1800_IdentifierBelongsToWorkersPublicBuildings_ShouldReturnCorrectGroup(string identifier)
        {
            // Arrange/Act
            var result = NewFactionAndGroup1800.GetNewFactionAndGroup1800(identifier, null, null);

            // Assert
            Assert.Contains("Public Buildings", result[1]);
            Assert.Contains("(2) Workers", result[0]);
        }

        [Theory]
        [MemberData(nameof(WorkersProductionBuildingsTestdata))]
        public void GetNewFactionAndGroup1800_IdentifierBelongsToWorkersProductionBuildings_ShouldReturnCorrectGroup(string identifier)
        {
            // Arrange/Act
            var result = NewFactionAndGroup1800.GetNewFactionAndGroup1800(identifier, null, null);

            // Assert
            Assert.Contains("Production Buildings", result[1]);
            Assert.Contains("(2) Workers", result[0]);
        }

        [Theory]
        [MemberData(nameof(WorkersFarmBuildingsTestdata))]
        public void GetNewFactionAndGroup1800_IdentifierBelongsToWorkersFarmBuildings_ShouldReturnCorrectGroup(string identifier)
        {
            // Arrange/Act
            var result = NewFactionAndGroup1800.GetNewFactionAndGroup1800(identifier, null, null);

            // Assert
            Assert.Contains("Farm Buildings", result[1]);
            Assert.Contains("(2) Workers", result[0]);
        }

        #endregion

        #region Artisans tests

        [Theory]
        [MemberData(nameof(ArtisansPublicBuildingsTestdata))]
        [MemberData(nameof(ArtisansProductionBuildingsTestdata))]
        [MemberData(nameof(ArtisansFarmBuildingsTestdata))]
        public void GetNewFactionAndGroup1800_IdentifierBelongsToArtisans_ShouldReturnCorrectFaction(string identifier)
        {
            // Arrange/Act
            var result = NewFactionAndGroup1800.GetNewFactionAndGroup1800(identifier, null, null);

            // Assert
            Assert.Contains("(3) Artisans", result[0]);
        }

        [Theory]
        [MemberData(nameof(ArtisansPublicBuildingsTestdata))]
        public void GetNewFactionAndGroup1800_IdentifierBelongsToArtisansPublicBuildings_ShouldReturnCorrectGroup(string identifier)
        {
            // Arrange/Act
            var result = NewFactionAndGroup1800.GetNewFactionAndGroup1800(identifier, null, null);

            // Assert
            Assert.Contains("Public Buildings", result[1]);
            Assert.Contains("(3) Artisans", result[0]);
        }

        [Theory]
        [MemberData(nameof(ArtisansProductionBuildingsTestdata))]
        public void GetNewFactionAndGroup1800_IdentifierBelongsToArtisansProductionBuildings_ShouldReturnCorrectGroup(string identifier)
        {
            // Arrange/Act
            var result = NewFactionAndGroup1800.GetNewFactionAndGroup1800(identifier, null, null);

            // Assert
            Assert.Contains("Production Buildings", result[1]);
            Assert.Contains("(3) Artisans", result[0]);
        }

        [Theory]
        [MemberData(nameof(ArtisansFarmBuildingsTestdata))]
        public void GetNewFactionAndGroup1800_IdentifierBelongsToArtisansFarmBuildings_ShouldReturnCorrectGroup(string identifier)
        {
            // Arrange/Act
            var result = NewFactionAndGroup1800.GetNewFactionAndGroup1800(identifier, null, null);

            // Assert
            Assert.Contains("Farm Buildings", result[1]);
            Assert.Contains("(3) Artisans", result[0]);
        }

        #endregion

        #region Engineers tests

        [Theory]
        [MemberData(nameof(EngineersPublicBuildingsTestdata))]
        [MemberData(nameof(EngineersProductionBuildingsTestdata))]
        public void GetNewFactionAndGroup1800_IdentifierBelongsToEngineers_ShouldReturnCorrectFaction(string identifier)
        {
            // Arrange/Act
            var result = NewFactionAndGroup1800.GetNewFactionAndGroup1800(identifier, null, null);

            // Assert
            Assert.Contains("(4) Engineers", result[0]);
        }

        [Theory]
        [MemberData(nameof(EngineersPublicBuildingsTestdata))]
        public void GetNewFactionAndGroup1800_IdentifierBelongsToEngineersPublicBuildings_ShouldReturnCorrectGroup(string identifier)
        {
            // Arrange/Act
            var result = NewFactionAndGroup1800.GetNewFactionAndGroup1800(identifier, null, null);

            // Assert
            Assert.Contains("Public Buildings", result[1]);
            Assert.Contains("(4) Engineers", result[0]);
        }

        [Theory]
        [MemberData(nameof(EngineersProductionBuildingsTestdata))]
        public void GetNewFactionAndGroup1800_IdentifierBelongsToEngineersProductionBuildings_ShouldReturnCorrectGroup(string identifier)
        {
            // Arrange/Act
            var result = NewFactionAndGroup1800.GetNewFactionAndGroup1800(identifier, null, null);

            // Assert
            Assert.Contains("Production Buildings", result[1]);
            Assert.Contains("(4) Engineers", result[0]);
        }

        #endregion

        #region Investors tests

        [Theory]
        [MemberData(nameof(InvestorsPublicBuildingsTestdata))]
        [MemberData(nameof(InvestorsProductionBuildingsTestdata))]
        [MemberData(nameof(InvestorsFarmBuildingsTestdata))]
        public void GetNewFactionAndGroup1800_IdentifierBelongsToInvestors_ShouldReturnCorrectFaction(string identifier)
        {
            // Arrange/Act
            var result = NewFactionAndGroup1800.GetNewFactionAndGroup1800(identifier, null, null);

            // Assert
            Assert.Contains("(5) Investors", result[0]);
        }

        [Theory]
        [MemberData(nameof(InvestorsPublicBuildingsTestdata))]
        public void GetNewFactionAndGroup1800_IdentifierBelongsToInvestorsPublicBuildings_ShouldReturnCorrectGroup(string identifier)
        {
            // Arrange/Act
            var result = NewFactionAndGroup1800.GetNewFactionAndGroup1800(identifier, null, null);

            // Assert
            Assert.Contains("Public Buildings", result[1]);
            Assert.Contains("(5) Investors", result[0]);
        }

        [Theory]
        [MemberData(nameof(InvestorsProductionBuildingsTestdata))]
        public void GetNewFactionAndGroup1800_IdentifierBelongsToInvestorsProductionBuildings_ShouldReturnCorrectGroup(string identifier)
        {
            // Arrange/Act
            var result = NewFactionAndGroup1800.GetNewFactionAndGroup1800(identifier, null, null);

            // Assert
            Assert.Contains("Production Buildings", result[1]);
            Assert.Contains("(5) Investors", result[0]);
        }

        [Theory]
        [MemberData(nameof(InvestorsFarmBuildingsTestdata))]
        public void GetNewFactionAndGroup1800_IdentifierBelongsToInvestorsFarmBuildings_ShouldReturnCorrectGroup(string identifier)
        {
            // Arrange/Act
            var result = NewFactionAndGroup1800.GetNewFactionAndGroup1800(identifier, null, null);

            // Assert
            Assert.Contains("Farm Buildings", result[1]);
            Assert.Contains("(5) Investors", result[0]);
        }

        #endregion

        #region Jornaleros tests

        [Theory]
        [MemberData(nameof(JornalerosPublicBuildingsTestdata))]
        [MemberData(nameof(JornalerosProductionBuildingsTestdata))]
        [MemberData(nameof(JornalerosFarmBuildingsTestdata))]
        public void GetNewFactionAndGroup1800_IdentifierBelongsToJornaleros_ShouldReturnCorrectFaction(string identifier)
        {
            // Arrange/Act
            var result = NewFactionAndGroup1800.GetNewFactionAndGroup1800(identifier, null, null);

            // Assert
            Assert.Contains("(7) Jornaleros", result[0]);
        }

        [Theory]
        [MemberData(nameof(JornalerosPublicBuildingsTestdata))]
        public void GetNewFactionAndGroup1800_IdentifierBelongsToJornalerosPublicBuildings_ShouldReturnCorrectGroup(string identifier)
        {
            // Arrange/Act
            var result = NewFactionAndGroup1800.GetNewFactionAndGroup1800(identifier, null, null);

            // Assert
            Assert.Contains("Public Buildings", result[1]);
            Assert.Contains("(7) Jornaleros", result[0]);
        }

        [Theory]
        [MemberData(nameof(JornalerosProductionBuildingsTestdata))]
        public void GetNewFactionAndGroup1800_IdentifierBelongsToJornalerosProductionBuildings_ShouldReturnCorrectGroup(string identifier)
        {
            // Arrange/Act
            var result = NewFactionAndGroup1800.GetNewFactionAndGroup1800(identifier, null, null);

            // Assert
            Assert.Contains("Production Buildings", result[1]);
            Assert.Contains("(7) Jornaleros", result[0]);
        }

        [Theory]
        [MemberData(nameof(JornalerosFarmBuildingsTestdata))]
        public void GetNewFactionAndGroup1800_IdentifierBelongsToJornalerosFarmBuildings_ShouldReturnCorrectGroup(string identifier)
        {
            // Arrange/Act
            var result = NewFactionAndGroup1800.GetNewFactionAndGroup1800(identifier, null, null);

            // Assert
            Assert.Contains("Farm Buildings", result[1]);
            Assert.Contains("(7) Jornaleros", result[0]);
        }

        #endregion

        #region Obreros tests

        [Theory]
        [MemberData(nameof(ObrerosPublicBuildingsTestdata))]
        [MemberData(nameof(ObrerosProductionBuildingsTestdata))]
        [MemberData(nameof(ObrerosFarmBuildingsTestdata))]
        public void GetNewFactionAndGroup1800_IdentifierBelongsToObreros_ShouldReturnCorrectFaction(string identifier)
        {
            // Arrange/Act
            var result = NewFactionAndGroup1800.GetNewFactionAndGroup1800(identifier, null, null);

            // Assert
            Assert.Contains("(8) Obreros", result[0]);
        }

        [Theory]
        [MemberData(nameof(ObrerosPublicBuildingsTestdata))]
        public void GetNewFactionAndGroup1800_IdentifierBelongsToObrerosPublicBuildings_ShouldReturnCorrectGroup(string identifier)
        {
            // Arrange/Act
            var result = NewFactionAndGroup1800.GetNewFactionAndGroup1800(identifier, null, null);

            // Assert
            Assert.Contains("Public Buildings", result[1]);
            Assert.Contains("(8) Obreros", result[0]);
        }

        [Theory]
        [MemberData(nameof(ObrerosProductionBuildingsTestdata))]
        public void GetNewFactionAndGroup1800_IdentifierBelongsToObrerosProductionBuildings_ShouldReturnCorrectGroup(string identifier)
        {
            // Arrange/Act
            var result = NewFactionAndGroup1800.GetNewFactionAndGroup1800(identifier, null, null);

            // Assert
            Assert.Contains("Production Buildings", result[1]);
            Assert.Contains("(8) Obreros", result[0]);
        }

        [Theory]
        [MemberData(nameof(ObrerosFarmBuildingsTestdata))]
        public void GetNewFactionAndGroup1800_IdentifierBelongsToObrerosFarmBuildings_ShouldReturnCorrectGroup(string identifier)
        {
            // Arrange/Act
            var result = NewFactionAndGroup1800.GetNewFactionAndGroup1800(identifier, null, null);

            // Assert
            Assert.Contains("Farm Buildings", result[1]);
            Assert.Contains("(8) Obreros", result[0]);
        }

        #endregion

        #region Special buildings tests

        [Theory]
        [MemberData(nameof(SpecialBuildingsTestdata))]
        public void GetNewFactionAndGroup1800_IdentifierBelongsToSpecialBuildings_ShouldReturnCorrectFaction(string identifier)
        {
            // Arrange/Act
            var result = NewFactionAndGroup1800.GetNewFactionAndGroup1800(identifier, null, null);

            // Assert
            Assert.Contains("All Worlds", result[0]);
        }

        [Theory]
        [MemberData(nameof(SpecialBuildingsTestdata))]
        public void GetNewFactionAndGroup1800_IdentifierBelongsToSpecialBuildings_ShouldReturnCorrectGroup(string identifier)
        {
            // Arrange/Act
            var result = NewFactionAndGroup1800.GetNewFactionAndGroup1800(identifier, null, null);

            // Assert
            Assert.Contains("Special Buildings", result[1]);
            Assert.Contains("All Worlds", result[0]);
        }

        #endregion

        #region Ornamentals tests

        [Theory]
        [MemberData(nameof(OrnamentalsTestdata))]
        public void GetNewFactionAndGroup1800_IdentifierBelongsToOrnamentals_ShouldReturnCorrectFaction(string identifier)
        {
            // Arrange/Act
            var result = NewFactionAndGroup1800.GetNewFactionAndGroup1800(identifier, null, null);

            // Assert
            Assert.Contains("All Worlds", result[0]);
        }

        [Theory]
        [MemberData(nameof(OrnamentalsTestdata))]
        public void GetNewFactionAndGroup1800_IdentifierBelongsToOrnamentals_ShouldReturnCorrectGroup(string identifier)
        {
            // Arrange/Act
            var result = NewFactionAndGroup1800.GetNewFactionAndGroup1800(identifier, null, null);

            // Assert
            Assert.Null(result[1]);
            Assert.Contains("All Worlds", result[0]);
        }

        #endregion
    }
}
