using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FandomParser.Core;
using FandomParser.Core.Presets.Models;
using FandomParser.WikiText;
using InfoboxParser;
using NLog;

namespace FandomParser
{
    public class WikiBuildingDetailProvider
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private const string DIRECTORY_BUILDING_DETAILS = "building_infos_detailed";
        private const string DIRECTORY_BUILDING_INFOBOX = "building_infoboxes_extracted";
        private const string FILENAME_MISSING_INFOS = "_missing_info.txt";
        private const string FILE_ENDING_WIKITEXT = ".txt";
        private const string FILE_ENDING_INFOBOX = ".infobox";
        private const string REVISION_HEADER_ID = "RevisionId";
        private const string REVISION_HEADER_DATE = "LastEdit";
        private const string REVISION_SEPARATOR = "=";

        private string _pathToDetailsFolder;
        private string _pathToExtractedInfoboxesFolder;
        private readonly ICommons _commons;
        private readonly IInfoboxExtractor _infoboxExtractor;

        public WikiBuildingDetailProvider(ICommons commons)
        {
            _commons = commons;

            _infoboxExtractor = new InfoboxExtractor(_commons);
        }

        public string PathToDetailsFolder
        {
            get { return _pathToDetailsFolder ?? (_pathToDetailsFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), DIRECTORY_BUILDING_DETAILS)); }
        }

        public string PathToExtractedInfoboxesFolder
        {
            get { return _pathToExtractedInfoboxesFolder ?? (_pathToExtractedInfoboxesFolder = Path.Combine(PathToDetailsFolder, DIRECTORY_BUILDING_INFOBOX)); }
        }

        public WikiBuildingInfoPresets FetchBuildingDetails(WikiBuildingInfoPresets wikiBuildingInfoList)
        {
            //download complete wikitext for each building
            SaveCompleteInfos(wikiBuildingInfoList);

            //extract infobox for each found wikitext
            ExtractAllInfoboxes();

            //parse infoboxes
            return GetUpdatedWikiBuildingInfoList(wikiBuildingInfoList);
        }

        private void SaveCompleteInfos(WikiBuildingInfoPresets wikiBuildingInfoList)
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

                    //only download when not present and not forced to download
                    var fileName = GetCleanedFilename(pageName);
                    var destinationFilePath = Path.Combine(PathToDetailsFolder, $"{fileName}{FILE_ENDING_WIKITEXT}");
                    if (!Program.ForceDownload && (File.Exists(destinationFilePath) || existingMissingInfos.Contains(curBuilding.Name)))
                    {
                        return;
                    }

                    var provider = new WikiTextProvider();
                    var providerResult = provider.GetWikiTextAsync(pageName).GetAwaiter().GetResult();

                    if (!string.IsNullOrWhiteSpace(providerResult.WikiText))
                    {
                        providerResult.WikiText = GetLineBreakAlignedWikiText(providerResult.WikiText);
                        //align infobox name
                        providerResult.WikiText = providerResult.WikiText.Replace("{{Infobox_Buildings", "{{Infobox Buildings");
                        File.WriteAllText(destinationFilePath, providerResult.WikiText, Encoding.UTF8);
                        File.AppendAllLines(destinationFilePath, new List<string>
                        {
                            Environment.NewLine,
                            $"{REVISION_HEADER_ID}{REVISION_SEPARATOR}{providerResult.RevisionId}",
                            $"{REVISION_HEADER_DATE}{REVISION_SEPARATOR}{providerResult.EditDate.ToString("o")}"
                        }, Encoding.UTF8);
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
            var message = $"finished fetching building details (took {sw.ElapsedMilliseconds} ms)";
            logger.Trace(message);
            Console.WriteLine(message);
        }

        private void ExtractAllInfoboxes()
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

                    //Note: Don't handle files with multiple infoboxes  -> should be separate page in wiki
                    var fileContent = File.ReadAllText(curFile, Encoding.UTF8);

                    var extractedInfobox = _infoboxExtractor.ExtractInfobox(fileContent);
                    if (string.IsNullOrWhiteSpace(extractedInfobox))
                    {
                        continue;
                    }

                    File.WriteAllText(destinationFilePath, extractedInfobox, Encoding.UTF8);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"error extracting infobox: \"{curFile}\"");
                    Console.WriteLine(ex);
                }
            }

            sw.Stop();
            Console.WriteLine($"finished extracting infoboxes (took {sw.ElapsedMilliseconds} ms)");
        }

        private WikiBuildingInfoPresets GetUpdatedWikiBuildingInfoList(WikiBuildingInfoPresets wikiBuildingInfoList)
        {
            Console.WriteLine("start parsing infoboxes");

            Stopwatch sw = new Stopwatch();
            sw.Start();

            try
            {
                var specialBuildingNameHelper = new SpecialBuildingNameHelper();
                var regionHelper = new RegionHelper();
                var infoboxParser = new InfoboxParser.InfoboxParser(_commons, specialBuildingNameHelper, regionHelper);

                foreach (var curFile in Directory.EnumerateFiles(PathToExtractedInfoboxesFolder, $"*{FILE_ENDING_INFOBOX}", SearchOption.TopDirectoryOnly))
                {
                    var fileContent = File.ReadAllText(curFile, Encoding.UTF8);
                    var infoboxes = infoboxParser.GetInfobox(fileContent);

                    if (infoboxes.Count == 1)
                    {
                        var parsedInfobox = infoboxes[0];

                        WikiBuildingInfo foundWikiBuildingInfo;
                        if (parsedInfobox.Region == WorldRegion.Unknown)
                        {
                            foundWikiBuildingInfo = wikiBuildingInfoList.Infos.FirstOrDefault(x => x.Name.Equals(parsedInfobox.Name, StringComparison.OrdinalIgnoreCase));
                        }
                        else
                        {
                            throw new Exception("expected unknown region");
                        }

                        if (foundWikiBuildingInfo == null)
                        {
                            if (parsedInfobox?.Name.Contains("Road") == true)
                            {
                                //is page with multiple infoboxes -> not supported yet
                                continue;
                            }

                            var exception = new Exception("No WikiBuildingInfo found!");
                            exception.Data.Add(nameof(curFile), curFile);
                            exception.Data.Add($"{nameof(parsedInfobox)}.{nameof(parsedInfobox.Name)}", parsedInfobox.Name);

                            throw exception;
                        }

                        var buildingNameForUrl = foundWikiBuildingInfo.Name.Replace(" ", "_");

                        foundWikiBuildingInfo.Type = parsedInfobox.Type;
                        foundWikiBuildingInfo.ProductionInfos = parsedInfobox.ProductionInfos;
                        foundWikiBuildingInfo.SupplyInfos = parsedInfobox.SupplyInfos;
                        foundWikiBuildingInfo.UnlockInfos = parsedInfobox.UnlockInfos;

                        //check Url for "World's Fair" | correct: https://anno1800.fandom.com/wiki/World%27s_Fair
                        if (buildingNameForUrl.Contains("World's_Fair"))
                        {
                            buildingNameForUrl = "World%27s_Fair";
                        }

                        //(maybe) TODO check Url for "Bombín Weaver" | correct: https://anno1800.fandom.com/wiki/Bomb%C2%AD%C3%ADn_Weaver
                        //contains unicode char (https://www.utf8-chartable.de/unicode-utf8-table.pl):  U+00AD	­	c2 ad	SOFT HYPHEN

                        foundWikiBuildingInfo.Url = new Uri("https://anno1800.fandom.com/wiki/" + buildingNameForUrl);

                        var revisionInfo = GetRevisionInfo(foundWikiBuildingInfo);
                        if (revisionInfo != null)
                        {
                            foundWikiBuildingInfo.RevisionId = revisionInfo.Item1;
                            foundWikiBuildingInfo.RevisionDate = revisionInfo.Item2;
                        }
                    }
                    else if (infoboxes.Count > 1)
                    {
                        foreach (var curInfobox in infoboxes)
                        {
                            //multiple entries possible e.g. "Police Station" or "Museum"
                            var foundWikiBuildingInfo = wikiBuildingInfoList.Infos.FirstOrDefault(x => x.Name.Equals(curInfobox.Name, StringComparison.OrdinalIgnoreCase) && x.Region == curInfobox.Region);
                            if (foundWikiBuildingInfo == null)
                            {
                                if (curInfobox?.Name.Contains("Road") == true)
                                {
                                    //is page with multiple infoboxes -> not supported yet
                                    continue;
                                }

                                var exception = new Exception("No WikiBuildingInfo found!");
                                exception.Data.Add(nameof(curFile), curFile);
                                exception.Data.Add($"{nameof(curInfobox)}.{nameof(curInfobox.Name)}", curInfobox.Name);

                                throw exception;
                            }

                            var buildingNameForUrl = foundWikiBuildingInfo.Name.Replace(" ", "_");

                            foundWikiBuildingInfo.Type = curInfobox.Type;
                            foundWikiBuildingInfo.ProductionInfos = curInfobox.ProductionInfos;
                            foundWikiBuildingInfo.SupplyInfos = curInfobox.SupplyInfos;
                            foundWikiBuildingInfo.UnlockInfos = curInfobox.UnlockInfos;
                            foundWikiBuildingInfo.Url = new Uri("https://anno1800.fandom.com/wiki/" + buildingNameForUrl);

                            var revisionInfo = GetRevisionInfo(foundWikiBuildingInfo);
                            if (revisionInfo != null)
                            {
                                foundWikiBuildingInfo.RevisionId = revisionInfo.Item1;
                                foundWikiBuildingInfo.RevisionDate = revisionInfo.Item2;
                            }
                        }
                    }
                    else
                    {
                        //got no infoboxes
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"error parsing infoboxes");
                Console.WriteLine(ex);
            }

            sw.Stop();
            Console.WriteLine($"finished parsing infoboxes (took {sw.ElapsedMilliseconds} ms)");

            return wikiBuildingInfoList;
        }

        private static string GetCleanedFilename(string fileNameToClean)
        {
            return fileNameToClean.Replace("#", "_")
                .Replace("|", "_")
                .Replace(":", "_");
        }

        private static string GetLineBreakAlignedWikiText(string wikiText)
        {
            //based on benchmarks, a Regex.Replace is slower and allocates more memory -> NOT better: return Regex.Replace(wikiText, @"\r\n|\n\r|\n|\r", Environment.NewLine);
            return wikiText.Replace("\r\n", "\n")
                .Replace("\r", "\n")
                .Replace("\n", Environment.NewLine);
        }

        private Tuple<int, DateTime> GetRevisionInfo(WikiBuildingInfo buildingInfo)
        {
            var buildingName = buildingInfo.Name.Replace(" ", "_");

            var fileName = GetCleanedFilename(buildingName);
            var wikiTextForBuildingFilePath = Path.Combine(PathToDetailsFolder, $"{fileName}{FILE_ENDING_WIKITEXT}");
            if (!File.Exists(wikiTextForBuildingFilePath))
            {
                return null;
            }

            var revisionId = -1;
            var lastEdit = DateTime.MinValue;

            var fileContent = File.ReadAllText(wikiTextForBuildingFilePath);
            using (var reader = new StringReader(fileContent))
            {
                string curLine;
                while ((curLine = reader.ReadLine()) != null)
                {
                    if (curLine.StartsWith(REVISION_HEADER_ID, StringComparison.OrdinalIgnoreCase))
                    {
                        var splittedLine = curLine.Split(new[] { REVISION_SEPARATOR }, StringSplitOptions.RemoveEmptyEntries);
                        if (splittedLine.Length == 2)
                        {
                            if (!int.TryParse(splittedLine[1], out revisionId))
                            {
                                revisionId = -1;
                            }
                        }
                    }
                    else if (curLine.StartsWith(REVISION_HEADER_DATE, StringComparison.OrdinalIgnoreCase))
                    {
                        var splittedLine = curLine.Split(new[] { REVISION_SEPARATOR }, StringSplitOptions.RemoveEmptyEntries);
                        if (splittedLine.Length == 2)
                        {
                            if (!DateTime.TryParseExact(splittedLine[1], "o", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out lastEdit))
                            {
                                lastEdit = DateTime.MinValue;
                            }
                        }
                    }
                }
            }

            return Tuple.Create(revisionId, lastEdit);
        }
    }
}
