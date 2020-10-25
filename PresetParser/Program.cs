using AnnoDesigner.Core.Extensions;
using AnnoDesigner.Core.Helper;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Core.Presets.Models;
using PresetParser.Anno1404_Anno2070;
using PresetParser.Anno1800;
using PresetParser.Anno1800.Models;
using PresetParser.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Xml;

namespace PresetParser
{
    public class Program
    {
        #region Initalisizing values
        private static string BASE_PATH { get; set; }
        private static string BASE_PATH_1404 { get; set; }
        private static string BASE_PATH_2070 { get; set; }
        private static string BASE_PATH_2205 { get; set; }
        private static string BASE_PATH_1800 { get; set; }

        public static bool isExcludedName = false;
        public static bool isExcludeIconName = false; /*Only for Anno 1800*/
        public static bool isExcludedTemplate = false;
        public static bool isExcludedGUID = false; /*only for Anno 1800 */

        private static Dictionary<string, Dictionary<string, PathRef[]>> VersionSpecificPaths { get; set; }
        private const string BUILDING_PRESETS_VERSION = "3.7";
        // Initalisizing Language Directory's and Filenames
        private static readonly string[] Languages = new[] { "eng", "ger", "fra", "pol", "rus" };
        private static readonly string[] LanguagesFiles2205 = new[] { "english", "german", "french", "polish", "russian" };
        private static readonly string[] LanguagesFiles1800 = new[] { "english", "german", "french", "polish", "russian" };
        // Internal Program Buildings Lists to skipp double buildings
        public static List<string> annoBuildingLists = new List<string>();
        public static List<string> anno1800IconNameLists = new List<string>();
        public static List<string> TempExcludeOrnamentsFromPreset_1800 = new List<string>();
        public static int annoBuildingsListCount = 0, printTestText = 0;
        public static bool testVersion = false;
        // The internal Building list for the Preset writing 
        public static List<IBuildingInfo> buildings = new List<IBuildingInfo>();
        private static readonly IconFileNameHelper _iconFileNameHelper;
        private static readonly BuildingBlockProvider _buildingBlockProvider;
        private static readonly IIfoFileProvider _ifoFileProvider;
        private static readonly LocalizationHelper _localizationHelper;

        #region Initalisizing Exclude IdentifierNames, FactionNames and TemplateNames for presets.json file 

        #region Anno 1404
        private static readonly List<string> ExcludeNameList1404 = new List<string> { "ResidenceRuin", "AmbassadorRuin", "CitizenHouse", "PatricianHouse",
            "NoblemanHouse", "AmbassadorHouse", "Gatehouse", "StorehouseTownPart", "ImperialCathedralPart", "SultanMosquePart", "Warehouse02", "Warehouse03",
            "Markethouse02", "Markethouse03", "TreeBuildCost", "BanditCamp"};
        private static readonly List<string> ExcludeTemplateList1404 = new List<string> { "OrnamentBuilding", "Wall" };
        #endregion

        #region Anno 2070 * Also on FactionName Excludes *
        private static readonly List<string> ExcludeNameList2070 = new List<string> { "distillery_field" , "citizen_residenc", "executive_residence", "leader_residence",
                "ruin_residence" , "villager_residence" ,"builder_residence", "creator_residence" ,"scientist_residence", "genius_residence", "monument_unfinished",
                "town_center_variation", "underwater_energy_transmitter", "iron_mine", "nuclearpowerplant_destroyed","limestone_quarry", "markethouse2", "markethouse3",
                "warehouse2","warehouse3", "cybernatic_factory","vegetable_farm_field","electronic_recycler"};
        private static readonly List<string> ExcludeTemplateList2070 = new List<string> { "OrnamentBuilding", "OrnamentFeedbackBuilding", "Ark" };
        private static readonly List<string> ExcludeFactionList2070 = new List<string> { "third party" };
        #endregion

        #region Anno 2205 Also a GUID Checker for excluding. Do not change any numbers below
        private static readonly List<string> ExcludeGUIDList2205 = new List<string> { "1001178", "1000737", "7000274", "1001175", "1000736", "7000275",
            "1000672", "1000755", "7000273", "1001171", "1000703", "7000272", "7000420", "7000421", "7000423", "7000424", "7000425", "7000427", "7000428",
            "7000429", "7000430", "7000431", "12000009", "12000010", "12000011", "12000020", "12000036", "1000063", "1000170", "1000212", "1000213", "1000174",
            "1000215", "1000217", "1000224", "1000250", "1000332", "1000886", "7001466", "7001467", "7001470", "7001471", "7001472", "7001473", "7001877",
            "7001878", "7001879", "7001880", "7001881", "7001882", "7001883", "7001884", "7001885", "7000310", "7000311", "7000315", "7000316",
            "7000313", "7000263", "7000262", "7000305", "7000306" };
        private static readonly List<string> ExcludeNameList2205 = new List<string> { "Placeholder", "tier02", "tier03", "tier04", "tier05", "voting",
            "CTU Reactor 2 (decommissioned)", "CTU Reactor 3 (decommissioned)", "CTU Reactor 4 (decommissioned)", "CTU Reactor 5 (decommissioned)", "CTU Reactor 6 (decommissioned)",
            "CTU Reactor 2 (active!)", "CTU Reactor 3 (active!)", "CTU Reactor 4 (active!)", "CTU Reactor 5 (active!)", "CTU Reactor 6 (active!)", "orbit module 07 (unused)" };
        private static readonly List<string> ExcludeTemplateList2205 = new List<string> { "SpacePort", "BridgeWithUpgrade", "DistributionBuilding" };
        private static readonly List<string> testGUIDNames2205 = new List<string> { "NODOUBLES YET" };
        #endregion

        #region anno 1800
        /// <summary>
        /// I need the IncludeBuildingsTemplateNames to get Building informaton from, as it is also the Presets Template String or Template GUID
        /// </summary>
        public static IList<FarmField> farmFieldList1800 = new List<FarmField>();
        // Removed IncludeBuildingsTemplate "CultureModule" (to must to handle and thus are replaced with the Zoo Module and Museum Module
        private static readonly List<string> IncludeBuildingsTemplateNames1800 = new List<string> { "ResidenceBuilding7", "FarmBuilding", "FreeAreaBuilding", "FactoryBuilding7", "HeavyFactoryBuilding",
            "SlotFactoryBuilding7", "Farmfield", "OilPumpBuilding", "PublicServiceBuilding", "CityInstitutionBuilding", "CultureBuilding", "Market", "Warehouse", "PowerplantBuilding",
            "HarborOffice", "HarborWarehouse7", "HarborDepot","Shipyard","HarborBuildingAttacker", "RepairCrane", "HarborLandingStage7", "VisitorPier", "WorkforceConnector", "Guildhouse", "OrnamentalBuilding",
            "CultureModule","Palace","BuffFactory", "BuildPermitBuilding", "BuildPermitModules", "OrnamentalModule", "IrrigationPropagationSource", "ResearchCenter" };
        private static readonly List<string> IncludeBuildingsTemplateGUID1800 = new List<string> { "100451", "1010266", "1010343", "1010288", "101331", "1010320", "1010263", "1010372", "1010359", "1010358", "1010462",
            "1010463", "1010464", "1010275", "1010271", "1010516", "1010517", "1010519"};
        private static readonly List<string> ExcludeBuildingsGUID1800 = new List<string> { "269850", "269851" };
        private static readonly List<string> ExcludeNameList1800 = new List<string> { "tier02", "tier03", "tier04", "tier05", "(Wood Field)", "(Hunting Grounds)", "(Wash House)", "Quay System",
            "module_01_birds", "module_02_peacock", "(Warehouse II)", "(Warehouse III)", "logistic_colony01_01 (Warehouse I)", "Kontor_main_02", "Kontor_main_03", "kontor_main_colony01",
            "Fake Ornament [test 2nd party]", "Kontor_imperial_02", "Kontor_imperial_03","(Oil Harbor II)","(Oil Harbor III)", "Third_party_", "CQO_", "Kontor_imperial_01", "- Pirates",
            "Harbor_colony01_09 (tourism_pier_01)", "Ai_", "AarhantLighthouseFake", "CO_Tunnel_Entrance01_Fake","Park_1x1_fence", "Electricity_01", "AI Version No Unlock",
            "Entertainment_musicpavillion_1701", "Entertainment_musicpavillion_1404", "Entertainment_musicpavillion_2070", "Entertainment_musicpavillion_2205", "Entertainment_musicpavillion_1800",
            "Culture_01_module_06_empty","Culture_02_module_06_empty", "AnarchyBanner", "Culture_props_system_all_nohedge", "Monument_arctic_01_01", "Monument_arctic_01_02", "Monument_arctic_01_03",
            "Active fertility","- Decree","Ministry of Public Services","Ministry of Productivity","Arctic Shepherd","fertility","Arctic Cook","Arctic Builder","Arctic Hunter","Arctic Sewer"," Buff"," Seeds",
            "PropagandaTower Merciers Version","tractor_module_02 (Harvester)", "Culture_1x1_statue", "Culture_prop_system_1x1_10", "Culture_prop_system_1x1_01", "Logistic_05 (Warehouse IV)", "Park_1x1_hedgeentrance",
            "Harbour Slot (Ghost) Arctic", "Tractor_module_01 (GASOLINE TEST)", "Fuel_station_01 (GASOLINE TEST)", "Kontor_main_04", "Kontor_imperial_04", "Culture_1x1_plaza","Harbor_12 (Coal Harbor)",
            "Harbor_13 (Coal Storage)", "Kontor_main_arctic_01", "Kontor_imperial_arctic_01", "ResearchCenter_02", "ResearchCenter_03", "StoryIsland01 Monastery Kontor","StoryIsland02 Military Kontor",
            "StoryIsland03 Economy Kontor" };
        //Skip the following icons to put in the presets for anno 1800, to avoid double Ornamentalbuildings
        public static List<string> ExcludeOrnamentsIcons_1800 = new List<string> { "A7_bush03.png", "A7_park_props_1x1_01.png", "A7_park_props_1x1_07.png", "A7_bush01.png", "A7_col_props_1x1_13_back.png", "A7_bush05.png", "A7_park_props_1x1_08.png",
            "A7_bush02.png", "A7_bush04.png", "A7_col_props_1x1_11_bac.pngk", "A7_col_props_1x1_01_back.png", "A7_col_props_1x1_07_back.png","A7_park_1x1_06.png","A7_park_1x1_02.png","A7_park_1x1_03.png","A7_col_park_props_system_1x1_21_back.png",
            "A7_park_3x3_02.png", "A7_park_2x2_05.png","A7_park_2x2_02.png", "A7_col_props_1x1_11_back.png", "A7_benches.png", "A7_park_2x2_04.png"};

