using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FandomParser.Core;
using FandomParser.Core.Presets.Models;
using InfoboxParser.Models;
using InfoboxParser.Parser;
using InfoboxParser.Tests.Attributes;
using Xunit;

namespace InfoboxParser.Tests
{
    public class ParserSingleRegionTests
    {
        private static readonly ICommons _mockedCommons;
        private static readonly ISpecialBuildingNameHelper _mockedSpecialBuildingNameHelper;
        private static readonly ITitleParserSingle _mockedTitleParserSingle;

        private static readonly string testDataSchnapps_Distillery;
        private static readonly string testDataBakery;
        private static readonly string testDataCannery;
        private static readonly string testDataChapel;
        private static readonly string testDataEmpty;

        static ParserSingleRegionTests()
        {
            _mockedCommons = Commons.Instance;
            _mockedSpecialBuildingNameHelper = new SpecialBuildingNameHelper();
            _mockedTitleParserSingle = new TitleParserSingle(_mockedCommons, _mockedSpecialBuildingNameHelper);

            string basePath = AppDomain.CurrentDomain.BaseDirectory;

            testDataSchnapps_Distillery = File.ReadAllText(Path.Combine(basePath, "Testdata", "Schnapps_Distillery.infobox"));
            testDataBakery = File.ReadAllText(Path.Combine(basePath, "Testdata", "Bakery.infobox"));
            testDataCannery = File.ReadAllText(Path.Combine(basePath, "Testdata", "Cannery.infobox"));
            testDataChapel = File.ReadAllText(Path.Combine(basePath, "Testdata", "Chapel.infobox"));
            testDataEmpty = File.ReadAllText(Path.Combine(basePath, "Testdata", "empty.infobox"));
        }

        private IParser GetParser(ICommons commonsToUse = null,
            ITitleParserSingle titleParserSingleToUse = null)
        {
            return new ParserSingleRegion(commonsToUse ?? _mockedCommons,
                titleParserSingleToUse ?? _mockedTitleParserSingle);
        }

        #region test data

        public static TheoryData<string, BuildingType> BuildingTypeTestData
        {
            get
            {
                return new TheoryData<string, BuildingType>
                {
                    { testDataSchnapps_Distillery, BuildingType.Production },
                    { testDataBakery, BuildingType.Production },
                    { testDataCannery, BuildingType.Production },
                    { testDataChapel, BuildingType.PublicService },
                };
            }
        }

        #endregion

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

        #region BuildingType tests

        [Fact]
        public void GetInfobox_WikiTextContainsNoBuildingType_ShouldReturnUnknown()
        {
            // Arrange
            var parser = GetParser();

            // Act
            var result = parser.GetInfobox("dummy");

            // Assert
            Assert.Equal(BuildingType.Unknown, result[0].Type);
        }

        [Fact]
        public void GetInfobox_WikiTextContainsUnknownBuildingType_ShouldReturnUnknown()
        {
            // Arrange
            var parser = GetParser();

            // Act
            var result = parser.GetInfobox("|Building Type = something special");

            // Assert
            Assert.Equal(BuildingType.Unknown, result[0].Type);
        }

        [Theory]
        [InlineData("|Building Type = Production", BuildingType.Production)]
        [InlineData("|Building Type = Public Service", BuildingType.PublicService)]
        [InlineData("|Building Type = Residence", BuildingType.Residence)]
        [InlineData("|Building Type = Infrastructure", BuildingType.Infrastructure)]
        [InlineData("|Building Type = Institution", BuildingType.Institution)]
        [InlineData("|Building Type = Ornament", BuildingType.Ornament)]
        [InlineData("|Building Type = Monument", BuildingType.Monument)]
        [InlineData("|Building Type = Administration", BuildingType.Administration)]
        [InlineData("|Building Type = Harbour", BuildingType.Harbour)]
        [InlineData("|Building Type = Street", BuildingType.Street)]
        [InlineData("|Building Type = something special", BuildingType.Unknown)]
        [MemberData(nameof(BuildingTypeTestData))]
        public void GetInfobox_WikiTextContainsBuildingType_ShouldReturnCorrectValue(string input, BuildingType expectedType)
        {
            // Arrange
            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(expectedType, result[0].Type);
        }

