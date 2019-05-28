using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FandomParser.Core.Presets.Models
{
    [DebuggerDisplay("{" + nameof(Icon) + "}")]
    [DataContract]
    public class InputProduct
    {
        [DataMember(Order = 0)]
        public double Amount { get; set; }

        [DataMember(Order = 1)]
        public double AmountElectricity { get; set; }

        [DataMember(Order = 2)]
        public string Icon { get; set; }

        public int Order { get; set; }
    }
}
