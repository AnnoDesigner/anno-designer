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
            get { return Assembly.GetEntryAssembly().Location; }

        }

        public static string ApplicationPath
        {
            get { return Path.GetDirectoryName(Assembly.GetEntryAssembly().Location); }
        }

        public static string FilenameArgument { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            // retrieve file argument if given
            if (e.Args.Length > 0)
            {
                FilenameArgument = e.Args[0];
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
                            //File.AppendAllText(Path.Combine(App.ApplicationPath, $"activity-{Process.GetCurrentProcess().Id}.txt"), $"Waiting for other processes to finish.Try { currentTry} of { maxTrys}{Environment.NewLine}");

                            createdNewMutex = mutexAnnoDesigner.WaitOne(TimeSpan.FromSeconds(1), true);
                            currentTry++;
                        }

                        if (!createdNewMutex)
                        {
                            MessageBox.Show($"Another instance of the app is already running.");
                            Environment.Exit(-1);
                        }
                    }
                    catch (AbandonedMutexException ex)
                    {
                        //mutex was killed
                        createdNewMutex = true;
                    }
                }

                //File.AppendAllText(Path.Combine(App.ApplicationPath, $"activity-{Process.GetCurrentProcess().Id}.txt"), $"start cleanup{Environment.NewLine}");
                ReplaceUpdatedPresetFile();

                MainWindow = new MainWindow();
                MainWindow.ShowDialog();
            }
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
                ////wait for old process to finish
                //while (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1)
                //{
                //    //File.AppendAllText(Path.Combine(App.ApplicationPath, $"activity-{Process.GetCurrentProcess().Id}.txt"), $"another process is running{Environment.NewLine}");
                //    Task.Delay(TimeSpan.FromMilliseconds(100)).GetAwaiter().GetResult();
                //}

                ////File.AppendAllText(Path.Combine(App.ApplicationPath, $"activity-{Process.GetCurrentProcess().Id}.txt"), $"no other processese are running{Environment.NewLine}");

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
                    //File.AppendAllText(Path.Combine(App.ApplicationPath, $"activity-{Process.GetCurrentProcess().Id}.txt"), $"start extraction{Environment.NewLine}");

                    using (var archive = ZipFile.OpenRead(pathToUpdatedPresetsAndIconsFile))
                    {
                        foreach (var curEntry in archive.Entries)
                        {
                            curEntry.ExtractToFile(Path.Combine(ApplicationPath, curEntry.FullName), true);
                        }
                    }

                    //File.AppendAllText(Path.Combine(App.ApplicationPath, $"activity-{Process.GetCurrentProcess().Id}.txt"), $"end extratcion{Environment.NewLine}");

                    //wait extra time for extraction to finish
                    Task.Delay(TimeSpan.FromMilliseconds(200)).GetAwaiter().GetResult();

                    //File.AppendAllText(Path.Combine(App.ApplicationPath, $"activity-{Process.GetCurrentProcess().Id}.txt"), $"delete zip{Environment.NewLine}");
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
