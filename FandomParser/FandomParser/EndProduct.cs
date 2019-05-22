using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FandomParser
{
    [DataContract]
    public class EndProduct
    {
        [DataMember(Order = 0)]
        public double Amount { get; set; }

        [DataMember(Order = 1)]
        public string Icon { get; set; }

        [DataMember(Order = 2)]
        public double AmountElectricity { get; set; }
    }
}
