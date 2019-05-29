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
        public void Load_StreamIsNull_ShouldThrow()
        {
            // Arrange
            IconMappingPresetsLoader loader = new IconMappingPresetsLoader();

            // Act
            var ex = Assert.Throws<ArgumentNullException>(() => loader.Load((Stream)null));

            // Assert
            Assert.NotNull(ex);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Load_FilePathIsNullOrWhiteSpace_ShouldThrow(string filePath)
        {
            // Arrange
            IconMappingPresetsLoader loader = new IconMappingPresetsLoader();

            // Act
            var ex = Assert.Throws<ArgumentNullException>(() => loader.Load((string)filePath));

            // Assert
            Assert.NotNull(ex);
        }

        [Fact]
        public void Load_FileHasNoVersionAndOneMapping_ShouldReturnListWithOneMapping()
        {
            // Arrange
            var loader = new IconMappingPresetsLoader();

            var content = "[{\"IconFilename\":\"icon.png\",\"Localizations\":{\"eng\":\"mapped name\"}}]";
            var streamWithIconMapping = new MemoryStream(Encoding.UTF8.GetBytes(content));

            // Act
            var result = loader.Load(streamWithIconMapping);

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
            var streamWithIconMapping = new MemoryStream(Encoding.UTF8.GetBytes(content));

            // Act
            var result = loader.Load(streamWithIconMapping);

            // Assert
            Assert.Single(result.IconNameMappings);
            Assert.Equal("0.1", result.Version);
        }
    }
}
