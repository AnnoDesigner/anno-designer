using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Core.Presets.Models;

namespace PresetParser
{
    public class BuildingBlockProvider
    {
        private const char BUILDING_BLOCKER_SEPARATOR = '.';
        private const string BUILDBLOCKER = "BuildBlocker";
        private const string X = "x";
        private const string Z = "z";
        private const string WATER_MILL_ECOS = "water_mill_ecos";
        private const string ORNAMENTAL_POST_09 = "ornamental_post_09";

        private readonly IIfoFileProvider _ifoFileProvider;

        public BuildingBlockProvider(IIfoFileProvider ifoFileProviderToUse)
        {
            _ifoFileProvider = ifoFileProviderToUse ?? throw new ArgumentNullException(nameof(ifoFileProviderToUse));
        }

        public bool GetBuildingBlocker(string basePath, IBuildingInfo building, string variationFilename, string annoVersion)
        {
            var ifoDocument = _ifoFileProvider.GetIfoFileContent(basePath, variationFilename);

            if (annoVersion.Equals(Constants.ANNO_VERSION_1800, StringComparison.OrdinalIgnoreCase))
            {
                return ParseBuildingBlockerForAnno1800(ifoDocument, building);
            }
            else
            {
                return ParseBuildingBlocker(ifoDocument, building, variationFilename);
            }
        }

        private bool ParseBuildingBlockerForAnno1800(XmlDocument ifoDocument, IBuildingInfo building)
        {
            try
            {
                XmlNode node = ifoDocument.FirstChild?[BUILDBLOCKER].FirstChild;

                //check of the node contains data
                if (string.IsNullOrEmpty(node?.InnerText))
                {
                    Console.WriteLine("-'X' and 'Z' are both 'Null' - Building will be skipped!");
                    return false;
                }

                building.BuildBlocker = new SerializableDictionary<int>();

                string xfNormal = node["xf"].InnerText;
                string zfNormal = node["zf"].InnerText;
                var xf = ParseBuildingBlockerNumber(xfNormal);
                var zf = ParseBuildingBlockerNumber(zfNormal);

                //In case buildings have wrong size change it here.
                //Check by identifier. *Caps Sensetive*
                if (building.Identifier == "Palace_Module_05 (gate)")
                {
                    xf = 3; zf = 3;
                }

                //if both values are zero, then skip building
                if (xf < 1 && zf < 1)
                {
                    Console.WriteLine("-'X' and 'Z' are both 0 - Building will be skipped!");
                    return false;
                }

                if (xf > 0)
                {
                    building.BuildBlocker[X] = xf;
                }
                else
                {
                    building.BuildBlocker[X] = 1;
                }

                if (zf > 0)
                {
                    building.BuildBlocker[Z] = zf;
                }
                else
                {
                    building.BuildBlocker[Z] = 1;
                }
            }
            catch (NullReferenceException)
            {
                Console.WriteLine("-BuildBlocker not found, skipping");
                return false;
            }

            return true;
        }

        private bool ParseBuildingBlocker(XmlDocument ifoDocument, IBuildingInfo building, string variationFilename)
        {
            var variationFilenameWithoutExtension = Path.GetFileNameWithoutExtension(variationFilename);

            try
            {
                XmlNode node = ifoDocument.FirstChild[BUILDBLOCKER]?.FirstChild;
                if (node is null)
                {
                    return false;
                }

                building.BuildBlocker = new SerializableDictionary<int>();

                var x = Math.Abs(Convert.ToInt32(node[X].InnerText) / 2048);
                var z = Math.Abs(Convert.ToInt32(node[Z].InnerText) / 2048);

                //if both values are zero, then skip building
                if (x < 1 && z < 1)
                {
                    Console.WriteLine("-'X' and 'Z' are both 0 - Building will skipped!");
                    return false;
                }

                if (x > 0)
                {
                    //Console.WriteLine("{0}", Path.GetFileNameWithoutExtension(variationFilename));
                    if (variationFilenameWithoutExtension != ORNAMENTAL_POST_09)
                    {
                        if (variationFilenameWithoutExtension != WATER_MILL_ECOS)
                        {
                            building.BuildBlocker[X] = x;
                        }
                        else
                        {
                            building.BuildBlocker[X] = 3;
                        }
                    }
                    else
                    {
                        building.BuildBlocker[X] = 7;
                    }
                }
                else
                {
                    building.BuildBlocker[X] = 1;
                }

                if (z > 0)
                {
                    if (variationFilenameWithoutExtension != WATER_MILL_ECOS && variationFilenameWithoutExtension != ORNAMENTAL_POST_09)
                    {
                        building.BuildBlocker[Z] = z;
                    }
                    else
                    {
                        building.BuildBlocker[Z] = 7;
                    }
                }
                else
                {
                    building.BuildBlocker[Z] = 1;
                }
            }
            catch (NullReferenceException)
            {
                Console.WriteLine("-BuildBlocker not found, skipping");
                return false;
            }

            return true;
        }

        private static int ParseBuildingBlockerNumber(string number)
        {
            int result;

            if (number.Contains(BUILDING_BLOCKER_SEPARATOR))
            {
                var xz = number.Split(BUILDING_BLOCKER_SEPARATOR);
                //Console.WriteLine("1: {0}  2: {1}", xz[0], xz[1]);
                int xz1 = Math.Abs(Convert.ToInt32(xz[0]));
                double xz2 = Math.Abs(Convert.ToInt32(xz[1]));
                //Console.WriteLine("xz1: {0}  xz2: {1}", xz1, xz2);
                var countNumberLenght = xz[1].Length;
                //Console.WriteLine("lebght= {0}", countNumberLenght);

                int i = 0;
                while (i < countNumberLenght)
                {
                    xz2 = xz2 / 10;
                    //Console.WriteLine("{0}", xz2);
                    i++;
                }

                xz1 = xz1 * 2;
                xz2 = xz2 * 2;

                result = xz1 + Convert.ToInt32(xz2);
            }
            else
            {
                result = Math.Abs(Convert.ToInt32(number));
                result = result * 2;
            }

            return result;
        }

    }
}
