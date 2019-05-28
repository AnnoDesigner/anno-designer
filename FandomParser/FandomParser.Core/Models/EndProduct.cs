using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FandomParser.Core.Models
{
    [DebuggerDisplay("{" + nameof(Icon) + "}")]
    [DataContract]
    public class EndProduct
    {
        [DataMember(Order = 0)]
        public double Amount { get; set; }

        [DataMember(Order = 1)]
        public double AmountElectricity { get; set; }

        [DataMember(Order = 2)]
        public string Icon { get; set; }
    }
}
