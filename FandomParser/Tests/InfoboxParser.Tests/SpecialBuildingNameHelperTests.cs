using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace InfoboxParser.Tests
{
    public class SpecialBuildingNameHelperTests
    {
        [Theory]
        [InlineData("no special name", "no special name")]
        [InlineData("Bombin Weaver", "Bomb­ín Weaver")]
        [InlineData("Caoutchouc", "Caoutchouc Plantation")]
        [InlineData("Fried Plaintain Kitchen", "Fried Plantain Kitchen")]
        [InlineData("World's Fair: Foundations", "World's Fair|World's Fair: Foundations")]
        [InlineData("World's Fair", "World's Fair|World's Fair: Foundations")]
        [InlineData("Airship Hangar", "Airship Hangar|Airship Hangar: Foundations")]
        [InlineData("Explorer Residence", "Explorer Residence|Explorer Shelter")]
        [InlineData("Explorer Shelter", "Explorer Residence|Explorer Shelter")]
        [InlineData("Deep Gold Mine", "Gold Mine|Deep Gold Mine")]
        [InlineData("Pristine Hunting Cabin", "Hunting Cabin|Pristine Hunting Cabin")]
        [InlineData("Lumberjack", "Lumberjack's Hut")]
        [InlineData("Saltpetre Works", "Saltpeter Works")]
        public void CheckSpecialBuildingName_BuildingNameIsSet_ShouldReturnCorrectValue(string buildingName, string expectedValue)
        {
            // Arrange
            var helper = new SpecialBuildingNameHelper();

            // Act
            var result = helper.CheckSpecialBuildingName(buildingName);

            // Assert
            Assert.Equal(expectedValue, result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void CheckSpecialBuildingName_BuildingNameIsNullOrWhitespace_ShouldReturnInput(string buildingName)
        {
            // Arrange
            var helper = new SpecialBuildingNameHelper();

            // Act
            var result = helper.CheckSpecialBuildingName(buildingName);

            // Assert
            Assert.Equal(buildingName, result);
        }
    }
}
