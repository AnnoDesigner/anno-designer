using System;
using System.Collections.Generic;
using System.Drawing;
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
    public class ParserOldAndNewWorldTests
    {
        private static readonly ICommons _mockedCommons;
        private readonly ITestOutputHelper _output;
        private static readonly ISpecialBuildingNameHelper _mockedSpecialBuildingNameHelper;
        private static readonly IRegionHelper _mockedRegionHelper;

        private static readonly string testDataPoliceStation;
        private static readonly string testDataBrickFactory;
        private static readonly string testDataHospital;
        private static readonly string testDataMarketplace;
        private static readonly string testDataSmallWareHouse;
        private static readonly string testDataEmpty_BothWorlds;

        #region ctor

        static ParserOldAndNewWorldTests()
        {
            _mockedCommons = Commons.Instance;
            _mockedSpecialBuildingNameHelper = new SpecialBuildingNameHelper();
            _mockedRegionHelper = new RegionHelper();

            string basePath = AppDomain.CurrentDomain.BaseDirectory;

            testDataPoliceStation = File.ReadAllText(Path.Combine(basePath, "Testdata", "Police_Station.infobox"));
            testDataBrickFactory = File.ReadAllText(Path.Combine(basePath, "Testdata", "Brick_Factory.infobox"));
            testDataHospital = File.ReadAllText(Path.Combine(basePath, "Testdata", "Hospital.infobox"));
            testDataMarketplace = File.ReadAllText(Path.Combine(basePath, "Testdata", "Marketplace.infobox"));
            testDataSmallWareHouse = File.ReadAllText(Path.Combine(basePath, "Testdata", "Small_Warehouse.infobox"));
            testDataEmpty_BothWorlds = File.ReadAllText(Path.Combine(basePath, "Testdata", "empty_BothWorlds.infobox"));
        }

        public ParserOldAndNewWorldTests(ITestOutputHelper testOutputHelperToUse)
        {
            _output = testOutputHelperToUse;
        }

        #endregion

        private IParser GetParser(ICommons commonsToUse = null,
           ISpecialBuildingNameHelper specialBuildingNameHelperToUse = null,
           IRegionHelper regionHelperToUse = null)
        {
            return new ParserOldAndNewWorld(commonsToUse ?? _mockedCommons,
                specialBuildingNameHelperToUse ?? _mockedSpecialBuildingNameHelper,
                regionHelperToUse ?? _mockedRegionHelper);
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

        public static TheoryData<string, int, double, WorldRegion> UnlockInfoAmountTestData
        {
            get
            {
                return new TheoryData<string, int, double, WorldRegion>
                {
                    { "|Unlock Condition 1 Amount (OW) = 42", 1, 42d, WorldRegion.OldWorld },
                    { "|Unlock Condition 1 Amount (OW) = -42", 1, -42d, WorldRegion.OldWorld },
                    { "|Unlock Condition 1 Amount (OW) = 42,21", 1, 42.21d, WorldRegion.OldWorld },
                    { "|Unlock Condition 1 Amount (OW) = -42,21", 1, -42.21d, WorldRegion.OldWorld },
                    { "|Unlock Condition   1   Amount (OW)   =   42", 1, 42d, WorldRegion.OldWorld },
                    { "|Unlock Condition 1 Amount (OW) = 42}}", 1, 42d, WorldRegion.OldWorld },
                    { "|Unlock Condition    1    Amount (OW)    =     42   }}", 1, 42d, WorldRegion.OldWorld },
                    { "|Unlock Condition 4 Amount (OW) = 42", 4, 42d, WorldRegion.OldWorld },
                    { "|Unlock Condition 1 Amount (NW) = 42", 1, 42d, WorldRegion.NewWorld },
                    { "|Unlock Condition 1 Amount (NW) = -42", 1, -42d, WorldRegion.NewWorld },
                    { "|Unlock Condition 1 Amount (NW) = 42,21", 1, 42.21d, WorldRegion.NewWorld },
                    { "|Unlock Condition 1 Amount (NW) = -42,21", 1, -42.21d, WorldRegion.NewWorld },
                    { "|Unlock Condition   1   Amount (NW)   =   42", 1, 42d, WorldRegion.NewWorld },
                    { "|Unlock Condition 1 Amount (NW) = 42}}", 1, 42d, WorldRegion.NewWorld },
                    { "|Unlock Condition    1    Amount (NW)    =     42   }}", 1, 42d, WorldRegion.NewWorld },
                    { "|Unlock Condition 4 Amount (NW) = 42", 4, 42d, WorldRegion.NewWorld },
                };
            }
        }

        public static TheoryData<string, int, string, WorldRegion> UnlockInfoTypeTestData
        {
            get
            {
                return new TheoryData<string, int, string, WorldRegion>
                {
                    { "|Unlock Condition 1 Type (OW) = Technicians", 1, "Technicians", WorldRegion.OldWorld },
                    { "|Unlock Condition 1 Type (OW) = Engineers", 1, "Engineers", WorldRegion.OldWorld },
                    { "|Unlock Condition 1 Type (OW) = Obreros", 1, "Obreros", WorldRegion.OldWorld },
                    { "|Unlock Condition 1 Type (OW) = Investors", 1, "Investors", WorldRegion.OldWorld },
                    { "|Unlock Condition 1 Type (OW) = Jornaleros", 1, "Jornaleros", WorldRegion.OldWorld },
                    { "|Unlock Condition 1 Type (OW) = Workers", 1, "Workers", WorldRegion.OldWorld },
                    { "|Unlock Condition 1 Type (OW) = dummy with spaces", 1, "dummy with spaces", WorldRegion.OldWorld },
                    { "|Unlock Condition 4 Type (OW) = Technicians", 4, "Technicians", WorldRegion.OldWorld },
                    { "|Unlock Condition    1   Type (OW)    =    Technicians   ", 1, "Technicians", WorldRegion.OldWorld },
                    { "|Unlock Condition 1 Type (OW) = Technicians}}", 1, "Technicians", WorldRegion.OldWorld },
                    { "|Unlock Condition    1    Type (OW)    =    Technicians   }}", 1, "Technicians", WorldRegion.OldWorld },
                    { "|Unlock Condition 1 Type (NW) = Technicians", 1, "Technicians", WorldRegion.NewWorld },
                    { "|Unlock Condition 1 Type (NW) = Engineers", 1, "Engineers", WorldRegion.NewWorld },
                    { "|Unlock Condition 1 Type (NW) = Obreros", 1, "Obreros", WorldRegion.NewWorld },
                    { "|Unlock Condition 1 Type (NW) = Investors", 1, "Investors", WorldRegion.NewWorld },
                    { "|Unlock Condition 1 Type (NW) = Jornaleros", 1, "Jornaleros", WorldRegion.NewWorld },
                    { "|Unlock Condition 1 Type (NW) = Workers", 1, "Workers", WorldRegion.NewWorld },
                    { "|Unlock Condition 1 Type (NW) = dummy with spaces", 1, "dummy with spaces", WorldRegion.NewWorld },
                    { "|Unlock Condition 4 Type (NW) = Technicians", 4, "Technicians", WorldRegion.NewWorld },
                    { "|Unlock Condition    1   Type (NW)    =    Technicians   ", 1, "Technicians", WorldRegion.NewWorld },
                    { "|Unlock Condition 1 Type (NW) = Technicians}}", 1, "Technicians", WorldRegion.NewWorld },
                    { "|Unlock Condition    1    Type (NW)    =    Technicians   }}", 1, "Technicians", WorldRegion.NewWorld },
                };
            }
        }

        public static TheoryData<string, Size, WorldRegion> BuildingSizeTestData
        {
            get
            {
                return new TheoryData<string, Size, WorldRegion>
                {
                    { "|Building Size (OW) = 3x3", new Size(3, 3), WorldRegion.OldWorld },
                    { "|Building Size (OW) = 1x3", new Size(1, 3), WorldRegion.OldWorld },
                    { "|Building Size (OW) = 1x10", new Size(1, 10), WorldRegion.OldWorld },
                    { "|Building Size (OW) = 10x1", new Size(10, 1), WorldRegion.OldWorld },
                    { "|Building Size (OW) = 18x22", new Size(18, 22), WorldRegion.OldWorld },
                    { "|Building Size (OW)      =      1   x   10  ", new Size(1, 10), WorldRegion.OldWorld },
                    { "|Building Size (OW) = 4 x 5", new Size(4, 5), WorldRegion.OldWorld },
                    { "|Building Size (OW) = 4x5}}", new Size(4, 5), WorldRegion.OldWorld },
                    { "|Building Size (OW) = 4x5   }}", new Size(4, 5), WorldRegion.OldWorld },
                    { "|Building Size (OW) = 5x11 (5x16 in water)", new Size(5, 11), WorldRegion.OldWorld },
                    { "|Building Size (OW) = 5x8 (5x13)", new Size(5, 8), WorldRegion.OldWorld },
                    { "|Building Size (OW)   = 5x7 (partially submerged)", new Size(5, 7), WorldRegion.OldWorld },
                    { "|Building Size (NW) = 3x3", new Size(3, 3), WorldRegion.NewWorld },
                    { "|Building Size (NW) = 1x3", new Size(1, 3), WorldRegion.NewWorld },
                    { "|Building Size (NW) = 1x10", new Size(1, 10), WorldRegion.NewWorld },
                    { "|Building Size (NW) = 10x1", new Size(10, 1), WorldRegion.NewWorld },
                    { "|Building Size (NW) = 18x22", new Size(18, 22), WorldRegion.NewWorld },
                    { "|Building Size (NW)      =      1   x   10  ", new Size(1, 10), WorldRegion.NewWorld },
                    { "|Building Size (NW) = 4 x 5", new Size(4, 5), WorldRegion.NewWorld },
                    { "|Building Size (NW) = 4x5}}", new Size(4, 5), WorldRegion.NewWorld },
                    { "|Building Size (NW) = 4x5   }}", new Size(4, 5), WorldRegion.NewWorld },
                    { "|Building Size (NW) = 5x11 (5x16 in water)", new Size(5, 11), WorldRegion.NewWorld },
                    { "|Building Size (NW) = 5x8 (5x13)", new Size(5, 8), WorldRegion.NewWorld },
                    { "|Building Size (NW)   = 5x7 (partially submerged)", new Size(5, 7), WorldRegion.NewWorld },
                };
            }
        }

        public static TheoryData<string, double, WorldRegion> ConstructionInfoCreditsTestData
        {
            get
            {
                return new TheoryData<string, double, WorldRegion>
                {
                    { "|Credits (OW) = 15000", 15000d, WorldRegion.OldWorld },
                    { "|Credits (NW) = 15000", 15000d, WorldRegion.NewWorld },
                    { "|Credits (OW) = 150", 150d, WorldRegion.OldWorld },
                    { "|Credits (NW) = 150", 150d, WorldRegion.NewWorld },
                    { "|Credits (OW) = 42,21", 42.21, WorldRegion.OldWorld },
                    { "|Credits (NW) = 42,21", 42.21, WorldRegion.NewWorld },
                    { "|Credits (OW) = -42,21", -42.21, WorldRegion.OldWorld },
                    { "|Credits (NW) = -42,21", -42.21, WorldRegion.NewWorld },
                    //{ "|Credits (OW) = 2,500", 2500, WorldRegion.OldWorld },//incorrect numberstyle on wiki
                    //{ "|Credits (NW) = 2,500", 2500, WorldRegion.NewWorld },//incorrect numberstyle on wiki
                    //{ "|Credits (OW) = 4,000", 4000, WorldRegion.OldWorld },//incorrect numberstyle on wiki
                    //{ "|Credits (NW) = 4,000", 4000, WorldRegion.NewWorld },//incorrect numberstyle on wiki
                    //{ "|Credits (OW) = 42.21", 42.21, WorldRegion.OldWorld },//incorrect numberstyle on wiki
                    //{ "|Credits (NW) = 42.21", 42.21, WorldRegion.NewWorld },//incorrect numberstyle on wiki
                    //{ "|Credits (OW) = -42.21", -42.21, WorldRegion.OldWorld },//incorrect numberstyle on wiki
                    //{ "|Credits (NW) = -42.21", -42.21, WorldRegion.NewWorld },//incorrect numberstyle on wiki
                    { "|Credits (OW) = -100", -100d, WorldRegion.OldWorld },
                    { "|Credits (NW) = -100", -100d, WorldRegion.NewWorld },
                    { "|Credits (OW)    =    150   ", 150d, WorldRegion.OldWorld },
                    { "|Credits (NW)    =    150   ", 150d, WorldRegion.NewWorld },
                    { "|Credits (OW) = 150}}", 150d, WorldRegion.OldWorld },
                    { "|Credits (NW) = 150}}", 150d, WorldRegion.NewWorld },
                    { "|Credits (OW) = 42,21   }}", 42.21, WorldRegion.OldWorld },
                    { "|Credits (NW) = 42,21   }}", 42.21, WorldRegion.NewWorld },
                };
            }
        }

        public static TheoryData<string, double, WorldRegion> ConstructionInfoTimberTestData
        {
            get
            {
                return new TheoryData<string, double, WorldRegion>
                {
                    { "|Timber (OW) = 15000", 15000d, WorldRegion.OldWorld },
                    { "|Timber (NW) = 15000", 15000d, WorldRegion.NewWorld },
                    { "|Timber (OW) = 150", 150d, WorldRegion.OldWorld },
                    { "|Timber (NW) = 150", 150d, WorldRegion.NewWorld },
                    { "|Timber (OW) = 42,21", 42.21, WorldRegion.OldWorld },
                    { "|Timber (NW) = 42,21", 42.21, WorldRegion.NewWorld },
                    { "|Timber (OW) = -42,21", -42.21, WorldRegion.OldWorld },
                    { "|Timber (NW) = -42,21", -42.21, WorldRegion.NewWorld },
                    //{ "|Timber (OW) = 42.21", 42.21, WorldRegion.OldWorld },//incorrect numberstyle on wiki
                    //{ "|Timber (NW) = 42.21", 42.21, WorldRegion.NewWorld },//incorrect numberstyle on wiki
                    //{ "|Timber (OW) = -42.21", -42.21, WorldRegion.OldWorld },//incorrect numberstyle on wiki
                    //{ "|Timber (NW) = -42.21", -42.21, WorldRegion.NewWorld },//incorrect numberstyle on wiki
                    { "|Timber (OW) = -100", -100d, WorldRegion.OldWorld },
                    { "|Timber (NW) = -100", -100d, WorldRegion.NewWorld },
                    { "|Timber (OW)    =    150   ", 150d, WorldRegion.OldWorld },
                    { "|Timber (NW)    =    150   ", 150d, WorldRegion.NewWorld },
                    { "|Timber (OW) = 150}}", 150d, WorldRegion.OldWorld },
                    { "|Timber (NW) = 150}}", 150d, WorldRegion.NewWorld },
                    { "|Timber (OW) = 42,21   }}", 42.21, WorldRegion.OldWorld },
                    { "|Timber (NW) = 42,21   }}", 42.21, WorldRegion.NewWorld },
                };
            }
        }

        public static TheoryData<string, double, WorldRegion> ConstructionInfoBricksTestData
        {
            get
            {
                return new TheoryData<string, double, WorldRegion>
                {
                    { "|Bricks (OW) = 15000", 15000d, WorldRegion.OldWorld },
                    { "|Bricks (NW) = 15000", 15000d, WorldRegion.NewWorld },
                    { "|Bricks (OW) = 150", 150d, WorldRegion.OldWorld },
                    { "|Bricks (NW) = 150", 150d, WorldRegion.NewWorld },
                    { "|Bricks (OW) = 42,21", 42.21, WorldRegion.OldWorld },
                    { "|Bricks (NW) = 42,21", 42.21, WorldRegion.NewWorld },
                    { "|Bricks (OW) = -42,21", -42.21, WorldRegion.OldWorld },
                    { "|Bricks (NW) = -42,21", -42.21, WorldRegion.NewWorld },
                    //{ "|Bricks (OW) = 42.21", 42.21, WorldRegion.OldWorld },//incorrect numberstyle on wiki
                    //{ "|Bricks (NW) = 42.21", 42.21, WorldRegion.NewWorld },//incorrect numberstyle on wiki
                    //{ "|Bricks (OW) = -42.21", -42.21, WorldRegion.OldWorld },//incorrect numberstyle on wiki
                    //{ "|Bricks (NW) = -42.21", -42.21, WorldRegion.NewWorld },//incorrect numberstyle on wiki
                    { "|Bricks (OW) = -100", -100d, WorldRegion.OldWorld },
                    { "|Bricks (NW) = -100", -100d, WorldRegion.NewWorld },
                    { "|Bricks (OW)    =    150   ", 150d, WorldRegion.OldWorld },
                    { "|Bricks (NW)    =    150   ", 150d, WorldRegion.NewWorld },
                    { "|Bricks (OW) = 150}}", 150d, WorldRegion.OldWorld },
                    { "|Bricks (NW) = 150}}", 150d, WorldRegion.NewWorld },
                    { "|Bricks (OW) = 42,21   }}", 42.21, WorldRegion.OldWorld },
                    { "|Bricks (NW) = 42,21   }}", 42.21, WorldRegion.NewWorld },
                };
            }
        }

        public static TheoryData<string, double, WorldRegion> ConstructionInfoSteelBeamsTestData
        {
            get
            {
                return new TheoryData<string, double, WorldRegion>
                {
                    { "|Steel Beams (OW) = 15000", 15000d, WorldRegion.OldWorld },
                    { "|Steel Beams (NW) = 15000", 15000d, WorldRegion.NewWorld },
                    { "|Steel Beams (OW) = 150", 150d, WorldRegion.OldWorld },
                    { "|Steel Beams (NW) = 150", 150d, WorldRegion.NewWorld },
                    { "|Steel Beams (OW) = 42,21", 42.21, WorldRegion.OldWorld },
                    { "|Steel Beams (NW) = 42,21", 42.21, WorldRegion.NewWorld },
                    { "|Steel Beams (OW) = -42,21", -42.21, WorldRegion.OldWorld },
                    { "|Steel Beams (NW) = -42,21", -42.21, WorldRegion.NewWorld },
                    //{ "|Steel Beams (OW) = 42.21", 42.21, WorldRegion.OldWorld },//incorrect numberstyle on wiki
                    //{ "|Steel Beams (NW) = 42.21", 42.21, WorldRegion.NewWorld },//incorrect numberstyle on wiki
                    //{ "|Steel Beams (OW) = -42.21", -42.21, WorldRegion.OldWorld },//incorrect numberstyle on wiki
                    //{ "|Steel Beams (NW) = -42.21", -42.21, WorldRegion.NewWorld },//incorrect numberstyle on wiki
                    { "|Steel Beams (OW) = -100", -100d, WorldRegion.OldWorld },
                    { "|Steel Beams (NW) = -100", -100d, WorldRegion.NewWorld },
                    { "|Steel Beams (OW)    =    150   ", 150d, WorldRegion.OldWorld },
                    { "|Steel Beams (NW)    =    150   ", 150d, WorldRegion.NewWorld },
                    { "|Steel Beams (OW) = 150}}", 150d, WorldRegion.OldWorld },
                    { "|Steel Beams (NW) = 150}}", 150d, WorldRegion.NewWorld },
                    { "|Steel Beams (OW) = 42,21   }}", 42.21, WorldRegion.OldWorld },
                    { "|Steel Beams (NW) = 42,21   }}", 42.21, WorldRegion.NewWorld },
                };
            }
        }

        public static TheoryData<string, double, WorldRegion> ConstructionInfoWindowsTestData
        {
            get
            {
                return new TheoryData<string, double, WorldRegion>
                {
                    { "|Windows (OW) = 15000", 15000d, WorldRegion.OldWorld },
                    { "|Windows (NW) = 15000", 15000d, WorldRegion.NewWorld },
                    { "|Windows (OW) = 150", 150d, WorldRegion.OldWorld },
                    { "|Windows (NW) = 150", 150d, WorldRegion.NewWorld },
                    { "|Windows (OW) = 42,21", 42.21, WorldRegion.OldWorld },
                    { "|Windows (NW) = 42,21", 42.21, WorldRegion.NewWorld },
                    { "|Windows (OW) = -42,21", -42.21, WorldRegion.OldWorld },
                    { "|Windows (NW) = -42,21", -42.21, WorldRegion.NewWorld },
                    //{ "|Windows (OW) = 42.21", 42.21, WorldRegion.OldWorld },//incorrect numberstyle on wiki
                    //{ "|Windows (NW) = 42.21", 42.21, WorldRegion.NewWorld },//incorrect numberstyle on wiki
                    //{ "|Windows (OW) = -42.21", -42.21, WorldRegion.OldWorld },//incorrect numberstyle on wiki
                    //{ "|Windows (NW) = -42.21", -42.21, WorldRegion.NewWorld },//incorrect numberstyle on wiki
                    { "|Windows (OW) = -100", -100d, WorldRegion.OldWorld },
                    { "|Windows (NW) = -100", -100d, WorldRegion.NewWorld },
                    { "|Windows (OW)    =    150   ", 150d, WorldRegion.OldWorld },
                    { "|Windows (NW)    =    150   ", 150d, WorldRegion.NewWorld },
                    { "|Windows (OW) = 150}}", 150d, WorldRegion.OldWorld },
                    { "|Windows (NW) = 150}}", 150d, WorldRegion.NewWorld },
                    { "|Windows (OW) = 42,21   }}", 42.21, WorldRegion.OldWorld },
                    { "|Windows (NW) = 42,21   }}", 42.21, WorldRegion.NewWorld },
                };
            }
        }

        public static TheoryData<string, double, WorldRegion> ConstructionInfoConcreteTestData
        {
            get
            {
                return new TheoryData<string, double, WorldRegion>
                {
                    { "|Concrete (OW) = 15000", 15000d, WorldRegion.OldWorld },
                    { "|Concrete (NW) = 15000", 15000d, WorldRegion.NewWorld },
                    { "|Concrete (OW) = 150", 150d, WorldRegion.OldWorld },
                    { "|Concrete (NW) = 150", 150d, WorldRegion.NewWorld },
                    { "|Concrete (OW) = 42,21", 42.21, WorldRegion.OldWorld },
                    { "|Concrete (NW) = 42,21", 42.21, WorldRegion.NewWorld },
                    { "|Concrete (OW) = -42,21", -42.21, WorldRegion.OldWorld },
                    { "|Concrete (NW) = -42,21", -42.21, WorldRegion.NewWorld },
                    //{ "|Concrete (OW) = 42.21", 42.21, WorldRegion.OldWorld },//incorrect numberstyle on wiki
                    //{ "|Concrete (NW) = 42.21", 42.21, WorldRegion.NewWorld },//incorrect numberstyle on wiki
                    //{ "|Concrete (OW) = -42.21", -42.21, WorldRegion.OldWorld },//incorrect numberstyle on wiki
                    //{ "|Concrete (NW) = -42.21", -42.21, WorldRegion.NewWorld },//incorrect numberstyle on wiki
                    { "|Concrete (OW) = -100", -100d, WorldRegion.OldWorld },
                    { "|Concrete (NW) = -100", -100d, WorldRegion.NewWorld },
                    { "|Concrete (OW)    =    150   ", 150d, WorldRegion.OldWorld },
                    { "|Concrete (NW)    =    150   ", 150d, WorldRegion.NewWorld },
                    { "|Concrete (OW) = 150}}", 150d, WorldRegion.OldWorld },
                    { "|Concrete (NW) = 150}}", 150d, WorldRegion.NewWorld },
                    { "|Concrete (OW) = 42,21   }}", 42.21, WorldRegion.OldWorld },
                    { "|Concrete (NW) = 42,21   }}", 42.21, WorldRegion.NewWorld },
                };
            }
        }

        public static TheoryData<string, double, WorldRegion> ConstructionInfoWeaponsTestData
        {
            get
            {
                return new TheoryData<string, double, WorldRegion>
                {
                    { "|Weapons (OW) = 15000", 15000d, WorldRegion.OldWorld },
                    { "|Weapons (NW) = 15000", 15000d, WorldRegion.NewWorld },
                    { "|Weapons (OW) = 150", 150d, WorldRegion.OldWorld },
                    { "|Weapons (NW) = 150", 150d, WorldRegion.NewWorld },
                    { "|Weapons (OW) = 42,21", 42.21, WorldRegion.OldWorld },
                    { "|Weapons (NW) = 42,21", 42.21, WorldRegion.NewWorld },
                    { "|Weapons (OW) = -42,21", -42.21, WorldRegion.OldWorld },
                    { "|Weapons (NW) = -42,21", -42.21, WorldRegion.NewWorld },
                    //{ "|Weapons (OW) = 42.21", 42.21, WorldRegion.OldWorld },//incorrect numberstyle on wiki
                    //{ "|Weapons (NW) = 42.21", 42.21, WorldRegion.NewWorld },//incorrect numberstyle on wiki
                    //{ "|Weapons (OW) = -42.21", -42.21, WorldRegion.OldWorld },//incorrect numberstyle on wiki
                    //{ "|Weapons (NW) = -42.21", -42.21, WorldRegion.NewWorld },//incorrect numberstyle on wiki
                    { "|Weapons (OW) = -100", -100d, WorldRegion.OldWorld },
                    { "|Weapons (NW) = -100", -100d, WorldRegion.NewWorld },
                    { "|Weapons (OW)    =    150   ", 150d, WorldRegion.OldWorld },
                    { "|Weapons (NW)    =    150   ", 150d, WorldRegion.NewWorld },
                    { "|Weapons (OW) = 150}}", 150d, WorldRegion.OldWorld },
                    { "|Weapons (NW) = 150}}", 150d, WorldRegion.NewWorld },
                    { "|Weapons (OW) = 42,21   }}", 42.21, WorldRegion.OldWorld },
                    { "|Weapons (NW) = 42,21   }}", 42.21, WorldRegion.NewWorld },
                };
            }
        }

        public static TheoryData<string, double, WorldRegion> ConstructionInfoAdvancedWeaponsTestData
        {
            get
            {
                return new TheoryData<string, double, WorldRegion>
                {
                    { "|Advanced Weapons (OW) = 15000", 15000d, WorldRegion.OldWorld },
                    { "|Advanced Weapons (NW) = 15000", 15000d, WorldRegion.NewWorld },
                    { "|Advanced Weapons (OW) = 150", 150d, WorldRegion.OldWorld },
                    { "|Advanced Weapons (NW) = 150", 150d, WorldRegion.NewWorld },
                    { "|Advanced Weapons (OW) = 42,21", 42.21, WorldRegion.OldWorld },
                    { "|Advanced Weapons (NW) = 42,21", 42.21, WorldRegion.NewWorld },
                    { "|Advanced Weapons (OW) = -42,21", -42.21, WorldRegion.OldWorld },
                    { "|Advanced Weapons (NW) = -42,21", -42.21, WorldRegion.NewWorld },
                    //{ "|Advanced Weapons (OW) = 42.21", 42.21, WorldRegion.OldWorld },//incorrect numberstyle on wiki
                    //{ "|Advanced Weapons (NW) = 42.21", 42.21, WorldRegion.NewWorld },//incorrect numberstyle on wiki
                    //{ "|Advanced Weapons (OW) = -42.21", -42.21, WorldRegion.OldWorld },//incorrect numberstyle on wiki
                    //{ "|Advanced Weapons (NW) = -42.21", -42.21, WorldRegion.NewWorld },//incorrect numberstyle on wiki
                    { "|Advanced Weapons (OW) = -100", -100d, WorldRegion.OldWorld },
                    { "|Advanced Weapons (NW) = -100", -100d, WorldRegion.NewWorld },
                    { "|Advanced Weapons (OW)    =    150   ", 150d, WorldRegion.OldWorld },
                    { "|Advanced Weapons (NW)    =    150   ", 150d, WorldRegion.NewWorld },
                    { "|Advanced Weapons (OW) = 150}}", 150d, WorldRegion.OldWorld },
                    { "|Advanced Weapons (NW) = 150}}", 150d, WorldRegion.NewWorld },
                    { "|Advanced Weapons (OW) = 42,21   }}", 42.21, WorldRegion.OldWorld },
                    { "|Advanced Weapons (NW) = 42,21   }}", 42.21, WorldRegion.NewWorld },
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
            var parser = GetParser();

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
            var parser = GetParser();

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
            var parser = GetParser();

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
            var parser = GetParser();

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
            var parser = GetParser();

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
            var parser = GetParser();

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
            var parser = GetParser();

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
            var parser = GetParser();

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
            var parser = GetParser();

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
            var parser = GetParser();

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
            var parser = GetParser();

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
            var parser = GetParser();

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
            var parser = GetParser();

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

            var parser = GetParser();

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

            var parser = GetParser();

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

            var parser = GetParser();

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

            var parser = GetParser();

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

            var parser = GetParser();

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

            var parser = GetParser();

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

            var parser = GetParser();

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

            var parser = GetParser();

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

            var parser = GetParser();

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

            var parser = GetParser();

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

            var parser = GetParser();

            // Act/Assert
            var ex = Assert.Throws<Exception>(() => parser.GetInfobox(input));
        }

        [Fact]
        public void GetInfobox_InputElectricityCounterIsNotParseable_ShouldThrow()
        {
            // Arrange
            var input = $"|Produces Amount (OW) = 1{Environment.NewLine}|Input {int.MaxValue + 1L} Amount Electricity (OW) = 42";

            var parser = GetParser();

            // Act/Assert
            var ex = Assert.Throws<Exception>(() => parser.GetInfobox(input));
        }

        [Fact]
        public void GetInfobox_InputIconCounterIsNotParseable_ShouldThrow()
        {
            // Arrange
            var input = $"|Produces Amount (OW) = 1{Environment.NewLine}|Input {int.MaxValue + 1L} Icon (OW) = dummy.png";

            var parser = GetParser();

            // Act/Assert
            var ex = Assert.Throws<Exception>(() => parser.GetInfobox(input));
        }

        [Fact]
        public void GetInfobox_WikiTextContainsInputAmount_ShouldReturnCorrectValue()
        {
            // Arrange
            var input = $"|Produces Amount{Environment.NewLine}|Input 1 Amount (OW) = 42";

            var parser = GetParser();

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

            var parser = GetParser();

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

            var parser = GetParser();

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

            var parser = GetParser();

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

            var parser = GetParser();

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

            var parser = GetParser();

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

            var parser = GetParser();

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
            var parser = GetParser();

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

            var parser = GetParser();

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

            var parser = GetParser();

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

            var parser = GetParser();

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

            var parser = GetParser();

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

            var parser = GetParser();

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

            var parser = GetParser();

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

            var parser = GetParser();

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

            var parser = GetParser();

            // Act/Assert
            var ex = Assert.Throws<Exception>(() => parser.GetInfobox(input));
        }

        [Fact(Skip = "regex needs adjustment")]
        public void GetInfobox_SupplyAmountIsNotParseable_ShouldThrow()
        {
            // Arrange
            var input = $"|Supplies 1 Amount (OW) = {double.MaxValue.ToString()}";

            var parser = GetParser();

            // Act/Assert
            var ex = Assert.Throws<Exception>(() => parser.GetInfobox(input));
        }

        [Fact]
        public void GetInfobox_SupplyAmountElectricityCounterIsNotParseable_ShouldThrow()
        {
            // Arrange
            var input = $"|Supplies {int.MaxValue + 1L} Amount Electricity (OW) = 42";

            var parser = GetParser();

            // Act/Assert
            var ex = Assert.Throws<Exception>(() => parser.GetInfobox(input));
        }

        [Fact(Skip = "regex needs adjustment")]
        public void GetInfobox_SupplyAmountElectricityIsNotParseable_ShouldThrow()
        {
            // Arrange
            var input = $"|Supplies 1 Amount Electricity (OW) = {double.MaxValue.ToString()}";

            var parser = GetParser();

            // Act/Assert
            var ex = Assert.Throws<Exception>(() => parser.GetInfobox(input));
        }

        [Fact]
        public void GetInfobox_SupplyTypeCounterIsNotParseable_ShouldThrow()
        {
            // Arrange
            var input = $"|Supplies {int.MaxValue + 1L} Type (OW) = Farmer";

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
            var result = parser.GetInfobox(testDataEmpty_BothWorlds);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Null(result[0].UnlockInfos);
            Assert.Null(result[1].UnlockInfos);
        }

        [Theory]
        [MemberData(nameof(UnlockInfoAmountTestData))]
        public void GetInfobox_WikiTextContainsUnlockAmount_ShouldReturnCorrectValue(string input, int expectedOrder, double expectedAmount, WorldRegion regionToTest)
        {
            // Arrange
            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(2, result.Count);

            if (regionToTest == WorldRegion.OldWorld)
            {
                Assert.Single(result[0].UnlockInfos.UnlockConditions);
                Assert.Equal(expectedAmount, result[0].UnlockInfos.UnlockConditions[0].Amount);
                Assert.Equal(expectedOrder, result[0].UnlockInfos.UnlockConditions[0].Order);
                Assert.Null(result[0].UnlockInfos.UnlockConditions[0].Type);
            }
            else
            {
                Assert.Single(result[1].UnlockInfos.UnlockConditions);
                Assert.Equal(expectedAmount, result[1].UnlockInfos.UnlockConditions[0].Amount);
                Assert.Equal(expectedOrder, result[1].UnlockInfos.UnlockConditions[0].Order);
                Assert.Null(result[1].UnlockInfos.UnlockConditions[0].Type);
            }

        }

        [Theory]
        [MemberData(nameof(UnlockInfoTypeTestData))]
        public void GetInfobox_WikiTextContainsUnlockType_ShouldReturnCorrectValue(string input, int expectedOrder, string expectedType, WorldRegion regionToTest)
        {
            // Arrange
            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(2, result.Count);

            if (regionToTest == WorldRegion.OldWorld)
            {
                Assert.Single(result[0].UnlockInfos.UnlockConditions);
                Assert.Equal(0, result[0].UnlockInfos.UnlockConditions[0].Amount);
                Assert.Equal(expectedOrder, result[0].UnlockInfos.UnlockConditions[0].Order);
                Assert.Equal(expectedType, result[0].UnlockInfos.UnlockConditions[0].Type);
            }
            else
            {
                Assert.Single(result[1].UnlockInfos.UnlockConditions);
                Assert.Equal(0, result[1].UnlockInfos.UnlockConditions[0].Amount);
                Assert.Equal(expectedOrder, result[1].UnlockInfos.UnlockConditions[0].Order);
                Assert.Equal(expectedType, result[1].UnlockInfos.UnlockConditions[0].Type);
            }
        }

        [Fact]
        public void GetInfobox_WikiTextContainsMultipleUnlockInfos_ShouldReturnCorrectOrderedValue()
        {
            // Arrange
            var input = $"|Unlock Condition 2 Amount (OW) = 21{Environment.NewLine}|Unlock Condition 1 Amount (OW) = 42";

            var parser = GetParser();

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

            var parser = GetParser();

            // Act/Assert
            var ex = Assert.Throws<Exception>(() => parser.GetInfobox(input));
        }

        [Fact(Skip = "regex needs adjustment")]
        public void GetInfobox_UnlockAmountIsNotParseable_ShouldThrow()
        {
            // Arrange
            var input = $"|Unlock Condition 1 Amount (OW) = {double.MaxValue.ToString()}";

            var parser = GetParser();

            // Act/Assert
            var ex = Assert.Throws<Exception>(() => parser.GetInfobox(input));
        }

        [Fact]
        public void GetInfobox_UnlockTypeCounterIsNotParseable_ShouldThrow()
        {
            // Arrange
            var input = $"|Unlock Condition {int.MaxValue + 1L} Type (OW) = Farmers";

            var parser = GetParser();

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
            var parser = GetParser();

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
            var parser = GetParser();

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

        #region BuildingSize tests

        [Theory]
        [InlineData("dummy", WorldRegion.OldWorld)]
        [InlineData("dummy", WorldRegion.NewWorld)]
        [InlineData("|Building Size (OW) = ", WorldRegion.OldWorld)]
        [InlineData("|Building Size (NW) = ", WorldRegion.NewWorld)]
        public void GetInfobox_WikiTextContainsNoBuildingSize_ShouldReturnEmptySize(string input, WorldRegion regionToTest)
        {
            // Arrange
            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            if (regionToTest == WorldRegion.OldWorld)
            {
                Assert.Equal(Size.Empty, result[0].BuildingSize);
            }
            else
            {
                Assert.Equal(Size.Empty, result[1].BuildingSize);
            }
        }

        [Theory]
        [InlineData("|Building Size (OW) = ?x?", WorldRegion.OldWorld)]
        [InlineData("|Building Size (OW) = ? x ?", WorldRegion.OldWorld)]
        [InlineData("|Building Size (OW) = ? x?", WorldRegion.OldWorld)]
        [InlineData("|Building Size (OW) = ?x ?", WorldRegion.OldWorld)]
        [InlineData("|Building Size (OW) = 3x", WorldRegion.OldWorld)]
        [InlineData("|Building Size (OW) = dummyxdummy", WorldRegion.OldWorld)]
        [InlineData("|Building Size (NW) = ?x?", WorldRegion.NewWorld)]
        [InlineData("|Building Size (NW) = ? x ?", WorldRegion.NewWorld)]
        [InlineData("|Building Size (NW) = ? x?", WorldRegion.NewWorld)]
        [InlineData("|Building Size (NW) = ?x ?", WorldRegion.NewWorld)]
        [InlineData("|Building Size (NW) = 3x", WorldRegion.NewWorld)]
        [InlineData("|Building Size (NW) = dummyxdummy", WorldRegion.NewWorld)]
        public void GetInfobox_WikiTextContainsUnknownBuildingSize_ShouldReturnEmptySize(string input, WorldRegion regionToTest)
        {
            // Arrange
            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            if (regionToTest == WorldRegion.OldWorld)
            {
                Assert.Equal(Size.Empty, result[0].BuildingSize);
            }
            else
            {
                Assert.Equal(Size.Empty, result[1].BuildingSize);
            }
        }

        [Theory]
        [MemberData(nameof(BuildingSizeTestData))]
        public void GetInfobox_WikiTextContainsBuildingSize_ShouldReturnCorrectValue(string input, Size expectedSize, WorldRegion regionToTest)
        {
            // Arrange
            _output.WriteLine($"{nameof(input)}: {input}");

            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            if (regionToTest == WorldRegion.OldWorld)
            {
                Assert.Equal(expectedSize, result[0].BuildingSize);
            }
            else
            {
                Assert.Equal(expectedSize, result[1].BuildingSize);
            }
        }

        #endregion

        #region ConstructionInfo tests

        [Theory]
        [MemberData(nameof(ConstructionInfoCreditsTestData))]
        [MemberData(nameof(ConstructionInfoTimberTestData))]
        [MemberData(nameof(ConstructionInfoBricksTestData))]
        [MemberData(nameof(ConstructionInfoSteelBeamsTestData))]
        [MemberData(nameof(ConstructionInfoWindowsTestData))]
        [MemberData(nameof(ConstructionInfoConcreteTestData))]
        [MemberData(nameof(ConstructionInfoWeaponsTestData))]
        [MemberData(nameof(ConstructionInfoAdvancedWeaponsTestData))]
        public void GetInfobox_WikiTextContainsConstructionInfo_ShouldReturnCorrectValue(string input, double expectedValue, WorldRegion regionToTest)
        {
            // Arrange
            _output.WriteLine($"{nameof(input)}: {input}");

            var parser = GetParser();

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            if (regionToTest == WorldRegion.OldWorld)
            {
                Assert.Equal(expectedValue, result[0].ConstructionInfos[0].Value);
            }
            else
            {
                Assert.Equal(expectedValue, result[1].ConstructionInfos[0].Value);
            }

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
            if ((WorldRegion)input[2] == WorldRegion.OldWorld)
            {
                Assert.Equal(expectedUnitName, result[0].ConstructionInfos[0].Unit.Name);
            }
            else
            {
                Assert.Equal(expectedUnitName, result[1].ConstructionInfos[0].Unit.Name);
            }
        }

        #endregion
    }
}
