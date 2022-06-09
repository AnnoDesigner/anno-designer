using System;
using CommandLine.Text;

namespace AnnoDesigner.CommandLine
{
    public class HelpException : Exception
    {
        public HelpException(HelpText helpText)
        {
            HelpText = helpText;
        }

        public HelpText HelpText { get; }
    }
}
