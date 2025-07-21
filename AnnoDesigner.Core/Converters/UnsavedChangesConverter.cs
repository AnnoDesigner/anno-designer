using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AnnoDesigner.Core.Converters;

public sealed class UnsavedChangesConverter : IMultiValueConverter
{
    public string Format { get; set; } = "{0} *";

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 1)
        {
            return DependencyProperty.UnsetValue;
        }

        if (values.Length < 2 || values[1] is false)
        {
            return values[0];
        }

        return string.Format(Format, values[0] as string);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
