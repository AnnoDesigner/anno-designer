﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Navigation;

namespace AnnoDesigner.Converters
{
    [ValueConversion(typeof(MouseAction), typeof(Visibility))]
    public class MouseActionToVisibilityConverter : IValueConverter
    {
        public MouseActionToVisibilityConverter()
        {
        }

        //see https://docs.microsoft.com/en-us/dotnet/framework/wpf/app-development/pack-uris-in-wpf
        private const string RESOURCE_ROOT_PATH = "pack://application:,,,Images/Icons";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is MouseAction mouseAction)) {
                return null;
            }
            if (!(parameter is int position))
            {
                Debug.Fail("No position specified in MouseActionToVisibilityConverter.");
                Debug.Assert(false); //error here when running in debug mode.
                return null;
            }
            List<int> validPositions = new List<int>() { 0, 1, 2 };
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
