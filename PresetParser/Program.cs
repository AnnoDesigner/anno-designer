using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Xml;
using AnnoDesigner.Core.Extensions;
using AnnoDesigner.Core.Helper;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Core.Presets.Models;
using PresetParser.Anno1404_Anno2070;
using PresetParser.Anno1800;
using PresetParser.Anno1800.Models;
using PresetParser.Extensions;
using PresetParser.Models;

namespace PresetParser;

public class Program
{
    #region Initializing values
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
    private const string BUILDING_PRESETS_VERSION = "5.1";
    // Initializing Language Directory's and Filenames
    private static readonly string[] Languages = ["eng", "ger", "fra", "pol", "rus", "esp"];
    private static readonly string[] LanguagesFiles2205 = ["english", "german", "french", "polish", "russian", "spanish"];
    private static readonly string[] LanguagesFiles1800 = ["english", "german", "french", "polish", "russian", "spanish"];
    // Internal Program Buildings Lists to skip double buildings
    public static List<string> annoBuildingLists = [];
    public static List<string> anno1800IconNameLists = [];
    public static List<string> TempExcludeOrnamentsFromPreset_1800 = [];
    public static List<string> IconIgnoreGuidOrIdentifier = [];
    public static int annoBuildingsListCount = 0, printTestText = 0;
    public static bool testVersion = false;
    // The internal Building list for the Preset writing 
    public static List<IBuildingInfo> buildings = [];
    private static readonly IconFileNameHelper _iconFileNameHelper;
    private static readonly BuildingBlockProvider _buildingBlockProvider;
    private static readonly IIfoFileProvider _ifoFileProvider;
    private static readonly LocalizationHelper _localizationHelper;
    private static readonly IFileSystem _fileSystem;

    // to solve preform ants issue with continues read in of language files (06-06-2022)
    public static XmlDocument langDocument_english = new();
    public static XmlDocument langDocument_german = new();
    public static XmlDocument langDocument_french = new();
    public static XmlDocument langDocument_polish = new();
    public static XmlDocument langDocument_russian = new();
    public static XmlDocument langDocument_spanish = new();

    // Information File for Duxvitae (DuxVitae-Replaced.csv)
    // For the Converter tool Anno1800SavegameVisualizer by Duxvitae i need to output a CSV file that he can use for the replaced / not added ornamentals
    public static string DVFileName = "replaced_guids.csv";
    public static string DVDataSeperator = ",";
    public static string[] DVDataList = new string[100000000];

    // This text file is made by me to get more inside of the TemplateNames we skip, and maybe need for buildings
    // I will use this for own use, to add or remove TemplateNames we may or not may use (04-06-2022)
    public static TextWriter PPTNFile = new StreamWriter("PresetsParserTemplateNames.txt");

    // To make is easier to find the missing icons in the Anno Assets Files, i will make a Text File "IconsMissing.txt" (14-06-2022)
    public static string IconFileCheckPath = "";
    public static bool checkIconFilePathDone = false;
    public static bool canCheckIconFiles = false;
    public static TextWriter IconNotExistFile = new StreamWriter("IconsMissing.txt");

    #region Initializing Exclude IdentifierNames, FactionNames and TemplateNames for presets.json file 

    #region Anno 1404
    private static readonly List<string> ExcludeNameList1404 =
    [ "ResidenceRuin", "AmbassadorRuin", "Gatehouse", "StorehouseTownPart", "ImperialCathedralPart", "SultanMosquePart", "Warehouse02", "Warehouse03",
        "Markethouse02", "Markethouse03", "TreeBuildCost", "BanditCamp"];
    private static readonly List<string> ExcludeTemplateList1404 = ["OrnamentBuilding", "Wall"];
    #endregion

    #region Anno 2070 * Also on FactionName Excludes *
    private static readonly List<string> ExcludeNameList2070 =
    [ "ruin_residence", "monument_unfinished", "town_center_variation", "nuclearpowerplant_destroyed",
        "limestone_quarry", "markethouse2", "markethouse3", "warehouse2","warehouse3", "cybernatic_factory","electronic_recycler"];
    private static readonly List<string> ExcludeTemplateList2070 = ["OrnamentBuilding", "Ark"];
    private static readonly List<string> ExcludeFactionList2070 = ["third party"];
    #endregion

    #region Anno 2205 Also a GUID Checker for excluding. Do not change any numbers below
    private static readonly List<string> ExcludeGUIDList2205 =
    [ "1001178", "1000737", "7000274", "1001175", "1000736", "7000275",
        "1000672", "1000755", "7000273", "1001171", "1000703", "7000272", "7000420", "7000421", "7000423", "7000424", "7000425", "7000427", "7000428",
        "7000429", "7000430", "7000431", "12000009", "12000010", "12000011", "12000020", "12000036", "1000063", "1000170", "1000212", "1000213", "1000174",
        "1000215", "1000217", "1000224", "1000250", "1000332", "1000886", "7001466", "7001467", "7001470", "7001471", "7001472", "7001473", "7001877",
        "7001878", "7001879", "7001880", "7001881", "7001882", "7001883", "7001884", "7001885", "7000310", "7000311", "7000315", "7000316",
        "7000313", "7000263", "7000262", "7000305", "7000306" ];
    private static readonly List<string> ExcludeNameList2205 =
    [ "Placeholder", "voting", "CTU Reactor 2 (decommissioned)", "CTU Reactor 3 (decommissioned)",
        "CTU Reactor 4 (decommissioned)", "CTU Reactor 5 (decommissioned)", "CTU Reactor 6 (decommissioned)", "CTU Reactor 2 (active!)", "CTU Reactor 3 (active!)",
        "CTU Reactor 4 (active!)", "CTU Reactor 5 (active!)", "CTU Reactor 6 (active!)", "orbit module 07 (unused)" ];
    private static readonly List<string> ExcludeTemplateList2205 = ["SpacePort", "BridgeWithUpgrade", "DistributionBuilding"];
    private static readonly List<string> testGUIDNames2205 = ["NODOUBLES YET"];
    #endregion

    #region anno 1800
    /// <summary>
    /// I need the IncludeBuildingsTemplateNames to get Building information from, as it is also the Presets Template String or Template GUID
    /// </summary>
    public static IList<FarmField> farmFieldList1800 = [];
    // Removed IncludeBuildingsTemplate "CultureModule" (to must to handle and thus are replaced with the Zoo Module and Museum Module
    private static readonly List<string> IncludeBuildingsTemplateNames1800 =
    [
        "ResidenceBuilding", "ResidenceBuilding7", "FarmBuilding", "FreeAreaBuilding", "FactoryBuilding7",
        "HeavyFactoryBuilding", "SlotFactoryBuilding7", "Farmfield", "OilPumpBuilding", "PublicServiceBuilding",
        "CityInstitutionBuilding", "CultureBuilding", "Market", "Warehouse", "PowerplantBuilding", "HarborOffice",
        "HarborWarehouse7", "HarborDepot", "Shipyard", "HarborBuildingAttacker", "RepairCrane", "HarborLandingStage7",
        "VisitorPier", "WorkforceConnector", "Guildhouse", "OrnamentalBuilding", "CultureModule", "Palace",
        "BuffFactory", "BuildPermitBuilding", "BuildPermitModules", "OrnamentalModule", "IrrigationPropagationSource",
        "ResearchCenter", "Dockland", "HarborOrnament", "Restaurant", "Busstop", "Multifactory",
        "FreeAreaRecipeBuilding", "Mall", "CultureModule", "Hacienda", "Heater_Arctic", "Monument",
        "HarborWarehouseStrategic", "WorkAreaRiverBuilding", "Slot", "WorkAreaSlot", "AdditionalModule", "RecipeFarm",
        "ItemWithUICrafting", "PostBoxBuildingWithDepot", "PostBoxBuildingWithPublicService", "AirshipPlatform",
        "AirshipPlatformModuleItemTransfer", "AirshipPlatformPostModule", "AirshipPlatformModuleWorkforceTransfer",
        "AirshipPostFreeModule"
    ];
    private static readonly List<string> IncludeBuildingsTemplateGUID1800 =
    [
        "100451", "1010266", "1010343", "1010288", "101331", "1010320", "1010263", "1010372", "1010359", "1010358",
        "1010462", "1010463", "1010464", "1010275", "1010271", "1010516", "1010517", "1010519", "1000155", "101623",
        "1003272", "118218", "100849", "1010186", "100438", "114435", "1010371", "100516", "100517", "102449", "100783",
        "100519", "100429", "100510", "100511", "119259", "101404", "1010311", "100415", "100586", "1010540", "100515",
        "100784", "1010525", "101403", "100416", "1010283", "1010520", "1010310", "1010522", "1010523", "101263",
        "24657", "24658", "24652", "101280", "1010321", "1010304", "1010309", "1010308", "1010305", "1010500", "1010501",
        "1010504", "1010505", "1010277", "1010542", "1010546", "1010543", "101272", "100514", "742", "962"
    ];
    // The following GUID's are given in the assets as <BaseAssetGUID> tags instead of Template name tags
    private static readonly List<string> ExcludeBuildingsGUID1800 = ["269850", "269851", "25175", "25176"];
    private static readonly List<string> ExcludeNameList1800 =
    [
        "TreePlanter_GGJ_TEST", "(Wood Field)", "(Hunting Grounds)", "(Wash House)", "Fake Ornament [test 2nd party]",
        "Third_party_", "CQO_", "CO_Tunnel_", "- Pirates", "Ai_", "AarhantLighthouseFake", "CO_Tunnel_Entrance01_Fake",
        "AI Version No Unlock", "Active fertility", "- Decree", "fertility", "Arctic Cook", "Arctic Builder",
        "Arctic Hunter", "Arctic Sewer", " Buff", " Seeds", "Harbour Slot (Ghost) Arctic",
        "Tractor_module_01 (GASOLINE TEST)", "Fuel_station_01 (GASOLINE TEST)", "StoryIsland01 Monastery Kontor",
        "StoryIsland02 Military Kontor", "StoryIsland03 Economy Kontor", "CourtOfJustice_", "Basin_Base", "- Paragon_",
        "setBuff", "Buff_", "Harbor_13 (Coal Storage)", "Harbor_12 (Coal Harbor)"
    ];

    #region List of skipped Template (Be aware, it is a long list)
    // To eliminate the TempplatesNames to be written in the file, as we check and looked at those and not need or can use in the presets file.
    // It will easier for me, when there is an update, new template tags will be added to a to file and later added to this list or to
    // the IncludeTemplateNames1800 or IncludeBuildingsTemplateGUID1800 list (04-06-2022)
    public static List<string> PPTNList =
    [
        "Text", "TrackingValue", "LandAnimal", "Bird", "Painter", "IncidentResolverUnit", "Inhabitant",
        "VisualObjectEditor", "VisualObject", "Prop", "SimpleVehicle", "StarterObject Enter/Leave Point",
        "RemovableWaterBlocker", "StaticEventBlockingObject", "StaticBlockingObject", "PositionMarker", "AudioSpots",
        "Projectile", "Collectable", "VisualQuestObject", "ScannerObject", "TradeShip", "QuestObject", "ChannelTarget",
        "QuestItem", "PaMSy_Base", "Product", "Transporter", "FeedbackParametersGlobal", "AnimalSessionDesc",
        "AnimalGlobalDesc", "DifficultySetup", "FireIncident", "ResolveActionCost", "IncidentCommunication",
        "RiotIncident", "IllnessIncident", "ExplosionIncident", "Festival", "StandaloneIncidentEffectConfiguration",
        "Trigger", "ProgressLevel", "FeatureUnlock", "MapTemplate", "NewsArticleList", "NewspaperArticle", "SimpleAsset",
        "Profile_Virtual_NeverOwnsObjects", "Profile_2ndParty", "Profile_3rdParty_NoDiplomacy_NoTrader", "QuestPool",
        "Quest", "Matcher", "SessionModerate", "SessionSouthAmerica", "ExpeditionEventPool", "Expedition",
        "ItemEffectTargetPool", "InfluenceTitleBuff", "ItemWithUI", "ActiveItem", "Audio", "GameParameter",
        "SwitchGroup", "StateGroup", "Video", "RewardPool", "MonumentEventReward", "MonumentEvent", "Notification",
        "InfoTip", "InfoLayerIcon", "ConstructionMenu", "IncidentOverlayConfig", "ObjectmenuResidenceScene",
        "ObjectmenuKontor", "ObjectmenuShipScene", "ObjectmenuVisitorHarborScene", "ObjectmenuCityInstitutionScene",
        "ObjectmenuCommuterHarbourScene", "ObjectmenuMilitary", "MaintenanceBarConfig", "ObjectMenuScenarioRuinScene",
        "IslandBarScene", "WorkforceMenu", "GenericPopup", "FilteredSelectionPopup", "NegotiationPopup",
        "NewspaperScene", "ValueAssetMap", "RightClickMenu", "ItemFilter", "KeywordFilter", "ItemKeywords",
        "StaticHelpConfig", "PlayerLogo", "Icon", "TargetGroup", "Portrait", "Seamine", "RewardItemPool", "UplayReward",
        "Island", "CraftingPopup", "TreasureMapScene", "Fertility", "Profile_3rdParty", "WorldMap", "MinimapDot",
        "NewspaperSpecialEditionArticle", "NewspaperImage", "Street", "IrrigationFeature", "ResearchFeature",
        "RiverslotFeature", "Region", "ResearchCentreScene", "TradeContractFeature", "ConstructionCategory", "Skin",
        "LandSpy", "TownhallItem", "ScenarioInformation", "SeasonFeature", "TownhallBuff", "CameraSequence",
        "EffectContainer", "HarbourOfficeBuff", "EcoSystemFeature", "EcoSystemBuff", "AssetPool", "ThirdpartyFeedback",
        "Fish", "IceFloe", "Herd", "Flock", "FeedbackVehicle", "FleetDummy", "CampaignUncleMansion", "ItemSpecialAction",
        "FeedbackBuildingGroup", "UnlockNewsTracker", "ObjectBuildNewsTracker", "OverallSatisfactionNewsTracker",
        "NeedSatisfactionNewsTracker", "IncomeBalanceNewsTracker", "WorkforceNewsTracker", "WorkforceSliderNewsTracker",
        "IncidentNewsTracker", "ShipBuiltNewsTracker", "MilitaryNewsTracker", "DiplomacyNewsTracker",
        "CityAttractivenessNewsTracker", "HostileTakeoverNewsTracker", "PlacementScore", "ScenarioSelectionMarker",
        "VehicleBuff", "ShipSpecialist", "VehicleItem", "FluffItem", "ItemSet", "SwitchChoice", "StateChoice",
        "StaticHelpTopic", "DivingBellObject", "VisualBuilding_NoLogic", "FeedbackObject", "UnlockableAsset",
        "ResearchSubcategory", "ProgressBalancing", "NeedsSatisfactionNews", "ObjectmenuPierScene", "AirShip",
        "RecipeList", "Recipe", "MovingMobPicturePuzzle", "FeedbackUnitClass", "TrafficFeedbackUnit", "SinglePlayerGame",
        "Season", "Profile_1stParty_Scenario_Narrator", "Resource", "VisualSoundEmitter", "WalkableObject",
        "ThreeHeadedAnimal", "ScenarioRuinEco", "ScenarioLoadingScene", "ScenarioGameOverScene", "WorkArea",
        "DivingBellShip", "FeedbackDescription", "FeedbackSessionDescriptionOverwritable", "WarShip", "ForwardBuff",
        "CultureBuff", "ItemCategory", "CultureItem", "ItemCrafterHarbor", "ReplenishPermit", "PopulationLevel7",
        "PopulationGroup7", "BuildPermitGroup", "BridgeBuilding", "Godlike", "Tree", "RFX", "ItemCrafterBuilding",
        "FertilizerBaseModule", "FertilizerBaseBuilding", "102664", "102665", "140492", "174", "121", "152", "119",
        "167", "169", "168", "170", "77", "1010524", "122", "123", "124", "120", "126", "145", "127", "128", "1010567",
        "100446", "101008", "117108", "100439", "100440", "1010361", "102229", "102383", "102892", "102483", "102450",
        "102666", "102448", "100442", "1010062", "100441", "118718", "100437", "100443", "101432", "102428", "102425",
        "101965", "1010158", "102631", "102635", "102638", "102641", "102644", "102371", "102588", "142613", "2001096",
        "142615", "142873", "141027", "141079", "142792", "141013", "141010", "141076", "141082", "141084", "141189",
        "803895", "501757", "501941", "112551", "113695", "113964", "113965", "113784", "113785", "113786", "113787",
        "113788", "113789", "1010035", "1000178", "2001019", "142467", "102344", "667", "25000035", "501516", "15000005",
        "15000006", "15000000", "130097", "130101", "130103", "130096", "130100", "190865", "190872", "21389", "118745",
        "668", "137943", "689", "764", "963", "138793", "139107", "140037", "140043", "101293", "101294", "101295",
        "101290", "101291", "101292", "101254", "101255", "130237", "130236", "130238", "130239", "130291", "130240",
        "130241", "130242", "130243", "130244", "130246", "130248", "22395", "22374", "270008", "269865", "24187",
        "24024", "24027", "24028", "24029", "24056", "24057", "24058", "24059", "949", "680", "685", "686", "24030",
        "24350", "139859", "24053", "24048", "24034", "24012", "24014", "24016", "24018", "24033", "24036", "24019",
        "24023", "24043", "24044", "24054", "24164", "24114", "24151", "24153", "24154", "24155", "24159", "24160",
        "24162", "24163", "24087", "24086", "24061", "24064", "24065", "24068", "24078", "24079", "24081", "24082",
        "24100", "24107", "24108", "24109", "24113", "24195", "24196", "24197", "24201", "24199", "24198", "24179",
        "24191", "24192", "24248", "24286", "141486", "142412", "142413", "140786", "140787", "140789", "140790",
        "140794", "500005", "500017", "25000193", "25000194", "501007", "500904", "500908", "500910", "500913", "500912",
        "500906", "500911", "501254", "500905", "500946", "500950", "25000195", "500951", "502008", "502009", "502022",
        "502023", "502000", "502001", "1010278", "1010280", "1010298", "1010297", "101061", "102498", "101332",
        "1010547", "100418", "101415", "1010333", "1010329", "101251", "1010342", "101252", "1010340", "1010334",
        "101253", "1010338", "101062", "101258", "101257", "101259", "101323", "101324", "101325", "1010348", "133004",
        "269848", "269849", "269958", "269835", "25056", "138761", "139917", "139935", "24250", "101309", "101308",
        "1010257", "1010193", "1010207", "1010202", "1010208", "1010200", "133095", "140500", "140503", "140504",
        "140505", "140506", "142932", "140595", "140596", "140597", "141414", "141893", "500107", "25000087", "500481",
        "1010017", "130098", "140041", "140039", "114327", "114759", "114328", "24792", "24793", "25224", "24768",
        "25743", "25546", "25506", "24807", "101344", "101329", "101405", "101406", "101330", "118219", "118216",
        "118215", "118220", "118221", "118222", "118223", "118224", "118225", "118226", "118227", "118228", "118952",
        "118953", "80022", "80027", "102443", "101327", "1003240", "1003250", "1000071", "1003231", "1001799", "1001792",
        "1001789", "80110", "502085", "502083", "502084", "502082", "502081", "502080", "502078", "130245", "2320",
        "19534", "118236", "117659", "117660", "117661", "117662", "114331", "24794", "24798", "25003", "25019", "25020",
        "24795", "24800", "24801", "25064", "25350", "25508", "24802", "24806", "25330", "101303", "1010318", "1010317",
        "101296", "1010330", "1010331", "101311", "101339", "102460", "102459", "1010335", "1010336", "1010337",
        "100524", "1010549", "1010507", "1010270", "1010273", "501008", "502075", "502038", "502044", "125295", "24828",
        "24829", "100009", "100722", "1010233", "1010192", "1010195", "1010250", "1010196", "1010216", "1010214",
        "1010258", "1010251", "1010252", "1010255", "1010256", "1010239", "1010259", "114452", "114448", "114441",
        "114495", "114490", "24825", "24836", "24820", "24844", "24845", "24856", "24857", "24860", "24861", "25547",
        "25548", "25549", "EffectExclusiveTag", "DropGoodPopup", "ItemSearchConfig", "180023", "1049", "849", "140985",
        "1060", "720", "102430", "102429", "962", "ProductList", "501996", "501995", "502017", "502021", "502050",
        "501422", "501423", "501424", "2006", "2005", "2013", "2014", "1379", "1797", "1798", "130260", "130247",
        "130261", "502034", "502027", "502067", "1000029", "192484", "192483", "192482", "191788", "191789", "191790",
        "191750", "191751", "191752", "191753", "191754", "191755", "190675", "190676", "191006", "190269", "191008",
        "191007", "191009", "191010", "191572", "192468", "192450", "190693", "190724", "190722", "190723", "190410",
        "190725", "190653", "190656", "191312", "191313", "190760", "190757", "190759", "191463", "191581", "191582",
        "191387", "191466", "190818", "190819", "190820", "192305", "190826", "190824", "190891", "190892", "190893",
        "190913", "190861", "1945", "4602", "4603", "4616", "4617", "4618", "100817", "1988", "535", "536", "4267",
        "2280", "1361", "2279", "2047", "2048", "102931", "103608", "103406", "103414", "103415", "103416", "103417",
        "103419", "103423", "103425", "103429", "103430", "103610", "103612", "103613", "103614", "103615", "103619",
        "103620", "103621", "1384", "2226", "2232", "2281", "117302", "117303", "116175", "116173", "116186", "116189",
        "140788", "140790", "140792", "140795", "142344", "142345", "142346", "142347", "142348", "142349", "24525",
        "24526", "24527", "24528", "141531", "141532", "141533", "141530", "500907", "502072", "501957", "2287", "2284",
        "102319", "2038", "4619", "4620", "4621", "4622", "4623", "3761", "3661", "692", "693", "695", "635", "835",
        "1418", "1308", "1353", "906", "538", "966", "4254", "103643", "103645", "103646", "103647", "103648", "103649",
        "103650", "103651", "103652", "103653", "114166", "110942", "110943", "110944", "110950", "110948", "110938",
        "110936", "110937", "111179", "111040", "111039", "111038", "111034", "111033", "111032", "111028", "111027",
        "111026", "111020", "111019", "111018", "1010218", "1010210", "1654", "1655", "1058", "1059", "112518", "2397",
        "2400", "ItemTransferFeature", "ScenarioWorkshopItem", "ScenarioWorkshopPackage"
    ];
    #endregion

