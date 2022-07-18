using System;
using System.Globalization;
using System.IO;
using System.Xml;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Core.Presets.Models;
using PresetParser.Extensions;

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

        //Adding Coastal Buildings and Coastal Farm Fields to a List, to avoid long 'if statements' (Anno 2205 01-07-2022)
        private static readonly List<string> Anno2205_CoastalFarmFieldsList = new List<string> { "production chemical arctic facility module 01 tier 01", "production food arctic facility module 02 tier 01", "production food arctic facility module 01 tier 01",
        "production chemical earth facility module 01 tier 01"};
        private static readonly List<string> Anno2205_CoastalFarmBuildingsList = new List<string> { "production agriculture earth facility 05 t", "production chemical earth facility 01 t", "production chemical arctic facility 01", "production energy earth facility 02",
        "production food arctic facility 02", "production food arctic facility 01", "production heavy arctic facility 04"};


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
                var xf = 0;
                var zf = 0;
                string xc, zc = ""; // just information for checking line calculated mode

                // Change since 25-05-2021 - Fixing measurements of Buildings Buildblockers. 
                // Instead of taking one XF * 2 and ZF * 2, it will check now the differences between 2 given XF's and ZF's
                // Get all 4 [Position] childs from xml .ifo document
                // change since 30-05-2022 - Fixing "no <buildblocker> tag in files"
                if (string.IsNullOrEmpty(ifoDocument.FirstChild?[BUILDBLOCKER]?.FirstChild?.InnerText))
                {
                    {
                        var oldColor = Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine("- No BUIDBLOCKER in files - Building will be skipped");
                        Console.ForegroundColor = oldColor;
                        return false;
                    }
                };

                XmlNode node1 = ifoDocument.FirstChild?[BUILDBLOCKER].FirstChild;
                XmlNode node2 = ifoDocument.FirstChild?[BUILDBLOCKER].FirstChild.NextSibling;
                XmlNode node3 = ifoDocument.FirstChild?[BUILDBLOCKER].FirstChild.NextSibling.NextSibling;
                XmlNode node4 = ifoDocument.FirstChild?[BUILDBLOCKER].FirstChild.NextSibling.NextSibling.NextSibling;

                //check of the nodes contains data
                if (string.IsNullOrEmpty(node1?.InnerText) || string.IsNullOrEmpty(node2?.InnerText) || string.IsNullOrEmpty(node3?.InnerText) || string.IsNullOrEmpty(node4?.InnerText))
                {
                    var oldColor = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("-'X' and 'Z' are both 'Null' - Building will be skipped!");
                    Console.ForegroundColor = oldColor;
                    return false;
                }

                building.BuildBlocker = new SerializableDictionary<int>();

                //Convert the strings to a Variable and replace the "." for a "," to keep calculatable numbers
                var xfNormal1 = double.Parse(node1["xf"].InnerText, NumberStyles.Any, CultureInfo.InvariantCulture);
                var zfNormal1 = double.Parse(node1["zf"].InnerText, NumberStyles.Any, CultureInfo.InvariantCulture);
                var xfNormal2 = double.Parse(node2["xf"].InnerText, NumberStyles.Any, CultureInfo.InvariantCulture);
                var zfNormal2 = double.Parse(node2["zf"].InnerText, NumberStyles.Any, CultureInfo.InvariantCulture);
                var xfNormal3 = double.Parse(node3["xf"].InnerText, NumberStyles.Any, CultureInfo.InvariantCulture);
                var zfNormal3 = double.Parse(node3["zf"].InnerText, NumberStyles.Any, CultureInfo.InvariantCulture);
                var xfNormal4 = double.Parse(node4["xf"].InnerText, NumberStyles.Any, CultureInfo.InvariantCulture);
                var zfNormal4 = double.Parse(node4["zf"].InnerText, NumberStyles.Any, CultureInfo.InvariantCulture);

                // Calculation mode check highest number minus lowest number
                // example 1:  9 - -2 = 11
                // example 2: 2,5 - -2,5 = 5
                // This will give the right BuildBlocker[X] and BuildBlocker[Y] for all buildings from anno 1800

                // XF Calculation 
                if (xfNormal1 > xfNormal3)
                {
                    xf = Convert.ToInt32(xfNormal1 - xfNormal3);
                    xc = "MA";// just information for checking line calculated mode
                }
                else
                {
                    xf = Convert.ToInt32(xfNormal3 - xfNormal1);
                    xc = "MB";// just information for checking line calculated mode
                }

                // zf Calculation 
                if (zfNormal1 > zfNormal2)
                {
                    zf = Convert.ToInt32(zfNormal1 - zfNormal2);
                    zc = "MA";// just information for checking line calculated mode
                }
                else if (zfNormal1 == zfNormal2)
                {
                    if (zfNormal1 > zfNormal3)
                    {
                        zf = Convert.ToInt32(zfNormal1 - zfNormal3);
                        zc = "MB";// just information for checking line calculated mode
                    }
                    else
                    {
                        zf = Convert.ToInt32(zfNormal3 - zfNormal1);
                        zc = "MD";// just information for checking line calculated mode
                    }
                }
                else
                {
                    zf = Convert.ToInt32(zfNormal2 - zfNormal1);
                    zc = "MC";// just information for checking line calculated mode
                }

                if ((xf == 0 || zf == 0) && building.Identifier != "Trail_05x05")
                {
                    //when something goes wrong on the measurements, report and stop till a key is hit
                    Console.WriteLine("MEASUREMENTS GOING WRONG!!! CHECK THIS BUILDING");
                    Console.WriteLine(" Node 1 - XF: {0} | ZF: {1} ;\n Node 2 - XF: {2} | ZF: {3} ;\n Node 3 - XF: {4} | ZF: {5} ;\n Node 4 - XF: {6} | ZF: {7}", xfNormal1, zfNormal1, xfNormal2, zfNormal2, xfNormal3, zfNormal3, xfNormal4, zfNormal4);
                    Console.WriteLine("Building measurement is : {0} x {1} (Method {2} and {3})", xf, zf, xc, zc);
                    Console.WriteLine("Press a key to continue");
                    //Console.ReadKey();
                }
                // if both values are zero, then skip building
                if (xf < 1 && zf < 1)
                {
                    var oldColor = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("-'X' and 'Z' are both 0 - Building will be skipped!");
                    Console.ForegroundColor = oldColor;
                    return false;
                }
                // Repair Palace Gate Wrong Z measurements
                if (building.Identifier == "Palace_Module_05 (gate)")
                {
                    zf = 3;
                }
                // correcting measurements of Industrial Zone Ornamentals (GUID Wise)
                if (building.Guid == 393 || building.Guid == 677 || building.Guid == 678)
                {
                    xf = 3;
                    zf = 1;
                }
                if (building.Guid == 388)
                {
                    xf = 3;
                    zf = 3;
                }

                //
                if (xf > 0)
                {
                    building.BuildBlocker[X] = Math.Abs(xf);
                }
                else
                {
                    building.BuildBlocker[X] = 1;
                }

                if (zf > 0)
                {
                    building.BuildBlocker[Z] = Math.Abs(zf);
                }
                else
                {
                    building.BuildBlocker[Z] = 1;
                }
            }
            catch (NullReferenceException)
            {
                var oldColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("- BuildBlocker not found, skipping");
                Console.ForegroundColor = oldColor;
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
                building.BuildBlocker = new SerializableDictionary<int>();

                var x = 0;
                var z = 0;

                if (node is null)
                {
                    //I have to do this here, as file does not contain <BuildBlocker> tag, and this farm field is needed
                    if (building.Header == "(A6) Anno 2205" && building.Identifier == "production heavy arctic facility module 04 tier 01")
                    {
                        x = 3;
                        z = 3;
                    }
                    else
                    {
                        return false;
                    }
                }
                if (building.Identifier != "production heavy arctic facility module 04 tier 01")
                {
                    x = Math.Abs(Convert.ToInt32(node[X].InnerText) / 2048);
                    z = Math.Abs(Convert.ToInt32(node[Z].InnerText) / 2048);
                }

                //if both values are zero, then skip building
                if (x < 1 && z < 1)
                {
                    Console.WriteLine("-'X' and 'Z' are both 0 - Building will skipped!");
                    return false;
                }
                //correcting measurement of anno 2205 building: Cybernetics Factory (file say 6x7, in game it is 6x8) (10-01-2021)
                if (variationFilenameWithoutExtension == "production_biotech_moon_facility_02")
                {
                    x = 6;
                    z = 8;
                }
                //correcting Measurement of anno 1404 building: Coal Mine (on assets 2x3, as all others are 4x3 i set it equal as that (18-06-2022)
                if (building.Header == "(A4) Anno 1404" && building.Identifier == "CoalMine")
                {
                    x = 4;
                    z = 3;
                }
                //correcting Underwater Warehouse (GUID 10035) from 2x6 to 3x6 (anno 2070, 28-06-2022)
                if (building.Header == "(A5) Anno 2070" && building.Identifier == "warehouse")
                {
                    x = 3;
                    z = 6;
                }
                //correcting Center Statistics (Tycoons) from 3x4 to 6x6 (anno 2070, 29-06-2022)
                if (building.Header == "(A5) Anno 2070" && building.Identifier == "statistic_center_tycoons")
                {
                    x = 6;
                    z = 6;
                }
                //Correcting some Water Farm Fields from 2x? to 3x3 (Anno 2205, 30-06-2022)
                if (building.Header == "(A6) Anno 2205" && building.Identifier.IsMatch(Anno2205_CoastalFarmFieldsList))
                {
                    x = 3;
                    z = 3;
                }
                //Correcting Algae Bed (Temperate) from 2x1 to 2x12 (Anno 2205, 30-06-2022)
                if (building.Header == "(A6) Anno 2205" && (building.Identifier == "production agriculture earth facility module 05 tier 01"))
                {
                    x = 2;
                    z = 12;
                }
                //Correcting Water Coastal Farm buildings, from 2x6 to 3x6 as in game measurements (Anno 2205, 01-07-2022)
                if (building.Header == "(A6) Anno 2205" && building.Identifier.IsMatch(Anno2205_CoastalFarmBuildingsList))
                {
                    x = 3;
                    z = 6;
                }
                //Correcting Mining Building Measurements, to set all Mining buildings the same size 4x8 (Anno 2205, 01-07-2022)
                if (building.Header == "(A6) Anno 2205" && building.Group == "Mining")
                {
                    x = 4;
                    z = 8;
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

    }
}
