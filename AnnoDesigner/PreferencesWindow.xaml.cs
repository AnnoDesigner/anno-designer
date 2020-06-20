using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using AnnoDesigner.Core.Models;
using System.Collections.ObjectModel;
using AnnoDesigner.ViewModels;
using AnnoDesigner.PreferencesPages;
using AnnoDesigner.Models;

namespace AnnoDesigner
{
    /// <summary>
    /// Interaction logic for Preferences.xaml
    /// </summary>
    public partial class PreferencesWindow : Window
    {
        public PreferencesWindow(IAppSettings appSettings, HotkeyCommandManager commandManager)
        {
            InitializeComponent();
            DataContext = new PreferencesViewModel(appSettings, commandManager, CurrentPage.NavigationService);
            Loaded += Preferences_Loaded;
        }

        private void Preferences_Loaded(object sender, RoutedEventArgs e)
        {
            CurrentPage.Navigated += CurrentPage_Navigated;
        }

        private void CurrentPage_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (e.Content is INavigatedTo page)
            {
                page.NavigatedTo(e.ExtraData);
            }
        }
    }
}