        /// <summary>
        /// in NewFactionAndGroup1800.cs are made the following lists
        /// ChangeBuildingTo<1>_<2>_1800 
        /// <1> can be : OW (All Worlds) - OW1 (New World - (1) Farmers) - OW2 (New World - (2) Workers) - OW3 (New World - (3) Artisans)
        ///              OW4 (New World - (4) Engineers  - OW5 (New World - (5) Investors) - OW6 (New World (13) Scholars)  
        ///              NW1 (Old World - (7) Jornaleros  - NW2 (Old World - (8) Obreros)
        ///              AT1 (Arctic - (10) Explorers)  - AT2 (Arctic - (11) Technicians)
        ///              AF1 (Africa - (14) Shepherds)  - AF2 (Africa - (15) Elders) 
        /// <2> wil be the Group under <1>, like Production, Public, etc
        ///
        /// Changed the mistake OW/NW (23-10-2020) it is as in game now OW = Old World and NW = New World
        #endregion

        #endregion

        static Program()
        {
            _iconFileNameHelper = new IconFileNameHelper();
            _ifoFileProvider = new IfoFileProvider();
            _buildingBlockProvider = new BuildingBlockProvider(_ifoFileProvider);

            _localizationHelper = new LocalizationHelper(new FileSystem());

            VersionSpecificPaths = new Dictionary<string, Dictionary<string, PathRef[]>>();
        }
        #endregion

        public static void Main(string[] args)
        {
            #region User Choices 
            string annoVersion = "";
            bool validVersion = false;
            while (!validVersion)
            {
                Console.Write("Please enter an Anno version (1 of: {0} {1} {2} {3}):", Constants.ANNO_VERSION_1404, Constants.ANNO_VERSION_2070, Constants.ANNO_VERSION_2205, Constants.ANNO_VERSION_1800);
                annoVersion = Console.ReadLine();
                if (annoVersion == "quit")
                {
                    Environment.Exit(0);
                }
                if (annoVersion == Constants.ANNO_VERSION_1404 || annoVersion == Constants.ANNO_VERSION_2070 || annoVersion == Constants.ANNO_VERSION_2205 || annoVersion == Constants.ANNO_VERSION_1800 || annoVersion == "-ALL")
                {
                    validVersion = true;
                }
                else if (annoVersion.ToLower() == "-test")
                {
                    Console.Write("Please enter an Anno version:");
                    annoVersion = Console.ReadLine();
                    if (annoVersion == Constants.ANNO_VERSION_1404 || annoVersion == Constants.ANNO_VERSION_2070 || annoVersion == Constants.ANNO_VERSION_2205 || annoVersion == Constants.ANNO_VERSION_1800)
                    {
                        validVersion = true;
                        testVersion = true;
                    }
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine("Invalid input, please try again or enter 'quit to exit.");
                }
            }
            if (annoVersion != "-ALL")
            {
                ///Add a trailing backslash if one is not present.
                BASE_PATH = GetBASE_PATH(annoVersion);
            }
            else
            {
                BASE_PATH_1404 = GetBASE_PATH(Constants.ANNO_VERSION_1404);
                BASE_PATH_2070 = GetBASE_PATH(Constants.ANNO_VERSION_2070);
                BASE_PATH_2205 = GetBASE_PATH(Constants.ANNO_VERSION_2205);
                BASE_PATH_1800 = GetBASE_PATH(Constants.ANNO_VERSION_1800);
            }
            if (!testVersion)
            {
                Console.WriteLine("Extracting and parsing RDA data from {0} for anno version {1}.", BASE_PATH, annoVersion);
            }
            else if (annoVersion != "-ALL")
            {
                Console.WriteLine("Tesing RDA data from {0} for anno version {1}.", BASE_PATH, annoVersion);
            }
            else
            {
                Console.WriteLine("Extracting and parsing RDA data for all Anno versions");
            }
            #endregion

            #region Anno Verion Data Paths
            /// <summary>
            /// Holds the paths and xpaths to parse the extracted RDA's for different Anno versions
            /// 
            /// The RDA's should all be extracted into the same directory.
            /// </summary>
            //These should stay constant for different anno versions (hopefully!)
            #region Anno 1404 xPaths
            if (annoVersion == Constants.ANNO_VERSION_1404 || annoVersion == "-ALL")
            {
                VersionSpecificPaths.Add(Constants.ANNO_VERSION_1404, new Dictionary<string, PathRef[]>());
                VersionSpecificPaths[Constants.ANNO_VERSION_1404].Add("icons", new PathRef[]
                {
                //new PathRef("data/config/game/icons.xml", "/Icons/i" ),
                new PathRef("addondata/config/game/icons.xml", "/Icons/i", "", "")
                });
                VersionSpecificPaths[Constants.ANNO_VERSION_1404].Add("localisation", new PathRef[]
                {
                new PathRef("data/loca"),
                new PathRef("addondata/loca")
                });
                VersionSpecificPaths[Constants.ANNO_VERSION_1404].Add("assets", new PathRef[]
                {
                new PathRef("data/config/game/assets.xml", "/AssetList/Groups/Group/Groups/Group", "Groups/Group/Assets/Asset", "PlayerBuildings"),
                new PathRef("data/config/game/assets.xml", "/AssetList/Groups/Group/Groups/Group", "Groups/Group/Groups/Group/Assets/Asset", "PlayerBuildings"),
                new PathRef("data/config/game/assets.xml", "/AssetList/Groups/Group/Groups/Group", "Groups/Group/Groups/Group/Groups/Group/Assets/Asset", "PlayerBuildings"),
                new PathRef("addondata/config/game/assets.xml", "/AssetList/Groups/Group/Groups/Group", "Groups/Group/Assets/Asset", "PlayerBuildings"),
                new PathRef("addondata/config/game/assets.xml", "/AssetList/Groups/Group/Groups/Group", "Groups/Group/Groups/Group/Assets/Asset", "PlayerBuildings"),
                new PathRef("addondata/config/balancing/addon_01_assets.xml", "/Group/Groups/Group/Groups/Group", "Groups/Group/Groups/Group/Assets/Asset", "PlayerBuildings")
                });
            }
            #endregion

            #region Anno 2070 xPaths
            if (annoVersion == Constants.ANNO_VERSION_2070 || annoVersion == "-ALL")
            {
                VersionSpecificPaths.Add(Constants.ANNO_VERSION_2070, new Dictionary<string, PathRef[]>());
                VersionSpecificPaths[Constants.ANNO_VERSION_2070].Add("icons", new PathRef[]
                {
                new PathRef("data/config/game/icons.xml", "/Icons/i", "", "")
                });
                VersionSpecificPaths[Constants.ANNO_VERSION_2070].Add("localisation", new PathRef[]
                {
                new PathRef("data/loca")
                });
                VersionSpecificPaths[Constants.ANNO_VERSION_2070].Add("assets", new PathRef[]
                {
                new PathRef("data/config/game/assets.xml", "/AssetList/Groups/Group/Groups/Group", "Groups/Group/Groups/Group/Assets/Asset", "Buildings"),
                new PathRef("data/config/game/assets.xml", "/AssetList/Groups/Group/Groups/Group", "Groups/Group/Groups/Group/Groups/Group/Assets/Asset", "Buildings"),
                new PathRef("data/config/dlc_01/assets.xml", "/Group/Groups/Group", "Groups/Group/Groups/Group/Assets/Asset", "Buildings"),
                new PathRef("data/config/dlc_02/assets.xml", "/Group/Groups/Group", "Groups/Group/Groups/Group/Assets/Asset", "Buildings"),
                new PathRef("data/config/dlc_03/assets.xml", "/Group/Groups/Group", "Groups/Group/Groups/Group/Assets/Asset", "Buildings"),
                new PathRef("addondata/config/balancing/addon_01_assets.xml", "/Group/Groups/Group", "Groups/Group/Groups/Group/Assets/Asset", "Buildings"),
                new PathRef("addondata/config/balancing/addon_01_assets.xml", "/Group/Groups/Group", "Groups/Group/Groups/Group/Groups/Group/Assets/Asset", "Buildings")
                });
            }
            #endregion

            #region Anno 2205 xPaths
            if (annoVersion == Constants.ANNO_VERSION_2205 || annoVersion == "-ALL")
            {
                VersionSpecificPaths.Add(Constants.ANNO_VERSION_2205, new Dictionary<string, PathRef[]>());
                /// Trying to read data from the objects.exm 
                Console.WriteLine();
                Console.WriteLine("Trying to read Buildings Data from the objects.xml of anno 2205");
                VersionSpecificPaths[Constants.ANNO_VERSION_2205].Add("assets", new PathRef[]
                {
                    #region Data Structure Normal Anno 2205
                    new PathRef("data/config/game/asset/objects/buildings.xml", "/Group/Groups/Group", "Groups/Group/Assets/Asset", "Earth"),
                    new PathRef("data/config/game/asset/objects/buildings.xml", "/Group/Groups/Group", "Groups/Group/Groups/Group/Assets/Asset", "Earth"),
                    new PathRef("data/config/game/asset/objects/buildings.xml", "/Group/Groups/Group", "Groups/Group/Groups/Group/Groups/Group/Assets/Asset", "Earth"),
                    new PathRef("data/config/game/asset/objects/buildings.xml", "/Group/Groups/Group", "Groups/Group/Assets/Asset", "Arctic"),
                    new PathRef("data/config/game/asset/objects/buildings.xml", "/Group/Groups/Group", "Groups/Group/Groups/Group/Assets/Asset", "Arctic"),
                    new PathRef("data/config/game/asset/objects/buildings.xml", "/Group/Groups/Group", "Groups/Group/Groups/Group/Groups/Group/Assets/Asset", "Arctic"),
                    new PathRef("data/config/game/asset/objects/buildings.xml", "/Group/Groups/Group", "Groups/Group/Assets/Asset", "Moon"),
                    new PathRef("data/config/game/asset/objects/buildings.xml", "/Group/Groups/Group", "Groups/Group/Groups/Group/Assets/Asset", "Moon"),
                    new PathRef("data/config/game/asset/objects/buildings.xml", "/Group/Groups/Group", "Groups/Group/Groups/Group/Groups/Group/Assets/Asset", "Moon"),
                    #endregion
                    #region Data Structure DLC 01 (Tundra)
                    new PathRef("data/dlc01/config/game/asset/objects.xml", "/Group/Groups/Group", "Groups/Group/Groups/Group/Assets/Asset", "Buildings"),
                    new PathRef("data/dlc01/config/game/asset/objects.xml", "/Group/Groups/Group", "Groups/Group/Groups/Group/Groups/Group/Assets/Asset", "Buildings"),
                    new PathRef("data/dlc01/config/game/asset/objects.xml", "/Group/Groups/Group", "Groups/Group/Groups/Group/Groups/Group/Groups/Group/Assets/Asset", "Buildings"),
                    #endregion
                    #region Data Structure DLC 02 (Orbit)
                    new PathRef("data/dlc02/config/game/asset/objects.xml", "/Group/Groups/Group", "Groups/Group/Groups/Group/Assets/Asset", "Buildings"),
                    #endregion
                    #region Data Structure DLC 03 (Frontiers) (and DCL 04, no data groups)
                    new PathRef("data/dlc03/config/game/asset/objects.xml", "/Group/Groups/Group", "Groups/Group/Groups/Group/Assets/Asset", "Buildings"),
                    new PathRef("data/dlc03/config/game/asset/objects.xml", "/Group/Groups/Group", "Groups/Group/Groups/Group/Groups/Group/Assets/Asset", "Buildings"),
                    //DLC 04 does not contain any building information that goes into the preset
                    #endregion
                    #region Data Structure FCP 01 / 02 / 02b
                    new PathRef("data/fcp01/config/game/asset/objects.xml", "/Group/Groups/Group", "Groups/Group/Groups/Group/Assets/Asset", "Buildings"),
                    new PathRef("data/fcp02/config/game/asset/objects.xml", "/Group/Groups/Group", "Groups/Group/Groups/Group/Assets/Asset", "Buildings"),
                    new PathRef("data/fcp02b/config/game/asset/objects.xml", "/Group/Groups/Group", "Groups/Group/Groups/Group/Assets/Asset", "Buildings")
                    #endregion
                });
            }
            #endregion

            #region Anno 1800 xPaths
            if (annoVersion == Constants.ANNO_VERSION_1800 || annoVersion == "-ALL")
            {
                VersionSpecificPaths.Add(Constants.ANNO_VERSION_1800, new Dictionary<string, PathRef[]>());
                /// Trying to read data from the objects.exm 
                Console.WriteLine();
                Console.WriteLine("Trying to read Buildings Data from the objects.xml of anno 1800");
                /// I have removed the lat pathname 'Values' as it does the same i wanted, 
                /// only the 'Values' will skip the <template> tag that i still need
                VersionSpecificPaths[Constants.ANNO_VERSION_1800].Add("assets", new PathRef[]
                {
                    new PathRef("data/config/export/main/asset/assets.xml", "AssetList/Groups/Group/Groups/Group/Groups/Group/Assets/Asset"),
                    new PathRef("data/config/export/main/asset/assets.xml", "AssetList/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Assets/Asset"),
                    new PathRef("data/config/export/main/asset/assets.xml", "AssetList/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Assets/Asset"),
                    new PathRef("data/config/export/main/asset/assets.xml", "AssetList/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Assets/Asset"),
                    new PathRef("data/config/export/main/asset/assets.xml", "AssetList/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Assets/Asset")
                });
            }
            #endregion

            #endregion

            #region Prepare JSON Files
            if (annoVersion != "-ALL")
            {
                //execute a Signle Anno Preset
                DoAnnoPreset(annoVersion, addRoads: true);
            }
            else
            {
                //Execute for all Anno Presets in one
                Console.WriteLine();
                Console.WriteLine("----------------------------------------------");
                Console.WriteLine("Reading RDA data from {0} for anno version {1}.", BASE_PATH_1404, Constants.ANNO_VERSION_1404);
                BASE_PATH = BASE_PATH_1404;
                DoAnnoPreset(Constants.ANNO_VERSION_1404, addRoads: false);
                annoBuildingLists.Clear();
                Console.WriteLine();
                Console.WriteLine("----------------------------------------------");
                Console.WriteLine("Reading RDA data from {0} for anno version {1}.", BASE_PATH_2070, Constants.ANNO_VERSION_2070);
                BASE_PATH = BASE_PATH_2070;
                DoAnnoPreset(Constants.ANNO_VERSION_2070, addRoads: false);
                annoBuildingLists.Clear();
                Console.WriteLine();
                Console.WriteLine("----------------------------------------------");
                Console.WriteLine("Reading RDA data from {0} for anno version {1}.", BASE_PATH_2205, Constants.ANNO_VERSION_2205);
                BASE_PATH = BASE_PATH_2205;
                DoAnnoPreset(Constants.ANNO_VERSION_2205, addRoads: false);
                annoBuildingLists.Clear();
                Console.WriteLine();
                Console.WriteLine("----------------------------------------------");
                Console.WriteLine("Reading RDA data from {0} for anno version {1}.", BASE_PATH_1800, Constants.ANNO_VERSION_1800);
                BASE_PATH = BASE_PATH_1800;
                DoAnnoPreset(Constants.ANNO_VERSION_1800, addRoads: true);
                annoBuildingLists.Clear();
            }
            #endregion

            #region Write preset.json and icon.json files
            BuildingPresets presets = new BuildingPresets() { Version = BUILDING_PRESETS_VERSION, Buildings = buildings.Cast<BuildingInfo>().ToList() };

            Console.WriteLine();
            if (!testVersion)
            {
                Console.WriteLine("Writing buildings to presets-{0}-{1}.json", annoVersion, BUILDING_PRESETS_VERSION);
                SerializationHelper.SaveToFile(presets, "presets-Anno" + annoVersion + "-v" + BUILDING_PRESETS_VERSION + ".json");
            }
            else
            {
                Console.WriteLine("THIS IS A TEST DUMMY FILE WRITEN!!!!");
                SerializationHelper.SaveToFile(presets, "DUMMY.json");
            }
            // wait for keypress before exiting
            Console.WriteLine("This list contains {0} Buildings", annoBuildingsListCount);
            Console.WriteLine();
            Console.WriteLine("Do not forget to copy the contents to the normal");
            Console.WriteLine("presets.json, in the Anno Designer directory!");
            Console.WriteLine();
            Console.WriteLine("DONE - press enter to exit");
            Console.ReadLine();
            #endregion //End Prepare JSON Files
        }

