using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WikiClientLibrary.Client;
using WikiClientLibrary.Pages;
using WikiClientLibrary.Sites;

namespace FandomParser.WikiText
{
    public class WikiTextProvider
    {
        //alternative provider? https://github.com/fablecode/wikia-core
        //API useable? https://anno1800.fandom.com/api/v1/

        //this is using https://github.com/CXuesong/WikiClientLibrary

        public async Task<string> GetWikiTextAsync(string pageToFetch = "Buildings")
        {
            var wikiText = string.Empty;

            using (var wikiClient = new WikiClient())
            {
                wikiClient.ClientUserAgent = "Parser-Test/1.0";

                var mainSite = new WikiSite(wikiClient, new SiteOptions("https://anno1800.fandom.com/api.php"));
                await mainSite.Initialization;

                var page = new WikiPage(mainSite, pageToFetch);
                await page.RefreshAsync(PageQueryOptions.FetchContent);

                if (!page.Exists)
                {
                    //Page not found
                    return wikiText;
                }

                //var pageId = page.Id;//186 = overview (Buildings)

                wikiText = page.Content;//complete article/page in wikitext

                return wikiText;
            }
        }
    }
}
