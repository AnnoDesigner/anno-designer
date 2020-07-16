using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using AnnoDesigner.Core.Models;

namespace AnnoDesigner.Converters
{
    [ValueConversion(typeof(ExtendedMouseAction), typeof(Visibility))]
    public class ExtendedMouseActionToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is ExtendedMouseAction mouseAction))
            {
                return null;
            }

            switch (mouseAction)
            {
                case ExtendedMouseAction.LeftClick:
                case ExtendedMouseAction.RightClick:
                case ExtendedMouseAction.MiddleClick:
                case ExtendedMouseAction.XButton1Click:
                case ExtendedMouseAction.XButton2Click:
                case ExtendedMouseAction.WheelClick:
                    return Visibility.Collapsed;
                case ExtendedMouseAction.LeftDoubleClick:
                case ExtendedMouseAction.RightDoubleClick:
                case ExtendedMouseAction.MiddleDoubleClick:
                    return Visibility.Visible;
                default:
                    return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