        [Theory]
        [InlineData("|Building Type     = Production", BuildingType.Production)]
        [InlineData("|Building Type =   Public Service", BuildingType.PublicService)]
        public void GetInfobox_WikiTextContainsBuildingTypeAndWhiteSpace_ShouldReturnCorrectValue(string input, BuildingType expectedType)
        {
            // Arrange
            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(expectedType, result[0].Type);
        }

        [Theory]
        [InlineData("|Building Type = Production}}", BuildingType.Production)]
        [InlineData("|Building Type = Public Service   }}", BuildingType.PublicService)]
        public void GetInfobox_WikiTextContainsBuildingTypeAndTemplateEnd_ShouldReturnCorrectValue(string input, BuildingType expectedType)
        {
            // Arrange
            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(expectedType, result[0].Type);
        }

        [Theory]
        [InlineData("|Building Type = prodUcTion", BuildingType.Production)]
        [InlineData("|Building Type = Public service", BuildingType.PublicService)]
        public void GetInfobox_WikiTextContainsBuildingTypeDifferentCasing_ShouldReturnCorrectValue(string input, BuildingType expectedType)
        {
            // Arrange
            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(expectedType, result[0].Type);
        }

        #endregion

        #region BuildingName tests

        [Fact]
        public void GetInfobox_WikiTextContainsNoTitle_ShouldReturnEmpty()
        {
            // Arrange
            var parser = GetParser();

            // Act
            var result = parser.GetInfobox("dummy");

            // Assert
            Assert.Equal(string.Empty, result[0].Name);
        }

        [Theory]
        [InlineData("|Title = Marketplace", "Marketplace")]
        [InlineData("|Title = Chapel", "Chapel")]
        [InlineData("|Title = Cannery", "Cannery")]
        [InlineData("|Title = Police Station", "Police Station")]
        [InlineData("|Title = Straight Path", "Straight Path")]
        [InlineData("|Title = Small Palm Tree", "Small Palm Tree")]
        [InlineData("|Title = Coffee Plantation", "Coffee Plantation")]
        [InlineData("|Title = Bomb­ín Weaver", "Bomb­ín Weaver")]
        public void GetInfobox_WikiTextContainsTitle_ShouldReturnCorrectValue(string input, string expectedName)
        {
            // Arrange
            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(expectedName, result[0].Name);
        }

        [Theory]
        [InlineData("|Title     = Marketplace", "Marketplace")]
        [InlineData("|Title =    Chapel", "Chapel")]
        [InlineData("|Title     =   Cannery", "Cannery")]
        [InlineData("|Title  =  Police Station  ", "Police Station")]
        [InlineData("|Title =   Straight Path   ", "Straight Path")]
        [InlineData("|Title     = Small Palm Tree   ", "Small Palm Tree")]
        [InlineData("|Title     =   Coffee Plantation   ", "Coffee Plantation")]
        [InlineData("|Title = Bomb­ín Weaver", "Bomb­ín Weaver")]
        public void GetInfobox_WikiTextContainsTitleAndWhiteSpace_ShouldReturnCorrectValue(string input, string expectedName)
        {
            // Arrange
            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(expectedName, result[0].Name);
        }

        [Theory]
        [InlineData("|Title  =   Cannery   }}  ", "Cannery")]
        [InlineData("|Title = Police Station}}  ", "Police Station")]
        [InlineData("|Title = Straight Path }}", "Straight Path")]
        [InlineData("|Title = Small Palm Tree}} ", "Small Palm Tree")]
        [InlineData("|Title = Coffee Plantation}}", "Coffee Plantation")]
        [InlineData("|Title = Bomb­ín Weaver    }}", "Bomb­ín Weaver")]
        public void GetInfobox_WikiTextContainsTitleAndTemplateEnd_ShouldReturnCorrectValue(string input, string expectedName)
        {
            // Arrange
            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(expectedName, result[0].Name);
        }

        [Theory]
        [InlineData("|Title = Bombin Weaver", "Bomb­ín Weaver")]
        public void GetInfobox_WikiTextContainsSpecialTitle_ShouldReturnCorrectValue(string input, string expectedName)
        {
            // Arrange
            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(expectedName, result[0].Name);
        }

        #endregion

        #region ProductionInfo tests

