using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using AnnoDesigner.CommandLine.Arguments;
using System.CommandLine.Parsing;

namespace AnnoDesigner.CommandLine
{
    public static class ArgumentParser
    {
        public static IProgramArgs Parse(IEnumerable<string> arguments)
        {
            IProgramArgs parsedArgs = null;

            void StoreParsedArgs(IProgramArgs args) => parsedArgs = args;

            var root = new RootCommand
            {
                new OpenArgs.Binder().ConfigureCommand(StoreParsedArgs),
                new ExportArgs.Binder().ConfigureCommand(StoreParsedArgs),
                new AdminRestartArgs.Binder().ConfigureCommand(StoreParsedArgs)
            };
            root.SetHandler(() => parsedArgs = new EmptyArgs());

            root.Invoke(arguments.ToArray(), new ConsoleManager.LazyConsole());
            return parsedArgs;
        }
    }
}
