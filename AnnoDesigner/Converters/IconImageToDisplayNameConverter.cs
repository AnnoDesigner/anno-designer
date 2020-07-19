using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using AnnoDesigner.Core.Models;

namespace AnnoDesigner.Converters
{
    [ValueConversion(typeof(IconImage), typeof(string))]
    public class IconImageToDisplayNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is IconImage iconImage))
            {
                return value;
            }

            return iconImage.NameForLanguage(Commons.Instance.CurrentLanguageCode);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
