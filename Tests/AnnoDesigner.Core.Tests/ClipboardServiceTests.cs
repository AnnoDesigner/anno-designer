using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnnoDesigner.Core.Layout;
using AnnoDesigner.Core.Layout.Models;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Core.Services;
using AnnoDesigner.Core.Tests.Mocks;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace AnnoDesigner.Core.Tests
{
    public class ClipboardServiceTests
    {
        private readonly MockedClipboard _mockedClipboard;
        private readonly ILayoutLoader _mockedLayoutLoader;

        public ClipboardServiceTests()
        {
            _mockedClipboard = new MockedClipboard();
            _mockedLayoutLoader = new LayoutLoader();
        }

        private IClipboardService GetService(ILayoutLoader layoutLoaderToUse = null,
            IClipboard clipboardToUse = null)
        {
            return new ClipboardService(layoutLoaderToUse ?? _mockedLayoutLoader,
                clipboardToUse ?? _mockedClipboard);
        }

        private List<AnnoObject> GetListOfObjects()
        {
            return new List<AnnoObject>
            {
                new AnnoObject
                {
                    Identifier = "my dummy"
                }
            };
        }

        #region Copy tests

        [Fact]
        public void Copy_ListIsEmpty_ShouldNotAddDataToClipboard()
        {
            // Arrange
            var service = GetService();

            // Act
            service.Copy(Array.Empty<AnnoObject>());

            // Assert
            Assert.Null(_mockedClipboard.GetData(CoreConstants.AnnoDesignerClipboardFormat));
        }

        [Fact]
        public void Copy_ListIsNull_ShouldNotThrowAndNotAddDataToClipboard()
        {
            // Arrange
            var service = GetService();

            // Act
            var ex = Record.Exception(() => service.Copy(null));

            // Assert
            Assert.Null(ex);
            Assert.Null(_mockedClipboard.GetData(CoreConstants.AnnoDesignerClipboardFormat));
        }

        [Fact]
        public void Copy_ListHasData_ShouldAddDataToClipboard()
        {
            // Arrange
            var service = GetService();
            var data = GetListOfObjects();

            // Act
            service.Copy(data);

            // Assert
            var clipboardStream = _mockedClipboard.GetData(CoreConstants.AnnoDesignerClipboardFormat) as System.IO.Stream;
            var copiedObjects = _mockedLayoutLoader.LoadLayout(clipboardStream, forceLoad: true).Objects;

            Assert.Equal(data.Count, copiedObjects.Count);
            Assert.All(data, x =>
            {
                //Assert.Contains(x, copiedObjects); //not useable because of missing cutom comparer for AnnoObject
                Assert.Contains(copiedObjects, y => y.Identifier.Equals(x.Identifier, StringComparison.OrdinalIgnoreCase));
            });
        }

        [Fact]
        public void Copy_ListHasData_ShouldClearClipboardBeforeAddingData()
        {
            // Arrange
            var mockedClipboard = new Mock<IClipboard>();
            var callOrder = 0;
            _ = mockedClipboard.Setup(x => x.Clear()).Callback(() => Assert.Equal(0, callOrder++));
            _ = mockedClipboard.Setup(x => x.SetData(It.IsAny<string>(), It.IsAny<object>())).Callback(() => Assert.Equal(1, callOrder++));

            var service = GetService(clipboardToUse: mockedClipboard.Object);
            var data = GetListOfObjects();

            // Act
            service.Copy(data);

            // Assert
            mockedClipboard.Verify(x => x.Clear(), Times.Once);
        }

        [Fact]
        public void Copy_ListHasData_ShouldFlushClipboardAfterAddingData()
        {
            // Arrange
            var mockedClipboard = new Mock<IClipboard>();
            var callOrder = 0;
            _ = mockedClipboard.Setup(x => x.SetData(It.IsAny<string>(), It.IsAny<object>())).Callback(() => Assert.Equal(0, callOrder++));
            _ = mockedClipboard.Setup(x => x.Flush()).Callback(() => Assert.Equal(1, callOrder++));

            var service = GetService(clipboardToUse: mockedClipboard.Object);
            var data = GetListOfObjects();

            // Act
            service.Copy(data);

            // Assert
            mockedClipboard.Verify(x => x.Flush(), Times.Once);
        }

        #endregion

        #region Paste tests

        [Fact]
        public void Paste_ClipboardHasNoData_ShouldReturnEmpty()
        {
            // Arrange
            var service = GetService();

            // Act
            var result = service.Paste();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void Paste_Files_ClipboardHasMultipleFiles_ShouldReturnEmpty()
        {
            // Arrange
            var service = GetService();
            _mockedClipboard.AddFilesToClipboard(new List<string> { "first file path", "second file path" });

            // Act
            var result = service.Paste();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void Paste_Files_ClipboardFileListReturnsNull_ShouldReturnEmpty()
        {
            // Arrange
            var mockedClipboard = new Mock<IClipboard>();
            _ = mockedClipboard.Setup(x => x.GetFileDropList()).Returns(() => null);

            var service = GetService(clipboardToUse: mockedClipboard.Object);

            // Act
            var result = service.Paste();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void Paste_Files_ClipboardHasSingleFile_ShouldReturnLayoutObjects()
        {
            // Arrange
            var data = GetListOfObjects();

            var mockedLayoutLoader = new Mock<ILayoutLoader>();
            _ = mockedLayoutLoader.Setup(x => x.LoadLayout(It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(() => new LayoutFile(data));

            var service = GetService(layoutLoaderToUse: mockedLayoutLoader.Object);
            _mockedClipboard.AddFilesToClipboard(new List<string> { "first" });

            // Act
            var result = service.Paste();

            // Assert
            Assert.Equal(data, result);
        }

        [Fact]
        public void Paste_Files_ClipboardHasOnlySingleFileAndLayoutLoaderThrows_ShouldReturnEmpty()
        {
            // Arrange
            var data = GetListOfObjects();

            var mockedLayoutLoader = new Mock<ILayoutLoader>();
            _ = mockedLayoutLoader.Setup(x => x.LoadLayout(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws(new JsonReaderException());

            var service = GetService(layoutLoaderToUse: mockedLayoutLoader.Object);
            _mockedClipboard.AddFilesToClipboard(new List<string> { "first" });

            // Act
            var result = service.Paste();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        #endregion
    }
}
