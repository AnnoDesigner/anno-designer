using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FandomParser.Core;
using FandomParser.Core.Presets.Models;
using InfoboxParser.Models;
using InfoboxParser.Parser;
using Xunit;
using Xunit.Abstractions;

namespace InfoboxParser.Tests
{
    public class ParserMultipleRegionsTests
    {
        private const string PLACEHOLDER_REGION = "~Region~";
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

        public static TheoryData<string, string, List<string>> BuildingNameTestData
        {
            get
            {
                return new TheoryData<string, string, List<string>>
                {
                    { $"|Title {PLACEHOLDER_REGION} = Marketplace", "Marketplace", _regionList2Regions },
                    { $"|Title {PLACEHOLDER_REGION} = Marketplace", "Marketplace", _regionList3Regions },
                    { $"|Title {PLACEHOLDER_REGION} = Police Station", "Police Station", _regionList2Regions},
                    { $"|Title {PLACEHOLDER_REGION} = Police Station", "Police Station", _regionList3Regions},
                    { $"|Title {PLACEHOLDER_REGION} = Small Palm Tree", "Small Palm Tree", _regionList2Regions},
                    { $"|Title {PLACEHOLDER_REGION} = Small Palm Tree", "Small Palm Tree", _regionList3Regions},
                    //{ $"|Title {PLACEHOLDER_REGION} = Bomb­ín Weaver", "Bomb­ín Weaver", _regionList2Regions},
                    //{ $"|Title {PLACEHOLDER_REGION} = Bomb­ín Weaver", "Bomb­ín Weaver", _regionList3Regions},
                    { $"|Title {PLACEHOLDER_REGION}     = Marketplace", "Marketplace", _regionList2Regions},
                    { $"|Title {PLACEHOLDER_REGION}     = Marketplace", "Marketplace", _regionList3Regions},
                    { $"|Title {PLACEHOLDER_REGION}     =     Marketplace    ", "Marketplace", _regionList2Regions},
                    { $"|Title {PLACEHOLDER_REGION}     =     Marketplace    ", "Marketplace", _regionList3Regions},
                    { $"|Title {PLACEHOLDER_REGION}     = Marketplace}}", "Marketplace", _regionList2Regions},
                    { $"|Title {PLACEHOLDER_REGION}     = Marketplace}}", "Marketplace", _regionList3Regions},
                    { $"|Title {PLACEHOLDER_REGION}     =     Marketplace    }}", "Marketplace", _regionList2Regions},
                    { $"|Title {PLACEHOLDER_REGION}     =     Marketplace    }}", "Marketplace", _regionList3Regions},
                    { $"|Title {PLACEHOLDER_REGION} = ", string.Empty, _regionList2Regions },
                    { $"|Title {PLACEHOLDER_REGION} = ", string.Empty, _regionList3Regions },
                };
            }
        }

        public static TheoryData<string, WorldRegion, List<string>> RegionNameTestData
        {
            get
            {
                return new TheoryData<string, WorldRegion, List<string>>
                {
                    { $"|Tab {PLACEHOLDER_REGION} = Arctic", WorldRegion.Arctic, _regionList2Regions },
                    { $"|Tab {PLACEHOLDER_REGION} = Arctic", WorldRegion.Arctic, _regionList3Regions },
                    { $"|Tab {PLACEHOLDER_REGION} = The Arctic", WorldRegion.Arctic, _regionList2Regions },
                    { $"|Tab {PLACEHOLDER_REGION} = The Arctic", WorldRegion.Arctic, _regionList3Regions },
                    { $"|Tab {PLACEHOLDER_REGION} = New World", WorldRegion.NewWorld, _regionList2Regions },
                    { $"|Tab {PLACEHOLDER_REGION} = New World", WorldRegion.NewWorld, _regionList3Regions },
                    { $"|Tab {PLACEHOLDER_REGION} = Old World", WorldRegion.OldWorld, _regionList2Regions },
                    { $"|Tab {PLACEHOLDER_REGION} = Old World", WorldRegion.OldWorld, _regionList3Regions },
                    { $"|Tab {PLACEHOLDER_REGION} = The New World", WorldRegion.NewWorld, _regionList2Regions },
                    { $"|Tab {PLACEHOLDER_REGION} = The New World", WorldRegion.NewWorld, _regionList3Regions },
                    { $"|Tab {PLACEHOLDER_REGION} = The Old World", WorldRegion.OldWorld, _regionList2Regions },
                    { $"|Tab {PLACEHOLDER_REGION} = The Old World", WorldRegion.OldWorld, _regionList3Regions },
                    { $"|Tab {PLACEHOLDER_REGION}     =     Arctic   ", WorldRegion.Arctic, _regionList2Regions },
                    { $"|Tab {PLACEHOLDER_REGION}     =     Arctic   ", WorldRegion.Arctic, _regionList3Regions },
                    { $"|Tab {PLACEHOLDER_REGION} = Arctic}}", WorldRegion.Arctic, _regionList2Regions },
                    { $"|Tab {PLACEHOLDER_REGION} = Arctic}}", WorldRegion.Arctic, _regionList3Regions },
                    { $"|Tab {PLACEHOLDER_REGION} =   Arctic    }}", WorldRegion.Arctic, _regionList2Regions },
                    { $"|Tab {PLACEHOLDER_REGION} =   Arctic    }}", WorldRegion.Arctic, _regionList3Regions },
                    { $"|Tab {PLACEHOLDER_REGION} = ", WorldRegion.Unknown, _regionList2Regions },
                    { $"|Tab {PLACEHOLDER_REGION} = ", WorldRegion.Unknown, _regionList3Regions },
                };
            }
        }