        [Fact]
        public void GetInfobox_WikiTextContainsNoProductionInfo_ShouldReturnNull()
        {
            // Arrange
            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(testDataEmpty);

            // Assert
            Assert.Single(result);
            Assert.Null(result[0].ProductionInfos);
        }

        [Fact]
        public void GetInfobox_ProducesAmountElectricityIsParseable_ShouldSetAmountElectricity()
        {
            // Arrange
            var input = "|Produces Amount Electricity = 42";

            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(42, result[0].ProductionInfos.EndProduct.AmountElectricity);
        }

        [Fact]
        public void GetInfobox_WikiTextContainsProductionAmountElectricityAndWhiteSpace_ShouldReturnCorrectValue()
        {
            // Arrange
            var input = "|Produces Amount Electricity     =    42    ";

            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(42, result[0].ProductionInfos.EndProduct.AmountElectricity);
        }

        [Fact]
        [UseCulture("en-US")]
        public void GetInfobox_ProducesAmountElectricityIsDouble_ShouldSetAmountElectricity()
        {
            // Arrange
            var input = "|Produces Amount Electricity = 42,21";

            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(42.21, result[0].ProductionInfos.EndProduct.AmountElectricity);
        }

        [Fact]
        public void GetInfobox_ProducesAmountElectricityIsNotParseable_ShouldNotSetAmountElectricity()
        {
            // Arrange
            var input = "|Produces Amount Electricity = no_number" + Environment.NewLine + "|Produces Amount = 1";

            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(0, result[0].ProductionInfos.EndProduct.AmountElectricity);
        }

        [Fact]
        public void GetInfobox_ProducesAmountIsParseable_ShouldSetAmount()
        {
            // Arrange
            var input = "|Produces Amount = 42";

            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(42, result[0].ProductionInfos.EndProduct.Amount);
        }

        [Fact]
        public void GetInfobox_WikiTextContainsProductionAmountAndWhiteSpace_ShouldReturnCorrectValue()
        {
            // Arrange
            var input = "|Produces Amount      =    42    ";

            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(42, result[0].ProductionInfos.EndProduct.Amount);
        }

        [Fact]
        [UseCulture("en-US")]
        public void GetInfobox_ProducesAmountIsDouble_ShouldSetAmount()
        {
            // Arrange
            var input = "|Produces Amount = 42,21";

            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(42.21, result[0].ProductionInfos.EndProduct.Amount);
        }

        [Fact]
        public void GetInfobox_ProducesAmountIsNotParseable_ShouldNotSetAmount()
        {
            // Arrange
            var input = "|Produces Amount = no_number" + Environment.NewLine + "|Produces Amount Electricity = 1";

            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(0, result[0].ProductionInfos.EndProduct.Amount);
        }

        [Fact]
        public void GetInfobox_ProducesIconIsPresent_ShouldSetIcon()
        {
            // Arrange
            var input = $"|Produces Amount {Environment.NewLine}|Produces Icon = dummy.png";

            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal("dummy.png", result[0].ProductionInfos.EndProduct.Icon);
        }

        [Fact]
        public void GetInfobox_ProducesIconIsPresentAndWhiteSpace_ShouldSetIcon()
        {
            // Arrange
            var input = $"|Produces Icon    =     dummy.png  {Environment.NewLine}|Produces Amount";

            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal("dummy.png", result[0].ProductionInfos.EndProduct.Icon);
        }

        [Fact]
        public void GetInfobox_InputCounterIsNotParseable_ShouldThrow()
        {
            // Arrange
            var input = $"|Produces Amount = 1{Environment.NewLine}|Input {int.MaxValue + 1L} Amount = 42";

            var parser = GetParser();

            // Act/Assert
            var ex = Assert.Throws<Exception>(() => parser.GetInfobox(input));
        }

        [Fact]
        public void GetInfobox_InputElectricityCounterIsNotParseable_ShouldThrow()
        {
            // Arrange
            var input = $"|Produces Amount = 1{Environment.NewLine}|Input {int.MaxValue + 1L} Amount Electricity = 42";

            var parser = GetParser();

            // Act/Assert
            var ex = Assert.Throws<Exception>(() => parser.GetInfobox(input));
        }

