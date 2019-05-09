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
        private const string PREFIX_GLOBAL = @"Global\";
        public static readonly string MUTEX_ANNO_DESIGNER = PREFIX_GLOBAL + App.ApplicationPath.Replace(Path.DirectorySeparatorChar, '_').Replace(':', '_');
    }
}
