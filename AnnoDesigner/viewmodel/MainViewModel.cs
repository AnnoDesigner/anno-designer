using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Localization;
using AnnoDesigner.model;
using AnnoDesigner.Properties;
using NLog;
using Xceed.Wpf.Toolkit;
using static AnnoDesigner.Core.CoreConstants;
using MessageBox = Xceed.Wpf.Toolkit.MessageBox;

namespace AnnoDesigner.viewmodel
{
    public class MainViewModel : Notify
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly ICommons _commons;

        private Dictionary<int, bool> _treeViewState;
        private bool _canvasShowGrid;
        private bool _canvasShowIcons;
        private bool _canvasShowLabels;
        private bool _automaticUpdateCheck;
        private string _versionValue;
        private string _fileVersionValue;
        private string _presetsVersionValue;
        private bool _useCurrentZoomOnExportedImageValue;
        private bool _renderSelectionHighlightsOnExportedImageValue;
        private bool _isLanguageChange;
        private bool _isBusy;
        private string _statusMessage;
        private string _statusMessageClipboard;

        public MainViewModel(ICommons commonsToUse)
        {
            _commons = commonsToUse;
            _commons.SelectedLanguageChanged += Commons_SelectedLanguageChanged;

            _statisticsViewModel = new StatisticsViewModel();
            _buildingSettingsViewModel = new BuildingSettingsViewModel();
            _presetsTreeViewModel = new PresetsTreeViewModel(new TreeLocalization());
            _presetsTreeSearchViewModel = new PresetsTreeSearchViewModel();
            _presetsTreeSearchViewModel.PropertyChanged += PresetsTreeSearchViewModel_PropertyChanged;
            _welcomeViewModel = new WelcomeViewModel();
            _aboutViewModel = new AboutViewModel();

            OpenProjectHomepageCommand = new RelayCommand(OpenProjectHomepage);
            CloseWindowCommand = new RelayCommand<ICloseable>(CloseWindow);
            CanvasResetZoomCommand = new RelayCommand(CanvasResetZoom);
            CanvasNormalizeCommand = new RelayCommand(CanvasNormalize);

            UpdateLanguage();
        }

        private void Commons_SelectedLanguageChanged(object sender, EventArgs e)
        {
            UpdateLanguage();

            BuildingSettingsViewModel.UpdateLanguageBuildingInfluenceType();
        }