        [Fact]
        public void GetInfobox_InputIconCounterIsNotParseable_ShouldThrow()
        {
            // Arrange
            var input = $"|Produces Amount = 1{Environment.NewLine}|Input {int.MaxValue + 1L} Icon = dummy.png";

            var parser = GetParser();

            // Act/Assert
            var ex = Assert.Throws<Exception>(() => parser.GetInfobox(input));
        }

        [Fact]
        public void GetInfobox_WikiTextContainsInputAmount_ShouldReturnCorrectValue()
        {
            // Arrange
            var input = $"|Produces Amount{Environment.NewLine}|Input 1 Amount = 42";

            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Single(result);
            Assert.Single(result[0].ProductionInfos.InputProducts);
            Assert.Equal(42, result[0].ProductionInfos.InputProducts[0].Amount);
        }

        [Fact]
        public void GetInfobox_WikiTextContainsInputAmountAndWhiteSpace_ShouldReturnCorrectValue()
        {
            // Arrange
            var input = $"|Produces Amount{Environment.NewLine}|Input 1 Amount  =    42  ";

            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Single(result);
            Assert.Single(result[0].ProductionInfos.InputProducts);
            Assert.Equal(42, result[0].ProductionInfos.InputProducts[0].Amount);
        }

        [Fact]
        public void GetInfobox_WikiTextContainsInputAmountElectricity_ShouldReturnCorrectValue()
        {
            // Arrange
            var input = $"|Produces Amount{Environment.NewLine}|Input 1 Amount Electricity = 42";

            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Single(result);
            Assert.Single(result[0].ProductionInfos.InputProducts);
            Assert.Equal(42, result[0].ProductionInfos.InputProducts[0].AmountElectricity);
        }

        [Fact]
        public void GetInfobox_WikiTextContainsInputAmountElectricityAndWhiteSpace_ShouldReturnCorrectValue()
        {
            // Arrange
            var input = $"|Produces Amount{Environment.NewLine}|Input 1 Amount Electricity   =    42    ";

            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Single(result);
            Assert.Single(result[0].ProductionInfos.InputProducts);
            Assert.Equal(42, result[0].ProductionInfos.InputProducts[0].AmountElectricity);
        }

        [Fact]
        public void GetInfobox_WikiTextContainsInputIcon_ShouldReturnCorrectValue()
        {
            // Arrange
            var input = $"|Produces Amount{Environment.NewLine}|Input 1 Icon = dummy.png";

            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Single(result);
            Assert.Single(result[0].ProductionInfos.InputProducts);
            Assert.Equal("dummy.png", result[0].ProductionInfos.InputProducts[0].Icon);
        }

        [Fact]
        public void GetInfobox_WikiTextContainsInputIconAndWhiteSpace_ShouldReturnCorrectValue()
        {
            // Arrange
            var input = $"|Produces Amount{Environment.NewLine}|Input 1 Icon    =  dummy.png    ";

            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Single(result);
            Assert.Single(result[0].ProductionInfos.InputProducts);
            Assert.Equal("dummy.png", result[0].ProductionInfos.InputProducts[0].Icon);
        }

        [Fact]
        public void GetInfobox_WikiTextContainsMultipleInputInfos_ShouldReturnCorrectOrderedValue()
        {
            // Arrange
            var input = $"|Produces Amount{Environment.NewLine}|Input 2 Amount = 21{Environment.NewLine}|Input 1 Amount = 42";

            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Single(result);
            Assert.Equal(2, result[0].ProductionInfos.InputProducts.Count);
            Assert.Equal(42, result[0].ProductionInfos.InputProducts[0].Amount);
            Assert.Equal(21, result[0].ProductionInfos.InputProducts[1].Amount);
        }

        #endregion

        #region SupplyInfo tests

        [Fact]
        public void GetInfobox_WikiTextContainsNoSupplyInfo_ShouldReturnCorrectValue()
        {
            // Arrange
            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(testDataEmpty);

            // Assert
            Assert.Single(result);
            Assert.Null(result[0].SupplyInfos);
        }

        [Fact]
        public void GetInfobox_WikiTextContainsSupplyAmount_ShouldReturnCorrectValue()
        {
            // Arrange
            var input = "|Supplies 1 Amount = 42";

            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Single(result);
            Assert.Single(result[0].SupplyInfos.SupplyEntries);
            Assert.Equal(42, result[0].SupplyInfos.SupplyEntries[0].Amount);
        }

