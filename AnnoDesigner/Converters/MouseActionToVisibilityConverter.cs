using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace AnnoDesigner.Converters
{
    [ValueConversion(typeof(MouseAction), typeof(Visibility))]
    public class MouseActionToVisibilityConverter : IValueConverter
    {
        private static readonly List<int> validPositions = new List<int>() { 0, 1, 2 };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is MouseAction mouseAction))
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
                case MouseAction.LeftClick:
                case MouseAction.RightClick:
                case MouseAction.MiddleClick:
                    return position > 0 ? Visibility.Collapsed : Visibility.Visible;
                //If we're on the item in position 1 (the first mouse action), then show, else hide.
                case MouseAction.LeftDoubleClick:
                case MouseAction.RightDoubleClick:
                case MouseAction.MiddleDoubleClick:
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
