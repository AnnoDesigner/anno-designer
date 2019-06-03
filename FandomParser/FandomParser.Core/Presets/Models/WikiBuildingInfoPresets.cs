using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;

namespace FandomParser.Core.Presets.Models
{
    [DebuggerDisplay("{" + nameof(Version) + "}")]
    [DataContract]
    public class WikiBuildingInfoPresets
    {
        public WikiBuildingInfoPresets()
        {
            Infos = new List<WikiBuildingInfo>();
        }

        [DataMember(Order = 0)]
        public Version Version { get; set; }

        [DataMember(Order = 1, Name = "DateGenerated")]
        public string FormattedDateGenerated { get; set; }

        [IgnoreDataMember]
        public DateTime DateGenerated
        {
            //"o" -> round-trippable format which is ISO-8601-compatible.
            get { return DateTime.ParseExact(FormattedDateGenerated, "o", CultureInfo.InvariantCulture); }
            set { FormattedDateGenerated = value.ToString("o"); }
        }

        [DataMember(Order = 2)]
        public List<WikiBuildingInfo> Infos { get; set; }
    }
}
