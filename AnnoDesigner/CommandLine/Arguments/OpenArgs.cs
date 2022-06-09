using CommandLine;

namespace AnnoDesigner.CommandLine.Arguments
{
    [Verb("open", HelpText = "Starts AnnoDesigner with specified layout file opened")]
    public class OpenArgs : IProgramArgs
    {
        [Value(0, MetaName = "layoutPath", HelpText = "Path to layout file (*.ad).", Required = true)]
        public string Filename { get; set; }
    }
}
