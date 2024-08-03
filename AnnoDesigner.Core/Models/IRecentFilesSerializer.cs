using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnoDesigner.Core.Models;

public interface IRecentFilesSerializer
{
    /// <summary>
    /// Serializes the list of recently used files.
    /// </summary>
    /// <param name="recentFiles">The list of recently used files.</param>
    void Serialize(List<RecentFile> recentFiles);

    /// <summary>
    /// Loads a list of recently used files.
    /// </summary>
    /// <remarks>Implementations should return an empty list when no list was serialized previously.</remarks>
    /// <returns>The list of recently used files.</returns>
    List<RecentFile> Deserialize();
}
