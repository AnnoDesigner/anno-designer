using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using AnnoDesigner.Models;

namespace AnnoDesigner.Converters
{
    [ValueConversion(typeof(UserDefinedColor), typeof(string))]
    public class UserDefinedColorToDisplayNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is UserDefinedColor userDefinedColor))
            {
                return value;
            }

            return userDefinedColor.DisplayName();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
