using ColorPresetsDesigner.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ColorPresetsDesigner
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                logUnhandledException((Exception)e.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException");

            DispatcherUnhandledException += (s, e) =>
                logUnhandledException(e.Exception, "Application.Current.DispatcherUnhandledException");

            TaskScheduler.UnobservedTaskException += (s, e) =>
                logUnhandledException(e.Exception, "TaskScheduler.UnobservedTaskException");
        }

        private void logUnhandledException(Exception ex, string @event)
        {
            var errorMessage = string.Format("{3:o}{0}An unhandled exception occurred:{0}{1}{0}{2}{0}", Environment.NewLine, @event, ex, DateTime.UtcNow);

            Trace.WriteLine(errorMessage);

            var logFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "error.txt");
            File.AppendAllText(logFilePath, errorMessage, Encoding.UTF8);

            MessageBox.Show($"An unhandled exception occurred.{Environment.NewLine}Details in \"{logFilePath}\".", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            Environment.Exit(-1);
        }

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
