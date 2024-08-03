using AnnoDesigner.Core.Models;
using System.Diagnostics;

namespace AnnoDesigner.Models;

[DebuggerDisplay("{" + nameof(Count) + ",nq} x {" + nameof(Name) + "}")]
public class StatisticsBuilding : Notify
{
    private int _count;
    private string _name;

    public int Count
    {
        get => _count;
        set => UpdateProperty(ref _count, value);
    }

    public string Name
    {
        get => _name;
        set => UpdateProperty(ref _name, value);
    }
}
