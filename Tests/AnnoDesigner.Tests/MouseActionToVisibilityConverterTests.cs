using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AnnoDesigner.Converters;
using Xunit;

namespace AnnoDesigner.Tests
{
    public class MouseActionToVisibilityConverterTests
    {
        #region Convert tests

        [Theory]
        [InlineData(MouseAction.LeftClick, Visibility.Visible)]
        [InlineData(MouseAction.MiddleClick, Visibility.Visible)]
        [InlineData(MouseAction.RightClick, Visibility.Visible)]
        [InlineData(MouseAction.LeftDoubleClick, Visibility.Visible)]
        [InlineData(MouseAction.MiddleDoubleClick, Visibility.Visible)]
        [InlineData(MouseAction.RightDoubleClick, Visibility.Visible)]
        public void Convert_PassedKnownValueAndParameter_ShouldReturnCorrectValue(MouseAction input, Visibility expected)
        {
            // Arrange
            var converter = new ExtendedMouseActionToVisibilityConverter();

            // Act
            var result = converter.Convert(input, typeof(Visibility), 0, CultureInfo.CurrentCulture);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(MouseAction.WheelClick)]
        [InlineData(MouseAction.None)]
        public void Convert_PassedUnknownValueAndParameter_ShouldReturnNull(MouseAction input)
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
            Assert.Throws<NotImplementedException>(() => converter.ConvertBack(MouseAction.LeftClick, typeof(Visibility), null, CultureInfo.CurrentCulture));
        }

        #endregion
    }
}
