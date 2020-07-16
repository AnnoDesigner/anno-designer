using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AnnoDesigner.Models;
using AnnoDesigner.ViewModels;

namespace AnnoDesigner.PreferencesPages
{
    /// <summary>
    /// Interaction logic for GeneralSettingsPage.xaml
    /// </summary>
    public partial class GeneralSettingsPage : Page, INavigatedTo
    {
        public GeneralSettingsPage()
        {
            InitializeComponent();
        }

        public void NavigatedTo(object viewModel)
        {
            if (viewModel is GeneralSettingsViewModel vm)
            {
                DataContext = vm;
            }
        }
    }
}
