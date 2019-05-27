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
        public void GetIconFilename_AnnoVersionIs1404_ShouldReturnFileNameWithIndex()
        {
            //// Arrange
            //var helper = new IconFileNameHelper();

            //var doc = new XmlDocument();
            //var rootNode = doc.CreateNode(XmlNodeType.Element, "IconFileID", "");

            //var iconNode = doc.CreateNode(XmlNodeType.Text, "IconFileID", "");
            //rootNode.InnerText = "inner";

            //rootNode.AppendChild(iconNode);

            //// Act
            //var result = helper.GetIconFilename(rootNode, Constants.ANNO_VERSION_1404);

            //// Assert
            //Assert.StartsWith("A4_", result);
        }
    }
}
