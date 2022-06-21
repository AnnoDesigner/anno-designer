using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO.Abstractions;

namespace AnnoDesigner.CommandLine
{
    /// <summary>
    /// Provides extension methods for <see cref="Argument" />.
    /// </summary>
    public static class ArgumentExtensions
    {
        /// <summary>
        /// Configures an argument to accept only values corresponding to an existing file.
        /// </summary>
        /// <param name="argument">The argument to configure.</param>
        /// <param name="fileSystem">The <see cref="IFileSystem"/> to use.</param>
        /// <returns>The configured argument.</returns>
        public static Argument<IFileInfo> ExistingOnly(this Argument<IFileInfo> argument, IFileSystem fileSystem)
        {
            argument.AddValidator(x => FileExists(x, fileSystem));
            return argument;
        }

        /// <summary>
        /// Configures an argument to accept only values corresponding to an existing directory.
        /// </summary>
        /// <param name="argument">The argument to configure.</param>
        /// <param name="fileSystem">The <see cref="IFileSystem"/> to use.</param>
        /// <returns>The configured argument.</returns>
        public static Argument<IDirectoryInfo> ExistingOnly(this Argument<IDirectoryInfo> argument, IFileSystem fileSystem)
        {
            argument.AddValidator(x => DirectoryExists(x, fileSystem));
            return argument;
        }

        private static void FileExists(ArgumentResult result, IFileSystem fileSystem)
        {
            for (var i = 0; i < result.Tokens.Count; i++)
            {
                var token = result.Tokens[i];

                if (!fileSystem.File.Exists(token.Value))
                {
                    result.ErrorMessage = result.LocalizationResources.FileDoesNotExist(token.Value);
                    return;
                }
            }
        }

        private static void DirectoryExists(ArgumentResult result, IFileSystem fileSystem)
        {
            for (var i = 0; i < result.Tokens.Count; i++)
            {
                var token = result.Tokens[i];

                if (!fileSystem.Directory.Exists(token.Value))
                {
                    result.ErrorMessage = result.LocalizationResources.DirectoryDoesNotExist(token.Value);
                    return;
                }
            }
        }
    }
}
