using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnoDesigner.Core
{
    public static class CoreConstants
    {
        /// <summary>
        /// Version number of the saved file format.
        /// Will be increased every time the file format is changed.
        /// </summary>
        public const int LayoutFileVersion = 3;

        /// <summary>
        /// Filter used to retrieve the icons within the IconFolder.
        /// </summary>
        public const string IconFolderFilter = "*.png";
    }
}
