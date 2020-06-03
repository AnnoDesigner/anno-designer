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
using System.Windows.Media;

namespace AnnoDesigner.Converters
{
    [ValueConversion(typeof(MouseAction), typeof(ImageSource))]
    public class MouseActionToImageSourceConverter : IValueConverter
    {
        public MouseActionToImageSourceConverter()
        {
        }

        //see https://docs.microsoft.com/en-us/dotnet/framework/wpf/app-development/pack-uris-in-wpf
        private const string RESOURCE_ROOT_PATH = "pack://application:,,,/Images/Icons";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is MouseAction mouseAction)) {
                return null;
            }
            var i = new ImageSourceConverter();
            switch (mouseAction)
            {
                case MouseAction.LeftClick:
                case MouseAction.LeftDoubleClick:
                    //return i.ConvertFromInvariantString(RESOURCE_ROOT_PATH + "/left-click.png");
                    return RESOURCE_ROOT_PATH + "/left-click.png";
                case MouseAction.RightClick:
                case MouseAction.RightDoubleClick:
                    //return i.ConvertFromInvariantString(RESOURCE_ROOT_PATH + "/right-click.png");
                    return RESOURCE_ROOT_PATH + "/right-click.png";
                case MouseAction.MiddleClick:
                case MouseAction.MiddleDoubleClick:
                    //return i.ConvertFromInvariantString(RESOURCE_ROOT_PATH + "/middle-click.png");
                    return RESOURCE_ROOT_PATH + "/middle-click.png";
                default:
                    return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
