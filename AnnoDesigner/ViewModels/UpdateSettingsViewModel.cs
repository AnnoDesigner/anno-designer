using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AnnoDesigner.CommandLine.Arguments;
using AnnoDesigner.Core.Extensions;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Core.Services;
using AnnoDesigner.Models;
using NLog;

namespace AnnoDesigner.ViewModels
{
    public class UpdateSettingsViewModel : Notify
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly ICommons _commons;
        private readonly IAppSettings _appSettings;
        private readonly IMessageBoxService _messageBoxService;
        private readonly IUpdateHelper _updateHelper;
        private readonly ILocalizationHelper _localizationHelper;

        private bool _automaticUpdateCheck;
        private bool _updateSupportsPrerelease;
        private string _versionValue;
        private string _updatedVersionValue;
        private string _fileVersionValue;
        private string _presetsVersionValue;
        private string _colorPresetsVersionValue;
        private string _treeLocalizationVersionValue;
        private bool _isUpdateAvailable;
        private bool _isPresetUpdateAvailable;
        private bool _isAppUpToDate;
        private bool _isUpdateError;
        private bool _isBusy;
        private string _busyContent;

        public UpdateSettingsViewModel(ICommons commonsToUse,
            IAppSettings appSettingsToUse,
            IMessageBoxService messageBoxServiceToUse,
            IUpdateHelper updateHelperToUse,
            ILocalizationHelper localizationHelperToUse)
        {
            _commons = commonsToUse;
            _appSettings = appSettingsToUse;
            _messageBoxService = messageBoxServiceToUse;
            _updateHelper = updateHelperToUse;
            _localizationHelper = localizationHelperToUse;

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

                BusyContent = _localizationHelper.GetLocalization("UpdatePreferencesBusyCheckUpdates");
                IsBusy = true;

                await CheckForNewAppVersionAsync();

                await CheckForPresetsAsync(isAutomaticUpdateCheck);

                if (!isAutomaticUpdateCheck)
                {
                    IsAppUpToDate = !IsUpdateAvailable && !IsPresetUpdateAvailable;
                }

                if (isAutomaticUpdateCheck)
                {
                    //If not already prompted
                    if (!_appSettings.PromptedForAutoUpdateCheck)
                    {
                        _appSettings.PromptedForAutoUpdateCheck = true;

                        if (!_messageBoxService.ShowQuestion(Application.Current.MainWindow,
                            _localizationHelper.GetLocalization("ContinueCheckingForUpdates"),
                            _localizationHelper.GetLocalization("ContinueCheckingForUpdatesTitle")))
                        {
                            AutomaticUpdateCheck = false;
                        }
                    }
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
                    _messageBoxService.ShowError(Application.Current.MainWindow,
                        _localizationHelper.GetLocalization("VersionCheckErrorMessage"),
                        _localizationHelper.GetLocalization("VersionCheckErrorTitle"));
                }
            }
        }

        private async Task CheckForNewAppVersionAsync()
        {
            (bool isNewAppVersionAvailable, Version newAppVersion) = await _updateHelper.IsNewAppVersionAvailableAsync();

            if (isNewAppVersionAvailable)
            {
                UpdatedVersionValue = newAppVersion.ToString();
                IsUpdateAvailable = true;
                _messageBoxService.ShowMessage(Application.Current.MainWindow,
                    _localizationHelper.GetLocalization("UpdatePreferencesNewAppUpdateAvailable") + Environment.NewLine + Environment.NewLine + "https://github.com/AnnoDesigner/anno-designer/releases/",
                    _localizationHelper.GetLocalization("UpdatePreferencesUpdates"));
            }
            else
            {
                IsUpdateAvailable = false;
            }
        }

        private async Task CheckForPresetsAsync(bool isAutomaticUpdateCheck)
        {
            FoundPresetRelease = null;

            var foundRelease = await _updateHelper.GetAvailableReleasesAsync(ReleaseType.Presets);
            if (foundRelease == null)
            {
                return;
            }

            var isNewReleaseAvailable = foundRelease.Version > new Version(PresetsVersionValue);
            if (isNewReleaseAvailable)
            {
                IsPresetUpdateAvailable = true;
                FoundPresetRelease = foundRelease;

                if (isAutomaticUpdateCheck)
                {
                    if (_messageBoxService.ShowQuestion(Application.Current.MainWindow,
                        _localizationHelper.GetLocalization("UpdateAvailablePresetMessage"),
                        _localizationHelper.GetLocalization("UpdateAvailableHeader")))
                    {
                        ExecuteDownloadPresets(null);
                    }
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

            BusyContent = _localizationHelper.GetLocalization("UpdatePreferencesBusyDownloadPresets");
            IsBusy = true;

            if (!_commons.CanWriteInFolder())
            {
                //already asked for admin rights?
                if (App.StartupArguments is AdminRestartArgs)
                {
                    _messageBoxService.ShowWarning($"You have no write access to the folder.{Environment.NewLine}The update can not be installed.",
                        _localizationHelper.GetLocalization("Error"));

                    IsBusy = false;
                    return;
                }

                _messageBoxService.ShowMessage(_localizationHelper.GetLocalization("UpdateRequiresAdminRightsMessage"),
                    _localizationHelper.GetLocalization("AdminRightsRequired"));

                _appSettings.Save();
                _commons.RestartApplication(true, AdminRestartArgs.Arguments, App.ExecutablePath);
            }

            //Context is required here, do not use ConfigureAwait(false)
            var newLocation = await _updateHelper.DownloadReleaseAsync(FoundPresetRelease);
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

        public string ColorPresetsVersionValue
        {
            get { return _colorPresetsVersionValue; }
            set { UpdateProperty(ref _colorPresetsVersionValue, value); }
        }

        public string TreeLocalizationVersionValue
        {
            get { return _treeLocalizationVersionValue; }
            set { UpdateProperty(ref _treeLocalizationVersionValue, value); }
        }

        public string UpdatedVersionValue
        {
            get { return _updatedVersionValue; }
            set { UpdateProperty(ref _updatedVersionValue, value); }
        }
    }
}
