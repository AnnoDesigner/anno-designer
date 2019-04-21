using System;
using System.Diagnostics;
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
                MessageBox.Show("We have encountered a problem with the application. Please check the error-log.txt file in the application directory for more information. \n\n Attempting to resume...", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                WriteToErrorLog("Application Unhandled Error", e.Message, e.StackTrace);
            }
            catch (Exception)
            {
                //Don't rethrow.
            }
        }

        public static void WriteToErrorLog(string heading, string message, string stackTrace)
        {
            try
            {
                File.AppendAllText(App.ApplicationPath + "/error-log.txt", string.Format("\n\n*** {0} *** {1} \n\nMessage: {2} \n Stack Trace:\n{3}\n\n", heading, DateTime.Now.ToString("yyyy-MM-dd"), message, stackTrace));
            }
            catch (Exception)
            {
                //Don't rethrow.
            }
        }
    }
}
