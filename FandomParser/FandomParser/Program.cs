using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FandomParser.Core;
using FandomParser.Core.Helper;
using FandomParser.WikiText;

namespace FandomParser
{
    public static class Program
    {
        private const string ARG_NO_WAIT = "--noWait";
        private const string ARG_FORCE_DOWNLOAD = "--forceDownload";
        private const string ARG_OUTPUT_DIRECTORY = "--out=";
        private const string ARG_VERSION = "--version=";
        private const string ARG_PRETTY_PRINT = "--prettyPrint";

        public static bool NoWait { get; set; }

        public static bool ForceDownload { get; set; } = true;

        public static string OutputDirectory { get; set; }

        public static bool UsePrettyPrint { get; set; }

        public static Version PresetVersion { get; set; } = new Version(2, 0, 0, 0);

        public static async Task Main(string[] args)
        {
            try
            {
                OutputDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "..", "..", "..", "..", "Presets");

                parseArguments(args);

                //Console.WriteLine($"{nameof(ForceDownload)}: {ForceDownload}");

                if (!Directory.Exists(OutputDirectory))
                {
                    Directory.CreateDirectory(OutputDirectory);
                }

                Console.WriteLine($"Directory for output: \"{Path.GetFullPath(OutputDirectory)}\"");
                Console.WriteLine($"Version of preset file: {PresetVersion}");
                Console.WriteLine();

                WikiTextTableContainer tableContainer = null;

                if (!File.Exists("wiki_basic_info.json") || ForceDownload)
                {
                    var provider = new WikiTextProvider();
                    var providerResult = await provider.GetWikiTextAsync();

                    var tableParser = new WikiTextTableParser();
                    tableContainer = tableParser.GetTables(providerResult.WikiText);

                    SerializationHelper.SaveToFile(tableContainer, "wiki_basic_info.json", prettyPrint: true);
                }
                else
                {
                    tableContainer = SerializationHelper.LoadFromFile<WikiTextTableContainer>("wiki_basic_info.json");
                }

                //get basic info of all buildings
                var wikiBuildingInfoProvider = new WikiBuildingInfoProvider();
                var wikiBuildingInfoPreset = wikiBuildingInfoProvider.GetWikiBuildingInfos(tableContainer);

                //get detail info of all buildings
                var wikiDetailProvider = new WikiBuildingDetailProvider(Commons.Instance);
                wikiBuildingInfoPreset = wikiDetailProvider.FetchBuildingDetails(wikiBuildingInfoPreset);
                wikiBuildingInfoPreset.Version = PresetVersion;
                wikiBuildingInfoPreset.DateGenerated = DateTime.UtcNow;

                SerializationHelper.SaveToFile(wikiBuildingInfoPreset, Path.Combine(OutputDirectory, CoreConstants.WikiBuildingInfoPresetsFile), prettyPrint: UsePrettyPrint);

                //for testing
                //load parsed file to test
                //wikiBuildingInfoPreset = SerializationHelper.LoadFromFile<WikiBuildingInfoPreset>(PRESET_FILENAME);

                var oldColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Finished parsing all building information.");
                Console.ForegroundColor = oldColor;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex);
            }
            finally
            {
                if (!NoWait)
                {
                    Console.WriteLine();
                    Console.WriteLine("To exit press any key ...");
                    Console.ReadLine();
                }
            }
        }

        private static void parseArguments(string[] args)
        {
            if (args?.Any(x => x.Trim().Equals(ARG_FORCE_DOWNLOAD, StringComparison.OrdinalIgnoreCase)) == true)
            {
                ForceDownload = true;
            }

            if (args?.Any(x => x.Trim().Equals(ARG_NO_WAIT, StringComparison.OrdinalIgnoreCase)) == true)
            {
                NoWait = true;
            }

            if (args?.Any(x => x.Trim().Equals(ARG_PRETTY_PRINT, StringComparison.OrdinalIgnoreCase)) == true)
            {
                UsePrettyPrint = true;
            }

            if (args?.Any(x => x.Trim().StartsWith(ARG_OUTPUT_DIRECTORY, StringComparison.OrdinalIgnoreCase)) == true)
            {
                var curArg = args.SingleOrDefault(x => x.Trim().StartsWith(ARG_OUTPUT_DIRECTORY, StringComparison.OrdinalIgnoreCase));

                if (!string.IsNullOrWhiteSpace(curArg))
                {
                    var splitted = curArg.Split('=');
                    if (splitted.Length == 2)
                    {
                        var outputPath = splitted[1];
                        if (string.IsNullOrWhiteSpace(outputPath))
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"Please specify an output directory.");
                            Environment.Exit(-1);
                        }
                        else
                        {
                            var parsedDirectory = Path.GetDirectoryName(outputPath);
                            if (!Directory.Exists(outputPath))
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"The specified directory was not found: \"{outputPath}\"");
                                Environment.Exit(-1);
                            }

                            OutputDirectory = outputPath;
                        }
                    }
                }
            }

            if (args?.Any(x => x.Trim().StartsWith(ARG_VERSION, StringComparison.OrdinalIgnoreCase)) == true)
            {
                var curArg = args.SingleOrDefault(x => x.Trim().StartsWith(ARG_VERSION, StringComparison.OrdinalIgnoreCase));

                if (!string.IsNullOrWhiteSpace(curArg))
                {
                    var splitted = curArg.Split('=');
                    if (splitted.Length == 2)
                    {
                        var versionString = splitted[1];
                        if (string.IsNullOrWhiteSpace(versionString))
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"Please specify a version. (x.x.x.x)");
                            Environment.Exit(-1);
                        }
                        else
                        {
                            if (!Version.TryParse(versionString, out Version parsedVersion))
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"The specified version could not be parsed: \"{versionString}\" Please use format x.x.x.x");
                                Environment.Exit(-1);
                            }

                            PresetVersion = parsedVersion;
                        }
                    }
                }
            }
        }
    }
}
