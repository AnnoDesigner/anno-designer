using CommandLine;

namespace AnnoDesigner.CommandLine.Arguments
{
    [Verb("askAdmin", Hidden = true)]
    public class AdminRestartArgs : IProgramArgs
    {
        public const string Arguments = "askAdmin";
    }
}
