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
        private static string BasePath { get; set; }

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

        private const string BUILDING_PRESETS_VERSION = "0.8.1";

        private static readonly string[] Languages = new[] { "cze", "eng", "esp", "fra", "ger", "ita", "pol", "rus" };

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
                    BasePath = path.LastOrDefault() == '\\' ? path : path + "\\";
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

            Console.WriteLine("Extracting and parsing RDA data from {0} for anno version {1}.", BasePath, annoVersion);

            #endregion

            #region Anno Verion Data Paths

            //These should stay constant for different anno versions (hopefully!)
            // Paths for Anno 1404
            if (annoVersion == "1404")
            {
                VersionSpecificPaths.Add(ANNO_VERSION_1404, new Dictionary<string, PathRef[]>());
                VersionSpecificPaths[ANNO_VERSION_1404].Add("icons", new PathRef[]
                {
                //new PathRef("data/config/game/icons.xml", "/Icons/i" ),
                new PathRef("addondata/config/game/icons.xml", "/Icons/i")
                });
                VersionSpecificPaths[ANNO_VERSION_1404].Add("localisation", new PathRef[]
                {
                new PathRef("data/loca", ""),
                new PathRef("addondata/loca", "")
                });
                VersionSpecificPaths[ANNO_VERSION_1404].Add("assets", new PathRef[]
                {
                new PathRef("addondata/config/game/assets.xml", "/AssetList/Groups/Group/Groups/Group"),
                //new PathRef("data/config/game/assets.xml", "/AssetList/Groups/Group/Groups/Group"),
                new PathRef("addondata/config/balancing/addon_01_assets.xml", "/Group/Groups/Group/Groups/Group")
                });
            }
            // Paths for 2070
            if (annoVersion == "2070")
            {
                VersionSpecificPaths.Add(ANNO_VERSION_2070, new Dictionary<string, PathRef[]>());
                VersionSpecificPaths[ANNO_VERSION_2070].Add("icons", new PathRef[]
                {
                new PathRef("data/config/game/icons.xml", "/Icons/i" )
                });
                VersionSpecificPaths[ANNO_VERSION_2070].Add("localisation", new PathRef[]
                {
                new PathRef("data/loca", "")
                });
                VersionSpecificPaths[ANNO_VERSION_2070].Add("assets", new PathRef[]
                {
                new PathRef("data/config/game/assets.xml", "/AssetList/Groups/Group/Groups/Group"),
                });
            }
            // Paths for 2205
            if (annoVersion == "2205")
            {
                VersionSpecificPaths.Add(ANNO_VERSION_2205, new Dictionary<string, PathRef[]>());
                //VersionSpecificPaths[ANNO_VERSION_2205].Add("icons", new PathRef[]
                //{
                //new PathRef("data/config/game/icons.xml", "/Icons/i" )
                //});
                VersionSpecificPaths[ANNO_VERSION_2205].Add("localisation", new PathRef[]
                {
                new PathRef("data/dlc01/config/game/asset/", "") 
               //We need to thing of a different solution for this, due to the lack of language codes as the directory names
                });
                VersionSpecificPaths[ANNO_VERSION_2205].Add("assets", new PathRef[]
                {
                new PathRef("data/dlc01/config/game/asset/assets.xml", "/AssetList/Groups/Group/Groups/Group")
                });
            }

            // Paths for 1800
            ///Because there is no extracted data available, the Program need to be terminated

            #endregion

            #region Preparing Writing JSON Files
            // prepare localizations
            Dictionary<string, SerializableDictionary<string>> localizations = GetLocalizations(annoVersion);

            #region Preparing icon.json file
            // prepare icon mapping
            XmlDocument iconsDocument = new XmlDocument();
            List<XmlNode> iconNodes = new List<XmlNode>();
            foreach (PathRef p in VersionSpecificPaths[annoVersion]["icons"])
            {
                iconsDocument.Load(BasePath + p.Path);
                iconNodes.AddRange(iconsDocument.SelectNodes(p.XPath).Cast<XmlNode>());
            }
            if (annoVersion != "2205") //skip the icons.json file writing on Anno 2205 , lack of text files
            {
                // write icon name mapping
                Console.WriteLine("Writing icon name mapping to icons.json");
                WriteIconNameMapping(iconNodes, localizations);
            }
            #endregion

            #region Preparing presets.json file
            // parse buildings
            List<BuildingInfo> buildings = new List<BuildingInfo>();

            // find buildings in assets.xml
            string innerNameTag = "";
            if (annoVersion == "1404") { innerNameTag = "PlayerBuildings"; Console.WriteLine(); Console.WriteLine("innerNameTag set to: {0}", innerNameTag);};
            if (annoVersion == "2070") { innerNameTag = "Buildings"; Console.WriteLine(); Console.WriteLine("innerNameTag set to: {0}", innerNameTag); };
            if (annoVersion == "2205") { innerNameTag = "Buildings"; Console.WriteLine(); Console.WriteLine("innerNameTag set to: {0}", innerNameTag); };
            //if (annoVersion == "1800") { innerNameTag = "Buildings"; Console.WriteLine(); Console.WriteLine("innerNameTag set to: {0}", innerNameTag); };
            Console.WriteLine();
            Console.WriteLine("Parsing assets.xml:");
            foreach (PathRef p in VersionSpecificPaths[annoVersion]["assets"])
            {
                ParseAssetsFile(BasePath + p.Path, p.XPath, buildings, iconNodes, localizations, innerNameTag );
            }

            //No longer needed
            //// find buildings in addon_01_assets.xml
            //Console.WriteLine();
            //Console.WriteLine("Parsing addon_01_assets.xml:");
            ////"Groups/Group/Groups/Group/"
            //ParseAssetsFile(BASE_PATH + "addondata/config/balancing/addon_01_assets.xml", "/Group/Groups/Group/Groups/Group", buildings, iconNodes, localizations);
            #endregion

            #endregion

            #region Finalizing presets.json file and Ending program 
            // serialize presets to json file
            BuildingPresets presets = new BuildingPresets { Version = BUILDING_PRESETS_VERSION, Buildings = buildings };
            Console.WriteLine("Writing buildings to presets.json");
            DataIO.SaveToFile(presets, "presets.json");

            // wait for keypress before exiting
            Console.WriteLine();
            Console.WriteLine("DONE - press enter to exit");
            Console.ReadLine();
            #endregion
        }

        private static void ParseAssetsFile(string filename, string xPathToBuildingsNode, List<BuildingInfo> buildings,
            IEnumerable<XmlNode> iconNodes, Dictionary<string, SerializableDictionary<string>> localizations, string innerNameTag)
        {
            XmlDocument assetsDocument = new XmlDocument();
            assetsDocument.Load(filename);
            XmlNode buildingNodes = assetsDocument.SelectNodes(xPathToBuildingsNode)
                .Cast<XmlNode>().Single(_ => _["Name"].InnerText == innerNameTag); //This differs between anno versions
            foreach (XmlNode buildingNode in buildingNodes.SelectNodes("Groups/Group/Groups/Group/Assets/Asset").Cast<XmlNode>())
            {
                ParseBuilding(buildings, buildingNode, iconNodes, localizations);
            }
        }

        private static void ParseBuilding(List<BuildingInfo> buildings, XmlNode buildingNode, IEnumerable<XmlNode> iconNodes, Dictionary<string, SerializableDictionary<string>> localizations)
        {
            XmlElement values = buildingNode["Values"];
            // skip invalid elements
            if (buildingNode["Template"] == null)
            {
                return;
            }
            // parse stuff
            BuildingInfo b = new BuildingInfo
            {
                Faction = buildingNode.ParentNode.ParentNode.ParentNode.ParentNode["Name"].InnerText,
                Group = buildingNode.ParentNode.ParentNode["Name"].InnerText,
                Template = buildingNode["Template"].InnerText,
                Identifier = values["Standard"]["Name"].InnerText
            };
            // print progress
            Console.WriteLine(b.Identifier);
            // parse building blocker
            if (!RetrieveBuildingBlocker(b, values["Object"]["Variations"].FirstChild["Filename"].InnerText))
            {
                return;
            }
            // find icon node based on guid match
            string buildingGuid = values["Standard"]["GUID"].InnerText;
            XmlNode icon = iconNodes.FirstOrDefault(_ => _["GUID"].InnerText == buildingGuid);
            if (icon != null)
            {
                b.IconFileName = GetIconFilename(icon["Icons"].FirstChild);
            }
            // read influence radius if existing
            try
            {
                b.InfluenceRadius = Convert.ToInt32(values["Influence"]["InfluenceRadius"].InnerText);
            }
            catch (NullReferenceException ex) { }
            // find localization
            if (localizations.ContainsKey(buildingGuid))
            {
                b.Localization = localizations[buildingGuid];
            }
            // add building to the list
            buildings.Add(b);
        }

        private static bool RetrieveBuildingBlocker(BuildingInfo building, string variationFilename)
        {
            XmlDocument ifoDocument = new XmlDocument();
            ifoDocument.Load(Path.Combine(BasePath + "/", string.Format("{0}.ifo", Path.GetDirectoryName(variationFilename) + "\\" + Path.GetFileNameWithoutExtension(variationFilename))));
            try
            {
                XmlNode node = ifoDocument.FirstChild["BuildBlocker"].FirstChild;
                building.BuildBlocker = new SerializableDictionary<int>();
                building.BuildBlocker["x"] = Math.Abs(Convert.ToInt32(node["x"].InnerText) / 2048);
                building.BuildBlocker["z"] = Math.Abs(Convert.ToInt32(node["z"].InnerText) / 2048);
            }
            catch (NullReferenceException)
            {
                Console.WriteLine("-BuildBlocker not found, skipping");
                return false;
            }
            return true;
        }

        private class GuidRef
        {
            public string Language;
            public string Guid;
            public string GuidReference;
        }

        private static Dictionary<string, SerializableDictionary<string>> GetLocalizations(string annoVersion)
        {
            string[] files = { "icons.txt", "guids.txt", "addon/texts.txt" };
            Dictionary<string, SerializableDictionary<string>> localizations = new Dictionary<string, SerializableDictionary<string>>();
            List<GuidRef> references = new List<GuidRef>();
            foreach (string language in Languages)
            {
                foreach (PathRef p in VersionSpecificPaths[annoVersion]["localisation"])
                {
                    string basePath = Path.Combine(BasePath, p.Path, language, "txt");
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

        private static void WriteIconNameMapping(IEnumerable<XmlNode> iconNodes, Dictionary<string, SerializableDictionary<string>> localizations)
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
            DataIO.SaveToFile(mapping, "icons.json");
        }

        private class PathRef
        {
            public string Path { get; set; }
            public string XPath { get; set; }

            public PathRef(string path, string xPath)
            {
                Path = path;
                XPath = xPath;
            }
        }
    }
}
