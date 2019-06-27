using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FandomParser.Core;
using Moq;
using Xunit;
using System.IO;
using System.Reflection;
using FandomParser.Core.Presets.Models;
using InfoboxParser.Tests.Attributes;

namespace InfoboxParser.Tests
{
    public class InfoboxParserTests
    {
        private static readonly ICommons mockedCommons;

        static InfoboxParserTests()
        {
            mockedCommons = Commons.Instance;
        }

        [Theory]
        [InlineData(null)]
        [InlineData(" ")]
        [InlineData("")]
        public void GetInfobox_WikiTextIsNullOrWhiteSpace_ShouldReturnNull(string input)
        {
            // Arrange
            var parser = new InfoboxParser(mockedCommons);

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetInfobox_WikiTextContainsInfobox_ShouldReturnSingleResult()
        {
            // Arrange
            var input = "{{Infobox Buildings";

            var parser = new InfoboxParser(mockedCommons);

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public void GetInfobox_WikiTextContainsInfoboxForBothWorlds_ShouldReturnMultipleResults()
        {
            // Arrange
            var input = "{{Infobox Buildings Old and New World";

            var parser = new InfoboxParser(mockedCommons);

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(2, result.Count);
        }
    }
}
