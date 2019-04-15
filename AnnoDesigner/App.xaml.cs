using System;
using System.IO;
using System.Reflection;
using System.Windows;

namespace AnnoDesigner
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
        : Application
    {
        public static string ExecutablePath
        {
            get
            {
                return Assembly.GetEntryAssembly().Location;
            }

        }

        public static string ApplicationPath
        {
            get
            {
                return Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            }
        }

        public static string FilenameArgument
        {
            get;
            private set;
        }


        private void AppOnStartup(object sender, StartupEventArgs e)
        {
            // retrieve file argument if given
            if (e.Args.Length > 0)
            {
                FilenameArgument = e.Args[0];
            }
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            App.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
        }

        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            LogErrorMessage(e.Exception);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            if (ex != null) LogErrorMessage(ex);
        }

        private void LogErrorMessage(Exception e)
        {
            try
            {
                MessageBox.Show("We have encountered a problem with the Application. Please check the error-log.txt file in the application directory for more information. \n\n Attempting to resume...", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                File.AppendAllText(App.ApplicationPath + "/error-log.txt", string.Format("\n\n***Application Error***\n\n Message: {0} \n Stack Trace:{1}\n\n", e.Message, e.StackTrace));
            }
            catch (Exception)
            {
                //Don't rethrow.
            }
        }
    }
}
