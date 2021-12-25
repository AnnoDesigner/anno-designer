using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AnnoDesigner.Core.Converters
{
    [ValueConversion(typeof(Enum), typeof(bool))]
    public sealed class FlagToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Enum e && parameter is Enum p)
            {
                return e.HasFlag(p);
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
