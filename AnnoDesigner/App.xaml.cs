using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace AnnoDesigner
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
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

            ReplaceUpdatedPresetFile();
        }

        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            LogErrorMessage(e.Exception);
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            if (ex != null) LogErrorMessage(ex);
        }

        /// <summary>
        /// Writes an exception to the error log
        /// </summary>
        /// <param name="e"></param>
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

        /// <summary>
        /// Writes a message to the error log.
        /// </summary>
        /// <param name="heading">The value for the heading in the error log.</param>
        /// <param name="message">The error message.</param>
        /// <param name="stackTrace">The stack trace for the error.</param>
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

        /// <summary>
        /// The DPI information for the current monitor.
        /// </summary>
        public static DpiScale DpiScale { get; set; }

        private void ReplaceUpdatedPresetFile()
        {
            try
            {
                var updateHelper = new UpdateHelper();
                var pathToUpdatedPresetsFile = updateHelper.PathToUpdatedPresetsFile;
                var pathToUpdatedPresetsAndIconsFile = updateHelper.PathToUpdatedPresetsAndIconsFile;
                if (!String.IsNullOrWhiteSpace(pathToUpdatedPresetsFile) && File.Exists(pathToUpdatedPresetsFile))
                {
                    var originalPathToPresetsFile = Path.Combine(App.ApplicationPath, Constants.BuildingPresetsFile);
                    File.Delete(originalPathToPresetsFile);
                    File.Move(pathToUpdatedPresetsFile, originalPathToPresetsFile);
                }
                else if (!String.IsNullOrWhiteSpace(pathToUpdatedPresetsAndIconsFile) && File.Exists(pathToUpdatedPresetsAndIconsFile))
                {
                    using (var archive = ZipFile.OpenRead(pathToUpdatedPresetsAndIconsFile))
                    {
                        foreach (var curEntry in archive.Entries)
                        {
                            curEntry.ExtractToFile(Path.Combine(ApplicationPath, curEntry.FullName), true);
                        }
                    }

                    //wait extra time for extraction to finish
                    Task.Delay(TimeSpan.FromMilliseconds(200)).GetAwaiter().GetResult();

                    File.Delete(pathToUpdatedPresetsAndIconsFile);
                }
            }
            catch (Exception ex)
            {
                WriteToErrorLog("error replacing updated presets file", ex.Message, ex.StackTrace);

                MessageBox.Show("Error installing update");
            }
        }
    }
}
