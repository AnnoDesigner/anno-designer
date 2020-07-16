using System;
using System.Globalization;
using AnnoDesigner.Converters;
using AnnoDesigner.Models;
using Moq;
using Xunit;

namespace AnnoDesigner.Tests
{
    public class UserDefinedColorToDisplayNameConverterTests
    {
        public UserDefinedColorToDisplayNameConverterTests()
        {
            var commonsMock = new Mock<ICommons>();
            commonsMock.SetupGet(x => x.SelectedLanguage).Returns(() => "English");
            Localization.Localization.Init(commonsMock.Object);
        }

        #region test data

        public static TheoryData<UserDefinedColor, string> KnownValueTestData
        {
            get
            {
                return new TheoryData<UserDefinedColor, string>
                {
                    { new UserDefinedColor{ Type = UserDefinedColorType.Custom }, "Custom" },
                    { new UserDefinedColor{ Type = UserDefinedColorType.Default }, "Default" },
                    { new UserDefinedColor{ Type = UserDefinedColorType.Light }, "Light" },
                };
            }
        }

        #endregion

        #region Convert tests

        [Theory]
        [MemberData(nameof(KnownValueTestData))]
        public void Convert_PassedKnownValue_ShouldReturnCorrectValue(UserDefinedColor input, string expected)
        {
            // Arrange
            var converter = new UserDefinedColorToDisplayNameConverter();

            // Act
            var result = converter.Convert(input, typeof(string), 0, CultureInfo.CurrentCulture);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("dummy")]
        [InlineData(42)]
        public void Convert_PassedUnknownValue_ShouldReturnInput(object input)
        {
            // Arrange
            var converter = new UserDefinedColorToDisplayNameConverter();

            // Act
            var result = converter.Convert(input, typeof(string), null, CultureInfo.CurrentCulture);

            // Assert
            Assert.Equal(input, result);
        }

        #endregion

        #region ConvertBack tests

        [Fact]
        public void ConvertBack_PassedAnyValue_ShouldThrow()
        {
            // Arrange
            var converter = new UserDefinedColorToDisplayNameConverter();

            // Act/Assert
            Assert.Throws<NotImplementedException>(() => converter.ConvertBack(string.Empty, typeof(UserDefinedColor), null, CultureInfo.CurrentCulture));
        }

        #endregion
    }
}
