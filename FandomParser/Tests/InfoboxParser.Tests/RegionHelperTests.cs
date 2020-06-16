using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FandomParser.Core.Presets.Models;
using InfoboxParser.Models;
using Xunit;
using Xunit.Abstractions;

namespace InfoboxParser.Tests
{
    public class RegionHelperTests
    {
        private readonly ITestOutputHelper _output;

        #region ctor       

        public RegionHelperTests(ITestOutputHelper testOutputHelperToUse)
        {
            _output = testOutputHelperToUse;
        }

        #endregion

        private IRegionHelper GetHelper()
        {
            return new RegionHelper();
        }

        [Theory]
        [InlineData(null)]
        [InlineData(" ")]
        [InlineData("")]
        public void GetRegion_RegionNameIsNullOrWhiteSpace_ShouldReturnUnknown(string regionName)
        {
            // Arrange
            var helper = GetHelper();

            // Act
            var result = helper.GetRegion(regionName);

            // Assert
            Assert.Equal(WorldRegion.Unknown, result);
        }

        [Theory]
        [InlineData("OW", WorldRegion.OldWorld)]
        [InlineData("Old World", WorldRegion.OldWorld)]
        [InlineData("The Old World", WorldRegion.OldWorld)]
        [InlineData("NW", WorldRegion.NewWorld)]
        [InlineData("The New World", WorldRegion.NewWorld)]
        [InlineData("New World", WorldRegion.NewWorld)]
        [InlineData("The Arctic", WorldRegion.Arctic)]
        [InlineData("Arctic", WorldRegion.Arctic)]
        public void GetRegion_RegionNameIsGiven_ShouldReturnExpectedResult(string regionName, WorldRegion expectedRegion)
        {
            // Arrange
            var helper = GetHelper();

            // Act
            var result = helper.GetRegion(regionName);

            // Assert
            Assert.Equal(expectedRegion, result);
        }

        [Fact]
        public void GetRegion_RegionNameIsUnknown_ShouldReturnUnknown()
        {
            // Arrange
            var regionName = "dummy";
            var helper = GetHelper();

            // Act
            var result = helper.GetRegion(regionName);

            // Assert
            Assert.Equal(WorldRegion.Unknown, result);
        }
    }
}
