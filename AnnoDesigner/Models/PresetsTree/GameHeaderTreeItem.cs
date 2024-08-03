using System.Diagnostics;
using static AnnoDesigner.Core.CoreConstants;

namespace AnnoDesigner.Models.PresetsTree;

[DebuggerDisplay("{" + nameof(Header) + ",nq}")]
public class GameHeaderTreeItem : GenericTreeItem
{
    private GameVersion _gameVersion;

    public GameHeaderTreeItem() : base(null)
    {
        GameVersion = GameVersion.Unknown;
    }

    public GameVersion GameVersion
    {
        get => _gameVersion;
        set => UpdateProperty(ref _gameVersion, value);
    }
}
