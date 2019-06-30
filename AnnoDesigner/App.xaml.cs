using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using AnnoDesigner.model;

namespace AnnoDesigner
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly ICommons _commons;

        static App()
        {
            _commons = Commons.Instance;
        }

        public static string ExecutablePath
        {
            get { return Assembly.GetEntryAssembly().Location; }

        }

        public static string ApplicationPath
        {
            get { return Path.GetDirectoryName(Assembly.GetEntryAssembly().Location); }
        }

        public static string FilenameArgument { get; private set; }

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
        public static void LogErrorMessage(Exception e)
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

        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            // retrieve file argument if given
            if (e.Args.Length > 0)
            {
                if (!e.Args[0].Equals(Constants.Argument_Ask_For_Admin, StringComparison.OrdinalIgnoreCase))
                {
                    FilenameArgument = e.Args[0];
                }
            }

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            App.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;

            using (var mutexAnnoDesigner = new Mutex(true, MutexHelper.MUTEX_ANNO_DESIGNER, out bool createdNewMutex))
            {
                //Are there other processes still running?
                if (!createdNewMutex)
                {
                    try
                    {
                        var currentTry = 0;
                        const int maxTrys = 10;
                        while (!createdNewMutex && currentTry < maxTrys)
                        {
                            Trace.WriteLine($"Waiting for other processes to finish. Try {currentTry} of {maxTrys}");

                            createdNewMutex = mutexAnnoDesigner.WaitOne(TimeSpan.FromSeconds(1), true);
                            currentTry++;
                        }

                        if (!createdNewMutex)
                        {
                            MessageBox.Show("Another instance of the app is already running.");
                            Environment.Exit(-1);
                        }
                    }
                    catch (AbandonedMutexException ex)
                    {
                        //mutex was killed
                        createdNewMutex = true;
                    }
                }

                try
                {
                    //check if file is not corrupt
                    AnnoDesigner.Properties.Settings.Default.Reload();

                    if (AnnoDesigner.Properties.Settings.Default.SettingsUpgradeNeeded)
                    {
                        AnnoDesigner.Properties.Settings.Default.Upgrade();
                        AnnoDesigner.Properties.Settings.Default.SettingsUpgradeNeeded = false;
                        AnnoDesigner.Properties.Settings.Default.Save();
                    }
                }
                catch (ConfigurationErrorsException ex)
                {
                    MessageBox.Show("The settings file has become corrupted. We must reset your settings.",
                          "Error",
                          MessageBoxButton.OK,
                          MessageBoxImage.Error);

                    var fileName = "";
                    if (!string.IsNullOrEmpty(ex.Filename))
                    {
                        fileName = ex.Filename;
                    }
                    else
                    {
                        var innerException = ex.InnerException as ConfigurationErrorsException;
                        if (innerException != null && !string.IsNullOrEmpty(innerException.Filename))
                        {
                            fileName = innerException.Filename;
                        }
                    }

                    if (File.Exists(fileName))
                    {
                        File.Delete(fileName);
                    }

                    AnnoDesigner.Properties.Settings.Default.Reload();
                }

                //var updateWindow = new UpdateWindow();                
                await _commons.UpdateHelper.ReplaceUpdatedPresetsFilesAsync();

                //TODO MainWindow.ctor calls AnnoCanvas.ctor loads presets -> change logic when to load data 
                MainWindow = new MainWindow(_commons);
                //MainWindow.Loaded += (s, args) => { updateWindow.Close(); };

                //updateWindow.Show();

                MainWindow.ShowDialog();

                //base.OnStartup(e);//needed?
            }
        }
    }
}
