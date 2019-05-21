using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FandomParser
{
    [DataContract]
    public class WikiCostUnit
    {
        [DataMember(Order = 0)]
        public WikiCostUnitType Type { get; set; }

        [DataMember(Order = 1)]
        public string Name { get; set; }
    }
}