        // Get the BASE_PATH for the given Anno
        #region Asking Directory path for the choiced Anno versions
        public static string GetBASE_PATH(string annoVersion)
        {
            bool validPath = false;
            string path = "";
            while (!validPath)
            {
                Console.WriteLine();
                Console.Write("Please enter the path to the extracted Anno {0} RDA files:", annoVersion);
                path = Console.ReadLine();
                if (path == "quit")
                {
                    Environment.Exit(0);
                }
                if (Directory.Exists(path))
                {
                    validPath = true;
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine("Invalid input, please try again or enter 'quit' to exit.");
                }
            }
            ///Add a trailing backslash if one is not present.
            return path.LastOrDefault() == '\\' ? path : path + "\\";
        }
        #endregion

        // Prepare building list for preset/icon file
        #region Prepare buildings list for presets, depending on the Anno Version thats given
        private static void DoAnnoPreset(string annoVersion, bool addRoads)
        {
            Console.WriteLine();
            Console.WriteLine("Parsing assets.xml:");
            var assetPathRefs = VersionSpecificPaths[annoVersion]["assets"];

            if (annoVersion == Constants.ANNO_VERSION_1404 || annoVersion == Constants.ANNO_VERSION_2070)
            {
                // prepare localizations
                // This call will set the extra Anno Version Number on the icon translation for Anno 1404 ('A4_')
                // and Anno 2070 ('A5_') that will be seen in the Icons Selection tree of the program (icons.json)
                var localizations = _localizationHelper.GetLocalization(annoVersion,
                    addPrefix: true,
                    VersionSpecificPaths,
                    Languages,
                    BASE_PATH);

                #region Preparing icon.json file

                // prepare icon mapping
                XmlDocument iconsDocument = new XmlDocument();
                List<XmlNode> iconNodes = new List<XmlNode>();
                foreach (PathRef p in VersionSpecificPaths[annoVersion]["icons"])
                {
                    iconsDocument.Load(BASE_PATH + p.Path);
                    iconNodes.AddRange(iconsDocument.SelectNodes(p.XPath).Cast<XmlNode>());
                }

                // write icon name mapping
                Console.WriteLine("Writing icon name mapping to icons.json");
                var iconNameMapper = new IconNameMapper();
                iconNameMapper.WriteIconNameMapping(_iconFileNameHelper, testVersion, iconNodes, localizations, annoVersion, BUILDING_PRESETS_VERSION);

                //This must be done, to clear the 'A4_' or 'A5_' and get normal translation for presets.json
                localizations.Clear();
                localizations = _localizationHelper.GetLocalization(annoVersion,
                    addPrefix: false,
                    VersionSpecificPaths,
                    Languages,
                    BASE_PATH);

                #endregion

                // parse buildings
                // find buildings in assets.xml
                foreach (PathRef p in assetPathRefs)
                {
                    ParseAssetsFile(BASE_PATH + p.Path, p.XPath, p.YPath, buildings, iconNodes, localizations, p.InnerNameTag, annoVersion);
                }

                // Add extra buildings to the anno version preset file
                AddExtraPreset(annoVersion, buildings);
                if (addRoads)
                {
                    AddExtraRoads(buildings);
                }
            }
            else if (annoVersion == Constants.ANNO_VERSION_2205)
            {
                foreach (PathRef p in assetPathRefs)
                {
                    ParseAssetsFile2205(BASE_PATH + p.Path, p.XPath, p.YPath, buildings, p.InnerNameTag, annoVersion);
                }
                // Add extra buildings to the anno version preset file
                AddExtraPreset(annoVersion, buildings);
                if (addRoads)
                {
                    AddExtraRoads(buildings);
                }
            }
            else if (annoVersion == Constants.ANNO_VERSION_1800)
            {
                foreach (PathRef p in assetPathRefs)
                {
                    ParseAssetsFile1800(BASE_PATH + p.Path, p.XPath, buildings);
                }
                // Add extra buildings to the anno version preset file
                AddExtraPreset(annoVersion, buildings);
                // Whaterver Annoversion is "-ALL" or "1800", add the Extra Roads Bars 
                if (addRoads)
                {
                    AddExtraRoads(buildings);
                }
            }
        }

