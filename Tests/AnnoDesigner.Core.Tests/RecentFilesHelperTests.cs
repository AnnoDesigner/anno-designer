using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Moq;
using AnnoDesigner.Core.Helper;
using System.IO.Abstractions.TestingHelpers;
using AnnoDesigner.Core.RecentFiles;
using AnnoDesigner.Core.Models;
using System.IO.Abstractions;
using System.IO;

namespace AnnoDesigner.Core.Tests
{
    public class RecentFilesHelperTests
    {
        private static readonly MockFileSystem fileSystemWithTestData;

        static RecentFilesHelperTests()
        {
            var mockedFileSystem = new MockFileSystem();
            mockedFileSystem.AddFile(@"C:\test\sub\file_01.ad", MockFileData.NullObject);
            mockedFileSystem.AddFile(@"C:\test\sub\file_02.ad", MockFileData.NullObject);
            mockedFileSystem.AddFile(@"C:\test\sub\file_03.ad", MockFileData.NullObject);
            mockedFileSystem.AddFile(@"C:\test\sub\file_04.ad", MockFileData.NullObject);
            mockedFileSystem.AddFile(@"C:\test\sub\file_05.ad", MockFileData.NullObject);
            mockedFileSystem.AddFile(@"C:\test\sub\file_06.ad", MockFileData.NullObject);
            mockedFileSystem.AddFile(@"C:\test\sub\file_07.ad", MockFileData.NullObject);
            mockedFileSystem.AddFile(@"C:\test\sub\file_08.ad", MockFileData.NullObject);
            mockedFileSystem.AddFile(@"C:\test\sub\file_09.ad", MockFileData.NullObject);
            mockedFileSystem.AddFile(@"C:\test\sub\file_10.ad", MockFileData.NullObject);
            mockedFileSystem.AddFile(@"C:\test\sub\file_11.ad", MockFileData.NullObject);

            fileSystemWithTestData = mockedFileSystem;
        }


        #region test data

        private IRecentFilesHelper GetHelper(IRecentFilesSerializer serializerToUse = null,
            IFileSystem fileSystemToUse = null)
        {
            var serializer = serializerToUse ?? new RecentFilesInMemorySerializer();
            var fileSystem = fileSystemToUse ?? new MockFileSystem();

            return new RecentFilesHelper(serializer, fileSystem);
        }

        #endregion

        #region ctor tests

        [Fact]
        public void Ctor_SerializerIsNull_ShouldThrow()
        {
            // Arrange/Act
            var ex = Assert.Throws<ArgumentNullException>(() => new RecentFilesHelper(null, new MockFileSystem()));

            // Assert
            Assert.NotNull(ex);
        }

        [Fact]
        public void Ctor_FileSystemIsNull_ShouldThrow()
        {
            // Arrange/Act
            var ex = Assert.Throws<ArgumentNullException>(() => new RecentFilesHelper(new RecentFilesInMemorySerializer(), null));

            // Assert
            Assert.NotNull(ex);
        }

        [Fact]
        public void Ctor_Defaults_Set()
        {
            // Arrange/Act
            var helper = new RecentFilesHelper(new RecentFilesInMemorySerializer(), new MockFileSystem());

            // Assert
            Assert.Empty(helper.RecentFiles);
            Assert.Equal(10, helper.MaximumItemCount);
        }

        [Fact]
        public void Ctor_Defaults_ShouldCallDeserialize()
        {
            // Arrange
            var mockedSerializer = new Mock<IRecentFilesSerializer>();
            mockedSerializer.Setup(_ => _.Deserialize()).Returns(() => new List<RecentFile>());

            // Arrange/Act
            var helper = new RecentFilesHelper(mockedSerializer.Object, new MockFileSystem());

            // Assert
            mockedSerializer.Verify(_ => _.Deserialize(), Times.Once);
        }

        #endregion

        #region AddFile tests

        [Fact]
        public void AddFile_ParameterIsNull_ShouldNotThrow()
        {
            // Arrange
            Exception expectedException = null;
            var helper = GetHelper();

            // Act
            try
            {
                helper.AddFile(null);
            }
            catch (Exception ex)
            {
                expectedException = ex;
            }

            // Assert
            Assert.Null(expectedException);
        }

        [Fact]
        public void AddFile_FileExists_ShouldPlaceItOnTop()
        {
            // Arrange
            var helper = GetHelper(fileSystemToUse: fileSystemWithTestData);
            var fileToAdd = new RecentFile(fileSystemWithTestData.AllFiles.Last(), DateTime.UtcNow);

            // Act
            helper.AddFile(fileToAdd);

            // Assert
            Assert.Equal(fileToAdd, helper.RecentFiles.First());
        }

        [Fact]
        public void AddFile_MaximumItemCountExceeded_ShouldEnsureMaximumItemCount()
        {
            // Arrange
            var maximumItemCountToSet = 5;

            var helper = GetHelper(fileSystemToUse: fileSystemWithTestData);
            helper.MaximumItemCount = maximumItemCountToSet;

            var fileToAdd = new RecentFile(@"C:\test\dummyFile.ad", DateTime.UtcNow);

            // Act
            helper.AddFile(fileToAdd);

            // Assert
            Assert.Equal(maximumItemCountToSet, helper.RecentFiles.Count);
        }

        [Fact]
        public void AddFile_FileNotNull_ShouldCallSerialize()
        {
            // Arrange
            var mockedSerializer = new Mock<IRecentFilesSerializer>();
            mockedSerializer.Setup(_ => _.Deserialize()).Returns(() => new List<RecentFile>());

            var helper = GetHelper(serializerToUse: mockedSerializer.Object, fileSystemToUse: fileSystemWithTestData);
            var fileToAdd = new RecentFile(fileSystemWithTestData.AllFiles.Last(), DateTime.UtcNow);

            // Act
            helper.AddFile(fileToAdd);

            // Assert
            mockedSerializer.Verify(_ => _.Serialize(It.IsAny<List<RecentFile>>()), Times.Once);
        }

        #endregion
    }
}
