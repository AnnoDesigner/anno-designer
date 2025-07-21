using AnnoDesigner.CommandLine.Arguments;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO.Abstractions;
using System.Linq;

namespace AnnoDesigner.CommandLine;

public class ArgumentParser : IArgumentParser
{
    private readonly IConsole _console;
    private readonly IFileSystem _fileSystem;

    public ArgumentParser(IConsole consoleToUse, IFileSystem fileSystemToUse)
    {
        _console = consoleToUse;
        _fileSystem = fileSystemToUse;
    }

    public IProgramArgs Parse(IEnumerable<string> arguments)
    {
        IProgramArgs parsedArgs = null;

        void StoreParsedArgs(IProgramArgs args)
        {
            parsedArgs = args;
        }

        RootCommand root =
        [
            new OpenArgs.Binder(_fileSystem).ConfigureCommand(StoreParsedArgs),
            new ExportArgs.Binder(_fileSystem).ConfigureCommand(StoreParsedArgs),
            new AdminRestartArgs.Binder().ConfigureCommand(StoreParsedArgs)
        ];
        root.SetHandler(() => parsedArgs = new EmptyArgs());

        _ = root.Invoke(arguments.ToArray(), _console);

        return parsedArgs;
    }
}
