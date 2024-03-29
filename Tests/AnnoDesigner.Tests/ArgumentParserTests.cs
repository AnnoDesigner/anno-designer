﻿using System.Collections.Generic;
using System.CommandLine;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using AnnoDesigner.CommandLine;
using AnnoDesigner.CommandLine.Arguments;
using Xunit;
using Xunit.Abstractions;

namespace AnnoDesigner.Tests
{
    public class ArgumentParserTests
    {
        private readonly MockFileSystem _fileSystem;
        private readonly IConsole _console;
        private readonly ITestOutputHelper _output;
        private readonly string _rootDirectory;
        private readonly string _pathToLayoutFile;
        private readonly string _pathToExportedFile;

        public ArgumentParserTests(ITestOutputHelper output)
        {
            _output = output;
            _fileSystem = new MockFileSystem();
            _console = new MockedConsole(output);

            _rootDirectory = @"A:\";

            _pathToLayoutFile = $@"{_rootDirectory}sub directory\layout.ad";
            _pathToExportedFile = $@"{_rootDirectory}directory for exports\exported.png";
        }

        #region helper methods

        private IArgumentParser GetParser()
        {
            return new ArgumentParser(_console, _fileSystem);
        }

        private void InitFileSystem()
        {
            var filePaths = new List<string>
            {
                _pathToLayoutFile,
                _pathToExportedFile
            };

            foreach (var curFilePath in filePaths)
            {
                _fileSystem.AddFile(curFilePath, new MockFileData(string.Empty));
            }
        }

        #endregion

        #region Option: none or unknown

        [Fact]
        public void Parse_EmptyArguments_ShouldReturnCorrectType()
        {
            // Arrange/Act
            var parsedArguments = GetParser().Parse(Enumerable.Empty<string>());

            // Assert
            Assert.IsType<EmptyArgs>(parsedArguments);
        }

        [Fact]
        public void Parse_UnknownVerb_ShouldReturnNull()
        {
            // Arrange/Act
            var parsedArguments = GetParser().Parse(new[] { "unknown" });

            // Assert
            Assert.Null(parsedArguments);
        }

        #endregion

        #region Option: askAdmin

        [Fact]
        public void Parse_AskAdminVerb_ShouldReturnCorrectType()
        {
            // Arrange/Act
            var parsedArguments = GetParser().Parse(new[] { "askAdmin" });

            // Assert
            Assert.IsType<AdminRestartArgs>(parsedArguments);
        }

        #endregion

        #region Option: open

        [Fact]
        public void Parse_OpenVerb_FilenameNotSpecified_ShouldReturnNull()
        {
            // Arrange/Act
            var parsedArguments = GetParser().Parse(new[] { "open" });

            // Assert
            Assert.Null(parsedArguments);
        }

        [Fact]
        public void Parse_OpenVerb_NotExistingFilenameSpecified_ShouldReturnNull()
        {
            // Arrange
            InitFileSystem();

            // Act
            var parsedArguments = GetParser().Parse(new[] { "open", "notExistingFile" });

            // Assert
            Assert.Null(parsedArguments);
        }

        [Fact]
        public void Parse_OpenVerb_ShouldReturnParsedFilename()
        {
            // Arrange
            InitFileSystem();

            // Act
            var parsedArguments = GetParser().Parse(new[] { "open", _pathToLayoutFile });

            // Assert
            var openArgs = Assert.IsType<OpenArgs>(parsedArguments);
            Assert.Equal(_pathToLayoutFile, openArgs.FilePath);
        }

        [Fact]
        public void Parse_OpenVerb_WrongFileExtension_ShouldReturnNull()
        {
            // Arrange
            InitFileSystem();
            var filePathWithWrongExtension = $@"{_rootDirectory}\layout.wrong";
            _fileSystem.AddFile(filePathWithWrongExtension, new MockFileData(string.Empty));

            // Act
            var parsedArguments = GetParser().Parse(new[] { "open", filePathWithWrongExtension });

            // Assert
            Assert.Null(parsedArguments);
        }

