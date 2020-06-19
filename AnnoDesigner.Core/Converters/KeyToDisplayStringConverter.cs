using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using AnnoDesigner.Core.Helper;

namespace AnnoDesigner.Core.Converters
{
    /// <summary>
    /// Retrieves the value that a <see cref="Key"/> value represents on a users keyboard (OEM and culture specific).
    /// For example, <see cref="Key.Oem1"/>  would return <c>";"</c> on a UK keyboard.
    /// </summary>
    [ValueConversion(typeof(Key), typeof(string))]
    public class KeyToDisplayStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Key key)
            {
                var s = KeyboardInteropHelper.GetDisplayString(key);
                return string.IsNullOrWhiteSpace(s) ? key.ToString() : s;
            }
#if DEBUG
            throw new ArgumentException("value is not a `Key`!", "value");
#else
            return "";
#endif
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
