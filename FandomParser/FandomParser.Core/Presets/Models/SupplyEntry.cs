using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text;

namespace FandomParser.Core.Presets.Models
{
    [DebuggerDisplay("{" + nameof(Type) + "}")]
    [DataContract]
    public class SupplyEntry
    {
        [DataMember(Order = 0)]
        public double Amount { get; set; }

        [DataMember(Order = 1)]
        public double AmountElectricity { get; set; }

        [DataMember(Order = 2)]
        public string Type { get; set; }

        public int Order { get; set; }
    }
}