        private void PresetsTreeSearchViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (string.Equals(e.PropertyName, nameof(PresetsTreeSearchViewModel.SearchText), StringComparison.OrdinalIgnoreCase))
            {
                PresetsTreeViewModel.FilterText = PresetsTreeSearchViewModel.SearchText;

                if (!IsLanguageChange && string.IsNullOrWhiteSpace(PresetsTreeSearchViewModel.SearchText))
                {
                    PresetsTreeViewModel.SetCondensedTreeState(_treeViewState, AnnoCanvas.BuildingPresets.Version);
                }
            }
            else if (string.Equals(e.PropertyName, nameof(PresetsTreeSearchViewModel.HasFocus), StringComparison.OrdinalIgnoreCase) &&
                    PresetsTreeSearchViewModel.HasFocus &&
                    string.IsNullOrWhiteSpace(PresetsTreeSearchViewModel.SearchText))
            {
                _treeViewState = PresetsTreeViewModel.GetCondensedTreeState();
            }
            else if (string.Equals(e.PropertyName, nameof(PresetsTreeSearchViewModel.SelectedGameVersionFilters), StringComparison.OrdinalIgnoreCase))
            {
                var filterGameVersion = GameVersion.Unknown;

                foreach (var curSelectedFilter in PresetsTreeSearchViewModel.SelectedGameVersionFilters)
                {
                    filterGameVersion |= curSelectedFilter.Type;
                }

                PresetsTreeViewModel.FilterGameVersion = filterGameVersion;
            }
        }

        private void AnnoCanvas_StatisticsUpdated(object sender, EventArgs e)
        {
            UpdateStatistics();
        }

        public void AnnoCanvas_ClipboardChanged(List<AnnoObject> itemsOnClipboard)
        {
            StatusMessageClipboard = StatusBarItemsOnClipboard + ": " + itemsOnClipboard.Count;
        }

        public void UpdateStatistics()
        {
            StatisticsViewModel.UpdateStatistics(_annoCanvas.PlacedObjects,
                _annoCanvas.SelectedObjects,
                _annoCanvas.BuildingPresets);
        }

        public async Task CheckForNewAppVersionAsync(bool forcedCheck)
        {
            try
            {
                var dowloadedContent = "0.1";
                using (var webClient = new WebClient())
                {
                    dowloadedContent = await webClient.DownloadStringTaskAsync(new Uri("https://raw.githubusercontent.com/AgmasGold/anno-designer/master/version.txt"));
                }

                if (double.Parse(dowloadedContent, CultureInfo.InvariantCulture) > Constants.Version)
                {
                    // new version found
                    if (MessageBox.Show("A newer version was found, do you want to visit the releases page?\nhttps://github.com/AgmasGold/anno-designer/releases\n\n Clicking 'Yes' will open a new tab in your web browser.", "Update available", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.OK) == MessageBoxResult.Yes)
                    {
                        Process.Start("https://github.com/AgmasGold/anno-designer/releases");
                    }
                }
                else
                {
                    StatusMessage = "Version is up to date.";
                    //StatusMessageChanged("Version is up to date.");

                    if (forcedCheck)
                    {
                        MessageBox.Show("This version is up to date.", "No updates found");
                    }
                }

                //If not already prompted
                if (!Settings.Default.PromptedForAutoUpdateCheck)
                {
                    Settings.Default.PromptedForAutoUpdateCheck = true;

                    if (MessageBox.Show("Do you want to continue checking for a new version on startup?\n\nThis option can be changed from the help menu.", "Continue checking for updates?", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.No)
                    {
                        AutomaticUpdateCheck = false;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error checking version.");
                MessageBox.Show("Error checking version. \n\nAdded more information to log.", "Version check failed");
                return;
            }
        }

        public async Task CheckForPresetsAsync()
        {
            var foundRelease = await _commons.UpdateHelper.GetAvailableReleasesAsync(ReleaseType.Presets);
            if (foundRelease == null)
            {
                return;
            }

            var isNewReleaseAvailable = foundRelease.Version > new Version(AnnoCanvas.BuildingPresets.Version);
            if (isNewReleaseAvailable)
            {
                string language = Localization.Localization.GetLanguageCodeFromName(MainWindow.SelectedLanguage);

                if (MessageBox.Show(Localization.Localization.Translations[language]["UpdateAvailablePresetMessage"],
                    Localization.Localization.Translations[language]["UpdateAvailableHeader"],
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Asterisk,
                    MessageBoxResult.OK) == MessageBoxResult.Yes)
                {
                    IsBusy = true;

                    if (!Commons.CanWriteInFolder())
                    {
                        //already asked for admin rights?
                        if (Environment.GetCommandLineArgs().Any(x => x.Trim().Equals(Constants.Argument_Ask_For_Admin, StringComparison.OrdinalIgnoreCase)))
                        {
                            MessageBox.Show($"You have no write access to the folder.{Environment.NewLine}The update can not be installed.",
                                Localization.Localization.Translations[language]["Error"],
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);

                            IsBusy = false;
                            return;
                        }

                        MessageBox.Show(Localization.Localization.Translations[language]["UpdateRequiresAdminRightsMessage"],
                            Localization.Localization.Translations[language]["AdminRightsRequired"],
                            MessageBoxButton.OK,
                            MessageBoxImage.Information,
                            MessageBoxResult.OK);

                        Commons.RestartApplication(true, Constants.Argument_Ask_For_Admin, App.ExecutablePath);
                    }

                    //Context is required here, do not use ConfigureAwait(false)
                    var newLocation = await _commons.UpdateHelper.DownloadReleaseAsync(foundRelease);

                    IsBusy = false;

                    Commons.RestartApplication(false, null, App.ExecutablePath);

                    Environment.Exit(-1);
                }
            }
        }

        #region properties

        public AnnoCanvas AnnoCanvas
        {
            get { return _annoCanvas; }
            set
            {
                if (_annoCanvas != null)
                {
                    _annoCanvas.StatisticsUpdated -= AnnoCanvas_StatisticsUpdated;
                }

                _annoCanvas = value;
                _annoCanvas.StatisticsUpdated += AnnoCanvas_StatisticsUpdated;
                _annoCanvas.OnClipboardChanged += AnnoCanvas_ClipboardChanged;
                BuildingSettingsViewModel.AnnoCanvasToUse = _annoCanvas;
            }
        }

        public bool CanvasShowGrid
        {
            get { return _canvasShowGrid; }
            set
            {
                UpdateProperty(ref _canvasShowGrid, value);
                AnnoCanvas.RenderGrid = _canvasShowGrid;
            }
        }

        public bool CanvasShowIcons
        {
            get { return _canvasShowIcons; }
            set
            {
                UpdateProperty(ref _canvasShowIcons, value);
                AnnoCanvas.RenderIcon = _canvasShowIcons;
            }
        }

        public bool CanvasShowLabels
        {
            get { return _canvasShowLabels; }
            set
            {
                UpdateProperty(ref _canvasShowLabels, value);
                AnnoCanvas.RenderLabel = _canvasShowLabels;
            }
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

        public bool UseCurrentZoomOnExportedImageValue
        {
            get { return _useCurrentZoomOnExportedImageValue; }
            set { UpdateProperty(ref _useCurrentZoomOnExportedImageValue, value); }
        }

        public bool RenderSelectionHighlightsOnExportedImageValue
        {
            get { return _renderSelectionHighlightsOnExportedImageValue; }
            set { UpdateProperty(ref _renderSelectionHighlightsOnExportedImageValue, value); }
        }

        public bool IsLanguageChange
        {
            get { return _isLanguageChange; }
            set { UpdateProperty(ref _isLanguageChange, value); }
        }

        public bool IsBusy
        {
            get { return _isBusy; }
            set { UpdateProperty(ref _isBusy, value); }
        }

        public string StatusMessage
        {
            get { return _statusMessage; }
            set { UpdateProperty(ref _statusMessage, value); }
        }

        public string StatusMessageClipboard
        {
            get { return _statusMessageClipboard; }
            set { UpdateProperty(ref _statusMessageClipboard, value); }
        }

        #endregion

        #region commands

        public ICommand OpenProjectHomepageCommand { get; private set; }

        private void OpenProjectHomepage(object param)
        {
            System.Diagnostics.Process.Start("https://github.com/AgmasGold/anno-designer/");
        }

        public ICommand CloseWindowCommand { get; private set; }

        private void CloseWindow(ICloseable window)
        {
            window?.Close();
        }

        public ICommand CanvasResetZoomCommand { get; private set; }

        private void CanvasResetZoom(object param)
        {
            AnnoCanvas.ResetZoom();
        }

        public ICommand CanvasNormalizeCommand { get; private set; }

        private void CanvasNormalize(object param)
        {
            AnnoCanvas.Normalize(1);
        }

        #endregion

        #region view models

        private StatisticsViewModel _statisticsViewModel;
        public StatisticsViewModel StatisticsViewModel
        {
            get { return _statisticsViewModel; }
            set { _statisticsViewModel = value; }
        }

        private BuildingSettingsViewModel _buildingSettingsViewModel;
        public BuildingSettingsViewModel BuildingSettingsViewModel
        {
            get { return _buildingSettingsViewModel; }
            set { _buildingSettingsViewModel = value; }
        }

        private PresetsTreeViewModel _presetsTreeViewModel;
        public PresetsTreeViewModel PresetsTreeViewModel
        {
            get { return _presetsTreeViewModel; }
            set { _presetsTreeViewModel = value; }
        }

        private PresetsTreeSearchViewModel _presetsTreeSearchViewModel;
        public PresetsTreeSearchViewModel PresetsTreeSearchViewModel
        {
            get { return _presetsTreeSearchViewModel; }
            set { _presetsTreeSearchViewModel = value; }
        }

        private WelcomeViewModel _welcomeViewModel;
        public WelcomeViewModel WelcomeViewModel
        {
            get { return _welcomeViewModel; }
            set { _welcomeViewModel = value; }
        }

        private AboutViewModel _aboutViewModel;
        public AboutViewModel AboutViewModel
        {
            get { return _aboutViewModel; }
            set { _aboutViewModel = value; }
        }

        #endregion

        #region localization

        public void UpdateLanguage()
        {
            string language = Localization.Localization.GetLanguageCodeFromName(MainWindow.SelectedLanguage);

            StatisticsViewModel.TextNothingPlaced = Localization.Localization.Translations[language]["StatNothingPlaced"];
            StatisticsViewModel.TextBoundingBox = Localization.Localization.Translations[language]["StatBoundingBox"];
            StatisticsViewModel.TextMinimumArea = Localization.Localization.Translations[language]["StatMinimumArea"];
            StatisticsViewModel.TextSpaceEfficiency = Localization.Localization.Translations[language]["StatSpaceEfficiency"];
            StatisticsViewModel.TextBuildings = Localization.Localization.Translations[language]["StatBuildings"];
            StatisticsViewModel.TextBuildingsSelected = Localization.Localization.Translations[language]["StatBuildingsSelected"];
            StatisticsViewModel.TextTiles = Localization.Localization.Translations[language]["StatTiles"];
            StatisticsViewModel.TextNameNotFound = Localization.Localization.Translations[language]["StatNameNotFound"];

            Ok = Localization.Localization.Translations[language]["OK"];
            Cancel = Localization.Localization.Translations[language]["Cancel"];

            //File Menu
            File = Localization.Localization.Translations[language]["File"];
            NewCanvas = Localization.Localization.Translations[language]["NewCanvas"];
            Open = Localization.Localization.Translations[language]["Open"];
            Save = Localization.Localization.Translations[language]["Save"];
            SaveAs = Localization.Localization.Translations[language]["SaveAs"];
            CopyLayoutToClipboard = Localization.Localization.Translations[language]["CopyLayoutToClipboard"];
            LoadLayoutFromJson = Localization.Localization.Translations[language]["LoadLayoutFromJson"];
            Exit = Localization.Localization.Translations[language]["Exit"];

            //Extras Menu
            Extras = Localization.Localization.Translations[language]["Extras"];
            Normalize = Localization.Localization.Translations[language]["Normalize"];
            ResetZoom = Localization.Localization.Translations[language]["ResetZoom"];
            RegisterFileExtension = Localization.Localization.Translations[language]["RegisterFileExtension"];
            UnregisterFileExtension = Localization.Localization.Translations[language]["UnregisterFileExtension"];

            //Export Menu
            Export = Localization.Localization.Translations[language]["Export"];
            ExportImage = Localization.Localization.Translations[language]["ExportImage"];
            UseCurrentZoomOnExportedImage = Localization.Localization.Translations[language]["UseCurrentZoomOnExportedImage"];
            RenderSelectionHighlightsOnExportedImage = Localization.Localization.Translations[language]["RenderSelectionHighlightsOnExportedImage"];

            //Manage Stats Menu
            ManageStats = Localization.Localization.Translations[language]["ManageStats"];
            ShowStats = Localization.Localization.Translations[language]["ShowStats"];
            BuildingCount = Localization.Localization.Translations[language]["BuildingCount"];

            //Language Menu
            Language = Localization.Localization.Translations[language]["Language"];

            //Help Menu
            Help = Localization.Localization.Translations[language]["Help"];
            Version = Localization.Localization.Translations[language]["Version"];
            FileVersion = Localization.Localization.Translations[language]["FileVersion"];
            PresetsVersion = Localization.Localization.Translations[language]["PresetsVersion"];
            CheckForUpdates = Localization.Localization.Translations[language]["CheckForUpdates"];
            EnableAutomaticUpdateCheck = Localization.Localization.Translations[language]["EnableAutomaticUpdateCheck"];
            GoToProjectHomepage = Localization.Localization.Translations[language]["GoToProjectHomepage"];
            OpenWelcomePage = Localization.Localization.Translations[language]["OpenWelcomePage"];
            AboutAnnoDesigner = Localization.Localization.Translations[language]["AboutAnnoDesigner"];

            //Other
            ShowGrid = Localization.Localization.Translations[language]["ShowGrid"];
            ShowLabels = Localization.Localization.Translations[language]["ShowLabels"];
            ShowIcons = Localization.Localization.Translations[language]["ShowIcons"];

            //DockPanel
            BuildingSettingsViewModel.TextHeader = Localization.Localization.Translations[language]["BuildingSettings"];
            BuildingSettingsViewModel.TextSize = Localization.Localization.Translations[language]["Size"];
            BuildingSettingsViewModel.TextColor = Localization.Localization.Translations[language]["Color"];
            BuildingSettingsViewModel.TextBuildingName = Localization.Localization.Translations[language]["Label"];
            BuildingSettingsViewModel.TextIcon = Localization.Localization.Translations[language]["Icon"];
            BuildingSettingsViewModel.TextInfluenceType = Localization.Localization.Translations[language]["InfluenceType"];
            None = Localization.Localization.Translations[language]["None"];
            BuildingSettingsViewModel.TextRadius = Localization.Localization.Translations[language]["Radius"];
            BuildingSettingsViewModel.TextDistance = Localization.Localization.Translations[language]["Distance"];
            Both = Localization.Localization.Translations[language]["Both"];
            BuildingSettingsViewModel.TextPavedStreet = Localization.Localization.Translations[language]["PavedStreet"];
            BuildingSettingsViewModel.TextPavedStreetWarningTitle = Localization.Localization.Translations[language]["PavedStreetWarningTitle"];
            BuildingSettingsViewModel.TextPavedStreetToolTip = Localization.Localization.Translations[language]["PavedStreetToolTip"];
            BuildingSettingsViewModel.TextOptions = Localization.Localization.Translations[language]["Options"];
            BuildingSettingsViewModel.TextEnableLabel = Localization.Localization.Translations[language]["EnableLabel"];
            BuildingSettingsViewModel.TextBorderless = Localization.Localization.Translations[language]["Borderless"];
            BuildingSettingsViewModel.TextRoad = Localization.Localization.Translations[language]["Road"];
            BuildingSettingsViewModel.TextPlaceBuilding = Localization.Localization.Translations[language]["PlaceBuilding"];
            BuildingSettingsViewModel.TextApplyColorToSelection = Localization.Localization.Translations[language]["ApplyColorToSelection"];
            BuildingSettingsViewModel.TextApplyColorToSelectionToolTip = Localization.Localization.Translations[language]["ApplyColorToSelectionToolTip"];
            BuildingSettingsViewModel.TextApplyPredefinedColorToSelection = Localization.Localization.Translations[language]["ApplyPredefinedColorToSelection"];
            BuildingSettingsViewModel.TextApplyPredefinedColorToSelectionToolTip = Localization.Localization.Translations[language]["ApplyPredefinedColorToSelectionToolTip"];
            BuildingSettingsViewModel.TextAvailableColors = Localization.Localization.Translations[language]["AvailableColors"];
            BuildingSettingsViewModel.TextStandardColors = Localization.Localization.Translations[language]["StandardColors"];
            BuildingSettingsViewModel.TextRecentColors = Localization.Localization.Translations[language]["RecentColors"];
            BuildingSettingsViewModel.TextStandard = Localization.Localization.Translations[language]["Standard"];
            BuildingSettingsViewModel.TextAdvanced = Localization.Localization.Translations[language]["Advanced"];
            BuildingSettingsViewModel.TextColorsInLayout = Localization.Localization.Translations[language]["ColorsInLayout"];
            BuildingSettingsViewModel.TextColorsInLayoutToolTip = Localization.Localization.Translations[language]["ColorsInLayoutToolTip"];

            PresetsTreeSearchViewModel.TextSearch = Localization.Localization.Translations[language]["Search"];
            PresetsTreeSearchViewModel.TextSearchToolTip = Localization.Localization.Translations[language]["SearchToolTip"];
            PresetsTreeSearchViewModel.TextSelectAll = Localization.Localization.Translations[language]["SelectAll"];

            //Status Bar
            StatusBarControls = Localization.Localization.Translations[language]["StatusBarControls"];
            StatusBarItemsOnClipboard = Localization.Localization.Translations[language]["StatusBarItemsOnClipboard"];
        }

        //Generated from:
        //...
        //public string Prop1 {get; set;}
        //public string Prop2 {get; set;}
        //...
        //find expr: public (string) (.+?) {.+
        //With the following regex (in a compatible editor that supports lowercasing of values
        //within regex expressions):
        //private $1 _\l$2; \r\n public $1 $2 \r\n { \r\n get { return _\l$2; } \r\n set \r\n { \r\n UpdateProperty\(ref _\l$2, value\); \r\n}\r\n}

        #region File Menu

        private string _file;
        public string File
        {
            get { return _file; }
            set
            {
                UpdateProperty(ref _file, value);
            }
        }

        private string _newCanvas;
        public string NewCanvas
        {
            get { return _newCanvas; }
            set
            {
                UpdateProperty(ref _newCanvas, value);
            }
        }

        private string _open;
        public string Open
        {
            get { return _open; }
            set
            {
                UpdateProperty(ref _open, value);
            }
        }

        private string _save;
        public string Save
        {
            get { return _save; }
            set
            {
                UpdateProperty(ref _save, value);
            }
        }

        private string _saveAs;
        public string SaveAs
        {
            get { return _saveAs; }
            set
            {
                UpdateProperty(ref _saveAs, value);
            }
        }

        private string _copyLayoutToClipboard;
        public string CopyLayoutToClipboard
        {
            get { return _copyLayoutToClipboard; }
            set
            {
                UpdateProperty(ref _copyLayoutToClipboard, value);
            }
        }

        private string _loadLayoutFromJson;
        public string LoadLayoutFromJson
        {
            get { return _loadLayoutFromJson; }
            set
            {
                UpdateProperty(ref _loadLayoutFromJson, value);
            }
        }

        private string _ok;
        public string Ok
        {
            get { return _ok; }
            set
            {
                UpdateProperty(ref _ok, value);
            }
        }

        private string _cancel;
        public string Cancel
        {
            get { return _cancel; }
            set
            {
                UpdateProperty(ref _cancel, value);
            }
        }

        private string _exit;
        public string Exit
        {
            get { return _exit; }
            set
            {
                UpdateProperty(ref _exit, value);
            }
        }

        #endregion

        #region Extras Menu

        private string _extras;
        public string Extras
        {
            get { return _extras; }
            set
            {
                UpdateProperty(ref _extras, value);
            }
        }
        private string _normalize;
        public string Normalize
        {
            get { return _normalize; }
            set
            {
                UpdateProperty(ref _normalize, value);
            }
        }
        private string _resetZoom;
        public string ResetZoom
        {
            get { return _resetZoom; }
            set
            {
                UpdateProperty(ref _resetZoom, value);
            }
        }
        private string _registerFileExtension;
        public string RegisterFileExtension
        {
            get { return _registerFileExtension; }
            set
            {
                UpdateProperty(ref _registerFileExtension, value);
            }
        }
        private string _unregisterFileExtension;
        public string UnregisterFileExtension
        {
            get { return _unregisterFileExtension; }
            set
            {
                UpdateProperty(ref _unregisterFileExtension, value);
            }
        }

        #endregion

        #region Export Menu

        private string _export;
        public string Export
        {
            get { return _export; }
            set
            {
                UpdateProperty(ref _export, value);
            }
        }
        private string _exportImage;
        public string ExportImage
        {
            get { return _exportImage; }
            set
            {
                UpdateProperty(ref _exportImage, value);
            }
        }
        private string _useCurrentZoomOnExportedImage;
        public string UseCurrentZoomOnExportedImage
        {
            get { return _useCurrentZoomOnExportedImage; }
            set
            {
                UpdateProperty(ref _useCurrentZoomOnExportedImage, value);
            }
        }
        private string _renderSelectionHighlightsOnExportedImage;
        public string RenderSelectionHighlightsOnExportedImage
        {
            get { return _renderSelectionHighlightsOnExportedImage; }
            set
            {
                UpdateProperty(ref _renderSelectionHighlightsOnExportedImage, value);
            }
        }

        #endregion

        #region Language Menu

        private string _language;
        public string Language
        {
            get { return _language; }
            set
            {
                UpdateProperty(ref _language, value);
            }
        }

        #endregion

        #region Manage Stats Menu

        private string _ManageStats;
        public string ManageStats
        {
            get { return _ManageStats; }
            set
            {
                UpdateProperty(ref _ManageStats, value);
            }
        }
        private string _showStats;
        public string ShowStats
        {
            get { return _showStats; }
            set
            {
                UpdateProperty(ref _showStats, value);
            }
        }
        private string _BuildingCount;
        public string BuildingCount
        {
            get { return _BuildingCount; }
            set
            {
                UpdateProperty(ref _BuildingCount, value);
            }
        }

        #endregion

        #region Help Menu

        private string _help;
        public string Help
        {
            get { return _help; }
            set
            {
                UpdateProperty(ref _help, value);
            }
        }

        private string _version;
        public string Version
        {
            get { return _version; }
            set
            {
                UpdateProperty(ref _version, value);
            }
        }

        private string _fileVersion;
        public string FileVersion
        {
            get { return _fileVersion; }
            set
            {
                UpdateProperty(ref _fileVersion, value);
            }
        }

        private string _presetsVersion;
        public string PresetsVersion
        {
            get { return _presetsVersion; }
            set
            {
                UpdateProperty(ref _presetsVersion, value);
            }
        }

        private string _checkForUpdates;
        public string CheckForUpdates
        {
            get { return _checkForUpdates; }
            set
            {
                UpdateProperty(ref _checkForUpdates, value);
            }
        }

        private string _enableAutomaticUpdateCheck;
        public string EnableAutomaticUpdateCheck
        {
            get { return _enableAutomaticUpdateCheck; }
            set
            {
                UpdateProperty(ref _enableAutomaticUpdateCheck, value);
            }
        }

        private string _goToProjectHomepage;
        public string GoToProjectHomepage
        {
            get { return _goToProjectHomepage; }
            set
            {
                UpdateProperty(ref _goToProjectHomepage, value);
            }
        }

        private string _openWelcomePage;
        public string OpenWelcomePage
        {
            get { return _openWelcomePage; }
            set
            {
                UpdateProperty(ref _openWelcomePage, value);
            }
        }

        private string _aboutAnnoDesigner;
        public string AboutAnnoDesigner
        {
            get { return _aboutAnnoDesigner; }
            set
            {
                UpdateProperty(ref _aboutAnnoDesigner, value);
            }
        }

        #endregion

        #region Other options

        private string _showGrid;
        public string ShowGrid
        {
            get { return _showGrid; }
            set
            {
                UpdateProperty(ref _showGrid, value);
            }
        }
        private string _showLabels;
        public string ShowLabels
        {
            get { return _showLabels; }
            set
            {
                UpdateProperty(ref _showLabels, value);
            }
        }
        private string _showIcons;
        public string ShowIcons
        {
            get { return _showIcons; }
            set
            {
                UpdateProperty(ref _showIcons, value);
            }
        }

        #endregion

        #region DockPanel       

        private string _none;
        public string None
        {
            get { return _none; }
            set
            {
                UpdateProperty(ref _none, value);
            }
        }

        private string _both;
        public string Both
        {
            get { return _both; }
            set
            {
                UpdateProperty(ref _both, value);
            }
        }

        #endregion

        #region Status Bar

        private string _statusBarControls;
        public string StatusBarControls
        {
            get { return _statusBarControls; }
            set
            {
                UpdateProperty(ref _statusBarControls, value);
            }
        }

        private string _statusBarItemsOnClipboard;
        private AnnoCanvas _annoCanvas;

        public string StatusBarItemsOnClipboard
        {
            get { return _statusBarItemsOnClipboard; }
            set
            {
                UpdateProperty(ref _statusBarItemsOnClipboard, value);
            }
        }

        #endregion

        #endregion      
    }
}


