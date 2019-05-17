using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AnnoDesigner.Export
{
    [DataContract]
    [DebuggerDisplay("{" + nameof(Version) + "}")]
    public class FandomNameMappingPresets
    {
        [DataMember(Order = 0)]
        public string Version { get; set; }

        [DataMember(Order = 1)]
        public List<FandomNameMapping> Names { get; set; }
    }
}
