using CommandLine;

namespace AnnoDesigner.CommandLine.Arguments
{
    [Verb("export", HelpText = "Exports layout to image and exits")]
    public class ExportArgs : IProgramArgs
    {
        [Value(0, HelpText = "Input AD file", Required = true)]
        public string Filename { get; set; }

        [Value(1, HelpText = "Exported png file", Required = true)]
        public string ExportedFilename { get; set; }

        [Option("border", Default = 1)]
        public int Border { get; set; }

        [Option("gridSize", Default = 20)]
        public int GridSize { get; set; }

        [Option("renderGrid", Default = true)]
        public bool RenderGrid { get; set; }

        [Option("renderHarborBlockedArea")]
        public bool RenderHarborBlockedArea { get; set; }

        [Option("renderIcon", Default = true)]
        public bool RenderIcon { get; set; }

        [Option("renderInfluences")]
        public bool RenderInfluences { get; set; }

        [Option("renderLabel", Default = true)]
        public bool RenderLabel { get; set; }

        [Option("renderPanorama")]
        public bool RenderPanorama { get; set; }

        [Option("renderStatistics")]
        public bool RenderStatistics { get; set; }

        [Option("renderTrueInfluenceRange")]
        public bool RenderTrueInfluenceRange { get; set; }

        [Option("renderVersion")]
        public bool RenderVersion { get; set; }
    }
}
