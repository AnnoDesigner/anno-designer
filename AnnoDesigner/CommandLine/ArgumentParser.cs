using System.Collections.Generic;
using AnnoDesigner.CommandLine.Arguments;
using CommandLine;
using CommandLine.Text;

namespace AnnoDesigner.CommandLine
{
    public static class ArgumentParser
    {
        public static IProgramArgs Parse(IEnumerable<string> arguments)
        {
            var parsed = Parser.Default.ParseArguments<AdminRestartArgs, OpenArgs, ExportArgs>(arguments);
            return parsed.MapResult<AdminRestartArgs, OpenArgs, ExportArgs, IProgramArgs>(
                adminArgs => adminArgs,
                openArgs => openArgs,
                exportArgs => exportArgs,
                errors =>
                {
                    if (errors.IsHelp() || errors.IsVersion())
                    {
                        var helpText = HelpText.AutoBuild(parsed,
                            h =>
                            {
                                h.AdditionalNewLineAfterOption = false;
                                h.MaximumDisplayWidth = 100;

                                return h;
                            });

                        throw new HelpException(helpText);
                    }

                    return null;
                });
        }
    }
}
