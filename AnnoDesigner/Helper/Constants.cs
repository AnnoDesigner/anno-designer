using System;
using System.Windows.Media.Imaging;

namespace AnnoDesigner.Helper;

/// <summary>
/// Contains application wide constants
/// </summary>
public static class Constants
{
    /// <summary>
    /// Version number of the application.
    /// Will be increased with each release.
    /// </summary>
    /// <remarks>To support old(er) versions of the app, there should be an agreement to just use major.minor for the version and just increment the minor value.
    /// At least for some time.
    /// This limits the possible versions to 255.255.</remarks>
    public static readonly Version Version = new(9, 4);

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
    /// The minimum value for zoom sensitivity. A larger value means a less sensitive minimum zoom.
    /// </summary>
    public const double ZoomSensitivityMinimum = 5;

    /// <summary>
    /// The chosen zoom sensitivity percentage is inverted then multiplied by this value.
    /// </summary>
    /// <example>
    /// ZoomSensitivityPercentage is set to 1(%). Calculated value would be 100*<see cref="ZoomSensitivityCoefficient"/>.
    /// ZoomSensitivityPercentage is set to 100(%). Calculated value would be 1*<see cref="ZoomSensitivityCoefficient"/>.
    /// </example>
    public const double ZoomSensitivityCoefficient = 1.1;

    /// <summary>
    /// Maximum value for the zoom sensitivity slider. A double value is required for databinding directly to the inherited 
    /// <see cref="System.Windows.Controls.Primitives.RangeBase.Maximum"/> property on a <see cref="System.Windows.Controls.Slider"/>.
    /// </summary>
    public const double ZoomSensitivitySliderMaximum = 100d;

    /// <summary>
    /// The default zoom sensitivity value.
    /// </summary>
    public const double ZoomSensitivityPercentageDefault = 50d;

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
    /// The default number of recent files to show.
    /// </summary>
    public const int MaxRecentFiles = 10;

    /// <summary>
    /// Key of registry used to specify command to open AD layout.
    /// </summary>
    public const string FileAssociationRegistryKey = @"HKEY_CURRENT_USER\Software\Classes\anno_designer\shell\open\command";
}