        #endregion

        #region Add extra preset buildings to the Anno version preset file       

        /// <summary>
        /// Add some extra presets for a specific version of Anno.
        /// </summary>
        /// <param name="annoVersion">The version of Anno.</param>
        /// <param name="buildings">The already existing buildings.</param>
        private static void AddExtraPreset(string annoVersion, List<IBuildingInfo> buildings)
        {
            foreach (var curExtraPreset in ExtraPresets.GetExtraPresets(annoVersion))
            {
                IBuildingInfo buildingToAdd = new BuildingInfo
                {
                    Header = curExtraPreset.Header,
                    Faction = curExtraPreset.Faction,
                    Group = curExtraPreset.Group,
                    IconFileName = curExtraPreset.IconFileName,
                    Identifier = curExtraPreset.Identifier,
                    InfluenceRadius = curExtraPreset.InfluenceRadius,
                    InfluenceRange = curExtraPreset.InfluenceRange,
                    Template = curExtraPreset.Template,
                    Road = false,
                    Borderless = false,
                };

                Console.WriteLine("Extra Building : {0}", buildingToAdd.Identifier);

                buildingToAdd.BuildBlocker = new SerializableDictionary<int>();
                buildingToAdd.BuildBlocker["x"] = curExtraPreset.BuildBlockerX;
                buildingToAdd.BuildBlocker["z"] = curExtraPreset.BuildBlockerZ;

                buildingToAdd.Localization = new SerializableDictionary<string>();
                buildingToAdd.Localization["eng"] = curExtraPreset.LocaEng;
                buildingToAdd.Localization["ger"] = curExtraPreset.LocaGer;
                buildingToAdd.Localization["fra"] = curExtraPreset.LocaFra;
                buildingToAdd.Localization["pol"] = curExtraPreset.LocaPol;
                buildingToAdd.Localization["rus"] = curExtraPreset.LocaRus;

                annoBuildingsListCount++;

                buildings.Add(buildingToAdd);
            }
        }

        private static void AddExtraRoads(List<IBuildingInfo> buildings)
        {
            foreach (var curExtraRoad in ExtraPresets.GetExtraRoads())
            {
                IBuildingInfo buildingToAdd = new BuildingInfo
                {
                    Header = curExtraRoad.Header,
                    Faction = curExtraRoad.Faction,
                    Group = curExtraRoad.Group,
                    IconFileName = curExtraRoad.IconFileName,
                    Identifier = curExtraRoad.Identifier,
                    InfluenceRadius = curExtraRoad.InfluenceRadius,
                    InfluenceRange = curExtraRoad.InfluenceRange,
                    Template = curExtraRoad.Template,
                    Road = curExtraRoad.Road,
                    Borderless = curExtraRoad.Borderless,
                };

                Console.WriteLine("Extra Road Bar : {0}", buildingToAdd.Identifier);

                buildingToAdd.BuildBlocker = new SerializableDictionary<int>();
                buildingToAdd.BuildBlocker["x"] = curExtraRoad.BuildBlockerX;
                buildingToAdd.BuildBlocker["z"] = curExtraRoad.BuildBlockerZ;

                buildingToAdd.Localization = new SerializableDictionary<string>();
                buildingToAdd.Localization["eng"] = curExtraRoad.LocaEng;
                buildingToAdd.Localization["ger"] = curExtraRoad.LocaGer;
                buildingToAdd.Localization["fra"] = curExtraRoad.LocaFra;
                buildingToAdd.Localization["pol"] = curExtraRoad.LocaPol;
                buildingToAdd.Localization["rus"] = curExtraRoad.LocaRus;

                annoBuildingsListCount++;

                buildings.Add(buildingToAdd);
            }
        }

        #endregion

        #region Parsing Buildngs for Anno 1404/2070

        private static void ParseAssetsFile(string filename, string xPathToBuildingsNode, string YPath, List<IBuildingInfo> buildings,
            IEnumerable<XmlNode> iconNodes, Dictionary<string, SerializableDictionary<string>> localizations, string innerNameTag, string annoVersion)
        {
            XmlDocument assetsDocument = new XmlDocument();
            assetsDocument.Load(filename);
            XmlNode buildingNodes = assetsDocument.SelectNodes(xPathToBuildingsNode)
                .Cast<XmlNode>().Single(_ => _["Name"].InnerText == innerNameTag); //This differs between anno versions
            foreach (XmlNode buildingNode in buildingNodes.SelectNodes(YPath).Cast<XmlNode>())
            {
                ParseBuilding(buildings, buildingNode, iconNodes, localizations, annoVersion);
            }
        }

        private static void ParseBuilding(List<IBuildingInfo> buildings, XmlNode buildingNode, IEnumerable<XmlNode> iconNodes, Dictionary<string, SerializableDictionary<string>> localizations, string annoVersion)
        {
            // skip invalid elements
            if (buildingNode["Template"] == null)
            {
                return;
            }

            #region Get valid Building Information

            var values = buildingNode["Values"];
            var nameValue = values["Standard"]["Name"].InnerText;
            var templateValue = buildingNode["Template"].InnerText;

            #region Skip Unused buildings in Anno Designer List

            var isExcludedFaction = false;

            if (annoVersion == Constants.ANNO_VERSION_1404)
            {
                isExcludedName = nameValue.Contains(ExcludeNameList1404);
                isExcludedTemplate = templateValue.Contains(ExcludeTemplateList1404);
            }
            else if (annoVersion == Constants.ANNO_VERSION_2070)
            {
                isExcludedName = nameValue.Contains(ExcludeNameList2070);
                isExcludedTemplate = templateValue.Contains(ExcludeTemplateList2070);

                string factionValue = buildingNode.ParentNode.ParentNode.ParentNode.ParentNode["Name"].InnerText;
                isExcludedFaction = factionValue.Contains(ExcludeFactionList2070);
            }

            if (isExcludedName || isExcludedTemplate || isExcludedFaction)
            {
                return;
            }

            #endregion

            #region Skip Double Database Buildings

            if (nameValue != "underwater markethouse")
            {
                isExcludedName = nameValue.IsPartOf(annoBuildingLists);
                if (isExcludedName)
                {
                    return;
                }
            }

            var identifierName = values["Standard"]["Name"].InnerText;

            if (nameValue == "underwater markethouse" && nameValue.IsPartOf(annoBuildingLists))
            {
                identifierName = "underwater markethouse II";
            }

            #endregion

            // Parse Stuff
            var factionName = buildingNode.ParentNode.ParentNode.ParentNode.ParentNode["Name"].InnerText.FirstCharToUpper();
            var groupName = buildingNode.ParentNode.ParentNode["Name"].InnerText.FirstCharToUpper();

            #region Regrouping several faction and group names

            if (annoVersion == Constants.ANNO_VERSION_1404)
            {
                if (factionName == "Farm") { factionName = "Production"; }
                if (identifierName == "Hospice") { factionName = "Public"; groupName = "Special"; }
            }
            else if (annoVersion == Constants.ANNO_VERSION_2070)
            {
                if (factionName == "Ecos") { factionName = "(1) Ecos"; }
                if (factionName == "Tycoons") { factionName = "(2) Tycoons"; }
                if (factionName == "Techs") { factionName = "(3) Techs"; }
                if (factionName == "(3) Techs" && identifierName == "underwater markethouse II") { factionName = "Others"; }
                if (identifierName == "techs_academy") { groupName = "Public"; }
                if (identifierName == "vineyard") { identifierName = "A5_vineyard"; }
                if (groupName == "Farmfields" || groupName == "Farmfield") { groupName = "Farm Fields"; }
                if (factionName == "Others" && identifierName.Contains("black_smoker_miner") == true) { groupName = "Black Smokers (Normal)"; }
                if (factionName == "(3) Techs" && identifierName == "black_smoker_miner_platinum")
                {
                    factionName = "Others";
                    groupName = "Black Smokers (Deep Sea)";
                }
            }

            #endregion

            #region Set Header Name 

            var headerName = "Anno" + annoVersion;//in case of if statements are passed by

            if (annoVersion == Constants.ANNO_VERSION_1404)
            {
                headerName = "(A4) Anno " + Constants.ANNO_VERSION_1404;
            }
            else if (annoVersion == Constants.ANNO_VERSION_2070)
            {
                headerName = "(A5) Anno " + Constants.ANNO_VERSION_2070;
            }

            #endregion

            IBuildingInfo b = new BuildingInfo
            {
                Header = headerName,
                Faction = factionName,
                Group = groupName,
                Template = templateValue,
                Identifier = identifierName,
            };

            // print progress
            Console.WriteLine(b.Identifier);

            #endregion

            #region Get/Set InfluenceRange information

            //because this number does not exist yet, we set this to zero
            b.InfluenceRange = 0;

            #endregion

            // parse building blocker
            if (!_buildingBlockProvider.GetBuildingBlocker(BASE_PATH, b, values["Object"]["Variations"].FirstChild["Filename"].InnerText, annoVersion))
            {
                return;
            }

            #region Get IconFilename from icons.xml

            // find icon node based on guid match
            string buildingGuid = values["Standard"]["GUID"].InnerText;
            XmlNode icon = iconNodes.FirstOrDefault(_ => _["GUID"].InnerText == buildingGuid);
            if (icon != null)
            {
                b.IconFileName = _iconFileNameHelper.GetIconFilename(icon["Icons"].FirstChild, annoVersion);
            }

            #endregion
            //get Influence Radius if existing
            try
            {
                b.InfluenceRadius = Convert.ToInt32(values["Influence"]?["InfluenceRadius"]?.InnerText);
            }
            catch (NullReferenceException)
            { }

            #region Get Translations for Building Names

            if (localizations.ContainsKey(buildingGuid))
            {
                b.Localization = localizations[buildingGuid];
            }
            else
            {
                Console.WriteLine("No Translation found, it will be set to Identifier.");

                b.Localization = new SerializableDictionary<string>();

                int languageCount = 0;
                string translation = values["Standard"]["Name"].InnerText;//TODO use identifierName?

                //Anno 2070 need some special translations
                if (translation == "former_balance_ecos")
                {
                    translation = "Guardian 1.0";
                }
                else if (translation == "former_balance_techs")
                {
                    translation = "Keeper 1.0";
                }
                else if (translation == "oil_driller_variation_Sokow")
                {
                    translation = "Oil Driller Sokow Transnational";
                }

                foreach (string Language in Languages)
                {
                    b.Localization.Dict.Add(Languages[languageCount], translation);
                    languageCount++;
                }
            }

            #endregion

            // add building to the list(s)
            annoBuildingsListCount++;
            annoBuildingLists.Add(values["Standard"]["Name"].InnerText);//TODO use identifierName?
            buildings.Add(b);
        }