        public static TheoryData<string, BuildingType, List<string>> BuildingTypeTestData
        {
            get
            {
                return new TheoryData<string, BuildingType, List<string>>
                {
                    { $"|Building Type {PLACEHOLDER_REGION} = Administration", BuildingType.Administration, _regionList2Regions },
                    { $"|Building Type {PLACEHOLDER_REGION} = Administration", BuildingType.Administration, _regionList3Regions },
                    { $"|Building Type {PLACEHOLDER_REGION} = Harbour", BuildingType.Harbour, _regionList2Regions },
                    { $"|Building Type {PLACEHOLDER_REGION} = Harbour", BuildingType.Harbour, _regionList3Regions },
                    { $"|Building Type {PLACEHOLDER_REGION} = Infrastructure", BuildingType.Infrastructure, _regionList2Regions },
                    { $"|Building Type {PLACEHOLDER_REGION} = Infrastructure", BuildingType.Infrastructure, _regionList3Regions },
                    { $"|Building Type {PLACEHOLDER_REGION} = Institution", BuildingType.Institution, _regionList2Regions },
                    { $"|Building Type {PLACEHOLDER_REGION} = Institution", BuildingType.Institution, _regionList3Regions },
                    { $"|Building Type {PLACEHOLDER_REGION} = Monument", BuildingType.Monument, _regionList2Regions },
                    { $"|Building Type {PLACEHOLDER_REGION} = Monument", BuildingType.Monument, _regionList3Regions },
                    { $"|Building Type {PLACEHOLDER_REGION} = Ornament", BuildingType.Ornament, _regionList2Regions },
                    { $"|Building Type {PLACEHOLDER_REGION} = Ornament", BuildingType.Ornament, _regionList3Regions },
                    { $"|Building Type {PLACEHOLDER_REGION} = Production", BuildingType.Production, _regionList2Regions },
                    { $"|Building Type {PLACEHOLDER_REGION} = Production", BuildingType.Production, _regionList3Regions },
                    { $"|Building Type {PLACEHOLDER_REGION} = PublicService", BuildingType.PublicService, _regionList2Regions },
                    { $"|Building Type {PLACEHOLDER_REGION} = PublicService", BuildingType.PublicService, _regionList3Regions },
                    { $"|Building Type {PLACEHOLDER_REGION} = Residence", BuildingType.Residence, _regionList2Regions },
                    { $"|Building Type {PLACEHOLDER_REGION} = Residence", BuildingType.Residence, _regionList3Regions },
                    { $"|Building Type {PLACEHOLDER_REGION} = Street", BuildingType.Street, _regionList2Regions },
                    { $"|Building Type {PLACEHOLDER_REGION} = Street", BuildingType.Street, _regionList3Regions },
                    { $"|Building Type {PLACEHOLDER_REGION} = dummy", BuildingType.Unknown, _regionList2Regions },
                    { $"|Building Type {PLACEHOLDER_REGION} = dummy", BuildingType.Unknown, _regionList3Regions },
                    { $"|Building Type {PLACEHOLDER_REGION} = ", BuildingType.Unknown, _regionList2Regions },
                    { $"|Building Type {PLACEHOLDER_REGION} = ", BuildingType.Unknown, _regionList3Regions },
                };
            }
        }

