using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FandomParser.Core;
using FandomParser.WikiText;

namespace FandomParser
{
    public class WikiBuildingDetailProvider
    {
        private const string DIRECTORY_BUILDING_DETAILS = "building_infos_detailed";
        private const string DIRECTORY_BUILDING_INFOBOX = "building_infoboxes_extracted";
        private const string FILENAME_MISSING_INFOS = "_missing_info.txt";
        private const string FILE_ENDING_WIKITEXT = ".txt";
        private const string FILE_ENDING_INFOBOX = ".infobox";

        private string _pathToDetailsFolder;
        private string _pathToExtractedInfoboxesFolder;
        private readonly ICommons _commons;

        public WikiBuildingDetailProvider(ICommons commons)
        {
            _commons = commons;
        }

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

                    var indexInfoboxBothWorldsStart = fileContent.IndexOf(_commons.InfoboxTemplateStartBothWorlds, StringComparison.OrdinalIgnoreCase);
                    var indexInfoboxStart = fileContent.IndexOf(_commons.InfoboxTemplateStart, StringComparison.OrdinalIgnoreCase);

                    var startIndex = indexInfoboxBothWorldsStart == -1 ? indexInfoboxStart : indexInfoboxBothWorldsStart;

                    //handle files with no infobox
                    if (startIndex == -1)
                    {
                        continue;
                    }

                    var endIndex = fileContent.IndexOf(_commons.InfoboxTemplateEnd, startIndex, StringComparison.OrdinalIgnoreCase);
                    int length = endIndex - startIndex + _commons.InfoboxTemplateEnd.Length;

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
                var infoboxParser = new InfoboxParser.InfoboxParser(_commons);

                foreach (var curFile in Directory.EnumerateFiles(PathToExtractedInfoboxesFolder, $"*{FILE_ENDING_INFOBOX}", SearchOption.TopDirectoryOnly))
                {
                    var fileContent = File.ReadAllText(curFile, Encoding.UTF8);
                    var infobox = infoboxParser.GetInfobox(fileContent);

                    //multiple entries possible e.g. "Police Station"
                    var foundWikiBuildingInfos = wikiBuildingInfoList.Infos.Where(x => x.Name.Equals(infobox.Name, StringComparison.OrdinalIgnoreCase));
                    foreach (var curBuilding in foundWikiBuildingInfos)
                    {
                        curBuilding.Type = infobox.Type;
                        curBuilding.ProductionInfos = infobox.ProductionInfos;
                        curBuilding.SupplyInfos = infobox.SupplyInfos;
                        curBuilding.UnlockInfos = infobox.UnlockInfos;
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
            //based on benchmarks, a Regex.Replace is slower and allocates more memory -> NOT better: return Regex.Replace(wikiText, @"\r\n|\n\r|\n|\r", Environment.NewLine);
            return wikiText.Replace("\r\n", "\n")
                .Replace("\r", "\n")
                .Replace("\n", Environment.NewLine);
        }
    }
}
