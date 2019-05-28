using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FandomParser.Core.Presets.Models
{
    [DataContract]
    public class CostUnit
    {
        [DataMember(Order = 0)]
        public CostUnitType Type { get; set; }

        [DataMember(Order = 1)]
        public string Name { get; set; }
    }
}
