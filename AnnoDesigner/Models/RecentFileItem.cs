using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnnoDesigner.Core.Models;

namespace AnnoDesigner.Models
{
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
            get { return _path; }
            private set { UpdateProperty(ref _path, value); }
        }

        /// <summary>
        /// The last time the file was loaded/used.
        /// </summary>
        public DateTime LastUsed { get; set; }
    }
}
