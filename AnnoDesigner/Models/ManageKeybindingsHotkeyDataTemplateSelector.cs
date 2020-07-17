﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AnnoDesigner.Core.Models;

namespace AnnoDesigner.Models
{
    public class ManageKeybindingsHotkeyDataTemplateSelector: DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (container is FrameworkElement element && item is Hotkey hotkey && hotkey.Binding is InputBinding binding && binding.Gesture is PolyGesture gesture)
            {
                if (gesture.Type == GestureType.MouseGesture)
                {
                    return element.FindResource("MouseBinding") as DataTemplate;
                }
                else if (gesture.Type == GestureType.KeyGesture)
                {
                    return element.FindResource("KeyBinding") as DataTemplate;
                }
            }
            return base.SelectTemplate(item, container);
        }
    }
}