        public static TheoryData<string, MemberDataSerializer<Size>, List<string>> BuildingSizeTestData
        {
            get
            {
                return new TheoryData<string, MemberDataSerializer<Size>, List<string>>
                {
                    { $"|Building Size {PLACEHOLDER_REGION} = 3x3", new MemberDataSerializer<Size>(new Size(3, 3)), _regionList2Regions },
                    { $"|Building Size {PLACEHOLDER_REGION} = 3x3", new MemberDataSerializer<Size>(new Size(3, 3)), _regionList3Regions },
                    { $"|Building Size {PLACEHOLDER_REGION} = 1x3", new MemberDataSerializer<Size>(new Size(1, 3)), _regionList2Regions },
                    { $"|Building Size {PLACEHOLDER_REGION} = 1x3", new MemberDataSerializer<Size>(new Size(1, 3)), _regionList3Regions },
                    { $"|Building Size {PLACEHOLDER_REGION} = 1x10", new MemberDataSerializer<Size>(new Size(1, 10)), _regionList2Regions },
                    { $"|Building Size {PLACEHOLDER_REGION} = 1x10", new MemberDataSerializer<Size>(new Size(1, 10)), _regionList3Regions },
                    { $"|Building Size {PLACEHOLDER_REGION} = 10x1", new MemberDataSerializer<Size>(new Size(10, 1)), _regionList2Regions },
                    { $"|Building Size {PLACEHOLDER_REGION} = 10x1", new MemberDataSerializer<Size>(new Size(10, 1)), _regionList3Regions },
                    { $"|Building Size {PLACEHOLDER_REGION} = 18x22", new MemberDataSerializer<Size>(new Size(18, 22)), _regionList2Regions },
                    { $"|Building Size {PLACEHOLDER_REGION} = 18x22", new MemberDataSerializer<Size>(new Size(18, 22)), _regionList3Regions },
                    { $"|Building Size {PLACEHOLDER_REGION}     =      1   x   10  ", new MemberDataSerializer<Size>(new Size(1, 10)), _regionList2Regions },
                    { $"|Building Size {PLACEHOLDER_REGION}     =      1   x   10  ", new MemberDataSerializer<Size>(new Size(1, 10)), _regionList3Regions },
                    { $"|Building Size {PLACEHOLDER_REGION} = 4x5}}}}", new MemberDataSerializer<Size>(new Size(4, 5)), _regionList2Regions },
                    { $"|Building Size {PLACEHOLDER_REGION} = 4x5}}}}", new MemberDataSerializer<Size>(new Size(4, 5)), _regionList3Regions },
                    { $"|Building Size {PLACEHOLDER_REGION} = 4x5    }}}}", new MemberDataSerializer<Size>(new Size(4, 5)), _regionList2Regions },
                    { $"|Building Size {PLACEHOLDER_REGION} = 4x5    }}}}", new MemberDataSerializer<Size>(new Size(4, 5)), _regionList3Regions },
                    { $"|Building Size {PLACEHOLDER_REGION} = 5x11 (5x16 in water)", new MemberDataSerializer<Size>(new Size(5, 11)), _regionList2Regions },
                    { $"|Building Size {PLACEHOLDER_REGION} = 5x11 (5x16 in water)", new MemberDataSerializer<Size>(new Size(5, 11)), _regionList3Regions },
                    { $"|Building Size {PLACEHOLDER_REGION} = 5x8 (5x13)", new MemberDataSerializer<Size>(new Size(5, 8)), _regionList2Regions },
                    { $"|Building Size {PLACEHOLDER_REGION} = 5x8 (5x13)", new MemberDataSerializer<Size>(new Size(5, 8)), _regionList3Regions },
                    { $"|Building Size {PLACEHOLDER_REGION}  = 5x7 (partially submerged)", new MemberDataSerializer<Size>(new Size(5, 7)), _regionList2Regions },
                    { $"|Building Size {PLACEHOLDER_REGION}  = 5x7 (partially submerged)", new MemberDataSerializer<Size>(new Size(5, 7)), _regionList3Regions },
                };
            }
        }

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
                    { "|Building Icon = Furs.png}}", "Furs.png", _regionList2Regions },
                    { "|Building Icon = Furs.png}}", "Furs.png", _regionList3Regions},
                    { "|Building Icon = Furs.png    }}", "Furs.png", _regionList2Regions},
                    { "|Building Icon = Furs.png    }}", "Furs.png", _regionList3Regions },
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

