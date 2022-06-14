using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace AnnoDesigner.CommandLine.Arguments
{
    [Verb("export", HelpText = "Exports the specified layout file to an image and closes immediately")]
    public class ExportArgs : IProgramArgs
    {
        [Usage]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("Export with default settings", new ExportArgs { Filename = @"C:\path\to\layout\file.ad", ExportedFilename = @"C:\path\to\exported\image.png" });
                yield return new Example("Export with current user settings", new ExportArgs { Filename = @"C:\path\to\layout\file.ad", ExportedFilename = @"C:\path\to\exported\image.png", UseUserSettings = true });
                yield return new Example("Export with rendered influences", new ExportArgs { Filename = @"C:\path\to\layout\file.ad", ExportedFilename = @"C:\path\to\exported\image.png", RenderInfluences = true });
                yield return new Example("Export with statistics and version information but no icons", new ExportArgs { Filename = @"C:\path\to\layout\file.ad", ExportedFilename = @"C:\path\to\exported\image.png", HideIcon = true });
            }
        }

        [Value(0, MetaName = "layoutPath", HelpText = "Path to layout file (*.ad).", Required = true)]
        public string Filename { get; set; }

        [Value(1, MetaName = "imagePath", HelpText = "Path for exported image file (*.png)", Required = true)]
        public string ExportedFilename { get; set; }

        [Option("useUserSettings", HelpText = "Uses render settings of current user")]
        public bool UseUserSettings { get; set; }

        [Option("border", Default = 1)]
        public int Border { get; set; }

        [Option("gridSize", Default = 20, HelpText = "Supports zoom values between 8 and 100.")]
        public int GridSize { get; set; }

        [Option("hideGrid")]
        public bool HideGrid { get; set; }

        [Option("renderHarborBlockedArea")]
        public bool RenderHarborBlockedArea { get; set; }

        [Option("hideIcon")]
        public bool HideIcon { get; set; }

        [Option("renderInfluences")]
        public bool RenderInfluences { get; set; }

        [Option("hideLabel")]
        public bool HideLabel { get; set; }

        [Option("renderPanorama")]
        public bool RenderPanorama { get; set; }

        [Option("hideStatistics")]
        public bool HideStatistics { get; set; }

        [Option("renderTrueInfluenceRange")]
        public bool RenderTrueInfluenceRange { get; set; }

        [Option("hideVersion")]
        public bool HideVersion { get; set; }
    }
}
