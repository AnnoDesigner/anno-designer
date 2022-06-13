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
                yield return new Example("Export with rendered influences", new ExportArgs { Filename = @"C:\path\to\layout\file.ad", ExportedFilename = @"C:\path\to\exported\image.png", RenderInfluences = true });
                yield return new Example("Export with statistics and version information but no icons", new ExportArgs { Filename = @"C:\path\to\layout\file.ad", ExportedFilename = @"C:\path\to\exported\image.png", RenderStatistics = true, RenderVersion = true, RenderIcon = false });
            }
        }

        [Value(0, MetaName = "layoutPath", HelpText = "Path to layout file (*.ad).", Required = true)]
        public string Filename { get; set; }

        [Value(1, MetaName = "imagePath", HelpText = "Path for exported image file (*.png)", Required = true)]
        public string ExportedFilename { get; set; }

        [Option("border", Default = 1)]
        public int Border { get; set; }

        [Option("gridSize", Default = 20, HelpText = "supports zoom values between 8 and 100")]
        public int GridSize { get; set; }

        [Option("renderGrid", Default = true)]
        public bool? RenderGrid { get; set; }

        [Option("renderHarborBlockedArea", Default = false)]
        public bool? RenderHarborBlockedArea { get; set; }

        [Option("renderIcon", Default = true)]
        public bool? RenderIcon { get; set; }

        [Option("renderInfluences", Default = false)]
        public bool? RenderInfluences { get; set; }

        [Option("renderLabel", Default = true)]
        public bool? RenderLabel { get; set; }

        [Option("renderPanorama", Default = false)]
        public bool? RenderPanorama { get; set; }

        [Option("renderStatistics", Default = true)]
        public bool? RenderStatistics { get; set; }

        [Option("renderTrueInfluenceRange", Default = false)]
        public bool? RenderTrueInfluenceRange { get; set; }

        [Option("renderVersion", Default = true)]
        public bool? RenderVersion { get; set; }
    }
}
