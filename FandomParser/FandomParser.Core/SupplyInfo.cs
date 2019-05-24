using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text;

namespace FandomParser.Core
{
    [DataContract]
    public class SupplyInfo
    {
        public SupplyInfo()
        {
            SupplyEntries = new List<SupplyEntry>();
        }

        [DataMember(Order = 0)]
        public List<SupplyEntry> SupplyEntries { get; set; }
    }
}
