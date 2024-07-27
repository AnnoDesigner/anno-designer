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

            return mouseAction switch
            {
                ExtendedMouseAction.LeftClick or ExtendedMouseAction.RightClick or ExtendedMouseAction.MiddleClick or ExtendedMouseAction.XButton1Click or ExtendedMouseAction.XButton2Click or ExtendedMouseAction.WheelClick => Visibility.Collapsed,
                ExtendedMouseAction.LeftDoubleClick or ExtendedMouseAction.RightDoubleClick or ExtendedMouseAction.MiddleDoubleClick => Visibility.Visible,
                _ => null,
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
