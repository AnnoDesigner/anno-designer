using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnoDesigner.Core.Models;

[DebuggerDisplay("{" + nameof(Path) + ",nq}")]
public class RecentFile
{
    public RecentFile(string path, DateTime lastUsed)
    {
        Path = path;
        LastUsed = lastUsed;
    }

    /// <summary>
    /// The path to the file.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// The last time the file was loaded/used.
    /// </summary>
    public DateTime LastUsed { get; set; }
}
