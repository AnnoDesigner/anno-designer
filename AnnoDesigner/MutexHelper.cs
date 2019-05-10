using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnoDesigner
{
    public static class MutexHelper
    {
        /// <summary>
        /// If its name begins with the prefix "Global\", the mutex is visible in all terminal server sessions (e.g. different users on same machine).
        /// </summary>
        /// <remarks>https://docs.microsoft.com/en-us/dotnet/api/system.threading.mutex?view=netframework-4.8#remarks</remarks>
        private const string PREFIX_GLOBAL = @"Global\";

        /// <summary>
        /// Mutex specific for to one location of the app. So multiple instances in different directorys are possible.
        /// </summary>
        public static readonly string MUTEX_ANNO_DESIGNER = PREFIX_GLOBAL + App.ApplicationPath.Replace(Path.DirectorySeparatorChar, '_').Replace(':', '_');
    }
}
