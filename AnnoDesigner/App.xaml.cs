using System;
using System.CommandLine;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using AnnoDesigner.CommandLine;
using AnnoDesigner.CommandLine.Arguments;
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
        private static readonly IArgumentParser _argumentParser;
        private static readonly IConsole _console;

        public new MainWindow MainWindow { get => base.MainWindow as MainWindow; set => base.MainWindow = value; }

        static App()
        {
            _commons = Commons.Instance;
            _appSettings = AppSettings.Instance;
            _messageBoxService = new MessageBoxService();

            Localization.Localization.Init(_commons);
            _localizationHelper = Localization.Localization.Instance;

            _updateHelper = new UpdateHelper(ApplicationPath, _appSettings, _messageBoxService, _localizationHelper);
            _fileSystem = new FileSystem();

            _console = new ConsoleManager.LazyConsole();
            _argumentParser = new ArgumentParser(_console, _fileSystem);
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

            ShowMessageWithUnexpectedError(false);
            MainWindow.DataContext.AnnoCanvas.CheckUnsavedChangesBeforeCrash();

            Environment.Exit(-1);
        }

        public static void ShowMessageWithUnexpectedError(bool exitProgram = true)
        {
            var message = "An unhandled exception occurred.";

            //find location of log file
            FileTarget? fileTarget = (FileTarget)LogManager.Configuration.FindTargetByName("MainLogger");
            if (fileTarget is null)
            {
                _messageBoxService.ShowError("Not Found");

            }
            var logFile = fileTarget.FileName.Render(new LogEventInfo());
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

            if (exitProgram)
            {
                Environment.Exit(-1);
            }
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

        public static IProgramArgs StartupArguments { get; private set; }

        /// <summary>
        /// The DPI information for the current monitor.
        /// </summary>
        public static DpiScale DpiScale { get; set; }

        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            StartupArguments = _argumentParser.Parse(e.Args);
            if (StartupArguments is null)
            {
                ConsoleManager.Show();
                if (ConsoleManager.StartedWithoutConsole)
                {
                    Console.WriteLine("Press enter to exit");
                    Console.ReadLine();
                }
                ConsoleManager.Hide();
                Environment.Exit(0);
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

            var anotherInstanceIsRunning = IsAnotherInstanceRunning();
            if (anotherInstanceIsRunning && _appSettings.ShowMultipleInstanceWarning && await _updateHelper.AreUpdatedPresetsFilesPresentAsync())
            {
                //prevent app from closing, because there is no main window yet
                var previousShutdownMode = Application.Current.ShutdownMode;
                Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

                //inform user that auto update is not applied
                _messageBoxService.ShowMessage(Localization.Localization.Instance.GetLocalization("WarningMultipleInstancesAreRunning"));

                Application.Current.ShutdownMode = previousShutdownMode;
            }

            if (!anotherInstanceIsRunning)
            {
                //var updateWindow = new UpdateWindow();                
                await _updateHelper.ReplaceUpdatedPresetsFilesAsync();
            }

            var recentFilesSerializer = new RecentFilesAppSettingsSerializer(_appSettings);

            IRecentFilesHelper recentFilesHelper = new RecentFilesHelper(recentFilesSerializer, _fileSystem, _appSettings.MaxRecentFiles);
            ITreeLocalizationLoader treeLocalizationLoader = new TreeLocalizationLoader(_fileSystem);

            var mainVM = new MainViewModel(_commons, _appSettings, recentFilesHelper, _messageBoxService, _updateHelper, _localizationHelper, _fileSystem, treeLocalizationLoader: treeLocalizationLoader);
            mainVM.UpdateRegisteredExtension();

            //TODO MainWindow.ctor calls AnnoCanvas.ctor loads presets -> change logic when to load data 
            MainWindow = new MainWindow(_appSettings);
            MainWindow.DataContext = mainVM;

            //language already set -> apply selected language
            if (_commons.LanguageCodeMap.ContainsKey(_appSettings.SelectedLanguage))
            {
                _commons.CurrentLanguage = _appSettings.SelectedLanguage;
            }
            else
            {
                //normal start and language not set -> show language selection
                if (StartupArguments is not ExportArgs)
                {
                    var w = new Welcome();
                    w.DataContext = mainVM.WelcomeViewModel;
                    w.ShowDialog();
                }
                //started via command line and language is not set -> set default language
                else
                {
                    _commons.CurrentLanguage = "English";
                }
            }

            MainWindow.ShowDialog();
        }

        private static bool IsAnotherInstanceRunning()
        {
            Process currentProcess = Process.GetCurrentProcess();
            var currentFileLocation = currentProcess.MainModule.FileName;

            var runningProcesses = from process in Process.GetProcessesByName(currentProcess.ProcessName)
                                   where process.Id != currentProcess.Id &&
                                   string.Equals(process.MainModule.FileName, currentFileLocation, StringComparison.OrdinalIgnoreCase)
                                   select process;

            return runningProcesses.Any();
        }
    }
}
