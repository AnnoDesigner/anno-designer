using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FandomParser.Core;
using FandomParser.Core.Presets.Models;
using InfoboxParser.Tests.Attributes;
using Xunit;

namespace InfoboxParser.Tests
{
    public class ParserBothWorldsTests
    {
        private static readonly ICommons mockedCommons;
        private static readonly string testDataPoliceStation;
        private static readonly string testDataBrickFactory;
        private static readonly string testDataHospital;
        private static readonly string testDataMarketplace;
        private static readonly string testDataSmallWareHouse;
        private static readonly string testDataEmpty_BothWorlds;

        static ParserBothWorldsTests()
        {
            mockedCommons = Commons.Instance;

            string basePath = AppDomain.CurrentDomain.BaseDirectory;

            testDataPoliceStation = File.ReadAllText(Path.Combine(basePath, "Testdata", "Police_Station.infobox"));
            testDataBrickFactory = File.ReadAllText(Path.Combine(basePath, "Testdata", "Brick_Factory.infobox"));
            testDataHospital = File.ReadAllText(Path.Combine(basePath, "Testdata", "Hospital.infobox"));
            testDataMarketplace = File.ReadAllText(Path.Combine(basePath, "Testdata", "Marketplace.infobox"));
            testDataSmallWareHouse = File.ReadAllText(Path.Combine(basePath, "Testdata", "Small_Warehouse.infobox"));
            testDataEmpty_BothWorlds = File.ReadAllText(Path.Combine(basePath, "Testdata", "empty_BothWorlds.infobox"));
        }

        #region test data