        #endregion

        #region Option: export

        [Fact]
        public void Parse_ExportVerb_LayoutFileNotSpecified_ShouldReturnNull()
        {
            // Arrange/Act
            var parsedArguments = GetParser().Parse(new[] { "export" });

            // Assert
            Assert.Null(parsedArguments);
        }

        [Fact]
        public void Parse_ExportVerb_OutputFileNotSpecified_ShouldReturnNull()
        {
            // Arrange
            InitFileSystem();

            // Act
            var parsedArguments = GetParser().Parse(new[] { "export", _pathToLayoutFile });

            // Assert
            Assert.Null(parsedArguments);
        }

        [Fact]
        public void Parse_ExportVerb_OverridesBorder_ShouldReturnParsedValues()
        {
            // Arrange
            InitFileSystem();
            var expectedBorder = 5;

            // Act
            var parsedArguments = GetParser().Parse(new[] { "export", _pathToLayoutFile, _pathToExportedFile, "--border", expectedBorder.ToString() });

            // Assert
            var exportArgs = Assert.IsType<ExportArgs>(parsedArguments);
            Assert.Equal(_pathToLayoutFile, exportArgs.LayoutFilePath);
            Assert.Equal(_pathToExportedFile, exportArgs.ExportedImageFilePath);
            Assert.Equal(expectedBorder, exportArgs.Border);
        }

        [Fact]
        public void Parse_ExportVerb_LayoutFileWithWrongFileExtension_ShouldReturnNull()
        {
            // Arrange
            InitFileSystem();
            var filePathWithWrongExtension = $@"{_rootDirectory}\layout.wrong";
            _fileSystem.AddFile(filePathWithWrongExtension, new MockFileData(string.Empty));

            // Act
            var parsedArguments = GetParser().Parse(new[] { "export", filePathWithWrongExtension, _pathToExportedFile });

            // Assert
            Assert.Null(parsedArguments);
        }


        [Fact]
        public void Parse_ExportVerb_ImageFileWithWrongFileExtension_ShouldReturnNull()
        {
            // Arrange
            InitFileSystem();
            var filePathWithWrongExtension = $@"{_rootDirectory}\layout.wrong";
            _fileSystem.AddFile(filePathWithWrongExtension, new MockFileData(string.Empty));

            // Act
            var parsedArguments = GetParser().Parse(new[] { "export", _pathToLayoutFile, filePathWithWrongExtension });

            // Assert
            Assert.Null(parsedArguments);
        }

        [Fact]
        public void Parse_ExportVerb_ValidPaths_ShouldReturnDefaultValues()
        {
            // Arrange
            InitFileSystem();

            // Act
            var parsedArguments = GetParser().Parse(new[] { "export", _pathToLayoutFile, _pathToExportedFile });

            // Assert
            var exportArgs = Assert.IsType<ExportArgs>(parsedArguments);
            Assert.Equal(_pathToLayoutFile, exportArgs.LayoutFilePath);
            Assert.Equal(_pathToExportedFile, exportArgs.ExportedImageFilePath);

            Assert.Equal(1, exportArgs.Border);
            Assert.Equal(20, exportArgs.GridSize);
            Assert.Null(exportArgs.RenderGrid);
            Assert.Null(exportArgs.RenderHarborBlockedArea);
            Assert.Null(exportArgs.RenderIcon);
            Assert.Null(exportArgs.RenderInfluences);
            Assert.Null(exportArgs.RenderLabel);
            Assert.Null(exportArgs.RenderPanorama);
            Assert.Null(exportArgs.RenderStatistics);
            Assert.Null(exportArgs.RenderTrueInfluenceRange);
            Assert.Null(exportArgs.RenderVersion);
            Assert.False(exportArgs.UseUserSettings);
        }

        #endregion
    }
}
