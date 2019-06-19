using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FandomParser.Core;
using Xunit;

namespace InfoboxParser.Tests
{
    public class ParserBothWorldsTests
    {
        private static readonly ICommons mockedCommons;

        static ParserBothWorldsTests()
        {
            mockedCommons = Commons.Instance;
        }

        [Theory]
        [InlineData(null)]
        [InlineData(" ")]
        [InlineData("")]
        public void GetInfobox_InputIsNullOrWhiteSpace_ShouldReturnNull(string input)
        {
            // Arrange
            var parser = new ParserBothWorlds(mockedCommons);

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Null(result);
        }
    }
}
