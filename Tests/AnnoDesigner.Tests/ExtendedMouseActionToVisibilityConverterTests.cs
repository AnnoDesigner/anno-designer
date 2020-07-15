using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AnnoDesigner.Converters;
using AnnoDesigner.Core.Models;
using Xunit;

namespace AnnoDesigner.Tests
{
    public class ExtendedMouseActionToVisibilityConverterTests
    {
        #region Convert tests

        [Theory]
        [InlineData(ExtendedMouseAction.LeftClick, Visibility.Visible)]
        [InlineData(ExtendedMouseAction.MiddleClick, Visibility.Visible)]
        [InlineData(ExtendedMouseAction.RightClick, Visibility.Visible)]
        [InlineData(ExtendedMouseAction.LeftDoubleClick, Visibility.Visible)]
        [InlineData(ExtendedMouseAction.MiddleDoubleClick, Visibility.Visible)]
        [InlineData(ExtendedMouseAction.RightDoubleClick, Visibility.Visible)]
        public void Convert_PassedKnownValueAndParameter_ShouldReturnCorrectValue(ExtendedMouseAction input, Visibility expected)
        {
            // Arrange
            var converter = new ExtendedMouseActionToVisibilityConverter();

            // Act
            var result = converter.Convert(input, typeof(Visibility), 0, CultureInfo.CurrentCulture);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData((ExtendedMouseAction)int.MaxValue)]
        [InlineData(ExtendedMouseAction.None)]
        public void Convert_PassedUnknownValue_ShouldReturnNull(ExtendedMouseAction input)
        {
            // Arrange
            var converter = new ExtendedMouseActionToVisibilityConverter();

            // Act
            var result = converter.Convert(input, typeof(Visibility), 0, CultureInfo.CurrentCulture);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Convert_PassedUnknownValueAndUnknownParameter_ShouldReturnNull()
        {
            // Arrange
            var converter = new ExtendedMouseActionToVisibilityConverter();

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
            var converter = new ExtendedMouseActionToVisibilityConverter();

            // Act/Assert
            Assert.Throws<NotImplementedException>(() => converter.ConvertBack(ExtendedMouseAction.LeftClick, typeof(Visibility), null, CultureInfo.CurrentCulture));
        }

        #endregion
    }
}
