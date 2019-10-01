using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using AnnoDesigner.Core;
using AnnoDesigner.Core.Extensions;
using AnnoDesigner.Core.Helper;
using AnnoDesigner.Core.Layout;
using AnnoDesigner.Core.Layout.Exceptions;
using AnnoDesigner.Core.Layout.Models;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Core.Presets.Helper;
using AnnoDesigner.Helper;
using AnnoDesigner.Localization;
using AnnoDesigner.model;
using Microsoft.Win32;
using NLog;
using MessageBox = Xceed.Wpf.Toolkit.MessageBox;

namespace AnnoDesigner.viewmodel
{
    public class MainViewModel : Notify
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly ICommons _commons;
        private readonly IAppSettings _appSettings;
        private readonly ILayoutLoader _layoutLoader;
        private readonly ICoordinateHelper _coordinateHelper;

        public event EventHandler<EventArgs> ShowStatisticsChanged;

        private AnnoCanvas _annoCanvas;
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
        private ObservableCollection<SupportedLanguage> _languages;
        private ObservableCollection<IconImage> _availableIcons;
        private IconImage _selectedIcon;
        private string _mainWindowTitle;
        private string _presetsSectionHeader;

        //for identifier checking process
        private static readonly List<string> IconFieldNamesCheck = new List<string> { "icon_116_22", "icon_27_6", "field", "general_module" };
        private readonly IconImage _noIconItem;

        public MainViewModel(ICommons commonsToUse, IAppSettings appSettingsToUse, ILayoutLoader _layoutLoaderToUse = null, ICoordinateHelper coordinateHelperToUse = null)
        {
            _commons = commonsToUse;
            _commons.SelectedLanguageChanged += Commons_SelectedLanguageChanged;

            _appSettings = appSettingsToUse;

            _layoutLoader = _layoutLoaderToUse ?? new LayoutLoader();
            _coordinateHelper = coordinateHelperToUse ?? new CoordinateHelper();

            _statisticsViewModel = new StatisticsViewModel(_commons);
            _statisticsViewModel.IsVisible = _appSettings.StatsShowStats;
            _statisticsViewModel.ShowStatisticsBuildingCount = _appSettings.StatsShowBuildingCount;

            _buildingSettingsViewModel = new BuildingSettingsViewModel(_commons, _appSettings);

            _presetsTreeViewModel = new PresetsTreeViewModel(new TreeLocalization(_commons), _commons);
            _presetsTreeViewModel.ApplySelectedItem += PresetTreeViewModel_ApplySelectedItem;

            _presetsTreeSearchViewModel = new PresetsTreeSearchViewModel();
            _presetsTreeSearchViewModel.PropertyChanged += PresetsTreeSearchViewModel_PropertyChanged;

            _welcomeViewModel = new WelcomeViewModel(_commons, _appSettings);

            _aboutViewModel = new AboutViewModel(_commons);

            OpenProjectHomepageCommand = new RelayCommand(OpenProjectHomepage);
            CloseWindowCommand = new RelayCommand<ICloseable>(CloseWindow);
            CanvasResetZoomCommand = new RelayCommand(CanvasResetZoom);
            CanvasNormalizeCommand = new RelayCommand(CanvasNormalize);
            LoadLayoutFromJsonCommand = new RelayCommand(ExecuteLoadLayoutFromJson);
            UnregisterExtensionCommand = new RelayCommand(UnregisterExtension);
            RegisterExtensionCommand = new RelayCommand(RegisterExtension);
            ExportImageCommand = new RelayCommand(ExecuteExportImage);
            CopyLayoutToClipboardCommand = new RelayCommand(ExecuteCopyLayoutToClipboard);
            LanguageSelectedCommand = new RelayCommand(ExecuteLanguageSelected);
            ShowAboutWindowCommand = new RelayCommand(ExecuteShowAboutWindow);
            ShowWelcomeWindowCommand = new RelayCommand(ExecuteShowWelcomeWindow);
            CheckForUpdatesCommand = new RelayCommand(ExecuteCheckForUpdates);
            ShowStatisticsCommand = new RelayCommand(ExecuteShowStatistics);
            ShowStatisticsBuildingCountCommand = new RelayCommand(ExecuteShowStatisticsBuildingCount);
            PlaceBuildingCommand = new RelayCommand(ExecutePlaceBuilding);

            AvailableIcons = new ObservableCollection<IconImage>();
            _noIconItem = new IconImage("None");
            AvailableIcons.Add(_noIconItem);
            SelectedIcon = _noIconItem;

            Languages = new ObservableCollection<SupportedLanguage>();
            Languages.Add(new SupportedLanguage("English")
            {
                FlagPath = "Flags/United Kingdom.png"
            });
            Languages.Add(new SupportedLanguage("Deutsch")
            {
                FlagPath = "Flags/Germany.png"
            });
            Languages.Add(new SupportedLanguage("Français")
            {
                FlagPath = "Flags/France.png"
            });
            Languages.Add(new SupportedLanguage("Polski")
            {
                FlagPath = "Flags/Poland.png"
            });
            Languages.Add(new SupportedLanguage("Русский")
            {
                FlagPath = "Flags/Russia.png"
            });
            //Languages.Add(new SupportedLanguage("Español"));
            //Languages.Add(new SupportedLanguage("Italiano"));
            //Languages.Add(new SupportedLanguage("český"));

            MainWindowTitle = "Anno Designer";
            PresetsSectionHeader = "Building presets - not loaded";

            VersionValue = Constants.Version.ToString("0.0#", CultureInfo.InvariantCulture);
            FileVersionValue = CoreConstants.LayoutFileVersion.ToString("0.#", CultureInfo.InvariantCulture);

            UpdateLanguage();
        }

