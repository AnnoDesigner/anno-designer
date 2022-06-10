using System.Linq;
using AnnoDesigner.CommandLine;
using AnnoDesigner.CommandLine.Arguments;
using Xunit;

namespace AnnoDesigner.Tests
{
    public class ArgumentParserTests
    {
        [Fact]
        public void Parse_EmptyArguments_ShouldReturnNull()
        {
            // Arrange/Act
            var parsedArguments = ArgumentParser.Parse(Enumerable.Empty<string>());

            // Assert
            Assert.Null(parsedArguments);
        }

        [Fact]
        public void Parse_UnknownVerb_ShouldThrow()
        {
            // Arrange/Act/Assert
            var ex = Assert.Throws<HelpException>(() => ArgumentParser.Parse(new[] { "unknown" }));
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
        public void Parse_OpenVerb_FilenameNotSpecified_ShouldThrow()
        {
            // Arrange/Act/Assert
            var ex = Assert.Throws<HelpException>(() => ArgumentParser.Parse(new[] { "open" }));
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
        public void Parse_ExportVerb_LayoutFileNotSpecified_ShouldThrow()
        {
            // Arrange/Act/Assert
            var ex = Assert.Throws<HelpException>(() => ArgumentParser.Parse(new[] { "export" }));
        }

        [Fact]
        public void Parse_ExportVerb_OutputFileNotSpecified_ShouldThrow()
        {
            // Arrange/Act/Assert
            var ex = Assert.Throws<HelpException>(() => ArgumentParser.Parse(new[] { "export", "filename" }));
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
