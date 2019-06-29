using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using WikiClientLibrary.Client;
using WikiClientLibrary.Pages;
using WikiClientLibrary.Sites;

namespace FandomParser.WikiText
{
    public class WikiTextProvider
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        //alternative provider? https://github.com/fablecode/wikia-core
        //API useable? https://anno1800.fandom.com/api/v1/

        //this is using https://github.com/CXuesong/WikiClientLibrary

        public async Task<WikiTextProviderResult> GetWikiTextAsync(string pageToFetch = "Buildings")
        {
            var result = new WikiTextProviderResult();

            try
            {
                using (var wikiClient = new WikiClient())
                {
                    wikiClient.ClientUserAgent = "Parser-Test/1.0";

                    var mainSite = new WikiSite(wikiClient, new SiteOptions("https://anno1800.fandom.com/api.php"));
                    await mainSite.Initialization;

                    Console.WriteLine($"fetching page: {pageToFetch}");

                    var page = new WikiPage(mainSite, pageToFetch);
                    await page.RefreshAsync(PageQueryOptions.FetchContent | PageQueryOptions.ResolveRedirects);

                    if (!page.Exists)
                    {
                        //Page not found
                        return result;
                    }

                    //var pageId = page.Id;//186 = overview (Buildings)

                    result.WikiText = page.Content;//complete article/page in wikitext
                    result.RevisionId = page.LastRevisionId;//Id of last revision
                    result.EditDate = page.LastTouched;//DateTime last edit

                    return result;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"error fetching wikitext: {pageToFetch}");
                Console.WriteLine($"error fetching wikitext ({pageToFetch})" + Environment.NewLine + ex.ToString());
                return result;
            }
        }
    }
}
