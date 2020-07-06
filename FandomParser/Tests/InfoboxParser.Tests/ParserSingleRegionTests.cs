using System;
using System.Collections.Generic;
using System.Drawing;
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
using Xunit.Abstractions;

namespace InfoboxParser.Tests
{
    public class ParserSingleRegionTests
    {
        private static readonly ICommons _mockedCommons;
        private readonly ITestOutputHelper _output;
        private static readonly ISpecialBuildingNameHelper _mockedSpecialBuildingNameHelper;
        private static readonly ITitleParserSingle _mockedTitleParserSingle;

        private static readonly string testDataSchnapps_Distillery;
        private static readonly string testDataBakery;
        private static readonly string testDataCannery;
        private static readonly string testDataChapel;
        private static readonly string testDataEmpty;

        #region ctor

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

        public ParserSingleRegionTests(ITestOutputHelper testOutputHelperToUse)
        {
            _output = testOutputHelperToUse;
        }

        #endregion

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

        public static TheoryData<string, string> BuildingNameTestData
        {
            get
            {
                return new TheoryData<string, string>
                {
                    { "|Title = Marketplace", "Marketplace"},
                    { "|Title = Police Station", "Police Station"},
                    { "|Title = Small Palm Tree", "Small Palm Tree"},
                    { "|Title = Bomb­ín Weaver", "Bomb­ín Weaver"},
                    { "|Title     = Marketplace", "Marketplace"},
                    { "|Title     =     Marketplace    ", "Marketplace"},
                    { "|Title     = Marketplace}}", "Marketplace"},
                    { "|Title     =     Marketplace    }}", "Marketplace"},
                };
            }
        }

        public static TheoryData<string, Size> BuildingSizeTestData
        {
            get
            {
                return new TheoryData<string, Size>
                {
                    { "|Building Size = 3x3", new Size(3, 3) },
                    { "|Building Size = 1x3", new Size(1, 3) },
                    { "|Building Size = 1x10", new Size(1, 10) },
                    { "|Building Size = 10x1", new Size(10, 1) },
                    { "|Building Size = 18x22", new Size(18, 22) },
                    { "|Building Size      =      1   x   10  ", new Size(1, 10) },
                    { "|Building Size = 4 x 5", new Size(4, 5) },
                    { "|Building Size = 4x5}}", new Size(4, 5) },
                    { "|Building Size = 4x5   }}", new Size(4, 5) },
                    { "|Building Size = 5x11 (5x16 in water)", new Size(5, 11) },
                    { "|Building Size = 5x8 (5x13)", new Size(5, 8) },
                    { "|Building Size   = 5x7 (partially submerged)", new Size(5, 7) },
                };
            }
        }

        public static TheoryData<string, string> BuildingIconTestData
        {
            get
            {
                return new TheoryData<string, string>
                {
                    { "|Building Icon = Charcoal_kiln.png", "Charcoal_kiln.png" },
                    { "|Building Icon = Furs.png", "Furs.png" },
                    { "|Building Icon = Furs.png}}", "Furs.png" },
                    { "|Building Icon = Furs.png    }}", "Furs.png" },
                    { "|Building Icon      =    Furs.png   ", "Furs.png" },
                    { "|Building Icon = Furs.jpeg", "Furs.jpeg" },
                    { "|Building Icon = Arctic Lodge.png", "Arctic Lodge.png" },
                    { "|Building Icon = Bear Hunting Cabin.png", "Bear Hunting Cabin.png" },
                    { "|Building Icon = Cocoa_0.png", "Cocoa_0.png" },
                    { "|Building Icon = Icon electric works gas 0.png ", "Icon electric works gas 0.png" },
                    { "|Building Icon = Harbourmaster's Office.png", "Harbourmaster's Office.png" },
                    { "|Building Icon = Harbourmaster´s Office.png", "Harbourmaster´s Office.png" },
                    { "|Building Icon = Harbourmaster`s Office.png", "Harbourmaster`s Office.png" },
                };
            }
        }

