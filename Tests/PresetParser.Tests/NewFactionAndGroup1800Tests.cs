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
        }

        [Theory]
        [MemberData(nameof(FarmersProductionBuildingsTestdata))]
        public void GetNewFactionAndGroup1800_IdentifierBelongsToFarmersProductionBuildings_ShouldReturnCorrectGroup(string identifier)
        {
            // Arrange/Act
            var result = NewFactionAndGroup1800.GetNewFactionAndGroup1800(identifier, null, null);

            // Assert
            Assert.Contains("Production Buildings", result[1]);
        }

        [Theory]
        [MemberData(nameof(FarmersFarmBuildingsTestdata))]
        public void GetNewFactionAndGroup1800_IdentifierBelongsToFarmersFarmBuildings_ShouldReturnCorrectGroup(string identifier)
        {
            // Arrange/Act
            var result = NewFactionAndGroup1800.GetNewFactionAndGroup1800(identifier, null, null);

            // Assert
            Assert.Contains("Farm Buildings", result[1]);
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
        }

        [Theory]
        [MemberData(nameof(WorkersProductionBuildingsTestdata))]
        public void GetNewFactionAndGroup1800_IdentifierBelongsToWorkersProductionBuildings_ShouldReturnCorrectGroup(string identifier)
        {
            // Arrange/Act
            var result = NewFactionAndGroup1800.GetNewFactionAndGroup1800(identifier, null, null);

            // Assert
            Assert.Contains("Production Buildings", result[1]);
        }

        [Theory]
        [MemberData(nameof(WorkersFarmBuildingsTestdata))]
        public void GetNewFactionAndGroup1800_IdentifierBelongsToWorkersFarmBuildings_ShouldReturnCorrectGroup(string identifier)
        {
            // Arrange/Act
            var result = NewFactionAndGroup1800.GetNewFactionAndGroup1800(identifier, null, null);

            // Assert
            Assert.Contains("Farm Buildings", result[1]);
        }

        #endregion
    }
}
