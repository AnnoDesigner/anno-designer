using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnnoDesigner.Core.Presets.Models;
using PresetParser.Extensions;
using Xunit;
using Moq;

namespace PresetParser.Tests
{
    public class StringExtensionsTests
    {
        private const string DUMMY = "Dummy";

        #region helper methods

        private IBuildingInfo GetBuilding(string identifierToUse)
        {
            var mockedBuilding = new Mock<IBuildingInfo>();
            mockedBuilding.Setup(x => x.Identifier).Returns(identifierToUse);

            return mockedBuilding.Object;
        }

        #endregion

        #region IsMatch - buildings

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void IsMatch_Buildings_IdentifierIsNullOrWhitespace_ShouldReturnFalse(string identifierToCheck)
        {
            // Arrange            
            var listToCheck = new List<IBuildingInfo>();
            listToCheck.Add(GetBuilding(DUMMY));

            // Act
            var result = StringExtensions.IsMatch(identifierToCheck, listToCheck);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsMatch_Buildings_ListIsEmpty_ShouldReturnFalse()
        {
            // Arrange            
            var listToCheck = new List<IBuildingInfo>();

            // Act
            var result = StringExtensions.IsMatch(DUMMY, listToCheck);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsMatch_Buildings_ListIsNull_ShouldReturnFalse()
        {
            // Arrange            
            List<IBuildingInfo> listToCheck = null;

            // Act
            var result = StringExtensions.IsMatch(DUMMY, listToCheck);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region IsMatch - strings

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void IsMatch_Strings_IdentifierIsNullOrWhitespace_ShouldReturnFalse(string identifierToCheck)
        {
            // Arrange            
            var listToCheck = new List<string>();
            listToCheck.Add(DUMMY);

            // Act
            var result = StringExtensions.IsMatch(identifierToCheck, listToCheck);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsMatch_Strings_ListIsEmpty_ShouldReturnFalse()
        {
            // Arrange            
            var listToCheck = new List<string>();

            // Act
            var result = StringExtensions.IsMatch(DUMMY, listToCheck);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsMatch_Strings_ListIsNull_ShouldReturnFalse()
        {
            // Arrange            
            List<string> listToCheck = null;

            // Act
            var result = StringExtensions.IsMatch(DUMMY, listToCheck);

            // Assert
            Assert.False(result);
        }

        #endregion
    }
}
