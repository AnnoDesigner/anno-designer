using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AnnoDesigner.Models
{
    public class ManageKeybindingsHotkeyDataTemplateSelector: DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var hotkey = item as Hotkey;
            FrameworkElement element = container as FrameworkElement;
            if (element != null && hotkey != null && hotkey.Binding is InputBinding)
            {
                if (hotkey.Binding is MouseBinding)
                {
                    return element.FindResource("MouseBinding") as DataTemplate;
                }
                else if (hotkey.Binding is KeyBinding)
                {
                    return element.FindResource("KeyBinding") as DataTemplate;
                }
            }
            return base.SelectTemplate(item, container);
        }
    }
}
