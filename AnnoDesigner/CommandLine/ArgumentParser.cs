﻿using System;
using System.Collections.Generic;
using System.Linq;
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
                    errors = errors.Where(e => e.Tag != ErrorType.NoVerbSelectedError);
                    if (!errors.Any())
                    {
                        return new EmptyArgs();
                    }

                    var helpText = HelpText.AutoBuild(parsed,
                        h =>
                        {
                            h.AdditionalNewLineAfterOption = false;
                            h.MaximumDisplayWidth = 100;

                            return h;
                        });

                    throw new ArgumentParsingException(helpText);
                });
        }
    }
}
