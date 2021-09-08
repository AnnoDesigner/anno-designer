using System;
using System.Configuration;
using System.IO;
using System.IO.Abstractions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using AnnoDesigner.Core.Extensions;
using AnnoDesigner.Core.Helper;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Core.Presets.Loader;
using AnnoDesigner.Core.RecentFiles;
using AnnoDesigner.Core.Services;
using AnnoDesigner.Models;
using AnnoDesigner.Services;
using AnnoDesigner.ViewModels;
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
        private static readonly IMessageBoxService _messageBoxService;
        private static readonly ILocalizationHelper _localizationHelper;
        private static readonly IUpdateHelper _updateHelper;
        private static readonly IFileSystem _fileSystem;

        static App()
        {
            _commons = Commons.Instance;
            _appSettings = AppSettings.Instance;
            _messageBoxService = new MessageBoxService();

            Localization.Localization.Init(_commons);
            _localizationHelper = Localization.Localization.Instance;

            _updateHelper = new UpdateHelper(ApplicationPath, _appSettings, _messageBoxService, _localizationHelper);
            _fileSystem = new FileSystem();
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

            _messageBoxService.ShowError(message);

            Environment.Exit(-1);
        }

        private static string _executablePath;
        public static string ExecutablePath
        {
            get
            {
                if (_executablePath is null)
                {
                    _executablePath = Assembly.GetEntryAssembly().Location;
                }
                return _executablePath;
            }
        }

        private static string _applicationPath;
        public static string ApplicationPath
        {
            get
            {
                if (_applicationPath is null)
                {
                    _applicationPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                }
                return _applicationPath;
            }
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

            using var mutexAnnoDesigner = new Mutex(true, MutexHelper.MUTEX_ANNO_DESIGNER, out var createdNewMutex);
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
                        _messageBoxService.ShowMessage(Localization.Localization.Instance.GetLocalization("AnotherInstanceIsAlreadyRunning"));
                        Environment.Exit(-1);
                    }
                }
                catch (AbandonedMutexException)
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

                _messageBoxService.ShowError(_localizationHelper.GetLocalization("ErrorUpgradingSettings"));

                var fileName = "";
                if (!string.IsNullOrEmpty(ex.Filename))
                {
                    fileName = ex.Filename;
                }
                else
                {
                    if (ex.InnerException is ConfigurationErrorsException innerException && !string.IsNullOrEmpty(innerException.Filename))
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
            await _updateHelper.ReplaceUpdatedPresetsFilesAsync();

            var recentFilesSerializer = new RecentFilesAppSettingsSerializer(_appSettings);

            IRecentFilesHelper recentFilesHelper = new RecentFilesHelper(recentFilesSerializer, _fileSystem);
            ITreeLocalizationLoader treeLocalizationLoader = new TreeLocalizationLoader(_fileSystem);

            var mainVM = new MainViewModel(_commons, _appSettings, recentFilesHelper, _messageBoxService, _updateHelper, _localizationHelper, _fileSystem, treeLocalizationLoader: treeLocalizationLoader);

            //TODO MainWindow.ctor calls AnnoCanvas.ctor loads presets -> change logic when to load data 
            MainWindow = new MainWindow(_appSettings);
            MainWindow.DataContext = mainVM;

            //If language is not recognized, bring up the language selection screen
            if (!_commons.LanguageCodeMap.ContainsKey(_appSettings.SelectedLanguage))
            {
                var w = new Welcome();
                w.DataContext = mainVM.WelcomeViewModel;
                w.ShowDialog();
            }
            else
            {
                _commons.CurrentLanguage = _appSettings.SelectedLanguage;
            }

            MainWindow.ShowDialog();
        }
    }
}