        [Fact]
        public void GetInfobox_WikiTextContainsSupplyAmountAndWhiteSpace_ShouldReturnCorrectValue()
        {
            // Arrange
            var input = "|Supplies 1 Amount   =     42  ";

            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Single(result);
            Assert.Single(result[0].SupplyInfos.SupplyEntries);
            Assert.Equal(42, result[0].SupplyInfos.SupplyEntries[0].Amount);
        }

        [Fact]
        public void GetInfobox_WikiTextContainsSupplyAmountElectricity_ShouldReturnCorrectValue()
        {
            // Arrange
            var input = "|Supplies 1 Amount Electricity = 42";

            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Single(result);
            Assert.Single(result[0].SupplyInfos.SupplyEntries);
            Assert.Equal(42, result[0].SupplyInfos.SupplyEntries[0].AmountElectricity);
        }

        [Fact]
        public void GetInfobox_WikiTextContainsSupplyAmountElectricityAndWhiteSpace_ShouldReturnCorrectValue()
        {
            // Arrange
            var input = "|Supplies 1 Amount Electricity  =   42     ";

            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Single(result);
            Assert.Single(result[0].SupplyInfos.SupplyEntries);
            Assert.Equal(42, result[0].SupplyInfos.SupplyEntries[0].AmountElectricity);
        }

        [Fact]
        public void GetInfobox_WikiTextContainsSupplyType_ShouldReturnCorrectValue()
        {
            // Arrange
            var input = "|Supplies 1 Type = Workers";

            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Single(result);
            Assert.Single(result[0].SupplyInfos.SupplyEntries);
            Assert.Equal("Workers", result[0].SupplyInfos.SupplyEntries[0].Type);
        }

        [Fact]
        public void GetInfobox_WikiTextContainsSupplyTypeAndWhiteSpace_ShouldReturnCorrectValue()
        {
            // Arrange
            var input = "|Supplies 1 Type    =        Workers       ";

            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Single(result);
            Assert.Single(result[0].SupplyInfos.SupplyEntries);
            Assert.Equal("Workers", result[0].SupplyInfos.SupplyEntries[0].Type);
        }

        [Fact]
        public void GetInfobox_WikiTextContainsMultipleSupplyInfos_ShouldReturnCorrectOrderedValue()
        {
            // Arrange
            var input = $"|Supplies 2 Amount = 21{Environment.NewLine}|Supplies 1 Amount = 42";

            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Single(result);
            Assert.Equal(2, result[0].SupplyInfos.SupplyEntries.Count);
            Assert.Equal(42, result[0].SupplyInfos.SupplyEntries[0].Amount);
            Assert.Equal(21, result[0].SupplyInfos.SupplyEntries[1].Amount);
        }

        [Fact]
        public void GetInfobox_SupplyAmountCounterIsNotParseable_ShouldThrow()
        {
            // Arrange
            var input = $"|Supplies {int.MaxValue + 1L} Amount = 42";

            var parser = GetParser();

            // Act/Assert
            var ex = Assert.Throws<Exception>(() => parser.GetInfobox(input));
        }

        [Fact(Skip = "regex needs adjustment")]
        public void GetInfobox_SupplyAmountIsNotParseable_ShouldThrow()
        {
            // Arrange
            var input = $"|Supplies 1 Amount = {double.MaxValue.ToString()}";

            var parser = GetParser();

            // Act/Assert
            var ex = Assert.Throws<Exception>(() => parser.GetInfobox(input));
        }

        [Fact]
        public void GetInfobox_SupplyAmountElectricityCounterIsNotParseable_ShouldThrow()
        {
            // Arrange
            var input = $"|Supplies {int.MaxValue + 1L} Amount Electricity = 42";

            var parser = GetParser();

            // Act/Assert
            var ex = Assert.Throws<Exception>(() => parser.GetInfobox(input));
        }

        [Fact(Skip = "regex needs adjustment")]
        public void GetInfobox_SupplyAmountElectricityIsNotParseable_ShouldThrow()
        {
            // Arrange
            var input = $"|Supplies 1 Amount Electricity = {double.MaxValue.ToString()}";

            var parser = GetParser();

            // Act/Assert
            var ex = Assert.Throws<Exception>(() => parser.GetInfobox(input));
        }

