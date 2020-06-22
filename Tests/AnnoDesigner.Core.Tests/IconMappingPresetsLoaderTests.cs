using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AnnoDesigner.Core.Helper;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Core.Presets.Loader;
using AnnoDesigner.Core.Presets.Models;
using Xunit;

namespace AnnoDesigner.Core.Tests
{
    public class IconMappingPresetsLoaderTests
    {
        [Fact]
        public void Load_ParameterIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            var loader = new IconMappingPresetsLoader();

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => loader.Load(null));
            //// Assert
            //Assert.NotNull(ex);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Load_FilePathIsNullOrWhiteSpace_ShouldThrow(string filePath)
        {
            // Arrange
            var loader = new IconMappingPresetsLoader();

            // Act and Assert
            var ex = Assert.Throws<ArgumentNullException>(() => loader.LoadFromFile(filePath));
        }

        [Fact]
        public void Load_FileHasNoVersionAndOneMapping_ShouldReturnListWithOneMapping()
        {
            // Arrange
            var loader = new IconMappingPresetsLoader();
            var content = "[{\"IconFilename\":\"icon.png\",\"Localizations\":{\"eng\":\"mapped name\"}}]";

            // Act
            var result = loader.Load(content);

            // Assert
            Assert.Single(result.IconNameMappings);
            Assert.Equal(string.Empty, result.Version);
        }

        [Fact]
        public void Load_FileHasVersionAndOneMapping_ShouldReturnListWithOneMapping()
        {
            // Arrange
            var loader = new IconMappingPresetsLoader();
            var content = "{\"Version\":\"0.1\",\"IconNameMappings\":[{\"IconFilename\":\"icon.png\",\"Localizations\":{\"eng\":\"mapped name\"}}]}";

            // Act
            var result = loader.Load(content);

            // Assert
            Assert.Single(result.IconNameMappings);
            Assert.Equal("0.1", result.Version);
        }
    }
}
