using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FandomParser.Core;
using FandomParser.Core.Helper;
using FandomParser.Core.Models;
using FandomParser.WikiText;

namespace FandomParser
{
    public static class Program
    {
        private const string ARG_NO_WAIT = "--noWait";
        private const string ARG_FORCE_DOWNLOAD = "--forceDownload";
        private const string ARG_FETCH_BUILDING_DETAILS = "--fetchBuildingDetails";

        public static bool NoWait { get; set; }

        public static bool ForceDownload { get; set; }

        public static bool FetchBuildingDetails { get; set; }

        public static async Task Main(string[] args)
        {
            try
            {
                parseArguments(args);

                Console.WriteLine($"{nameof(ForceDownload)}: {ForceDownload}");
                Console.WriteLine($"{nameof(FetchBuildingDetails)}: {FetchBuildingDetails}");

                WikiTextTableContainer list = null;

                if (!File.Exists("wiki_info.json") || ForceDownload)
                {
                    Console.WriteLine("new download");

                    var provider = new WikiTextProvider();
                    var wikiText = await provider.GetWikiTextAsync();

                    var tableParser = new WikiTextTableParser();
                    list = tableParser.GetTables(wikiText);

                    SerializationHelper.SaveToFile(list, "wiki_info.json");
                }
                else
                {
                    list = SerializationHelper.LoadFromFile<WikiTextTableContainer>("wiki_info.json");
                }

                var wikiBuildingInfoProvider = new WikiBuildingInfoProvider();
                var wikibuildingList = wikiBuildingInfoProvider.GetWikiBuildingInfos(list);

                //get production info of all buildings
                if (FetchBuildingDetails)
                {
                    var wikiDetailProvider = new WikiBuildingDetailProvider(Commons.Instance);
                    wikibuildingList = wikiDetailProvider.FetchBuildingDetails(wikibuildingList);
                }

                SerializationHelper.SaveToFile(wikibuildingList, "wiki_info_parsed.json");

                //load parsed file to test
                wikibuildingList = SerializationHelper.LoadFromFile<WikiBuildingInfoPreset>("wiki_info_parsed.json");
            }
            catch (Exception ex)
            {
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
            if (args?.Any(x => x.Equals(ARG_FORCE_DOWNLOAD, StringComparison.OrdinalIgnoreCase)) == true)
            {
                ForceDownload = true;
            }

            if (args?.Any(x => x.Equals(ARG_NO_WAIT, StringComparison.OrdinalIgnoreCase)) == true)
            {
                NoWait = true;
            }

            if (args?.Any(x => x.Equals(ARG_FETCH_BUILDING_DETAILS, StringComparison.OrdinalIgnoreCase)) == true)
            {
                FetchBuildingDetails = true;
            }
        }
    }
}