        public static TheoryData<string, int, double, List<string>> UnlockInfoAmountTestData
        {
            get
            {
                return new TheoryData<string, int, double, List<string>>
                {
                    { $"|Unlock Condition 1 Amount {PLACEHOLDER_REGION} = 42", 1, 42d, _regionList2Regions },
                    { $"|Unlock Condition 1 Amount {PLACEHOLDER_REGION} = 42", 1, 42d, _regionList3Regions },
                    { $"|Unlock Condition 1 Amount {PLACEHOLDER_REGION} = -42", 1, -42d, _regionList2Regions },
                    { $"|Unlock Condition 1 Amount {PLACEHOLDER_REGION} = -42", 1, -42d, _regionList3Regions },
                    { $"|Unlock Condition 1 Amount {PLACEHOLDER_REGION} = 42,21", 1, 42.21d, _regionList2Regions },
                    { $"|Unlock Condition 1 Amount {PLACEHOLDER_REGION} = 42,21", 1, 42.21d, _regionList3Regions },
                    { $"|Unlock Condition 1 Amount {PLACEHOLDER_REGION} = -42,21", 1, -42.21d, _regionList2Regions },
                    { $"|Unlock Condition 1 Amount {PLACEHOLDER_REGION} = -42,21", 1, -42.21d, _regionList3Regions },
                    { $"|Unlock Condition   1   Amount {PLACEHOLDER_REGION}   =   42", 1, 42d, _regionList2Regions },
                    { $"|Unlock Condition   1   Amount {PLACEHOLDER_REGION}   =   42", 1, 42d, _regionList3Regions },
                    { $"|Unlock Condition 1 Amount {PLACEHOLDER_REGION} = 42}}", 1, 42d, _regionList2Regions },
                    { $"|Unlock Condition 1 Amount {PLACEHOLDER_REGION} = 42}}", 1, 42d, _regionList3Regions },
                    { $"|Unlock Condition    1    Amount {PLACEHOLDER_REGION}    =     42   }}", 1, 42d, _regionList2Regions },
                    { $"|Unlock Condition    1    Amount {PLACEHOLDER_REGION}    =     42   }}", 1, 42d, _regionList3Regions },
                    { $"|Unlock Condition 4 Amount {PLACEHOLDER_REGION} = 42", 4, 42d, _regionList2Regions },
                    { $"|Unlock Condition 4 Amount {PLACEHOLDER_REGION} = 42", 4, 42d, _regionList3Regions },
                };
            }
        }

        public static TheoryData<string, int, string, List<string>> UnlockInfoTypeTestData
        {
            get
            {
                return new TheoryData<string, int, string, List<string>>
                {
                    { $"|Unlock Condition 1 Type {PLACEHOLDER_REGION} = Technicians", 1, "Technicians", _regionList2Regions },
                    { $"|Unlock Condition 1 Type {PLACEHOLDER_REGION} = Technicians", 1, "Technicians", _regionList3Regions },
                    { $"|Unlock Condition 1 Type {PLACEHOLDER_REGION} = Engineers", 1, "Engineers", _regionList2Regions },
                    { $"|Unlock Condition 1 Type {PLACEHOLDER_REGION} = Engineers", 1, "Engineers", _regionList3Regions },
                    { $"|Unlock Condition 1 Type {PLACEHOLDER_REGION} = Obreros", 1, "Obreros", _regionList2Regions },
                    { $"|Unlock Condition 1 Type {PLACEHOLDER_REGION} = Obreros", 1, "Obreros", _regionList3Regions },
                    { $"|Unlock Condition 1 Type {PLACEHOLDER_REGION} = Investors", 1, "Investors", _regionList2Regions },
                    { $"|Unlock Condition 1 Type {PLACEHOLDER_REGION} = Investors", 1, "Investors", _regionList3Regions },
                    { $"|Unlock Condition 1 Type {PLACEHOLDER_REGION} = Jornaleros", 1, "Jornaleros", _regionList2Regions },
                    { $"|Unlock Condition 1 Type {PLACEHOLDER_REGION} = Jornaleros", 1, "Jornaleros", _regionList3Regions },
                    { $"|Unlock Condition 1 Type {PLACEHOLDER_REGION} = Workers", 1, "Workers", _regionList2Regions },
                    { $"|Unlock Condition 1 Type {PLACEHOLDER_REGION} = Workers", 1, "Workers", _regionList3Regions },
                    { $"|Unlock Condition 1 Type {PLACEHOLDER_REGION} = dummy with spaces", 1, "dummy with spaces", _regionList2Regions },
                    { $"|Unlock Condition 1 Type {PLACEHOLDER_REGION} = dummy with spaces", 1, "dummy with spaces", _regionList3Regions },
                    { $"|Unlock Condition 4 Type {PLACEHOLDER_REGION} = Technicians", 4, "Technicians", _regionList2Regions },
                    { $"|Unlock Condition 4 Type {PLACEHOLDER_REGION} = Technicians", 4, "Technicians", _regionList3Regions },
                    { $"|Unlock Condition    1   Type {PLACEHOLDER_REGION}    =    Technicians   ", 1, "Technicians", _regionList2Regions },
                    { $"|Unlock Condition    1   Type {PLACEHOLDER_REGION}    =    Technicians   ", 1, "Technicians", _regionList3Regions },
                    { $"|Unlock Condition 1 Type {PLACEHOLDER_REGION} = Technicians}}", 1, "Technicians", _regionList2Regions },
                    { $"|Unlock Condition 1 Type {PLACEHOLDER_REGION} = Technicians}}", 1, "Technicians", _regionList3Regions },
                    { $"|Unlock Condition    1    Type {PLACEHOLDER_REGION}    =    Technicians   }}", 1, "Technicians", _regionList2Regions },
                    { $"|Unlock Condition    1    Type {PLACEHOLDER_REGION}    =    Technicians   }}", 1, "Technicians", _regionList3Regions },
                };
            }
        }

