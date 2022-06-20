using System;
using System.CommandLine;
using System.CommandLine.Binding;
using System.IO.Abstractions;
using System.Linq;

namespace AnnoDesigner.CommandLine.Arguments
{
    public class OpenArgs : IProgramArgs
    {
        public class Binder : ArgsBinderBase<OpenArgs>
        {
            private readonly Argument<IFileInfo> argumentFilePath;

            public Binder(IFileSystem fileSystem)
            {
                argumentFilePath = new Argument<IFileInfo>("layoutPath",
                    parse: arg => fileSystem.FileInfo.FromFileName(arg.Tokens.First().Value),
                    description: "Path to layout file (*.ad)")
                    .ExistingOnly(fileSystem);

                argumentFilePath.AddValidator(parseResults =>
                {
                    var parsedFileinfo = parseResults.GetValueOrDefault<IFileInfo>();
                    if (!string.Equals(parsedFileinfo.Extension, Constants.SavedLayoutExtension, StringComparison.OrdinalIgnoreCase))
                    {
                        parseResults.ErrorMessage = $"File \"{parsedFileinfo.Name}\" must have extension \"{Constants.SavedLayoutExtension}\".";
                    }
                });

                command = new Command("open", "Starts AnnoDesigner with specified layout file opened")
                {
                    argumentFilePath
                };
            }

            protected override OpenArgs GetBoundValue(BindingContext bindingContext)
            {
                var parsedFileInfo = bindingContext.ParseResult.GetValueForArgument(argumentFilePath);

                return new OpenArgs()
                {
                    FilePath = parsedFileInfo.FullName
                };
            }
        }

        public string FilePath { get; set; }
    }
}
