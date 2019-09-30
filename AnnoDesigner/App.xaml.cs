using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using AnnoDesigner.model;
using AnnoDesigner.viewmodel;
using NLog;
using NLog.Targets;

namespace AnnoDesigner
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly ICommons _commons;
        private static readonly IAppSettings _appSettings;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        static App()
        {
            _commons = Commons.Instance;
            _appSettings = new AppSettings();
        }

        public App()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                LogUnhandledException(e.ExceptionObject as Exception, "AppDomain.CurrentDomain.UnhandledException");

            DispatcherUnhandledException += (s, e) =>
                    LogUnhandledException(e.Exception, "Application.Current.DispatcherUnhandledException");

            TaskScheduler.UnobservedTaskException += (s, e) =>
                    LogUnhandledException(e.Exception, "TaskScheduler.UnobservedTaskException");

            logger.Info($"program version: {Assembly.GetExecutingAssembly().GetName().Version}");
        }

        private void LogUnhandledException(Exception ex, string @event)
        {
            logger.Error(ex, @event);

            ShowMessageWithUnexpectedErrorAndExit();
        }

        public static void ShowMessageWithUnexpectedErrorAndExit()
        {
            var message = "An unhandled exception occurred.";

            //find loaction of log file
            var fileTarget = LogManager.Configuration.FindTargetByName("MainLogger") as FileTarget;
            var logFile = fileTarget?.FileName.Render(new LogEventInfo());
            if (!string.IsNullOrWhiteSpace(logFile))
            {
                logFile = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), logFile);
                if (File.Exists(logFile))
                {
                    logFile = logFile.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
                    message += $"{Environment.NewLine}{Environment.NewLine}Details in \"{logFile}\".";
                }
            }

            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            Environment.Exit(-1);
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
                            logger.Trace($"Waiting for other processes to finish. Try {currentTry} of {maxTrys}");

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
                    _appSettings.Reload();

                    if (_appSettings.SettingsUpgradeNeeded)
                    {
                        _appSettings.Upgrade();
                        _appSettings.SettingsUpgradeNeeded = false;
                        _appSettings.Save();
                    }
                }
                catch (ConfigurationErrorsException ex)
                {
                    logger.Error(ex, "Error upgrading settings.");

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

                    _appSettings.Reload();
                }

                //var updateWindow = new UpdateWindow();                
                await _commons.UpdateHelper.ReplaceUpdatedPresetsFilesAsync();

                var mainVM = new MainViewModel(_commons, _appSettings);

                //TODO MainWindow.ctor calls AnnoCanvas.ctor loads presets -> change logic when to load data 
                MainWindow = new MainWindow();
                MainWindow.DataContext = mainVM;
                //MainWindow.Loaded += (s, args) => { updateWindow.Close(); };

                //updateWindow.Show();

                //If language is not recognized, bring up the language selection screen
                if (!Localization.Localization.LanguageCodeMap.ContainsKey(_appSettings.SelectedLanguage))
                {
                    var w = new Welcome();
                    w.DataContext = mainVM.WelcomeViewModel;
                    w.ShowDialog();
                }
                else
                {
                    _commons.SelectedLanguage = _appSettings.SelectedLanguage;
                }

                MainWindow.ShowDialog();

                //base.OnStartup(e);//needed?
            }
        }
    }
}