        [Fact]
        public void GetInfobox_SupplyTypeCounterIsNotParseable_ShouldThrow()
        {
            // Arrange
            var input = $"|Supplies {int.MaxValue + 1L} Type = Farmer";

            var parser = GetParser();

            // Act/Assert
            var ex = Assert.Throws<Exception>(() => parser.GetInfobox(input));
        }

        #endregion

        #region UnlockInfo tests

        [Fact]
        public void GetInfobox_WikiTextContainsNoUnlockInfo_ShouldReturnCorrectValue()
        {
            // Arrange
            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(testDataEmpty);

            // Assert
            Assert.Single(result);
            Assert.Null(result[0].UnlockInfos);
        }

        [Fact]
        public void GetInfobox_WikiTextContainsUnlockAmount_ShouldReturnCorrectValue()
        {
            // Arrange
            var input = "|Unlock Condition 1 Amount = 42";

            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Single(result);
            Assert.Single(result[0].UnlockInfos.UnlockConditions);
            Assert.Equal(42, result[0].UnlockInfos.UnlockConditions[0].Amount);
        }

        [Fact]
        public void GetInfobox_WikiTextContainsUnlockAmountAndWhiteSpace_ShouldReturnCorrectValue()
        {
            // Arrange
            var input = "|Unlock Condition 1 Amount          =    42     ";

            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Single(result);
            Assert.Single(result[0].UnlockInfos.UnlockConditions);
            Assert.Equal(42, result[0].UnlockInfos.UnlockConditions[0].Amount);
        }

        [Fact]
        public void GetInfobox_WikiTextContainsUnlockType_ShouldReturnCorrectValue()
        {
            // Arrange
            var input = "|Unlock Condition 1 Type = Workers";

            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Single(result);
            Assert.Single(result[0].UnlockInfos.UnlockConditions);
            Assert.Equal("Workers", result[0].UnlockInfos.UnlockConditions[0].Type);
        }

        [Fact]
        public void GetInfobox_WikiTextContainsUnlockTypeAndWhiteSpace_ShouldReturnCorrectValue()
        {
            // Arrange
            var input = "|Unlock Condition 1 Type   =       Workers   ";

            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Single(result);
            Assert.Single(result[0].UnlockInfos.UnlockConditions);
            Assert.Equal("Workers", result[0].UnlockInfos.UnlockConditions[0].Type);
        }

        [Fact]
        public void GetInfobox_WikiTextContainsMultipleUnlockInfos_ShouldReturnCorrectOrderedValue()
        {
            // Arrange
            var input = $"|Unlock Condition 2 Amount = 21{Environment.NewLine}|Unlock Condition 1 Amount = 42";

            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Single(result);
            Assert.Equal(2, result[0].UnlockInfos.UnlockConditions.Count);
            Assert.Equal(42, result[0].UnlockInfos.UnlockConditions[0].Amount);
            Assert.Equal(21, result[0].UnlockInfos.UnlockConditions[1].Amount);
        }

        [Fact]
        public void GetInfobox_UnlockAmountCounterIsNotParseable_ShouldThrow()
        {
            // Arrange
            var input = $"|Unlock Condition {int.MaxValue + 1L} Amount = 42";

            var parser = GetParser();

            // Act/Assert
            var ex = Assert.Throws<Exception>(() => parser.GetInfobox(input));
        }

        [Fact(Skip = "regex needs adjustment")]
        public void GetInfobox_UnlockAmountIsNotParseable_ShouldThrow()
        {
            // Arrange
            var input = $"|Unlock Condition 1 Amount = {double.MaxValue.ToString()}";

            var parser = GetParser();

            // Act/Assert
            var ex = Assert.Throws<Exception>(() => parser.GetInfobox(input));
        }

        [Fact]
        public void GetInfobox_UnlockTypeCounterIsNotParseable_ShouldThrow()
        {
            // Arrange
            var input = $"|Unlock Condition {int.MaxValue + 1L} Type = Farmers";

            var parser = GetParser();

            // Act/Assert
            var ex = Assert.Throws<Exception>(() => parser.GetInfobox(input));
        }

        #endregion

        #region specific buildings tests

