using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AnnoDesigner.Core.Presets.Models
{
    [DebuggerDisplay("{" + nameof(Version) + "}")]
    [DataContract]
    public class IconMappingPresets
    {
        public IconMappingPresets()
        {
            Version = string.Empty;
            IconNameMappings = new List<IconNameMap>();
        }

        [DataMember(Order = 0)]
        public string Version { get; set; }

        [DataMember(Order = 1)]
        public List<IconNameMap> IconNameMappings { get; set; }
    }
}
