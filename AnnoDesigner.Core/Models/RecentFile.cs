using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnoDesigner.Core.Models
{
    public class RecentFile
    {
        public RecentFile(string pathToUse, DateTime lastUsedToUse)
        {
            Path = pathToUse;
            LastUsed = lastUsedToUse;
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
}
