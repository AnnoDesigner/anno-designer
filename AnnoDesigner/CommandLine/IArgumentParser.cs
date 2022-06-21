using System.Collections.Generic;
using AnnoDesigner.CommandLine.Arguments;

namespace AnnoDesigner.CommandLine
{
    public interface IArgumentParser
    {
        IProgramArgs Parse(IEnumerable<string> arguments);
    }
}