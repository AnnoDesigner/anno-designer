using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FandomParser.WikiText;

namespace FandomParser
{

    public static class Program
    {
        private const string ARG_NO_WAIT = "--noWait";
        private const string ARG_FORCE_DOWNLOAD = "--forceDownload";
        private const string ARG_FETCH_BUILDING_DETAILS = "--fetchBuildingDetails";

        public static async Task Main(string[] args)
        {
            var noWait = false;
            try
            {
                var foceDownload = false;
                if (args?.Any(x => x.Equals(ARG_FORCE_DOWNLOAD, StringComparison.OrdinalIgnoreCase)) == true)
                {
                    foceDownload = true;
                }

                if (args?.Any(x => x.Equals(ARG_NO_WAIT, StringComparison.OrdinalIgnoreCase)) == true)
                {
                    noWait = true;
                }

                var fetchBuildingDetails = false;
                if (args?.Any(x => x.Equals(ARG_FETCH_BUILDING_DETAILS, StringComparison.OrdinalIgnoreCase)) == true)
                {
                    fetchBuildingDetails = true;
                }

                Console.WriteLine($"{nameof(foceDownload)}: {foceDownload}");
                Console.WriteLine($"{nameof(fetchBuildingDetails)}: {fetchBuildingDetails}");

                TableEntryList list = null;

                if (!File.Exists("wiki_info.json") || foceDownload)
                {
                    Console.WriteLine("new download");

                    var provider = new WikiTextProvider();
                    var wikiText = await provider.GetWikiTextAsync();

                    var tableProvider = new TableProvider();
                    list = tableProvider.GetTables(wikiText);

                    SerializationHelper.SaveToFile(list, "wiki_info.json");
                }
                else
                {
                    list = SerializationHelper.LoadFromFile<TableEntryList>("wiki_info.json");
                }

                var wikiBuildingInfoProvider = new WikiBuildingInfoProvider();
                var wikibuildingList = wikiBuildingInfoProvider.GetWikiBuildingInfos(list);

                //get production info of all buildings
                if (fetchBuildingDetails)
                {
                    var wikiDetailProvider = new WikiBuildingDetailProvider();
                    wikiDetailProvider.FetchBuildingDetails(wikibuildingList);
                }

                SerializationHelper.SaveToFile(wikibuildingList, "wiki_info_parsed.json");

                //load parsed file to test
                wikibuildingList = SerializationHelper.LoadFromFile<WikiBuildingInfoList>("wiki_info_parsed.json");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                if (!noWait)
                {
                    Console.WriteLine();
                    Console.WriteLine("To exit press any key ...");
                    Console.ReadLine();
                }
            }
        }


    }
}