        #endregion

        #region Parsing Buildngs for Anno 2205

        private static void ParseAssetsFile2205(string filename, string xPathToBuildingsNode, string YPath, List<IBuildingInfo> buildings, string innerNameTag, string annoVersion)
        {
            XmlDocument assetsDocument = new XmlDocument();
            assetsDocument.Load(filename);
            XmlNode buildingNodes = assetsDocument.SelectNodes(xPathToBuildingsNode)
                .Cast<XmlNode>().Single(_ => _["Name"].InnerText == innerNameTag); //This differs between anno versions
            foreach (XmlNode buildingNode in buildingNodes.SelectNodes(YPath).Cast<XmlNode>())
            {
                ParseBuilding2205(buildings, buildingNode, Constants.ANNO_VERSION_2205);
            }
        }

        private static void ParseBuilding2205(List<IBuildingInfo> buildings, XmlNode buildingNode, string annoVersion)
        {
            // skip invalid elements
            if (buildingNode["Template"] == null)
            {
                return;
            }

            #region Get valid Building Information 

            var values = buildingNode["Values"];
            var nameValue = values["Standard"]["Name"].InnerText;
            var templateValue = buildingNode["Template"].InnerText;

            #region Skip Unused buildings in Anno Designer List

            isExcludedName = nameValue.Contains(ExcludeNameList2205);
            isExcludedTemplate = templateValue.Contains(ExcludeTemplateList2205);
            if (isExcludedName || isExcludedTemplate)
            {
                return;
            }

            #endregion

            #region Skip Double Database Buildings

            isExcludedName = nameValue.IsPartOf(annoBuildingLists);
            if (isExcludedName)
            {
                return;
            }

            #endregion

            string buildingGuid = values["Standard"]["GUID"].InnerText;

            #region TEST SECTION OF GUID CHECK

            if (!testVersion && buildingGuid.Contains(ExcludeGUIDList2205))
            {
                return;
            }
            else
            {
                if (testVersion)
                {
                    if (printTestText == 0)
                    {
                        Console.WriteLine();
                        Console.WriteLine("Testing GUID Result :");
                        printTestText = 1;
                    }

                    if (buildingGuid.Contains(ExcludeGUIDList2205))
                    {
                        Console.WriteLine("GUID : {0} (Checked GUID)", buildingGuid);
                        Console.WriteLine("Name : {0}", nameValue);
                    }
                    else
                    {
                        Console.WriteLine("GUID : {0} <<-- NOT IN GUID CHECK", buildingGuid);
                        Console.WriteLine("Name : {0}", nameValue);
                    }
                }
            }

            #endregion

            // parse stuff
            var identifierName = values["Standard"]["Name"].InnerText;

            var factionName = buildingNode.ParentNode.ParentNode.ParentNode.ParentNode["Name"].InnerText.FirstCharToUpper();
            var groupName = buildingNode.ParentNode.ParentNode["Name"].InnerText.FirstCharToUpper();

            #region Regrouping several faction or group names

            switch (factionName)
            {
                case "Earth": factionName = "(1) Earth"; break;
                case "Arctic": factionName = "(2) Arctic"; break;
                case "Moon": factionName = "(3) Moon"; break;
                case "Tundra": factionName = "(4) Tundra"; break;
                case "Orbit": factionName = "(5) Orbit"; break;
            }

            if (identifierName == "orbit connection 01") { groupName = "Special"; }

            #endregion

            var headerName = "(A6) Anno " + Constants.ANNO_VERSION_2205;

            IBuildingInfo b = new BuildingInfo
            {
                Header = headerName,
                Faction = factionName,
                Group = groupName,
                Template = templateValue,
                Identifier = identifierName
            };

            // print progress
            if (!testVersion)
            {
                Console.WriteLine(b.Identifier);
            }

            #endregion

            #region Get/Set InfluenceRange information

            // New 29-09-2020 : Head shield generation into radius parameter, on request #296
            // Read the xml key : <ShieldGenerator> / <ShieldedRadius> for heating arctic buildings (raw number)
            //                    and Moon Shield Generators
            b.InfluenceRadius = Convert.ToInt32(values?["ShieldGenerator"]?["ShieldedRadius"]?.InnerText);
            // read the xml key : <Energy> / <RadiusUsed> and then devide by 4096 for the training centers
            if (string.IsNullOrEmpty(Convert.ToString(b.InfluenceRadius)) || b.InfluenceRadius == 0)
            {
                b.InfluenceRadius = (Convert.ToInt32(values?["Energy"]?["RadiusUsed"]?.InnerText) / 4096);
            }
            // Set influenceRadius to 0, if it is still Null/Empty
            if (string.IsNullOrEmpty(Convert.ToString(b.InfluenceRadius)))
            {
                b.InfluenceRadius = 0;
            }
            #endregion

            #region Get BuildBlockers information

            //Get building blocker
            if (values["Object"] != null)
            {
                if (values["Object"]?["Variations"]?.FirstChild["Filename"]?.InnerText != null)
                {
                    if (!_buildingBlockProvider.GetBuildingBlocker(BASE_PATH, b, values["Object"]["Variations"].FirstChild["Filename"].InnerText, annoVersion))
                    {
                        return;
                    }
                }
                else
                {
                    Console.WriteLine("-BuildBlocker not found, skipping: Missing Object File");
                    return;
                }
            }
            else
            {
                Console.WriteLine("-BuildBlocker not found, skipping: Object Informaion not fount");
                return;
            }

            #endregion

            #region Get IconFilenames

            // find icon node in values (diverent vs 1404/2070)
            string icon = null;
            if (values["Standard"]?["IconFilename"]?.InnerText != null)
            {
                icon = values["Standard"]["IconFilename"].InnerText;
            }

            if (icon != null)
            {
                // Split the Value <IconFilenames>innertext</IconFilenames> to get only the Name.png
                b.IconFileName = icon.Split('/').LastOrDefault().Replace("icon_", "A6_");
            }
            else
            {
                b.IconFileName = null;
            }

            #endregion

            #region Get localizations            

            string languageFileName = ""; // This will be given thru the static LanguagesFiles array
            string languageFilePath = "data/config/gui/";
            string languageFileStart = "texts_";
            string langNodeStartPath = "/TextExport/Texts/Text";
            string langNodeDepth = "Text";
            int languageCount = 0;
            var languages2205 = LanguagesFiles2205;

            //Initialise the dictionary
            b.Localization = new SerializableDictionary<string>();

            foreach (string Language in Languages)
            {
                languageFileName = BASE_PATH + languageFilePath + languageFileStart + languages2205[languageCount] + ".xml";
                XmlDocument langDocument = new XmlDocument();
                langDocument.Load(languageFileName);
                string translation = "";
                XmlNode translationNodes = langDocument.SelectNodes(langNodeStartPath)
                    .Cast<XmlNode>().SingleOrDefault(_ => _["GUID"].InnerText == buildingGuid);
                if (translationNodes != null)
                {
                    translation = translationNodes?.SelectNodes(langNodeDepth)?.Item(0).InnerText;
                    if (buildingGuid == "7000422")
                    {
                        if (languageCount == 0) { translation = "Storage Depot (4x4)"; }
                        if (languageCount == 1) { translation = "Lager (4x4)"; }
                        if (languageCount == 2) { translation = "Magazyn (4x4)"; }
                        if (languageCount == 3) { translation = "Хранилище (4x4)"; }
                    }
                    if (buildingGuid == "7000426")
                    {
                        if (languageCount == 0) { translation = "Storage Depot (2x2)"; }
                        if (languageCount == 1) { translation = "Lager (2x2)"; }
                        if (languageCount == 2) { translation = "Magazyn (2x2)"; }
                        if (languageCount == 3) { translation = "Хранилище (2x2)"; }
                    }
                    if (translation == null)
                    {
                        throw new InvalidOperationException("Cannot get translation, text node not found");
                    }

                    while (translation.Contains("GUIDNAME"))
                    {
                        //"[GUIDNAME 2001009]",
                        //remove the [ and ] marking the GUID, and remove the GUIDNAME identifier.
                        string nextGuid = translation.Substring(1, translation.Length - 2).Replace("GUIDNAME", "").Trim();
                        translationNodes = langDocument.SelectNodes(langNodeStartPath)
                            .Cast<XmlNode>().SingleOrDefault(_ => _["GUID"].InnerText == nextGuid);
                        translation = translationNodes?.SelectNodes(langNodeDepth)?.Item(0).InnerText;
                    }
                }
                else
                {
                    if (languageCount < 1) /*just set the text one time */
                    {
                        Console.WriteLine("No Translation found, it will set to Identifier.");
                    }

                    translation = values["Standard"]["Name"].InnerText;
                }

                b.Localization.Dict.Add(Languages[languageCount], translation);

                if (testVersion && languageCount == 0)
                {
                    Console.WriteLine("ENG name: {0}", translation);

                    if (translation.IsPartOf(testGUIDNames2205))
                    {
                        Console.WriteLine(">>------------------------------------------------------------------------<<");
                        Console.ReadKey();

                        if (buildingGuid.Contains(ExcludeGUIDList2205))
                        {
                            return;
                        }
                    }
                }

                languageCount++;
            }

            #endregion

            // add building to the list
            annoBuildingsListCount++;
            annoBuildingLists.Add(values["Standard"]["Name"].InnerText);
            buildings.Add(b);
        }

        #endregion

        #region Parsing Buildings for Anno 1800

        private static void ParseAssetsFile1800(string filename, string xPathToBuildingsNode, List<IBuildingInfo> buildings)
        {
            XmlDocument assetsDocument = new XmlDocument();
            assetsDocument.Load(filename);
            List<XmlNode> buildingNodes = assetsDocument.SelectNodes(xPathToBuildingsNode).Cast<XmlNode>().ToList();

            foreach (XmlNode buildingNode in buildingNodes)
            {
                ParseBuilding1800(buildings, buildingNode, Constants.ANNO_VERSION_1800);
            }
        }