        public static TheoryData<string, int, double> UnlockInfoAmountTestData
        {
            get
            {
                return new TheoryData<string, int, double>
                {
                    { "|Unlock Condition 1 Amount = 42", 1, 42d },
                    { "|Unlock Condition 1 Amount = -42", 1, -42d },
                    { "|Unlock Condition 1 Amount = 42,21", 1, 42.21d },
                    { "|Unlock Condition 1 Amount = -42,21", 1, -42.21d },
                    { "|Unlock Condition   1   Amount   =   42", 1, 42d },
                    { "|Unlock Condition 1 Amount = 42}}", 1, 42d },
                    { "|Unlock Condition    1    Amount    =     42   }}", 1, 42d },
                    { "|Unlock Condition 4 Amount = 42", 4, 42d },
                };
            }
        }

        public static TheoryData<string, int, string> UnlockInfoTypeTestData
        {
            get
            {
                return new TheoryData<string, int, string>
                {
                    { "|Unlock Condition 1 Type = Technicians", 1, "Technicians" },
                    { "|Unlock Condition 1 Type = Engineers", 1, "Engineers" },
                    { "|Unlock Condition 1 Type = Obreros", 1, "Obreros" },
                    { "|Unlock Condition 1 Type = Investors", 1, "Investors" },
                    { "|Unlock Condition 1 Type = Jornaleros", 1, "Jornaleros" },
                    { "|Unlock Condition 1 Type = Workers", 1, "Workers" },
                    { "|Unlock Condition 1 Type = dummy with spaces", 1, "dummy with spaces" },
                    { "|Unlock Condition 4 Type = Technicians", 4, "Technicians" },
                    { "|Unlock Condition    1   Type    =    Technicians   ", 1, "Technicians" },
                    { "|Unlock Condition 1 Type = Technicians}}", 1, "Technicians" },
                    { "|Unlock Condition    1    Type    =    Technicians   }}", 1, "Technicians" },
                };
            }
        }

        public static TheoryData<string, int, double> SupplyInfoAmountTestData
        {
            get
            {
                return new TheoryData<string, int, double>
                {
                    { "|Supplies 1 Amount = 42", 1, 42d },
                    { "|Supplies 1 Amount = -42", 1, -42d },
                    { "|Supplies 1 Amount = 42,21", 1, 42.21d },
                    { "|Supplies 1 Amount = -42,21", 1, -42.21d },
                    { "|Supplies   1   Amount   =   42", 1, 42d },
                    { "|Supplies 1 Amount = 42}}", 1, 42d },
                    { "|Supplies    1    Amount    =     42   }}", 1, 42d },
                    { "|Supplies 4 Amount = 42", 4, 42d },
                };
            }
        }

        public static TheoryData<string, int, double> SupplyInfoAmountElectricityTestData
        {
            get
            {
                return new TheoryData<string, int, double>
                {
                    { "|Supplies 1 Amount Electricity = 42", 1, 42d },
                    { "|Supplies 1 Amount Electricity = -42", 1, -42d },
                    { "|Supplies 1 Amount Electricity = 42,21", 1, 42.21d },
                    { "|Supplies 1 Amount Electricity = -42,21", 1, -42.21d },
                    { "|Supplies   1   Amount Electricity   =   42", 1, 42d },
                    { "|Supplies 1 Amount Electricity = 42}}", 1, 42d },
                    { "|Supplies    1    Amount Electricity    =     42   }}", 1, 42d },
                    { "|Supplies 4 Amount Electricity = 42", 4, 42d },
                };
            }
        }

