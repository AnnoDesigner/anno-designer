using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace AnnoDesigner.Core.Presets.Models
{
    [DebuggerDisplay("{" + nameof(Version) + "} - {" + nameof(Languages) + ".Count} languages")]
    public class TreeLocalizationContainer
    {
        public string Version { get; set; }

        public DateTime DateGenerated { get; set; }

        public List<LanguageContainer> Languages { get; set; }
    }
}