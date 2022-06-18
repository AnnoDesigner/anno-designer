using System.CommandLine;
using System.CommandLine.Binding;

namespace AnnoDesigner.CommandLine.Arguments
{
    public class ExportArgs : IProgramArgs
    {
        public class Binder : ArgsBinderBase<ExportArgs>
        {
            private readonly Argument<string> layoutFilepath;
            private readonly Argument<string> imageFilepath;

            private readonly Option<int> border;
            private readonly Option<int> gridSize;

            private readonly Option<bool> useUserSettings;
            private readonly Option<bool?> renderGrid;
            private readonly Option<bool?> renderHarborBlockedArea;
            private readonly Option<bool?> renderIcon;
            private readonly Option<bool?> renderInfluences;
            private readonly Option<bool?> renderLabel;
            private readonly Option<bool?> renderPanorama;
            private readonly Option<bool?> renderStatistics;
            private readonly Option<bool?> renderTrueInfluenceRange;
            private readonly Option<bool?> renderVersion;

            public Binder()
            {
                layoutFilepath = new("layoutPath", "Path to layout file (*.ad)");
                imageFilepath = new("imagePath", "Path for exported image file (*.png)");

                border = new("--border", () => 1);
                gridSize = new("--gridSize", () => 20, $"Grid size in pixelsc, can range from {Constants.GridStepMin} to {Constants.GridStepMax}");
                gridSize.AddValidator(parseResults =>
                {
                    var value = parseResults.GetValueOrDefault<int>();
                    if (value < Constants.GridStepMin || value > Constants.GridStepMax)
                    {
                        parseResults.ErrorMessage = $"{gridSize.Name} must be between {Constants.GridStepMin} and {Constants.GridStepMax}";
                    }
                });

                useUserSettings = new("--useUserSettings", "Use render settings of current user");
                renderGrid = new("--renderGrid", $"Defaults to true if not specified and {useUserSettings.Name} is not set");
                renderHarborBlockedArea = new("--renderHarborBlockedArea");
                renderIcon = new("--renderIcon", $"Defaults to true if not specified and {useUserSettings.Name} is not set");
                renderInfluences = new("--renderInfluences");
                renderLabel = new("--renderLabel", $"Defaults to true if not specified and {useUserSettings.Name} is not set");
                renderPanorama = new("--renderPanorama");
                renderStatistics = new("--renderStatistics", $"Defaults to true if not specified and {useUserSettings.Name} is not set");
                renderTrueInfluenceRange = new("--renderTrueInfluenceRange");
                renderVersion = new("--renderVersion", $"Defaults to true if not specified and {useUserSettings.Name} is not set");

                command = new Command("export", "Exports the specified layout file to an image and closes immediately")
                {
                    layoutFilepath,
                    imageFilepath,
                    border,
                    gridSize,
                    useUserSettings,
                    renderGrid,
                    renderHarborBlockedArea,
                    renderIcon,
                    renderInfluences,
                    renderLabel,
                    renderPanorama,
                    renderStatistics,
                    renderTrueInfluenceRange,
                    renderVersion
                };
            }

            protected override ExportArgs GetBoundValue(BindingContext bindingContext)
            {
                return new ExportArgs()
                {
                    Filename = bindingContext.ParseResult.GetValueForArgument(layoutFilepath),
                    ExportedFilename = bindingContext.ParseResult.GetValueForArgument(imageFilepath),
                    Border = bindingContext.ParseResult.GetValueForOption(border),
                    GridSize = bindingContext.ParseResult.GetValueForOption(gridSize),
                    UseUserSettings = bindingContext.ParseResult.GetValueForOption(useUserSettings),
                    RenderGrid = bindingContext.ParseResult.GetValueForOption(renderGrid),
                    RenderHarborBlockedArea = bindingContext.ParseResult.GetValueForOption(renderHarborBlockedArea),
                    RenderIcon = bindingContext.ParseResult.GetValueForOption(renderIcon),
                    RenderInfluences = bindingContext.ParseResult.GetValueForOption(renderInfluences),
                    RenderLabel = bindingContext.ParseResult.GetValueForOption(renderLabel),
                    RenderPanorama = bindingContext.ParseResult.GetValueForOption(renderPanorama),
                    RenderStatistics = bindingContext.ParseResult.GetValueForOption(renderStatistics),
                    RenderTrueInfluenceRange = bindingContext.ParseResult.GetValueForOption(renderTrueInfluenceRange),
                    RenderVersion = bindingContext.ParseResult.GetValueForOption(renderVersion),

                };
            }
        }

        public string Filename { get; set; }
        public string ExportedFilename { get; set; }
        public int Border { get; set; }
        public int GridSize { get; set; }
        public bool UseUserSettings { get; set; }
        public bool? RenderGrid { get; set; }
        public bool? RenderHarborBlockedArea { get; set; }
        public bool? RenderIcon { get; set; }
        public bool? RenderInfluences { get; set; }
        public bool? RenderLabel { get; set; }
        public bool? RenderPanorama { get; set; }
        public bool? RenderStatistics { get; set; }
        public bool? RenderTrueInfluenceRange { get; set; }
        public bool? RenderVersion { get; set; }
    }
}
