using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Models;
using NLog;
using MessageBox = Xceed.Wpf.Toolkit.MessageBox;

namespace AnnoDesigner.ViewModels
{
    public class UpdateSettingsViewModel : Notify
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly ICommons _commons;
        private readonly IAppSettings _appSettings;

        private bool _automaticUpdateCheck;
        private bool _updateSupportsPrerelease;
        private string _versionValue;
        private string _fileVersionValue;
        private string _presetsVersionValue;
        private bool _isUpdateAvailable;
        private bool _isPresetUpdateAvailable;
        private bool _isAppUpToDate;
        private bool _isUpdateError;
        private bool _isBusy;
        private string _busyContent;

        public UpdateSettingsViewModel(ICommons commonsToUse,
            IAppSettings appSettingsToUse)
        {
            _commons = commonsToUse;
            _appSettings = appSettingsToUse;

            CheckForUpdatesCommand = new RelayCommand(ExecuteCheckForUpdates);
            OpenReleasesCommand = new RelayCommand(ExecuteOpenReleases);
            DownloadPresetsCommand = new RelayCommand(ExecuteDownloadPresets);
        }

        private AvailableRelease FoundPresetRelease { get; set; }

        public async Task CheckForUpdates(bool isAutomaticUpdateCheck)
        {
            try
            {
                IsUpdateError = false;
                IsAppUpToDate = false;
                IsUpdateAvailable = false;
                IsPresetUpdateAvailable = false;

                BusyContent = Localization.Localization.Translations["UpdatePrefBusyCheckUpdates"];
                IsBusy = true;

                await CheckForNewAppVersionAsync(isAutomaticUpdateCheck);

                await CheckForPresetsAsync(isAutomaticUpdateCheck);

                if (!isAutomaticUpdateCheck)
                {
                    IsAppUpToDate = !IsUpdateAvailable && !IsPresetUpdateAvailable;
                }

                IsBusy = false;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error checking version.");

                IsUpdateError = true;

                IsBusy = false;

                if (isAutomaticUpdateCheck)
                {
                    MessageBox.Show(Application.Current.MainWindow,
                        $"Error checking version.{Environment.NewLine}{Environment.NewLine}More information is found in the log.",
                        "Version check failed",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private async Task CheckForNewAppVersionAsync(bool isAutomaticUpdateCheck)
        {
            var dowloadedContent = "0.1";
            using (var webClient = new WebClient())
            {
                dowloadedContent = await webClient.DownloadStringTaskAsync(new Uri("https://raw.githubusercontent.com/AnnoDesigner/anno-designer/master/version.txt"));
            }

            if (double.Parse(dowloadedContent, CultureInfo.InvariantCulture) > Constants.Version)
            {
                IsUpdateAvailable = true;
            }
            else
            {
                if (isAutomaticUpdateCheck)
                {
                    //show messagebox ?
                }
                else
                {
                    IsUpdateAvailable = false;
                    //StatusMessage = "Version is up to date.";
                }
            }

            if (isAutomaticUpdateCheck)
            {
                //If not already prompted
                if (!_appSettings.PromptedForAutoUpdateCheck)
                {
                    _appSettings.PromptedForAutoUpdateCheck = true;

                    if (MessageBox.Show(Application.Current.MainWindow,
                        "Do you want to continue checking for a new version on startup?\n\nThis option can be changed from the help menu.",
                        "Continue checking for updates?",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question) == MessageBoxResult.No)
                    {
                        AutomaticUpdateCheck = false;
                    }
                }
            }
        }

        private async Task CheckForPresetsAsync(bool isAutomaticUpdateCheck)
        {
            FoundPresetRelease = null;

            var foundRelease = await _commons.UpdateHelper.GetAvailableReleasesAsync(ReleaseType.Presets);
            if (foundRelease == null)
            {
                return;
            }

            var isNewReleaseAvailable = foundRelease.Version > new Version(PresetsVersionValue);
            if (isNewReleaseAvailable)
            {
                if (isAutomaticUpdateCheck)
                {
                    if (MessageBox.Show(Application.Current.MainWindow,
                        Localization.Localization.Translations["UpdateAvailablePresetMessage"],
                        Localization.Localization.Translations["UpdateAvailableHeader"],
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Asterisk,
                        MessageBoxResult.OK) == MessageBoxResult.Yes)
                    {
                        ExecuteDownloadPresets(null);
                    }
                }
                else
                {
                    IsPresetUpdateAvailable = true;
                    FoundPresetRelease = foundRelease;
                }
            }
        }

        public ICommand CheckForUpdatesCommand { get; private set; }

        private async void ExecuteCheckForUpdates(object param)
        {
            await CheckForUpdates(isAutomaticUpdateCheck: false);
        }

        public ICommand OpenReleasesCommand { get; private set; }

        private void ExecuteOpenReleases(object param)
        {
            Process.Start("https://github.com/AnnoDesigner/anno-designer/releases");
        }

        public ICommand DownloadPresetsCommand { get; private set; }

        private async void ExecuteDownloadPresets(object param)
        {
            if (FoundPresetRelease is null)
            {
                return;
            }

            BusyContent = Localization.Localization.Translations["UpdatePrefBusyDownloadPresets"];
            IsBusy = true;

            if (!_commons.CanWriteInFolder())
            {
                //already asked for admin rights?
                if (Environment.GetCommandLineArgs().Any(x => x.Trim().Equals(Constants.Argument_Ask_For_Admin, StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show($"You have no write access to the folder.{Environment.NewLine}The update can not be installed.",
                        Localization.Localization.Translations["Error"],
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    IsBusy = false;
                    return;
                }

                MessageBox.Show(Localization.Localization.Translations["UpdateRequiresAdminRightsMessage"],
                    Localization.Localization.Translations["AdminRightsRequired"],
                    MessageBoxButton.OK,
                    MessageBoxImage.Information,
                    MessageBoxResult.OK);

                _appSettings.Save();
                _commons.RestartApplication(true, Constants.Argument_Ask_For_Admin, App.ExecutablePath);
            }

            //Context is required here, do not use ConfigureAwait(false)
            var newLocation = await _commons.UpdateHelper.DownloadReleaseAsync(FoundPresetRelease);
            logger.Debug($"downloaded new preset ({FoundPresetRelease.Version}): {newLocation}");

            IsBusy = false;

            _appSettings.Save();
            _commons.RestartApplication(false, null, App.ExecutablePath);

            Environment.Exit(-1);
        }

        public bool AutomaticUpdateCheck
        {
            get { return _automaticUpdateCheck; }
            set
            {
                if (UpdateProperty(ref _automaticUpdateCheck, value))
                {
                    _appSettings.Save();
                }
            }
        }

        public string VersionValue
        {
            get { return _versionValue; }
            set { UpdateProperty(ref _versionValue, value); }
        }

        public string FileVersionValue
        {
            get { return _fileVersionValue; }
            set { UpdateProperty(ref _fileVersionValue, value); }
        }

        public string PresetsVersionValue
        {
            get { return _presetsVersionValue; }
            set { UpdateProperty(ref _presetsVersionValue, value); }
        }

        public bool UpdateSupportsPrerelease
        {
            get { return _updateSupportsPrerelease; }
            set
            {
                if (UpdateProperty(ref _updateSupportsPrerelease, value))
                {
                    _appSettings.Save();
                }
            }
        }

        public bool IsUpdateAvailable
        {
            get { return _isUpdateAvailable; }
            set { UpdateProperty(ref _isUpdateAvailable, value); }
        }

        public bool IsPresetUpdateAvailable
        {
            get { return _isPresetUpdateAvailable; }
            set { UpdateProperty(ref _isPresetUpdateAvailable, value); }
        }

        public bool IsAppUpToDate
        {
            get { return _isAppUpToDate; }
            set { UpdateProperty(ref _isAppUpToDate, value); }
        }

        public bool IsUpdateError
        {
            get { return _isUpdateError; }
            set { UpdateProperty(ref _isUpdateError, value); }
        }

        public bool IsBusy
        {
            get { return _isBusy; }
            set { UpdateProperty(ref _isBusy, value); }
        }

        public string BusyContent
        {
            get { return _busyContent; }
            set { UpdateProperty(ref _busyContent, value); }
        }
    }
}
