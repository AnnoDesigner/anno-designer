using System;
using CommandLine.Text;

namespace AnnoDesigner.CommandLine
{
    public class HelpException : Exception
    {
        public HelpText HelpText { get; }

        public HelpException(HelpText helpText)
        {
            HelpText = helpText;
        }
    }
}