        public static TheoryData<string, int, string> SupplyInfoTypeTestData
        {
            get
            {
                return new TheoryData<string, int, string>
                {
                    { "|Supplies 1 Type = Technicians", 1, "Technicians" },
                    { "|Supplies 1 Type = Engineers", 1, "Engineers" },
                    { "|Supplies 1 Type = Obreros", 1, "Obreros" },
                    { "|Supplies 1 Type = Investors", 1, "Investors" },
                    { "|Supplies 1 Type = Jornaleros", 1, "Jornaleros" },
                    { "|Supplies 1 Type = Workers", 1, "Workers" },
                    { "|Supplies 1 Type = dummy with spaces", 1, "dummy with spaces" },
                    { "|Supplies 4 Type = Technicians", 4, "Technicians" },
                    { "|Supplies    1   Type    =    Technicians   ", 1, "Technicians" },
                    { "|Supplies 1 Type = Technicians}}", 1, "Technicians" },
                    { "|Supplies    1    Type    =    Technicians   }}", 1, "Technicians" },
                };
            }
        }

        public static TheoryData<string, double> ConstructionInfoCreditsTestData
        {
            get
            {
                return new TheoryData<string, double>
                {
                    { "|Credits = 15000", 15000d },
                    { "|Credits = 150", 150d },
                    { "|Credits = 42,21", 42.21 },
                    { "|Credits = -42,21", -42.21 },
                    //{ "|Credits = 2,500", 2500 },//incorrect numberstyle on wiki
                    //{ "|Credits = 4,000", 4000 },//incorrect numberstyle on wiki
                    //{ "|Credits = 42.21", 42.21 },//incorrect numberstyle on wiki
                    //{ "|Credits = -42.21", -42.21 },//incorrect numberstyle on wiki
                    { "|Credits = -100", -100d },
                    { "|Credits    =    150   ", 150d },
                    { "|Credits = 150}}", 150d },
                    { "|Credits = 42,21   }}", 42.21 },
                };
            }
        }

        public static TheoryData<string, double> ConstructionInfoTimberTestData
        {
            get
            {
                return new TheoryData<string, double>
                {
                    { "|Timber = 15000", 15000d },
                    { "|Timber = 150", 150d },
                    { "|Timber = 42,21", 42.21 },
                    { "|Timber = -42,21", -42.21 },
                    //{ "|Timber = 42.21", 42.21 },//incorrect numberstyle on wiki
                    //{ "|Timber = -42.21", -42.21 },//incorrect numberstyle on wiki
                    { "|Timber = -100", -100d },
                    { "|Timber    =    150   ", 150d },
                    { "|Timber = 150}}", 150d },
                    { "|Timber = 42,21   }}", 42.21 },
                };
            }
        }

        public static TheoryData<string, double> ConstructionInfoBricksTestData
        {
            get
            {
                return new TheoryData<string, double>
                {
                    { "|Bricks = 15000", 15000d },
                    { "|Bricks = 150", 150d },
                    { "|Bricks = 42,21", 42.21 },
                    { "|Bricks = -42,21", -42.21 },
                    //{ "|Bricks = 42.21", 42.21 },//incorrect numberstyle on wiki
                    //{ "|Bricks = -42.21", -42.21 },//incorrect numberstyle on wiki
                    { "|Bricks = -100", -100d },
                    { "|Bricks    =    150   ", 150d },
                    { "|Bricks = 150}}", 150d },
                    { "|Bricks = 42,21   }}", 42.21 },
                };
            }
        }

        public static TheoryData<string, double> ConstructionInfoSteelBeamsTestData
        {
            get
            {
                return new TheoryData<string, double>
                {
                    { "|Steel Beams = 15000", 15000d },
                    { "|Steel Beams = 150", 150d },
                    { "|Steel Beams = 42,21", 42.21 },
                    { "|Steel Beams = -42,21", -42.21 },
                    //{ "|Steel Beams = 42.21", 42.21 },//incorrect numberstyle on wiki
                    //{ "|Steel Beams = -42.21", -42.21 },//incorrect numberstyle on wiki
                    { "|Steel Beams = -100", -100d },
                    { "|Steel Beams    =    150   ", 150d },
                    { "|Steel Beams = 150}}", 150d },
                    { "|Steel Beams = 42,21   }}", 42.21 },
                };
            }
        }

