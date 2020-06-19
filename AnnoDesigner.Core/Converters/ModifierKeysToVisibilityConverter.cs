using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace AnnoDesigner.Core.Converters
{
    [ValueConversion(typeof(ModifierKeys), typeof(Visibility))]
    public class ModifierKeysToVisibilityConverter : IValueConverter
    {
        public ModifierKeysToVisibilityConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ModifierKeys v;
            if (value is ModifierKeys modifiers)
            {
                v = modifiers;
            }
            else if (parameter is ModifierKeys modifiers2)
            {
                v = modifiers2;
            }
            else
            {
                return null;
            }

            return (v == ModifierKeys.None) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
