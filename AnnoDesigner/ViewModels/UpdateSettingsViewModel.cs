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
        private string _versionValue;
        private string _fileVersionValue;
        private string _presetsVersionValue;

        public UpdateSettingsViewModel(ICommons commonsToUse, 
            IAppSettings appSettingsToUse)
        {
            _commons = commonsToUse;
            _appSettings = appSettingsToUse;

            CheckForUpdatesCommand = new RelayCommand(ExecuteCheckForUpdates);
        }

        public async Task CheckForUpdatesSub(bool forcedCheck)
        {
            if (AutomaticUpdateCheck || forcedCheck)
            {
                try
                {
                    await CheckForNewAppVersionAsync(forcedCheck);

                    await CheckForPresetsAsync();
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Error checking version.");
                    MessageBox.Show("Error checking version. \n\nAdded more information to log.", "Version check failed");
                    return;
                }
            }
        }

        private async Task CheckForNewAppVersionAsync(bool forcedCheck)
        {
            try
            {
                var dowloadedContent = "0.1";
                using (var webClient = new WebClient())
                {
                    dowloadedContent = await webClient.DownloadStringTaskAsync(new Uri("https://raw.githubusercontent.com/AnnoDesigner/anno-designer/master/version.txt"));
                }

                if (double.Parse(dowloadedContent, CultureInfo.InvariantCulture) > Constants.Version)
                {
                    // new version found
                    if (MessageBox.Show("A newer version was found, do you want to visit the releases page?\nhttps://github.com/AnnoDesigner/anno-designer/releases\n\n Clicking 'Yes' will open a new tab in your web browser.",
                        "Update available",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Asterisk,
                        MessageBoxResult.OK) == MessageBoxResult.Yes)
                    {
                        Process.Start("https://github.com/AnnoDesigner/anno-designer/releases");
                    }
                }
                else
                {
                    //StatusMessage = "Version is up to date.";

                    if (forcedCheck)
                    {
                        MessageBox.Show("This version is up to date.", "No updates found");
                    }
                }

                //If not already prompted
                if (!_appSettings.PromptedForAutoUpdateCheck)
                {
                    _appSettings.PromptedForAutoUpdateCheck = true;

                    if (MessageBox.Show("Do you want to continue checking for a new version on startup?\n\nThis option can be changed from the help menu.", "Continue checking for updates?", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.No)
                    {
                        AutomaticUpdateCheck = false;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error checking version.");
                MessageBox.Show($"Error checking version.{Environment.NewLine}{Environment.NewLine}More information is found in the log.",
                    "Version check failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
        }

        private async Task CheckForPresetsAsync()
        {
            var foundRelease = await _commons.UpdateHelper.GetAvailableReleasesAsync(ReleaseType.Presets);
            if (foundRelease == null)
            {
                return;
            }

            var isNewReleaseAvailable = foundRelease.Version > new Version(PresetsVersionValue);
            if (isNewReleaseAvailable)
            {
                if (MessageBox.Show(Localization.Localization.Translations["UpdateAvailablePresetMessage"],
                    Localization.Localization.Translations["UpdateAvailableHeader"],
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Asterisk,
                    MessageBoxResult.OK) == MessageBoxResult.Yes)
                {
                    //IsBusy = true;

                    if (!Commons.CanWriteInFolder())
                    {
                        //already asked for admin rights?
                        if (Environment.GetCommandLineArgs().Any(x => x.Trim().Equals(Constants.Argument_Ask_For_Admin, StringComparison.OrdinalIgnoreCase)))
                        {
                            MessageBox.Show($"You have no write access to the folder.{Environment.NewLine}The update can not be installed.",
                                Localization.Localization.Translations["Error"],
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);

                            //IsBusy = false;
                            return;
                        }

                        MessageBox.Show(Localization.Localization.Translations["UpdateRequiresAdminRightsMessage"],
                            Localization.Localization.Translations["AdminRightsRequired"],
                            MessageBoxButton.OK,
                            MessageBoxImage.Information,
                            MessageBoxResult.OK);

                        Commons.RestartApplication(true, Constants.Argument_Ask_For_Admin, App.ExecutablePath);
                    }

                    //Context is required here, do not use ConfigureAwait(false)
                    var newLocation = await _commons.UpdateHelper.DownloadReleaseAsync(foundRelease);
                    logger.Debug($"downloaded new preset ({foundRelease.Version}): {newLocation}");

                    //IsBusy = false;

                    Commons.RestartApplication(false, null, App.ExecutablePath);

                    Environment.Exit(-1);
                }
            }
        }

        public ICommand CheckForUpdatesCommand { get; private set; }

        private async void ExecuteCheckForUpdates(object param)
        {
            await CheckForUpdatesSub(true);
        }

        public bool AutomaticUpdateCheck
        {
            get { return _automaticUpdateCheck; }
            set { UpdateProperty(ref _automaticUpdateCheck, value); }
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
    }
}
