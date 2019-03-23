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
        ///<summary>
        ///Path to extracted RDA files (prompt the user for this eventually?)
        ///</summary>   
        private static string BASE_PATH = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\RDAExplorer\Working copies\Extract\"; //Include the trailing backslash on this

        private const string ANNO_VERSION_1404 = "1404";

        private static Dictionary<string, Dictionary<string, PathRef[]>> VersionSpecificPaths { get; set; }


        private static readonly string[] Languages = new[] { "cze", "eng", "esp", "fra", "frus", "ger", "ita", "pol", "rus", "spus", "usa" }; //TODO add languages after this


        private static string GetIconFilename(XmlNode iconNode)
        {
            return string.Format("icon_{0}_{1}.png", iconNode["IconFileID"].InnerText, iconNode["IconIndex"] != null ? iconNode["IconIndex"].InnerText : "0");
        }

        static Program()
        {
            VersionSpecificPaths = new Dictionary<string, Dictionary<string, PathRef[]>>();
        }

        public static void Main(string[] args)
        {

            VersionSpecificPaths.Add(ANNO_VERSION_1404, new Dictionary<string, PathRef[]>());
            VersionSpecificPaths[ANNO_VERSION_1404].Add("icons", new PathRef[]
            {
                new PathRef("data/config/game/icons.xml", "/Icons/i" ),
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
                new PathRef("data/config/game/assets.xml", "/AssetList/Groups/Group/Groups/Group"),
                new PathRef("addondata/config/balancing/addon_01_assets.xml", "/Group/Groups/Group/Groups/Group")
            });



            // prepare localizations
            Dictionary<string, SerializableDictionary<string>> localizations = GetLocalizations(ANNO_VERSION_1404);

            // prepare icon mapping
            XmlDocument iconsDocument = new XmlDocument();
            List<XmlNode> iconNodes = new List<XmlNode>();

            foreach (PathRef p in VersionSpecificPaths[ANNO_VERSION_1404]["icons"])
            {
                iconsDocument.Load(BASE_PATH + p.Path);
                iconNodes.AddRange(iconsDocument.SelectNodes(p.XPath).Cast<XmlNode>());
            }


            // write icon name mapping
            Console.WriteLine("Writing icon name mapping to icons.json");
            WriteIconNameMapping(iconNodes, localizations);

            // parse buildings
            List<BuildingInfo> buildings = new List<BuildingInfo>();

            // find buildings in assets.xml
            Console.WriteLine();
            Console.WriteLine("Parsing assets.xml:");
            foreach (PathRef p in VersionSpecificPaths[ANNO_VERSION_1404]["assets"])
            {
                ParseAssetsFile(BASE_PATH + p.Path, p.XPath, buildings, iconNodes, localizations);
            }

            //No longer needed
            //// find buildings in addon_01_assets.xml
            //Console.WriteLine();
            //Console.WriteLine("Parsing addon_01_assets.xml:");
            ////"Groups/Group/Groups/Group/"
            //ParseAssetsFile(BASE_PATH + "addondata/config/balancing/addon_01_assets.xml", "/Group/Groups/Group/Groups/Group", buildings, iconNodes, localizations);

            // serialize presets to json file
            BuildingPresets presets = new BuildingPresets { Version = "0.5", Buildings = buildings };
            Console.WriteLine("Writing buildings to presets.json");
            DataIO.SaveToFile(presets, "presets.json");

            // wait for keypress before exiting
            Console.WriteLine();
            Console.WriteLine("DONE - press enter to exit");
            Console.ReadLine();
        }

        private static void ParseAssetsFile(string filename, string xPathToBuildingsNode, List<BuildingInfo> buildings,
            IEnumerable<XmlNode> iconNodes, Dictionary<string, SerializableDictionary<string>> localizations)
        {
            XmlDocument assetsDocument = new XmlDocument();
            assetsDocument.Load(filename);
            XmlNode buildingNodes = assetsDocument.SelectNodes(xPathToBuildingsNode)
                .Cast<XmlNode>().Single(_ => _["Name"].InnerText == "PlayerBuildings"); //This was different before (for a different anno) 

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
            ifoDocument.Load(Path.Combine(BASE_PATH + "/", string.Format("{0}.ifo", Path.GetDirectoryName(variationFilename) + "\\" + Path.GetFileNameWithoutExtension(variationFilename))));
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
