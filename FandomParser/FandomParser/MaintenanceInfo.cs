using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FandomParser
{
    [DataContract]
    public class MaintenanceInfo
    {
        [DataMember(Order = 0)]
        public double Value { get; set; }

        [DataMember(Order = 1)]
        public WikiCostUnit Unit { get; set; }
    }
}
