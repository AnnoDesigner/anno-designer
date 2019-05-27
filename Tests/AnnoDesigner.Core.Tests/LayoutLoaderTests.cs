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
        private static readonly string testData_LayoutWithNoVersionAndObjects;

        #endregion

        static LayoutLoaderTests()
        {
            testData_v3_LayoutWithVersionAndObjects = File.ReadAllText(Path.Combine("Testdata", "Layout", "v3_layoutWithVersionAndObjects.ad"), Encoding.UTF8);
            testData_LayoutWithNoVersionAndObjects = File.ReadAllText(Path.Combine("Testdata", "Layout", "layoutWithNoVersionAndObjects.ad"), Encoding.UTF8);
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
            var ex = Assert.Throws<LayoutFileVersionMismatchException>(() => loader.LoadLayout(streamWithLayout));

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
            var ex = Assert.Throws<LayoutFileVersionMismatchException>(() => loader.LoadLayout(streamWithLayout));

            // Assert
            Assert.NotNull(ex);
        }

        [Fact]
        public void LoadLayout_LayoutHasVersionInfo_ShouldReturnListWithObjects()
        {
            // Arrange
            ILayoutLoader loader = new LayoutLoader();

            var streamWithLayout = new MemoryStream(Encoding.UTF8.GetBytes(testData_v3_LayoutWithVersionAndObjects));

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
    }
}
