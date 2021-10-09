using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnnoDesigner.Core.Layout.Models;

namespace AnnoDesigner.Core.Models
{
    public interface IRecentFilesHelper
    {
        /*
         *  void Load();
         *  void Save();
         *  are called implicitly in ctor, AddFile and RemoveFile
         *  
         *  ctor has a pahToSettingsFile, a IRecentFilesSerializer and a IFileSystem as parameter
         */

        /// <summary>
        /// Occurs when the <see cref="RecentFiles"/> property has been updated.
        /// </summary>
        event EventHandler<EventArgs> Updated;

        /// <summary>
        /// Gets the recently used files.
        /// </summary>
        /// <value>The files.</value>
        List<RecentFile> RecentFiles { get; }

        /// <summary>
        /// Gets or sets the maximum item count.
        /// <para />
        /// The default value is <c>10</c>.
        /// </summary>
        /// <value>The maximum item count.</value>
        int MaximumItemCount { get; set; }

        /// <summary>
        /// Adds the file to the top of the list of recently used files.
        /// </summary>
        /// <param name="fileToAdd">The file.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="fileToAdd"/> is <c>null</c>.</exception>
        void AddFile(RecentFile fileToAdd);

        /// <summary>
        /// Removes the file from the list of recently used files.
        /// </summary>
        /// <param name="fileToRemove">The file.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="fileToRemove"/> is <c>null</c>.</exception>
        void RemoveFile(RecentFile fileToRemove);

        /// <summary>
        /// Clears the list of recently used files.
        /// </summary>
        void ClearRecentFiles();
    }
}
