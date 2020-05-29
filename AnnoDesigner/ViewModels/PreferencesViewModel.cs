using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Models;
using NLog;

namespace AnnoDesigner.ViewModels
{
    public class PreferencesViewModel : Notify
    {
        public PreferencesViewModel(ICommons commons, IAppSettings appSettings, HotkeyCommandManager manager, Frame navigator)
        {
            this.commons = commons;
            this.appSettings = appSettings;
            Manager = manager;
            this.navigator = navigator;
        }

        private HotkeyCommandManager _manager;
        public HotkeyCommandManager Manager
        {
            get { return _manager; }
            set { UpdateProperty(ref _manager, value); }
        }
        private Frame navigator;
        private ICommons commons;
        private IAppSettings appSettings;

        private ListViewItem _selectedItem;
        public ListViewItem SelectedItem
        {
            get { return _selectedItem; }
            set 
            { 
                UpdateProperty(ref _selectedItem, value);
                //var t = Type.GetType($"AnnoDesigner.PreferencesPages.{value.Name}");
                //var page = Activator.CreateInstance(t, manager);
                //navigator.Navigate(page);
                navigator.Navigate(new Uri($@"pack://application:,,,/PreferencesPages\{value.Name}.xaml", UriKind.RelativeOrAbsolute), value.DataContext);
            }
        }

        
    }


}
