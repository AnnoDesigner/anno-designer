using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace FandomParser.Core
{
    [DataContract]
    public class UnlockInfo
    {
        public UnlockInfo()
        {
            UnlockConditions = new List<UnlockCondition>();
        }

        [DataMember(Order = 0)]
        public List<UnlockCondition> UnlockConditions { get; set; }
    }
}
