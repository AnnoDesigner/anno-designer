using AnnoDesigner.Core.Models;
using System;
using System.Diagnostics;

namespace AnnoDesigner.Models;

[DebuggerDisplay("{" + nameof(Path) + ",nq}")]
public class RecentFileItem : Notify
{
    private string _path;

    public RecentFileItem(string pathToUse)
    {
        Path = pathToUse;
    }

    public string Path
    {
        get => _path;
        private set => UpdateProperty(ref _path, value);
    }

    /// <summary>
    /// The last time the file was loaded/used.
    /// </summary>
    public DateTime LastUsed { get; set; }
}