        private void Commons_SelectedLanguageChanged(object sender, EventArgs e)
        {
            try
            {
                InitLanguageMenu(_commons.SelectedLanguage);
                UpdateLanguage();

                if (AnnoCanvas == null)
                {
                    return;
                }

                RepopulateTreeView();

                BuildingSettingsViewModel.UpdateLanguageBuildingInfluenceType();

                //Force a language update on the clipboard status item.
                if (!string.IsNullOrWhiteSpace(StatusMessageClipboard))
                {
                    AnnoCanvas_ClipboardChanged(AnnoCanvas.ObjectClipboard);
                }

                //update settings
                _appSettings.SelectedLanguage = _commons.SelectedLanguage;

                UpdateStatistics();

                PresetsTreeSearchViewModel.SearchText = string.Empty;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error when changing the language.");
            }
            finally
            {
                IsLanguageChange = false;
            }
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
                var filterGameVersion = CoreConstants.GameVersion.Unknown;

                foreach (var curSelectedFilter in PresetsTreeSearchViewModel.SelectedGameVersionFilters)
                {
                    filterGameVersion |= curSelectedFilter.Type;
                }

                PresetsTreeViewModel.FilterGameVersion = filterGameVersion;
            }
        }

        private void PresetTreeViewModel_ApplySelectedItem(object sender, EventArgs e)
        {
            ApplyPreset(PresetsTreeViewModel.SelectedItem.AnnoObject);
        }

        private void ApplyPreset(AnnoObject selectedItem)
        {
            try
            {
                if (selectedItem != null)
                {
                    UpdateUIFromObject(new AnnoObject(selectedItem)
                    {
                        Color = BuildingSettingsViewModel.SelectedColor ?? Colors.Red,
                    });

                    ApplyCurrentObject();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error applying preset.");
                MessageBox.Show("Something went wrong while applying the preset.");
            }
        }

        private void ApplyCurrentObject()
        {
            // parse user inputs and create new object
            var obj = new AnnoObject
            {
                Size = new Size(BuildingSettingsViewModel.BuildingWidth, BuildingSettingsViewModel.BuildingHeight),
                Color = BuildingSettingsViewModel.SelectedColor ?? Colors.Red,
                Label = BuildingSettingsViewModel.IsEnableLabelChecked ? BuildingSettingsViewModel.BuildingName : string.Empty,
                Icon = SelectedIcon == _noIconItem ? null : SelectedIcon.Name,
                Radius = BuildingSettingsViewModel.BuildingRadius,
                InfluenceRange = BuildingSettingsViewModel.BuildingInfluenceRange,
                PavedStreet = BuildingSettingsViewModel.IsPavedStreet,
                Borderless = BuildingSettingsViewModel.IsBorderlessChecked,
                Road = BuildingSettingsViewModel.IsRoadChecked,
                Identifier = BuildingSettingsViewModel.BuildingIdentifier,
                Template = BuildingSettingsViewModel.BuildingTemplate
            };

            var objIconFileName = "";
            //Parse the Icon path into something we can check.
            if (!string.IsNullOrWhiteSpace(obj.Icon))
            {
                if (obj.Icon.StartsWith("A5_"))
                {
                    objIconFileName = obj.Icon.Remove(0, 3) + ".png"; //when Anno 2070, it use not A5_ in the original naming.
                }
                else
                {
                    objIconFileName = obj.Icon + ".png";
                }
            }

            // do some sanity checks
            if (obj.Size.Width > 0 && obj.Size.Height > 0 && obj.Radius >= 0)
            {
                if (!string.IsNullOrWhiteSpace(obj.Icon) && !obj.Icon.Contains(IconFieldNamesCheck))
                {
                    //the identifier text 'Uknown Object' is localized within the StatisticsView, which is why it's not localized here  
                    //gets icons origin building info
                    var buildingInfo = AnnoCanvas.BuildingPresets.Buildings.FirstOrDefault(_ => _.IconFileName?.Equals(objIconFileName, StringComparison.OrdinalIgnoreCase) ?? false);
                    if (buildingInfo != null)
                    {
                        // Check X and Z Sizes of the Building Info, if one or both not right, the Object will be Unknown
                        //Building could be in rotated form - so 5x4 should be equivalent to checking for 4x5
                        if ((obj.Size.Width == buildingInfo.BuildBlocker["x"] && obj.Size.Height == buildingInfo.BuildBlocker["z"])
                            || (obj.Size.Height == buildingInfo.BuildBlocker["x"] && obj.Size.Width == buildingInfo.BuildBlocker["z"]))
                        {
                            //if sizes match and icon is a existing building in the presets, call it that object
                            if (obj.Identifier != "Residence_New_World")
                            {
                                obj.Identifier = buildingInfo.Identifier;
                            }
                        }
                        else
                        {
                            //Sizes and icon do not match
                            obj.Identifier = "Unknown Object";
                        }
                    }
                    else if (!BuildingSettingsViewModel.BuildingTemplate.Contains("field", StringComparison.OrdinalIgnoreCase)) //check if the icon is removed from a template field
                    {
                        obj.Identifier = "Unknown Object";
                    }
                }
                else if (!string.IsNullOrWhiteSpace(obj.Icon) && obj.Icon.Contains(IconFieldNamesCheck))
                {
                    //Check if Field Icon belongs to the field identifier, else set the official icon
                    var buildingInfo = AnnoCanvas.BuildingPresets.Buildings.FirstOrDefault(_ => _.Identifier == obj.Identifier);
                    if (buildingInfo != null)
                    {
                        if (!string.Equals(objIconFileName, buildingInfo.IconFileName, StringComparison.OrdinalIgnoreCase))
                        {
                            obj.Icon = buildingInfo.IconFileName.Remove(buildingInfo.IconFileName.Length - 4, 4); //remove the .png for the combobox
                            try
                            {
                                SelectedIcon = string.IsNullOrEmpty(obj.Icon) ? _noIconItem : AvailableIcons.Single(_ => _.Name == Path.GetFileNameWithoutExtension(obj.Icon));
                            }
                            catch (Exception)
                            {
                                SelectedIcon = _noIconItem;
                            }
                        }
                    }
                    else
                    {
                        obj.Identifier = "Unknown Object";
                    }
                }
                if (string.IsNullOrEmpty(obj.Icon) && !BuildingSettingsViewModel.BuildingTemplate.Contains("field", StringComparison.OrdinalIgnoreCase))
                {
                    obj.Identifier = "Unknown Object";
                }

                AnnoCanvas.SetCurrentObject(obj);
            }
            else
            {
                throw new Exception("Invalid building configuration.");
            }
        }

        /// <summary>
        /// Fired on the OnCurrentObjectChanged event
        /// </summary>
        /// <param name="obj"></param>
        private void UpdateUIFromObject(AnnoObject obj)
        {
            if (obj == null)
            {
                return;
            }

            // size
            BuildingSettingsViewModel.BuildingWidth = (int)obj.Size.Width;
            BuildingSettingsViewModel.BuildingHeight = (int)obj.Size.Height;
            // color
            BuildingSettingsViewModel.SelectedColor = ColorPresetsHelper.Instance.GetPredefinedColor(obj) ?? obj.Color;
            // label
            BuildingSettingsViewModel.BuildingName = obj.Label;
            // Identifier
            BuildingSettingsViewModel.BuildingIdentifier = obj.Identifier;
            // Template
            BuildingSettingsViewModel.BuildingTemplate = obj.Template;
            // icon
            try
            {
                if (string.IsNullOrWhiteSpace(obj.Icon))
                {
                    SelectedIcon = _noIconItem;
                }
                else
                {
                    var foundIconImage = AvailableIcons.SingleOrDefault(x => x.Name.Equals(Path.GetFileNameWithoutExtension(obj.Icon), StringComparison.OrdinalIgnoreCase));
                    SelectedIcon = foundIconImage ?? _noIconItem;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error finding {nameof(IconImage)} for value \"{obj.Icon}\".{Environment.NewLine}{ex}");

                SelectedIcon = _noIconItem;
            }

            // radius
            BuildingSettingsViewModel.BuildingRadius = obj.Radius;
            //InfluenceRange
            if (!BuildingSettingsViewModel.IsPavedStreet)
            {
                BuildingSettingsViewModel.BuildingInfluenceRange = obj.InfluenceRange;
            }
            else
            {
                BuildingSettingsViewModel.GetDistanceRange(true, AnnoCanvas.BuildingPresets.Buildings.FirstOrDefault(_ => _.Identifier == BuildingSettingsViewModel.BuildingIdentifier));
            }

            //Set Influence Type
            if (obj.Radius > 0 && obj.InfluenceRange > 0)
            {
                //Building uses both a radius and an influence
                //Has to be set manually 
                BuildingSettingsViewModel.SelectedBuildingInfluence = BuildingSettingsViewModel.BuildingInfluences.Single(x => x.Type == BuildingInfluenceType.Both);
            }
            else if (obj.Radius > 0)
            {
                BuildingSettingsViewModel.SelectedBuildingInfluence = BuildingSettingsViewModel.BuildingInfluences.Single(x => x.Type == BuildingInfluenceType.Radius);
            }
            else if (obj.InfluenceRange > 0)
            {
                BuildingSettingsViewModel.SelectedBuildingInfluence = BuildingSettingsViewModel.BuildingInfluences.Single(x => x.Type == BuildingInfluenceType.Distance);

                if (obj.PavedStreet)
                {
                    BuildingSettingsViewModel.IsPavedStreet = obj.PavedStreet;
                }
            }
            else
            {
                BuildingSettingsViewModel.SelectedBuildingInfluence = BuildingSettingsViewModel.BuildingInfluences.Single(x => x.Type == BuildingInfluenceType.None);
            }

            // flags            
            //BuildingSettingsViewModel.IsEnableLabelChecked = !string.IsNullOrEmpty(obj.Label);
            BuildingSettingsViewModel.IsBorderlessChecked = obj.Borderless;
            BuildingSettingsViewModel.IsRoadChecked = obj.Road;
        }

        private void AnnoCanvas_StatisticsUpdated(object sender, EventArgs e)
        {
            UpdateStatistics();
        }

        private void AnnoCanvas_ClipboardChanged(List<AnnoObject> itemsOnClipboard)
        {
            StatusMessageClipboard = StatusBarItemsOnClipboard + ": " + itemsOnClipboard.Count;
        }

        private void AnnoCanvas_StatusMessageChanged(string message)
        {
            StatusMessage = message;
            logger.Trace($"Status message changed: {message}");
        }

        private void AnnoCanvas_LoadedFileChanged(string filename)
        {
            MainWindowTitle = string.IsNullOrEmpty(filename) ? "Anno Designer" : string.Format("{0} - Anno Designer", Path.GetFileName(filename));
            logger.Info($"Loaded file: {(string.IsNullOrEmpty(filename) ? "(none)" : filename)}");
        }

        public void UpdateStatistics()
        {
            StatisticsViewModel.UpdateStatistics(AnnoCanvas.PlacedObjects,
                AnnoCanvas.SelectedObjects,
                AnnoCanvas.BuildingPresets);
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
                MessageBox.Show("Error checking version. \n\nAdded more information to log.", "Version check failed");
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

            var isNewReleaseAvailable = foundRelease.Version > new Version(AnnoCanvas.BuildingPresets.Version);
            if (isNewReleaseAvailable)
            {
                string language = Localization.Localization.GetLanguageCodeFromName(_commons.SelectedLanguage);

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

        /// <summary>
        /// Called when localisation is changed, to repopulate the tree view
        /// </summary>
        private void RepopulateTreeView()
        {
            if (AnnoCanvas.BuildingPresets != null)
            {
                var treeState = PresetsTreeViewModel.GetCondensedTreeState();

                PresetsTreeViewModel.LoadItems(AnnoCanvas.BuildingPresets);

                PresetsTreeViewModel.SetCondensedTreeState(treeState, AnnoCanvas.BuildingPresets.Version);
            }
        }

        public void LoadAvailableIcons()
        {
            foreach (var icon in AnnoCanvas.Icons)
            {
                AvailableIcons.Add(icon.Value);
            }
        }

        public void LoadSettings()
        {
            StatisticsViewModel.ToggleBuildingList(_appSettings.StatsShowBuildingCount, AnnoCanvas.PlacedObjects, AnnoCanvas.SelectedObjects, AnnoCanvas.BuildingPresets);

            AutomaticUpdateCheck = _appSettings.EnableAutomaticUpdateCheck;

            UseCurrentZoomOnExportedImageValue = _appSettings.UseCurrentZoomOnExportedImageValue;
            RenderSelectionHighlightsOnExportedImageValue = _appSettings.RenderSelectionHighlightsOnExportedImageValue;

            CanvasShowGrid = _appSettings.ShowGrid;
            CanvasShowIcons = _appSettings.ShowIcons;
            CanvasShowLabels = _appSettings.ShowLabels;

            BuildingSettingsViewModel.IsPavedStreet = _appSettings.IsPavedStreet;
        }

        public void LoadPresets()
        {
            var presets = AnnoCanvas.BuildingPresets;
            if (presets == null)
            {
                PresetsSectionHeader = "Building presets - load failed";
                return;
            }

            PresetsSectionHeader = string.Format("Building presets - loaded v{0}", presets.Version);

            PresetsVersionValue = presets.Version;
            PresetsTreeViewModel.LoadItems(presets);

            RestoreSearchAndFilter();
        }

        private void RestoreSearchAndFilter()
        {
            var isFiltered = false;

            //apply saved search before restoring state
            if (!string.IsNullOrWhiteSpace(_appSettings.TreeViewSearchText))
            {
                PresetsTreeSearchViewModel.SearchText = _appSettings.TreeViewSearchText;
                isFiltered = true;
            }

            if (Enum.TryParse<CoreConstants.GameVersion>(_appSettings.PresetsTreeGameVersionFilter, ignoreCase: true, out var parsedValue))
            {
                //if all games were deselected on last app run, now select all
                if (parsedValue == CoreConstants.GameVersion.Unknown)
                {
                    foreach (CoreConstants.GameVersion curGameVersion in Enum.GetValues(typeof(CoreConstants.GameVersion)))
                    {
                        if (curGameVersion == CoreConstants.GameVersion.Unknown || curGameVersion == CoreConstants.GameVersion.All)
                        {
                            continue;
                        }

                        parsedValue |= curGameVersion;
                    }
                }

                PresetsTreeSearchViewModel.SelectedGameVersions = parsedValue;
                isFiltered = true;
            }
            else
            {
                //if saved value is not known, now select all
                parsedValue = CoreConstants.GameVersion.Unknown;

                foreach (CoreConstants.GameVersion curGameVersion in Enum.GetValues(typeof(CoreConstants.GameVersion)))
                {
                    if (curGameVersion == CoreConstants.GameVersion.Unknown || curGameVersion == CoreConstants.GameVersion.All)
                    {
                        continue;
                    }

                    parsedValue |= curGameVersion;
                }

                PresetsTreeSearchViewModel.SelectedGameVersions = parsedValue;
            }

            //if not filtered, then restore tree state
            if (!isFiltered && !string.IsNullOrWhiteSpace(_appSettings.PresetsTreeExpandedState))
            {
                Dictionary<int, bool> savedTreeState = null;
                using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(_appSettings.PresetsTreeExpandedState)))
                {
                    savedTreeState = SerializationHelper.LoadFromStream<Dictionary<int, bool>>(ms);
                }

                PresetsTreeViewModel.SetCondensedTreeState(savedTreeState, _appSettings.PresetsTreeLastVersion);
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
                _annoCanvas.OnCurrentObjectChanged += UpdateUIFromObject;
                _annoCanvas.OnStatusMessageChanged += AnnoCanvas_StatusMessageChanged;
                _annoCanvas.OnLoadedFileChanged += AnnoCanvas_LoadedFileChanged;
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

        public ObservableCollection<SupportedLanguage> Languages
        {
            get { return _languages; }
            set { UpdateProperty(ref _languages, value); }
        }

        private void InitLanguageMenu(string selectedLanguage)
        {
            //unselect all other languages
            foreach (var curLanguage in Languages)
            {
                curLanguage.IsSelected = string.Equals(curLanguage.Name, selectedLanguage, StringComparison.OrdinalIgnoreCase);
            }
        }

        public ObservableCollection<IconImage> AvailableIcons
        {
            get { return _availableIcons; }
            set { UpdateProperty(ref _availableIcons, value); }
        }

        public IconImage SelectedIcon
        {
            get { return _selectedIcon; }
            set { UpdateProperty(ref _selectedIcon, value); }
        }

        public string MainWindowTitle
        {
            get { return _mainWindowTitle; }
            set { UpdateProperty(ref _mainWindowTitle, value); }
        }

        public string PresetsSectionHeader
        {
            get { return _presetsSectionHeader; }
            set { UpdateProperty(ref _presetsSectionHeader, value); }
        }

        #endregion

        #region commands

        public ICommand OpenProjectHomepageCommand { get; private set; }

        private void OpenProjectHomepage(object param)
        {
            Process.Start("https://github.com/AgmasGold/anno-designer/");
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

        public ICommand LoadLayoutFromJsonCommand { get; private set; }

        private void ExecuteLoadLayoutFromJson(object param)
        {
            string language = Localization.Localization.GetLanguageCodeFromName(_commons.SelectedLanguage);

            var input = InputWindow.Prompt(this, Localization.Localization.Translations[language]["LoadLayoutMessage"],
                Localization.Localization.Translations[language]["LoadLayoutHeader"]);

            ExecuteLoadLayoutFromJsonSub(input, false);
        }

        private void ExecuteLoadLayoutFromJsonSub(string jsonString, bool forceLoad = false)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(jsonString))
                {
                    var jsonArray = Encoding.UTF8.GetBytes(jsonString);
                    using (var ms = new MemoryStream(jsonArray))
                    {
                        var loadedLayout = _layoutLoader.LoadLayout(ms, forceLoad);
                        if (loadedLayout != null)
                        {
                            AnnoCanvas.SelectedObjects.Clear();
                            AnnoCanvas.PlacedObjects = loadedLayout;
                            AnnoCanvas.LoadedFile = string.Empty;
                            AnnoCanvas.Normalize(1);

                            UpdateStatistics();
                        }
                    }
                }
            }
            catch (LayoutFileVersionMismatchException layoutEx)
            {
                logger.Warn(layoutEx, "Version of layout does not match.");

                if (MessageBox.Show(
                        "Try loading anyway?\nThis is very likely to fail or result in strange things happening.",
                        "File version mismatch", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    ExecuteLoadLayoutFromJsonSub(jsonString, true);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error loading layout from JSON.");
                MessageBox.Show("Something went wrong while loading the layout.",
                        Localization.Localization.Translations[Localization.Localization.GetLanguageCodeFromName(_commons.SelectedLanguage)]["Error"],
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
            }
        }

        public ICommand UnregisterExtensionCommand { get; private set; }

        private void UnregisterExtension(object param)
        {
            var regCheckADFileExtension = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default).OpenSubKey(@"Software\Classes\anno_designer", false);
            if (regCheckADFileExtension != null)
            {
                // removes the registry entries when exists          
                Registry.CurrentUser.DeleteSubKeyTree(@"Software\Classes\anno_designer");
                Registry.CurrentUser.DeleteSubKeyTree(@"Software\Classes\.ad");

                ShowRegistrationMessageBox(isDeregistration: true);
            }
        }

        public ICommand RegisterExtensionCommand { get; private set; }

        private void RegisterExtension(object param)
        {
            // registers the anno_designer class type and adds the correct command string to pass a file argument to the application
            Registry.SetValue(@"HKEY_CURRENT_USER\Software\Classes\anno_designer\shell\open\command", null, string.Format("\"{0}\" \"%1\"", App.ExecutablePath));
            Registry.SetValue(@"HKEY_CURRENT_USER\Software\Classes\anno_designer\DefaultIcon", null, string.Format("\"{0}\",0", App.ExecutablePath));
            // registers the .ad file extension to the anno_designer class
            Registry.SetValue(@"HKEY_CURRENT_USER\Software\Classes\.ad", null, "anno_designer");

            ShowRegistrationMessageBox(isDeregistration: false);
        }

        private void ShowRegistrationMessageBox(bool isDeregistration)
        {
            var language = Localization.Localization.GetLanguageCodeFromName(_commons.SelectedLanguage);
            var message = isDeregistration ? Localization.Localization.Translations[language]["UnregisterFileExtensionSuccessful"] : Localization.Localization.Translations[language]["RegisterFileExtensionSuccessful"];

            MessageBox.Show(message,
                Localization.Localization.Translations[language]["Successful"],
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        public ICommand ExportImageCommand { get; private set; }

        private void ExecuteExportImage(object param)
        {
            ExecuteExportImageSub(UseCurrentZoomOnExportedImageValue, RenderSelectionHighlightsOnExportedImageValue);
        }

        /// <summary>
        /// Renders the current layout to file.
        /// </summary>
        /// <param name="exportZoom">indicates whether the current zoom level should be applied, if false the default zoom is used</param>
        /// <param name="exportSelection">indicates whether selection and influence highlights should be rendered</param>
        private void ExecuteExportImageSub(bool exportZoom, bool exportSelection)
        {
            var dialog = new SaveFileDialog
            {
                DefaultExt = Constants.ExportedImageExtension,
                Filter = Constants.ExportDialogFilter
            };

            if (!string.IsNullOrEmpty(AnnoCanvas.LoadedFile))
            {
                // default the filename to the same name as the saved layout
                dialog.FileName = Path.GetFileNameWithoutExtension(AnnoCanvas.LoadedFile);
            }

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    RenderToFile(dialog.FileName, 1, exportZoom, exportSelection, StatisticsViewModel.IsVisible);

                    MessageBox.Show(//this,
                        Localization.Localization.Translations[Localization.Localization.GetLanguageCodeFromName(_commons.SelectedLanguage)]["ExportImageSuccessful"],
                        Localization.Localization.Translations[Localization.Localization.GetLanguageCodeFromName(_commons.SelectedLanguage)]["Successful"],
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Error exporting image.");
                    MessageBox.Show("Something went wrong while exporting the image.",
                        Localization.Localization.Translations[Localization.Localization.GetLanguageCodeFromName(_commons.SelectedLanguage)]["Error"],
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Asynchronously renders the current layout to file.
        /// </summary>
        /// <param name="filename">filename of the output image</param>
        /// <param name="border">normalization value used prior to exporting</param>
        /// <param name="exportZoom">indicates whether the current zoom level should be applied, if false the default zoom is used</param>
        /// <param name="exportSelection">indicates whether selection and influence highlights should be rendered</param>
        private void RenderToFile(string filename, int border, bool exportZoom, bool exportSelection, bool renderStatistics)
        {
            if (AnnoCanvas.PlacedObjects.Count == 0)
            {
                return;
            }

            // copy all objects
            var allObjects = AnnoCanvas.PlacedObjects.Select(_ => new AnnoObject(_)).Cast<AnnoObject>().ToList();
            // copy selected objects
            // note: should be references to the correct copied objects from allObjects
            var selectedObjects = AnnoCanvas.SelectedObjects.Select(_ => new AnnoObject(_)).ToList();

            logger.Trace($"UI thread: {Thread.CurrentThread.ManagedThreadId} ({Thread.CurrentThread.Name})");
            void renderThread()
            {
                logger.Trace($"Render thread: {Thread.CurrentThread.ManagedThreadId} ({Thread.CurrentThread.Name})");

                var sw = new Stopwatch();
                sw.Start();

                var icons = new Dictionary<string, IconImage>(StringComparer.OrdinalIgnoreCase);
                foreach (var curIcon in AnnoCanvas.Icons)
                {
                    icons.Add(curIcon.Key, new IconImage(curIcon.Value.Name, curIcon.Value.Localizations, curIcon.Value.IconPath));
                }

                // initialize output canvas
                var target = new AnnoCanvas(AnnoCanvas.BuildingPresets, icons, _coordinateHelper)
                {
                    PlacedObjects = allObjects,
                    RenderGrid = AnnoCanvas.RenderGrid,
                    RenderIcon = AnnoCanvas.RenderIcon,
                    RenderLabel = AnnoCanvas.RenderLabel
                };

                sw.Stop();
                logger.Trace($"creating canvas took: {sw.ElapsedMilliseconds}ms");

                // normalize layout
                target.Normalize(border);
                // set zoom level
                if (exportZoom)
                {
                    target.GridSize = AnnoCanvas.GridSize;
                }
                // set selection
                if (exportSelection)
                {
                    target.SelectedObjects.AddRange(selectedObjects);
                }

                // calculate output size
                var width = _coordinateHelper.GridToScreen(target.PlacedObjects.Max(_ => _.Position.X + _.Size.Width) + border, target.GridSize);//if +1 then there are weird black lines next to the statistics view
                var height = _coordinateHelper.GridToScreen(target.PlacedObjects.Max(_ => _.Position.Y + _.Size.Height) + border, target.GridSize) + 1;//+1 for black grid line at bottom

                if (renderStatistics)
                {
                    var exportStatisticsViewModel = new StatisticsViewModel(_commons);

                    var exportStatisticsView = new StatisticsView
                    {
                        Margin = new Thickness(5, 0, 0, 0)
                    };
                    exportStatisticsView.DataContext = exportStatisticsViewModel;

                    exportStatisticsViewModel.UpdateStatistics(target.PlacedObjects, target.SelectedObjects, target.BuildingPresets);
                    exportStatisticsViewModel.CopyLocalization(StatisticsViewModel);
                    exportStatisticsViewModel.ShowBuildingList = StatisticsViewModel.ShowBuildingList;

                    target.StatisticsPanel.Children.Add(exportStatisticsView);

                    //according to https://stackoverflow.com/a/25507450
                    // and https://stackoverflow.com/a/1320666
                    exportStatisticsView.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    //exportStatisticsView.Arrange(new Rect(new Point(0, 0), exportStatisticsView.DesiredSize));

                    if (exportStatisticsView.DesiredSize.Height > height)
                    {
                        height = exportStatisticsView.DesiredSize.Height + target.LinePenThickness + border;
                    }

                    width += exportStatisticsView.DesiredSize.Width + target.LinePenThickness;
                }

                target.Width = width;
                target.Height = height;
                target.UpdateLayout();

                // apply size
                var outputSize = new Size(width, height);
                target.Measure(outputSize);
                target.Arrange(new Rect(outputSize));

                // render canvas to file
                DataIO.RenderToFile(target, filename);
            }

            var thread = new Thread(renderThread);
            thread.IsBackground = true;
            thread.Name = "exportImage";
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join(TimeSpan.FromSeconds(10));
        }

        public ICommand CopyLayoutToClipboardCommand { get; private set; }

        private void ExecuteCopyLayoutToClipboard(object param)
        {
            ExecuteCopyLayoutToClipboardSub();
        }

        private void ExecuteCopyLayoutToClipboardSub()
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    AnnoCanvas.Normalize(1);
                    _layoutLoader.SaveLayout(AnnoCanvas.PlacedObjects, ms);

                    var jsonString = Encoding.UTF8.GetString(ms.ToArray());

                    Clipboard.SetText(jsonString, TextDataFormat.UnicodeText);

                    string language = Localization.Localization.GetLanguageCodeFromName(_commons.SelectedLanguage);

                    MessageBox.Show(Localization.Localization.Translations[language]["ClipboardContainsLayoutAsJson"],
                        Localization.Localization.Translations[language]["Successful"],
                        MessageBoxButton.OK,
                        MessageBoxImage.Information,
                        MessageBoxResult.OK);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error saving layout to JSON.");
                MessageBox.Show(ex.Message, "Something went wrong while saving the layout.");
            }
        }

        public ICommand LanguageSelectedCommand { get; private set; }

        private void ExecuteLanguageSelected(object param)
        {
            if (IsLanguageChange)
            {
                return;
            }

            try
            {
                IsLanguageChange = true;

                if (!(param is SupportedLanguage selectedLanguage))
                {
                    return;
                }

                InitLanguageMenu(selectedLanguage.Name);

                _commons.SelectedLanguage = selectedLanguage.Name;
            }
            finally
            {
                IsLanguageChange = false;
            }
        }

        public ICommand ShowAboutWindowCommand { get; private set; }

        private void ExecuteShowAboutWindow(object param)
        {
            var aboutWindow = new About
            {
                Owner = Application.Current.MainWindow
            };

            aboutWindow.DataContext = AboutViewModel;
            aboutWindow.ShowDialog();
        }

        public ICommand ShowWelcomeWindowCommand { get; private set; }

        private void ExecuteShowWelcomeWindow(object param)
        {
            var welcomeWindow = new Welcome
            {
                Owner = Application.Current.MainWindow
            };
            welcomeWindow.DataContext = WelcomeViewModel;
            welcomeWindow.Show();
        }

        public ICommand CheckForUpdatesCommand { get; private set; }

        private async void ExecuteCheckForUpdates(object param)
        {
            await CheckForUpdatesSub(true);
        }

        public ICommand ShowStatisticsCommand { get; private set; }

        private void ExecuteShowStatistics(object param)
        {
            ShowStatisticsChanged?.Invoke(this, EventArgs.Empty);
        }

        public ICommand ShowStatisticsBuildingCountCommand { get; private set; }

        private void ExecuteShowStatisticsBuildingCount(object param)
        {
            StatisticsViewModel.ToggleBuildingList(StatisticsViewModel.ShowStatisticsBuildingCount, AnnoCanvas.PlacedObjects, AnnoCanvas.SelectedObjects, AnnoCanvas.BuildingPresets);
        }

        public ICommand PlaceBuildingCommand { get; private set; }

        private void ExecutePlaceBuilding(object param)
        {
            try
            {
                ApplyCurrentObject();
            }
            catch (Exception)
            {
                MessageBox.Show("Error: Invalid building configuration.");
            }
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
            AboutViewModel.UpdateLanguage();

            string language = Localization.Localization.GetLanguageCodeFromName(_commons.SelectedLanguage);

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


