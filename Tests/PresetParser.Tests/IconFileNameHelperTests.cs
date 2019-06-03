using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Xunit;

namespace PresetParser.Tests
{
    public class IconFileNameHelperTests
    {
        [Fact]
        public void GetIconFilename_AnnoVersionIs1404_ShouldReturnFileNameWithPrefix()
        {
            // Arrange
            var helper = new IconFileNameHelper();

            var doc = new XmlDocument();
            doc.LoadXml("<root><IconFileID>myFileId</IconFileID><IconIndex>42</IconIndex></root>");
            var rootNode = doc["root"];

            // Act
            var result = helper.GetIconFilename(rootNode, Constants.ANNO_VERSION_1404);

            // Assert
            Assert.StartsWith("A4_", result);
        }

        [Fact]
        public void GetIconFilename_AnnoVersionIs1404AndNoIconIndex_ShouldReturnFileNameWithPrefixAndIconIndexZero()
        {
            // Arrange
            var helper = new IconFileNameHelper();

            var doc = new XmlDocument();
            doc.LoadXml("<root><IconFileID>myFileId</IconFileID></root>");
            var rootNode = doc["root"];

            // Act
            var result = helper.GetIconFilename(rootNode, Constants.ANNO_VERSION_1404);

            // Assert
            Assert.Equal("A4_icon_myFileId_0.png", result);
        }

        [Fact]
        public void GetIconFilename_AnnoVersionIs1404AndIconIndex_ShouldReturnFileNameWithPrefixAndIconIndex()
        {
            // Arrange
            var helper = new IconFileNameHelper();

            var doc = new XmlDocument();
            doc.LoadXml("<root><IconFileID>myFileId</IconFileID><IconIndex>42</IconIndex></root>");
            var rootNode = doc["root"];

            // Act
            var result = helper.GetIconFilename(rootNode, Constants.ANNO_VERSION_1404);

            // Assert
            Assert.Equal("A4_icon_myFileId_42.png", result);
        }

        [Fact]
        public void GetIconFilename_AnnoVersionIsNot1404AndNoIconIndex_ShouldReturnFileNameWithIconIndexZero()
        {
            // Arrange
            var helper = new IconFileNameHelper();

            var doc = new XmlDocument();
            doc.LoadXml("<root><IconFileID>myFileId</IconFileID></root>");
            var rootNode = doc["root"];

            // Act
            var result = helper.GetIconFilename(rootNode, Constants.ANNO_VERSION_1800);

            // Assert
            Assert.Equal("icon_myFileId_0.png", result);
        }

        [Fact]
        public void GetIconFilename_AnnoVersionIsNot1404AndIconIndex_ShouldReturnFileNameWithIconIndex()
        {
            // Arrange
            var helper = new IconFileNameHelper();

            var doc = new XmlDocument();
            doc.LoadXml("<root><IconFileID>myFileId</IconFileID><IconIndex>42</IconIndex></root>");
            var rootNode = doc["root"];

            // Act
            var result = helper.GetIconFilename(rootNode, Constants.ANNO_VERSION_2205);

            // Assert
            Assert.Equal("icon_myFileId_42.png", result);
        }
    }
}
