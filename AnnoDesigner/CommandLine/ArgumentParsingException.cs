using System;
using CommandLine.Text;

namespace AnnoDesigner.CommandLine
{
    public class ArgumentParsingException : Exception
    {
        public ArgumentParsingException(HelpText helpText)
        {
            HelpText = helpText;
        }

        public string HelpText { get; }
    }
}
