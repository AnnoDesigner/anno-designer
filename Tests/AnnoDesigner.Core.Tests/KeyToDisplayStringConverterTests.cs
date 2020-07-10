using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AnnoDesigner.Core.Converters;
using Xunit;

namespace AnnoDesigner.Core.Tests
{
    public class KeyToDisplayStringConverterTests
    {
        #region Convert tests

        [Theory]
        [InlineData(Key.A, "A")]
        [InlineData(Key.Space, "Space")]
        [InlineData(Key.LeftAlt, "LeftAlt")]
        [InlineData(Key.Delete, "Delete")]
        public void Convert_PassedKnownValue_ShouldReturnCorrectValue(Key input, string expected)
        {
            // Arrange
            var converter = new KeyToDisplayStringConverter();

            // Act
            var result = converter.Convert(input, typeof(string), null, CultureInfo.CurrentCulture);

            // Assert
            Assert.Equal(expected, result);
        }

#if DEBUG
        [Fact]
        public void Convert_PassedUnknownValue_ShouldThrow()
        {
            // Arrange
            var converter = new KeyToDisplayStringConverter();

            // Act/Assert
            Assert.Throws<ArgumentException>(() => converter.Convert("dummy", typeof(string), null, CultureInfo.CurrentCulture));
        }
#else
        [Fact]
        public void Convert_PassedUnknownValue_ShouldReturnEmpty()
        {
            // Arrange
            var converter = new KeyToDisplayStringConverter();

            // Act
            var result = converter.Convert("dummy", typeof(string), null, CultureInfo.CurrentCulture);

            // Assert
            Assert.Equal(string.Empty, result);
        }
#endif

        #endregion

        #region ConvertBack tests

        [Fact]
        public void ConvertBack_PassedAnyValue_ShouldThrow()
        {
            // Arrange
            var converter = new KeyToDisplayStringConverter();

            // Act/Assert
            Assert.Throws<NotImplementedException>(() => converter.ConvertBack(Key.A, typeof(string), null, CultureInfo.CurrentCulture));
        }

        #endregion
    }
}
