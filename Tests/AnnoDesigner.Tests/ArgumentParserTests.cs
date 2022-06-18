using System.Linq;
using AnnoDesigner.CommandLine;
using AnnoDesigner.CommandLine.Arguments;
using Xunit;

namespace AnnoDesigner.Tests
{
    public class ArgumentParserTests
    {
        [Fact]
        public void Parse_EmptyArguments_ShouldReturnCorrectType()
        {
            // Arrange/Act
            var parsedArguments = ArgumentParser.Parse(Enumerable.Empty<string>());

            // Assert
            Assert.IsType<EmptyArgs>(parsedArguments);
        }

        [Fact]
        public void Parse_UnknownVerb_ShouldReturnNull()
        {
            // Arrange/Act
            var parsedArguments = ArgumentParser.Parse(new[] { "unknown" });

            // Assert
            Assert.Null(parsedArguments);
        }

        [Fact]
        public void Parse_AskAdminVerb_ShouldReturnCorrectType()
        {
            // Arrange/Act
            var parsedArguments = ArgumentParser.Parse(new[] { "askAdmin" });

            // Assert
            Assert.IsType<AdminRestartArgs>(parsedArguments);
        }

        [Fact]
        public void Parse_OpenVerb_FilenameNotSpecified_ShouldReturnNull()
        {
            // Arrange/Act
            var parsedArguments = ArgumentParser.Parse(new[] { "open" });

            // Assert
            Assert.Null(parsedArguments);
        }

        [Fact]
        public void Parse_OpenVerb_ShouldReturnParsedFilename()
        {
            // Arrange/Act
            var parsedArguments = ArgumentParser.Parse(new[] { "open", "filename" });

            // Assert
            Assert.IsType<OpenArgs>(parsedArguments);
            Assert.Equal("filename", (parsedArguments as OpenArgs).Filename);
        }

        [Fact]
        public void Parse_ExportVerb_LayoutFileNotSpecified_ShouldReturnNull()
        {
            // Arrange/Act
            var parsedArguments = ArgumentParser.Parse(new[] { "export" });

            // Assert
            Assert.Null(parsedArguments);
        }

        [Fact]
        public void Parse_ExportVerb_OutputFileNotSpecified_ShouldReturnNull()
        {
            // Arrange/Act
            var parsedArguments = ArgumentParser.Parse(new[] { "export", "filename" });

            // Assert
            Assert.Null(parsedArguments);
        }

        [Fact]
        public void Parse_ExportVerb_ShouldReturnParsedValues()
        {
            // Arrange/Act
            var parsedArguments = ArgumentParser.Parse(new[] { "export", "filename", "output", "--border", "5" });

            // Assert
            Assert.IsType<ExportArgs>(parsedArguments);
            Assert.Equal("filename", (parsedArguments as ExportArgs).Filename);
            Assert.Equal("output", (parsedArguments as ExportArgs).ExportedFilename);
            Assert.Equal(5, (parsedArguments as ExportArgs).Border);
        }
    }
}
