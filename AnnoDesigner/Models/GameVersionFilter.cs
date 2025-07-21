using AnnoDesigner.Core.Models;
using System.Diagnostics;
using static AnnoDesigner.Core.CoreConstants;

namespace AnnoDesigner.Models;

[DebuggerDisplay("{" + nameof(Name) + ",nq} ({" + nameof(IsSelected) + "})")]
public class GameVersionFilter : Notify
{
    private GameVersion _gameVersion;
    private string _name;
    private bool _isSelected;
    private int _order;

    public GameVersion Type
    {
        get => _gameVersion;
        set => UpdateProperty(ref _gameVersion, value);
    }

    public string Name
    {
        get => _name;
        set => UpdateProperty(ref _name, value);
    }

    public bool IsSelected
    {
        get => _isSelected;
        set => UpdateProperty(ref _isSelected, value);
    }

    public int Order
    {
        get => _order;
        set => UpdateProperty(ref _order, value);
    }
}
