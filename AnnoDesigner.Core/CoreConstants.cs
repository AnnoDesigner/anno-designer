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

        /// <summary>
        /// Prefix for temporary updated presets file.
        /// </summary>
        public const string PrefixUpdatedPresetsFile = "temp_";

        public static class PresetsFiles
        {
            /// <summary>
            /// Json encoded file containing the color presets.
            /// </summary>
            public const string ColorPresetsFile = "colors.json";

            /// <summary>
            /// Json encoded file containing the building presets.
            /// </summary>
            public const string BuildingPresetsFile = "presets.json";

            /// <summary>
            /// Json encoded file containing the localized names for the icons.
            /// </summary>
            public const string IconNameFile = "icons.json";

            /// <summary>
            /// Json encoded file containing the detailed informations for building presets.
            /// </summary>
            public const string WikiBuildingInfoPresetsFile = "wikiBuildingInfo.json";
        }
    }
}
