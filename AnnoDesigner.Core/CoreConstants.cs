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
        /// Json encoded file containing the color presets.
        /// </summary>
        public const string ColorPresetsFile = "colors.json";

        /// <summary>
        /// Json encoded file containing the building presets.
        /// </summary>
        public const string BuildingPresetsFile = "presets.json";

        /// <summary>
        /// Json encoded file containing the localized names for the icons
        /// </summary>
        public const string IconNameFile = "icons.json";

        /// <summary>
        /// Filter used to retrieve the icons within the IconFolder.
        /// </summary>
        public const string IconFolderFilter = "*.png";

        [Flags]
        public enum GameVersion
        {
            Unknown = 0,
            Anno1404 = 1 << 0,
            Anno2070 = 1 << 1,
            Anno2205 = 1 << 2,
            Anno1800 = 1 << 3,
            //All = Anno1404 | Anno2070 | Anno2205 | Anno1800
            All = ~Unknown//https://stackoverflow.com/a/8488314
        }
    }
}