        public static TheoryData<string, BuildingType> BuildingTypeTestData
        {
            get
            {
                return new TheoryData<string, BuildingType>
                {
                    { testDataPoliceStation, BuildingType.Institution },
                    { testDataBrickFactory, BuildingType.Production },
                    { testDataHospital, BuildingType.Institution },
                    { testDataMarketplace, BuildingType.PublicService },
                    { testDataSmallWareHouse, BuildingType.Infrastructure }
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
            var parser = new ParserBothWorlds(mockedCommons);

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
            var parser = new ParserBothWorlds(mockedCommons);

            // Act
            var result = parser.GetInfobox("dummy");

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(BuildingType.Unknown, result[0].Type);
            Assert.Equal(BuildingType.Unknown, result[1].Type);
        }

        [Theory]
        [InlineData("|Building Type (OW) = something special")]
        [InlineData("|Building Type (NW) = something special")]
        public void GetInfobox_WikiTextContainsUnknownBuildingType_ShouldReturnUnknown(string input)
        {
            // Arrange
            var parser = new ParserBothWorlds(mockedCommons);

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(BuildingType.Unknown, result[0].Type);
            Assert.Equal(BuildingType.Unknown, result[1].Type);
        }

        [Theory]
        [MemberData(nameof(BuildingTypeTestData))]
        public void GetInfobox_WikiTextContainsBuildingTypeSpecificBuildings_ShouldReturnCorrectValue(string input, BuildingType expectedType)
        {
            // Arrange
            var parser = new ParserBothWorlds(mockedCommons);

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(expectedType, result[0].Type);
            Assert.Equal(expectedType, result[1].Type);
        }

        [Theory]
        [InlineData("|Building Type (OW) = Production", BuildingType.Production)]
        [InlineData("|Building Type (OW) = Public Service", BuildingType.PublicService)]
        [InlineData("|Building Type (OW) = Residence", BuildingType.Residence)]
        [InlineData("|Building Type (OW) = Infrastructure", BuildingType.Infrastructure)]
        [InlineData("|Building Type (OW) = Institution", BuildingType.Institution)]
        [InlineData("|Building Type (OW) = Ornament", BuildingType.Ornament)]
        [InlineData("|Building Type (OW) = Monument", BuildingType.Monument)]
        [InlineData("|Building Type (OW) = Administration", BuildingType.Administration)]
        [InlineData("|Building Type (OW) = Harbour", BuildingType.Harbour)]
        [InlineData("|Building Type (OW) = Street", BuildingType.Street)]
        [InlineData("|Building Type (OW) = something special", BuildingType.Unknown)]
        public void GetInfobox_WikiTextContainsBuildingTypeOldWorld_ShouldReturnCorrectValue(string input, BuildingType expectedType)
        {
            // Arrange
            var parser = new ParserBothWorlds(mockedCommons);

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(BuildingType.Unknown, result.Single(x => x.Region == WorldRegion.NewWorld).Type);
            Assert.Equal(expectedType, result.Single(x => x.Region == WorldRegion.OldWorld).Type);
        }

        [Theory]
        [InlineData("|Building Type (NW) = Production", BuildingType.Production)]
        [InlineData("|Building Type (NW) = Public Service", BuildingType.PublicService)]
        [InlineData("|Building Type (NW) = Residence", BuildingType.Residence)]
        [InlineData("|Building Type (NW) = Infrastructure", BuildingType.Infrastructure)]
        [InlineData("|Building Type (NW) = Institution", BuildingType.Institution)]
        [InlineData("|Building Type (NW) = Ornament", BuildingType.Ornament)]
        [InlineData("|Building Type (NW) = Monument", BuildingType.Monument)]
        [InlineData("|Building Type (NW) = Administration", BuildingType.Administration)]
        [InlineData("|Building Type (NW) = Harbour", BuildingType.Harbour)]
        [InlineData("|Building Type (NW) = Street", BuildingType.Street)]
        [InlineData("|Building Type (NW) = something special", BuildingType.Unknown)]
        public void GetInfobox_WikiTextContainsBuildingTypeNewWorld_ShouldReturnCorrectValue(string input, BuildingType expectedType)
        {
            // Arrange
            var parser = new ParserBothWorlds(mockedCommons);

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(BuildingType.Unknown, result.Single(x => x.Region == WorldRegion.OldWorld).Type);
            Assert.Equal(expectedType, result.Single(x => x.Region == WorldRegion.NewWorld).Type);
        }

        [Theory]
        [InlineData("|Building Type (OW)     =   Ornament\r\n|Building Type (NW)     =   Ornament", BuildingType.Ornament)]
        [InlineData("|Building Type (NW) =    Harbour\r\n|Building Type (OW) =  Harbour  ", BuildingType.Harbour)]
        public void GetInfobox_WikiTextContainsBuildingTypeAndWhiteSpace_ShouldReturnCorrectValue(string input, BuildingType expectedType)
        {
            // Arrange
            var parser = new ParserBothWorlds(mockedCommons);

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(expectedType, result[0].Type);
            Assert.Equal(expectedType, result[1].Type);
        }

        [Theory]
        [InlineData("|Building Type (NW)     =   Ornament\r\n|Building Type (OW)     =   Ornament   }}", BuildingType.Ornament)]
        [InlineData("|Building Type (OW) = Harbour\r\n|Building Type (NW) = Harbour}}", BuildingType.Harbour)]
        public void GetInfobox_WikiTextContainsBuildingTypeAndTemplateEnd_ShouldReturnCorrectValue(string input, BuildingType expectedType)
        {
            // Arrange
            var parser = new ParserBothWorlds(mockedCommons);

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(expectedType, result[0].Type);
            Assert.Equal(expectedType, result[1].Type);
        }

        [Theory]
        [InlineData("|Building Type (OW) = ornaMent\r\n|Building Type (NW) = ornaMent", BuildingType.Ornament)]
        [InlineData("|Building Type (NW) = HarBouR\r\n|Building Type (OW) = harbouR", BuildingType.Harbour)]
        public void GetInfobox_WikiTextContainsBuildingTypeDifferentCasing_ShouldReturnCorrectValue(string input, BuildingType expectedType)
        {
            // Arrange
            var parser = new ParserBothWorlds(mockedCommons);

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(expectedType, result[0].Type);
            Assert.Equal(expectedType, result[1].Type);
        }

        #endregion

        #region BuildingName tests

        [Fact]
        public void GetInfobox_WikiTextContainsNoTitle_ShouldReturnEmpty()
        {
            // Arrange
            var parser = new ParserBothWorlds(mockedCommons);

            // Act
            var result = parser.GetInfobox("dummy");

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(string.Empty, result[0].Name);
            Assert.Equal(string.Empty, result[1].Name);
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
            var parser = new ParserBothWorlds(mockedCommons);

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(expectedName, result[0].Name);
            Assert.Equal(expectedName, result[1].Name);
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
            var parser = new ParserBothWorlds(mockedCommons);

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(expectedName, result[0].Name);
            Assert.Equal(expectedName, result[1].Name);
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
            var parser = new ParserBothWorlds(mockedCommons);

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(expectedName, result[0].Name);
            Assert.Equal(expectedName, result[1].Name);
        }

        [Theory]
        [InlineData("|Title = Bombin Weaver", "Bomb­ín Weaver")]
        public void GetInfobox_WikiTextContainsSpecialTitle_ShouldReturnCorrectValue(string input, string expectedName)
        {
            // Arrange
            var parser = new ParserBothWorlds(mockedCommons);

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(expectedName, result[0].Name);
            Assert.Equal(expectedName, result[1].Name);
        }

        #endregion

        #region ProductionInfo tests

        [Fact]
        public void GetInfobox_WikiTextContainsNoProductionInfo_ShouldReturnNull()
        {
            // Arrange
            var parser = new ParserBothWorlds(mockedCommons);

            // Act
            var result = parser.GetInfobox(testDataEmpty_BothWorlds);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Null(result[0].ProductionInfos);
            Assert.Null(result[1].ProductionInfos);
        }

        [Fact]
        public void GetInfobox_ProducesAmountElectricityIsParseable_ShouldSetAmountElectricity()
        {
            // Arrange
            var input = "|Produces Amount Electricity (OW) = 42";

            var parser = new ParserBothWorlds(mockedCommons);

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(42, result.Single(x => x.Region == WorldRegion.OldWorld).ProductionInfos.EndProduct.AmountElectricity);
            Assert.Null(result.Single(x => x.Region == WorldRegion.NewWorld).ProductionInfos);
        }

        [Fact]
        public void GetInfobox_WikiTextContainsProductionAmountElectricityAndWhiteSpace_ShouldReturnCorrectValue()
        {
            // Arrange
            var input = "|Produces Amount Electricity  (OW)   =    42    ";

            var parser = new ParserBothWorlds(mockedCommons);

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(42, result.Single(x => x.Region == WorldRegion.OldWorld).ProductionInfos.EndProduct.AmountElectricity);
            Assert.Null(result.Single(x => x.Region == WorldRegion.NewWorld).ProductionInfos);
        }

        [Fact]
        [UseCulture("en-US")]
        public void GetInfobox_ProducesAmountElectricityIsDouble_ShouldSetAmountElectricity()
        {
            // Arrange
            var input = "|Produces Amount Electricity (OW) = 42,21";

            var parser = new ParserBothWorlds(mockedCommons);

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(42.21, result.Single(x => x.Region == WorldRegion.OldWorld).ProductionInfos.EndProduct.AmountElectricity);
            Assert.Null(result.Single(x => x.Region == WorldRegion.NewWorld).ProductionInfos);
        }

        [Fact]
        public void GetInfobox_ProducesAmountElectricityIsNotParseable_ShouldNotSetAmountElectricity()
        {
            // Arrange
            var input = "|Produces Amount Electricity (OW) = no_number" + Environment.NewLine + "|Produces Amount (OW) = 1";

            var parser = new ParserBothWorlds(mockedCommons);

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(0, result.Single(x => x.Region == WorldRegion.OldWorld).ProductionInfos.EndProduct.AmountElectricity);
            Assert.Equal(1, result.Single(x => x.Region == WorldRegion.OldWorld).ProductionInfos.EndProduct.Amount);
            Assert.Null(result.Single(x => x.Region == WorldRegion.NewWorld).ProductionInfos);
        }

        [Fact]
        public void GetInfobox_ProducesAmountIsParseable_ShouldSetAmount()
        {
            // Arrange
            var input = "|Produces Amount (OW) = 42";

            var parser = new ParserBothWorlds(mockedCommons);

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(42, result.Single(x => x.Region == WorldRegion.OldWorld).ProductionInfos.EndProduct.Amount);
            Assert.Null(result.Single(x => x.Region == WorldRegion.NewWorld).ProductionInfos);
        }

        [Fact]
        public void GetInfobox_WikiTextContainsProductionAmountAndWhiteSpace_ShouldReturnCorrectValue()
        {
            // Arrange
            var input = "|Produces Amount   (OW)   =    42    ";

            var parser = new ParserBothWorlds(mockedCommons);

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(42, result.Single(x => x.Region == WorldRegion.OldWorld).ProductionInfos.EndProduct.Amount);
            Assert.Null(result.Single(x => x.Region == WorldRegion.NewWorld).ProductionInfos);
        }

        [Fact]
        [UseCulture("en-US")]
        public void GetInfobox_ProducesAmountIsDouble_ShouldSetAmount()
        {
            // Arrange
            var input = "|Produces Amount (OW) = 42,21";

            var parser = new ParserBothWorlds(mockedCommons);

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(42.21, result.Single(x => x.Region == WorldRegion.OldWorld).ProductionInfos.EndProduct.Amount);
            Assert.Null(result.Single(x => x.Region == WorldRegion.NewWorld).ProductionInfos);
        }

        [Fact]
        public void GetInfobox_ProducesAmountIsNotParseable_ShouldNotSetAmount()
        {
            // Arrange
            var input = "|Produces Amount (OW) = no_number" + Environment.NewLine + "|Produces Amount Electricity (OW) = 1";

            var parser = new ParserBothWorlds(mockedCommons);

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(0, result.Single(x => x.Region == WorldRegion.OldWorld).ProductionInfos.EndProduct.Amount);
            Assert.Equal(1, result.Single(x => x.Region == WorldRegion.OldWorld).ProductionInfos.EndProduct.AmountElectricity);
            Assert.Null(result.Single(x => x.Region == WorldRegion.NewWorld).ProductionInfos);
        }

        [Fact]
        public void GetInfobox_ProducesIconIsPresent_ShouldSetIcon()
        {
            // Arrange
            var input = $"|Produces Amount (OW) {Environment.NewLine}|Produces Icon (OW) = dummy.png";

            var parser = new ParserBothWorlds(mockedCommons);

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("dummy.png", result.Single(x => x.Region == WorldRegion.OldWorld).ProductionInfos.EndProduct.Icon);
            Assert.Null(result.Single(x => x.Region == WorldRegion.NewWorld).ProductionInfos);
        }

        [Fact]
        public void GetInfobox_ProducesIconIsPresentAndWhiteSpace_ShouldSetIcon()
        {
            // Arrange
            var input = $"|Produces Icon  (OW)  =     dummy.png  {Environment.NewLine}|Produces Amount (OW)";

            var parser = new ParserBothWorlds(mockedCommons);

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("dummy.png", result.Single(x => x.Region == WorldRegion.OldWorld).ProductionInfos.EndProduct.Icon);
            Assert.Null(result.Single(x => x.Region == WorldRegion.NewWorld).ProductionInfos);
        }

        [Fact]
        public void GetInfobox_InputCounterIsNotParseable_ShouldThrow()
        {
            // Arrange
            var input = $"|Produces Amount (OW) = 1{Environment.NewLine}|Input {int.MaxValue + 1L} Amount (OW) = 42";

            var parser = new ParserBothWorlds(mockedCommons);

            // Act/Assert
            var ex = Assert.Throws<Exception>(() => parser.GetInfobox(input));
        }

        [Fact]
        public void GetInfobox_InputElectricityCounterIsNotParseable_ShouldThrow()
        {
            // Arrange
            var input = $"|Produces Amount (OW) = 1{Environment.NewLine}|Input {int.MaxValue + 1L} Amount Electricity (OW) = 42";

            var parser = new ParserBothWorlds(mockedCommons);

            // Act/Assert
            var ex = Assert.Throws<Exception>(() => parser.GetInfobox(input));
        }

        [Fact]
        public void GetInfobox_InputIconCounterIsNotParseable_ShouldThrow()
        {
            // Arrange
            var input = $"|Produces Amount (OW) = 1{Environment.NewLine}|Input {int.MaxValue + 1L} Icon (OW) = dummy.png";

            var parser = new ParserBothWorlds(mockedCommons);

            // Act/Assert
            var ex = Assert.Throws<Exception>(() => parser.GetInfobox(input));
        }

        [Fact]
        public void GetInfobox_WikiTextContainsInputAmount_ShouldReturnCorrectValue()
        {
            // Arrange
            var input = $"|Produces Amount{Environment.NewLine}|Input 1 Amount (OW) = 42";

            var parser = new ParserBothWorlds(mockedCommons);

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Single(result.Single(x => x.Region == WorldRegion.OldWorld).ProductionInfos.InputProducts);
            Assert.Equal(42, result.Single(x => x.Region == WorldRegion.OldWorld).ProductionInfos.InputProducts[0].Amount);
            Assert.Null(result.Single(x => x.Region == WorldRegion.NewWorld).ProductionInfos);
        }

        [Fact]
        public void GetInfobox_WikiTextContainsInputAmountAndWhiteSpace_ShouldReturnCorrectValue()
        {
            // Arrange            
            var input = $"|Produces Amount{Environment.NewLine}|Input 1 Amount  (OW)   =     42    ";

            var parser = new ParserBothWorlds(mockedCommons);

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Single(result.Single(x => x.Region == WorldRegion.OldWorld).ProductionInfos.InputProducts);
            Assert.Equal(42, result.Single(x => x.Region == WorldRegion.OldWorld).ProductionInfos.InputProducts[0].Amount);
            Assert.Null(result.Single(x => x.Region == WorldRegion.NewWorld).ProductionInfos);
        }

        [Fact]
        public void GetInfobox_WikiTextContainsInputAmountElectricity_ShouldReturnCorrectValue()
        {
            // Arrange
            var input = $"|Produces Amount{Environment.NewLine}|Input 1 Amount Electricity (OW) = 42";

            var parser = new ParserBothWorlds(mockedCommons);

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Single(result.Single(x => x.Region == WorldRegion.OldWorld).ProductionInfos.InputProducts);
            Assert.Equal(42, result.Single(x => x.Region == WorldRegion.OldWorld).ProductionInfos.InputProducts[0].AmountElectricity);
            Assert.Null(result.Single(x => x.Region == WorldRegion.NewWorld).ProductionInfos);
        }

        [Fact]
        public void GetInfobox_WikiTextContainsInputAmountElectricityAndWhiteSpace_ShouldReturnCorrectValue()
        {
            // Arrange
            var input = $"|Produces Amount{Environment.NewLine}|Input 1 Amount Electricity   (OW)     =     42      ";

            var parser = new ParserBothWorlds(mockedCommons);

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Single(result.Single(x => x.Region == WorldRegion.OldWorld).ProductionInfos.InputProducts);
            Assert.Equal(42, result.Single(x => x.Region == WorldRegion.OldWorld).ProductionInfos.InputProducts[0].AmountElectricity);
            Assert.Null(result.Single(x => x.Region == WorldRegion.NewWorld).ProductionInfos);
        }

        [Fact]
        public void GetInfobox_WikiTextContainsInputIcon_ShouldReturnCorrectValue()
        {
            // Arrange
            var input = $"|Produces Amount{Environment.NewLine}|Input 1 Icon (OW) = dummy.png";

            var parser = new ParserBothWorlds(mockedCommons);

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Single(result.Single(x => x.Region == WorldRegion.OldWorld).ProductionInfos.InputProducts);
            Assert.Equal("dummy.png", result.Single(x => x.Region == WorldRegion.OldWorld).ProductionInfos.InputProducts[0].Icon);
            Assert.Null(result.Single(x => x.Region == WorldRegion.NewWorld).ProductionInfos);
        }

        [Fact]
        public void GetInfobox_WikiTextContainsInputIconAndWhiteSpace_ShouldReturnCorrectValue()
        {
            // Arrange
            var input = $"|Produces Amount{Environment.NewLine}|Input 1 Icon  (OW)   =     dummy.png        ";

            var parser = new ParserBothWorlds(mockedCommons);

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Single(result.Single(x => x.Region == WorldRegion.OldWorld).ProductionInfos.InputProducts);
            Assert.Equal("dummy.png", result.Single(x => x.Region == WorldRegion.OldWorld).ProductionInfos.InputProducts[0].Icon);
            Assert.Null(result.Single(x => x.Region == WorldRegion.NewWorld).ProductionInfos);
        }

        [Fact]
        public void GetInfobox_WikiTextContainsMultipleInputInfos_ShouldReturnCorrectOrderedValue()
        {
            // Arrange
            var input = $"|Produces Amount{Environment.NewLine}|Input 2 Amount (OW) = 21{Environment.NewLine}|Input 1 Amount (OW) = 42";

            var parser = new ParserBothWorlds(mockedCommons);

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(2, result.Single(x => x.Region == WorldRegion.OldWorld).ProductionInfos.InputProducts.Count);
            Assert.Equal(42, result.Single(x => x.Region == WorldRegion.OldWorld).ProductionInfos.InputProducts[0].Amount);
            Assert.Equal(21, result.Single(x => x.Region == WorldRegion.OldWorld).ProductionInfos.InputProducts[1].Amount);
            Assert.Null(result.Single(x => x.Region == WorldRegion.NewWorld).ProductionInfos);
        }

        #endregion

        #region SupplyInfo tests

        [Fact]
        public void GetInfobox_WikiTextContainsNoSupplyInfo_ShouldReturnCorrectValue()
        {
            // Arrange
            var parser = new ParserBothWorlds(mockedCommons);

            // Act
            var result = parser.GetInfobox(testDataEmpty_BothWorlds);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Null(result[0].SupplyInfos);
            Assert.Null(result[1].SupplyInfos);
        }

        [Fact]
        public void GetInfobox_WikiTextContainsSupplyAmount_ShouldReturnCorrectValue()
        {
            // Arrange
            var input = "|Supplies 1 Amount (OW) = 42";

            var parser = new ParserBothWorlds(mockedCommons);

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Single(result.Single(x => x.Region == WorldRegion.OldWorld).SupplyInfos.SupplyEntries);
            Assert.Equal(42, result.Single(x => x.Region == WorldRegion.OldWorld).SupplyInfos.SupplyEntries[0].Amount);
            Assert.Null(result.Single(x => x.Region == WorldRegion.NewWorld).SupplyInfos);
        }

        [Fact]
        public void GetInfobox_WikiTextContainsSupplyAmountAndWhiteSpace_ShouldReturnCorrectValue()
        {
            // Arrange
            var input = "|Supplies 1 Amount (OW)     =  42      ";

            var parser = new ParserBothWorlds(mockedCommons);

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Single(result.Single(x => x.Region == WorldRegion.OldWorld).SupplyInfos.SupplyEntries);
            Assert.Equal(42, result.Single(x => x.Region == WorldRegion.OldWorld).SupplyInfos.SupplyEntries[0].Amount);
            Assert.Null(result.Single(x => x.Region == WorldRegion.NewWorld).SupplyInfos);
        }

        [Fact]
        public void GetInfobox_WikiTextContainsSupplyAmountElectricity_ShouldReturnCorrectValue()
        {
            // Arrange
            var input = "|Supplies 1 Amount Electricity (OW) = 42";

            var parser = new ParserBothWorlds(mockedCommons);

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Single(result.Single(x => x.Region == WorldRegion.OldWorld).SupplyInfos.SupplyEntries);
            Assert.Equal(42, result.Single(x => x.Region == WorldRegion.OldWorld).SupplyInfos.SupplyEntries[0].AmountElectricity);
            Assert.Null(result.Single(x => x.Region == WorldRegion.NewWorld).SupplyInfos);
        }

        [Fact]
        public void GetInfobox_WikiTextContainsSupplyAmountElectricityAndWhiteSpace_ShouldReturnCorrectValue()
        {
            // Arrange
            var input = "|Supplies 1 Amount Electricity (OW)        =    42     ";

            var parser = new ParserBothWorlds(mockedCommons);

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Single(result.Single(x => x.Region == WorldRegion.OldWorld).SupplyInfos.SupplyEntries);
            Assert.Equal(42, result.Single(x => x.Region == WorldRegion.OldWorld).SupplyInfos.SupplyEntries[0].AmountElectricity);
            Assert.Null(result.Single(x => x.Region == WorldRegion.NewWorld).SupplyInfos);
        }

        [Fact]
        public void GetInfobox_WikiTextContainsSupplyAmountType_ShouldReturnCorrectValue()
        {
            // Arrange
            var input = "|Supplies 1 Type (OW) = Workers";

            var parser = new ParserBothWorlds(mockedCommons);

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Single(result.Single(x => x.Region == WorldRegion.OldWorld).SupplyInfos.SupplyEntries);
            Assert.Equal("Workers", result.Single(x => x.Region == WorldRegion.OldWorld).SupplyInfos.SupplyEntries[0].Type);
            Assert.Null(result.Single(x => x.Region == WorldRegion.NewWorld).SupplyInfos);
        }

        [Fact]
        public void GetInfobox_WikiTextContainsSupplyAmountTypeAndWhiteSpace_ShouldReturnCorrectValue()
        {
            // Arrange
            var input = "|Supplies 1 Type (OW)  =    Workers         ";

            var parser = new ParserBothWorlds(mockedCommons);

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Single(result.Single(x => x.Region == WorldRegion.OldWorld).SupplyInfos.SupplyEntries);
            Assert.Equal("Workers", result.Single(x => x.Region == WorldRegion.OldWorld).SupplyInfos.SupplyEntries[0].Type);
            Assert.Null(result.Single(x => x.Region == WorldRegion.NewWorld).SupplyInfos);
        }

        [Fact]
        public void GetInfobox_WikiTextContainsMultipleSupplyInfos_ShouldReturnCorrectOrderedValue()
        {
            // Arrange
            var input = $"|Supplies 2 Amount (OW) = 21{Environment.NewLine}|Supplies 1 Amount (OW) = 42";

            var parser = new ParserBothWorlds(mockedCommons);

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(2, result.Single(x => x.Region == WorldRegion.OldWorld).SupplyInfos.SupplyEntries.Count);
            Assert.Equal(42, result.Single(x => x.Region == WorldRegion.OldWorld).SupplyInfos.SupplyEntries[0].Amount);
            Assert.Equal(21, result.Single(x => x.Region == WorldRegion.OldWorld).SupplyInfos.SupplyEntries[1].Amount);
            Assert.Null(result.Single(x => x.Region == WorldRegion.NewWorld).SupplyInfos);
        }

        [Fact]
        public void GetInfobox_SupplyAmountCounterIsNotParseable_ShouldThrow()
        {
            // Arrange
            var input = $"|Supplies {int.MaxValue + 1L} Amount (OW) = 42";

            var parser = new ParserBothWorlds(mockedCommons);

            // Act/Assert
            var ex = Assert.Throws<Exception>(() => parser.GetInfobox(input));
        }

        [Fact(Skip = "regex needs adjustment")]
        public void GetInfobox_SupplyAmountIsNotParseable_ShouldThrow()
        {
            // Arrange
            var input = $"|Supplies 1 Amount (OW) = {double.MaxValue.ToString()}";

            var parser = new ParserBothWorlds(mockedCommons);

            // Act/Assert
            var ex = Assert.Throws<Exception>(() => parser.GetInfobox(input));
        }

        [Fact]
        public void GetInfobox_SupplyAmountElectricityCounterIsNotParseable_ShouldThrow()
        {
            // Arrange
            var input = $"|Supplies {int.MaxValue + 1L} Amount Electricity (OW) = 42";

            var parser = new ParserBothWorlds(mockedCommons);

            // Act/Assert
            var ex = Assert.Throws<Exception>(() => parser.GetInfobox(input));
        }

        [Fact(Skip = "regex needs adjustment")]
        public void GetInfobox_SupplyAmountElectricityIsNotParseable_ShouldThrow()
        {
            // Arrange
            var input = $"|Supplies 1 Amount Electricity (OW) = {double.MaxValue.ToString()}";

            var parser = new ParserBothWorlds(mockedCommons);

            // Act/Assert
            var ex = Assert.Throws<Exception>(() => parser.GetInfobox(input));
        }

        [Fact]
        public void GetInfobox_SupplyTypeCounterIsNotParseable_ShouldThrow()
        {
            // Arrange
            var input = $"|Supplies {int.MaxValue + 1L} Type (OW) = Farmer";

            var parser = new ParserBothWorlds(mockedCommons);

            // Act/Assert
            var ex = Assert.Throws<Exception>(() => parser.GetInfobox(input));
        }

        #endregion

        #region UnlockInfo tests

        [Fact]
        public void GetInfobox_WikiTextContainsNoUnlockInfo_ShouldReturnCorrectValue()
        {
            // Arrange
            var parser = new ParserBothWorlds(mockedCommons);

            // Act
            var result = parser.GetInfobox(testDataEmpty_BothWorlds);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Null(result[0].UnlockInfos);
            Assert.Null(result[1].UnlockInfos);
        }

        [Fact]
        public void GetInfobox_WikiTextContainsUnlockAmount_ShouldReturnCorrectValue()
        {
            // Arrange
            var input = "|Unlock Condition 1 Amount (OW) = 42";

            var parser = new ParserBothWorlds(mockedCommons);

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Single(result.Single(x => x.Region == WorldRegion.OldWorld).UnlockInfos.UnlockConditions);
            Assert.Equal(42, result.Single(x => x.Region == WorldRegion.OldWorld).UnlockInfos.UnlockConditions[0].Amount);
            Assert.Null(result.Single(x => x.Region == WorldRegion.NewWorld).UnlockInfos);
        }

        [Fact]
        public void GetInfobox_WikiTextContainsUnlockAmountAndWhiteSpace_ShouldReturnCorrectValue()
        {
            // Arrange
            var input = "|Unlock Condition 1 Amount (OW)   =    42          ";

            var parser = new ParserBothWorlds(mockedCommons);

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Single(result.Single(x => x.Region == WorldRegion.OldWorld).UnlockInfos.UnlockConditions);
            Assert.Equal(42, result.Single(x => x.Region == WorldRegion.OldWorld).UnlockInfos.UnlockConditions[0].Amount);
            Assert.Null(result.Single(x => x.Region == WorldRegion.NewWorld).UnlockInfos);
        }

        [Fact]
        public void GetInfobox_WikiTextContainsUnlockType_ShouldReturnCorrectValue()
        {
            // Arrange
            var input = "|Unlock Condition 1 Type (OW) = Workers";

            var parser = new ParserBothWorlds(mockedCommons);

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Single(result.Single(x => x.Region == WorldRegion.OldWorld).UnlockInfos.UnlockConditions);
            Assert.Equal("Workers", result.Single(x => x.Region == WorldRegion.OldWorld).UnlockInfos.UnlockConditions[0].Type);
            Assert.Null(result.Single(x => x.Region == WorldRegion.NewWorld).UnlockInfos);
        }

        [Fact]
        public void GetInfobox_WikiTextContainsUnlockTypeAndWhiteSpace_ShouldReturnCorrectValue()
        {
            // Arrange
            var input = "|Unlock Condition 1 Type (OW)  =   Workers     ";

            var parser = new ParserBothWorlds(mockedCommons);

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Single(result.Single(x => x.Region == WorldRegion.OldWorld).UnlockInfos.UnlockConditions);
            Assert.Equal("Workers", result.Single(x => x.Region == WorldRegion.OldWorld).UnlockInfos.UnlockConditions[0].Type);
            Assert.Null(result.Single(x => x.Region == WorldRegion.NewWorld).UnlockInfos);
        }

        [Fact]
        public void GetInfobox_WikiTextContainsMultipleUnlockInfos_ShouldReturnCorrectOrderedValue()
        {
            // Arrange
            var input = $"|Unlock Condition 2 Amount (OW) = 21{Environment.NewLine}|Unlock Condition 1 Amount (OW) = 42";

            var parser = new ParserBothWorlds(mockedCommons);

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(2, result.Single(x => x.Region == WorldRegion.OldWorld).UnlockInfos.UnlockConditions.Count);
            Assert.Equal(42, result.Single(x => x.Region == WorldRegion.OldWorld).UnlockInfos.UnlockConditions[0].Amount);
            Assert.Equal(21, result.Single(x => x.Region == WorldRegion.OldWorld).UnlockInfos.UnlockConditions[1].Amount);
            Assert.Null(result.Single(x => x.Region == WorldRegion.NewWorld).UnlockInfos);
        }

        [Fact]
        public void GetInfobox_UnlockAmountCounterIsNotParseable_ShouldThrow()
        {
            // Arrange
            var input = $"|Unlock Condition {int.MaxValue + 1L} Amount (OW) = 42";

            var parser = new ParserBothWorlds(mockedCommons);

            // Act/Assert
            var ex = Assert.Throws<Exception>(() => parser.GetInfobox(input));
        }

        [Fact(Skip = "regex needs adjustment")]
        public void GetInfobox_UnlockAmountIsNotParseable_ShouldThrow()
        {
            // Arrange
            var input = $"|Unlock Condition 1 Amount (OW) = {double.MaxValue.ToString()}";

            var parser = new ParserBothWorlds(mockedCommons);

            // Act/Assert
            var ex = Assert.Throws<Exception>(() => parser.GetInfobox(input));
        }

        [Fact]
        public void GetInfobox_UnlockTypeCounterIsNotParseable_ShouldThrow()
        {
            // Arrange
            var input = $"|Unlock Condition {int.MaxValue + 1L} Type (OW) = Farmers";

            var parser = new ParserBothWorlds(mockedCommons);

            // Act/Assert
            var ex = Assert.Throws<Exception>(() => parser.GetInfobox(input));
        }

        #endregion

        #region specific buildings tests       

        [Fact]
        [UseCulture("en-US")]
        public void GetInfobox_WikiTextIsPoliceStation_ShouldReturnCorrectResult()
        {
            // Arrange
            var parser = new ParserBothWorlds(mockedCommons);

            // Act
            var result = parser.GetInfobox(testDataPoliceStation);

            // Assert
            //Old World
            Assert.Equal("Police Station", result[0].Name);
            Assert.Equal(BuildingType.Institution, result[0].Type);
            Assert.Null(result[0].ProductionInfos);
            Assert.Null(result[0].SupplyInfos);

            Assert.Equal(WorldRegion.OldWorld, result[0].Region);
            Assert.Single(result[0].UnlockInfos.UnlockConditions);
            Assert.Equal(500, result[0].UnlockInfos.UnlockConditions[0].Amount);
            Assert.Equal("Workers", result[0].UnlockInfos.UnlockConditions[0].Type);

            //New World
            Assert.Equal("Police Station", result[1].Name);
            Assert.Equal(BuildingType.Institution, result[1].Type);
            Assert.Null(result[1].ProductionInfos);
            Assert.Null(result[1].SupplyInfos);

            Assert.Equal(WorldRegion.NewWorld, result[1].Region);
            Assert.Single(result[1].UnlockInfos.UnlockConditions);
            Assert.Equal(0, result[1].UnlockInfos.UnlockConditions[0].Amount);
            Assert.Equal("Jornaleros", result[1].UnlockInfos.UnlockConditions[0].Type);
        }

        [Fact]
        [UseCulture("en-US")]
        public void GetInfobox_WikiTextIsBrickFactory_ShouldReturnCorrectResult()
        {
            // Arrange
            var parser = new ParserBothWorlds(mockedCommons);

            // Act
            var result = parser.GetInfobox(testDataBrickFactory);

            // Assert
            //Old World
            Assert.Equal("Brick Factory", result[0].Name);
            Assert.Equal(BuildingType.Production, result[0].Type);
            Assert.NotNull(result[0].ProductionInfos);
            Assert.NotNull(result[0].ProductionInfos.EndProduct);
            Assert.Equal(1, result[0].ProductionInfos.EndProduct.Amount);
            Assert.Equal(2, result[0].ProductionInfos.EndProduct.AmountElectricity);
            Assert.Equal("Bricks.png", result[0].ProductionInfos.EndProduct.Icon);
            Assert.Single(result[0].ProductionInfos.InputProducts);
            Assert.Equal(1, result[0].ProductionInfos.InputProducts[0].Amount);
            Assert.Equal(2, result[0].ProductionInfos.InputProducts[0].AmountElectricity);
            Assert.Equal("Clay.png", result[0].ProductionInfos.InputProducts[0].Icon);
            Assert.Null(result[0].SupplyInfos);
            Assert.Equal(WorldRegion.OldWorld, result[0].Region);
            Assert.Single(result[0].UnlockInfos.UnlockConditions);
            Assert.Equal(1, result[0].UnlockInfos.UnlockConditions[0].Amount);
            Assert.Equal("Workers", result[0].UnlockInfos.UnlockConditions[0].Type);

            //New World
            Assert.Equal("Brick Factory", result[1].Name);
            Assert.Equal(BuildingType.Production, result[1].Type);
            Assert.NotNull(result[1].ProductionInfos);
            Assert.NotNull(result[1].ProductionInfos.EndProduct);
            Assert.Equal(1, result[1].ProductionInfos.EndProduct.Amount);
            Assert.Equal(0, result[1].ProductionInfos.EndProduct.AmountElectricity);
            Assert.Equal("Bricks.png", result[1].ProductionInfos.EndProduct.Icon);
            Assert.Single(result[1].ProductionInfos.InputProducts);
            Assert.Equal(1, result[1].ProductionInfos.InputProducts[0].Amount);
            Assert.Equal(0, result[1].ProductionInfos.InputProducts[0].AmountElectricity);
            Assert.Equal("Clay.png", result[1].ProductionInfos.InputProducts[0].Icon);
            Assert.Null(result[1].SupplyInfos);
            Assert.Equal(WorldRegion.NewWorld, result[1].Region);
            Assert.Single(result[1].UnlockInfos.UnlockConditions);
            Assert.Equal(1, result[1].UnlockInfos.UnlockConditions[0].Amount);
            Assert.Equal("Obreros", result[1].UnlockInfos.UnlockConditions[0].Type);
        }

        #endregion
    }
}