        public static TheoryData<string, int, double, List<string>> SupplyInfoAmountTestData
        {
            get
            {
                return new TheoryData<string, int, double, List<string>>
                {
                    { $"|Supplies 1 Amount {PLACEHOLDER_REGION} = 42", 1, 42d, _regionList2Regions },
                    { $"|Supplies 1 Amount {PLACEHOLDER_REGION} = 42", 1, 42d, _regionList3Regions },
                    { $"|Supplies 1 Amount {PLACEHOLDER_REGION} = -42", 1, -42d, _regionList2Regions },
                    { $"|Supplies 1 Amount {PLACEHOLDER_REGION} = -42", 1, -42d, _regionList3Regions },
                    { $"|Supplies 1 Amount {PLACEHOLDER_REGION} = 42,21", 1, 42.21d, _regionList2Regions },
                    { $"|Supplies 1 Amount {PLACEHOLDER_REGION} = 42,21", 1, 42.21d, _regionList3Regions },
                    { $"|Supplies 1 Amount {PLACEHOLDER_REGION} = -42,21", 1, -42.21d, _regionList2Regions },
                    { $"|Supplies 1 Amount {PLACEHOLDER_REGION} = -42,21", 1, -42.21d, _regionList3Regions },
                    { $"|Supplies   1   Amount {PLACEHOLDER_REGION}   =   42", 1, 42d, _regionList2Regions },
                    { $"|Supplies   1   Amount {PLACEHOLDER_REGION}   =   42", 1, 42d, _regionList3Regions },
                    { $"|Supplies 1 Amount {PLACEHOLDER_REGION} = 42}}", 1, 42d, _regionList2Regions },
                    { $"|Supplies 1 Amount {PLACEHOLDER_REGION} = 42}}", 1, 42d, _regionList3Regions },
                    { $"|Supplies    1    Amount {PLACEHOLDER_REGION}    =     42   }}", 1, 42d, _regionList2Regions },
                    { $"|Supplies    1    Amount {PLACEHOLDER_REGION}    =     42   }}", 1, 42d, _regionList3Regions },
                    { $"|Supplies 4 Amount {PLACEHOLDER_REGION} = 42", 4, 42d, _regionList2Regions },
                    { $"|Supplies 4 Amount {PLACEHOLDER_REGION} = 42", 4, 42d, _regionList3Regions },
                };
            }
        }

        public static TheoryData<string, int, double, List<string>> SupplyInfoAmountElectricityTestData
        {
            get
            {
                return new TheoryData<string, int, double, List<string>>
                {
                    { $"|Supplies 1 Amount Electricity {PLACEHOLDER_REGION} = 42", 1, 42d, _regionList2Regions },
                    { $"|Supplies 1 Amount Electricity {PLACEHOLDER_REGION} = 42", 1, 42d, _regionList3Regions },
                    { $"|Supplies 1 Amount Electricity {PLACEHOLDER_REGION} = -42", 1, -42d, _regionList2Regions },
                    { $"|Supplies 1 Amount Electricity {PLACEHOLDER_REGION} = -42", 1, -42d, _regionList3Regions },
                    { $"|Supplies 1 Amount Electricity {PLACEHOLDER_REGION} = 42,21", 1, 42.21d, _regionList2Regions },
                    { $"|Supplies 1 Amount Electricity {PLACEHOLDER_REGION} = 42,21", 1, 42.21d, _regionList3Regions },
                    { $"|Supplies 1 Amount Electricity {PLACEHOLDER_REGION} = -42,21", 1, -42.21d, _regionList2Regions },
                    { $"|Supplies 1 Amount Electricity {PLACEHOLDER_REGION} = -42,21", 1, -42.21d, _regionList3Regions },
                    { $"|Supplies   1   Amount Electricity {PLACEHOLDER_REGION}   =   42", 1, 42d, _regionList2Regions },
                    { $"|Supplies   1   Amount Electricity {PLACEHOLDER_REGION}   =   42", 1, 42d, _regionList3Regions },
                    { $"|Supplies 1 Amount Electricity {PLACEHOLDER_REGION} = 42}}", 1, 42d, _regionList2Regions },
                    { $"|Supplies 1 Amount Electricity {PLACEHOLDER_REGION} = 42}}", 1, 42d, _regionList3Regions },
                    { $"|Supplies    1    Amount Electricity {PLACEHOLDER_REGION}    =     42   }}", 1, 42d, _regionList2Regions },
                    { $"|Supplies    1    Amount Electricity {PLACEHOLDER_REGION}    =     42   }}", 1, 42d, _regionList3Regions },
                    { $"|Supplies 4 Amount Electricity {PLACEHOLDER_REGION} = 42", 4, 42d, _regionList2Regions },
                    { $"|Supplies 4 Amount Electricity {PLACEHOLDER_REGION} = 42", 4, 42d, _regionList3Regions },
                };
            }
        }

