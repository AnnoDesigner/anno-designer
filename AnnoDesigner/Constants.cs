using System;
using System.Windows.Media.Imaging;

namespace AnnoDesigner
{
    /// <summary>
    /// Contains application wide constants
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Version number of the application.
        /// Will be increased with each release.
        /// </summary>
        public const double Version = 8.2;

        /// <summary>
        /// Prefix for temporary updated presets file.
        /// </summary>
        public const string PrefixTempBuildingPresetsFile = "temp_";

        /// <summary>
        /// Json encoded file containing the localized names for the icons
        /// </summary>
        public const string IconNameFile = "icons.json";

        /// <summary>
        /// Json encoded file containing the fandom name presets.
        /// </summary>
        public const string FandomNameMappingFile = "fandomNameMapping.json";

        /// <summary>
        /// The minimal grid size to which the user can zoom out.
        /// </summary>
        public const int GridStepMin = 8;

        /// <summary>
        /// The maximum grid size to which the user can zoom in.
        /// </summary>
        public const int GridStepMax = 100;

        /// <summary>
        /// The default grid size.
        /// </summary>
        public const int GridStepDefault = 20;

        /// <summary>
        /// The value that affects the rendering sizes of icons on the anno canvas. 1 produces an icon of the biggest size.
        /// </summary>
        public const double IconSizeFactor = 1.1;

        /// <summary>
        /// The folder containing all icon files.
        /// </summary>
        public const string IconFolder = "icons";

        /// <summary>
        /// File extension used for saved layouts.
        /// </summary>
        public const string SavedLayoutExtension = ".ad";

        /// <summary>
        /// Filter used for the open, save and save as dialogs.
        /// </summary>
        public const string SaveOpenDialogFilter = "Anno Designer Files (*.ad)|*.ad|All Files (*.*)|*.*";

        /// <summary>
        /// BitmapEncoder used for encoding exported images.
        /// </summary>
        /// <returns></returns>
        public static Func<BitmapEncoder> GetExportImageEncoder = () => new PngBitmapEncoder();

        /// <summary>
        /// File extension used for exported images. Should correspong to the used encoding.
        /// </summary>
        public const string ExportedImageExtension = ".png";

        /// <summary>
        /// Filter used for the export image dialog.
        /// </summary>
        public const string ExportDialogFilter = "PNG (*.png)|*.png|All Files (*.*)|*.*";

        /// <summary>
        /// Used to provide space for the statistics panel when exporting a layout as an image.
        /// </summary>
        public const int StatisticsMargin = 142;

        /// <summary>
        /// Used to prevent endless loop on updates.
        /// </summary>
        public const string Argument_Ask_For_Admin = "-askAdmin";
    }
}
