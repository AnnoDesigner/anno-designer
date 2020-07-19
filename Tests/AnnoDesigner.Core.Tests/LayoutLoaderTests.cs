using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnnoDesigner.Core.Helper;
using AnnoDesigner.Core.Layout;
using AnnoDesigner.Core.Layout.Exceptions;
using AnnoDesigner.Core.Layout.Models;
using AnnoDesigner.Core.Models;
using Xunit;

namespace AnnoDesigner.Core.Tests
{
    public class LayoutLoaderTests
    {
        #region testdata

        private static readonly string testData_v3_LayoutWithVersionAndObjects;
        private static readonly string testData_v4_LayoutWithVersionAndObjects;
        private static readonly string testData_LayoutWithNoVersionAndObjects;

        #endregion

        static LayoutLoaderTests()
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;

            testData_v3_LayoutWithVersionAndObjects = File.ReadAllText(Path.Combine(basePath, "Testdata", "Layout", "v3_layoutWithVersionAndObjects.ad"), Encoding.UTF8);
            testData_v4_LayoutWithVersionAndObjects = File.ReadAllText(Path.Combine(basePath, "Testdata", "Layout", "v4_layoutWithVersionAndObjects.ad"), Encoding.UTF8);
            testData_LayoutWithNoVersionAndObjects = File.ReadAllText(Path.Combine(basePath, "Testdata", "Layout", "layoutWithNoVersionAndObjects.ad"), Encoding.UTF8);
        }

