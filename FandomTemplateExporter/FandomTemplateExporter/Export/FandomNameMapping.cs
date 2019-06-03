using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FandomTemplateExporter.Export
{
    [DataContract]
    [DebuggerDisplay("{" + nameof(FandomName) + "}")]
    public class FandomNameMapping
    {
        [DataMember(Order = 0)]
        public string FandomName { get; set; }

        [DataMember(Order = 1)]
        public List<string> Identifiers { get; set; }
    }
}
