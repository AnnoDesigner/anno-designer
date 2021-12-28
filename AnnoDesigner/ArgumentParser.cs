using System;
using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace AnnoDesigner
{
    public interface IProgramArgs { }

    [Verb("askAdmin", Hidden = true)]
    public class AdminRestartArgs : IProgramArgs
    {
        public const string Arguments = "askAdmin";
    }

    [Verb("open", HelpText = "Opens layout after AnnoDesigner starts")]
    public class OpenArgs : IProgramArgs
    {
        [Value(0, HelpText = "Input AD file", Required = true)]
        public string Filename { get; set; }
    }

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

    public class HelpException : Exception
    {
        public HelpText HelpText { get; }

        public HelpException(HelpText helpText)
        {
            HelpText = helpText;
        }
    }

    public static class ArgumentParser
    {
        public static IProgramArgs Parse(IEnumerable<string> arguments)
        {
            var parsed = Parser.Default.ParseArguments<AdminRestartArgs, OpenArgs, ExportArgs>(arguments);
            return parsed.MapResult<AdminRestartArgs, OpenArgs, ExportArgs, IProgramArgs>(x => x, x => x, x => x, errors =>
            {
                if (errors.IsHelp() || errors.IsVersion())
                {
                    throw new HelpException(HelpText.AutoBuild(parsed));
                }
                return null;
            });
        }
    }
}
