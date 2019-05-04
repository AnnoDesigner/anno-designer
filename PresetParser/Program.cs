using AnnoDesigner;
using AnnoDesigner.Presets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public static bool isExcludedTemplate = false;
        public static bool isExcludedFaction = false; /* Only for Anno 2070 */
        public static bool isExcludedGUID = false; /*only for anno 1800 */

        private static Dictionary<string, Dictionary<string, PathRef[]>> VersionSpecificPaths { get; set; }
        public const string ANNO_VERSION_1404 = "1404";
        public const string ANNO_VERSION_2070 = "2070";
        public const string ANNO_VERSION_2205 = "2205";
        public const string ANNO_VERSION_1800 = "1800";

        private const string BUILDING_PRESETS_VERSION = "3.0.1";
        // Initalisizing Language Directory's and Filenames
        private static readonly string[] Languages = new[] { "eng", "ger", "pol", "rus" };
        private static readonly string[] LanguagesFiles2205 = new[] { "english", "german", "polish", "russian" };
        private static readonly string[] LanguagesFiles1800 = new[] { "english", "german", "polish", "russian" };
        // Internal Program Buildings List to skipp double buildings
        public static List<string> annoBuildingLists = new List<string>();
        public static int annoBuildingsListCount = 0, printTestText = 0;
        public static bool testVersion = false;
        // The internal Building list for the Preset writing 
        public static List<BuildingInfo> buildings = new List<BuildingInfo>();

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
        /// i need the IncludeBuildingsTemplateNames to get Building informaton from, as it is also the Presets Template String or Template GUID
        /// </summary>
        public static IList<FarmField> farmFieldList1800 = new List<FarmField>();
        // Removed IncludeBuildingsTemplate "CultureModule" (to must to handle and thus are replaced with the Zoo Module and Museum Module
        private static readonly List<string> IncludeBuildingsTemplateNames1800 = new List<string> { "ResidenceBuilding7", "FarmBuilding", "FreeAreaBuilding", "FactoryBuilding7", "HeavyFactoryBuilding",
            "SlotFactoryBuilding7", "Farmfield", "OilPumpBuilding", "PublicServiceBuilding", "CityInstitutionBuilding", "CultureBuilding", "Market", "Warehouse", "PowerplantBuilding",
            "HarborOffice", "HarborWarehouse7", "HarborDepot","Shipyard","HarborBuildingAttacker", "RepairCrane", "HarborLandingStage7", "VisitorPier", "WorkforceConnector", "Guildhouse", "OrnamentalBuilding"};
        private static readonly List<string> IncludeBuildingsTemplateGUID1800 = new List<string> { "100451", "1010266", "1010343", "1010288", "101331", "1010320", "1010263", "1010372", "1010359", "1010358" };
        //private static readonly List<string> ExcludeBuildingsGUID1800 = new List<string> { "102139", "102140", "102141", "102142", "102143", "102828" };
        private static readonly List<string> ExcludeNameList1800 = new List<string> { "tier02", "tier03", "tier04", "tier05", "(Wood Field)", "(Hunting Grounds)", "(Wash House)", "Quay System",
            "module_01_birds", "module_02_peacock", "(Warehouse II)", "(Warehouse III)", "logistic_colony01_01 (Warehouse I)", "Kontor_main_02", "Kontor_main_03", "kontor_main_colony01",
            "Fake Ornament [test 2nd party]", "Kontor_imperial_02", "Kontor_imperial_03","(Oil Harbor II)","(Oil Harbor III)", "Third_party_", "CQO_", "Kontor_imperial_01", "- Pirates",
            "Harbor_colony01_09 (tourism_pier_01)", "Ai_", "AarhantLighthouseFake", "CO_Tunnel_Entrance01_Fake","Park_1x1_fence"};
        /// <summary>
        /// in NewFactionAndGroup1800.cs are made the following lists
        /// ChangeBuildingTo<1>_<2>_1800 
        /// <1> can be : AW (All Worlds) - NW1 (New World - Farmers) - NW2 (New World - Workers) - NW3 (New World - Artisans)
        ///              NW4 (New World - Engineers) - NW5 (New World - Investors) - 
        ///              OW1 (Old World - Jornaleros) and OW2 (Old World - Obreros)
        /// <2> wil be the Group under <1>, like Production, Public, etc
        /// </summary>
        #endregion

        #endregion

        #region Set Icon File Name seperations
        private static string GetIconFilename(XmlNode iconNode, string annoVersion)
        {
            string annoIdexNumber = "";
            if (annoVersion == ANNO_VERSION_1404)
            {
                annoIdexNumber = "A4_";
            }
            /* For Anno 2070, we use the normal icon names, without the AnnoIndexNUmber ('A5_'),
                because anno 2070 has already the right names in previous Anno Designer versions. */
            return string.Format("{0}icon_{1}_{2}.png", annoIdexNumber, iconNode["IconFileID"].InnerText, iconNode["IconIndex"] != null ? iconNode["IconIndex"].InnerText : "0"); //TODO: check this icon format is consistent between Anno versions
        }
        #endregion

        static Program()
        {
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
                Console.Write("Please enter an Anno version (1 of: {0} {1} {2} {3}):", ANNO_VERSION_1404, ANNO_VERSION_2070, ANNO_VERSION_2205, ANNO_VERSION_1800);
                annoVersion = Console.ReadLine();
                if (annoVersion == "quit")
                {
                    Environment.Exit(0);
                }
                if (annoVersion == ANNO_VERSION_1404 || annoVersion == ANNO_VERSION_2070 || annoVersion == ANNO_VERSION_2205 || annoVersion == ANNO_VERSION_1800 || annoVersion == "-ALL")
                {
                    validVersion = true;
                }
                else if (annoVersion.ToLower() == "-test")
                {
                    Console.Write("Please enter an Anno version:");
                    annoVersion = Console.ReadLine();
                    if (annoVersion == ANNO_VERSION_1404 || annoVersion == ANNO_VERSION_2070 || annoVersion == ANNO_VERSION_2205 || annoVersion == ANNO_VERSION_1800)
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
                BASE_PATH_1404 = GetBASE_PATH(ANNO_VERSION_1404);
                BASE_PATH_2070 = GetBASE_PATH(ANNO_VERSION_2070);
                BASE_PATH_2205 = GetBASE_PATH(ANNO_VERSION_2205);
                BASE_PATH_1800 = GetBASE_PATH(ANNO_VERSION_1800);
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
            if (annoVersion == ANNO_VERSION_1404 || annoVersion == "-ALL")
            {
                VersionSpecificPaths.Add(ANNO_VERSION_1404, new Dictionary<string, PathRef[]>());
                VersionSpecificPaths[ANNO_VERSION_1404].Add("icons", new PathRef[]
                {
                //new PathRef("data/config/game/icons.xml", "/Icons/i" ),
                new PathRef("addondata/config/game/icons.xml", "/Icons/i", "", "")
                });
                VersionSpecificPaths[ANNO_VERSION_1404].Add("localisation", new PathRef[]
                {
                new PathRef("data/loca"),
                new PathRef("addondata/loca")
                });
                VersionSpecificPaths[ANNO_VERSION_1404].Add("assets", new PathRef[]
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
            if (annoVersion == ANNO_VERSION_2070 || annoVersion == "-ALL")
            {
                VersionSpecificPaths.Add(ANNO_VERSION_2070, new Dictionary<string, PathRef[]>());
                VersionSpecificPaths[ANNO_VERSION_2070].Add("icons", new PathRef[]
                {
                new PathRef("data/config/game/icons.xml", "/Icons/i", "", "")
                });
                VersionSpecificPaths[ANNO_VERSION_2070].Add("localisation", new PathRef[]
                {
                new PathRef("data/loca")
                });
                VersionSpecificPaths[ANNO_VERSION_2070].Add("assets", new PathRef[]
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
            if (annoVersion == ANNO_VERSION_2205 || annoVersion == "-ALL")
            {
                VersionSpecificPaths.Add(ANNO_VERSION_2205, new Dictionary<string, PathRef[]>());
                /// Trying to read data from the objects.exm 
                Console.WriteLine();
                Console.WriteLine("Trying to read Buildings Data from the objects.xml of anno 2205");
                VersionSpecificPaths[ANNO_VERSION_2205].Add("assets", new PathRef[]
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
            if (annoVersion == ANNO_VERSION_1800 || annoVersion == "-ALL")
            {
                VersionSpecificPaths.Add(ANNO_VERSION_1800, new Dictionary<string, PathRef[]>());
                /// Trying to read data from the objects.exm 
                Console.WriteLine();
                Console.WriteLine("Trying to read Buildings Data from the objects.xml of anno 1800");
                /// I have removed the lat pathname 'Values' as it does the same i wanted, 
                /// only the 'Values' will skip the <template> tag that i still need
                VersionSpecificPaths[ANNO_VERSION_1800].Add("assets", new PathRef[]
                {
                    new PathRef("data/config/export/main/asset/assets.xml", "AssetList/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Assets/Asset"),
                    new PathRef("data/config/export/main/asset/assets.xml", "AssetList/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Assets/Asset"),
                    //new PathRef("data/config/export/main/asset/assets.xml", "AssetList/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Assets/Asset"),
                    new PathRef("data/config/export/main/asset/assets.xml", "AssetList/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Assets/Asset")
                });
            }
            #endregion

            #endregion

            #region Prepare JSON Files
            if (annoVersion != "-ALL")
            {
                //execute a Signle Anno Preset
                DoAnnoPreset(annoVersion);
            }
            else
            {
                //Execute for all Anno Presets in one
                Console.WriteLine();
                Console.WriteLine("----------------------------------------------");
                Console.WriteLine("Tesing RDA data from {0} for anno version {1}.", BASE_PATH_1404, ANNO_VERSION_1404);
                BASE_PATH = BASE_PATH_1404;
                DoAnnoPreset(ANNO_VERSION_1404);
                Console.WriteLine();
                Console.WriteLine("----------------------------------------------");
                Console.WriteLine("Tesing RDA data from {0} for anno version {1}.", BASE_PATH_2070, ANNO_VERSION_2070);
                BASE_PATH = BASE_PATH_2070;
                DoAnnoPreset(ANNO_VERSION_2070);
                Console.WriteLine();
                Console.WriteLine("----------------------------------------------");
                Console.WriteLine("Tesing RDA data from {0} for anno version {1}.", BASE_PATH_2205, ANNO_VERSION_2205);
                BASE_PATH = BASE_PATH_2205;
                DoAnnoPreset(ANNO_VERSION_2205);
                Console.WriteLine();
                Console.WriteLine("----------------------------------------------");
                Console.WriteLine("Tesing RDA data from {0} for anno version {1}.", BASE_PATH_1800, ANNO_VERSION_1800);
                BASE_PATH = BASE_PATH_1800;
                DoAnnoPreset(ANNO_VERSION_1800);
            }
            #endregion

            #region Write preset.json and icon.json files
            BuildingPresets presets = new BuildingPresets() { Version = BUILDING_PRESETS_VERSION, Buildings = buildings };

            Console.WriteLine();
            if (!testVersion)
            {
                Console.WriteLine("Writing buildings to presets-{0}-{1}.json", annoVersion, BUILDING_PRESETS_VERSION);
                DataIO.SaveToFile(presets, "presets-Anno" + annoVersion + "-v" + BUILDING_PRESETS_VERSION + ".json");
            }
            else
            {
                Console.WriteLine("THIS IS A TEST DUMMY FILE WRITEN!!!!");
                DataIO.SaveToFile(presets, "DUMMY.json");
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

        /// Get the BASE_PATH for the given Anno
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

        /// Prepare building list for preset/icon file
        #region Prepare buildings list for presets, depending on the Anno Version thats given
        //original Code minus the 'List<BuildingInfo> buildings' initialization  
        private static void DoAnnoPreset(string annoVersion)
        {
            Console.WriteLine();
            Console.WriteLine("Parsing assets.xml:");
            var assetPathRefs = VersionSpecificPaths[annoVersion]["assets"];

            if (annoVersion == ANNO_VERSION_1404 || annoVersion == ANNO_VERSION_2070)
            {
                // prepare localizations
                // This call will set the extra Anno Version Number on the icon translation for Anno 1404 ('A4_')
                // and Anno 2070 ('A5_') that will be seen in the Icons Selection tree of the program (icon.json)
                Dictionary<string, SerializableDictionary<string>> localizations = GetLocalizations(annoVersion, 1);
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
                WriteIconNameMapping(iconNodes, localizations, annoVersion, BUILDING_PRESETS_VERSION);
                //This must be done, to clear the 'A4_' or 'A5_' and get normal translation for presets.json
                localizations.Clear();
                localizations = GetLocalizations(annoVersion, 0);
                #endregion
                // parse buildings
                // find buildings in assets.xml
                foreach (PathRef p in assetPathRefs)
                {
                    ParseAssetsFile(BASE_PATH + p.Path, p.XPath, p.YPath, buildings, iconNodes, localizations, p.InnerNameTag, annoVersion);
                }
                // Add extra buildings to the anno version preset file
                AddExtraPreset(annoVersion, buildings);
            }
            else if (annoVersion == ANNO_VERSION_2205)
            {
                foreach (PathRef p in assetPathRefs)
                {
                    ParseAssetsFile2205(BASE_PATH + p.Path, p.XPath, p.YPath, buildings, p.InnerNameTag, annoVersion);
                }
                // Add extra buildings to the anno version preset file
                AddExtraPreset(annoVersion, buildings);
            }
            else if (annoVersion == ANNO_VERSION_1800)
            {
                foreach (PathRef p in assetPathRefs)
                {
                    ParseAssetsFile1800(BASE_PATH + p.Path, p.XPath, buildings);
                }
                // Add extra buildings to the anno version preset file
                AddExtraPreset(annoVersion, buildings);
            }
        }
        #endregion


        #region Add extra preset buildings to the Anno version preset file       

        /// <summary>
        /// Add some extra presets for a specific version of Anno.
        /// </summary>
        /// <param name="annoVersion">The version of Anno.</param>
        /// <param name="buildings">The already existing buildings.</param>
        private static void AddExtraPreset(string annoVersion, List<BuildingInfo> buildings)
        {
            foreach (var curExtraPreset in ExtraPresets.GetExtraPresets(annoVersion))
            {
                BuildingInfo buildingToAdd = new BuildingInfo
                {
                    Header = curExtraPreset.Header,
                    Faction = curExtraPreset.Faction,
                    Group = curExtraPreset.Group,
                    IconFileName = curExtraPreset.IconFileName,
                    Identifier = curExtraPreset.Identifier,
                    InfluenceRadius = curExtraPreset.InfluenceRadius,
                    InfluenceRange = curExtraPreset.InfluenceRange,
                    Template = curExtraPreset.Template,
                };

                Console.WriteLine("Extra Building : {0}", buildingToAdd.Identifier);

                buildingToAdd.BuildBlocker = new SerializableDictionary<int>();
                buildingToAdd.BuildBlocker["x"] = Convert.ToInt32(curExtraPreset.BuildBlockerX);
                buildingToAdd.BuildBlocker["z"] = Convert.ToInt32(curExtraPreset.BuildBlockerZ);
                buildingToAdd.Localization = new SerializableDictionary<string>();
                buildingToAdd.Localization["eng"] = curExtraPreset.LocaEng;
                buildingToAdd.Localization["ger"] = curExtraPreset.LocaGer;
                buildingToAdd.Localization["pol"] = curExtraPreset.LocaPol;
                buildingToAdd.Localization["rus"] = curExtraPreset.LocaRus;

                annoBuildingsListCount++;

                buildings.Add(buildingToAdd);
            }
        }

        #endregion

        /// Parsing Part for 1404 and 2070
        #region Parsing Buildngs for Anno 1404/2070
        private static void ParseAssetsFile(string filename, string xPathToBuildingsNode, string YPath, List<BuildingInfo> buildings,
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

        private static void ParseBuilding(List<BuildingInfo> buildings, XmlNode buildingNode, IEnumerable<XmlNode> iconNodes, Dictionary<string, SerializableDictionary<string>> localizations, string annoVersion)
        {
            #region Get valid Building Information 
            XmlElement values = buildingNode["Values"]; string nameValue = "", templateValue = "";
            // skip invalid elements
            if (buildingNode["Template"] == null)
            {
                return;
            }

            #region Skip Unused buildings in Anno Designer List
            if (annoVersion == ANNO_VERSION_1404)
            {
                nameValue = values["Standard"]["Name"].InnerText;
                isExcludedName = nameValue.Contains(ExcludeNameList1404);
                templateValue = buildingNode["Template"].InnerText;
                isExcludedTemplate = templateValue.Contains(ExcludeTemplateList1404);
            }
            if (annoVersion == ANNO_VERSION_2070)
            {
                nameValue = values["Standard"]["Name"].InnerText;
                isExcludedName = nameValue.Contains(ExcludeNameList2070);
                templateValue = buildingNode["Template"].InnerText;
                isExcludedTemplate = templateValue.Contains(ExcludeTemplateList2070);
                string factionValue = buildingNode.ParentNode.ParentNode.ParentNode.ParentNode["Name"].InnerText;
                isExcludedFaction = factionValue.Contains(ExcludeFactionList2070);
            }
            if (isExcludedName == true || isExcludedTemplate == true || isExcludedFaction == true)
            {
                return;
            }
            #endregion
            #region Skip Double Database Buildings
            nameValue = values["Standard"]["Name"].InnerText;
            if (nameValue != "underwater markethouse")
            {
                isExcludedName = nameValue.IsPartOf(annoBuildingLists);
                if (isExcludedName)
                {
                    return;
                }
            }
            string identifierName = values["Standard"]["Name"].InnerText;
            if (nameValue == "underwater markethouse")
            {
                if (nameValue.IsPartOf(annoBuildingLists))
                {
                    identifierName = "underwater markethouse II";
                }
            }

            #endregion

            // Parse Stuff
            string factionName = buildingNode.ParentNode.ParentNode.ParentNode.ParentNode["Name"].InnerText;
            string groupName = buildingNode.ParentNode.ParentNode["Name"].InnerText;
            groupName = groupName.FirstCharToUpper();
            factionName = factionName.FirstCharToUpper();
            #region Regrouping several faction or group names for Anno 1404
            if (annoVersion == ANNO_VERSION_1404)
            {
                if (factionName == "Farm") { factionName = "Production"; }
                if (identifierName == "Hospice") { factionName = "Public"; groupName = "Special"; }
            }
            #endregion
            #region Regrouping several faction or group names for Anno 2070
            if (annoVersion == ANNO_VERSION_2070)
            {
                if (factionName == "Ecos") { factionName = "(1) Ecos"; }
                if (factionName == "Tycoons") { factionName = "(2) Tycoons"; }
                if (factionName == "Techs") { factionName = "(3) Techs"; }
                if (factionName == "(3) Techs" && identifierName == "underwater markethouse II") { factionName = "Others"; }
                if (identifierName == "techs_academy") { groupName = "Public"; }
                if (groupName == "Farmfields" || groupName == "Farmfield") { groupName = "Farm Fields"; }
                if (factionName == "Others" && identifierName.Contains("black_smoker_miner") == true) { groupName = "Black Smokers (Normal)"; }
                if (factionName == "(3) Techs" && identifierName == "black_smoker_miner_platinum")
                {
                    factionName = "Others";
                    groupName = "Black Smokers (Deep Sea)";
                }
            }
            #endregion
            #region Set Header Name for Anno 1404 and Anno 2070
            string headerName = "Anno" + annoVersion;/*in case if statments are passed by*/
            if (annoVersion == ANNO_VERSION_1404) { headerName = "(A4) Anno " + ANNO_VERSION_1404; }
            if (annoVersion == ANNO_VERSION_2070) { headerName = "(A5) Anno " + ANNO_VERSION_2070; }
            #endregion
            BuildingInfo b = new BuildingInfo
            {
                Header = headerName,
                Faction = factionName,
                Group = groupName,
                Template = buildingNode["Template"].InnerText,
                Identifier = identifierName,
            };
            // print progress
            Console.WriteLine(b.Identifier);
            #endregion

            #region Get/Set InfluenceRange information
            //because this number is not exists yet, we set this to '0'
            b.InfluenceRange = 0;
            #endregion

            #region Parse building blocker
            // parse building blocker
            if (!RetrieveBuildingBlocker(b, values["Object"]["Variations"].FirstChild["Filename"].InnerText, annoVersion))
            {
                return;
            }
            #endregion

            #region Get IconFilename from icons.xml
            // find icon node based on guid match
            string buildingGuid = values["Standard"]["GUID"].InnerText;
            XmlNode icon = iconNodes.FirstOrDefault(_ => _["GUID"].InnerText == buildingGuid);
            if (icon != null)
            {
                b.IconFileName = GetIconFilename(icon["Icons"].FirstChild, annoVersion);
            }
            #endregion

            #region Get Influence Radius
            // read influence radius if existing
            try
            {
                b.InfluenceRadius = Convert.ToInt32(values["Influence"]["InfluenceRadius"].InnerText);
            }
            catch (NullReferenceException ex) { }
            #endregion

            #region Get Localization Translations ofr Building Names
            // find localization
            if (localizations.ContainsKey(buildingGuid))
            {
                b.Localization = localizations[buildingGuid];
            }
            else
            {
                Console.WriteLine("No Translation found, it will set to Identifier.");
                b.Localization = new SerializableDictionary<string>();
                int languageCount = 0;
                string translation = values["Standard"]["Name"].InnerText;
                //Anno 2070 need some special translations
                if (translation == "former_balance_ecos")
                {
                    translation = "Guardian 1.0";
                }
                if (translation == "former_balance_techs")
                {
                    translation = "Keeper 1.0";
                }
                if (translation == "oil_driller_variation_Sokow")
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
            annoBuildingLists.Add(values["Standard"]["Name"].InnerText);
            buildings.Add(b);
        }
        #endregion

        /// Parsing Part for 2205
        #region Parsing Buildngs for Anno 2205
        private static void ParseAssetsFile2205(string filename, string xPathToBuildingsNode, string YPath, List<BuildingInfo> buildings, string innerNameTag, string annoVersion)
        {
            XmlDocument assetsDocument = new XmlDocument();
            assetsDocument.Load(filename);
            XmlNode buildingNodes = assetsDocument.SelectNodes(xPathToBuildingsNode)
                .Cast<XmlNode>().Single(_ => _["Name"].InnerText == innerNameTag); //This differs between anno versions
            foreach (XmlNode buildingNode in buildingNodes.SelectNodes(YPath).Cast<XmlNode>())
            {
                ParseBuilding2205(buildings, buildingNode, ANNO_VERSION_2205);
            }
        }

        /// ORGLINE: private static void ParseBuilding2205(List<BuildingInfo> buildings, XmlNode buildingNode, IEnumerable<XmlNode> iconNodes, Dictionary<string, SerializableDictionary<string>> localizations)
        private static void ParseBuilding2205(List<BuildingInfo> buildings, XmlNode buildingNode, string annoVersion)
        {
            string[] LanguagesFiles = { "" };
            string nameValue = "", templateValue = "";
            #region Get valid Building Information 
            XmlElement values = buildingNode["Values"];
            // skip invalid elements
            if (buildingNode["Template"] == null)
            {
                return;
            }

            #region Skip Unused buildings in Anno Designer List
            nameValue = values["Standard"]["Name"].InnerText;
            isExcludedName = nameValue.Contains(ExcludeNameList2205);
            templateValue = buildingNode["Template"].InnerText;
            isExcludedTemplate = templateValue.Contains(ExcludeTemplateList2205);
            if (isExcludedName == true || isExcludedTemplate == true || isExcludedFaction == true)
            {
                return;
            }
            #endregion
            #region Skip Double Database Buildings
            nameValue = values["Standard"]["Name"].InnerText;
            isExcludedName = nameValue.IsPartOf(annoBuildingLists);
            if (isExcludedName)
            {
                return;
            }
            #endregion

            string buildingGuid = values["Standard"]["GUID"].InnerText;
            #region TEST SECTION OF GUID CHECK
            if (!testVersion)
            {
                if (buildingGuid.Contains(ExcludeGUIDList2205) == true) { return; }
            }
            else
            {
                if (printTestText == 0)
                {
                    Console.WriteLine();
                    Console.WriteLine("Testing GUID Resuld :");
                    printTestText = 1;
                }
                if (buildingGuid.Contains(ExcludeGUIDList2205) == true)
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
            #endregion

            // parse stuff
            string factionName = buildingNode.ParentNode.ParentNode.ParentNode.ParentNode["Name"].InnerText;
            string groupName = buildingNode.ParentNode.ParentNode["Name"].InnerText;
            string identifierName = values["Standard"]["Name"].InnerText;
            groupName = groupName.FirstCharToUpper();
            factionName = factionName.FirstCharToUpper();
            #region Regrouping several faction or group names for Anno 2205
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
            string headerName = "(A6) Anno " + ANNO_VERSION_2205;
            BuildingInfo b = new BuildingInfo
            {
                Header = headerName,
                Faction = factionName,
                Group = groupName,
                Template = buildingNode["Template"].InnerText,
                Identifier = identifierName
            };
            // print progress
            if (!testVersion)
            {
                Console.WriteLine(b.Identifier);
            }
            #endregion

            #region Get/Set InfluenceRange information
            //because this number is not exists yet, we set this to 'null'
            b.InfluenceRange = 0;
            #endregion

            #region Get BuildBlockers information
            //Get building blocker
            if (values["Object"] != null)
            {
                if (values["Object"]?["Variations"]?.FirstChild["Filename"]?.InnerText != null)
                {
                    if (!RetrieveBuildingBlocker(b, values["Object"]["Variations"].FirstChild["Filename"].InnerText, annoVersion))
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
                /// Split the Value <IconFilenames>innertext</IconFilenames> to get only the Name.png
                string replaceName = "";
                if (annoVersion == ANNO_VERSION_2205)
                {
                    replaceName = "A6_";
                }
                string[] sIcons = icon.Split('/');
                icon = sIcons.LastOrDefault().Replace("icon_", replaceName);
                b.IconFileName = icon;
            }
            else
            {
                b.IconFileName = null;
            }
            #endregion

            #region Get localizations
            /// find localization

            string languageFileName = ""; /// This will be given thru the static LanguagesFiles array
            string languageFilePath = "data/config/gui/";
            string languageFileStart = "texts_";
            string langNodeStartPath = "/TextExport/Texts/Text";
            string langNodeDepth = "Text";
            int languageCount = 0;
            LanguagesFiles = LanguagesFiles2205;

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
                if (testVersion == true && annoVersion == ANNO_VERSION_2205)
                {
                    if (languageCount == 0)
                    {
                        Console.WriteLine("ENG name: {0}", translation);
                        if (translation.IsPartOf(testGUIDNames2205))
                        {
                            Console.WriteLine(">>------------------------------------------------------------------------<<");
                            Console.ReadKey();
                            if (buildingGuid.Contains(ExcludeGUIDList2205) == true) { return; }
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

        /// Parsing Part for 1800
        #region Parsing Buildings for Anno 1800
        private static void ParseAssetsFile1800(string filename, string xPathToBuildingsNode, List<BuildingInfo> buildings)
        {
            XmlDocument assetsDocument = new XmlDocument();
            assetsDocument.Load(filename);
            List<XmlNode> buildingNodes = assetsDocument.SelectNodes(xPathToBuildingsNode).Cast<XmlNode>().ToList();

            foreach (XmlNode buildingNode in buildingNodes)
            {
                ParseBuilding1800(buildings, buildingNode, ANNO_VERSION_1800);
            }
        }

        private static void ParseBuilding1800(List<BuildingInfo> buildings, XmlNode buildingNode, string annoVersion)
        {
            string[] LanguagesFiles = { "" };
            string templateName = "", factionName = "", identifierName = "", groupName = "";
            string headerName = "(A7) Anno " + ANNO_VERSION_1800;
            #region Get valid Building Information 
            XmlElement values = buildingNode["Values"]; //Set the value List as normaly
            if (buildingNode.HasChildNodes)
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
                    if (templateName == null) { Console.WriteLine("No Template found, Building is skipped"); return; }
                    if (templateName.Contains(IncludeBuildingsTemplateNames1800) == false && templateName.Contains(IncludeBuildingsTemplateGUID1800) == false) { return; }
                    if (values.HasChildNodes == false) { return; }
                }
            }
            else
            {
                //Go next when no firstValue<ChildNames> are found
                return;
            }
            string guidName = values["Standard"]["GUID"].InnerText;
            identifierName = values["Standard"]["Name"].InnerText;
            identifierName = identifierName.FirstCharToUpper();
            isExcludedName = identifierName.Contains(ExcludeNameList1800);
            //isExcludedGUID = guidName.Contains(ExcludeBuildingsGUID1800);
            if (isExcludedName == true || isExcludedTemplate == true || isExcludedGUID == true)
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
            if (groupName == "") { groupName = "Not Placed Yet"; }
            switch (templateName)
            {
                case "Farmfield": groupName = "Farm Fields"; break;
                case "SlotFactoryBuilding7": { factionName = "All Worlds"; groupName = "Mining Buildings"; break; }
                case "Warehouse": { factionName = "(1) Farmers"; groupName = null; break; }
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
                default: groupName = templateName.FirstCharToUpper(); break;
            }
            if (groupName == "Farm Fields")
            {
                if (factionName == "Moderate") { factionName = "(6) Old World Fields"; groupName = null; }
                if (factionName == "Colony01") { factionName = "(9) New World Fields"; groupName = null; }
            }
            switch (identifierName)
            {
                case "Culture_01 (Zoo)": { factionName = "Attractiveness"; groupName = null; break; }
                case "Culture_02 (Museum)": { factionName = "Attractiveness"; groupName = null; break; }
                case "Residence_tier01": { factionName = "(1) Farmers"; identifierName = "Residence_Old_World"; groupName = "Residence"; break; }
                case "Residence_colony01_tier01": { factionName = "(7) Jornaleros"; identifierName = "Residence_New_World"; groupName = "Residence"; break; }
                case "Coastal_03 (Quartz Sand Coast Building)": { factionName = "All Worlds"; groupName = "Mining Buildings"; break; }
            }

            // Place the rest of the buildings in the right Faction > Group menu
            #region Order the Buildings to the right tiers and factions as in the game
            string[] newFactionGroupName = NewFactionAndGroup1800.get(identifierName, factionName, groupName);
            factionName = newFactionGroupName[0];
            groupName = newFactionGroupName[1];
            #endregion
            if (factionName == "" || factionName == "Moderate" || factionName == "Colony01")
            {
                factionName = "Not Placed Yet -" + factionName;
            }
            #endregion

            #region Starting buildup the preset data
            BuildingInfo b = new BuildingInfo
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
                    if (!RetrieveBuildingBlocker(b, values["Object"]["Variations"].FirstChild["Filename"].InnerText, annoVersion))
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
                }
                b.IconFileName = icon;
            }
            else
            {
                b.IconFileName = null;
                //Buildings that came in with the BaseAssetGUID template has no icons, this will fix that;
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
                }
            }
            #endregion

            #region Get Infuence Radios of Buildings
            // read influence radius if existing 
            try
            {
                b.InfluenceRadius = Convert.ToInt32(values["FreeAreaProductivity"]["InfluenceRadius"].InnerText);
            }
            catch (NullReferenceException ex) { }
            #endregion

            #region Get/Set InfluenceRange information
            //because this number is not exists yet, we set this to 'null'
            b.InfluenceRange = 0;
            #endregion
            // Building the Localizations for building b
            #region Get localizations
            /// find localization
            string buildingGuid = values["Standard"]["GUID"].InnerText;
            if (buildingGuid == "102133") { buildingGuid = "102085"; } /*rename the Big Tree to Mature Tree (as in game) */
            string languageFileName = ""; /// This will be given thru the static LanguagesFiles array
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
                            case 2: { translation = "Żywopłot Chodnikowy"; break; }
                            case 3: { translation = "Боковая изгородь"; break; }
                        }
                    }
                    if (buildingGuid == "102166")
                    {
                        switch (languageCount)
                        {
                            case 0: { translation = "Sidewalk Hedge Corner"; break; }
                            case 1: { translation = "Gehweg Heckenecke Ecke"; break; }
                            case 2: { translation = "Żywopłot Chodnikowy narożnik"; break; }
                            case 3: { translation = "Боковая изгородь (угол)"; break; }
                        }
                    }
                    if (buildingGuid == "102167")
                    {
                        switch (languageCount)
                        {
                            case 0: { translation = "Sidewalk Hedge End"; break; }
                            case 1: { translation = "Gehweg Heckenende"; break; }
                            case 2: { translation = "Żywopłot Chodnikowy Koniec"; break; }
                            case 3: { translation = "Боковая изгородь (край)"; break; }
                        }
                    }
                    if (buildingGuid == "102169")
                    {
                        switch (languageCount)
                        {
                            case 0: { translation = "Sidewalk Hedge Junction"; break; }
                            case 1: { translation = "Gehweg Hecken Verbindungsstelle"; break; }
                            case 2: { translation = "Żywopłot Chodnikowy Złącze"; break; }
                            case 3: { translation = "Боковая изгородь (Перекресток)"; break; }
                        }
                    }
                    if (buildingGuid == "102171")
                    {
                        switch (languageCount)
                        {
                            case 0: { translation = "Sidewalk Hedge Crossing"; break; }
                            case 1: { translation = "Gehweg Hecken Kreuzung"; break; }
                            case 2: { translation = "Żywopłot Chodnikowy Skrzyżowanie"; break; }
                            case 3: { translation = "Боковая изгородь (образного)"; break; }
                        }
                    }
                    if (buildingGuid == "102161")
                    {
                        switch (languageCount)
                        {
                            case 0: { translation = "Railings"; break; }
                            case 1: { translation = "Zaune"; break; }
                            case 2: { translation = "Poręcze"; break; }
                            case 3: { translation = "Ограда"; break; }
                        }
                    }
                    if (buildingGuid == "102170")
                    {
                        switch (languageCount)
                        {
                            case 0: { translation = "Railings Junction"; break; }
                            case 1: { translation = "Zaune Verbindungsstelle"; break; }
                            case 2: { translation = "Poręcze Złącze"; break; }
                            case 3: { translation = "Ограда (Перекресток)"; break; }
                        }
                    }
                    if (buildingGuid == "102134")
                    {
                        switch (languageCount)
                        {
                            case 0: { translation = "Hedge"; break; }
                            case 1: { translation = "Hecke"; break; }
                            case 2: { translation = "żywopłot"; break; }
                            case 3: { translation = "изгородь"; break; }
                        }
                    }
                    if (buildingGuid == "102139")
                    {
                        switch (languageCount)
                        {
                            case 0: { translation = "Path"; break; }
                            case 1: { translation = "Pfad"; break; }
                            case 2: { translation = "ścieżka"; break; }
                            case 3: { translation = "Тропинка"; break; }
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
                    string fieldAmountValue = null, fieldGuidValue = null;
                    switch (templateName)
                    {
                        case "FarmBuilding": { fieldGuidValue = values["ModuleOwner"]["ConstructionOptions"]["Item"]["ModuleGUID"].InnerText; fieldAmountValue = values?["ModuleOwner"]?["ModuleLimit"]?.InnerText; break; };
                        case "Farmfield": { fieldGuidValue = values["Standard"]["GUID"].InnerText; fieldAmountValue = "0"; break; }
                    }
                    if (fieldAmountValue != null)
                    {
                        bool getFieldGuidBool = false;
                        foreach (var getFieldGuid in farmFieldList1800)
                        {
                            if (getFieldGuid.fieldGUID == fieldGuidValue)
                            {
                                getFieldGuidBool = true;
                                fieldAmountValue = getFieldGuid.fieldAmount;
                                break;
                            }
                        }
                        if (!getFieldGuidBool)
                        {
                            farmFieldList1800.Add(new FarmField() { fieldGUID = fieldGuidValue, fieldAmount = fieldAmountValue });
                        }
                        translation = translation + " - (" + fieldAmountValue + ")";
                    }
                }
                b.Localization.Dict.Add(Languages[languageCount], translation);
                languageCount++;
            }
            #endregion

            #endregion
            // add building to the list
            annoBuildingsListCount++;
            annoBuildingLists.Add(values["Standard"]["Name"].InnerText);
            buildings.Add(b);
        }
        #endregion

        /// Other Classes and or Internal Commands used in this program
        #region Retrieving BuildingBlockers from Buidings Nodes
        private static bool RetrieveBuildingBlocker(BuildingInfo building, string variationFilename, string annoVersion)
        {
            if (annoVersion == ANNO_VERSION_1800)
            {
                XmlDocument ifoDocument = new XmlDocument();
                ifoDocument.Load(Path.Combine(BASE_PATH + "/", string.Format("{0}.ifo", Path.GetDirectoryName(variationFilename) + "\\" + Path.GetFileNameWithoutExtension(variationFilename))));
                try
                {
                    XmlNode node = ifoDocument.FirstChild["BuildBlocker"].FirstChild;
                    building.BuildBlocker = new SerializableDictionary<int>();
                    string xfNormal = node["xf"].InnerText;
                    string zfNormal = node["zf"].InnerText;
                    decimal XF = ParseBuildingBlockerNumber(xfNormal);
                    decimal ZF = ParseBuildingBlockerNumber(zfNormal);
                    if (XF < 1 && ZF < 0) /* When both values are zero, then skipp building */
                    {
                        Console.WriteLine("-'X' and 'Z' are both 0 - Building will skipped!");
                        return false;
                    }
                    if (XF > 0)
                    {
                        building.BuildBlocker["x"] = Convert.ToInt32(XF);
                    }
                    else
                    {
                        building.BuildBlocker["x"] = 1;
                    }
                    if (ZF > 0)
                    {
                        building.BuildBlocker["z"] = Convert.ToInt32(ZF);
                    }
                    else
                    {
                        building.BuildBlocker["z"] = 1;
                    }
                }
                catch (NullReferenceException)
                {
                    Console.WriteLine("-BuildBlocker not found, skipping");
                    return false;
                }
                return true;
            }
            else
            {
                XmlDocument ifoDocument = new XmlDocument();
                ifoDocument.Load(Path.Combine(BASE_PATH + "/", string.Format("{0}.ifo", Path.GetDirectoryName(variationFilename) + "\\" + Path.GetFileNameWithoutExtension(variationFilename))));
                try
                {
                    XmlNode node = ifoDocument.FirstChild["BuildBlocker"].FirstChild;
                    building.BuildBlocker = new SerializableDictionary<int>();
                    if (Math.Abs(Convert.ToInt32(node["x"].InnerText) / 2048) < 1 && Math.Abs(Convert.ToInt32(node["z"].InnerText) / 2048) < 1) /* When both values are zero, then skipp building */
                    {
                        Console.WriteLine("-'X' and 'Z' are both 0 - Building will skipped!");
                        return false;
                    }
                    if (Math.Abs(Convert.ToInt32(node["x"].InnerText) / 2048) > 0)
                    {
                        //Console.WriteLine("{0}", Path.GetFileNameWithoutExtension(variationFilename));
                        if (Path.GetFileNameWithoutExtension(variationFilename) != "ornamental_post_09")
                        {
                            if (Path.GetFileNameWithoutExtension(variationFilename) != "water_mill_ecos")
                            {
                                building.BuildBlocker["x"] = Math.Abs(Convert.ToInt32(node["x"].InnerText) / 2048);
                            }
                            else
                            {
                                building.BuildBlocker["x"] = 3;
                            }
                        }
                        else
                        {
                            building.BuildBlocker["x"] = 7;
                        }
                    }
                    else
                    {
                        building.BuildBlocker["x"] = 1;
                    }
                    if (Math.Abs(Convert.ToInt32(node["z"].InnerText) / 2048) > 0)
                    {
                        if (Path.GetFileNameWithoutExtension(variationFilename) != "water_mill_ecos" && Path.GetFileNameWithoutExtension(variationFilename) != "ornamental_post_09")
                        {
                            building.BuildBlocker["z"] = Math.Abs(Convert.ToInt32(node["z"].InnerText) / 2048);
                        }
                        else
                        {
                            building.BuildBlocker["z"] = 7;
                        }
                    }
                    else
                    {
                        building.BuildBlocker["z"] = 1;
                    }
                }
                catch (NullReferenceException)
                {
                    Console.WriteLine("-BuildBlocker not found, skipping");
                    return false;
                }
                return true;
            }
        }
        #endregion

        #region ParseBuildingBlockerNumber for anno 1800
        private static int ParseBuildingBlockerNumber(string number)
        {
            string[] xz = new[] { "" };
            int countNumberLenght = 0;
            int i = 0, zx = 0, xz1 = 0;
            double xz2 = 0;
            if (number.Contains("."))
            {
                xz = number.Split(new char[] { '.' });
                //Console.WriteLine("1: {0}  2: {1}", xz[0], xz[1]);
                xz1 = Math.Abs(Convert.ToInt32(xz[0]));
                xz2 = Math.Abs(Convert.ToInt32(xz[1]));
                //Console.WriteLine("xz1: {0}  xz2: {1}", xz1, xz2);
                countNumberLenght = xz[1].Length;
                //Console.WriteLine("lebght= {0}", countNumberLenght);
                while (i < countNumberLenght)
                {
                    xz2 = xz2 / 10;
                    //Console.WriteLine("{0}", xz2);
                    i++;
                }
                xz1 = xz1 * 2;
                xz2 = xz2 * 2;
                zx = xz1 + Convert.ToInt32(xz2);
            }
            else
            {
                zx = Math.Abs(Convert.ToInt32(number));
                zx = zx * 2;
            }
            return zx;
        }
        #endregion

        #region GuidRef Class
        private class GuidRef
        {
            public string Language;
            public string Guid;
            public string GuidReference;
        }
        #endregion

        #region Getting the Localizations from files (Anno 1404 / Anno 2070)
        private static Dictionary<string, SerializableDictionary<string>> GetLocalizations(string annoVersion, int DoExtraAnumber)
        {
            string[] files = { "icons.txt", "guids.txt", "addon/texts.txt" };
            Dictionary<string, SerializableDictionary<string>> localizations = new Dictionary<string, SerializableDictionary<string>>();
            List<GuidRef> references = new List<GuidRef>();
            foreach (string language in Languages)
            {
                foreach (PathRef p in VersionSpecificPaths[annoVersion]["localisation"])
                {
                    string basePath = Path.Combine(BASE_PATH, p.Path, language, "txt");
                    foreach (string path in files.Select(_ => Path.Combine(basePath, _)))
                    {
                        if (!File.Exists(path))
                        {
                            continue;
                        }

                        StreamReader reader = new StreamReader(path);
                        while (!reader.EndOfStream)
                        {
                            string line = reader.ReadLine();
                            // skip commentary and empty lines
                            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                            {
                                continue;
                            }
                            // split lines and skip invalid results
                            int separator = line.IndexOf('=');
                            if (separator == -1)
                            {
                                continue;
                            }
                            string guid = line.Substring(0, separator);
                            string translation = line.Substring(separator + 1);
                            // add new entry if needed
                            if (!localizations.ContainsKey(guid))
                            {
                                localizations.Add(guid, new SerializableDictionary<string>());
                            }
                            // add localization string
                            /// Translation of GUID 10239 (Anno 2070) is needed, else it will be named 
                            /// as Metal Converter, witch it is not.
                            if (annoVersion == ANNO_VERSION_2070 && guid == "10239" && DoExtraAnumber == 0)
                            {
                                if (language == "eng") { translation = "Black Smoker"; }
                                if (language == "ger") { translation = "Black Smoker"; }
                                if (language == "pol") { translation = "Komin hydrotermalny"; }
                                if (language == "rus") { translation = "Черный курильщик"; }
                            }
                            /// Icon.json extra A number for the Icon Selection tree
                            if (annoVersion == ANNO_VERSION_1404 && DoExtraAnumber == 1)
                            {
                                translation = "A4_" + translation;
                            }
                            if (annoVersion == ANNO_VERSION_2070 && DoExtraAnumber == 1)
                            {
                                translation = "A5_" + translation;
                            }
                            localizations[guid][language] = translation;
                            // remember entry if guid it is a reference to another guid
                            if (translation.StartsWith("[GUIDNAME"))
                            {
                                references.Add(new GuidRef
                                {
                                    Language = language,
                                    Guid = guid,
                                    GuidReference = translation.Substring(10, translation.Length - 11)
                                });
                            }
                            if (translation.StartsWith("A4_[GUIDNAME"))
                            {
                                references.Add(new GuidRef
                                {
                                    Language = language,
                                    Guid = guid,
                                    GuidReference = "A4_" + translation.Substring(13, translation.Length - 14)
                                });
                            }
                            if (translation.StartsWith("A5_[GUIDNAME"))
                            {
                                references.Add(new GuidRef
                                {
                                    Language = language,
                                    Guid = guid,
                                    GuidReference = "A5_" + translation.Substring(13, translation.Length - 14)
                                });
                            }
                        }
                    }
                }
            }

            // copy over references
            foreach (GuidRef reference in references)
            {
                if (localizations.ContainsKey(reference.GuidReference))
                {
                    localizations[reference.Guid][reference.Language] =
                        localizations[reference.GuidReference][reference.Language];
                }
                else
                {
                    localizations.Remove(reference.Guid);
                }
            }
            return localizations;
        }
        #endregion

        #region Writting the icons.json File (Anno 1404 / Anno 2070)
        private static void WriteIconNameMapping(IEnumerable<XmlNode> iconNodes, Dictionary<string, SerializableDictionary<string>> localizations, string annoVersion, string BUILDING_PRESETS_VERSION)
        {
            List<IconNameMap> mapping = new List<IconNameMap>();
            foreach (XmlNode iconNode in iconNodes)
            {
                string guid = iconNode["GUID"].InnerText;
                string iconFilename = GetIconFilename(iconNode["Icons"].FirstChild, annoVersion);
                if (!localizations.ContainsKey(guid) || mapping.Exists(_ => _.IconFilename == iconFilename))
                {
                    continue;
                }
                mapping.Add(new IconNameMap
                {
                    IconFilename = iconFilename,
                    Localizations = localizations[guid]
                });
            }
            if (!testVersion)
            {
                DataIO.SaveToFile(mapping, "icons-Anno" + annoVersion + "-v" + BUILDING_PRESETS_VERSION + ".json");
            }
            else
            {
                Console.WriteLine("TIS IS A TEST: No icon.sjon File is writen");
            }

        }
        #endregion

        #region FarmField Ilist commands for Anno 1800
        public class FarmField
        {
            public string fieldGUID { get; set; }
            public string fieldAmount { get; set; }
        }
        #endregion

        #region PathRef Class

        private class PathRef
        {
            public string Path { get; }
            public string XPath { get; }
            public string YPath { get; } //A secondary path used match xml within a secondary file
            public string InnerNameTag { get; }

            public PathRef(string path, string xPath, string yPath, string innerNameTag)
            {
                Path = path;
                XPath = xPath;
                YPath = yPath;
                InnerNameTag = innerNameTag;
            }

            public PathRef(string path, string xPath)
            {
                Path = path;
                XPath = xPath;
                // YPath = yPath;
                // InnerNameTag = innerNameTag;
            }

            public PathRef(string path)
            {
                Path = path;
            }
        }
        #endregion
    }
}
