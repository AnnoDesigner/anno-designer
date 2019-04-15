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
        public const int Version = 8;

        /// <summary>
        /// Version number of the saved file format.
        /// Will be increased every time the file format is changed.
        /// </summary>
        public const int FileVersion = 2;

        /// <summary>
        /// Json encoded file containing the color presets.
        /// </summary>
        public const string ColorPresetsFile = "colors.json";
        
        /// <summary>
        /// Json encoded file containing the building presets.
        /// </summary>
        public const string BuildingPresetsFile = "presetsV8.json";
        
        /// <summary>
        /// Json encoded file containing the localized names for the icons
        /// </summary>
        public const string IconNameFile = "iconsV8.json";

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
        /// Defines the margin in pixels on the right side of the layout which is used to render the statistics
        /// </summary>
        public const int StatisticsMargin = 120;

        /// <summary>
        /// The folder containing all icon files.
        /// </summary>
        public const string IconFolder = "icons-v8";

        /// <summary>
        /// Filter used to retrieve the icons within the IconFolder.
        /// </summary>
        public const string IconFolderFilter = "*.png";

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
    }
}
