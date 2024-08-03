using System.Collections.Generic;
using System.Diagnostics;

namespace AnnoDesigner.Core.Presets.Models;

[DebuggerDisplay("{" + nameof(LanguageCode) + "} - {" + nameof(Translations) + ".Count} translations")]
public class LanguageContainer
{
    public string LanguageCode { get; set; }

    public string Language { get; set; }

    public List<Translation> Translations { get; set; }
}