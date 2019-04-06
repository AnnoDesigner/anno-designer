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
        
        public static bool isExcludedName = false;
        public static bool isExcludedTemplate = false;
        /// <summary>
        /// Holds the paths and xpaths to parse the extracted RDA's for different Anno versions
        /// 
        /// The RDA's should all be extracted into the same directory.
        /// </summary>
        private static Dictionary<string, Dictionary<string, PathRef[]>> VersionSpecificPaths { get; set; }
        private const string ANNO_VERSION_1404 = "1404";
        private const string ANNO_VERSION_2070 = "2070";
        private const string ANNO_VERSION_2205 = "2205";
        private const string ANNO_VERSION_1800 = "1800";

        private const string BUILDING_PRESETS_VERSION = "0.8.2";
        // Initalisizing Language Directory's and Filenames
        private static readonly string[] Languages = new[] { "cze", "eng", "esp", "fra", "ger", "ita", "pol", "rus" };
        private static readonly string[] LanguagesFiles = new[] { "english", "english", "spanish", "french", "german", "english", "polish", "russian" };
        // Initalisizing Exclude <Name>"text"</Name> and <Template>"text"</Template> for presets.json file 
        // Anno 2205
        private static readonly List<string> ExcludeNameList2205 = new List<string> { "Placeholder", "tier02", "tier03", "tier04", "tier05", "voting"};
        private static readonly List<string> ExcludeTemplateList2205 = new List<string> { "SpacePort", "BridgeWithUpgrade" };

            // Set Icon File Name seperations
        private static string GetIconFilename(XmlNode iconNode)
        {
            return string.Format("icon_{0}_{1}.png", iconNode["IconFileID"].InnerText, iconNode["IconIndex"] != null ? iconNode["IconIndex"].InnerText : "0"); //TODO: check this icon format is consistent between Anno versions
        }

        static Program()
        {
            VersionSpecificPaths = new Dictionary<string, Dictionary<string, PathRef[]>>();
        }
        #endregion

        public static void Main(string[] args)
        {
            #region User Choices 
            bool validPath = false;
            var a = "Testing tracking branch";
            string path = "";
            while (!validPath)
            {
                Console.Write("Please enter the path to the extracted RDA files:");
                path = Console.ReadLine();
                if (path == "quit")
                {
                    Environment.Exit(0);
                }
                if (Directory.Exists(path))
                {
                    validPath = true;
                    ///Add a trailing backslash if one is not present.
                    BASE_PATH = path.LastOrDefault() == '\\' ? path : path + "\\";
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine("Invalid input, please try again or enter 'quit' to exit.");
                }
            }
            string annoVersion = "";
            bool validVersion = false;
            while (!validVersion)
            {
                Console.WriteLine();
                Console.Write("Please enter an Anno version (1 of: {0} {1} {2}):", ANNO_VERSION_1404, ANNO_VERSION_2070, ANNO_VERSION_2205);
                annoVersion = Console.ReadLine();
                if (annoVersion == "quit")
                {
                    Environment.Exit(0);
                }
                if (annoVersion == ANNO_VERSION_1404 || annoVersion == ANNO_VERSION_2070 || annoVersion == ANNO_VERSION_2205)
                {
                    validVersion = true;
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine("Invalid input, please try again or enter 'quit to exit.");
                }
            }

            Console.WriteLine("Extracting and parsing RDA data from {0} for anno version {1}.", BASE_PATH, annoVersion);

            #endregion

            #region Anno Verion Data Paths
            //These should stay constant for different anno versions (hopefully!)
                #region Anno 1404 xPaths
            if (annoVersion == "1404")
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
                new PathRef("addondata/config/game/assets.xml", "/AssetList/Groups/Group/Groups/Group", "Groups/Group/Groups/Group/Assets/Asset", "PlayerBuildings"),
                //new PathRef("data/config/game/assets.xml", "/AssetList/Groups/Group/Groups/Group"),
                new PathRef("addondata/config/balancing/addon_01_assets.xml", "/Group/Groups/Group/Groups/Group", "Groups/Group/Groups/Group/Assets/Asset", "PlayerBuildings")
                });
            }
                #endregion

                #region Anno 2070 xPaths
            if (annoVersion == "2070")
            {
                VersionSpecificPaths.Add(ANNO_VERSION_2070, new Dictionary<string, PathRef[]>());
                VersionSpecificPaths[ANNO_VERSION_2070].Add("icons", new PathRef[]
                {
                new PathRef("data/config/game/icons.xml", "/Icons/i", "", "")
                });
                VersionSpecificPaths[ANNO_VERSION_2070].Add("localisation", new PathRef[]
                {
                new PathRef("data/loca", "", "", "")
                });
                VersionSpecificPaths[ANNO_VERSION_2070].Add("assets", new PathRef[]
                {
                new PathRef("data/config/game/assets.xml", "/AssetList/Groups/Group/Groups/Group", "Groups/Group/Groups/Group/Assets/Asset", "Buildings"),
                });
            }
                #endregion

                #region Anno 2205 xPaths
            if (annoVersion == "2205")
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
            // Paths for 1800
            ///Because there is no extracted data available, the Program need to be terminated
                #endregion

            #endregion

            #region Preparing Writing JSON Files for 1404 and 2070
            if (annoVersion == "1404" || annoVersion == "2070")
            {
                // prepare localizations
                Dictionary<string, SerializableDictionary<string>> localizations = GetLocalizations(annoVersion);

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
                WriteIconNameMapping(iconNodes, localizations, annoVersion);
                #endregion

                #region Preparing presets.json file
                // parse buildings
                List<BuildingInfo> buildings = new List<BuildingInfo>();

                // find buildings in assets.xml
                Console.WriteLine();
                Console.WriteLine("Parsing assets.xml:");
                foreach (PathRef p in VersionSpecificPaths[annoVersion]["assets"])
                {
                    ParseAssetsFile(BASE_PATH + p.Path, p.XPath, p.YPath, buildings, iconNodes, localizations, p.InnerNameTag, annoVersion);
                }

                //No longer needed
                //// find buildings in addon_01_assets.xml
                //Console.WriteLine();
                //Console.WriteLine("Parsing addon_01_assets.xml:");
                ////"Groups/Group/Groups/Group/"
                //ParseAssetsFile(BASE_PATH + "addondata/config/balancing/addon_01_assets.xml", "/Group/Groups/Group/Groups/Group", buildings, iconNodes, localizations);
                #endregion

                #region Finalizing presets.json file and Ending program 
                // serialize presets to json file
                BuildingPresets presets = new BuildingPresets { Version = BUILDING_PRESETS_VERSION, Buildings = buildings };
                // I have put '-Anno<annoVersion>" to the presets.json name, to avoid overwriting the other presets.json
                // so, the name now will as i.e. presets-Anno1404.json
                Console.WriteLine("Writing buildings to presets-{0}.json", annoVersion);
                DataIO.SaveToFile(presets, "presets-Anno" + annoVersion + ".json");
                // wait for keypress before exiting
                Console.WriteLine();
                Console.WriteLine("Do not forget to copy the contents to the normal");
                Console.WriteLine("presets.json, in the Anno Designer directory!");
                Console.WriteLine();
                Console.WriteLine("DONE - press enter to exit");
                Console.ReadLine();
                #endregion
            }
            #endregion

            #region Preparing Writing JSON Files for 2205 (and maybe 1800)
            else if (annoVersion == "2205" || annoVersion == "1800")
            {
                #region Preparing icon.json file (Not Used on 2205)
                // prepare icon mapping
                //XmlDocument iconsDocument = new XmlDocument();
                //List<XmlNode> iconNodes = new List<XmlNode>();
                //foreach (PathRef p in VersionSpecificPaths[annoVersion]["icons"])
                //{
                //    iconsDocument.Load(BasePath + p.Path);
                //    iconNodes.AddRange(iconsDocument.SelectNodes(p.XPath).Cast<XmlNode>());
                //}
                // write icon name mapping
                //Console.WriteLine("Writing icon name mapping to icons.json");
                //WriteIconNameMapping(iconNodes, localizations, annoVerion);
                #endregion

                #region Preparing presets.json file
                // parse buildings
                List<BuildingInfo> buildings = new List<BuildingInfo>();

                // find buildings in assets.xml
                Console.WriteLine();
                Console.WriteLine("Parsing assets.xml:");
                foreach (PathRef p in VersionSpecificPaths[annoVersion]["assets"])
                {
                    ParseAssetsFile2205(BASE_PATH + p.Path, p.XPath, p.YPath, buildings, p.InnerNameTag, annoVersion);
                }
                //No longer needed
                //// find buildings in addon_01_assets.xml
                //Console.WriteLine();
                //Console.WriteLine("Parsing addon_01_assets.xml:");
                ////"Groups/Group/Groups/Group/"
                //ParseAssetsFile(BASE_PATH + "addondata/config/balancing/addon_01_assets.xml", "/Group/Groups/Group/Groups/Group", buildings, iconNodes, localizations);
                #endregion

                #region Finalizing presets.json file and Ending program 
                // serialize presets to json file
                BuildingPresets presets = new BuildingPresets { Version = BUILDING_PRESETS_VERSION, Buildings = buildings };
                // I have put '-Anno<annoVersion>" to the presets.json name, to avoid overwriting the other presets.json
                // so, the name now will as ex. presets-Anno2205.json
                Console.WriteLine("Writing buildings to presets-{0}.json", annoVersion);
                DataIO.SaveToFile(presets, "presets-Anno" + annoVersion + ".json");
                // wait for keypress before exiting
                Console.WriteLine();
                Console.WriteLine("Do not forget to copy the contents to the normal");
                Console.WriteLine("presets.json, in the Anno Designer directory!");
                Console.WriteLine();
                Console.WriteLine("DONE - press enter to exit");
                Console.ReadLine();
                #endregion
            }
            #endregion
        }
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
            XmlElement values = buildingNode["Values"];
            // skip invalid elements
            if (buildingNode["Template"] == null)
            {
                return;
            }
            // parse stuff
            BuildingInfo b = new BuildingInfo
            {
                Header = "Anno " + (annoVersion),
                Faction = buildingNode.ParentNode.ParentNode.ParentNode.ParentNode["Name"].InnerText,
                Group = buildingNode.ParentNode.ParentNode["Name"].InnerText,
                Template = buildingNode["Template"].InnerText,
                Identifier = values["Standard"]["Name"].InnerText
            };
            // print progress
            Console.WriteLine(b.Identifier);
            #endregion

            #region Parse building blocker
            // parse building blocker
            if (!RetrieveBuildingBlocker(b, values["Object"]["Variations"].FirstChild["Filename"].InnerText))
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
                b.IconFileName = GetIconFilename(icon["Icons"].FirstChild);
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
                string translation = "";
                foreach (string Language in Languages)
                {
                    translation = values["Standard"]["Name"].InnerText;
                    b.Localization.Dict.Add(Languages[languageCount], translation);
                    languageCount++;
                }
            }
             #endregion

            // add building to the list
            buildings.Add(b);
        }
        #endregion

        /// Parsing Part for 2205 (and maybe 1800)
        #region Parsing Buildngs for Anno 2205 (and maybe 1800)
        private static void ParseAssetsFile2205(string filename, string xPathToBuildingsNode, string YPath, List<BuildingInfo> buildings, string innerNameTag, string annoVersion)
        {
            XmlDocument assetsDocument = new XmlDocument();
            assetsDocument.Load(filename);
            XmlNode buildingNodes = assetsDocument.SelectNodes(xPathToBuildingsNode)
                .Cast<XmlNode>().Single(_ => _["Name"].InnerText == innerNameTag); //This differs between anno versions
            foreach (XmlNode buildingNode in buildingNodes.SelectNodes(YPath).Cast<XmlNode>())
            {
                ParseBuilding2205(buildings, buildingNode, annoVersion);
            }
        }

        /// ORGLINE: private static void ParseBuilding2205(List<BuildingInfo> buildings, XmlNode buildingNode, IEnumerable<XmlNode> iconNodes, Dictionary<string, SerializableDictionary<string>> localizations)
        private static void ParseBuilding2205(List<BuildingInfo> buildings, XmlNode buildingNode, string annoVersion)
        {
            #region Get valid Building Information 
            XmlElement values = buildingNode["Values"];
            // skip invalid elements
            if (buildingNode["Template"] == null)
            {
                return;
            }
            // Skip Unused buildings in Anno Designer List;
            if (annoVersion == "2205")
            {
                string nameValue = values["Standard"]["Name"].InnerText;
                isExcludedName = nameValue.Contains(ExcludeNameList2205);
                string templateValue = buildingNode["Template"].InnerText;
                isExcludedTemplate = templateValue.Contains(ExcludeTemplateList2205);
                if (isExcludedName == true || isExcludedTemplate == true)
                {
                    Console.WriteLine("{0} <---> {1}", nameValue, templateValue);
                    Console.WriteLine("- Building will skipped - Unused Designer Object");
                    return;

                }
            }
            // parse stuff
            BuildingInfo b = new BuildingInfo
            {
                Header = "Anno " + (annoVersion),
                Faction = buildingNode.ParentNode.ParentNode.ParentNode.ParentNode["Name"].InnerText,
                Group = buildingNode.ParentNode.ParentNode["Name"].InnerText,
                Template = buildingNode["Template"].InnerText,
                Identifier = values["Standard"]["Name"].InnerText
            };
            // print progress
            Console.WriteLine(b.Identifier);
            #endregion

            #region parse building blocker
            if (values["Object"] != null)
            {
                if (values["Object"]?["Variations"]?.FirstChild["Filename"]?.InnerText != null)
                {
                    if (!RetrieveBuildingBlocker(b, values["Object"]["Variations"].FirstChild["Filename"].InnerText))
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
                if (annoVersion == "2205")
                {
                    replaceName = "A6_";
                }
                else if (annoVersion == "1800")
                {
                    replaceName = "A7_";
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

            #region Get Influence Radius
            //Ther will no Influence Radius for anno 2205 (Lines will stay if we need them for 1800)
            if (annoVersion != "2205")
            {   // read influence radius if existing 
                //try
                //{
                //    b.InfluenceRadius = Convert.ToInt32(values["Influence"]["InfluenceRadius"].InnerText);
                //}
                //catch (NullReferenceException ex) { }
                // Building the Localizations for building b

            }
            #endregion

            #region Get Localization Translations ofr Building Names
            /// find localization
            string buildingGuid = values["Standard"]["GUID"].InnerText;
            string languageFileName = ""; /// This will be given thru the static LanguagesFiles array
            /// Do not change any value's below till ----
            string languageFilePath = "";
            string languageFileStart = ""; 
            string langNodeStartPath = ""; 
            string langNodeDepth = "";
            int languageCount = 0;
            ///------------------------------------------
            if (annoVersion == "2205")
            {
                languageFilePath = "data/config/gui/"; 
                languageFileStart = "texts_"; 
                langNodeStartPath = "/TextExport/Texts/Text"; 
                langNodeDepth = "Text"; 
            }

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
                if (translationNodes != null) {
                    translation = translationNodes?.SelectNodes(langNodeDepth)?.Item(0).InnerText;
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
                languageCount++;
            }
            /// original Block in case anno 1800 works diverent
            //if (localizations.ContainsKey(buildingGuid))
            //{
            //   b.Localization = localizations[buildingGuid];
            //}
            #endregion
            // add building to the list
            buildings.Add(b);
        }
        #endregion

        /// Other Classes and or Internal Commands used in this program
        #region Retrieving BuildingBlockers from Buidings Nodes
        private static bool RetrieveBuildingBlocker(BuildingInfo building, string variationFilename)
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
                    building.BuildBlocker["x"] = Math.Abs(Convert.ToInt32(node["x"].InnerText) / 2048);
                }
                else
                {
                   building.BuildBlocker["x"] = 1;
                }
                if (Math.Abs(Convert.ToInt32(node["z"].InnerText) / 2048) > 0)
                {
                    building.BuildBlocker["z"] = Math.Abs(Convert.ToInt32(node["z"].InnerText) / 2048);
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
        private static Dictionary<string, SerializableDictionary<string>> GetLocalizations(string annoVersion)
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
        private static void WriteIconNameMapping(IEnumerable<XmlNode> iconNodes, Dictionary<string, SerializableDictionary<string>> localizations, string annoVersion)
        {
            List<IconNameMap> mapping = new List<IconNameMap>();
            foreach (XmlNode iconNode in iconNodes)
            {
                string guid = iconNode["GUID"].InnerText;
                string iconFilename = GetIconFilename(iconNode["Icons"].FirstChild);
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
            DataIO.SaveToFile(mapping, "icons"+annoVersion+".json");
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

            public PathRef(string path)
            {
                Path = path;
            }
        }
        #endregion
    }
}
