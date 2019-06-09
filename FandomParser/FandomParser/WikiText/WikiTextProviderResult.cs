using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FandomParser.WikiText
{
    public class WikiTextProviderResult
    {
        public WikiTextProviderResult()
        {
            WikiText = string.Empty;
        }

        public string WikiText { get; set; }

        public int RevisionId { get; set; }

        public DateTime EditDate { get; set; }
    }
}
