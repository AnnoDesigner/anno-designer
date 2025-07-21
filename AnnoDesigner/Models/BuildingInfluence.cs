using AnnoDesigner.Core.Models;
using System.Diagnostics;

namespace AnnoDesigner.Models;

[DebuggerDisplay("{" + nameof(Type) + ",nq} - {" + nameof(Name) + "}")]
public class BuildingInfluence : Notify
{
    private BuildingInfluenceType _type;
    private string _name;

    public BuildingInfluenceType Type
    {
        get => _type;
        set => UpdateProperty(ref _type, value);
    }

    public string Name
    {
        get => _name;
        set => UpdateProperty(ref _name, value);
    }
}
