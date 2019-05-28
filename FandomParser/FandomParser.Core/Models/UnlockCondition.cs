using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text;

namespace FandomParser.Core.Models
{
    [DebuggerDisplay("{" + nameof(Type) + "}")]
    [DataContract]
    public class UnlockCondition
    {
        [DataMember(Order = 0)]
        public double Amount { get; set; }

        [DataMember(Order = 1)]
        public string Type { get; set; }

        public int Order { get; set; }
    }
}