        public static TheoryData<string, double> ConstructionInfoWindowsTestData
        {
            get
            {
                return new TheoryData<string, double>
                {
                    { "|Windows = 15000", 15000d },
                    { "|Windows = 150", 150d },
                    { "|Windows = 42,21", 42.21 },
                    { "|Windows = -42,21", -42.21 },
                    //{ "|Windows = 42.21", 42.21 },//incorrect numberstyle on wiki
                    //{ "|Windows = -42.21", -42.21 },//incorrect numberstyle on wiki
                    { "|Windows = -100", -100d },
                    { "|Windows    =    150   ", 150d },
                    { "|Windows = 150}}", 150d },
                    { "|Windows = 42,21   }}", 42.21 },
                };
            }
        }

        public static TheoryData<string, double> ConstructionInfoConcreteTestData
        {
            get
            {
                return new TheoryData<string, double>
                {
                    { "|Concrete = 15000", 15000d },
                    { "|Concrete = 150", 150d },
                    { "|Concrete = 42,21", 42.21 },
                    { "|Concrete = -42,21", -42.21 },
                    //{ "|Concrete = 42.21", 42.21 },//incorrect numberstyle on wiki
                    //{ "|Concrete = -42.21", -42.21 },//incorrect numberstyle on wiki
                    { "|Concrete = -100", -100d },
                    { "|Concrete    =    150   ", 150d },
                    { "|Concrete = 150}}", 150d },
                    { "|Concrete = 42,21   }}", 42.21 },
                };
            }
        }

        public static TheoryData<string, double> ConstructionInfoWeaponsTestData
        {
            get
            {
                return new TheoryData<string, double>
                {
                    { "|Weapons = 15000", 15000d },
                    { "|Weapons = 150", 150d },
                    { "|Weapons = 42,21", 42.21 },
                    { "|Weapons = -42,21", -42.21 },
                    //{ "|Weapons = 42.21", 42.21 },//incorrect numberstyle on wiki
                    //{ "|Weapons = -42.21", -42.21 },//incorrect numberstyle on wiki
                    { "|Weapons = -100", -100d },
                    { "|Weapons    =    150   ", 150d },
                    { "|Weapons = 150}}", 150d },
                    { "|Weapons = 42,21   }}", 42.21 },
                };
            }
        }

