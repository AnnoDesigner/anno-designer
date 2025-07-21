using AnnoDesigner.CommandLine.Arguments;
using System.Collections.Generic;

namespace AnnoDesigner.CommandLine;

public interface IArgumentParser
{
    IProgramArgs Parse(IEnumerable<string> arguments);
}