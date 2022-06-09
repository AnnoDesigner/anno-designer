using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace AnnoDesigner.CommandLine.Arguments
{
    [Verb("open", HelpText = "Starts AnnoDesigner with specified layout file opened")]
    public class OpenArgs : IProgramArgs
    {
        [Usage]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("Open with layout", new OpenArgs { Filename = @"C:\path\to\layout\file.ad" });
            }
        }

        [Value(0, MetaName = "layoutPath", HelpText = "Path to layout file (*.ad).", Required = true)]
        public string Filename { get; set; }
    }
}