        public static TheoryData<string, double> ConstructionInfoAdvancedWeaponsTestData
        {
            get
            {
                return new TheoryData<string, double>
                {
                    { "|Advanced Weapons = 15000", 15000d },
                    { "|Advanced Weapons = 150", 150d },
                    { "|Advanced Weapons = 42,21", 42.21 },
                    { "|Advanced Weapons = -42,21", -42.21 },
                    //{ "|Advanced Weapons = 42.21", 42.21 },//incorrect numberstyle on wiki
                    //{ "|Advanced Weapons = -42.21", -42.21 },//incorrect numberstyle on wiki
                    { "|Advanced Weapons = -100", -100d },
                    { "|Advanced Weapons    =    150   ", 150d },
                    { "|Advanced Weapons = 150}}", 150d },
                    { "|Advanced Weapons = 42,21   }}", 42.21 },
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
            _output.WriteLine($"{nameof(input)}: {input}");

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
        [MemberData(nameof(BuildingNameTestData))]
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

        [Theory]
        [MemberData(nameof(SupplyInfoAmountTestData))]
        public void GetInfobox_WikiTextContainsSupplyAmount_ShouldReturnCorrectValue(string input, int expectedOrder, double expectedAmount)
        {
            // Arrange
            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Single(result);
            Assert.Single(result[0].SupplyInfos.SupplyEntries);
            Assert.Equal(0, result[0].SupplyInfos.SupplyEntries[0].AmountElectricity);
            Assert.Equal(expectedAmount, result[0].SupplyInfos.SupplyEntries[0].Amount);
            Assert.Equal(expectedOrder, result[0].SupplyInfos.SupplyEntries[0].Order);
            Assert.Null(result[0].SupplyInfos.SupplyEntries[0].Type);
        }

        [Theory]
        [MemberData(nameof(SupplyInfoAmountElectricityTestData))]
        public void GetInfobox_WikiTextContainsSupplyAmountElectricity_ShouldReturnCorrectValue(string input, int expectedOrder, double expectedAmount)
        {
            // Arrange
            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Single(result);
            Assert.Single(result[0].SupplyInfos.SupplyEntries);
            Assert.Equal(0, result[0].SupplyInfos.SupplyEntries[0].Amount);
            Assert.Equal(expectedAmount, result[0].SupplyInfos.SupplyEntries[0].AmountElectricity);
            Assert.Equal(expectedOrder, result[0].SupplyInfos.SupplyEntries[0].Order);
            Assert.Null(result[0].SupplyInfos.SupplyEntries[0].Type);
        }

        [Theory]
        [MemberData(nameof(SupplyInfoTypeTestData))]
        public void GetInfobox_WikiTextContainsSupplyType_ShouldReturnCorrectValue(string input, int expectedOrder, string expectedType)
        {
            // Arrange
            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Single(result);
            Assert.Single(result[0].SupplyInfos.SupplyEntries);
            Assert.Equal(0, result[0].SupplyInfos.SupplyEntries[0].Amount);
            Assert.Equal(0, result[0].SupplyInfos.SupplyEntries[0].AmountElectricity);
            Assert.Equal(expectedType, result[0].SupplyInfos.SupplyEntries[0].Type);
            Assert.Equal(expectedOrder, result[0].SupplyInfos.SupplyEntries[0].Order);
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

        [Theory]
        [MemberData(nameof(UnlockInfoAmountTestData))]
        public void GetInfobox_WikiTextContainsUnlockAmount_ShouldReturnCorrectValue(string input, int expectedOrder, double expectedAmount)
        {
            // Arrange
            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Single(result);
            Assert.Single(result[0].UnlockInfos.UnlockConditions);
            Assert.Equal(expectedAmount, result[0].UnlockInfos.UnlockConditions[0].Amount);
            Assert.Equal(expectedOrder, result[0].UnlockInfos.UnlockConditions[0].Order);
            Assert.Null(result[0].UnlockInfos.UnlockConditions[0].Type);
        }

        [Theory]
        [MemberData(nameof(UnlockInfoTypeTestData))]
        public void GetInfobox_WikiTextContainsUnlockType_ShouldReturnCorrectValue(string input, int expectedOrder, string expectedType)
        {
            // Arrange
            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Single(result);
            Assert.Single(result[0].UnlockInfos.UnlockConditions);
            Assert.Equal(0, result[0].UnlockInfos.UnlockConditions[0].Amount);
            Assert.Equal(expectedOrder, result[0].UnlockInfos.UnlockConditions[0].Order);
            Assert.Equal(expectedType, result[0].UnlockInfos.UnlockConditions[0].Type);
        }

        [Fact]
        public void GetInfobox_WikiTextContainsMultipleUnlockAmountInfos_ShouldReturnCorrectOrderedValue()
        {
            // Arrange
            var input = $"|Unlock Condition 2 Amount = 21{Environment.NewLine}|Unlock Condition 1 Amount = 42";

            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Single(result);
            Assert.Equal(2, result[0].UnlockInfos.UnlockConditions.Count);
            Assert.Equal(1, result[0].UnlockInfos.UnlockConditions[0].Order);
            Assert.Equal(2, result[0].UnlockInfos.UnlockConditions[1].Order);
        }

        [Fact]
        public void GetInfobox_WikiTextContainsMultipleUnlockTypeInfos_ShouldReturnCorrectOrderedValue()
        {
            // Arrange
            var input = $"|Unlock Condition 2 Type = Technicians{Environment.NewLine}|Unlock Condition 1 Type = Workers";

            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Single(result);
            Assert.Equal(2, result[0].UnlockInfos.UnlockConditions.Count);
            Assert.Equal(1, result[0].UnlockInfos.UnlockConditions[0].Order);
            Assert.Equal(2, result[0].UnlockInfos.UnlockConditions[1].Order);
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

        #region BuildingSize tests

        [Theory]
        [InlineData("dummy")]
        [InlineData("|Building Size = ")]
        public void GetInfobox_WikiTextContainsNoBuildingSize_ShouldReturnEmptySize(string input)
        {
            // Arrange
            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(Size.Empty, result[0].BuildingSize);
        }

        [Theory]
        [InlineData("|Building Size = ?x?")]
        [InlineData("|Building Size = ? x ?")]
        [InlineData("|Building Size = ? x?")]
        [InlineData("|Building Size = ?x ?")]
        [InlineData("|Building Size = 3x")]
        [InlineData("|Building Size = dummyxdummy")]
        public void GetInfobox_WikiTextContainsUnknownBuildingSize_ShouldReturnEmptySize(string input)
        {
            // Arrange
            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(Size.Empty, result[0].BuildingSize);
        }

        [Theory]
        [MemberData(nameof(BuildingSizeTestData))]
        public void GetInfobox_WikiTextContainsBuildingSize_ShouldReturnCorrectValue(string input, Size expectedSize)
        {
            // Arrange
            _output.WriteLine($"{nameof(input)}: {input}");

            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(expectedSize, result[0].BuildingSize);
        }

        #endregion

        #region BuildingIcon tests

        [Theory]
        [InlineData("dummy")]
        [InlineData("|Building Icon = ")]
        public void GetInfobox_WikiTextContainsNoBuildingIcon_ShouldReturnEmpty(string input)
        {
            // Arrange
            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(string.Empty, result[0].Icon);
        }

        [Theory]
        [MemberData(nameof(BuildingIconTestData))]
        public void GetInfobox_WikiTextContainsBuildingIcon_ShouldReturnCorrectValue(string input, string expectedIcon)
        {
            // Arrange
            _output.WriteLine($"{nameof(input)}: {input}");

            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(expectedIcon, result[0].Icon);
        }

        #endregion

        #region ConstructionInfo tests

        [Theory]
        [InlineData("dummy")]
        [InlineData("|Credits = ")]
        [InlineData("|Timber = ")]
        [InlineData("|Bricks = ")]
        [InlineData("|Steel Beams = ")]
        [InlineData("|Windows = ")]
        [InlineData("|Concrete = ")]
        [InlineData("|Weapons = ")]
        [InlineData("|Advanced Weapons = ")]
        public void GetInfobox_WikiTextContainsNoConstructionInfo_ShouldReturnEmptyList(string input)
        {
            // Arrange
            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Empty(result[0].ConstructionInfos);
        }

        [Theory]
        [MemberData(nameof(ConstructionInfoCreditsTestData))]
        [MemberData(nameof(ConstructionInfoTimberTestData))]
        [MemberData(nameof(ConstructionInfoBricksTestData))]
        [MemberData(nameof(ConstructionInfoSteelBeamsTestData))]
        [MemberData(nameof(ConstructionInfoWindowsTestData))]
        [MemberData(nameof(ConstructionInfoConcreteTestData))]
        [MemberData(nameof(ConstructionInfoWeaponsTestData))]
        [MemberData(nameof(ConstructionInfoAdvancedWeaponsTestData))]
        public void GetInfobox_WikiTextContainsConstructionInfo_ShouldReturnCorrectValue(string input, double expectedValue)
        {
            // Arrange
            _output.WriteLine($"{nameof(input)}: {input}");

            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(expectedValue, result[0].ConstructionInfos[0].Value);
        }

        [Fact]
        public void GetInfobox_ConstructionInfoContainsConcrete_ShouldReturnAdjustedValue()
        {
            // Arrange
            var input = ConstructionInfoConcreteTestData.First();
            var expectedUnitName = "Reinforced Concrete";

            var parser = GetParser();

            // Act
            var result = parser.GetInfobox((string)input[0]);

            // Assert
            Assert.Equal(expectedUnitName, result[0].ConstructionInfos[0].Unit.Name);
        }

        #endregion
    }
}