        public static TheoryData<string, int, string, List<string>> SupplyInfoTypeTestData
        {
            get
            {
                return new TheoryData<string, int, string, List<string>>
                {
                    { $"|Supplies 1 Type {PLACEHOLDER_REGION} = Technicians", 1, "Technicians", _regionList2Regions },
                    { $"|Supplies 1 Type {PLACEHOLDER_REGION} = Technicians", 1, "Technicians", _regionList3Regions },
                    { $"|Supplies 1 Type {PLACEHOLDER_REGION} = Engineers", 1, "Engineers", _regionList2Regions },
                    { $"|Supplies 1 Type {PLACEHOLDER_REGION} = Engineers", 1, "Engineers", _regionList3Regions },
                    { $"|Supplies 1 Type {PLACEHOLDER_REGION} = Obreros", 1, "Obreros", _regionList2Regions },
                    { $"|Supplies 1 Type {PLACEHOLDER_REGION} = Obreros", 1, "Obreros", _regionList3Regions },
                    { $"|Supplies 1 Type {PLACEHOLDER_REGION} = Investors", 1, "Investors", _regionList2Regions },
                    { $"|Supplies 1 Type {PLACEHOLDER_REGION} = Investors", 1, "Investors", _regionList3Regions },
                    { $"|Supplies 1 Type {PLACEHOLDER_REGION} = Jornaleros", 1, "Jornaleros", _regionList2Regions },
                    { $"|Supplies 1 Type {PLACEHOLDER_REGION} = Jornaleros", 1, "Jornaleros", _regionList3Regions },
                    { $"|Supplies 1 Type {PLACEHOLDER_REGION} = Workers", 1, "Workers", _regionList2Regions },
                    { $"|Supplies 1 Type {PLACEHOLDER_REGION} = Workers", 1, "Workers", _regionList3Regions },
                    { $"|Supplies 1 Type {PLACEHOLDER_REGION} = dummy with spaces", 1, "dummy with spaces", _regionList2Regions },
                    { $"|Supplies 1 Type {PLACEHOLDER_REGION} = dummy with spaces", 1, "dummy with spaces", _regionList3Regions },
                    { $"|Supplies 4 Type {PLACEHOLDER_REGION} = Technicians", 4, "Technicians", _regionList2Regions },
                    { $"|Supplies 4 Type {PLACEHOLDER_REGION} = Technicians", 4, "Technicians", _regionList3Regions },
                    { $"|Supplies    1   Type {PLACEHOLDER_REGION}    =    Technicians   ", 1, "Technicians", _regionList2Regions },
                    { $"|Supplies    1   Type {PLACEHOLDER_REGION}    =    Technicians   ", 1, "Technicians", _regionList3Regions },
                    { $"|Supplies 1 Type {PLACEHOLDER_REGION} = Technicians}}", 1, "Technicians", _regionList2Regions },
                    { $"|Supplies 1 Type {PLACEHOLDER_REGION} = Technicians}}", 1, "Technicians", _regionList3Regions },
                    { $"|Supplies    1    Type {PLACEHOLDER_REGION}    =    Technicians   }}", 1, "Technicians", _regionList2Regions },
                    { $"|Supplies    1    Type {PLACEHOLDER_REGION}    =    Technicians   }}", 1, "Technicians", _regionList3Regions },
                };
            }
        }

        #endregion

        [Theory]
        [InlineData(null)]
        [InlineData(" ")]
        [InlineData("")]
        public void GetInfobox_2Regions_WikiTextIsNullOrWhiteSpace_ShouldReturnNull(string input)
        {
            // Arrange
            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(input, _regionList2Regions);

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(" ")]
        [InlineData("")]
        public void GetInfobox_3Regions_WikiTextIsNullOrWhiteSpace_ShouldReturnNull(string input)
        {
            // Arrange
            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(input, _regionList3Regions);

            // Assert
            Assert.Null(result);
        }

        #region BuildingName tests        

        [Theory]
        [MemberData(nameof(BuildingNameTestData))]
        public void GetInfobox_WikiTextContainsTitle_ShouldReturnCorrectValue(string input, string expectedName, List<string> possibleRegions)
        {
            // Arrange
            //_output.WriteLine($"{nameof(input)}: {input}");

            var parser = GetParser();
            var inputToParse = new StringBuilder();
            foreach (var curRegion in possibleRegions)
            {
                inputToParse.AppendLine(input.Replace(PLACEHOLDER_REGION, curRegion));
            }

            // Act
            var result = parser.GetInfobox(inputToParse.ToString(), possibleRegions);

            // Assert
            Assert.Equal(possibleRegions.Count, result.Count);
            foreach (var curResult in result)
            {
                Assert.Equal(expectedName, curResult.Name);
            }
        }

        #endregion

        #region RegionName tests

