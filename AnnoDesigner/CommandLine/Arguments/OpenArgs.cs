using CommandLine;

namespace AnnoDesigner.CommandLine.Arguments
{
    [Verb("open", HelpText = "Opens layout after AnnoDesigner starts")]
    public class OpenArgs : IProgramArgs
    {
        [Value(0, HelpText = "Input AD file", Required = true)]
        public string Filename { get; set; }
    }
}
