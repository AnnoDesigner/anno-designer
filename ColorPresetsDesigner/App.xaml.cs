using ColorPresetsDesigner.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ColorPresetsDesigner
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (ColorPresetsDesigner.Properties.Settings.Default.SettingsUpgradeNeeded)
            {
                ColorPresetsDesigner.Properties.Settings.Default.Upgrade();
                ColorPresetsDesigner.Properties.Settings.Default.SettingsUpgradeNeeded = false;
                ColorPresetsDesigner.Properties.Settings.Default.Save();
            }

            var mainVM = new MainWindowViewModel();

            MainWindow = new MainWindow();
            MainWindow.DataContext = mainVM;

            MainWindow.ShowDialog();
        }
    }
}
