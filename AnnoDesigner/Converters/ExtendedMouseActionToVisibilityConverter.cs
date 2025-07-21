using AnnoDesigner.Core.Models;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AnnoDesigner.Converters;

[ValueConversion(typeof(ExtendedMouseAction), typeof(Visibility))]
public class ExtendedMouseActionToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is not ExtendedMouseAction mouseAction
            ? null
            : (object)(mouseAction switch
            {
                ExtendedMouseAction.LeftClick or ExtendedMouseAction.RightClick or ExtendedMouseAction.MiddleClick or ExtendedMouseAction.XButton1Click or ExtendedMouseAction.XButton2Click or ExtendedMouseAction.WheelClick => Visibility.Collapsed,
                ExtendedMouseAction.LeftDoubleClick or ExtendedMouseAction.RightDoubleClick or ExtendedMouseAction.MiddleDoubleClick => Visibility.Visible,
                _ => null,
            });
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