        [Theory]
        [MemberData(nameof(RegionNameTestData))]
        public void GetInfobox_WikiTextContainsRegion_ShouldReturnCorrectValue(string input, WorldRegion expectedRegion, List<string> possibleRegions)
        {
            // Arrange
            //_output.WriteLine($"{nameof(input)}: {input}");

            var parser = GetParser();
            var inputToParse = new StringBuilder();
            foreach (var curRegion in possibleRegions)
            {
                inputToParse.AppendLine(input.Replace(PLACEHOLDER_REGION, curRegion));
            }

            // Act
            var result = parser.GetInfobox(inputToParse.ToString(), possibleRegions);

            // Assert
            Assert.Equal(possibleRegions.Count, result.Count);
            foreach (var curResult in result)
            {
                Assert.Equal(expectedRegion, curResult.Region);
            }
        }

        #endregion

        #region BuildingType tests

        [Theory]
        [MemberData(nameof(BuildingTypeTestData))]
        public void GetInfobox_WikiTextContainsBuildingType_ShouldReturnCorrectValue(string input, BuildingType expectedType, List<string> possibleRegions)
        {
            // Arrange
            //_output.WriteLine($"{nameof(input)}: {input}");

            var parser = GetParser();
            var inputToParse = new StringBuilder();
            foreach (var curRegion in possibleRegions)
            {
                inputToParse.AppendLine(input.Replace(PLACEHOLDER_REGION, curRegion));
            }

            // Act
            var result = parser.GetInfobox(inputToParse.ToString(), possibleRegions);

            // Assert
            Assert.Equal(possibleRegions.Count, result.Count);
            foreach (var curResult in result)
            {
                Assert.Equal(expectedType, curResult.Type);
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

        #region BuildingSize tests

        //[Theory]
        //[InlineData("dummy")]
        //[InlineData("|Building Size = ")]
        //public void GetInfobox_WikiTextContainsNoBuildingSize_ShouldReturnEmptySize(string input, List<string> possibleRegions)
        //{
        //    // Arrange
        //    var parser = GetParser();

        //    // Act
        //    var result = parser.GetInfobox(input);

        //    // Assert
        //    Assert.Equal(Size.Empty, result[0].BuildingSize);
        //}

        //[Theory]
        //[InlineData("|Building Size = ?x?")]
        //[InlineData("|Building Size = ? x ?")]
        //[InlineData("|Building Size = ? x?")]
        //[InlineData("|Building Size = ?x ?")]
        //[InlineData("|Building Size = 3x")]
        //[InlineData("|Building Size = dummyxdummy")]
        //public void GetInfobox_WikiTextContainsUnknownBuildingSize_ShouldReturnEmptySize(string input, List<string> possibleRegions)
        //{
        //    // Arrange
        //    var parser = GetParser();

        //    // Act
        //    var result = parser.GetInfobox(input);

        //    // Assert
        //    Assert.Equal(Size.Empty, result[0].BuildingSize);
        //}

        [Theory]
        [MemberData(nameof(BuildingSizeTestData))]
        public void GetInfobox_WikiTextContainsBuildingSize_ShouldReturnCorrectValue(string input, MemberDataSerializer<Size> expectedSize, List<string> possibleRegions)
        {
            // Arrange
            //_output.WriteLine($"{nameof(input)}: {input}");

            var parser = GetParser();
            var inputToParse = new StringBuilder();
            foreach (var curRegion in possibleRegions)
            {
                inputToParse.AppendLine(input.Replace(PLACEHOLDER_REGION, curRegion));
            }

            // Act
            var result = parser.GetInfobox(inputToParse.ToString(), possibleRegions);

            // Assert
            Assert.Equal(possibleRegions.Count, result.Count);
            foreach (var curResult in result)
            {
                Assert.Equal(expectedSize.Object, curResult.BuildingSize);
            }
        }

        #endregion

        #region UnlockInfo tests

        [Theory]
        [MemberData(nameof(UnlockInfoAmountTestData))]
        public void GetInfobox_WikiTextContainsUnlockAmount_ShouldReturnCorrectValue(string input, int expectedOrder, double expectedAmount, List<string> possibleRegions)
        {
            // Arrange
            var parser = GetParser();
            var inputToParse = new StringBuilder();
            foreach (var curRegion in possibleRegions)
            {
                inputToParse.AppendLine(input.Replace(PLACEHOLDER_REGION, curRegion));
            }

            // Act
            var result = parser.GetInfobox(inputToParse.ToString(), possibleRegions);

            // Assert
            Assert.Equal(possibleRegions.Count, result.Count);
            foreach (var curResult in result)
            {
                Assert.Single(curResult.UnlockInfos.UnlockConditions);
                Assert.Equal(expectedAmount, curResult.UnlockInfos.UnlockConditions[0].Amount);
                Assert.Equal(expectedOrder, curResult.UnlockInfos.UnlockConditions[0].Order);
                Assert.Null(curResult.UnlockInfos.UnlockConditions[0].Type);
            }

        }

        [Theory]
        [MemberData(nameof(UnlockInfoTypeTestData))]
        public void GetInfobox_WikiTextContainsUnlockType_ShouldReturnCorrectValue(string input, int expectedOrder, string expectedType, List<string> possibleRegions)
        {
            // Arrange
            var parser = GetParser();
            var inputToParse = new StringBuilder();
            foreach (var curRegion in possibleRegions)
            {
                inputToParse.AppendLine(input.Replace(PLACEHOLDER_REGION, curRegion));
            }

            // Act
            var result = parser.GetInfobox(inputToParse.ToString(), possibleRegions);

            // Assert
            Assert.Equal(possibleRegions.Count, result.Count);
            foreach (var curResult in result)
            {
                Assert.Single(curResult.UnlockInfos.UnlockConditions);
                Assert.Equal(0, curResult.UnlockInfos.UnlockConditions[0].Amount);
                Assert.Equal(expectedOrder, curResult.UnlockInfos.UnlockConditions[0].Order);
                Assert.Equal(expectedType, curResult.UnlockInfos.UnlockConditions[0].Type);
            }

        }

        #endregion

        #region SupplyInfo tests

        [Theory]
        [MemberData(nameof(SupplyInfoAmountTestData))]
        public void GetInfobox_WikiTextContainsSupplyAmount_ShouldReturnCorrectValue(string input, int expectedOrder, double expectedAmount, List<string> possibleRegions)
        {
            // Arrange
            var parser = GetParser();
            var inputToParse = new StringBuilder();
            foreach (var curRegion in possibleRegions)
            {
                inputToParse.AppendLine(input.Replace(PLACEHOLDER_REGION, curRegion));
            }

            // Act
            var result = parser.GetInfobox(inputToParse.ToString(), possibleRegions);

            // Assert
            Assert.Equal(possibleRegions.Count, result.Count);
            foreach (var curResult in result)
            {
                Assert.Single(curResult.SupplyInfos.SupplyEntries);
                Assert.Equal(0, curResult.SupplyInfos.SupplyEntries[0].AmountElectricity);
                Assert.Equal(expectedAmount, curResult.SupplyInfos.SupplyEntries[0].Amount);
                Assert.Equal(expectedOrder, curResult.SupplyInfos.SupplyEntries[0].Order);
                Assert.Null(curResult.SupplyInfos.SupplyEntries[0].Type);
            }
        }

        [Theory]
        [MemberData(nameof(SupplyInfoAmountElectricityTestData))]
        public void GetInfobox_WikiTextContainsSupplyAmountElectricity_ShouldReturnCorrectValue(string input, int expectedOrder, double expectedAmount, List<string> possibleRegions)
        {
            // Arrange
            var parser = GetParser();
            var inputToParse = new StringBuilder();
            foreach (var curRegion in possibleRegions)
            {
                inputToParse.AppendLine(input.Replace(PLACEHOLDER_REGION, curRegion));
            }

            // Act
            var result = parser.GetInfobox(inputToParse.ToString(), possibleRegions);

            // Assert
            Assert.Equal(possibleRegions.Count, result.Count);
            foreach (var curResult in result)
            {
                Assert.Single(curResult.SupplyInfos.SupplyEntries);
                Assert.Equal(0, curResult.SupplyInfos.SupplyEntries[0].Amount);
                Assert.Equal(expectedAmount, curResult.SupplyInfos.SupplyEntries[0].AmountElectricity);
                Assert.Equal(expectedOrder, curResult.SupplyInfos.SupplyEntries[0].Order);
                Assert.Null(curResult.SupplyInfos.SupplyEntries[0].Type);
            }
        }

        [Theory]
        [MemberData(nameof(SupplyInfoTypeTestData))]
        public void GetInfobox_WikiTextContainsSupplyType_ShouldReturnCorrectValue(string input, int expectedOrder, string expectedType, List<string> possibleRegions)
        {
            // Arrange
            var parser = GetParser();
            var inputToParse = new StringBuilder();
            foreach (var curRegion in possibleRegions)
            {
                inputToParse.AppendLine(input.Replace(PLACEHOLDER_REGION, curRegion));
            }

            // Act
            var result = parser.GetInfobox(inputToParse.ToString(), possibleRegions);

            // Assert
            Assert.Equal(possibleRegions.Count, result.Count);
            foreach (var curResult in result)
            {
                Assert.Single(curResult.SupplyInfos.SupplyEntries);
                Assert.Equal(0, curResult.SupplyInfos.SupplyEntries[0].Amount);
                Assert.Equal(0, curResult.SupplyInfos.SupplyEntries[0].AmountElectricity);
                Assert.Equal(expectedType, curResult.SupplyInfos.SupplyEntries[0].Type);
                Assert.Equal(expectedOrder, curResult.SupplyInfos.SupplyEntries[0].Order);
            }
        }

        #endregion
    }
}
