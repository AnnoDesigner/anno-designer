using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace AnnoDesigner.Core.Converters;

[ValueConversion(typeof(ModifierKeys), typeof(Visibility))]
public class ModifierKeysToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        ModifierKeys localValue;

        if (value is ModifierKeys modifiers)
        {
            localValue = modifiers;
        }
        else if (parameter is ModifierKeys modifiers2)
        {
            localValue = modifiers2;
        }
        else
        {
            return null;
        }

        return localValue == ModifierKeys.None ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
