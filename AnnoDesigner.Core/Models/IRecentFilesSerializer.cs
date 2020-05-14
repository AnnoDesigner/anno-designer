using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnoDesigner.Core.Models
{
    public interface IRecentFilesSerializer
    {
        /*
         * Implementations could serialize to XML or JSON or SQLite or ...
         * I would prefer JSON in the "AppData\Local\AnnoDesigner" directory. The app-settings are already there.
         */

        /// <summary>
        /// Serializes the list of recently used files to a file.
        /// </summary>
        /// <param name="path">The path to the file with the recently used files.</param>
        /// <param name="recentFiles">The list of recently used files.</param>
        void Serialize(string path, List<RecentFile> recentFiles);

        /// <summary>
        /// Loads a list of recently used files from a file.
        /// </summary>
        /// <param name="path">The path to the file with the recently used files.</param>
        /// <returns>The list of recently used files.</returns>
        List<RecentFile> Deserialize(string path);
    }
}
