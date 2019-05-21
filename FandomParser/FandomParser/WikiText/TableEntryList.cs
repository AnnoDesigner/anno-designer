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
    public class TableEntryList
    {
        public TableEntryList()
        {
            Entries = new List<TableEntry>();
        }

        [DataMember(Order = 0)]
        public Version Version { get; set; }

        [DataMember(Order = 1)]
        public List<TableEntry> Entries { get; set; }
    }
}