        private static void ParseBuilding1800(List<IBuildingInfo> buildings, XmlNode buildingNode, string annoVersion)
        {
            string[] LanguagesFiles = { "" };
            string templateName = "";
            string factionName = "";
            string identifierName = "";
            string groupName = "";
            string headerName = "(A7) Anno " + Constants.ANNO_VERSION_1800;

            #region Get valid Building Information 

            XmlElement values = buildingNode["Values"]; //Set the value List as normaly
            if (!buildingNode.HasChildNodes)
            {
                //Go next when no firstValue<ChildNames> are found
                return;
            }
            else
            {
                for (int i = 0; i < buildingNode.ChildNodes.Count; i++)
                {
                    string firstChildName = buildingNode.ChildNodes[i].Name;
                    //if (firstChildName != "") { Console.WriteLine("--> {0}", firstChildName); }
                    switch (firstChildName)
                    {
                        case "BaseAssetGUID": templateName = buildingNode["BaseAssetGUID"].InnerText; break;
                        case "Template": templateName = buildingNode["Template"].InnerText; break;
                    }

                    if (templateName == null)
                    {
                        Console.WriteLine("No Template found, Building is skipped");
                        return;
                    }

                    if (!templateName.Contains(IncludeBuildingsTemplateNames1800) && !templateName.Contains(IncludeBuildingsTemplateGUID1800))
                    {
                        return;
                    }

                    if (!values.HasChildNodes)
                    {
                        return;
                    }
                }
            }

            string guidName = values["Standard"]["GUID"].InnerText;
            isExcludedGUID = guidName.Contains(ExcludeBuildingsGUID1800);

            identifierName = values["Standard"]["Name"].InnerText.FirstCharToUpper();
            isExcludedName = identifierName.Contains(ExcludeNameList1800);

            if (isExcludedName || isExcludedTemplate || isExcludedGUID)
            {
                return;
            }

            // Setting the factionname, thats the first menu after header
            string associatedRegion = "";
            associatedRegion = values?["Building"]?["AssociatedRegions"]?.InnerText;
            switch (associatedRegion)
            {
                case "Moderate;Colony01": factionName = "All Worlds"; break;
                default: factionName = associatedRegion.FirstCharToUpper(); break;
            }

            if (values?["Building"]?["BuildingType"]?.InnerText != null)
            {
                groupName = values["Building"]["BuildingType"].InnerText;
            }

            if (groupName == "")
            {
                groupName = "Not Placed Yet";
            }

            //skipp double trade union, as i made the trade union manually!
            if (templateName == "1010516" && identifierName == "Guild_house_colony01")
            {
                return;
            }

            switch (templateName)
            {
                case "BuildPermitBuilding": { factionName = "Ornaments"; groupName = "13 World's Fair Rewards"; break; }
                case "Farmfield": { groupName = "Farm Fields"; break; }
                case "SlotFactoryBuilding7": { factionName = "All Worlds"; groupName = "Mining Buildings"; break; }
                case "Warehouse": { factionName = "(01) Farmers"; groupName = null; break; }
                case "HarborWarehouse7": { factionName = "Harbor"; groupName = null; break; }
                case "HarborWarehouseStrategic": { factionName = "Harbor"; groupName = "Logistics"; break; }
                case "HarborDepot": { factionName = "Harbor"; groupName = "Depots"; break; }
                case "HarborLandingStage7": { factionName = "Harbor"; groupName = null; break; }
                case "HarborBuildingAttacker": { factionName = "Harbor"; groupName = "Military"; break; }
                case "Shipyard": { factionName = "Harbor"; groupName = "Shipyards"; break; }
                case "VisitorPier": { factionName = "Harbor"; groupName = "Special Buildings"; break; }
                case "WorkforceConnector": { factionName = "Harbor"; groupName = "Special Buildings"; break; }
                case "RepairCrane": { factionName = "Harbor"; groupName = "Special Buildings"; break; }
                case "HarborOffice": { factionName = "Harbor"; groupName = "Special Buildings"; break; }
                case "PowerplantBuilding": { factionName = "Electricity"; groupName = null; break; }
                case "1010462": { templateName = "CityInstitutionBuilding"; break; }
                case "1010463": { templateName = "CityInstitutionBuilding"; break; }
                case "1010464": { templateName = "CityInstitutionBuilding"; break; }
                case "1010358": { templateName = "PublicServiceBuilding"; break; }
                case "1010359": { templateName = "PublicServiceBuilding"; break; }
                case "1010372": { templateName = "Market"; break; }
                case "1010275": { templateName = "Farmfield"; groupName = "Farm Fields"; break; }
                case "1010263": { templateName = "FarmBuilding"; break; }
                case "1010271": { templateName = "Farmfield"; groupName = "Farm Fields"; break; }
                case "1010266": { templateName = "FreeAreaBuilding"; break; }
                case "100451": { templateName = "FactoryBuilding7"; break; }
                case "1010288": { templateName = "FactoryBuilding7"; break; }
                case "1010320": { templateName = "FactoryBuilding7"; break; }
                case "101331": { templateName = "HeavyFactoryBuilding"; break; }
                case "FarmBuilding_Arctic": { templateName = "FarmBuilding"; break; }
                case "PalaceModule": { templateName = "PalaceBuilding"; factionName = "(05) Investors"; groupName = "Palace Buildings"; break; }
                case "Palace": { templateName = "PalaceBuilding"; factionName = "(05) Investors"; groupName = "Palace Buildings"; break; }
                case "PalaceMinistry": { templateName = "PalaceBuilding"; factionName = "All Worlds"; groupName = "Special Buildings"; break; }
                case "1010516": { templateName = "ArcticLodge"; factionName = "(11) Technicians"; groupName = "Special Buildings"; break; }
                case "1010517": { templateName = "SkyTradingPost"; factionName = "(11) Technicians"; groupName = "Special Buildings"; break; }
                case "FactoryBuilding7_BuildPermit": { factionName = "(13) Scholars"; groupName = "Permitted Buildings"; break;  }
                default: { groupName = templateName.FirstCharToUpper(); break; }
            }

            if (groupName == "Farm Fields")
            {
                if (factionName == "Moderate") { factionName = "(06) Old World Fields"; groupName = null; }
                if (factionName == "Colony01") { factionName = "(09) New World Fields"; groupName = null; }
                if (factionName == "Arctic") { factionName = "(12) Arctic Farm Fields"; groupName = null; }
                if (factionName == "Africa") { factionName = "(16) Enbesa Farm Fields"; groupName = null; }
            }

            //Renaming the Fuel Station for Moderate (OW) site, to avoid double listsed on Obreros tree
            if (factionName == "Moderate" && identifierName == "Fuel_station_01 (FuelStation)") { identifierName = "Moderate_fuel_station_01 (FuelStation)"; }

            switch (identifierName)
            {
                case "Silo (Grain)": { factionName = "(06) Old World Fields"; groupName = null; break; }
                case "Tractor_module_01 (Tractor)": { factionName = "(06) Old World Fields"; groupName = null; break; }
                case "Silo (Corn)": { factionName = "(09) New World Fields"; groupName = null; break; }
                case "Colony01_tractor_module_01 (Tractor)": { factionName = "(09) New World Fields"; groupName = null; break; }
                case "Africa_silo (Teff)": { factionName = "(16) Enbesa Farm Fields"; groupName = null; break; }
                case "Africa_tractor_module_01 (Tractor)": { factionName = "(16) Enbesa Farm Fields"; groupName = null; break; }
                case "Entertainment_musicpavillion_empty": { factionName = "Attractiveness"; groupName = null; break; }
                case "Culture_01 (Zoo)": { factionName = "Attractiveness"; groupName = null; break; }
                case "Culture_02 (Museum)": { factionName = "Attractiveness"; groupName = null; break; }
                case "Culture_03 (BotanicalGarden)": { factionName = "Attractiveness"; groupName = null; break; }
                case "Residence_tier01": { factionName = "(01) Farmers"; identifierName = "Residence_Old_World"; groupName = "Residence"; break; }
                case "Residence_colony01_tier01": { factionName = "(07) Jornaleros"; identifierName = "Residence_New_World"; groupName = "Residence"; templateName = "ResidenceBuilding7"; break; }
                case "Residence_arctic_tier01": { factionName = "(10) Explorers"; identifierName = "Residence_Arctic_World"; groupName = "Residence"; break; }
                case "Residence_colony02_tier01": { factionName = "(14) Shepherds"; identifierName = "Residence_Africa_World"; groupName = "Residence"; templateName = "ResidenceBuilding7"; break; }
                case "Coastal_03 (Quartz Sand Coast Building)": { factionName = "All Worlds"; groupName = "Mining Buildings"; break; }
                case "Electricity_03 (Gas Power Plant)": { factionName = "(11) Technicians"; groupName = "Public Buildings"; break; }
                case "Event_ornament_historyedition": { factionName = "Ornaments"; groupName = "11 Special Ornaments"; break; }
                case "Harbor_arctic_01 (Depot)": { factionName = "Harbor"; groupName = "Depots";break; }
            }

            // Place the rest of the buildings in the right Faction > Group menu
            #region Order the Buildings to the right tiers and factions as in the game

            var groupInfo = NewFactionAndGroup1800.GetNewFactionAndGroup1800(identifierName, factionName, groupName, templateName);
            factionName = groupInfo.Faction;
            groupName = groupInfo.Group;
            templateName = groupInfo.Template;

            #endregion

            if (factionName?.Length == 0 || factionName == "Moderate" || factionName == "Colony01" || factionName == "Arctic" || factionName == "Africa" )
            {
                factionName = "Not Placed Yet -" + factionName;
                // Because the Culture_03 (BotanicalGarden) is in the xPath that i normaly skipp, i must skipp this group here now.
                if (groupName == "CultureModule") { return; }
            }
            if (factionName == "Meta;Moderate;Colony01;Arctic;Africa")
            {
                factionName = "Not Placed Yet -All Worlds";
            }

            #region Sorting the Ornaments for the new Ornaments Menu (11/05/2020)

            //Sorting to the new menu
            groupInfo = NewOrnamentsGroup1800.GetNewOrnamentsGroup1800(identifierName, factionName, groupName, templateName);
            factionName = groupInfo.Faction;
            groupName = groupInfo.Group;
            templateName = groupInfo.Template;

            //set right Group on the ornamentals of Enbesa (just need a Faction and Group change)
            if (templateName == "OrnamentalBuilding" && factionName == "Not Placed Yet -Africa")
            {
                factionName = "Ornaments"; groupName = "21 Enbesa Ornaments";
            }
            //Set the right Colors to the Enbesa Ornament Group
            groupInfo = MapToTemplateName1800.GetNewOrnamentsGroup1800(identifierName, factionName, groupName, templateName);
            factionName = groupInfo.Faction;
            groupName = groupInfo.Group;
            templateName = groupInfo.Template;

            #endregion

            #region Temperary exclude the following OrnamentalBuildings from Presets.json
            /// The following process is to eliminate ornaments that not belong in the preset, till it is made into the game
            /// I do it here, so when it is in game, I can remove it here, so it will appear in the preset
            /// Behind the add line, I comment the English name, what it should have in game.
            if (!TempExcludeOrnamentsFromPreset_1800.Any())
            {
                TempExcludeOrnamentsFromPreset_1800.Add("City_props_system_all"); // Cityscape 
                TempExcludeOrnamentsFromPreset_1800.Add("City_prop_system_1x1_01"); // Small Square
                TempExcludeOrnamentsFromPreset_1800.Add("City_props_system_1x1_global"); // Small City Ornaments
                TempExcludeOrnamentsFromPreset_1800.Add("City_prop_system_2x2_01"); // Piazza
                TempExcludeOrnamentsFromPreset_1800.Add("City_props_system_2x2_global"); // Medium City Ornaments
                TempExcludeOrnamentsFromPreset_1800.Add("City_prop_system_3x3_01"); // Large Square
                TempExcludeOrnamentsFromPreset_1800.Add("City_props_system_3x3_global"); //Large City Ornaments
            }

            if (identifierName.IsPartOf(TempExcludeOrnamentsFromPreset_1800)) { return; };
            #endregion

            #endregion

            #region Starting buildup the preset data

            IBuildingInfo b = new BuildingInfo
            {
                Header = headerName,
                Faction = factionName,
                Group = groupName,
                Template = templateName,
                Identifier = identifierName,
            };

            // print progress
            Console.WriteLine(b.Identifier);

            #region Get BuildBlockers information

            //Get building blocker
            if (values["Object"] != null)
            {
                if (values["Object"]?["Variations"]?.FirstChild["Filename"]?.InnerText != null)
                {
                    if (!_buildingBlockProvider.GetBuildingBlocker(BASE_PATH, b, values["Object"]["Variations"].FirstChild["Filename"].InnerText, annoVersion))
                    {
                        return;
                    }
                }
                else
                {
                    Console.WriteLine("-BuildBlocker not found, skipping: Missing Object File");
                    return;
                }
            }
            else
            {
                Console.WriteLine("-BuildBlocker not found, skipping: Object Informaion not found");
                return;
            }

            #endregion

            #region Get and set new IconFilenames

            // find icon node in values
            string replaceName = "A7_";
            string icon = null;
            if (values["Standard"]?["IconFilename"]?.InnerText != null)
            {
                icon = values["Standard"]["IconFilename"].InnerText;
            }

            if (icon != null)
            {
                /// Split the Value <IconFilenames>innertext</IconFilenames> to get only the Name.png
                string[] sIcons = icon.Split('/');
                if (sIcons.LastOrDefault().StartsWith("icon_"))
                {
                    icon = sIcons.LastOrDefault().Replace("icon_", replaceName);
                }
                else /* Put the Replace name on front*/
                {
                    icon = replaceName + sIcons.LastOrDefault();
                }

                switch (guidName)
                {
                    case "102133": { icon = replaceName + "park_props_1x1_21.png"; break; } /*Change the Big Tree icon to Mature Tree icon (as in game) */
                    case "102139": { icon = replaceName + "park_props_1x1_27.png"; break; } //Path Corecting Icon
                    case "102140": { icon = replaceName + "park_props_1x1_28.png"; break; } //Path Corecting Icon
                    case "102141": { icon = replaceName + "park_props_1x1_29.png"; break; } //Path Corecting Icon
                    case "102142": { icon = replaceName + "park_props_1x1_30.png"; break; } //Path Corecting Icon
                    case "102143": { icon = replaceName + "park_props_1x1_31.png"; break; } //Path Corecting Icon 
                    case "102131": { icon = replaceName + "park_props_1x1_17.png"; break; } //Cypress corecting Icon
                    case "101284": { icon = replaceName + "community_lodge.png"; break; } //corecting Arctic Lodge Icon
                }
                switch(b.Identifier)
                {
                    case "AmusementPark CottonCandy": { icon = replaceName + "cotton_candy.png"; break; } // faulty naming fix icn_ instead of icon_
                }

                b.IconFileName = icon;
            }
            else
            {
                b.IconFileName = null;

                //Buildings that came in with the BaseAssetGUID template has no icons or just have no icon, this will fix that;
                switch (identifierName)
                {
                    case "Residence_New_World": b.IconFileName = replaceName + "resident.png"; break;
                    case "Agriculture_colony01_06 (Timber Yard)": b.IconFileName = replaceName + "wood_log.png"; break;
                    case "Factory_colony01_01 (Timber Factory)": b.IconFileName = replaceName + "wooden_planks.png"; break;
                    case "Heavy_colony01_01 (Oil Heavy Industry)": b.IconFileName = replaceName + "oil.png"; break;
                    case "Processing_colony01_03 (Inlay Processing)": b.IconFileName = replaceName + "inlay.png"; break;
                    case "Factory_colony01_02 (Sailcloth Factory)": b.IconFileName = replaceName + "sail.png"; break;
                    case "Agriculture_colony01_09 (Cattle Farm)": b.IconFileName = replaceName + "meat_raw.png"; break;
                    case "Service_colony01_01 (Marketplace)": b.IconFileName = replaceName + "market.png"; break;
                    case "Service_colony01_02 (Chapel)": b.IconFileName = replaceName + "church.png"; break;
                    case "Kontor_main_01": b.IconFileName = replaceName + "harbour_buildings.png"; break;
                    case "Institution_colony01_01 (Police)": b.IconFileName = replaceName + "police.png"; break;  //set NW Police Station Icon
                    case "Institution_colony01_02 (Fire Department)": b.IconFileName = replaceName + "fire_house.png"; break; //set NW Fire Staion Icon
                    case "Institution_colony01_03 (Hospital)": b.IconFileName = replaceName + "hospital.png"; break;  //set NW Hospital Icon
                    case "Agriculture_colony01_11_field (Alpaca Pasture)": { b.IconFileName = replaceName + "general_module_01.png"; break; }
                    case "Agriculture_colony01_09_field (Cattle Pasture)": { b.IconFileName = replaceName + "general_module_01.png"; break; }
                    case "Residence_Africa_World": { b.IconFileName = replaceName + "resident.png"; break; } //set Shepherd Residence to default resident.png (has none)
                    case "Harbor_arctic_01 (Depot)": { b.IconFileName = replaceName + "depot.png"; break; }
                    case "Institution_colony02_02 (Police)": { b.IconFileName = replaceName + "police.png"; break; } // Fix non icon Africa Police Station
                    case "Institution_colony02_03 (Hospital)": { b.IconFileName = replaceName + "hospital.png"; break; } // fix non icon for Africa Hospital 
                }
            }

            /// New process for OrnamentalBuildings(_*) only.
            /// Step 1 : Check if Ornament Name and IconFileName are both used, then skipp 
            ///          double Ornaments.
            /// Step 2 : if iconfilename is not used, and name is, rename the Identifier of the 
            ///          double Ornament, remane the identifier and place it to the right preset
            ///          menu (Faction & Group) or switch the icons if need.
            /// Setp 3 : Exclude the OrnamentalBuildings with the iconfilenames that are in the
            ///          list ExcludeOrnamentsIcons_1800
            ///          
            /// (_*) means all Ornamentalbuildings + _extraname for the Color Assignments  
            #region See comment above
            if (b.Template == "OrnamentalBuilding" || b.Template == "OrnamentalBuilding_Park" || b.Template == "OrnamentalBuilding_Industrial")
            {
                isExcludedName = identifierName.IsPartOf(annoBuildingLists);
                isExcludeIconName = b.IconFileName.IsPartOf(anno1800IconNameLists);
                if (isExcludedName && isExcludeIconName)
                {
                    Console.WriteLine("-----> Ornament Skipped, Already in preset (A)");
                    return;
                }
                //Those Identifier changes will not harm any users, as they where never in the presets before....
                //This will be done irectly to the building info structure (b)
                //Also switch wrong icons beween two objects (see issue #178)
                switch (b.IconFileName)
                {
                    case "A7_park_props_1x1_14.png": b.Identifier = "Park_1x1_statue_grass"; b.Faction = "Ornaments"; b.Group = "05 Park Statues"; break;
                    case "A7_city_2x2_03.png": b.IconFileName = "A7_city_2x2_02.png"; break; // Switch to right icon
                    case "A7_city_2x2_02.png": b.IconFileName = "A7_city_2x2_03.png"; break; // Switch to right icon
                }
                //Skip the following icons into the presett
                if (b.IconFileName.IsPartOf(ExcludeOrnamentsIcons_1800))
                {
                    Console.WriteLine("-----> Ornament Skipped, Already in preset (B)");
                    return;
                }
            }

            #endregion

            #endregion

            #region Get Infuence Radius of Buildings

            // read influence radius if existing 
            b.InfluenceRadius = Convert.ToInt32(values?["FreeAreaProductivity"]?["InfluenceRadius"]?.InnerText);

            //on Module Radius Range (Farm/Oil Refineries) 
            if (string.IsNullOrEmpty(Convert.ToString(b.InfluenceRadius)) || b.InfluenceRadius == 0)
            {
                b.InfluenceRadius = Convert.ToInt32(values?["ModuleOwner"]?["ModuleBuildRadius"]?.InnerText);
            }

            //on Item Slots (Like trade unions, town halls etc)
            if (string.IsNullOrEmpty(Convert.ToString(b.InfluenceRadius)) || b.InfluenceRadius == 0)
            {
                b.InfluenceRadius = Convert.ToInt32(values?["ItemContainer"]?["SocketScopeRadius"]?.InnerText);
            }

            switch (b.Identifier)
            {
                case "Agriculture_colony01_06 (Timber Yard)": b.InfluenceRadius = 9; break;
                case "Heavy_colony01_01 (Oil Heavy Industry)": b.InfluenceRadius = 12; break;
                case "Town hall": b.InfluenceRadius = 20; break;
                case "Guild_house_arctic": b.InfluenceRadius = 15; break;
                case "Mining_arctic_01 (Gas Mine)": b.InfluenceRadius = 10; break;
            }

            #endregion

            #region Get/Set InfluenceRange information

            b.InfluenceRange = 0;

            if (b.Template == "CityInstitutionBuilding")
            {
                b.InfluenceRange = 26; //Police - Fire stations and Hospiitals
            }
            else if (!string.IsNullOrEmpty(values?["PublicService"]?["FullSatisfactionDistance"]?.InnerText))
            {
                b.InfluenceRange = Convert.ToInt32(values["PublicService"]["FullSatisfactionDistance"].InnerText);
            }
            else
            {
                switch (identifierName)
                {
                    case "Service_colony01_03 (Boxing Arena)": b.InfluenceRange = 30; break;
                    case "Service_colony01_01 (Marketplace)": b.InfluenceRange = 35; break;
                    case "Electricity_02 (Oil Power Plant)": b.InfluenceRange = 35; break;
                }
            }

            #endregion

            #region Get localizations

            string buildingGuid = values["Standard"]["GUID"].InnerText;
            //rename the Big Tree to Mature Tree (as in game)
            if (buildingGuid == "102133")
            {
                buildingGuid = "102085";
            }

            string languageFileName = ""; // This will be given thru the static LanguagesFiles array
            string languageFilePath = "data/config/gui/";
            string languageFileStart = "texts_";
            string langNodeStartPath = "/TextExport/Texts/Text";
            string langNodeDepth = "Text";
            int languageCount = 0;
            LanguagesFiles = LanguagesFiles1800;

            //Initialise the dictionary
            b.Localization = new SerializableDictionary<string>();

            foreach (string Language in Languages)
            {
                languageFileName = BASE_PATH + languageFilePath + languageFileStart + LanguagesFiles[languageCount] + ".xml";
                XmlDocument langDocument = new XmlDocument();
                langDocument.Load(languageFileName);
                string translation = "";

                XmlNode translationNodes = langDocument.SelectNodes(langNodeStartPath)
                    .Cast<XmlNode>().SingleOrDefault(_ => _["GUID"].InnerText == buildingGuid);
                if (translationNodes != null)
                {
                    translation = translationNodes?.SelectNodes(langNodeDepth)?.Item(0).InnerText;
                    if (translation == null)
                    {
                        throw new InvalidOperationException("Cannot get translation, text node not found");
                    }

                    while (translation.Contains("AssetData"))
                    {
                        //"[AsserData(2001009): <text>",
                        //Split the taranslation text ( and ) marking the GUID, and use the second value in nextGUID[].
                        string[] nextGuid = translation.Split('(', ')');
                        translationNodes = langDocument.SelectNodes(langNodeStartPath)
                            .Cast<XmlNode>().SingleOrDefault(_ => _["GUID"].InnerText == nextGuid[1]);
                        translation = translationNodes?.SelectNodes(langNodeDepth)?.Item(0).InnerText;
                    }

                    if (buildingGuid == "102165")
                    {
                        switch (languageCount)
                        {
                            case 0: { translation = "Sidewalk Hedge"; break; }
                            case 1: { translation = "Gehweg Hecke"; break; }
                            case 2: { translation = "Haies de trottoirs"; break; }
                            case 3: { translation = "Żywopłot Chodnikowy"; break; }
                            case 4: { translation = "Боковая изгородь"; break; }
                        }
                    }
                    else if (buildingGuid == "102166")
                    {
                        switch (languageCount)
                        {
                            case 0: { translation = "Sidewalk Hedge Corner"; break; }
                            case 1: { translation = "Gehweg Heckenecke Ecke"; break; }
                            case 2: { translation = "Coin de haies de trottoirs"; break; }
                            case 3: { translation = "Żywopłot Chodnikowy narożnik"; break; }
                            case 4: { translation = "Боковая изгородь (угол)"; break; }
                        }
                    }
                    else if (buildingGuid == "102167")
                    {
                        switch (languageCount)
                        {
                            case 0: { translation = "Sidewalk Hedge End"; break; }
                            case 1: { translation = "Gehweg Heckenende"; break; }
                            case 2: { translation = "Extrémité de haie de trottoir"; break; }
                            case 3: { translation = "Żywopłot Chodnikowy Koniec"; break; }
                            case 4: { translation = "Боковая изгородь (край)"; break; }
                        }
                    }
                    else if (buildingGuid == "102169")
                    {
                        switch (languageCount)
                        {
                            case 0: { translation = "Sidewalk Hedge Junction"; break; }
                            case 1: { translation = "Gehweg Hecken Verbindungsstelle"; break; }
                            case 2: { translation = "Jonction de haie de trottoir"; break; }
                            case 3: { translation = "Żywopłot Chodnikowy Złącze"; break; }
                            case 4: { translation = "Боковая изгородь (Перекресток)"; break; }
                        }
                    }
                    else if (buildingGuid == "102171")
                    {
                        switch (languageCount)
                        {
                            case 0: { translation = "Sidewalk Hedge Crossing"; break; }
                            case 1: { translation = "Gehweg Hecken Kreuzung"; break; }
                            case 2: { translation = "Traversée de haie de trottoir"; break; }
                            case 3: { translation = "Żywopłot Chodnikowy Skrzyżowanie"; break; }
                            case 4: { translation = "Боковая изгородь (образного)"; break; }
                        }
                    }
                    else if (buildingGuid == "102161")
                    {
                        switch (languageCount)
                        {
                            case 0: { translation = "Railings"; break; }
                            case 1: { translation = "Zaune"; break; }
                            case 2: { translation = "Garde-corps"; break; }
                            case 3: { translation = "Poręcze"; break; }
                            case 4: { translation = "Ограда"; break; }
                        }
                    }
                    else if (buildingGuid == "102170")
                    {
                        switch (languageCount)
                        {
                            case 0: { translation = "Railings Junction"; break; }
                            case 1: { translation = "Zaune Verbindungsstelle"; break; }
                            case 2: { translation = "Garde-corps Jonction"; break; }
                            case 3: { translation = "Poręcze Złącze"; break; }
                            case 4: { translation = "Ограда (Перекресток)"; break; }
                        }
                    }
                    else if (buildingGuid == "102134")
                    {
                        switch (languageCount)
                        {
                            case 0: { translation = "Hedge"; break; }
                            case 1: { translation = "Hecke"; break; }
                            case 2: { translation = "Haie (droite)"; break; }
                            case 3: { translation = "żywopłot"; break; }
                            case 4: { translation = "изгородь"; break; }
                        }
                    }
                    else if (buildingGuid == "102139")
                    {
                        switch (languageCount)
                        {
                            case 0: { translation = "Path"; break; }
                            case 1: { translation = "Pfad"; break; }
                            case 2: { translation = "Allée (droite)"; break; }
                            case 3: { translation = "ścieżka"; break; }
                            case 4: { translation = "Тропинка"; break; }
                        }
                    }
                    else if (buildingGuid == "118938")
                    {
                        switch (languageCount)
                        {
                            case 0: { translation = "Research Institute"; break; }
                            case 1: { translation = "Forschungsinstitut"; break; }
                            case 2: { translation = "Institut de recherche"; break; }
                            case 3: { translation = "Instytut Badawczy"; break; }
                            case 4: { translation = "Исследовательский институт"; break; }
                        }
                    }
                    else if (buildingGuid == "112670")
                    {
                        switch (languageCount)
                        {
                            case 0: { translation = "Arctic Depot"; break; }
                            case 1: { translation = "Arktisches Depot"; break; }
                            case 2: { translation = "Dépôt de l'Arctique"; break; }
                            case 3: { translation = "Skład Arktyczny"; break; }
                            case 4: { translation = "арктическая депо"; break; }
                        }
                    }
                }
                else
                {
                    if (languageCount < 1) /*just set the text one time */
                    {
                        Console.WriteLine("No Translation found, it will set to Identifier.");
                    }

                    translation = values["Standard"]["Name"].InnerText;
                }

                if (templateName == "FarmBuilding" || templateName == "Farmfield")
                {
                    string fieldAmountValue = null;
                    string fieldGuidValue = null;

                    switch (templateName)
                    {
                        case "FarmBuilding":
                            {
                                fieldGuidValue = values["ModuleOwner"]["ConstructionOptions"]["Item"]["ModuleGUID"].InnerText;
                                fieldAmountValue = values?["ModuleOwner"]?["ModuleLimits"]?["Main"]?["Limit"]?.InnerText;
                                break;
                            };
                        case "Farmfield":
                            {
                                fieldGuidValue = values["Standard"]["GUID"].InnerText;
                                fieldAmountValue = "0";
                                break;
                            }
                    }

                    if (fieldAmountValue != null)
                    {
                        var isFieldInfoFound = false;
                        foreach (var curFieldInfo in farmFieldList1800)
                        {
                            if (string.Equals(curFieldInfo.FieldGuid, fieldGuidValue, StringComparison.OrdinalIgnoreCase))
                            {
                                isFieldInfoFound = true;
                                fieldAmountValue = curFieldInfo.FieldAmount;
                                break;
                            }
                        }

                        if (!isFieldInfoFound)
                        {
                            farmFieldList1800.Add(new FarmField() { FieldGuid = fieldGuidValue, FieldAmount = fieldAmountValue });
                        }

                        translation = translation + " - (" + fieldAmountValue + ")";
                    }
                }

                b.Localization.Dict.Add(Languages[languageCount], translation);
                languageCount++;
            }

            #endregion

            #endregion

            // Remove CultureModules Menu that Appeared
            if (b.Header == "(A7) Anno 1800" && b.Faction == "All Worlds" && b.Group == "CultureModule") { return; }

            // Remove the Not Placed Buildings
            /// commentout the line below if you make a new preset after update of the game 'ANNO 1800', or when a new 'ANNO 1800 DLC' is released 
            if (b.Faction == "Not Placed Yet -Moderate" || b.Faction == "Not Placed Yet -Arctic" || b.Faction == "Not Placed Yet -Africa" || b.Faction == "Not Placed Yet -Colony01" || b.Faction == "Not Placed Yet -All Worlds") { return; }

            // add building to the list
            annoBuildingsListCount++;//countup amount of buildings
            annoBuildingLists.Add(values["Standard"]["Name"].InnerText);//add building name to the list, for checking double building names usage
            anno1800IconNameLists.Add(b.IconFileName);//add Icon file to the list, for checking double icon file usage 
            buildings.Add(b); // add building data to file data
        }

        #endregion

    }
}
