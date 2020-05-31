using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FandomParser.Core;
using InfoboxParser.Models;
using InfoboxParser.Parser;
using Xunit;
using Xunit.Abstractions;

namespace InfoboxParser.Tests
{
    public class ParserMultipleRegionsTests
    {
        private static readonly ICommons _mockedCommons;
        private readonly ITestOutputHelper _output;
        private static readonly ISpecialBuildingNameHelper _mockedSpecialBuildingNameHelper;
        private static readonly IRegionHelper _mockedRegionHelper;
        private static readonly List<string> _regionList2Regions;
        private static readonly List<string> _regionList3Regions;

        #region ctor

        static ParserMultipleRegionsTests()
        {
            _mockedCommons = Commons.Instance;
            _mockedSpecialBuildingNameHelper = new SpecialBuildingNameHelper();
            _mockedRegionHelper = new RegionHelper();

            _regionList2Regions = new List<string> { "A", "B" };
            _regionList3Regions = new List<string> { "A", "B", "C" };
        }

        public ParserMultipleRegionsTests(ITestOutputHelper testOutputHelperToUse)
        {
            _output = testOutputHelperToUse;
        }

        #endregion

        private IParserMultipleRegions GetParser(ICommons commonsToUse = null,
            ISpecialBuildingNameHelper specialBuildingNameHelperToUse = null,
            IRegionHelper regionHelperToUse = null)
        {
            return new ParserMultipleRegions(commonsToUse ?? _mockedCommons,
                specialBuildingNameHelperToUse ?? _mockedSpecialBuildingNameHelper,
                regionHelperToUse ?? _mockedRegionHelper);
        }

        #region test data       

        public static TheoryData<string, string, List<string>> BuildingIconTestData
        {
            get
            {
                return new TheoryData<string, string, List<string>>
                {
                    { "|Building Icon = Charcoal_kiln.png", "Charcoal_kiln.png", _regionList2Regions },
                    { "|Building Icon = Charcoal_kiln.png", "Charcoal_kiln.png", _regionList3Regions },
                    { "|Building Icon = Furs.png", "Furs.png", _regionList2Regions },
                    { "|Building Icon = Furs.png", "Furs.png", _regionList3Regions },
                    { "|Building Icon      =    Furs.png   ", "Furs.png", _regionList2Regions },
                    { "|Building Icon      =    Furs.png   ", "Furs.png", _regionList3Regions },
                    { "|Building Icon = Furs.jpeg", "Furs.jpeg", _regionList2Regions },
                    { "|Building Icon = Furs.jpeg", "Furs.jpeg", _regionList3Regions },
                    { "|Building Icon = Arctic Lodge.png", "Arctic Lodge.png", _regionList2Regions },
                    { "|Building Icon = Arctic Lodge.png", "Arctic Lodge.png", _regionList3Regions },
                    { "|Building Icon = Bear Hunting Cabin.png", "Bear Hunting Cabin.png", _regionList2Regions },
                    { "|Building Icon = Bear Hunting Cabin.png", "Bear Hunting Cabin.png", _regionList3Regions },
                    { "|Building Icon = Cocoa_0.png", "Cocoa_0.png", _regionList2Regions },
                    { "|Building Icon = Cocoa_0.png", "Cocoa_0.png", _regionList3Regions },
                    { "|Building Icon = Icon electric works gas 0.png ", "Icon electric works gas 0.png", _regionList2Regions },
                    { "|Building Icon = Icon electric works gas 0.png ", "Icon electric works gas 0.png", _regionList3Regions },
                    { "|Building Icon = Harbourmaster's Office.png", "Harbourmaster's Office.png", _regionList2Regions },
                    { "|Building Icon = Harbourmaster's Office.png", "Harbourmaster's Office.png", _regionList3Regions },
                    { "|Building Icon = Harbourmaster´s Office.png", "Harbourmaster´s Office.png", _regionList2Regions },
                    { "|Building Icon = Harbourmaster´s Office.png", "Harbourmaster´s Office.png", _regionList3Regions },
                    { "|Building Icon = Harbourmaster`s Office.png", "Harbourmaster`s Office.png", _regionList2Regions },
                    { "|Building Icon = Harbourmaster`s Office.png", "Harbourmaster`s Office.png", _regionList3Regions },
                };
            }
        }

        #endregion

        #region BuildingIcon tests

        [Theory]
        [InlineData("dummy")]
        [InlineData("|Building Icon = ")]
        public void GetInfobox_2Regions_WikiTextContainsNoBuildingIcon_ShouldReturnEmpty(string input)
        {
            // Arrange
            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(input, _regionList2Regions);

            // Assert
            Assert.Equal(string.Empty, result[0].Icon);
        }

        [Theory]
        [InlineData("dummy")]
        [InlineData("|Building Icon = ")]
        public void GetInfobox_3Regions_WikiTextContainsNoBuildingIcon_ShouldReturnEmpty(string input)
        {
            // Arrange
            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(input, _regionList3Regions);

            // Assert
            Assert.Equal(string.Empty, result[0].Icon);
        }

        [Theory]
        [MemberData(nameof(BuildingIconTestData))]
        public void GetInfobox_WikiTextContainsBuildingIcon_ShouldReturnCorrectValue(string input, string expectedIcon, List<string> regionList)
        {
            // Arrange
            _output.WriteLine($"{nameof(input)}: {input}");

            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(input, regionList);

            // Assert
            Assert.Equal(expectedIcon, result[0].Icon);
        }

        #endregion
    }
}
