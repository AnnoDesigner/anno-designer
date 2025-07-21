using AnnoDesigner.Core.Models;
using System.Diagnostics;

namespace AnnoDesigner.Models;

[DebuggerDisplay("{" + nameof(Name) + ",nq} ({" + nameof(IsSelected) + "})")]
public class SupportedLanguage : Notify
{
    private string _name;
    private string _flagPath;
    private bool _isSelected;

    public SupportedLanguage(string nameToUse)
    {
        Name = nameToUse;
    }

    public string Name
    {
        get => _name;
        private set => UpdateProperty(ref _name, value);
    }

    public string FlagPath
    {
        get => _flagPath;
        set => UpdateProperty(ref _flagPath, value);
    }

    public bool IsSelected
    {
        get => _isSelected;
        set => UpdateProperty(ref _isSelected, value);
    }
}


