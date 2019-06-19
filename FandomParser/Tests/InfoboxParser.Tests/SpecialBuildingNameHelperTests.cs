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
        [InlineData("Bombin Weaver", "Bomb­ín Weaver")]
        [InlineData("Caoutchouc", "Caoutchouc Plantation")]
        [InlineData("Fried Plaintain Kitchen", "Fried Plantain Kitchen")]
        [InlineData("World's Fair: Foundations", "World's Fair|World's Fair: Foundations")]
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