        [Fact]
        [UseCulture("en-US")]
        public void GetInfobox_WikiTextIsCannery_ShouldReturnCorrectResult()
        {
            // Arrange
            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(testDataCannery);

            // Assert
            Assert.Equal("Cannery", result[0].Name);
            Assert.Equal(BuildingType.Production, result[0].Type);
            Assert.Equal(0.667, result[0].ProductionInfos.EndProduct.Amount);
            Assert.Equal(1.333, result[0].ProductionInfos.EndProduct.AmountElectricity);
            Assert.Equal("Canned_food.png", result[0].ProductionInfos.EndProduct.Icon);
            Assert.Equal(2, result[0].ProductionInfos.InputProducts.Count);
            Assert.Equal(0.667, result[0].ProductionInfos.InputProducts[0].Amount);
            Assert.Equal(1.333, result[0].ProductionInfos.InputProducts[0].AmountElectricity);
            Assert.Equal("Goulash.png", result[0].ProductionInfos.InputProducts[0].Icon);
            Assert.Equal(0.667, result[0].ProductionInfos.InputProducts[1].Amount);
            Assert.Equal(1.333, result[0].ProductionInfos.InputProducts[1].AmountElectricity);
            Assert.Equal("Iron.png", result[0].ProductionInfos.InputProducts[1].Icon);

            Assert.Equal(2, result[0].SupplyInfos.SupplyEntries.Count);
            Assert.Equal(65, result[0].SupplyInfos.SupplyEntries[0].Amount);
            Assert.Equal(130, result[0].SupplyInfos.SupplyEntries[0].AmountElectricity);
            Assert.Equal("Artisans", result[0].SupplyInfos.SupplyEntries[0].Type);
            Assert.Equal(32.5, result[0].SupplyInfos.SupplyEntries[1].Amount);
            Assert.Equal(65, result[0].SupplyInfos.SupplyEntries[1].AmountElectricity);
            Assert.Equal("Engineers", result[0].SupplyInfos.SupplyEntries[1].Type);

            Assert.Single(result[0].UnlockInfos.UnlockConditions);
            Assert.Equal(1, result[0].UnlockInfos.UnlockConditions[0].Amount);
            Assert.Equal("Artisans", result[0].UnlockInfos.UnlockConditions[0].Type);
        }

        [Fact]
        [UseCulture("en-US")]
        public void GetInfobox_WikiTextIsBakery_ShouldReturnCorrectResult()
        {
            // Arrange
            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(testDataBakery);

            // Assert
            Assert.Equal("Bakery", result[0].Name);
            Assert.Equal(BuildingType.Production, result[0].Type);
            Assert.Equal(1, result[0].ProductionInfos.EndProduct.Amount);
            Assert.Equal(2, result[0].ProductionInfos.EndProduct.AmountElectricity);
            Assert.Equal("Bread.png", result[0].ProductionInfos.EndProduct.Icon);
            Assert.Single(result[0].ProductionInfos.InputProducts);
            Assert.Equal(1, result[0].ProductionInfos.InputProducts[0].Amount);
            Assert.Equal(2, result[0].ProductionInfos.InputProducts[0].AmountElectricity);
            Assert.Equal("Flour.png", result[0].ProductionInfos.InputProducts[0].Icon);

            Assert.Equal(2, result[0].SupplyInfos.SupplyEntries.Count);
            Assert.Equal(55, result[0].SupplyInfos.SupplyEntries[0].Amount);
            Assert.Equal(110, result[0].SupplyInfos.SupplyEntries[0].AmountElectricity);
            Assert.Equal("Workers", result[0].SupplyInfos.SupplyEntries[0].Type);
            Assert.Equal(27.5, result[0].SupplyInfos.SupplyEntries[1].Amount);
            Assert.Equal(55, result[0].SupplyInfos.SupplyEntries[1].AmountElectricity);
            Assert.Equal("Artisans", result[0].SupplyInfos.SupplyEntries[1].Type);

            Assert.Single(result[0].UnlockInfos.UnlockConditions);
            Assert.Equal(150, result[0].UnlockInfos.UnlockConditions[0].Amount);
            Assert.Equal("Workers", result[0].UnlockInfos.UnlockConditions[0].Type);
        }

        #endregion
    }
}
