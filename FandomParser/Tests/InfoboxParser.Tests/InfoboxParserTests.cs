using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FandomParser.Core;
using Moq;
using Xunit;
using System.IO;
using System.Reflection;
using FandomParser.Core.Presets.Models;
using InfoboxParser.Tests.Attributes;
using InfoboxParser.Models;

namespace InfoboxParser.Tests
{
    public class InfoboxParserTests
    {
        private static readonly ICommons _mockedCommons;
        private static readonly ISpecialBuildingNameHelper _mockedSpecialBuildingNameHelper;
        private static readonly IRegionHelper _mockedRegionHelper;

        static InfoboxParserTests()
        {
            _mockedCommons = Commons.Instance;
            _mockedSpecialBuildingNameHelper = new SpecialBuildingNameHelper();
            _mockedRegionHelper = new RegionHelper();
        }

        private InfoboxParser GetParser(ICommons commonsToUse = null,
            ISpecialBuildingNameHelper specialBuildingNameHelperToUse = null,
            IRegionHelper regionHelperToUse = null)
        {
            return new InfoboxParser(commonsToUse ?? _mockedCommons,
                specialBuildingNameHelperToUse ?? _mockedSpecialBuildingNameHelper,
                regionHelperToUse ?? _mockedRegionHelper);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(" ")]
        [InlineData("")]
        public void GetInfobox_WikiTextIsNullOrWhiteSpace_ShouldReturnNull(string input)
        {
            // Arrange
            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetInfobox_WikiTextContainsInfobox_ShouldReturnSingleResult()
        {
            // Arrange
            var input = "{{Infobox Buildings";

            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public void GetInfobox_WikiTextContainsInfoboxForBothWorlds_ShouldReturnMultipleResults()
        {
            // Arrange
            var input = "{{Infobox Buildings Old and New World";

            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void GetInfobox_WikiTextContainsInfoboxFor2Regions_ShouldReturnMultipleResults()
        {
            // Arrange
            var input = "{{Infobox Buildings 2 Regions";

            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(2, result.Count);
        }
    }
}
