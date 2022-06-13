using CommandLine;

namespace AnnoDesigner.CommandLine.Arguments
{
    [Verb("empty", isDefault: true, Hidden = true)]
    public class EmptyArgs : IProgramArgs { }
}
