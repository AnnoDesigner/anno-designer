using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AnnoDesigner.Core.Converters;
using Xunit;

namespace AnnoDesigner.Core.Tests
{
    public class ModifierKeysToVisibilityConverterTests
    {
        #region Convert tests

        [Theory]
        [InlineData(ModifierKeys.Control, Visibility.Visible)]
        [InlineData(ModifierKeys.Alt, Visibility.Visible)]
        [InlineData(ModifierKeys.Shift, Visibility.Visible)]
        [InlineData(ModifierKeys.Windows, Visibility.Visible)]
        [InlineData(ModifierKeys.None, Visibility.Collapsed)]
        [InlineData(ModifierKeys.Control | ModifierKeys.Alt, Visibility.Visible)]
        public void Convert_PassedKnownValue_ShouldReturnCorrectValue(ModifierKeys input, Visibility expected)
        {
            // Arrange
            var converter = new ModifierKeysToVisibilityConverter();

            // Act
            var result = converter.Convert(input, typeof(Visibility), null, CultureInfo.CurrentCulture);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(ModifierKeys.Control, Visibility.Visible)]
        [InlineData(ModifierKeys.Alt, Visibility.Visible)]
        [InlineData(ModifierKeys.Shift, Visibility.Visible)]
        [InlineData(ModifierKeys.Windows, Visibility.Visible)]
        [InlineData(ModifierKeys.None, Visibility.Collapsed)]
        [InlineData(ModifierKeys.Control | ModifierKeys.Alt, Visibility.Visible)]
        public void Convert_PassedParameterValue_ShouldReturnCorrectValue(ModifierKeys input, Visibility expected)
        {
            // Arrange
            var converter = new ModifierKeysToVisibilityConverter();

            // Act
            var result = converter.Convert(null, typeof(Visibility), input, CultureInfo.CurrentCulture);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Convert_PassedUnknownValueAndUnknownParameter_ShouldReturnNull()
        {
            // Arrange
            var converter = new ModifierKeysToVisibilityConverter();

            // Act
            var result = converter.Convert(null, typeof(Visibility), null, CultureInfo.CurrentCulture);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region ConvertBack tests

        [Fact]
        public void ConvertBack_PassedAnyValue_ShouldThrow()
        {
            // Arrange
            var converter = new ModifierKeysToVisibilityConverter();

            // Act/Assert
            Assert.Throws<NotImplementedException>(() => converter.ConvertBack(ModifierKeys.Control, typeof(Visibility), null, CultureInfo.CurrentCulture));
        }

        #endregion
    }
}
