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
        private static readonly List<int> validPositions = new List<int>() { 0, 1, 2 };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is ExtendedMouseAction mouseAction))
            {
                return null;
            }

            if (!(parameter is int position))
            {
                Debug.Fail("No position specified in MouseActionToVisibilityConverter.");
                Debug.Assert(false); //error here when running in debug mode.
                return null;
            }

            if (!validPositions.Contains(position))
            {
                Debug.Fail($"Specified position {position} is not valid in MouseActionToVisibilityConverter.");
                Debug.Assert(false); //error here when running in debug mode.
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
                    return position > 0 ? Visibility.Collapsed : Visibility.Visible;
                //If we're on the item in position 1 (the first mouse action), then show, else hide.
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
