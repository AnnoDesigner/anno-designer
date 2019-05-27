using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AnnoDesigner.Core.Converter;
using Xunit;

namespace AnnoDesigner.Core.Tests
{
    public class BoolToVisibilityConverterTests
    {
        #region ctor tests

        [Fact]
        public void ctor_Defaults_Set()
        {
            // Arrange/Act
            var converter = new BoolToVisibilityConverter();

            // Assert
            Assert.Equal(Visibility.Visible, converter.TrueValue);
            Assert.Equal(Visibility.Collapsed, converter.FalseValue);
        }

        #endregion

        #region Convert tests

        [Fact]
        public void Convert_PassedTrue_ShouldReturnVisible()
        {
            // Arrange/Act
            var converter = new BoolToVisibilityConverter();

            // Act
            var result = converter.Convert(true, typeof(Visibility), null, CultureInfo.CurrentCulture);

            // Assert
            Assert.Equal(Visibility.Visible, result);
        }

        [Fact]
        public void Convert_PassedFalse_ShouldReturnCollapsed()
        {
            // Arrange/Act
            var converter = new BoolToVisibilityConverter();

            // Act
            var result = converter.Convert(false, typeof(Visibility), null, CultureInfo.CurrentCulture);

            // Assert
            Assert.Equal(Visibility.Collapsed, result);
        }

        [Fact]
        public void Convert_ChangedFalsValue_ShouldReturnSetValue()
        {
            // Arrange/Act
            var converter = new BoolToVisibilityConverter();
            converter.FalseValue = Visibility.Hidden;

            // Act
            var result = converter.Convert(false, typeof(Visibility), null, CultureInfo.CurrentCulture);

            // Assert
            Assert.Equal(Visibility.Hidden, result);
        }

        [Fact]
        public void Convert_PassedUnknownValue_ShouldReturnNull()
        {
            // Arrange/Act
            var converter = new BoolToVisibilityConverter();

            // Act
            var result = converter.Convert(String.Empty, typeof(Visibility), null, CultureInfo.CurrentCulture);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region ConvertBack tests

        [Fact]
        public void ConvertBack_PassedTrueValue_ShouldReturnTrue()
        {
            // Arrange/Act
            var converter = new BoolToVisibilityConverter();

            // Act
            var result = (bool)converter.ConvertBack(Visibility.Visible, typeof(bool), null, CultureInfo.CurrentCulture);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ConvertBack_PassedFalseValue_ShouldReturnFalse()
        {
            // Arrange/Act
            var converter = new BoolToVisibilityConverter();

            // Act
            var result = (bool)converter.ConvertBack(Visibility.Collapsed, typeof(bool), null, CultureInfo.CurrentCulture);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ConvertBack_PassedUnknownValue_ShouldReturnNull()
        {
            // Arrange/Act
            var converter = new BoolToVisibilityConverter();

            // Act
            var result = converter.ConvertBack(Visibility.Hidden, typeof(bool), null, CultureInfo.CurrentCulture);

            // Assert
            Assert.Null(result);
        }

        #endregion
    }
}