    #endregion

    #endregion

    static Program()
    {
        _iconFileNameHelper = new IconFileNameHelper();
        _ifoFileProvider = new IfoFileProvider();
        _buildingBlockProvider = new BuildingBlockProvider(_ifoFileProvider);

        _fileSystem = new FileSystem();
        _localizationHelper = new LocalizationHelper(_fileSystem);

        VersionSpecificPaths = [];
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
            else if (annoVersion.Equals("-test", StringComparison.CurrentCultureIgnoreCase))
            {
                Console.Write("Please enter an Anno version:");
                annoVersion = Console.ReadLine();
                if (annoVersion == Constants.ANNO_VERSION_1404 || annoVersion == Constants.ANNO_VERSION_2070 || annoVersion == Constants.ANNO_VERSION_2205 || annoVersion == Constants.ANNO_VERSION_1800)
                {
                    validVersion = true;
                    testVersion = true;
                }
            }
            else if (annoVersion == "-validate")
            {
                Console.Write("Please enter path to file for validation: ");
                string filePathToValidate = Console.ReadLine();

                //get rid of quotes in the file-path (could contain spaces)
                if (!string.IsNullOrWhiteSpace(filePathToValidate))
                {
                    filePathToValidate = filePathToValidate.Trim('"');
                }

                if (string.IsNullOrWhiteSpace(filePathToValidate) || !_fileSystem.File.Exists(filePathToValidate))
                {
                    ConsoleColor oldColor = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("The path to the file was not valid!");
                    Console.ForegroundColor = oldColor;
                    Console.ReadKey();
                    Environment.Exit(0);
                }

                try
                {
                    BuildingPresets loadedPresets = SerializationHelper.LoadFromFile<BuildingPresets>(filePathToValidate);

                    ValidateBuildings(loadedPresets.Buildings.Cast<IBuildingInfo>().ToList());
                    Console.ReadLine();
                    Environment.Exit(0);
                }
                catch (Exception ex)
                {
                    ConsoleColor oldColor = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("There was an error validating the file:");
                    Console.WriteLine(ex);
                    Console.ForegroundColor = oldColor;
                    Environment.Exit(0);
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
            // Add a trailing backslash if one is not present.
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
            Console.WriteLine("Testing RDA data from {0} for anno version {1}.", BASE_PATH, annoVersion);
        }
        else
        {
            Console.WriteLine("Extracting and parsing RDA data for all Anno versions");
        }

        #endregion

        #region Anno Version Data Paths
        // <summary>
        // Holds the paths and xpath's to parse the extracted RDA's for different Anno versions
        // 
        // The RDA's should all be extracted into the same directory.
        // </summary>
        //These should stay constant for different anno versions (hopefully!)
        #region Anno 1404 xPaths
        if (annoVersion == Constants.ANNO_VERSION_1404 || annoVersion == "-ALL")
        {
            VersionSpecificPaths.Add(Constants.ANNO_VERSION_1404, []);
            VersionSpecificPaths[Constants.ANNO_VERSION_1404].Add("icons",
            [
            //new PathRef("data/config/game/icons.xml", "/Icons/i" ),
            new("addondata/config/game/icons.xml", "/Icons/i", "", "")
            ]);
            VersionSpecificPaths[Constants.ANNO_VERSION_1404].Add("localisation",
            [
            new("data/loca"),
            new("addondata/loca")
            ]);
            VersionSpecificPaths[Constants.ANNO_VERSION_1404].Add("assets",
            [
            new("data/config/game/assets.xml", "/AssetList/Groups/Group/Groups/Group", "Groups/Group/Assets/Asset", "PlayerBuildings"),
            new("data/config/game/assets.xml", "/AssetList/Groups/Group/Groups/Group", "Groups/Group/Groups/Group/Assets/Asset", "PlayerBuildings"),
            new("data/config/game/assets.xml", "/AssetList/Groups/Group/Groups/Group", "Groups/Group/Groups/Group/Groups/Group/Assets/Asset", "PlayerBuildings"),
            new("addondata/config/game/assets.xml", "/AssetList/Groups/Group/Groups/Group", "Groups/Group/Assets/Asset", "PlayerBuildings"),
            new("addondata/config/game/assets.xml", "/AssetList/Groups/Group/Groups/Group", "Groups/Group/Groups/Group/Assets/Asset", "PlayerBuildings"),
            new("addondata/config/game/assets.xml", "/AssetList/Groups/Group/Groups/Group", "Groups/Group/Groups/Group/Groups/Group/Assets/Asset", "PlayerBuildings"),
            new("addondata/config/balancing/addon_01_assets.xml", "/Group/Groups/Group/Groups/Group", "Groups/Group/Groups/Group/Assets/Asset", "PlayerBuildings")
            ]);
        }
        #endregion

        #region Anno 2070 xPaths
        if (annoVersion == Constants.ANNO_VERSION_2070 || annoVersion == "-ALL")
        {
            VersionSpecificPaths.Add(Constants.ANNO_VERSION_2070, []);
            VersionSpecificPaths[Constants.ANNO_VERSION_2070].Add("icons",
            [
            new("data/config/game/icons.xml", "/Icons/i", "", "")
            ]);
            VersionSpecificPaths[Constants.ANNO_VERSION_2070].Add("localisation",
            [
            new("data/loca")
            ]);
            VersionSpecificPaths[Constants.ANNO_VERSION_2070].Add("assets",
            [
            new("data/config/game/assets.xml", "/AssetList/Groups/Group/Groups/Group", "Groups/Group/Groups/Group/Assets/Asset", "Buildings"),
            new("data/config/game/assets.xml", "/AssetList/Groups/Group/Groups/Group", "Groups/Group/Groups/Group/Groups/Group/Assets/Asset", "Buildings"),
            new("data/config/dlc_01/assets.xml", "/Group/Groups/Group", "Groups/Group/Groups/Group/Assets/Asset", "Buildings"),
            new("data/config/dlc_02/assets.xml", "/Group/Groups/Group", "Groups/Group/Groups/Group/Assets/Asset", "Buildings"),
            new("data/config/dlc_03/assets.xml", "/Group/Groups/Group", "Groups/Group/Groups/Group/Assets/Asset", "Buildings"),
            new("data/config/dlc_04/assets.xml", "/Group/Groups/Group", "Groups/Group/Groups/Group/Assets/Asset", "Buildings"),
            new("addondata/config/balancing/addon_01_assets.xml", "/Group/Groups/Group", "Groups/Group/Groups/Group/Assets/Asset", "Buildings"),
            new("addondata/config/balancing/addon_01_assets.xml", "/Group/Groups/Group", "Groups/Group/Groups/Group/Groups/Group/Assets/Asset", "Buildings")
            ]);
        }
        #endregion

        #region Anno 2205 xPaths
        if (annoVersion == Constants.ANNO_VERSION_2205 || annoVersion == "-ALL")
        {
            VersionSpecificPaths.Add(Constants.ANNO_VERSION_2205, []);
            // Trying to read data from the objects.exm 
            Console.WriteLine();
            Console.WriteLine("Trying to read Buildings Data from the objects.xml of anno 2205");
            VersionSpecificPaths[Constants.ANNO_VERSION_2205].Add("assets",
            [
                #region Data Structure Normal Anno 2205
                new("data/config/game/asset/objects/buildings.xml", "/Group/Groups/Group", "Groups/Group/Assets/Asset", "Earth"),
                new("data/config/game/asset/objects/buildings.xml", "/Group/Groups/Group", "Groups/Group/Groups/Group/Assets/Asset", "Earth"),
                new("data/config/game/asset/objects/buildings.xml", "/Group/Groups/Group", "Groups/Group/Groups/Group/Groups/Group/Assets/Asset", "Earth"),
                new("data/config/game/asset/objects/buildings.xml", "/Group/Groups/Group", "Groups/Group/Groups/Group/Groups/Group/Groups/Group/Assets/Asset", "Earth"),
                new("data/config/game/asset/objects/buildings.xml", "/Group/Groups/Group", "Groups/Group/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Assets/Asset", "Earth"),
                new("data/config/game/asset/objects/buildings.xml", "/Group/Groups/Group", "Groups/Group/Assets/Asset", "Arctic"),
                new("data/config/game/asset/objects/buildings.xml", "/Group/Groups/Group", "Groups/Group/Groups/Group/Assets/Asset", "Arctic"),
                new("data/config/game/asset/objects/buildings.xml", "/Group/Groups/Group", "Groups/Group/Groups/Group/Groups/Group/Assets/Asset", "Arctic"),
                new("data/config/game/asset/objects/buildings.xml", "/Group/Groups/Group", "Groups/Group/Groups/Group/Groups/Group/Groups/Group/Assets/Asset", "Arctic"),
                new("data/config/game/asset/objects/buildings.xml", "/Group/Groups/Group", "Groups/Group/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Assets/Asset", "Arctic"),
                new("data/config/game/asset/objects/buildings.xml", "/Group/Groups/Group", "Groups/Group/Assets/Asset", "Moon"),
                new("data/config/game/asset/objects/buildings.xml", "/Group/Groups/Group", "Groups/Group/Groups/Group/Assets/Asset", "Moon"),
                new("data/config/game/asset/objects/buildings.xml", "/Group/Groups/Group", "Groups/Group/Groups/Group/Groups/Group/Assets/Asset", "Moon"),
                new("data/config/game/asset/objects/buildings.xml", "/Group/Groups/Group", "Groups/Group/Groups/Group/Groups/Group/Groups/Group/Assets/Asset", "Moon"),
                new("data/config/game/asset/objects/buildings.xml", "/Group/Groups/Group", "Groups/Group/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Assets/Asset", "Moon"),
                #endregion
                #region Data Structure DLC 01 (Tundra)
                new("data/dlc01/config/game/asset/objects.xml", "/Group/Groups/Group", "Groups/Group/Groups/Group/Assets/Asset", "Buildings"),
                new("data/dlc01/config/game/asset/objects.xml", "/Group/Groups/Group", "Groups/Group/Groups/Group/Groups/Group/Assets/Asset", "Buildings"),
                new("data/dlc01/config/game/asset/objects.xml", "/Group/Groups/Group", "Groups/Group/Groups/Group/Groups/Group/Groups/Group/Assets/Asset", "Buildings"),
                #endregion
                #region Data Structure DLC 02 (Orbit)
                new("data/dlc02/config/game/asset/objects.xml", "/Group/Groups/Group", "Groups/Group/Groups/Group/Assets/Asset", "Buildings"),
                #endregion
                #region Data Structure DLC 03 (Frontiers) (and DCL 04, no data groups)
                new("data/dlc03/config/game/asset/objects.xml", "/Group/Groups/Group", "Groups/Group/Groups/Group/Assets/Asset", "Buildings"),
                new("data/dlc03/config/game/asset/objects.xml", "/Group/Groups/Group", "Groups/Group/Groups/Group/Groups/Group/Assets/Asset", "Buildings"),
                //DLC 04 does not contain any building information that goes into the preset
                #endregion
                #region Data Structure FCP 01 / 02 / 02b
                new("data/fcp01/config/game/asset/objects.xml", "/Group/Groups/Group", "Groups/Group/Groups/Group/Assets/Asset", "Buildings"),
                new("data/fcp02/config/game/asset/objects.xml", "/Group/Groups/Group", "Groups/Group/Groups/Group/Assets/Asset", "Buildings"),
                new("data/fcp02b/config/game/asset/objects.xml", "/Group/Groups/Group", "Groups/Group/Groups/Group/Assets/Asset", "Buildings")
                #endregion
            ]);
        }
        #endregion

        #region Anno 1800 xPaths
        if (annoVersion == Constants.ANNO_VERSION_1800 || annoVersion == "-ALL")
        {
            VersionSpecificPaths.Add(Constants.ANNO_VERSION_1800, []);
            // Trying to read data from the objects.exm 
            Console.WriteLine();
            Console.WriteLine("Trying to read Buildings Data from the objects.xml of anno 1800");
            // I have removed the lat pathname 'Values' as it does the same i wanted, 
            // only the 'Values' will skip the <template> tag that i still need
            VersionSpecificPaths[Constants.ANNO_VERSION_1800].Add("assets",
            [
                // Base Game with DLC's
                new("data/config/export/main/asset/assets.xml", "AssetList/Groups/Group/Groups/Group/Groups/Group/Assets/Asset"),
                new("data/config/export/main/asset/assets.xml", "AssetList/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Assets/Asset"),
                new("data/config/export/main/asset/assets.xml", "AssetList/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Assets/Asset"),
                new("data/config/export/main/asset/assets.xml", "AssetList/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Assets/Asset"),
                new("data/config/export/main/asset/assets.xml", "AssetList/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Assets/Asset"),
                new("data/config/export/main/asset/assets.xml", "AssetList/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Assets/Asset"),
                // Scenario 1 Extra / Changed Building Lists
                new("data/eoy21/config/game/assets/scenario/config/export/main/asset/assets.xml", "AssetList/Groups/Group/Groups/Group/Groups/Group/Assets/Asset"),
                new("data/eoy21/config/game/assets/scenario/config/export/main/asset/assets.xml", "AssetList/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Assets/Asset"),
                new("data/eoy21/config/game/assets/scenario/config/export/main/asset/assets.xml", "AssetList/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Assets/Asset"),
                new("data/eoy21/config/game/assets/scenario/config/export/main/asset/assets.xml", "AssetList/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Assets/Asset"),
                new("data/eoy21/config/game/assets/scenario/config/export/main/asset/assets.xml", "AssetList/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Assets/Asset"),
                new("data/eoy21/config/game/assets/scenario/config/export/main/asset/assets.xml", "AssetList/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Assets/Asset"),
                // Scenario 2 Extra / Changed Building Lists
                // Nothing in the assets to add, ad there are no buildings in it (not with GUID's) and the other buildings are in the normal Base Xpath's
                new("data/dlc10/scenario02/config/game/assets/scenario/config/export/main/asset/assets.xml", "AssetList/Groups/Group/Groups/Group/Groups/Group/Assets/Asset"),
                new("data/dlc10/scenario02/config/game/assets/scenario/config/export/main/asset/assets.xml", "AssetList/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Assets/Asset"),
                new("data/dlc10/scenario02/config/game/assets/scenario/config/export/main/asset/assets.xml", "AssetList/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Assets/Asset"),
                new("data/dlc10/scenario02/config/game/assets/scenario/config/export/main/asset/assets.xml", "AssetList/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Assets/Asset"),
                new("data/dlc10/scenario02/config/game/assets/scenario/config/export/main/asset/assets.xml", "AssetList/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Assets/Asset"),
                new("data/dlc10/scenario02/config/game/assets/scenario/config/export/main/asset/assets.xml", "AssetList/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Assets/Asset"),
                // Scenario 03 Extra / Changed Buildings list
                new("data/dlc11/scenario03/config/game/assets/scenario/config/export/main/asset/assets.xml", "AssetList/Groups/Group/Groups/Group/Groups/Group/Assets/Asset"),
                new("data/dlc11/scenario03/config/game/assets/scenario/config/export/main/asset/assets.xml", "AssetList/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Assets/Asset"),
                new("data/dlc11/scenario03/config/game/assets/scenario/config/export/main/asset/assets.xml", "AssetList/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Assets/Asset"),
                new("data/dlc11/scenario03/config/game/assets/scenario/config/export/main/asset/assets.xml", "AssetList/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Assets/Asset"),
                new("data/dlc11/scenario03/config/game/assets/scenario/config/export/main/asset/assets.xml", "AssetList/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Assets/Asset"),
                new("data/dlc11/scenario03/config/game/assets/scenario/config/export/main/asset/assets.xml", "AssetList/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Groups/Group/Assets/Asset")
            ]);
        }
        #endregion

        #endregion

        #region Prepare JSON Files
        if (annoVersion != "-ALL")
        {
            //execute a Single Anno Preset
            ValidateIconFile("Test.png", "TestBuilding", "Checker"); // Enable the WriteFile system for Missing Icons if Directory Exists
            DoAnnoPreset(annoVersion, addRoads: true);
        }
        else
        {
            //Execute for all Anno Presets in one
            ValidateIconFile("Test.png", "TestBuilding", "Checker"); // Enable the WriteFile system for Missing Icons if Directory Exists
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

        #region Validate list of buildings

        ValidateBuildings(buildings);

        #endregion

        #region Write preset.json and icon.json files
        BuildingPresets presets = new() { Version = BUILDING_PRESETS_VERSION, Buildings = buildings.Cast<BuildingInfo>().ToList() };

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
        // wait for key-press before exiting
        Console.WriteLine("This list contains {0} Buildings", annoBuildingsListCount);
        Console.WriteLine();
        #region Make DVDataFile "DuxVitae-Replaced.csv" for Convert Tool by DuxVitae
        if (annoVersion == "-ALL" || annoVersion == Constants.ANNO_VERSION_1800) // Only for Anno 1800 purpose
        {
            // This file is need to get the right building objects from the presets when GUID's are removed, not added,
            // or replaced by other objects, so DuxVitae can use this file to pinpoint the right GUID's in the presets.  
            Console.WriteLine("Saving " + DVFileName + " for converting tool purpose by DuxVitae");
            StreamWriter DVFile = new(DVFileName);
            DVFile.WriteLine("This File is based on Anno Designer Presets version ");
            DVFile.WriteLine(BUILDING_PRESETS_VERSION);
            DVFile.WriteLine("------------------------------------------------------");
            DVFile.WriteLine("GUID , Icon Filename , IdentifierName , Replaced Guids");
            #region Adding manual to existing DVDataListas 
            //This is need, because no other option is possible to automate this
            DVDataList[24829] = DVDataList[24829] + ",24828";
            DVDataList[100416] = DVDataList[100416] + ",101267";
            DVDataList[101061] = DVDataList[101061] + ",100417";
            DVDataList[112691] = DVDataList[112691] + ",113750";
            DVDataList[129024] = DVDataList[129024] + ",129025";
            DVDataList[102151] = DVDataList[102151] + ",101516";
            DVDataList[101498] = DVDataList[101498] + ",102093";
            DVDataList[102131] = DVDataList[102131] + ",102112";
            DVDataList[102892] = DVDataList[102892] + ",103049";
            DVDataList[102383] = DVDataList[102383] + ",103048";
            DVDataList[102229] = DVDataList[102229] + ",103047";
            DVDataList[114435] = DVDataList[114435] + ",117633";
            DVDataList[1010310] = DVDataList[1010310] + ",101303";
            #endregion
            foreach (string DVData in DVDataList)
            {
                if (!String.IsNullOrEmpty(DVData))
                {
                    string[] DVDataCheck = DVData.Split(',');
                    if ((DVDataCheck.Length > 3))
                    {
                        int DVDataGUID = Convert.ToInt32(DVDataCheck[0]);
                        DVFile.WriteLine(DVDataList[DVDataGUID]);
                    }
                }
            }
            DVFile.Close();
            Console.WriteLine("File Saved.");
            Console.WriteLine();
            //End of DVDataFile

        }
        #endregion
        PPTNFile.Close(); // Closing the TextWriter for Skipped TemplateName. Is only used by Anno 1800 but opened globally. 
        IconNotExistFile.Close(); //Close the TextWriter for Missing Icon Files
        Console.WriteLine("Do not forget to copy the contents to the normal");
        Console.WriteLine("presets.json, in the Anno Designer directory!");
        Console.WriteLine();
        Console.WriteLine("DONE - press enter to exit");
        Console.ReadLine();
        #endregion //End Prepare JSON Files0
    }

    #region Validate process for Double Identifiers 
    private static void ValidateBuildings(List<IBuildingInfo> buildingsToCheck)
    {
        // This list contains identifiers which are duplicated on purpose (on various places inside the preset tree) and known to not cause any errors (e.g. translation or statistics).
        List<string> knownDuplicates = [ "Logistic_02 (Warehouse I)", "Residence_Old_World", "Residence_tier02", "Residence_tier03", "Residence_tier04",
            "Residence_tier05", "Residence_tier05b", "Residence_New_World", "Residence_colony01_tier02", "Residence_Arctic_World", "Residence_arctic_tier02",
            "Residence_Africa_World", "Residence_colony02_tier02" ];

        Validator validator = new();
        (bool isValid, List<string> duplicateIdentifiers) = validator.CheckForUniqueIdentifiers(buildingsToCheck, knownDuplicates);
        ConsoleColor oldColor = Console.ForegroundColor;
        if (!isValid)
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine();
                Console.WriteLine($"### There are duplicate identifiers ({duplicateIdentifiers.Count}) ###");
                foreach (string curDuplicateIndentifier in duplicateIdentifiers)
                {
                    Console.WriteLine(curDuplicateIndentifier);
                }
                Console.WriteLine();
            }
            finally
            {
                Console.ForegroundColor = oldColor;
            }
        }
        else
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Green;

                Console.WriteLine();
                Console.WriteLine("There are no duplicate Identifiers.");
                Console.WriteLine();
            }
            finally
            {
                Console.ForegroundColor = oldColor;
            }
        }
    }
    #endregion

    #region  Validate process for Icon Files Existence
    // This will Validate if the IconFileName is exists in the Icons directory, when it is not,
    // it will be written in a file IconMissing.txt so i can easy search for them in the Assets FIles
    // (Made 14-06-2022) 
    private static void ValidateIconFile(string IconFileName, string GUID_or_IdentifierName, string BuildingHeader)
    {
        var _fs = new FileSystem();
        if (!canCheckIconFiles && !checkIconFilePathDone)
        {
            string workingDirectory = Environment.CurrentDirectory;
            string WorkPathRepo = _fs.Directory.GetParent(workingDirectory).Parent.Parent.Parent.FullName;
            IconFileCheckPath = WorkPathRepo + "\\AnnoDesigner\\icons";
            string IconFilePathExists = $"{IconFileCheckPath}";
            if (_fs.Directory.Exists(IconFilePathExists))
            {
                canCheckIconFiles = true;
                checkIconFilePathDone = true;
                Console.WriteLine("");
                Console.WriteLine("IconFileNames will be checked if they exist in the following directory:");
                Console.WriteLine(IconFileCheckPath);
                IconNotExistFile.WriteLine("This file is created by PresetParser " + DateTime.Now + ",");
                IconNotExistFile.WriteLine("based on Preset.json creation version " + BUILDING_PRESETS_VERSION);
            }
            else
            {
                canCheckIconFiles = false;
                checkIconFilePathDone = true;
                Console.WriteLine("");
                Console.WriteLine("IconFileNames will NOT checked on existence");
                IconNotExistFile.Close(); //Close the open TextWriter for Missing Icon Files as we not need it
            }
        }
        if (canCheckIconFiles && IconFileName != "Test.png")
        {
            string IconFileToCheck = $"{IconFileCheckPath}\\{IconFileName}";
            if (!_fs.File.Exists(IconFileToCheck))
            {
                // Put here the GUID's or IdentifierName that may skipped, in a LIST String Format.
                // Those will skipped by checking if file exists, as they have no IconfileName, 
                // or not need an Icon anyway (i.e. Street kind objects and Quay Objects) 
                IconIgnoreGuidOrIdentifier = ["harbourprops", "Harboursystem", "HarbourSystem"];
                if (GUID_or_IdentifierName.IsMatch(IconIgnoreGuidOrIdentifier))
                {
                    return;
                }
                // write the Presets Building Header (anno version), GUID/Identifier and IconFileName to IconsMissing FIle
                // to find the buildings in the Anno Presets file, and know which anno version it is.
                IconNotExistFile.WriteLine(BuildingHeader + " || " + GUID_or_IdentifierName + " - " + IconFileName);
            }
        }
    }
    #endregion

    // Get the BASE_PATH for the given Anno
    #region Asking Directory path for the choices Anno versions
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
            if (new FileSystem().Directory.Exists(path))
            {
                validPath = true;
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("Invalid input, please try again or enter 'quit' to exit.");
            }
        }
        // Add a trailing backslash if one is not present.
        return path.LastOrDefault() == '\\' ? path : path + "\\";
    }
    #endregion

    // Prepare building list for preset/icon file
    #region Prepare buildings list for presets, depending on the Anno Version thats given
    private static void DoAnnoPreset(string annoVersion, bool addRoads)
    {
        Console.WriteLine();
        Console.WriteLine("Parsing assets.xml:");
        PathRef[] assetPathRefs = VersionSpecificPaths[annoVersion]["assets"];
        #region Start prepare Anno 1404 or Anno 2070
        if (annoVersion == Constants.ANNO_VERSION_1404 || annoVersion == Constants.ANNO_VERSION_2070)
        {
            // prepare localizations
            // This call will set the extra Anno Version Number on the icon translation for Anno 1404 ('A4_')
            // and Anno 2070 ('A5_') that will be seen in the Icons Selection tree of the program (icons.json)
            Dictionary<string, SerializableDictionary<string>> localizations = _localizationHelper.GetLocalization(annoVersion,
                addPrefix: true,
                VersionSpecificPaths,
                Languages,
                BASE_PATH);

            #region Preparing icon.json file

            // prepare icon mapping
            XmlDocument iconsDocument = new();
            List<XmlNode> iconNodes = [];
            foreach (PathRef p in VersionSpecificPaths[annoVersion]["icons"])
            {
                iconsDocument.Load(BASE_PATH + p.Path);
                iconNodes.AddRange(iconsDocument.SelectNodes(p.XPath).Cast<XmlNode>());
            }

            // write icon name mapping
            Console.WriteLine("Writing icon name mapping to icons.json");
            IconNameMapper iconNameMapper = new();
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
                AddBlockingTiles(buildings);
            }
        }
        #endregion

        #region Start prepare Anno 2205
        else if (annoVersion == Constants.ANNO_VERSION_2205)
        {
            #region Read in Language Files
            Console.WriteLine("Parsing Language files....");
            // To boost performances we need to read in all supported language files into programs memory.
            string languageFileName; // This will be given thou the static LanguagesFiles array
            string languageFilePath = "data/config/gui/";
            string languageFileStart = "texts_";
            int languageCount = 0;
            foreach (string Language in Languages)
            {
                languageFileName = BASE_PATH + languageFilePath + languageFileStart + LanguagesFiles2205[languageCount] + ".xml";
                XmlDocument langDocument = new();
                langDocument.Load(languageFileName);
                switch (languageCount)
                {
                    case 0: { langDocument_english = langDocument; break; }
                    case 1: { langDocument_german = langDocument; break; }
                    case 2: { langDocument_french = langDocument; break; }
                    case 3: { langDocument_polish = langDocument; break; }
                    case 4: { langDocument_russian = langDocument; break; }
                    case 5: { langDocument_spanish = langDocument; break; }
                }
                languageCount++;
            }
            #endregion;

            Console.WriteLine("Parsing buildings...");
            foreach (PathRef p in assetPathRefs)
            {
                ParseAssetsFile2205(BASE_PATH + p.Path, p.XPath, p.YPath, buildings, p.InnerNameTag, annoVersion);
            }
            // Add extra buildings to the anno version preset file
            AddExtraPreset(annoVersion, buildings);
            if (addRoads)
            {
                AddExtraRoads(buildings);
                AddBlockingTiles(buildings);
            }
        }
        #endregion

        #region Start prepare Anno 1800
        else if (annoVersion == Constants.ANNO_VERSION_1800)
        {
            #region Start adding DVDataList Buildings that are not created in order
            // Preparing Preset DataList GUIDS, i need to create this here, as
            // i need to insert all GUIDs of the Modules from those buildings
            // and some other buildings that have more levels like harbors, World Fair etc
            DVDataList[100455] = "100455,A7_Zoo module.png,Culture_01_module_06_empty";
            DVDataList[100454] = "100454,A7_Museum module.png,Culture_02_module_06_empty";
            DVDataList[111104] = "111104,A7_botanic_module.png,C03_M06_empty";
            DVDataList[113452] = "113452,A7_music_pavillion.png,Entertainment_musicpavillion_empty";
            DVDataList[112685] = "112685,A7_airship_hangar.png,Monument_arctic_01_00";
            DVDataList[132765] = "132765,A7_eiffel_tower.png,Tourist_monument_00";
            DVDataList[118938] = "118938,A7_research_center.png,ResearchCenter_01";
            DVDataList[1010371] = "1010371,A7_warehouse.png,Logistic_02(Warehouse I)";
            DVDataList[1010540] = "1010540,A7_kontor_main.png,Kontor_imperial_01";
            DVDataList[100783] = "100783,A7_oil_habour_01.png,Harbor_14a (Oil Harbor I)";
            DVDataList[100429] = "100429,A7_visitor_harbour.png,Harbor_09 (tourism_pier_01)";
            DVDataList[686] = "686,A7_dam_a.png,GGJDam_01_03";
            DVDataList[1372] = "1372,A7_bauxit.png,Mining_20_slot (Bauxite Ore Mine),1308";
            DVDataList[1375] = "1375,A7_helium.png,Mining_21_slot (SA GasWell),1353";
            PPTNFile.WriteLine("This File is created with the Anno Designer Presets version: " + BUILDING_PRESETS_VERSION);
            PPTNFile.WriteLine("-----------------------------------------------------------> TSL <-");

            #endregion

            #region Read in Language Files
            // To boost performances we need to read in all supported language files into programs memory.
            Console.WriteLine("Parsing Language files....");
            string languageFileName; // This will be given thou the static LanguagesFiles array
            string languageFilePath = "data/config/gui/";
            string languageFileStart = "texts_";
            int languageCount = 0;
            foreach (string Language in Languages)
            {
                languageFileName = BASE_PATH + languageFilePath + languageFileStart + LanguagesFiles1800[languageCount] + ".xml";
                XmlDocument langDocument = new();
                langDocument.Load(languageFileName);
                switch (languageCount)
                {
                    case 0: { langDocument_english = langDocument; break; }
                    case 1: { langDocument_german = langDocument; break; }
                    case 2: { langDocument_french = langDocument; break; }
                    case 3: { langDocument_polish = langDocument; break; }
                    case 4: { langDocument_russian = langDocument; break; }
                    case 5: { langDocument_spanish = langDocument; break; }
                }
                languageCount++;
            }
            #endregion

            string PPTNFileXPath = "";
            Console.WriteLine("Parsing buildings...");
            foreach (PathRef p in assetPathRefs)
            {
                // To know in what assets file is the Missing Template Name
                // for the missing TemplateNames file (20-09-2022) 
                if (p.Path != PPTNFileXPath)
                {
                    PPTNFile.WriteLine("XPath : " + p.Path + ": ");
                    PPTNFileXPath = p.Path;
                }
                ParseAssetsFile1800(BASE_PATH + p.Path, p.XPath, buildings);
            }

            // Add extra buildings to the anno version preset file
            AddExtraPreset(annoVersion, buildings);

            // Whatever Annoversion is "-ALL" or "1800", add the Extra Roads Bars 
            if (addRoads)
            {
                AddExtraRoads(buildings);
                AddBlockingTiles(buildings);
            }
        }
        #endregion
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
        foreach (ExtraPreset curExtraPreset in ExtraPresets.GetExtraPresets(annoVersion))
        {
            BuildingInfo buildingToAdd = new()
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
                Guid = curExtraPreset.Guid,
            };

            Console.WriteLine("Extra Building: {0}", buildingToAdd.Identifier);

            buildingToAdd.BuildBlocker = new SerializableDictionary<int>();
            buildingToAdd.BuildBlocker["x"] = curExtraPreset.BuildBlockerX;
            buildingToAdd.BuildBlocker["z"] = curExtraPreset.BuildBlockerZ;

            buildingToAdd.Localization = new SerializableDictionary<string>();
            buildingToAdd.Localization["eng"] = curExtraPreset.LocaEng;
            buildingToAdd.Localization["ger"] = curExtraPreset.LocaGer;
            buildingToAdd.Localization["fra"] = curExtraPreset.LocaFra;
            buildingToAdd.Localization["pol"] = curExtraPreset.LocaPol;
            buildingToAdd.Localization["rus"] = curExtraPreset.LocaRus;
            buildingToAdd.Localization["esp"] = curExtraPreset.LocaEsp;

            annoBuildingsListCount++;

            buildings.Add(buildingToAdd);
        }
    }

    private static void AddExtraRoads(List<IBuildingInfo> buildings)
    {
        foreach (ExtraRoads curExtraRoad in ExtraPresets.GetExtraRoads())
        {
            BuildingInfo buildingToAdd = new()
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

            Console.WriteLine("Extra Road Bar: {0}", buildingToAdd.Identifier);

            buildingToAdd.BuildBlocker = new SerializableDictionary<int>();
            buildingToAdd.BuildBlocker["x"] = curExtraRoad.BuildBlockerX;
            buildingToAdd.BuildBlocker["z"] = curExtraRoad.BuildBlockerZ;

            buildingToAdd.Localization = new SerializableDictionary<string>();
            buildingToAdd.Localization["eng"] = curExtraRoad.LocaEng;
            buildingToAdd.Localization["ger"] = curExtraRoad.LocaGer;
            buildingToAdd.Localization["fra"] = curExtraRoad.LocaFra;
            buildingToAdd.Localization["pol"] = curExtraRoad.LocaPol;
            buildingToAdd.Localization["rus"] = curExtraRoad.LocaRus;
            buildingToAdd.Localization["esp"] = curExtraRoad.LocaEsp;

            annoBuildingsListCount++;

            buildings.Add(buildingToAdd);
        }
    }

    private static void AddBlockingTiles(List<IBuildingInfo> buildings)
    {
        foreach (BlockingTile curBlockingTile in ExtraPresets.GetBlockingTiles())
        {
            BuildingInfo buildingToAdd = new()
            {
                Header = curBlockingTile.Header,
                Faction = curBlockingTile.Faction,
                Group = curBlockingTile.Group,
                IconFileName = curBlockingTile.IconFileName,
                Identifier = curBlockingTile.Identifier,
                InfluenceRadius = curBlockingTile.InfluenceRadius,
                InfluenceRange = curBlockingTile.InfluenceRange,
                Template = curBlockingTile.Template,
                Road = curBlockingTile.Road,
                Borderless = curBlockingTile.Borderless,
            };

            Console.WriteLine("Extra Blocker: {0}", buildingToAdd.Identifier);

            buildingToAdd.BuildBlocker = new SerializableDictionary<int>();
            buildingToAdd.BuildBlocker["x"] = curBlockingTile.BuildBlockerX;
            buildingToAdd.BuildBlocker["z"] = curBlockingTile.BuildBlockerZ;

            buildingToAdd.Localization = new SerializableDictionary<string>();
            buildingToAdd.Localization["eng"] = curBlockingTile.LocaEng;
            buildingToAdd.Localization["ger"] = curBlockingTile.LocaGer;
            buildingToAdd.Localization["fra"] = curBlockingTile.LocaFra;
            buildingToAdd.Localization["pol"] = curBlockingTile.LocaPol;
            buildingToAdd.Localization["rus"] = curBlockingTile.LocaRus;
            buildingToAdd.Localization["esp"] = curBlockingTile.LocaEsp;

            annoBuildingsListCount++;

            buildings.Add(buildingToAdd);
        }
    }

    #endregion

    // Parsing Buildings Info 
    #region Parsing Buildings for Anno 1404/2070

    private static void ParseAssetsFile(string filename, string xPathToBuildingsNode, string YPath, List<IBuildingInfo> buildings,
        IEnumerable<XmlNode> iconNodes, Dictionary<string, SerializableDictionary<string>> localizations, string innerNameTag, string annoVersion)
    {
        XmlDocument assetsDocument = new();
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

        XmlElement values = buildingNode["Values"];
        string nameValue = values["Standard"]["Name"].InnerText;
        string templateValue = buildingNode["Template"].InnerText;

        string guidValue = values["Standard"]?["GUID"].InnerText;
        if (string.IsNullOrEmpty(guidValue))
        {
            guidValue = "0";
        }

        #region Skip Unused buildings in Anno Designer List

        bool isExcludedFaction = false;

        // to get 2 buildings from OrnamentFeedbackBuilding, all other OrnamentFeedbackBuildings will be skipped
        if (templateValue == "OrnamentFeedbackBuilding" && annoVersion == Constants.ANNO_VERSION_2070)
        {
            if (guidValue != "7110000" && guidValue != "7110001")
            {
                return;
            }
            else
            {
                templateValue = "Statistics_Building";
            }
        }

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

        string identifierName = values["Standard"]["Name"].InnerText;

        if (nameValue == "underwater markethouse" && nameValue.IsPartOf(annoBuildingLists))
        {
            identifierName = "underwater markethouse II";
        }

        #endregion

        // Parse Stuff
        string factionName = buildingNode.ParentNode.ParentNode.ParentNode.ParentNode["Name"].InnerText.FirstCharToUpper();
        string groupName = buildingNode.ParentNode.ParentNode["Name"].InnerText.FirstCharToUpper();

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

        string headerName = "Anno" + annoVersion;//in case of if statements are passed by

        if (annoVersion == Constants.ANNO_VERSION_1404)
        {
            headerName = "(A4) Anno " + Constants.ANNO_VERSION_1404;
        }
        else if (annoVersion == Constants.ANNO_VERSION_2070)
        {
            headerName = "(A5) Anno " + Constants.ANNO_VERSION_2070;
        }

        #endregion

        BuildingInfo b = new()
        {
            Header = headerName,
            Faction = factionName,
            Group = groupName,
            Template = templateValue,
            Identifier = identifierName,
            Guid = Convert.ToInt32(guidValue),
        };

        //Place both statistic Buildings into Others > Statistic Buildings (anno 2070 - 29-06 2022)
        if ((b.Guid == 7110000 || b.Guid == 7110001) && annoVersion == Constants.ANNO_VERSION_2070)
        {
            b.Faction = "Others";
            b.Group = "Statistic Buildings";
        }
        //Skip unused Underwater Warehouse (3x6)
        if (annoVersion == Constants.ANNO_VERSION_2070 && b.Guid == 10035)
        {
            return;
        }

        // print progress
        Console.WriteLine(b.Identifier + " -- " + Convert.ToString(b.Guid));

        #endregion

        #region Get Building Blocker Information
        if (!_buildingBlockProvider.GetBuildingBlocker(BASE_PATH, b, values["Object"]["Variations"].FirstChild["Filename"].InnerText, annoVersion))
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
            b.IconFileName = _iconFileNameHelper.GetIconFilename(icon["Icons"].FirstChild, annoVersion);
        }

        #endregion

        #region Get Influence Radius is Existing
        try
        {
            b.InfluenceRadius = Convert.ToInt32(values["Influence"]?["InfluenceRadius"]?.InnerText);
        }
        catch (NullReferenceException)
        { }
        #region Set InfluenceRange to 0, as this not exist in Anno 1404 and Anno 2070
        //because this number does not exist yet, we set this to zero
        b.InfluenceRange = 0;
        #endregion
        #endregion

        #region Get Translations for Building Names
        int languageCount = 0;
        if (localizations.TryGetValue(buildingGuid, out SerializableDictionary<string> value))
        {
            b.Localization = value;
            #region Change default known language text (can be used for anno 1404 and 2070).
            // if something need to be added or changed to the languages, put guid and header as checker, 
            // and make the changes you want.
            #region ANNO 1404:
            //if (annoVersion == Constants.ANNO_VERSION_1404)
            //{
            //    // Nothing to translate yet
            //}
            #endregion

            #region ANNO 2070:
            if (annoVersion == Constants.ANNO_VERSION_2070)
            {
                //Add extra name to the translation of Rice Paddles for the Distillery
                if (b.Guid == 10047)
                {
                    foreach (string Language in Languages)
                    {
                        switch (languageCount)
                        {
                            case 0: b.Localization[Languages[languageCount]] = b.Localization[Languages[languageCount]] + " (Distillery)"; break;
                            case 1: b.Localization[Languages[languageCount]] = b.Localization[Languages[languageCount]] + " (Spirituosenfabrik)"; break;
                            case 2: b.Localization[Languages[languageCount]] = b.Localization[Languages[languageCount]] + " (Distillerie)"; break;
                            case 3: b.Localization[Languages[languageCount]] = b.Localization[Languages[languageCount]] + " (Destylarnia)"; break;
                            case 4: b.Localization[Languages[languageCount]] = b.Localization[Languages[languageCount]] + " (Перегонный завод)"; break;
                            case 5: b.Localization[Languages[languageCount]] = b.Localization[Languages[languageCount]] + " (Destilería)"; break;
                        }
                        languageCount++;
                    }
                }
                //put tier number before translation of residences for anno 2070
                if (b.Guid == 10011 || b.Guid == 10021 || b.Guid == 10088)
                {
                    foreach (string Language in Languages)
                    {
                        b.Localization[Languages[languageCount]] = "(1) " + b.Localization[Languages[languageCount]];
                        languageCount++;
                    }
                }
                if (b.Guid == 10013 || b.Guid == 10076 || b.Guid == 10209)
                {
                    foreach (string Language in Languages)
                    {
                        b.Localization[Languages[languageCount]] = "(2) " + b.Localization[Languages[languageCount]];
                        languageCount++;
                    }
                }
                if (b.Guid == 10119 || b.Guid == 10116 || b.Guid == 40000006)
                {
                    foreach (string Language in Languages)
                    {
                        b.Localization[Languages[languageCount]] = "(3) " + b.Localization[Languages[languageCount]];
                        languageCount++;
                    }
                }
                if (b.Guid == 10117 || b.Guid == 10118)
                {
                    foreach (string Language in Languages)
                    {
                        b.Localization[Languages[languageCount]] = "(4) " + b.Localization[Languages[languageCount]];
                        languageCount++;
                    }
                }
            }
            #endregion

            #endregion
        }
        else
        {
            Console.WriteLine("No Translation found, it will be set to Identifier.");

            b.Localization = new SerializableDictionary<string>();

            string translation = values["Standard"]["Name"].InnerText;//TODO use identifierName?

            foreach (string Language in Languages)
            {
                b.Localization.Dict.Add(Languages[languageCount], translation);
                languageCount++;
            }
        }
        // removing the iconFileName for the Quay Walls (on Identifier) in Anno 1404 and 2070 
        if (b.Identifier.Equals("harboursystem", StringComparison.CurrentCultureIgnoreCase))
        {
            b.IconFileName = null;
        }
        #endregion

        //comment out this line below only when you need to lookup GUID's to change translations in either anno 1404 or 2070
        b.Guid = 0; //set the Building GUID back to 0, as we not need to use them anymore

        //Because GUIDs are not used in 1404 and 2070, send an Identifier to the IconFile checker to find the object in the presests.json
        ValidateIconFile(b.IconFileName, b.Identifier, b.Header);

        // add building to the list(s)
        annoBuildingsListCount++;
        annoBuildingLists.Add(values["Standard"]["Name"].InnerText);//TODO use identifierName?
        buildings.Add(b);
    }

    #endregion

    #region Parsing Buildings for Anno 2205

    private static void ParseAssetsFile2205(string filename, string xPathToBuildingsNode, string YPath, List<IBuildingInfo> buildings, string innerNameTag, string annoVersion)
    {
        XmlDocument assetsDocument = new();
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

        XmlElement values = buildingNode["Values"];
        string nameValue = values["Standard"]["Name"].InnerText;
        string templateValue = buildingNode["Template"].InnerText;
        string guidValue = values["Standard"]?["GUID"].InnerText;
        if (string.IsNullOrEmpty(guidValue))
        {
            guidValue = "0";
        }

        #region Skip Unused buildings in Anno Designer List
        //Skip Energy Connector Top Object (no field, nor object | 01-07-2022) 
        if (guidValue == "1003535" || guidValue == "1002878" || guidValue == "1001410" || guidValue == "13000158" || guidValue == "13000424")
        {
            return;
        }

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
        string identifierName = values["Standard"]["Name"].InnerText;

        string factionName = buildingNode.ParentNode.ParentNode.ParentNode.ParentNode["Name"].InnerText.FirstCharToUpper();
        string groupName = buildingNode.ParentNode.ParentNode["Name"].InnerText.FirstCharToUpper();

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

        string headerName = "(A6) Anno " + Constants.ANNO_VERSION_2205;

        BuildingInfo b = new()
        {
            Header = headerName,
            Faction = factionName,
            Group = groupName,
            Template = templateValue,
            Identifier = identifierName,
            Guid = Convert.ToInt32(guidValue),
        };

        // print progress
        if (!testVersion)
        {
            Console.WriteLine(b.Identifier + " -- " + b.Guid);
        }

        #endregion

        #region Get/Set InfluenceRange information

        // New 29-09-2020 : Head shield generation into radius parameter, on request #296
        // Read the xml key : <ShieldGenerator> / <ShieldedRadius> for heating arctic buildings (raw number)
        //                    and Moon Shield Generators
        b.InfluenceRadius = Convert.ToInt32(values?["ShieldGenerator"]?["ShieldedRadius"]?.InnerText);
        // read the xml key : <Energy> / <RadiusUsed> and then divide by 4096 for the training centers
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
                    Console.WriteLine("-<BuidBlocker> Tag not found in Object File!");
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
            Console.WriteLine("-BuildBlocker not found, skipping: Object File Information not fount");
            return;
        }

        #endregion

        #region Get IconFilenames

        // find icon node in values (different vs 1404/2070)
        string icon = null;
        if (values["Standard"]?["IconFilename"]?.InnerText != null)
        {
            icon = values["Standard"]["IconFilename"].InnerText;
        }

        if (icon != null)
        {
            // Split the Value <IconFilenames>innertext</IconFilenames> to get only the Name.png
            b.IconFileName = icon.Split('/').LastOrDefault().Replace("icon_", "A6_");

            // Add Module Field Icons to the Modules of the Facilities, to distinguish Module Fields vs Farm-Buildings. (29-06-2022)
            // Icons are added to the Icon Folder, the icons is based on the main building icon, but has the Anno 1800 module icon
            // included, and this change the names of the IconFileName to pick the right Icon for the fields.
            if (b.Faction == "Facility Modules")
            {
                b.IconFileName = b.IconFileName.Replace(".png", "_module.png");
            }
        }
        else
        {
            b.IconFileName = null;
        }

        #endregion

        #region Get localizations            

        //Initialize the dictionary
        string langNodeStartPath = "/TextExport/Texts/Text";
        string langNodeDepth = "Text";
        int languageCount = 0;
        b.Localization = new SerializableDictionary<string>();

        foreach (string Language in Languages)
        {
            // Changed because of performance issues (06-06-2022)
            XmlDocument langDocument = new();
            switch (languageCount)
            {
                case 0: { langDocument = langDocument_english; break; }
                case 1: { langDocument = langDocument_german; break; }
                case 2: { langDocument = langDocument_french; break; }
                case 3: { langDocument = langDocument_polish; break; }
                case 4: { langDocument = langDocument_russian; break; }
                case 5: { langDocument = langDocument_spanish; break; }
            }

            string translation = "";

            // To get the right residence building inhabitants name, instead of region residence building name (anno 2205, 01-07-2022)
            // if values["Residence"]["PopulationLevel"] has a value, then put translation buidingGuid to that translation GUID
            if (!string.IsNullOrEmpty(values?["Residence"]?["PopulationLevel"].InnerText))
            {
                buildingGuid = values["Residence"]["PopulationLevel"].InnerText;
            }

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
                    if (languageCount == 4) { translation = "Almacén de depósito (4x4)"; }
                }
                if (buildingGuid == "7000426")
                {
                    if (languageCount == 0) { translation = "Storage Depot (2x2)"; }
                    if (languageCount == 1) { translation = "Lager (2x2)"; }
                    if (languageCount == 2) { translation = "Magazyn (2x2)"; }
                    if (languageCount == 3) { translation = "Хранилище (2x2)"; }
                    if (languageCount == 4) { translation = "Almacén de depósito (2x2)"; }
                }

                # region Set tier numbers and mesurements on the residence buildings (02-07-2022)
                //Small Residences (3x3, Temperate only)
                if (b.Guid == 1000005 || b.Guid == 1000152 || b.Guid == 1000153 || b.Guid == 1000154)
                {
                    translation += " (3x3)";
                }
                //Small Residences (6x6, Temperate only)
                if (b.Guid == 1000151 || b.Guid == 1000192 || b.Guid == 1000193 || b.Guid == 1000194 || b.Guid == 13000388)
                {
                    translation += " (6x6)";
                }
                //Tier numbers 1 (all regions)
                if (b.Guid == 1000005 || b.Guid == 1000151 || b.Guid == 1000247 || b.Guid == 1000183 || b.Guid == 7000007)
                {
                    translation = $"(1) {translation}";
                }
                //Tier numbers 2 (all regions)
                if (b.Guid == 1000152 || b.Guid == 1000192 || b.Guid == 1000248 || b.Guid == 1000184 || b.Guid == 7000008)
                {
                    translation = $"(2) {translation}";
                }
                //Tier numbers 3 (Temperate only)
                if (b.Guid == 1000153 || b.Guid == 1000193)
                {
                    translation = $"(3) {translation}";
                }
                //Tier numbers 4 (Temperate only)
                if (b.Guid == 1000154 || b.Guid == 1000194)
                {
                    translation = $"(4) {translation}";
                }
                //Tier numbers 5 (Temperate only)
                if (b.Guid == 13000388)
                {
                    translation = $"(5) {translation}";
                }
                #endregion

                if (translation == null)
                {
                    throw new InvalidOperationException("Cannot get translation, text node not found");
                }

                while (translation.Contains("GUIDNAME"))
                {
                    //"[GUIDNAME 2001009]",
                    //remove the [ and ] marking the GUID, and remove the GUIDNAME identifier.
                    string nextGuid = translation[1..^1].Replace("GUIDNAME", "").Trim();
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

        //comment out this line below only when you need to lookup GUID's to change/check buildings of Anno 2205
        b.Guid = 0; //set the Building GUID back to 0, as we not need to use them anymore

        //Because GUIDs are not used in 2205, send an Identifier to the IconFile checker to find the object in the presests.json
        ValidateIconFile(b.IconFileName, b.Identifier, b.Header);
        // add building to the list
        annoBuildingsListCount++;
        annoBuildingLists.Add(values["Standard"]["Name"].InnerText);
        buildings.Add(b);
    }

    #endregion

    #region Parsing Buildings for Anno 1800

    private static void ParseAssetsFile1800(string filename, string xPathToBuildingsNode, List<IBuildingInfo> buildings)
    {
        XmlDocument assetsDocument = new();
        assetsDocument.Load(filename);
        List<XmlNode> buildingNodes = assetsDocument.SelectNodes(xPathToBuildingsNode).Cast<XmlNode>().ToList();

        foreach (XmlNode buildingNode in buildingNodes)
        {
            ParseBuilding1800(buildings, buildingNode, Constants.ANNO_VERSION_1800);
        }
    }

    private static void ParseBuilding1800(List<IBuildingInfo> buildings, XmlNode buildingNode, string annoVersion)
    {
        string[] LanguagesFiles = [""];
        string templateName = "";
        string factionName = "";
        string identifierName = "";
        string groupName = "";
        string headerName = "(A7) Anno " + Constants.ANNO_VERSION_1800;
        int guidNumber = 0;
        ConsoleColor oldColor = Console.ForegroundColor;

        #region Get valid Building Information 

        XmlElement values = buildingNode["Values"]; //Set the value List as normally
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
                switch (firstChildName)
                {
                    case "BaseAssetGUID": templateName = buildingNode["BaseAssetGUID"].InnerText; break;
                    case "Template": templateName = buildingNode["Template"].InnerText; break;
                    case "ScenarioBaseAssetGUID": templateName = buildingNode["ScenarioBaseAssetGUID"].InnerText; break;
                }

                if (templateName == null)
                {
                    oldColor = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("--> No Template found, Building is skipped");
                    Console.ForegroundColor = oldColor;
                    return;
                }

                // to skip template names that are in the PPTNList (internal)
                if (templateName.IsMatch(PPTNList))
                {
                    return;
                }

                // If Template name is not in the Include LIsts, then write it to file, so i can search manually in the Assets.xml 
                // files to see if building needs to be added into this lists.
                if (!templateName.Contains(IncludeBuildingsTemplateNames1800) && !templateName.Contains(IncludeBuildingsTemplateGUID1800))
                {
                    if (!templateName.Contains(PPTNList) && !string.IsNullOrEmpty(templateName))
                    {
                        PPTNFile.WriteLine('"' + templateName + '"' + ',');
                        PPTNList.Add(templateName);
                    }
                    return;
                }

                if (!values.HasChildNodes)
                {
                    return;
                }
            }
        }

        string guidName = values["Standard"]?["GUID"]?.InnerText;
        if (!string.IsNullOrEmpty(guidName))
        {
            isExcludedGUID = guidName.Contains(ExcludeBuildingsGUID1800);
            guidNumber = Convert.ToInt32(values["Standard"]["GUID"].InnerText);
        }

        if (guidNumber == 0)
        {
            return;
        }

        //Skip the 2 following mines, as those are manual added to the DVDataList
        //1308 = second bauxit mine , 1353 = second helium mine
        if (guidNumber == 1308 || guidNumber == 1353)
        {
            return;
        }

        isExcludedTemplate = identifierName.Contains(PPTNList);

        if (string.IsNullOrEmpty(values["Standard"]?["Name"]?.InnerText))
        {
            oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("--> Error in Identifier Name : " + guidName + " >> " + templateName + ".");
            Console.ForegroundColor = oldColor;
            return;
        }

        identifierName = values["Standard"]["Name"].InnerText.FirstCharToUpper();
        isExcludedName = identifierName.Contains(ExcludeNameList1800);

        if (isExcludedName || isExcludedTemplate || isExcludedGUID)
        {
            return;
        }

        // Skip the Double list creating Docklands Ornaments, as they can placed in New world as well
        if (identifierName.Contains("SA_Docklands_Orna_")) { return; }

        // Because Game Dev's removed some items (DEPRECATED) and we still want them in AD,
        // so i remove this word from the identifierName strings (since Game Update 10) Change made 03-03-2021
        if (identifierName.Contains("DEPRECATED_"))
        {
            oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            identifierName = identifierName.Replace("DEPRECATED_", "");
            Console.WriteLine("--> Removed 'DEPRECATED_' to get object still in AD: ");
            Console.ForegroundColor = oldColor;
        }

        // Setting the factionname, thats the first menu after header
        string associatedRegion = "";
        associatedRegion = values?["Building"]?["AssociatedRegions"]?.InnerText;
        factionName = associatedRegion switch
        {
            "Moderate;Colony01" => "All Worlds",
            _ => associatedRegion.FirstCharToUpper(),
        };
        if (values?["Building"]?["BuildingType"]?.InnerText != null)
        {
            groupName = values["Building"]["BuildingType"].InnerText;
        }

        if (groupName == "")
        {
            groupName = "Not Placed Yet";
        }

        switch (templateName)
        {
            case "BuildPermitBuilding": if (!identifierName.StartsWith("GG_OldNate")) { factionName = "Ornaments"; groupName = "13 World's Fair Rewards"; } break;
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
            case "PalaceMinistry": { templateName = "PalaceBuilding"; factionName = "All Worlds"; groupName = "Special Buildings"; break; }
            case "1010517": { templateName = "SkyTradingPost"; factionName = "(11) Technicians"; groupName = "Public Buildings"; break; }
            case "FactoryBuilding7_BuildPermit": { factionName = "(13) Scholars"; groupName = "Permitted Buildings"; break; }
            case "HarborOrnament": { factionName = "Ornaments"; groupName = "22 Docklands Ornaments"; break; }
            default: { groupName = templateName.FirstCharToUpper(); break; }
        }

        if (groupName == "Farm Fields")
        {
            if (factionName == "Moderate") { factionName = "(06) Old World Fields"; groupName = null; }
            if (factionName == "All Worlds") { factionName = "(06) Old World Fields"; groupName = null; } // Error since GU14
            if (factionName == "Colony01") { factionName = "(09) New World Fields"; groupName = null; }
            if (factionName == "Arctic") { factionName = "(12) Arctic Farm Fields"; groupName = null; }
            if (factionName == "Africa") { factionName = "(16) Enbesa Farm Fields"; groupName = null; }
            if (factionName == "Africa;Colony01") { factionName = "(16) Enbesa Farm Fields"; groupName = null; } // Error since GU14
        }

        //Renaming the Fuel Station for Moderate (OW) site, to avoid double listed on Obreros tree
        if (factionName == "Moderate" && identifierName == "Fuel_station_01 (FuelStation)") { identifierName = "Moderate_fuel_station_01 (FuelStation)"; }

        switch (identifierName)
        {
            case "Silo (Grain)": { factionName = "(06) Old World Fields"; groupName = null; break; }
            case "Tractor_module_01 (Tractor)": { factionName = "(06) Old World Fields"; groupName = null; break; }
            case "Farm Fertilizer Module Moderate": { factionName = "(06) Old World Fields"; groupName = null; break; }
            case "Silo (Corn)": { factionName = "(09) New World Fields"; groupName = null; break; }
            case "Colony01_tractor_module_01 (Tractor)": { factionName = "(09) New World Fields"; groupName = null; break; }
            case "Farm Fertilizer Module Colony01": { factionName = "(09) New World Fields"; groupName = null; break; }
            case "Africa_silo (Teff)": { factionName = "(16) Enbesa Farm Fields"; groupName = null; break; }
            case "Africa_tractor_module_01 (Tractor)": { factionName = "(16) Enbesa Farm Fields"; groupName = null; break; }
            case "Farm Fertilizer Module Africa": { factionName = "(16) Enbesa Farm Fields"; groupName = null; break; }
            case "Entertainment_musicpavillion_empty": { factionName = "Attractiveness"; groupName = null; break; }
            case "Culture_01 (Zoo)": { factionName = "Attractiveness"; groupName = null; break; }
            case "Culture_02 (Museum)": { factionName = "Attractiveness"; groupName = null; break; }
            case "Culture_03 (BotanicalGarden)": { factionName = "Attractiveness"; groupName = null; break; }
            case "Monument_01_00": { factionName = "Attractiveness"; groupName = null; break; }
            case "Culture_1x1_plaza": { factionName = "Attractiveness"; groupName = "Modules"; break; }
            case "Residence_tier01": { factionName = "(01) Farmers"; identifierName = "Residence_Old_World"; groupName = "Residence"; break; }
            case "Residence_colony01_tier01": { factionName = "(07) Jornaleros"; identifierName = "Residence_New_World"; groupName = "Residence"; templateName = "ResidenceBuilding7"; break; }
            case "Residence_arctic_tier01": { factionName = "(10) Explorers"; identifierName = "Residence_Arctic_World"; groupName = "Residence"; break; }
            case "Residence_colony02_tier01": { factionName = "(14) Shepherds"; identifierName = "Residence_Africa_World"; groupName = "Residence"; templateName = "ResidenceBuilding7"; break; }
            case "Coastal_03 (Quartz Sand Coast Building)": { factionName = "All Worlds"; groupName = "Mining Buildings"; break; }
            case "Mining_arctic_02 (Gold Mine)": { factionName = "All Worlds"; groupName = "Mining Buildings"; break; }
            case "Electricity_03 (Gas Power Plant)": { factionName = "(05) Investors"; groupName = "Public Buildings"; break; }
            case "Event_ornament_historyedition": { factionName = "Ornaments"; groupName = "11 Special Ornaments"; break; }
            case "Hotel": { factionName = "(17) Tourists"; groupName = null; break; }
            case "Tourist_monument_00": { factionName = "(17) Tourists"; groupName = null; break; }
            case "Multifactory_Chemical_Blank": { factionName = "(17) Tourists"; groupName = null; break; }
            case "Bus Stop": { factionName = "(17) Tourists"; groupName = null; break; }
            case "HighLife_monument_00": { factionName = "(18) High Life"; groupName = null; break; }
            case "Random slot mining": { factionName = "All Worlds"; groupName = "Empty Slots"; break; }
            case "Mining_03_slot (Clay Pit Slot)": { factionName = "All Worlds"; groupName = "Empty Slots"; break; }
            case "Random slot oil pump": { factionName = "All Worlds"; groupName = "Empty Slots"; break; }
            case "Mining_arctic_01_slot (Gas Mine Slot)": { factionName = "All Worlds"; groupName = "Empty Slots"; break; }
            case "Oasis_Riverslot": { factionName = "All Worlds"; groupName = "Empty Slots"; break; }
            case "Agriculture_colony01_13 (Forestation)": { factionName = "(30) Scenario 1: Eden Burning"; groupName = "Farm Buildings"; templateName = "Scenario1"; ; break; }
            case "Coastal_02 (Water Purifier)": { factionName = "(30) Scenario 1: Eden Burning"; groupName = "Harbor Buildings"; templateName = "Scenario1"; break; }
        }

        // Place all TouristSeason Ornament in the right Tree Menu
        if (identifierName.Contains("TouristSeason Ornament") || identifierName.Contains("TouristSeason FlowerBed"))
        {
            factionName = "Ornaments";
            groupName = "23 Tourist Ornaments";
        }

        // Place the Tourist Restaurants in the right Tree Menu
        if (groupName == "Restaurant")
        {
            factionName = "(17) Tourists";
        }

        // Place all High Life Ornament in the right Tree Menu
        if (identifierName.Contains("HighLife Ornament") || identifierName.Contains("Fountain_system"))
        {
            factionName = "Ornaments";
            groupName = "24 High Life Ornaments";
        }

        // Place all High Life Productions Buildings in the right Tree Menu
        if (identifierName.Contains("Multifactory_SA_") || identifierName.Contains("Multifactory_Manufacturer_") || identifierName.Contains("Multifactory_Assembly_") || identifierName.Contains("Multifactory_Chemical_LaqcuerColor"))
        {
            factionName = "(18) High Life";
            groupName = "Production Buildings";
        }

        // Place all High Life Malls in the right Tree Menu 
        if (groupName == "Mall")
        {
            factionName = "(18) High Life";
        }

        // place all Hacienda buildings in the right menu : 
        if (identifierName.StartsWith("Hacienda"))
        {
            factionName = "(19) Seeds Of Change";
            switch (templateName)
            {
                case "HarborDepot": groupName = "Harbor Buildings"; break;
                case "ResidenceBuilding7_Colony": groupName = "Residences"; break;
                case "Multifactory": groupName = "Production Buildings"; break;
                case "BuffFactoryCulture": groupName = "Modules: Production"; break;
                case "OrnamentalBuilding": groupName = "Modules: Ornaments"; break;
                case "Hacienda": groupName = null; break;
            }
            //place the Hacienda universal farm blank is the right menu, and the rest of them 
            //point them to this building for DVDataList and do not add them in the tree
            if (templateName == "RecipeFarm")
            {
                if (guidNumber != 24794)
                {
                    DVDataList[24794] = DVDataList[24794] + "," + guidNumber;
                    return;
                }
                if (guidNumber == 24794)
                {
                    groupName = "Production Buildings";
                }
            }
        }
        switch (guidNumber)
        {
            case 24770: factionName = "(19) Seeds Of Change"; groupName = "Modules: Ornaments"; break;
            case 25224: factionName = "(19) Seeds Of Change"; groupName = "Modules: Ornaments"; break;
        }

        //Place all Orchards in the overall 'Orchards' tree menu 
        if (identifierName.Contains("TreePlanter_"))
        {
            factionName = "Orchards";
            groupName = null;
        }

        //place all Pedestrian Zone Pack CDLC Object in the right menu
        if (identifierName.Contains("PedestrianZone") || identifierName == "Groundplane System")
        {
            factionName = "Ornaments";
            groupName = "25 Pedestrian Zone";
        }

        // Empire of the Skies (DLC 20-09-2022)
        if (guidNumber == 835 || guidNumber == 648 || guidNumber == 1345 || guidNumber == 1418)
        {
            factionName = "(20) Empire of the Skies";
            groupName = "Production Buildings";
            if (guidNumber == 835)
            {
                templateName = "FactoryBuilding7";
            }
        }
        if (guidNumber == 1372 || guidNumber == 1375 || guidNumber == 2399)
        {
            factionName = "(20) Empire of the Skies";
            groupName = "Mining Buildings";
        }
        if (identifierName.StartsWith("DLC11 "))
        {
            factionName = "(20) Empire of the Skies";
            groupName = "Ornaments";
        }
        // skipp the old Post Offices of the Arctic, and replace with new one
        if (guidNumber == 112684)
        {
            DVDataList[4260] = "4260,A7_post_office.png,Service_arctic_02 (Post Office),112684";
            oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("---> Building added to Replacement List (DLC11 Replacement): 3327 << " + guidNumber);
            Console.ForegroundColor = oldColor;
            return;
        }
        if (guidNumber == 2654)
        {
            DVDataList[2654] = "2654,A7_airship_platform_southamerica.png,airship landing platform colony01,963";
            oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("---> Building added to Replacement List (Just adding right GUID): 2654 << 963 ");
            Console.ForegroundColor = oldColor;
            identifierName = "airship landing platform colony01";
            templateName = "AirshipPlatform";
        }

        // Put the Free Module Airmail Sorting Office (for old Platforms DLC03) in replacement file
        // Skipp this one and point this to the right module that is for the New Platforms (DLC11)
        if (guidNumber == 4513)
        {
            DVDataList[4259] = "4259,A7_airship_platform_post.png,Platform module post Passage,4513";
            oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("---> Building added to Replacement List (DLC11 Replacement): 4259 << " + guidNumber);
            Console.ForegroundColor = oldColor;
            return;
        }

        // move the new post office, post boxes and Airship Platforms to the right menu's
        if (guidNumber == 4260)
        {
            factionName = "(11) Technicians";
            groupName = "Public Buildings";
            identifierName = "Service_arctic_02 (Post Office)";
        }
        if (guidNumber == 538 || guidNumber == 962 || guidNumber == 3741)
        {
            factionName = "(03) Artisans";
            groupName = "Public Buildings";
        }
        if (guidNumber == 3661 || guidNumber == 3761 || guidNumber == 2654)
        {
            factionName = "(08) Obreros";
            groupName = "Public Buildings";
        }

        // Move the Airship Platforms to the right menus
        if (guidNumber == 4259)
        {
            factionName = "(11) Technicians";
            groupName = "Airship Platform Module";
        }
        if (guidNumber == 966 || guidNumber == 964)
        {
            factionName = "(03) Artisans";
            groupName = "Airship Platform Module";
        }
        if (guidNumber == 967 || guidNumber == 2274 || guidNumber == 2276)
        {
            factionName = "(08) Obreros";
            groupName = "Airship Platform Module";
        }

        // Move DLC11 Multi-factories on the right Menu's
        if (identifierName.StartsWith("Multifactory_Magazin(DropGoods)_Moderate_"))
        {
            factionName = "(03) Artisans";
        }
        if (identifierName.StartsWith("Multifactory_Magazin(DropGoods)_SA"))
        {
            factionName = "(08) Obreros";
        }

        //Scenario 1 : Eden Burning Ornamentals/Buildings 
        if (identifierName.StartsWith("GGJ_2x2_") || identifierName.StartsWith("Eoy21_Charity"))
        {
            factionName = "(30) Scenario 1: Eden Burning";
            groupName = "Ornaments";
            templateName = "Scenario1";
        }
        if (((guidNumber > 769 && guidNumber < 951) && guidNumber != 835 && !identifierName.StartsWith("Multifactory_Magazin(DropGoods)_")) || guidNumber == 686)
        {
            factionName = "(30) Scenario 1: Eden Burning";
            switch (templateName)
            {
                case "HeavyFreeAreaBuilding": groupName = "Production Buildings"; break;
                case "HeavyFactoryBuilding": groupName = "Production Buildings"; break;
                case "FactoryBuilding7": groupName = "Production Buildings"; break;
                case "101272": groupName = "Production Buildings"; break;
                case "101280": groupName = "Farm Fields"; break;
                case "101263": groupName = "Farm Buildings"; break;
            }
            templateName = "Scenario1";
        }
        if (guidNumber == 24134)
        {
            factionName = "(30) Scenario 1: Eden Burning";
            groupName = "Farm Buildings";
            templateName = "Scenario1";
        }
        if (guidNumber == 24136)
        {
            factionName = "(30) Scenario 1: Eden Burning";
            groupName = "Public Buildings";
            templateName = "Scenario1";
        }

        //Scenario 2 : Seasons of Silver Ornamentals/Buildings
        if ((identifierName.Contains("Scenario02")) || (identifierName == "Amoniac Factory") || (identifierName == "Cyanide Pool Module") || (identifierName == "Cyanide Leacher") || (identifierName == "SilverMint") || (identifierName == "SilverSmelter"))
        {
            factionName = "(31) Scenario 2: Seasons of Silver";
            groupName = null;
        }

        //Scenario 3: Clash of the Curiers
        if (identifierName.Contains("scenario03") || identifierName.Contains("Scenario03"))
        {
            factionName = "(32) Scenario 3: Clash of the Curiers";
            groupName = null;
        }

        //Set all Industrial Zone-pack CLDC (08) in the right menu
        if (identifierName.StartsWith("CDLC08"))
        {
            factionName = "Ornaments";
            groupName = "27 Industrial Zone";
        }

        //The Grand Gallery earning System Ornamentals and other things (GU15 - 20-09-2022)
        if (identifierName.StartsWith("GG_OldNate_"))
        {
            factionName = "Ornaments";
            groupName = "28 Grand Gallery";
        }

        // Place the rest of the buildings in the right Faction > Group menu
        #region Order the Buildings to the right tiers and factions as in the game

        (string Faction, string Group, string Template) groupInfo = NewFactionAndGroup1800.GetNewFactionAndGroup1800(identifierName, factionName, groupName, templateName);
        factionName = groupInfo.Faction;
        groupName = groupInfo.Group;
        templateName = groupInfo.Template;

        // Buildings that belong to the Eden Scenario Menu, but are not placed yet, or wrongly placed 
        if (guidNumber == 24119) { factionName = ""; groupName = "FactoryBuilding7"; } // Place back a building that not belongs to the Workers Tree
        if (guidNumber == 24121) { factionName = ""; groupName = "FactoryBuilding7"; } // Place back a building that not belongs to the Artisans Tree
        if (guidNumber == 24124) { factionName = ""; groupName = "FactoryBuilding7"; } // Place back a building that not belongs to the Artisans Tree
        if (guidNumber == 24110) { factionName = ""; groupName = "FactoryBuilding7"; } // place back a building that not belongs to the Jornaleros Tree 
        if (guidNumber == 24116) { factionName = ""; groupName = "FactoryBuilding7"; } // place back a building that not belongs to the Obreros Tree 
        if (guidNumber == 24055) { factionName = ""; groupName = "FactoryBuilding7"; } // place back a building that not belongs to the Elders Tree 


        if (factionName?.Length == 0 || factionName == "Moderate" || factionName == "Colony01" || factionName == "Arctic" || factionName == "Africa")
        {
            factionName = "Not Placed Yet -" + factionName;
        }
        if (factionName == "Meta;Moderate;Colony01;Arctic;Africa")
        {
            factionName = "Not Placed Yet -All Worlds";
        }

        #endregion

        #region Sorting the Ornaments for the new Ornaments Menu (11/05/2020)

        //Sorting to the new menu
        groupInfo = NewOrnamentsGroup1800.GetNewOrnamentsGroup1800(identifierName, factionName, groupName, templateName);
        factionName = groupInfo.Faction;
        groupName = groupInfo.Group;
        templateName = groupInfo.Template;

        //to keep Palace on his place where it belongs
        if (identifierName == "Palace")
        {
            templateName = "PalaceBuilding"; factionName = "(05) Investors"; groupName = "Palace Buildings";
        }

        //Bring all Docklands Modules in to 1 menu, Template is adjusted for Color Preset
        if (templateName.Contains("Dockland"))
        {
            if (templateName != "DocklandMain")
            {
                factionName = "Harbor";
                groupName = "Docklands Modules";
                templateName = "DocklandsHarbor";
            }
            else
            {
                factionName = "Harbor";
                groupName = null;
                templateName = "DocklandsHarbor";
            }
        }

        //Set right group to the City Lights DLC (just need a Faction and Group change by starting identifiername) (10-01-2021)
        //if (templateName == "OrnamentalBuilding" && factionName == "Not Placed Yet -Moderate") {
        if (templateName == "OrnamentalBuilding")
        {
            if (identifierName.Contains("CityOrnament "))
            {
                factionName = "Ornaments"; groupName = "20 City Lights";
            }
        }

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

        #region Manual on Identifiers DVDatalist inserts, do not add the current building

        if (identifierName == "Africa_tractor_module_02 (Harvester)") { DVDataList[119026] = DVDataList[119026] + DVDataSeperator + Convert.ToString(guidNumber); return; };
        if (identifierName == "Scenario02_tractor_module_02 (Harvester)") { DVDataList[25547] = DVDataList[25547] + DVDataSeperator + Convert.ToString(guidNumber); return; };

        #endregion

        #endregion

        #region Starting buildup the preset data

        BuildingInfo b = new()
        {
            Header = headerName,
            Faction = factionName,
            Group = groupName,
            Template = templateName,
            Identifier = identifierName,
            Guid = guidNumber,
        };

        // Process for Modules by just an empty module per building and for buildings that has multiple levels,
        // in order to get GUID's pointing to the right way for the Tool of DuxVitae
        // Also Skip Manual added GUID's to the DVDataList
        #region Add all Zoo, Museum, Botanical Modules to the DVDataLisy
        // The code will be redone to compacter code after update 5.0 !
        string DVreplaceName = "A7_";
        string DVicon = null;
        if (values["Standard"]?["IconFilename"]?.InnerText != null)
        {
            DVicon = values["Standard"]["IconFilename"].InnerText;
        }

        if (DVicon != null)
        {
            // Split the Value <IconFilenames>innertext</IconFilenames> to get only the Name.png
            string[] sDVIcons = DVicon.Split('/');
            if (sDVIcons.LastOrDefault().StartsWith("icon_"))
            {
                DVicon = sDVIcons.LastOrDefault().Replace("icon_", DVreplaceName);
            }
            else /* Put the Replace name on front*/
            {
                DVicon = DVreplaceName + sDVIcons.LastOrDefault();
            }
            if ((DVicon == "A7_Zoo module.png") && (b.Guid != 100455))
            {
                string DVisExcludedGuidStr = Convert.ToString(b.Guid);
                DVDataList[100455] = DVDataList[100455] + DVDataSeperator + DVisExcludedGuidStr;
                return;
            }
            if ((DVicon == "A7_music_pavillion.png") && (b.Guid != 113452))
            {
                string DVisExcludedGuidStr = Convert.ToString(b.Guid);
                DVDataList[113452] = DVDataList[113452] + DVDataSeperator + DVisExcludedGuidStr;
                return;
            }
        }
        // DVDataList for the Zoo Modules that has not the Default Zoo Icon
        if ((((b.Identifier.StartsWith("Culture_01_module_")) || ((b.Group == "CultureModule") && (b.Faction == "All Worlds") && (DVicon == "A7_general_module_01.png"))) && (b.Guid != 100455)))
        {
            string DVisExcludedGuidStr = Convert.ToString(b.Guid);
            DVDataList[100455] = DVDataList[100455] + DVDataSeperator + DVisExcludedGuidStr;
            return;
        }

        // DVDataList for the Museum Modules on part of IdentifierName
        if ((b.Identifier.StartsWith("Culture_02_module_")) && (b.Guid != 100454))
        {
            string DVisExcludedGuidStr = Convert.ToString(b.Guid);
            DVDataList[100454] = DVDataList[100454] + DVDataSeperator + DVisExcludedGuidStr;
            return;
        }

        // DVDataList for the Botanica Garden Modules on part of IdentifierName
        if ((b.Identifier.StartsWith("C03_")) && (b.Guid != 111104))
        {
            string DVisExcludedGuidStr = Convert.ToString(b.Guid);
            DVDataList[111104] = DVDataList[111104] + DVDataSeperator + DVisExcludedGuidStr;
            return;
        }

        // Skip buildings that are added Manual into the DVDataList on GUID's
        switch (b.Guid)
        {
            //Skipped DVDataList added
            case 24828: { return; }
            case 101267: { return; }
            case 100417: { return; }
            case 113750: { return; }
            case 129025: { return; }
            case 101516: { return; }
            case 102093: { return; }
            case 102112: { return; }
        }
        #endregion

        // print progress
        Console.WriteLine(b.Identifier + " - " + b.Guid);

        #region Set the extra Residences in the right order, selected by b.guid
        // those residences need to be into the Presets for the Converting Tool of DuxVitae
        switch (b.Guid)
        {
            case 1010343: b.Faction = "Residences"; b.Template = "DefColDef"; b.Group = "(1) Old World"; break;
            case 1010344: b.Faction = "Residences"; b.Template = "DefColDef"; b.Group = "(1) Old World"; break;
            case 1010345: b.Faction = "Residences"; b.Template = "DefColDef"; b.Group = "(1) Old World"; break;
            case 1010346: b.Faction = "Residences"; b.Template = "DefColDef"; b.Group = "(1) Old World"; break;
            case 1010347: b.Faction = "Residences"; b.Template = "DefColDef"; b.Group = "(1) Old World"; break;
            case 114445: b.Faction = "Residences"; b.Template = "DefColDef"; b.Group = "(1) Old World"; break;
            case 101254: b.Faction = "Residences"; b.Template = "DefColDef"; b.Group = "(2) New World"; break;
            case 101255: b.Faction = "Residences"; b.Template = "DefColDef"; b.Group = "(2) New World"; break;
            //case <unknown>: b.Faction = "Residences"; b.Template = "DefColDef"; b.Group = "(2) New World"; break;
            case 112091: b.Faction = "Residences"; b.Template = "DefColDef"; b.Group = "(3) Arctic"; break;
            case 112652: b.Faction = "Residences"; b.Template = "DefColDef"; b.Group = "(3) Arctic"; break;
            case 114436: b.Faction = "Residences"; b.Template = "DefColDef"; b.Group = "(4) Enbesa"; break;
            case 114437: b.Faction = "Residences"; b.Template = "DefColDef"; b.Group = "(4) Enbesa"; break;
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
                oldColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("- BuildBlocker not found, skipping: Missing Object File (B)");
                Console.ForegroundColor = oldColor;
                return;
            }
        }
        else
        {
            oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("- BuildBlocker not found, skipping: Object Information not found (A)");
            Console.ForegroundColor = oldColor;
            return;
        }

        #endregion

        #region Set the BlockedArea's and Start Direction
        // Set the BlockedArea's and Start Direction for Coastal buildings that have a Blocked Area
        // I do this by hand, as automatically is not an option for now, as the .ifo file are messy to get the
        //  <QuayArea> blocks from it. Read here for the complete story why 
        //  https://discord.com/channels/571011757317947406/571064812042321927/885817431136817162

        switch (b.Identifier)
        {
            case "Coastal_01 (Fish Coast Building)": { b.BlockedAreaLength = 5; b.Direction = GridDirection.Right; break; }
            case "Coastal_colony01_02 (Fish Coast Building)": { b.BlockedAreaLength = 5; b.Direction = GridDirection.Right; break; }
            case "Coastal_arctic_02 (Seal Hunter)": { b.BlockedAreaLength = 6; b.Direction = GridDirection.Right; break; }
            case "Coastal_colony02_01 (Salt Coast Building)": { b.BlockedAreaLength = 6; b.Direction = GridDirection.Right; break; }
            case "Coastal_colony02_02 (Seafood Fisher)": { b.BlockedAreaLength = 5; b.Direction = GridDirection.Right; break; }
            case "Coastal_02 (Niter Coast Building)": { b.BlockedAreaLength = 7; b.Direction = GridDirection.Right; break; }
            case "Coastal_arctic_01 (Whale Coast Building)": { b.BlockedAreaLength = 13; b.Direction = GridDirection.Right; break; }
            case "Harbor_16 (Commuter Pier)": { b.BlockedAreaLength = 7; b.Direction = GridDirection.Right; break; }
            case "Dockland_Module_Storage": { b.BlockedAreaLength = 3; b.Direction = GridDirection.Right; break; }
            case "Dockland_Module_RepairCrane": { b.BlockedAreaLength = 3; b.Direction = GridDirection.Right; break; }
            case "Dockland_Module_SpeedUp": { b.BlockedAreaLength = 3; b.Direction = GridDirection.Right; break; }
            case "Harbor_02 (Sailing Shipyard)": { b.BlockedAreaLength = 25; b.Direction = GridDirection.Right; break; }
            case "Harbor_03 (Steam Shipyard)": { b.BlockedAreaLength = 25; b.Direction = GridDirection.Right; break; }
            case "Harbor_08 (Pier)": { b.BlockedAreaLength = 25; b.Direction = GridDirection.Right; break; }
            case "Harbor_09 (tourism_pier_01)": { b.BlockedAreaLength = 25; b.Direction = GridDirection.Right; break; }
            case "Kontor_imperial_01": { b.BlockedAreaLength = 25; b.BlockedAreaWidth = 4.5; b.Direction = GridDirection.Right; break; }
            case "Harbor_14a (Oil Harbor I)": { b.BlockedAreaLength = 25; b.Direction = GridDirection.Right; break; }
            case "Dockland - Main": { b.BlockedAreaLength = 25; b.Direction = GridDirection.Right; break; }
            case "Dockland_Module_Pier": { b.BlockedAreaLength = 25; b.Direction = GridDirection.Right; break; }
            case "Coastal_03 (Quartz Sand Coast Building)": { b.BlockedAreaLength = 6; b.Direction = GridDirection.Right; break; }
            case "Coastal_02 (Water Purifier)": { b.BlockedAreaLength = 6; b.Direction = GridDirection.Right; break; }
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
            // Split the Value <IconFilenames>innertext</IconFilenames> to get only the Name.png
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
                case "102139": { icon = replaceName + "park_props_1x1_27.png"; break; } //Path Correcting Icon
                case "102140": { icon = replaceName + "park_props_1x1_28.png"; break; } //Path Correcting Icon
                case "102141": { icon = replaceName + "park_props_1x1_29.png"; break; } //Path Correcting Icon
                case "102142": { icon = replaceName + "park_props_1x1_30.png"; break; } //Path Correcting Icon
                case "102143": { icon = replaceName + "park_props_1x1_31.png"; break; } //Path Correcting Icon 
                case "102131": { icon = replaceName + "park_props_1x1_17.png"; break; } //Cypress correcting Icon
                case "101284": { icon = replaceName + "community_lodge.png"; break; } //correcting Arctic Lodge Icon
            }
            switch (b.Identifier)
            {
                case "AmusementPark CottonCandy": { icon = replaceName + "cotton_candy.png"; break; } // faulty naming fix icn_ instead of icon_
                case "Coastal_colony02_01 (Salt Coast Building)": icon = replaceName + "salt_africa.png"; break;
                case "Random slot mining": { icon = replaceName + "mineral_desposits.png"; break; }
                case "Random slot oil pump": { icon = replaceName + "oil.png"; break; }
                case "Season4 random mining slot colony01": { icon = replaceName + "mineral_desposits.png"; break; }
            }
            switch (b.Guid)
            {
                case 4258: { icon = replaceName + "airship_landing_plattform.png"; break; }
            }

            //Place all (24) Seasonal Decorations Pack CDLC in the right menu (on IconFileName)
            // spring_ / summer_ / winter_ / autumn_
            if ((icon.StartsWith("A7_spring_")) || (icon.StartsWith("A7_summer_")) || (icon.StartsWith("A7_autumn_")) || (icon.StartsWith("A7_winter_")))
            {
                b.Faction = "Ornaments"; b.Group = "26 Seasonal Decorations";
            }

            b.IconFileName = icon;
        }
        else
        {
            b.IconFileName = null;

            //Buildings that came in with the BaseAssetGUID template has no icons or just have no icon, this will fix/set that;
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
                case "Institution_colony01_02 (Fire Department)": b.IconFileName = replaceName + "fire_house.png"; break; //set NW Fire Station Icon
                case "Institution_colony01_03 (Hospital)": b.IconFileName = replaceName + "hospital.png"; break;  //set NW Hospital Icon
                case "Agriculture_colony01_11_field (Alpaca Pasture)": { b.IconFileName = replaceName + "general_module_01.png"; break; }
                case "Agriculture_colony01_09_field (Cattle Pasture)": { b.IconFileName = replaceName + "general_module_01.png"; break; }
                case "Residence_Africa_World": { b.IconFileName = replaceName + "resident.png"; break; } //set Shepherd Residence to default resident.png (has none)
                case "Harbor_arctic_01 (Depot)": { b.IconFileName = replaceName + "depot.png"; break; }
                case "Institution_colony02_02 (Police)": { b.IconFileName = replaceName + "police.png"; break; } // Fix non icon Africa Police Station
                case "Institution_colony02_03 (Hospital)": { b.IconFileName = replaceName + "hospital.png"; break; } // fix non icon for Africa Hospital 
                case "Factory_colony01_05 (Brick Factory)": { b.IconFileName = replaceName + "bricks.png"; break; }
                case "Agriculture_colony01_12_field (Palm Tree Field)": { b.IconFileName = replaceName + "coconut_palm_trees.png"; break; }
            }
            //Some icons need to be set/fixed by TemplateNames
            switch (b.Template)
            {
                case "WorkAreaSlot": { b.IconFileName = replaceName + "mineral_desposits.png"; break; };
            }

            //Some icons need to be set/fixed by building GUID's
            switch (b.Guid)
            {
                case 101290: { b.IconFileName = replaceName + "kontor_main.png"; break; }
                case 112659: { b.IconFileName = replaceName + "kontor_main.png"; break; }
                case 112865: { b.IconFileName = replaceName + "kontor_main.png"; break; }
                case 114626: { b.IconFileName = replaceName + "kontor_main.png"; break; }
                case 114629: { b.IconFileName = replaceName + "kontor_main.png"; break; }
                case 24134: { b.IconFileName = replaceName + "fish_ggj_1.png"; break; }
                case 24658: { b.IconFileName = replaceName + "pigs.png"; break; }
                case 24136: { b.IconFileName = replaceName + "aqua_well.png"; break; }
                case 4260: { b.IconFileName = replaceName + "post_office.png"; break; }
            }
        }

        #region Check and Change Icons is need or remove objects for the DVDataList
        // Add some iconfilenames to objects that not have any yet on start and not placed yet,
        // that need to be filtered for GUID purpose
        if (b.Faction.StartsWith("Not Placed Yet -"))
        {
            if (b.Identifier.Contains("(Warehouse ")) { b.IconFileName = replaceName + "warehouse.png"; }
            if (b.Identifier.Contains("(Depot)") || b.Group.StartsWith("1010519")) { b.IconFileName = replaceName + "depot.png"; }
            if (b.Group == "100783" || b.Group == "101403") { b.IconFileName = replaceName + "oil_habour_01.png"; }
            if (b.Group == "100519") { b.IconFileName = "A7_pier.png"; }
            if (b.Group == "100429") { b.IconFileName = "A7_visitor_harbour.png"; }
            if (b.Identifier.StartsWith("Kontor_airship_arctic_")) { b.IconFileName = replaceName + "airship_landing_plattform.png"; }
            if (b.Identifier.StartsWith("Kontor_imperial_") || b.Identifier.StartsWith("Kontor_main_")) { b.IconFileName = replaceName + "kontor_main.png"; }
            if (b.Group == "101404" || b.Group == "119259") { b.IconFileName = replaceName + "oil_habour_01.png"; }
            if (b.Group == "1010311") { b.IconFileName = replaceName + "gold_ore.png"; }
            if (b.Group == "100415") { b.IconFileName = replaceName + "townhall.png"; }
            if (b.Group == "100586") { b.IconFileName = replaceName + "harbour_kontor.png"; }
            if (b.Group == "100784") { b.IconFileName = replaceName + "oil_storage.png"; }
            if (b.Group == "1010525") { b.IconFileName = replaceName + "repair_crane.png"; }
            if (b.Group == "1010516") { b.IconFileName = replaceName + "guildhouse.png"; }
            if ((b.Group == "Slot" && b.Faction == "Not Placed Yet -Moderate" && b.Identifier != "Heavy_01_01_slot (Oil Pump Slot)")) { b.IconFileName = replaceName + "mineral_desposits.png"; }
            if (b.Identifier == "Random slot mining arctic") { b.IconFileName = replaceName + "oil.png"; }
            if (b.Group == "1010522") { b.IconFileName = replaceName + "defense_tower_pucklegun.png"; }
            if (b.Group == "1010523") { b.IconFileName = replaceName + "defense_tower_cannon.png"; }
            if (b.Group == "1010520") { b.IconFileName = replaceName + "sail_shipyard.png"; }
        }
        if (b.Guid != 681 && ((guidNumber > 679 && guidNumber < 687) || (guidNumber == 949)))
        {
            //Place Icons and factionName on the Dam Buildings (Scenario 1), so it would be filtered and added to the DVDataList 
            factionName = "(30) Scenario 1: Eden Burning";
            b.IconFileName = replaceName + "dam_a.png";
        }

        // Put Double DLC11 Ornamentals into DVDatalist Manualy, to avoid Double names there
        bool DoDLC11_OrnamentRemove = false;
        oldColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        if (!string.IsNullOrEmpty(b.IconFileName))
        {
            if (b.IconFileName.Contains("_nw.png") && b.Faction == "(20) Empire of the Skies")
            {
                switch (guidNumber)
                {
                    case 3356: { DVDataList[3327] = "3327,A7_airport_cafe_ow.png,DLC11 Cafe Moderate,3356"; Console.WriteLine("---> Building added to Replacement List (DLC11 Ornament): 3327 << 3356"); DoDLC11_OrnamentRemove = true; break; }
                    case 3357: { DVDataList[3330] = "3330,A7_airport_cafetables_ow.png,DLC11 Cafe Tables Moderate,3357"; Console.WriteLine("---> Building added to Replacement List (DLC11 Ornament): 3330 << 3357"); DoDLC11_OrnamentRemove = true; break; }
                    case 3358: { DVDataList[3331] = "3331,A7_airport_clock_ow.png,DLC11 Clock Moderate,3358"; Console.WriteLine("---> Building added to Replacement List (DLC11 Ornament): 3331 << 3358"); DoDLC11_OrnamentRemove = true; break; }
                    case 3360: { DVDataList[3337] = "3337,A7_airport_flag_02_ow.png,DLC11 Flagpole Moderate,3360"; Console.WriteLine("---> Building added to Replacement List (DLC11 Ornament): 3337 << 3360 "); DoDLC11_OrnamentRemove = true; break; }
                    case 3361: { DVDataList[3338] = "3338,A7_airport_seats_ow.png,DLC11 Benches Small Moderate,3361"; Console.WriteLine("---> Building added to Replacement List (DLC11 Ornament): 3338 << 3361"); DoDLC11_OrnamentRemove = true; break; }
                    case 3362: { DVDataList[3339] = "3339,A7_airport_seats_large_ow.png,DLC11 Benches Large Moderate,3362"; Console.WriteLine("---> Building added to Replacement List (DLC11 Ornament): 3339 << 3362"); DoDLC11_OrnamentRemove = true; break; }
                    case 3363: { DVDataList[3340] = "3340,A7_airport_sign_ow.png,DLC11 Sign Moderate,3363"; Console.WriteLine("---> Building added to Replacement List (DLC11 Ornament): 3340 << 3363"); DoDLC11_OrnamentRemove = true; break; }
                    case 3368: { DVDataList[3341] = "3341,A7_airport_arrivals_ow.png,DLC11 Gate Moderate,3368"; Console.WriteLine("---> Building added to Replacement List (DLC11 Ornament): 3341 << 3368"); DoDLC11_OrnamentRemove = true; break; }
                }
            }
        }
        Console.ForegroundColor = oldColor;
        if (DoDLC11_OrnamentRemove == true) { return; }


        string isExcludedGuidStr = Convert.ToString(b.Guid);
        //Those Identifier changes will not harm any users, as they where never in the presets before....
        //This will be done directly to the building info structure (b)
        //Also switch wrong icons between two objects (see issue #178)
        switch (b.IconFileName)
        {
            case "A7_park_props_1x1_14.png": b.Identifier = "Park_1x1_statue_grass"; b.Faction = "Ornaments"; b.Group = "05 Park Statues"; break;
            case "A7_city_2x2_03.png": b.IconFileName = replaceName + "city_2x2_02.png"; break; // Switch to right icon
            case "A7_city_2x2_02.png": b.IconFileName = replaceName + "city_2x2_03.png"; break; // Switch to right icon
        }

        isExcludedName = identifierName.IsPartOf(annoBuildingLists);
        isExcludeIconName = b.IconFileName.IsPartOf(anno1800IconNameLists);

        if (isExcludedName || isExcludeIconName)
        {
            // The following code is remade in order to get all GUID's in the Datafile
            // The following is for DuxVitae Convert tool, so he knows what GUID's are excluded, and what GUID he
            // must use to get the right AD Object into his converted ad file.
            // This will also prevent Double Identifier Objects
            oldColor = Console.ForegroundColor;
            foreach (string DVData in DVDataList)
            {
                if (!String.IsNullOrEmpty(DVData))
                {
                    string[] DVDataCheck = DVData.Split(',');
                    if (DVDataCheck.Length > 2)
                    {
                        // Check first the Double Identifiers on this check, so double Buildings Identifiers
                        // will be pointed to the first added building in the list.
                        if ((b.Identifier == DVDataCheck[2]) && !string.IsNullOrEmpty(DVDataCheck[2]))
                        {
                            if (b.Identifier.IsMatch(buildings))
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                int DVDataGUID = Convert.ToInt32(DVDataCheck[0]);
                                DVDataList[DVDataGUID] = DVDataList[DVDataGUID] + DVDataSeperator + isExcludedGuidStr;
                                Console.WriteLine("---> Building added to Replacement List (Ident): " + DVDataGUID + " << " + isExcludedGuidStr);
                                Console.ForegroundColor = oldColor; return;
                            }
                        }
                        if (b.IconFileName == DVDataCheck[1] && !string.IsNullOrEmpty(DVDataCheck[1]) && isExcludedName && isExcludeIconName)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                            int DVDataGUID = Convert.ToInt32(DVDataCheck[0]);
                            DVDataList[DVDataGUID] = DVDataList[DVDataGUID] + DVDataSeperator + isExcludedGuidStr;
                            Console.WriteLine("---> Building added to Replacement List (Icon): " + DVDataGUID + " << " + isExcludedGuidStr);
                            Console.ForegroundColor = oldColor; return;
                        }
                    }
                }
            }
        }

        #endregion

        #endregion

        #endregion

        #region Get Influence Radius of Buildings

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

        //Tourist DLC Restaurants, Bars and Cafe's
        if (string.IsNullOrEmpty(Convert.ToString(b.InfluenceRadius)) || b.InfluenceRadius == 0)
        {
            b.InfluenceRadius = Convert.ToInt32(values?["BuffFactory"]?["ProductionBuffDistance"]?.InnerText);
        }

        switch (b.Identifier)
        {
            case "Agriculture_colony01_06 (Timber Yard)": b.InfluenceRadius = 9; break;
            case "Heavy_colony01_01 (Oil Heavy Industry)": b.InfluenceRadius = 12; break;
            case "Town hall": b.InfluenceRadius = 20; break;
            case "Guild_house_arctic": b.InfluenceRadius = 15; break;
            case "Mining_arctic_01 (Gas Mine)": b.InfluenceRadius = 10; break;
            case "DepartmentStore_Blank": b.InfluenceRadius = 45; break;
            case "Pharmacy_Blank": b.InfluenceRadius = 45; break;
            case "FurnitureStore_Blank": b.InfluenceRadius = 45; break;
            case "Harbor_07 (Repair Crane)": b.InfluenceRadius = 20; break;
            case "Dockland_Module_RepairCrane": b.InfluenceRadius = 20; break;
            case "Harbor_office": b.InfluenceRadius = 20; break;
            case "Tourist_monument_00": b.InfluenceRadius = 107; break;
        }

        #endregion

        #region Get/Set InfluenceRange information

        b.InfluenceRange = 0;

        if (b.Template == "CityInstitutionBuilding")
        {
            b.InfluenceRange = 26; //Police - Fire stations and Hospitals
            if (b.Identifier == "Institution_arctic_01 (Ranger Station)")
            {
                b.InfluenceRange = 50; //fix Ranger Station InfluencRange as this is separated from normal ones (10-01-2021) 
            }
        }
        else if (!string.IsNullOrEmpty(values?["PublicService"]?["FullSatisfactionDistance"]?.InnerText))
        {
            b.InfluenceRange = Convert.ToInt32(values["PublicService"]["FullSatisfactionDistance"].InnerText);
        }
        else if ((!string.IsNullOrEmpty(values?["BuffFactory"]?["PublicServiceData"]?["FullSatisfactionDistance"]?.InnerText)))
        {
            // Fix #407, missing InfluenceRange on Stores/Malls (and maybe some other buildings)
            b.InfluenceRange = Convert.ToInt32(values["BuffFactory"]["PublicServiceData"]["FullSatisfactionDistance"].InnerText);
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
        //fix #407, missing InfluenceRange on Stores/Malls_Blank ones (and maybe some other buildings)
        if (templateName == "Mall" && groupName == "Mall" && identifierName.Contains("_Blank"))
        {
            b.InfluenceRange = 44;
        }
        if (b.Guid == 24136)
        {
            b.InfluenceRange = 18;
        }

        // Get/Set Influence Radius and Influence Range (Dual on 1 building : Busstop)
        // Bussttop (has an other range name)
        if (b.Template == "Busstop")
        {
            b.InfluenceRadius = Convert.ToInt32(values?["BusStop"]?["ActivationRadius"]?.InnerText);
            b.InfluenceRange = Convert.ToInt32(values["BusStop"]["StreetConnectionRange"].InnerText);
        }

        #endregion

        #region Get localizations
        string buildingGuid = null;
        if (guidNumber != 0)
        {
            buildingGuid = Convert.ToString(guidNumber);
        }
        // If there is a TAG TextOverride GUID in the Assets file.
        // If there is one, then read that TAG GUID for translations
        if (!string.IsNullOrEmpty(values?["Text"]?["TextOverride"]?.InnerText))
        {
            buildingGuid = values["Text"]["TextOverride"].InnerText;
        }

        //Manual Override of translations, like on renewed/replaced objects
        if (b.Guid == 24134)
        {
            buildingGuid = "972";
        }
        if (b.Guid == 24136)
        {
            buildingGuid = "993";
        }
        if (b.Guid == 112726)
        {
            buildingGuid = "4258";
        }

        //Initialize the dictionary
        string langNodeStartPath = "/TextExport/Texts/Text";
        string langNodeDepth = "Text";
        int languageCount = 0;
        b.Localization = new SerializableDictionary<string>();

        foreach (string Language in Languages)
        {
            // Changed because of performance issues (06-06-2022)
            XmlDocument langDocument = new();
            switch (languageCount)
            {
                case 0: { langDocument = langDocument_english; break; }
                case 1: { langDocument = langDocument_german; break; }
                case 2: { langDocument = langDocument_french; break; }
                case 3: { langDocument = langDocument_polish; break; }
                case 4: { langDocument = langDocument_russian; break; }
                case 5: { langDocument = langDocument_spanish; break; }
            }

            string translation = "";
            if (!string.IsNullOrEmpty(buildingGuid))
            {
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
                        //Split the translation text ( and ) marking the GUID, and use the second value in nextGUID[].
                        string[] nextGuid = translation.Split('(', ')');
                        translationNodes = langDocument.SelectNodes(langNodeStartPath)
                            .Cast<XmlNode>().SingleOrDefault(_ => _["GUID"].InnerText == nextGuid[1]);
                        translation = translationNodes?.SelectNodes(langNodeDepth)?.Item(0).InnerText;
                    }

                    //re translated the following buildings:
                    if (buildingGuid == "102165")
                    {
                        switch (languageCount)
                        {
                            case 0: { translation = "Sidewalk Hedge"; break; }
                            case 1: { translation = "Gehweg Hecke"; break; }
                            case 2: { translation = "Haies de trottoirs"; break; }
                            case 3: { translation = "Żywopłot Chodnikowy"; break; }
                            case 4: { translation = "Боковая изгородь"; break; }
                            case 5: { translation = "Seto de la acera"; break; }
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
                            case 5: { translation = "Esquina del seto de la acera"; break; }
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
                            case 5: { translation = "Acera Final de seto"; break; }
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
                            case 5: { translation = "Acera Seto Empalme"; break; }
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
                            case 5: { translation = "Cruce de setos en la acera"; break; }
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
                            case 5: { translation = "Barandillas"; break; }
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
                            case 5: { translation = "Empalme de barandillas"; break; }
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
                            case 5: { translation = "Cobertura"; break; }
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
                            case 5: { translation = "Ruta"; break; }
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
                            case 5: { translation = "Instituto de Investigación"; break; }
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
                            case 5: { translation = "Depósito Ártico"; break; }
                        }
                    }
                    else if (buildingGuid == "1000029")
                    {
                        switch (languageCount)
                        {
                            case 0: { translation = "Empty Mining Slot"; break; }
                            case 1: { translation = "Leerer Bergbau-Slot"; break; }
                            case 2: { translation = "Emplacement minier vide"; break; }
                            case 3: { translation = "Pusta szczelina wydobywcza"; break; }
                            case 4: { translation = "Пустой горный отсек"; break; }
                            case 5: { translation = "Ranura minera vacía"; break; }
                        }
                    }
                    else if (buildingGuid == "100849")
                    {
                        switch (languageCount)
                        {
                            case 0: { translation = "Oil Spring"; break; }
                            case 1: { translation = "Ölquelle"; break; }
                            case 2: { translation = "Puits de pétrole"; break; }
                            case 3: { translation = "Pole naftowe"; break; }
                            case 4: { translation = "Нефтяной источник"; break; }
                            case 5: { translation = "Fuente de petróleo"; break; }
                        }
                    }
                    else if (buildingGuid == "972") //fix translations of the Aquafarm that uses 5 fields
                    {
                        switch (languageCount)
                        {
                            case 0: { translation = translation += " - (5)"; break; }
                            case 1: { translation = translation += " - (5)"; break; }
                            case 2: { translation = translation += " - (5)"; break; }
                            case 3: { translation = translation += " - (5)"; break; }
                            case 4: { translation = translation += " - (5)"; break; }
                            case 5: { translation = translation += " - (5)"; break; }
                        }
                    }
                    #region Give all residences a Tier Number
                    //Tier numbers 1 (all regions)
                    if (b.Guid == 1010343 || b.Guid == 101254 || b.Guid == 112091 || b.Guid == 114436)
                    {
                        translation = "(1) " + translation;
                    }
                    //Tier numbers 2 (all regions)
                    if (b.Guid == 1010344 || b.Guid == 101255 || b.Guid == 112652 || b.Guid == 114437)
                    {
                        translation = "(2) " + translation;
                    }
                    //Tier numbers 3 (Temperate only)
                    if (b.Guid == 1010345)
                    {
                        translation = "(3) " + translation;
                    }
                    //Tier numbers 4 (Temperate only)
                    if (b.Guid == 1010346)
                    {
                        translation = "(4) " + translation;
                    }
                    //Tier numbers 5 (Temperate only)
                    if (b.Guid == 1010347)
                    {
                        translation = "(5) " + translation;
                    }
                    if (b.Guid == 114445)
                    {
                        translation = "(6) " + translation;
                    }
                    #endregion
                }
                else
                {
                    if (languageCount < 1)
                    {
                        oldColor = Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.DarkCyan;
                        Console.WriteLine("---> No Translation found, it will set to Identifier.");
                        Console.ForegroundColor = oldColor;
                    }
                    translation = values["Standard"]["Name"].InnerText;
                }
                // Remove Internal Colony names, and Caps the first letter.
                if (translation.StartsWith("river_colony02_"))
                {
                    translation = translation.Remove(6, 12);
                    translation = translation.FirstCharToUpper();
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
                        bool isFieldInfoFound = false;
                        foreach (FarmField curFieldInfo in farmFieldList1800)
                        {
                            if (string.Equals(curFieldInfo.FieldGuid, fieldGuidValue, StringComparison.OrdinalIgnoreCase))
                            {
                                isFieldInfoFound = true;
                                fieldAmountValue = curFieldInfo.FieldAmount;
                                if (Convert.ToInt32(fieldAmountValue) <= 0)
                                {
                                    // ERROR ? Farm without field amount found
                                    oldColor = Console.ForegroundColor;
                                    Console.ForegroundColor = ConsoleColor.DarkRed;
                                    Console.WriteLine("-- > Farm field Skipped, Zero Field counter");
                                    Console.ForegroundColor = oldColor;
                                    return;
                                }
                                break;
                            }
                        }

                        if (!isFieldInfoFound)
                        {
                            farmFieldList1800.Add(new FarmField(fieldGuidValue, fieldAmountValue));
                        }

                        translation = translation + " - (" + fieldAmountValue + ")";
                    }
                }
            }
            else
            {
                translation = values["Standard"]["Name"].InnerText;
            }
            b.Localization.Dict.Add(Languages[languageCount], translation);
            languageCount++;
        }
        if (string.IsNullOrEmpty(buildingGuid))
        {
            oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("---> No GUID Number found, it is set to the Identifier.");
            Console.ForegroundColor = oldColor;
        }

        #endregion

        #region Rename some duplicate identifiers to avoid double identifiers on the hand of Icon Files

        switch (b.IconFileName)
        {
            case "A7_col_park_props_system_1x1_24_back.png": b.Identifier = "Park_1x1_bush_02"; break;
            case "A7_park_props_1x1_26.png": b.Identifier = "Park_1x1_bush_03"; break;
            case "A7_col_park_props_system_2x2_03_back.png": b.Identifier = "Park_2x2_garden_02"; break;
            case "A7_col_park_props_system_3x3_02_front.png": b.Identifier = "Park_3x3_fountain_02"; break;
            case "A7_col_park_props_system_3x3_03_back.png": b.Identifier = "Park_3x3_gazebo_02"; break;
        }

        #endregion

        #region Add all other (Upgrading) buildings to the DVDataLisy
        // Add CSV GUID Data to List for DuxVitae Convert/extract tool, Games save --> Layout AD File (01-06-2022)
        // Skip Zoo, Museum, Botanical Garden, Music Pavilion empty modules and other buildings, as those are added on start
        bool DVDatacounted2 = false;
        int DVDataGUID2 = 0;
        if (b.IconFileName != null)
        {
            switch (b.IconFileName)
            {
                //pre-arrange ones (line 651 >) made by hand, and first GUID is skipped on making the DVDataList 
                case "A7_airship_hangar.png": if (b.Guid != 112685) { DVDataGUID2 = 112685; DVDatacounted2 = true; } break;
                case "A7_research_center.png": if (b.Guid != 118938) { DVDataGUID2 = 118938; DVDatacounted2 = true; } break;
                case "A7_warehouse.png": if (b.Guid != 1010371) { DVDataGUID2 = 1010371; DVDatacounted2 = true; } break;
                case "A7_oil_habour_01.png": if (b.Guid != 100783) { DVDataGUID2 = 100783; DVDatacounted2 = true; } break;
                case "A7_kontor_main.png": if (b.Guid != 1010540) { DVDataGUID2 = 1010540; DVDatacounted2 = true; } break;
                case "A7_visitor_harbour.png": if (b.Guid != 100429) { DVDataGUID2 = 100429; DVDatacounted2 = true; } break;
                case "A7_dam_a.png": if (b.Guid != 686) { DVDataGUID2 = 686; DVDatacounted2 = true; } break;
                //in order of the Presets, as they are read in when the presets are created, and thus added on the first GUID at the DVDataList  
                case "A7_highlife_skyliner_monument.png": if (b.Guid != 403) { DVDataGUID2 = 403; DVDatacounted2 = true; } break;
                case "A7_depot.png": if (b.Guid != 1010519) { DVDataGUID2 = 1010519; DVDatacounted2 = true; } break;
                case "A7_pier.png": if (b.Guid != 100519) { DVDataGUID2 = 100519; DVDatacounted2 = true; } break;
                case "A7_world_fair_2.png": if (b.Guid != 1010489) { DVDataGUID2 = 1010489; DVDatacounted2 = true; } break;
                case "A7_botanic_garden.png": if (b.Guid != 110935) { DVDataGUID2 = 110935; DVDatacounted2 = true; } break;
                case "A7_museum.png": if (b.Guid != 1010471) { DVDataGUID2 = 1010471; DVDatacounted2 = true; } break;
                case "A7_zoo.png": if (b.Guid != 1010470) { DVDataGUID2 = 1010470; DVDatacounted2 = true; } break;
                case "A7_airship_landing_plattform.png": if (b.Guid != 112726) { DVDataGUID2 = 112726; DVDatacounted2 = true; } break;
                case "A7_gold_ore.png": if (b.Guid != 1010311) { DVDataGUID2 = 1010311; DVDatacounted2 = true; } break;
                case "A7_townhall.png": if (b.Guid != 100415) { DVDataGUID2 = 100415; DVDatacounted2 = true; } break;
                case "A7_harbour_kontor.png": if (b.Guid != 100586) { DVDataGUID2 = 100586; DVDatacounted2 = true; } break;
                case "A7_oil_storage.png": if (b.Guid != 100784) { DVDataGUID2 = 100784; DVDatacounted2 = true; } break;
                case "A7_repair_crane.png": if (b.Guid != 1010525) { DVDataGUID2 = 1010525; DVDatacounted2 = true; } break;
                case "A7_guildhouse.png": if (b.Guid != 1010516) { DVDataGUID2 = 1010516; DVDatacounted2 = true; } break;
                case "A7_mineral_desposits.png": if (b.Guid != 1000029) { DVDataGUID2 = 1000029; DVDatacounted2 = true; } break;
                case "A7_oil.png": if (b.Guid != 100849 && b.Guid != 101331 && b.Guid != 1010561) { DVDataGUID2 = 100849; DVDatacounted2 = true; } else if (b.Guid != 101331 && b.Guid != 100849 && b.Guid != 101062 && b.Guid != 116037) { DVDataGUID2 = 101331; DVDatacounted2 = true; } break;
                case "A7_defense_tower_pucklegun.png": if (b.Guid != 1010522) { DVDataGUID2 = 1010522; DVDatacounted2 = true; } break;
                case "A7_defense_tower_cannon.png": if (b.Guid != 1010523) { DVDataGUID2 = 1010523; DVDatacounted2 = true; } break;
                case "A7_sail_shipyard.png": if (b.Guid != 1010520) { DVDataGUID2 = 1010520; DVDatacounted2 = true; } break;
                case "A7_airship_hangar_southamerica.png": if (b.Guid != 648) { DVDataGUID2 = 648; DVDatacounted2 = true; } break;
            }
        }

        // DVDatalilst for other buildings on (part of) Identifier/faction names:
        if ((b.Identifier.StartsWith("Tourist_monument_0") && (b.Guid != 132765))) { DVDataGUID2 = 132765; DVDatacounted2 = true; }
        if ((b.Faction == "Not Placed Yet -Moderate" && b.Identifier == "Forester" && (b.IconFileName == "A7_wood_log.png") && (b.Guid != 1010266))) { DVDataGUID2 = 1010266; DVDatacounted2 = true; }
        if ((b.Faction.StartsWith("Not Placed Yet -") && (b.IconFileName == "A7_tractor.png") && (b.Guid != 269837))) { DVDataGUID2 = 269837; DVDatacounted2 = true; }
        if ((b.Identifier.Contains("(Clay Harvester)") && b.Guid != 117743)) { DVDataGUID2 = 117743; DVDatacounted2 = true; }
        if ((b.Identifier.Contains("(Paper Mill)") && b.Guid != 117744)) { DVDataGUID2 = 117744; DVDatacounted2 = true; }
        if ((b.Identifier.Contains("(Water Pump)") && b.Guid != 114544)) { DVDataGUID2 = 114544; DVDatacounted2 = true; }

        // If one of the object above is placed in the datafile as replaced building,
        // send a message on console and skip the building; 
        if (DVDatacounted2 == true)
        {
            ConsoleColor oldColor2 = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("---> Building added to Replacement List: " + DVDataGUID2 + " << " + isExcludedGuidStr + " || " + b.IconFileName);
            string DVisExcludedGuidStr = Convert.ToString(b.Guid);
            DVDataList[DVDataGUID2] = DVDataList[DVDataGUID2] + DVDataSeperator + DVisExcludedGuidStr;
            Console.ForegroundColor = oldColor2; return;
        }

        if ((b.Guid != 100455) && (b.Guid != 100454) && (b.Guid != 111104) && (b.Guid != 113452) &&
            (b.Guid != 112685) && (b.Guid != 132765) && (b.Guid != 118938) && (b.Guid != 1010371) &&
            (b.Guid != 100783) && (b.Guid != 1010540) && (b.Guid != 100429) && (b.Guid != 686) &&
            (b.Guid != 4260) && (b.Guid != 4258) && (b.Guid != 2654) && (b.Guid != 1372) && (b.Guid != 1375))
        {
            if (string.IsNullOrEmpty(DVDataList[b.Guid]))
            {
                string DVIdent = b.Identifier;
                if (DVIdent.Contains(',')) { DVIdent = DVIdent.Replace(",", ""); }
                DVDataList[b.Guid] = Convert.ToString(b.Guid) + DVDataSeperator + b.IconFileName + DVDataSeperator + DVIdent;
            }
            else //give Error MSG if GUID was already in list and hold
            {
                ConsoleColor oldColor2 = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("---> ERROR GUID WAS ALREADY IN DATALIST: GUID " + Convert.ToString(b.Guid));
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Enter any key to continue");
                Console.ForegroundColor = oldColor2;
                Console.ReadLine();
            }
        }
        #endregion

        // Remove CultureModules Menu and the OrnamentalBuilding Menu that Appeared
        if ((b.Header == "(A7) Anno 1800" && b.Faction == "All Worlds") && (b.Group == "CultureModule" || b.Group == "OrnamentalBuilding")) { return; }

        // Remove the Not Placed Buildings
        // comment out the line below if you make a new preset after update of the game 'ANNO 1800', or when a new 'ANNO 1800 DLC' is released 
        if (b.Faction == "Not Placed Yet -Moderate" || b.Faction == "Not Placed Yet -Arctic" || b.Faction == "Not Placed Yet -Africa" || b.Faction == "Not Placed Yet -Colony01" || b.Faction == "Not Placed Yet -All Worlds") { return; }
        if (b.Faction == "Not Placed Yet -") { return; };

        //Validate iconfilename, if not exists it will be written into a file of missing icons
        ValidateIconFile(b.IconFileName, Convert.ToString(b.Guid), b.Header);
        // add building to the list
        annoBuildingsListCount++;//countup amount of buildings
        annoBuildingLists.Add(values["Standard"]["Name"].InnerText);//add building name to the list, for checking double building names usage
        anno1800IconNameLists.Add(b.IconFileName);//add Icon file to the list, for checking double icon file usage 
        buildings.Add(b); // add building data to file data
    }

    #endregion

}
