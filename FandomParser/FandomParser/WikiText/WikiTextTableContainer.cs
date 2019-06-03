using FandomParser.WikiText;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FandomParser.WikiText
{
    [DataContract]
    public class WikiTextTableContainer
    {
        public WikiTextTableContainer()
        {
            Entries = new List<WikiTextTableEntry>();
        }

        [DataMember(Order = 0)]
        public Version Version { get; set; }

        [DataMember(Order = 1)]
        public List<WikiTextTableEntry> Entries { get; set; }
    }
}
