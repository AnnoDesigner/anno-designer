using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FandomParser.WikiText;

namespace FandomParser
{
    public class WikiBuildingDetailProvider
    {
        private const string DIRECTORY_BUILDING_DETAILS = "building_infos_detailed";
        private const string DIRECTORY_BUILDING_INFOBOX = "building_infoboxes_extracted";
        private const string FILENAME_MISSING_INFOS = "_missing_info.txt";
        private const string INFOBOX_TEMPLATE_START = "{{Infobox Buildings";
        private const string INFOBOX_TEMPLATE_BOTH_WORLDS_START = "{{Infobox Buildings Old and New World";
        private const string INFOBOX_TEMPLATE_END = "}}";
        private const string FILE_ENDING_WIKITEXT = ".txt";
        private const string FILE_ENDING_INFOBOX = ".infobox";

        private string _pathToDetailsFolder;
        private string _pathToExtractedInfoboxesFolder;

        private Regex regexInputAmount = new Regex(@"(?<begin>\|Input)\s*(?<counter>\d+)\s*(?<end>Amount)\s*(?<equalSign>[=])\s*(?<value>\d*(?:[\.\,]\d*)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private Regex regexInputAmountElectricity = new Regex(@"(?<begin>\|Input)\s*(?<counter>\d+)\s*(?<end>Amount Electricity)\s*(?<equalSign>[=])\s*(?<value>\d*(?:[\.\,]\d*)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private Regex regexInputIcon = new Regex(@"(?<begin>\|Input)\s*(?<counter>\d+)\s*(?<end>Icon)\s*(?<equalSign>[=])\s*(?<fileName>(\w*\s*)+([\.]\w*)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public string PathToDetailsFolder
        {
            get { return _pathToDetailsFolder ?? (_pathToDetailsFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), DIRECTORY_BUILDING_DETAILS)); }
        }

        public string PathToExtractedInfoboxesFolder
        {
            get { return _pathToExtractedInfoboxesFolder ?? (_pathToExtractedInfoboxesFolder = Path.Combine(PathToDetailsFolder, DIRECTORY_BUILDING_INFOBOX)); }
        }

        public WikiBuildingInfoList FetchBuildingDetails(WikiBuildingInfoList wikiBuildingInfoList)
        {
            //download complete wikitext for each building
            saveCompleteInfos(wikiBuildingInfoList);

            //extract infobox for each found wikitext
            extractAllInfoboxes();

            //parse infoboxes
            return getUpdatedWikiBuildingInfoList(wikiBuildingInfoList);
        }

        private void saveCompleteInfos(WikiBuildingInfoList wikiBuildingInfoList)
        {
            Console.WriteLine("start fetching building details");

            if (!Directory.Exists(PathToDetailsFolder))
            {
                Directory.CreateDirectory(PathToDetailsFolder);
            }

            var existingMissingInfos = new List<string>();
            if (File.Exists(Path.Combine(PathToDetailsFolder, FILENAME_MISSING_INFOS)))
            {
                existingMissingInfos.AddRange(File.ReadAllLines(Path.Combine(PathToDetailsFolder, FILENAME_MISSING_INFOS), Encoding.UTF8));
            }

            var missingInfos = new List<string>();

            Stopwatch sw = new Stopwatch();
            sw.Start();

            Parallel.ForEach(wikiBuildingInfoList.Infos,
                new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
                curBuilding =>
                {
                    var pageName = curBuilding.Name.Replace(" ", "_");

                    //only download when not present
                    var fileName = getCleanedFilename(pageName);
                    var destinationFilePath = Path.Combine(PathToDetailsFolder, $"{fileName}{FILE_ENDING_WIKITEXT}");
                    if (File.Exists(destinationFilePath) || existingMissingInfos.Contains(curBuilding.Name))
                    {
                        return;
                    }

                    var provider = new WikiTextProvider();
                    var wikiText = provider.GetWikiTextAsync(pageName).GetAwaiter().GetResult();

                    if (!string.IsNullOrWhiteSpace(wikiText))
                    {
                        wikiText = getLineBreakAlignedWikiText(wikiText);
                        //align infobox name
                        wikiText = wikiText.Replace("{{Infobox_Buildings", "{{Infobox Buildings");
                        File.WriteAllText(destinationFilePath, wikiText, Encoding.UTF8);
                    }
                    else
                    {
                        missingInfos.Add(curBuilding.Name);
                    }
                });

            if (missingInfos.Count > 0)
            {
                File.WriteAllLines(Path.Combine(PathToDetailsFolder, FILENAME_MISSING_INFOS), missingInfos.Distinct().OrderBy(x => x), Encoding.UTF8);
            }

            sw.Stop();
            Console.WriteLine($"finished fetching building details (took {sw.ElapsedMilliseconds} ms)");
        }

        private void extractAllInfoboxes()
        {
            Console.WriteLine("start extracting infoboxes");

            Stopwatch sw = new Stopwatch();
            sw.Start();

            if (!Directory.Exists(PathToExtractedInfoboxesFolder))
            {
                Directory.CreateDirectory(PathToExtractedInfoboxesFolder);
            }

            foreach (var curFile in Directory.EnumerateFiles(PathToDetailsFolder, $"*{FILE_ENDING_WIKITEXT}", SearchOption.TopDirectoryOnly)
                .Where(x => !Path.GetFileName(x).Equals(FILENAME_MISSING_INFOS, StringComparison.OrdinalIgnoreCase)))
            {
                try
                {
                    var fileName = Path.GetFileNameWithoutExtension(curFile);

                    var destinationFilePath = Path.Combine(PathToExtractedInfoboxesFolder, $"{fileName}{FILE_ENDING_INFOBOX}");
                    if (File.Exists(destinationFilePath))
                    {
                        continue;
                    }

                    //TODO handle files with multiple infoboxes e.g. Shrubbery

                    var fileContent = File.ReadAllText(curFile, Encoding.UTF8);

                    var indexInfoboxBothWorldsStart = fileContent.IndexOf(INFOBOX_TEMPLATE_BOTH_WORLDS_START, StringComparison.OrdinalIgnoreCase);
                    var indexInfoboxStart = fileContent.IndexOf(INFOBOX_TEMPLATE_START, StringComparison.OrdinalIgnoreCase);

                    var startIndex = indexInfoboxBothWorldsStart == -1 ? indexInfoboxStart : indexInfoboxBothWorldsStart;

                    //handle files with no infobox
                    if (startIndex == -1)
                    {
                        continue;
                    }

                    var endIndex = fileContent.IndexOf(INFOBOX_TEMPLATE_END, startIndex, StringComparison.OrdinalIgnoreCase);
                    int length = endIndex - startIndex + INFOBOX_TEMPLATE_END.Length;

                    var infoBox = fileContent.Substring(startIndex, length);
                    if (!string.IsNullOrWhiteSpace(infoBox))
                    {
                        //format infobox with new entries on separate lines for later parsing
                        var splittedInfobox = infoBox.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                        var infoboxWithLineBreaks = string.Join(Environment.NewLine + "|", splittedInfobox);

                        File.WriteAllText(destinationFilePath, infoboxWithLineBreaks, Encoding.UTF8);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            sw.Stop();
            Console.WriteLine($"finished extracting infoboxes (took {sw.ElapsedMilliseconds} ms)");
        }

        private WikiBuildingInfoList getUpdatedWikiBuildingInfoList(WikiBuildingInfoList wikiBuildingInfoList)
        {
            Console.WriteLine("start parsing infoboxes");

            Stopwatch sw = new Stopwatch();
            sw.Start();

            try
            {
                foreach (var curFile in Directory.EnumerateFiles(PathToExtractedInfoboxesFolder, $"*{FILE_ENDING_INFOBOX}", SearchOption.TopDirectoryOnly))
                {
                    var fileContent = File.ReadAllText(curFile, Encoding.UTF8);

                    var buildingName = getBuildingName(fileContent);
                    if (string.IsNullOrWhiteSpace(buildingName))
                    {
                    }

                    var buildingType = getBuildingType(fileContent);
                    if (buildingType == BuildingType.Unknown)
                    {

                    }

                    ProductionInfo productionInfo = null;
                    //TODO parse infoboxes containing information of multiple worlds
                    if (!fileContent.StartsWith(INFOBOX_TEMPLATE_BOTH_WORLDS_START))
                    {
                        productionInfo = getProductionInfo(fileContent);
                        if (productionInfo == null && buildingType == BuildingType.Production)
                        {

                        }
                    }

                    //handle special cases
                    switch (buildingName)
                    {
                        case "Bombin Weaver":
                            buildingName = "Bomb­ín Weaver";
                            break;
                        default:
                            break;
                    }

                    //multiple entries possible e.g. "Police Station"
                    var foundWikiBuildingInfos = wikiBuildingInfoList.Infos.Where(x => x.Name.Equals(buildingName, StringComparison.OrdinalIgnoreCase));
                    foreach (var curBuilding in foundWikiBuildingInfos)
                    {
                        curBuilding.Type = buildingType;
                        curBuilding.ProductionInfos = productionInfo;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            sw.Stop();
            Console.WriteLine($"finished parsing infoboxes (took {sw.ElapsedMilliseconds} ms)");

            return wikiBuildingInfoList;
        }

        private static string getCleanedFilename(string fileNameToClean)
        {
            return fileNameToClean.Replace("#", "_")
                .Replace("|", "_")
                .Replace(":", "_");
        }

        private static string getLineBreakAlignedWikiText(string wikiText)
        {
            //better? return Regex.Replace(wikiText, @"\r\n|\n\r|\n|\r", Environment.NewLine);
            return wikiText.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", Environment.NewLine);
        }

        private static BuildingType getBuildingType(string infobox)
        {
            var result = BuildingType.Unknown;

            if (!infobox.Contains("|Building Type"))
            {
                return result;
            }

            using (var reader = new StringReader(infobox))
            {
                string curLine;
                while ((curLine = reader.ReadLine()) != null)
                {
                    curLine = curLine.Replace(INFOBOX_TEMPLATE_END, string.Empty);

                    if (curLine.StartsWith("|Building Type", StringComparison.OrdinalIgnoreCase))
                    {
                        var buildingType = curLine.Replace("|Building Type (OW)", string.Empty)
                            .Replace("|Building Type (NW)", string.Empty)
                            .Replace("|Building Type", string.Empty)
                            .Replace("=", string.Empty)
                            .Trim();

                        if (Enum.TryParse(buildingType, ignoreCase: true, out BuildingType parsedBuildingType))
                        {
                            result = parsedBuildingType;
                        }

                        break;
                    }
                }
            }

            return result;
        }

        private static string getBuildingName(string infobox)
        {
            var result = string.Empty;

            if (!infobox.Contains("|Title"))
            {
                return result;
            }

            using (var reader = new StringReader(infobox))
            {
                string curLine;
                while ((curLine = reader.ReadLine()) != null)
                {
                    curLine = curLine.Replace(INFOBOX_TEMPLATE_END, string.Empty);

                    if (curLine.StartsWith("|Title", StringComparison.OrdinalIgnoreCase))
                    {
                        result = curLine.Replace("|Title", string.Empty)
                            .Replace("=", string.Empty)
                            .Trim();
                        break;
                    }
                }
            }

            return result;
        }

        private ProductionInfo getProductionInfo(string infobox)
        {
            ProductionInfo result = null;

            //short circuit infoboxes without production info
            if (!infobox.Contains("|Produces Amount"))
            {
                return result;
            }

            result = new ProductionInfo();
            result.EndProduct = new EndProduct();

            using (var reader = new StringReader(infobox))
            {
                string curLine;
                while ((curLine = reader.ReadLine()) != null)
                {
                    curLine = curLine.Replace(INFOBOX_TEMPLATE_END, string.Empty);

                    if (curLine.StartsWith("|Produces Amount Electricity", StringComparison.OrdinalIgnoreCase))
                    {
                        var productionAmountElectricity = curLine.Replace("|Produces Amount Electricity", string.Empty)
                            .Replace("=", string.Empty)
                            .Trim();

                        if (double.TryParse(productionAmountElectricity, out double parsedProductionAmountElectricity))
                        {
                            result.EndProduct.AmountElectricity = parsedProductionAmountElectricity;
                        }
                    }
                    else if (curLine.StartsWith("|Produces Amount", StringComparison.OrdinalIgnoreCase))
                    {
                        var productionAmount = curLine.Replace("|Produces Amount", string.Empty)
                            .Replace("=", string.Empty)
                            .Trim();

                        if (double.TryParse(productionAmount, out double parsedProductionAmount))
                        {
                            result.EndProduct.Amount = parsedProductionAmount;
                        }
                    }
                    else if (curLine.StartsWith("|Produces Icon", StringComparison.OrdinalIgnoreCase))
                    {
                        var icon = curLine.Replace("|Produces Icon", string.Empty)
                            .Replace("=", string.Empty)
                            .Trim();

                        result.EndProduct.Icon = icon;
                    }
                    else if (curLine.StartsWith("|Input ", StringComparison.OrdinalIgnoreCase))
                    {
                        var matchAmount = regexInputAmount.Match(curLine);
                        if (matchAmount.Success)
                        {
                            var matchedCounter = matchAmount.Groups["counter"].Value;
                            if (!int.TryParse(matchedCounter, out int counter))
                            {
                                throw new Exception("could not find counter");
                            }

                            //handle entry with no value e.g. "|Input 2 Amount     = "
                            var matchedValue = matchAmount.Groups["value"].Value.Replace(",", ".");
                            if (string.IsNullOrWhiteSpace(matchedValue))
                            {
                                continue;
                            }

                            if (!double.TryParse(matchedValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double inputValue))
                            {
                                throw new Exception("could not find value for input");
                            }

                            var foundInputProduct = result.InputProducts.FirstOrDefault(x => x.Order == counter);
                            if (foundInputProduct == null)
                            {
                                foundInputProduct = new InputProduct
                                {
                                    Order = counter
                                };

                                result.InputProducts.Add(foundInputProduct);
                            }

                            foundInputProduct.Amount = inputValue;

                            continue;
                        }

                        var matchAmountElectricity = regexInputAmountElectricity.Match(curLine);
                        if (matchAmountElectricity.Success)
                        {
                            var matchedCounter = matchAmountElectricity.Groups["counter"].Value;
                            if (!int.TryParse(matchedCounter, out int counter))
                            {
                                throw new Exception("could not find counter");
                            }

                            //handle entry with no value e.g. "|Input 1 Amount Electricity    = "
                            var matchedValue = matchAmountElectricity.Groups["value"].Value.Replace(",", ".");
                            if (string.IsNullOrWhiteSpace(matchedValue))
                            {
                                continue;
                            }

                            if (!double.TryParse(matchedValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double inputValue))
                            {
                                throw new Exception("could not find value for input");
                            }

                            var foundInputProduct = result.InputProducts.FirstOrDefault(x => x.Order == counter);
                            if (foundInputProduct == null)
                            {
                                foundInputProduct = new InputProduct
                                {
                                    Order = counter
                                };

                                result.InputProducts.Add(foundInputProduct);
                            }

                            foundInputProduct.AmountElectricity = inputValue;

                            continue;
                        }

                        var matchIcon = regexInputIcon.Match(curLine);
                        if (matchIcon.Success)
                        {
                            var matchedCounter = matchIcon.Groups["counter"].Value;
                            if (!int.TryParse(matchedCounter, out int counter))
                            {
                                throw new Exception("could not find counter");
                            }

                            //handle entry with no value e.g. "|Input 1 Icon = "
                            var matchedFileName = matchIcon.Groups["fileName"].Value;
                            if (string.IsNullOrWhiteSpace(matchedFileName))
                            {
                                continue;
                            }

                            var foundInputProduct = result.InputProducts.FirstOrDefault(x => x.Order == counter);
                            if (foundInputProduct == null)
                            {
                                foundInputProduct = new InputProduct
                                {
                                    Order = counter
                                };

                                result.InputProducts.Add(foundInputProduct);
                            }

                            foundInputProduct.Icon = matchedFileName;

                            continue;
                        }
                    }
                }
            }

            return result;
        }
    }
}
