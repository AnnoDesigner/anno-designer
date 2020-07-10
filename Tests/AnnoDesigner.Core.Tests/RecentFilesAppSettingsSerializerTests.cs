using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Core.RecentFiles;
using Moq;
using Xunit;

namespace AnnoDesigner.Core.Tests
{
    public class RecentFilesAppSettingsSerializerTests
    {
        private IRecentFilesSerializer GetSerializer(IAppSettings appSettingsToUse = null)
        {
            var mockedSettings = new Mock<IAppSettings>();
            mockedSettings.SetupAllProperties();

            var settings = appSettingsToUse ?? mockedSettings.Object;

            return new RecentFilesAppSettingsSerializer(settings);
        }

        #region Deserialize tests

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Deserialize_SavedListIsIsNullOrWhiteSpace_ShouldReturnEmptyList(string savedList)
        {
            // Arrange
            var mockedSettings = new Mock<IAppSettings>();
            mockedSettings.SetupAllProperties();
            mockedSettings.SetupGet(x => x.RecentFiles).Returns(() => savedList);

            var serializer = GetSerializer(mockedSettings.Object);

            // Act
            var result = serializer.Deserialize();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void Deserialize_CanNotDeserialize_ShouldReturnEmptyList()
        {
            // Arrange
            var mockedSettings = new Mock<IAppSettings>();
            mockedSettings.SetupAllProperties();
            mockedSettings.SetupGet(x => x.RecentFiles).Returns(() => "[{\"myPath\":\"dummyPath\"}]");

            var serializer = GetSerializer(mockedSettings.Object);

            // Act
            var result = serializer.Deserialize();

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region Serialize tests

        [Fact]
        public void Serialize_ParameterIsNull_ShouldNotThrow()
        {
            // Arrange
            var serializer = GetSerializer();

            // Act
            var ex = Record.Exception(() => serializer.Serialize(null));

            // Assert
            Assert.Null(ex);
        }

        [Fact]
        public void Serialize_ParameterIsNotNull_ShouldCallSaveOnAppSettings()
        {
            // Arrange
            var mockedSettings = new Mock<IAppSettings>();
            mockedSettings.SetupAllProperties();
            mockedSettings.SetupGet(x => x.RecentFiles).Returns(() => string.Empty);

            var serializer = GetSerializer(mockedSettings.Object);

            // Act
            serializer.Serialize(new List<RecentFile>());

            // Assert
            mockedSettings.Verify(x => x.Save(), Times.Once);
        }

        #endregion
    }
}
