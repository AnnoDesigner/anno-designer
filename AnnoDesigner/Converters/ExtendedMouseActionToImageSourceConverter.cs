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
using AnnoDesigner.Core.Models;

namespace AnnoDesigner.Converters
{
    /// <summary>
    /// A converter used in the ManageKeybindings page to retrieve the correct image source based on the mouse action given.
    /// </summary>
    [ValueConversion(typeof(ExtendedMouseAction), typeof(ImageSource))]
    public class ExtendedMouseActionToImageSourceConverter : IValueConverter
    {
        public ExtendedMouseActionToImageSourceConverter()
        {
        }

        //see https://docs.microsoft.com/en-us/dotnet/framework/wpf/app-development/pack-uris-in-wpf
        private const string RESOURCE_ROOT_PATH = "pack://application:,,,/Images/Icons";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is ExtendedMouseAction mouseAction)) {
                return null;
            }
            return mouseAction switch
            {
                ExtendedMouseAction.LeftClick or ExtendedMouseAction.LeftDoubleClick => RESOURCE_ROOT_PATH + "/left-click.png",
                ExtendedMouseAction.RightClick or ExtendedMouseAction.RightDoubleClick => RESOURCE_ROOT_PATH + "/right-click.png",
                ExtendedMouseAction.MiddleClick or ExtendedMouseAction.MiddleDoubleClick => RESOURCE_ROOT_PATH + "/middle-click.png",
                _ => "",
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