        [Fact]
        public void LoadLayout_StreamIsNull_ShouldThrow()
        {
            // Arrange
            ILayoutLoader loader = new LayoutLoader();

            // Act
            var ex = Assert.Throws<ArgumentNullException>(() => loader.LoadLayout((Stream)null));

            // Assert
            Assert.NotNull(ex);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void LoadLayout_FilePathIsNullOrWhiteSpace_ShouldThrow(string filePath)
        {
            // Arrange
            ILayoutLoader loader = new LayoutLoader();

            // Act
            var ex = Assert.Throws<ArgumentNullException>(() => loader.LoadLayout((string)filePath));

            // Assert
            Assert.NotNull(ex);
        }

        [Fact]
        public void LoadLayout_LayoutHasOlderVersion_ShouldThrow()
        {
            // Arrange
            ILayoutLoader loader = new LayoutLoader();

            var layoutContent = $"{{\"FileVersion\":{CoreConstants.LayoutFileVersion - 1},\"Objects\":[]}}";
            var streamWithLayout = new MemoryStream(Encoding.UTF8.GetBytes(layoutContent));

            // Act
            var ex = Assert.Throws<LayoutFileUnsupportedFormatException>(() => loader.LoadLayout(streamWithLayout));

            // Assert
            Assert.NotNull(ex);
        }

        [Fact]
        public void LoadLayout_LayoutHasNewerVersion_ShouldThrow()
        {
            // Arrange
            ILayoutLoader loader = new LayoutLoader();

            var layoutContent = $"{{\"FileVersion\":{CoreConstants.LayoutFileVersion + 1},\"Objects\":[]}}";
            var streamWithLayout = new MemoryStream(Encoding.UTF8.GetBytes(layoutContent));

            // Act
            var ex = Assert.Throws<LayoutFileUnsupportedFormatException>(() => loader.LoadLayout(streamWithLayout));

            // Assert
            Assert.NotNull(ex);
        }

        [Fact]
        public void LoadLayout_LayoutHasVersionInfo_ShouldReturnListWithObjects()
        {
            // Arrange
            ILayoutLoader loader = new LayoutLoader();

            var streamWithLayout = new MemoryStream(Encoding.UTF8.GetBytes(testData_v4_LayoutWithVersionAndObjects));

            // Act
            var result = loader.LoadLayout(streamWithLayout);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void LoadLayout_LayoutHasNoVersionInfoAndIsForcedLoad_ShouldReturnListWithObjects()
        {
            // Arrange
            ILayoutLoader loader = new LayoutLoader();

            var streamWithLayout = new MemoryStream(Encoding.UTF8.GetBytes(testData_LayoutWithNoVersionAndObjects));

            // Act
            var result = loader.LoadLayout(streamWithLayout, true);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void LoadLayout_LayoutHasNewerVersionAndIsForcedToLoad_ShouldReturnListWithObjects()
        {
            // Arrange
            ILayoutLoader loader = new LayoutLoader();

            var layoutContent = $"{{\"FileVersion\":{CoreConstants.LayoutFileVersion + 1},\"Objects\":[{{\"Identifier\":\"Lorem\"}}]}}";
            var streamWithLayout = new MemoryStream(Encoding.UTF8.GetBytes(layoutContent));

            // Act
            var result = loader.LoadLayout(streamWithLayout, true);

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public void LoadLayout_LayoutHasOlderVersionAndIsForcedToLoad_ShouldReturnListWithObjects()
        {
            // Arrange
            ILayoutLoader loader = new LayoutLoader();
            var streamWithLayout = new MemoryStream(Encoding.UTF8.GetBytes(testData_v3_LayoutWithVersionAndObjects));

            // Act
            var result = loader.LoadLayout(streamWithLayout, true);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void LoadLayout_LayoutHasBuildingWithTransparentColor_ShouldReturnListWithObjectAndTransparentColor()
        {
            // Arrange
            ILayoutLoader loader = new LayoutLoader();

            var expectedA = 92;
            var expectedR = 0;
            var expectedG = 242;
            var expectedB = 63;
            var layoutContent = $"{{\"FileVersion\":{CoreConstants.LayoutFileVersion},\"Objects\":[{{\"Identifier\":\"Lorem\",\"Color\":{{\"A\":{expectedA},\"R\":{expectedR},\"G\":{expectedG},\"B\":{expectedB}}}}}]}}";
            var streamWithLayout = new MemoryStream(Encoding.UTF8.GetBytes(layoutContent));

            // Act
            var result = loader.LoadLayout(streamWithLayout, true);

            // Assert
            Assert.Single(result);
            Assert.Equal(expectedA, result[0].Color.A);
            Assert.Equal(expectedR, result[0].Color.R);
            Assert.Equal(expectedG, result[0].Color.G);
            Assert.Equal(expectedB, result[0].Color.B);
        }

        [Fact]
        public void SaveLayout_LayoutHasOneObject_ShouldReturnListWithOneObject()
        {
            // Arrange
            ILayoutLoader loader = new LayoutLoader();

            var savedStream = new MemoryStream();

            var listToSave = new List<AnnoObject> { new AnnoObject { Identifier = "Lorem" } };

            // Act
            loader.SaveLayout(listToSave, savedStream);

            savedStream.Position = 0;

            var result = loader.LoadLayout(savedStream);

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public void SaveLayout_LayoutHasBuildingWithTransparentColor_ShouldReturnListWithOneObjectAndTransparentColor()
        {
            // Arrange
            ILayoutLoader loader = new LayoutLoader();

            byte expectedA = 92;
            byte expectedR = 0;
            byte expectedG = 242;
            byte expectedB = 63;

            var savedStream = new MemoryStream();

            var listToSave = new List<AnnoObject> { new AnnoObject { Identifier = "Lorem", Color = new SerializableColor(expectedA, expectedR, expectedG, expectedB) } };

            // Act
            loader.SaveLayout(listToSave, savedStream);

            savedStream.Position = 0;

            var result = loader.LoadLayout(savedStream);

            // Assert
            Assert.Single(result);
            Assert.Equal(expectedA, result[0].Color.A);
            Assert.Equal(expectedR, result[0].Color.R);
            Assert.Equal(expectedG, result[0].Color.G);
            Assert.Equal(expectedB, result[0].Color.B);
        }
    }
}
