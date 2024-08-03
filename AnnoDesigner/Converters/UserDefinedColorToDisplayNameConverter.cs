using AnnoDesigner.Models;
using System;
using System.Globalization;
using System.Windows.Data;

namespace AnnoDesigner.Converters;

[ValueConversion(typeof(UserDefinedColor), typeof(string))]
public class UserDefinedColorToDisplayNameConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is not UserDefinedColor userDefinedColor ? value : userDefinedColor.DisplayName();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
