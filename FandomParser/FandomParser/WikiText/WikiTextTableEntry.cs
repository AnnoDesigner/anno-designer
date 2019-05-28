using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using FandomParser.Core.Presets.Models;

namespace FandomParser.WikiText
{

    [DataContract]
    public class WikiTextTableEntry
    {
        [DataMember(Order = 0)]
        public string Name { get; set; }

        [DataMember(Order = 1)]
        public string Icon { get; set; }

        [DataMember(Order = 2)]
        public string Description { get; set; }

        [DataMember(Order = 3)]
        public WorldRegion Region { get; set; }

        [DataMember(Order = 4)]
        public string Tier { get; set; }

        [DataMember(Order = 5)]
        public string ConstructionCost { get; set; }

        [DataMember(Order = 6)]
        public string MaintenanceCost { get; set; }

        [DataMember(Order = 7)]
        public string Size { get; set; }
    }
}
