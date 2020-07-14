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
using AnnoDesigner.Core.Services;
using AnnoDesigner.CustomEventArgs;
using AnnoDesigner.Helper;
using AnnoDesigner.Localization;
using AnnoDesigner.Models;
using Microsoft.Win32;
using NLog;

namespace AnnoDesigner.ViewModels
{
    public class MainViewModel : Notify
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly ICommons _commons;
        private readonly IAppSettings _appSettings;
        private readonly ILayoutLoader _layoutLoader;
        private readonly ICoordinateHelper _coordinateHelper;
        private readonly IBrushCache _brushCache;
        private readonly IPenCache _penCache;
        private readonly IRecentFilesHelper _recentFilesHelper;
        private readonly IMessageBoxService _messageBoxService;

        public event EventHandler<EventArgs> ShowStatisticsChanged;

        private IAnnoCanvas _annoCanvas;
        private Dictionary<int, bool> _treeViewState;
        private bool _canvasShowGrid;
        private bool _canvasShowIcons;
        private bool _canvasShowLabels;
        private bool _canvasShowTrueInfluenceRange;
        private bool _canvasShowInfluences;
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
        private double _mainWindowHeight;
        private double _mainWindowWidth;
        private double _mainWindowLeft;
        private double _mainWindowTop;
        private WindowState _minWindowWindowState;
        private HotkeyCommandManager _hotkeyCommandManager;
        private ObservableCollection<RecentFileItem> _recentFiles;

        //for identifier checking process
        private static readonly List<string> IconFieldNamesCheck = new List<string> { "icon_116_22", "icon_27_6", "field", "general_module" };
        private readonly IconImage _noIconItem;

        public MainViewModel(ICommons commonsToUse,
            IAppSettings appSettingsToUse,
            IRecentFilesHelper recentFilesHelperToUse,
            IMessageBoxService messageBoxServiceToUse,
            ILayoutLoader layoutLoaderToUse = null,
            ICoordinateHelper coordinateHelperToUse = null,
            IBrushCache brushCacheToUse = null,
            IPenCache penCacheToUse = null)
        {
            _commons = commonsToUse;
            _commons.SelectedLanguageChanged += Commons_SelectedLanguageChanged;

            _appSettings = appSettingsToUse;
            _recentFilesHelper = recentFilesHelperToUse;
            _messageBoxService = messageBoxServiceToUse;

            _layoutLoader = layoutLoaderToUse ?? new LayoutLoader();
            _coordinateHelper = coordinateHelperToUse ?? new CoordinateHelper();
            _brushCache = brushCacheToUse ?? new BrushCache();
            _penCache = penCacheToUse ?? new PenCache();


            HotkeyCommandManager = new HotkeyCommandManager(Localization.Localization.Instance);

            StatisticsViewModel = new StatisticsViewModel();
            StatisticsViewModel.IsVisible = _appSettings.StatsShowStats;
            StatisticsViewModel.ShowStatisticsBuildingCount = _appSettings.StatsShowBuildingCount;

            BuildingSettingsViewModel = new BuildingSettingsViewModel(_appSettings, _messageBoxService);

            PresetsTreeViewModel = new PresetsTreeViewModel(new TreeLocalization(_commons), _commons);
            PresetsTreeViewModel.ApplySelectedItem += PresetTreeViewModel_ApplySelectedItem;

            PresetsTreeSearchViewModel = new PresetsTreeSearchViewModel();
            PresetsTreeSearchViewModel.PropertyChanged += PresetsTreeSearchViewModel_PropertyChanged;

            WelcomeViewModel = new WelcomeViewModel(_commons, _appSettings);

            AboutViewModel = new AboutViewModel();

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
            ShowPreferencesWindowCommand = new RelayCommand(ExecuteShowPreferencesWindow);
            ShowLicensesWindowCommand = new RelayCommand(ExecuteShowLicensesWindow);
            OpenRecentFileCommand = new RelayCommand(ExecuteOpenRecentFile);

            AvailableIcons = new ObservableCollection<IconImage>();
            _noIconItem = GenerateNoIconItem();
            AvailableIcons.Add(_noIconItem);
            SelectedIcon = _noIconItem;

            RecentFiles = new ObservableCollection<RecentFileItem>();
            _recentFilesHelper.Updated += RecentFilesHelper_Updated;

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

            RecentFilesHelper_Updated(this, EventArgs.Empty);
        }

        private IconImage GenerateNoIconItem()
        {
            var localizations = new Dictionary<string, string>();

            foreach (var curLanguageCode in Localization.Localization.LanguageCodeMap)
            {
                if (Localization.Localization.TranslationsRaw.TryGetValue(curLanguageCode.Value, out var foundTranslations))
                {
                    if (foundTranslations.TryGetValue("NoIcon", out var curTranslationOfNone))
                    {
                        localizations.Add(curLanguageCode.Value, curTranslationOfNone);
                    }
                }
            }

            return new IconImage("NoIcon") { Localizations = localizations };
        }

        private void Commons_SelectedLanguageChanged(object sender, EventArgs e)
        {
            try
            {
                InitLanguageMenu(_commons.SelectedLanguage);

                if (AnnoCanvas == null)
                {
                    return;
                }

                RepopulateTreeView();

                BuildingSettingsViewModel.UpdateLanguageBuildingInfluenceType();

                //Force a language update on the clipboard status item.
                if (!string.IsNullOrWhiteSpace(StatusMessageClipboard))
                {
                    AnnoCanvas_ClipboardChanged(AnnoCanvas.ClipboardObjects);
                }

                //update settings
                _appSettings.SelectedLanguage = _commons.SelectedLanguage;

                _ = UpdateStatisticsAsync(UpdateMode.All);

                PresetsTreeSearchViewModel.SearchText = string.Empty;
                HotkeyCommandManager.UpdateLanguage();

                AvailableIcons.Clear();
                AvailableIcons.Add(_noIconItem);
                LoadAvailableIcons();
                SelectedIcon = _noIconItem;
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

        private void RecentFilesHelper_Updated(object sender, EventArgs e)
        {
            RecentFiles.Clear();

            foreach (var curRecentFile in _recentFilesHelper.RecentFiles)
            {
                RecentFiles.Add(new RecentFileItem(curRecentFile.Path));
            }

            OnPropertyChanged(nameof(HasRecentFiles));
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
                    var copySelectedItem = new AnnoObject(selectedItem);
                    copySelectedItem.Color = ColorPresetsHelper.Instance.GetPredefinedColor(copySelectedItem) ?? BuildingSettingsViewModel.SelectedColor ?? Colors.Red;
                    UpdateUIFromObject(new LayoutObject(copySelectedItem, _coordinateHelper, _brushCache, _penCache));

                    ApplyCurrentObject();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error applying preset.");
                _messageBoxService.ShowError("Something went wrong while applying the preset.",
                   Localization.Localization.Translations["Error"]);
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

                AnnoCanvas.SetCurrentObject(new LayoutObject(obj, _coordinateHelper, _brushCache, _penCache));
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
        private void UpdateUIFromObject(LayoutObject layoutObject)
        {
            var obj = layoutObject?.WrappedAnnoObject;
            if (obj == null)
            {
                return;
            }

            // size
            BuildingSettingsViewModel.BuildingWidth = (int)layoutObject.Size.Width;
            BuildingSettingsViewModel.BuildingHeight = (int)layoutObject.Size.Height;
            // color
            BuildingSettingsViewModel.SelectedColor = layoutObject.Color;
            // label
            BuildingSettingsViewModel.BuildingName = obj.Label;
            // Identifier
            BuildingSettingsViewModel.BuildingIdentifier = layoutObject.Identifier;
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

        private void AnnoCanvas_StatisticsUpdated(object sender, UpdateStatisticsEventArgs e)
        {
            _ = UpdateStatisticsAsync(e.Mode);
        }

        private void AnnoCanvas_ClipboardChanged(List<LayoutObject> itemsOnClipboard)
        {
            StatusMessageClipboard = Localization.Localization.Translations["StatusBarItemsOnClipboard"] + ": " + itemsOnClipboard.Count;
        }

        private void AnnoCanvas_StatusMessageChanged(string message)
        {
            StatusMessage = message;
            logger.Trace($"Status message changed: {message}");
        }

        private void AnnoCanvas_LoadedFileChanged(string filePath)
        {
            MainWindowTitle = string.IsNullOrEmpty(filePath) ? "Anno Designer" : string.Format("{0} - Anno Designer", Path.GetFileName(filePath));
            logger.Info($"Loaded file: {(string.IsNullOrEmpty(filePath) ? "(none)" : filePath)}");

            _recentFilesHelper.AddFile(new RecentFile(filePath, DateTime.UtcNow));
        }

        public Task UpdateStatisticsAsync(UpdateMode mode)
        {
            return StatisticsViewModel.UpdateStatisticsAsync(mode,
                AnnoCanvas.PlacedObjects,
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
                    _messageBoxService.ShowError("Error checking version. \n\nAdded more information to log.", "Version check failed");
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
                    if (_messageBoxService.ShowQuestion("A newer version was found, do you want to visit the releases page?\nhttps://github.com/AgmasGold/anno-designer/releases\n\n Clicking 'Yes' will open a new tab in your web browser.",
                        "Update available"))
                    {
                        Process.Start("https://github.com/AnnoDesigner/anno-designer/releases");
                    }
                }
                else
                {
                    StatusMessage = "Version is up to date.";

                    if (forcedCheck)
                    {
                        _messageBoxService.ShowMessage("This version is up to date.",
                            "No updates found");
                    }
                }

                //If not already prompted
                if (!_appSettings.PromptedForAutoUpdateCheck)
                {
                    _appSettings.PromptedForAutoUpdateCheck = true;

                    if (_messageBoxService.ShowQuestion("Do you want to continue checking for a new version on startup?\n\nThis option can be changed from the help menu.",
                        "Continue checking for updates?"))
                    {
                        AutomaticUpdateCheck = false;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error checking version.");
                _messageBoxService.ShowError($"Error checking version.{Environment.NewLine}{Environment.NewLine}More information is found in the log.",
                   "Version check failed");
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
                if (_messageBoxService.ShowQuestion(Localization.Localization.Translations["UpdateAvailablePresetMessage"],
                    Localization.Localization.Translations["UpdateAvailableHeader"]))
                {
                    IsBusy = true;

                    if (!Commons.CanWriteInFolder())
                    {
                        //already asked for admin rights?
                        if (Environment.GetCommandLineArgs().Any(x => x.Trim().Equals(Constants.Argument_Ask_For_Admin, StringComparison.OrdinalIgnoreCase)))
                        {
                            _messageBoxService.ShowWarning($"You have no write access to the folder.{Environment.NewLine}The update can not be installed.",
                                  Localization.Localization.Translations["Error"]);

                            IsBusy = false;
                            return;
                        }

                        _messageBoxService.ShowMessage(Localization.Localization.Translations["UpdateRequiresAdminRightsMessage"],
                            Localization.Localization.Translations["AdminRightsRequired"]);

                        Commons.RestartApplication(true, Constants.Argument_Ask_For_Admin, App.ExecutablePath);
                    }

                    //Context is required here, do not use ConfigureAwait(false)
                    var newLocation = await _commons.UpdateHelper.DownloadReleaseAsync(foundRelease);
                    logger.Debug($"downloaded new preset ({foundRelease.Version}): {newLocation}");

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
            foreach (var icon in AnnoCanvas.Icons.OrderBy(x => x.Value.NameForLanguage(Localization.Localization.Instance.SelectedLanguage)))
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
            CanvasShowTrueInfluenceRange = _appSettings.ShowTrueInfluenceRange;
            CanvasShowInfluences = _appSettings.ShowInfluences;

            BuildingSettingsViewModel.IsPavedStreet = _appSettings.IsPavedStreet;

            MainWindowHeight = _appSettings.MainWindowHeight;
            MainWindowWidth = _appSettings.MainWindowWidth;
            MainWindowLeft = _appSettings.MainWindowLeft;
            MainWindowTop = _appSettings.MainWindowTop;
            MainWindowWindowState = _appSettings.MainWindowWindowState;
            HotkeyCommandManager.LoadHotkeyMappings(SerializationHelper.LoadFromJsonString<Dictionary<string, HotkeyInformation>>(_appSettings.HotkeyMappings));
        }

        public void SaveSettings()
        {
            _appSettings.IsPavedStreet = BuildingSettingsViewModel.IsPavedStreet;

            _appSettings.ShowGrid = CanvasShowGrid;
            _appSettings.ShowIcons = CanvasShowIcons;
            _appSettings.ShowLabels = CanvasShowLabels;
            _appSettings.ShowTrueInfluenceRange = CanvasShowTrueInfluenceRange;
            _appSettings.ShowInfluences = CanvasShowInfluences;

            _appSettings.StatsShowStats = StatisticsViewModel.IsVisible;
            _appSettings.StatsShowBuildingCount = StatisticsViewModel.ShowStatisticsBuildingCount;

            _appSettings.EnableAutomaticUpdateCheck = AutomaticUpdateCheck;

            _appSettings.UseCurrentZoomOnExportedImageValue = UseCurrentZoomOnExportedImageValue;
            _appSettings.RenderSelectionHighlightsOnExportedImageValue = RenderSelectionHighlightsOnExportedImageValue;

            string savedTreeState;
            savedTreeState = SerializationHelper.SaveToJsonString(PresetsTreeViewModel.GetCondensedTreeState());

            _appSettings.PresetsTreeExpandedState = savedTreeState;
            _appSettings.PresetsTreeLastVersion = PresetsTreeViewModel.BuildingPresetsVersion;

            _appSettings.TreeViewSearchText = PresetsTreeSearchViewModel.SearchText;
            _appSettings.PresetsTreeGameVersionFilter = PresetsTreeViewModel.FilterGameVersion.ToString();

            _appSettings.MainWindowHeight = MainWindowHeight;
            _appSettings.MainWindowWidth = MainWindowWidth;
            _appSettings.MainWindowLeft = MainWindowLeft;
            _appSettings.MainWindowTop = MainWindowTop;
            _appSettings.MainWindowWindowState = MainWindowWindowState;

            var remappedHotkeys = HotkeyCommandManager.GetRemappedHotkeys();
            _appSettings.HotkeyMappings = SerializationHelper.SaveToJsonString(remappedHotkeys);

            _appSettings.Save();
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
                Dictionary<int, bool> savedTreeState;
                savedTreeState = SerializationHelper.LoadFromJsonString<Dictionary<int, bool>>(_appSettings.PresetsTreeExpandedState);
                PresetsTreeViewModel.SetCondensedTreeState(savedTreeState, _appSettings.PresetsTreeLastVersion);
            }
        }

        #region properties

        public IAnnoCanvas AnnoCanvas
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
                if (AnnoCanvas != null)
                {
                    AnnoCanvas.RenderGrid = _canvasShowGrid;
                }
            }
        }

        public bool CanvasShowIcons
        {
            get { return _canvasShowIcons; }
            set
            {
                UpdateProperty(ref _canvasShowIcons, value);
                if (AnnoCanvas != null)
                {
                    AnnoCanvas.RenderIcon = _canvasShowIcons;
                }
            }
        }

        public bool CanvasShowLabels
        {
            get { return _canvasShowLabels; }
            set
            {
                UpdateProperty(ref _canvasShowLabels, value);
                if (AnnoCanvas != null)
                {
                    AnnoCanvas.RenderLabel = _canvasShowLabels;
                }
            }
        }

        public bool CanvasShowTrueInfluenceRange
        {
            get { return _canvasShowTrueInfluenceRange; }
            set
            {
                UpdateProperty(ref _canvasShowTrueInfluenceRange, value);
                if (AnnoCanvas != null)
                {
                    AnnoCanvas.RenderTrueInfluenceRange = _canvasShowTrueInfluenceRange;
                }
            }
        }

        public bool CanvasShowInfluences
        {
            get { return _canvasShowInfluences; }
            set
            {
                UpdateProperty(ref _canvasShowInfluences, value);
                if (AnnoCanvas != null)
                {
                    AnnoCanvas.RenderInfluences = _canvasShowInfluences;
                }
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

        public double MainWindowHeight
        {
            get { return _mainWindowHeight; }
            set { UpdateProperty(ref _mainWindowHeight, value); }
        }

        public double MainWindowWidth
        {
            get { return _mainWindowWidth; }
            set { UpdateProperty(ref _mainWindowWidth, value); }
        }

        public double MainWindowLeft
        {
            get { return _mainWindowLeft; }
            set { UpdateProperty(ref _mainWindowLeft, value); }
        }

        public double MainWindowTop
        {
            get { return _mainWindowTop; }
            set { UpdateProperty(ref _mainWindowTop, value); }
        }

        public WindowState MainWindowWindowState
        {
            get { return _minWindowWindowState; }
            set { UpdateProperty(ref _minWindowWindowState, value); }
        }

        public HotkeyCommandManager HotkeyCommandManager
        {
            get { return _hotkeyCommandManager; }
            set { UpdateProperty(ref _hotkeyCommandManager, value); }
        }

        public ObservableCollection<RecentFileItem> RecentFiles
        {
            get { return _recentFiles; }
            set
            {
                if (UpdateProperty(ref _recentFiles, value))
                {
                    OnPropertyChanged(nameof(HasRecentFiles));
                }
            }
        }

        public bool HasRecentFiles
        {
            get { return RecentFiles.Count > 0; }
        }

        #endregion

        #region Commands

        public ICommand OpenProjectHomepageCommand { get; private set; }

        private void OpenProjectHomepage(object param)
        {
            Process.Start("https://github.com/AnnoDesigner/anno-designer/");
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
            var input = InputWindow.Prompt(this, Localization.Localization.Translations["LoadLayoutMessage"],
                Localization.Localization.Translations["LoadLayoutHeader"]);

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

                            AnnoCanvas.PlacedObjects.Clear();
                            AnnoCanvas.PlacedObjects.AddRange(loadedLayout.Select(x => new LayoutObject(x, _coordinateHelper, _brushCache, _penCache)));
                            AnnoCanvas.LoadedFile = string.Empty;
                            AnnoCanvas.Normalize(1);

                            _ = UpdateStatisticsAsync(UpdateMode.All);
                        }
                    }
                }
            }
            catch (LayoutFileVersionMismatchException layoutEx)
            {
                logger.Warn(layoutEx, "Version of layout does not match.");

                if (_messageBoxService.ShowQuestion("Try loading anyway?\nThis is very likely to fail or result in strange things happening.",
                        "File version mismatch"))
                {
                    ExecuteLoadLayoutFromJsonSub(jsonString, true);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error loading layout from JSON.");
                _messageBoxService.ShowError("Something went wrong while loading the layout.",
                        Localization.Localization.Translations["Error"]);
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
            var message = isDeregistration ? Localization.Localization.Translations["UnregisterFileExtensionSuccessful"] : Localization.Localization.Translations["RegisterFileExtensionSuccessful"];

            _messageBoxService.ShowMessage(message, Localization.Localization.Translations["Successful"]);
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

                    _messageBoxService.ShowMessage(Application.Current.MainWindow,
                       Localization.Localization.Translations["ExportImageSuccessful"],
                       Localization.Localization.Translations["Successful"]);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Error exporting image.");
                    _messageBoxService.ShowError(Application.Current.MainWindow,
                        "Something went wrong while exporting the image.",
                        Localization.Localization.Translations["Error"]);
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
            var allObjects = AnnoCanvas.PlacedObjects.Select(_ => new LayoutObject(new AnnoObject(_.WrappedAnnoObject), _coordinateHelper, _brushCache, _penCache)).ToList();
            // copy selected objects
            // note: should be references to the correct copied objects from allObjects
            var selectedObjects = AnnoCanvas.SelectedObjects.Select(_ => new LayoutObject(new AnnoObject(_.WrappedAnnoObject), _coordinateHelper, _brushCache, _penCache)).ToList();

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
                var target = new AnnoCanvas(AnnoCanvas.BuildingPresets, icons, _coordinateHelper, _brushCache, _penCache, _messageBoxService)
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
                    var exportStatisticsViewModel = new StatisticsViewModel();

                    var exportStatisticsView = new StatisticsView
                    {
                        Margin = new Thickness(5, 0, 0, 0)
                    };
                    exportStatisticsView.DataContext = exportStatisticsViewModel;

                    exportStatisticsViewModel.UpdateStatisticsAsync(UpdateMode.All, target.PlacedObjects, target.SelectedObjects, target.BuildingPresets).GetAwaiter().GetResult(); ;
                    exportStatisticsViewModel.ShowBuildingList = StatisticsViewModel.ShowBuildingList;

                    target.StatisticsPanel.Children.Add(exportStatisticsView);

                    //fix wrong for wrong width: https://stackoverflow.com/q/27894477
                    exportStatisticsView.Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.Render);
                    //according to https://stackoverflow.com/a/25507450
                    //and https://stackoverflow.com/a/1320666
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
                    _layoutLoader.SaveLayout(AnnoCanvas.PlacedObjects.Select(x => x.WrappedAnnoObject).ToList(), ms);

                    var jsonString = Encoding.UTF8.GetString(ms.ToArray());

                    Clipboard.SetText(jsonString, TextDataFormat.UnicodeText);

                    _messageBoxService.ShowMessage(Localization.Localization.Translations["ClipboardContainsLayoutAsJson"],
                        Localization.Localization.Translations["Successful"]);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error saving layout to JSON.");
                _messageBoxService.ShowError(ex.Message, "Something went wrong while saving the layout.");
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
                _messageBoxService.ShowError("Error: Invalid building configuration.",
                   Localization.Localization.Translations["Error"]);
            }
        }

        public ICommand ShowPreferencesWindowCommand { get; private set; }

        private void ExecuteShowPreferencesWindow(object param)
        {
            var preferencesWindow = new PreferencesWindow(_appSettings, _commons, HotkeyCommandManager, _messageBoxService)
            {
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            preferencesWindow.Show();
        }

        public ICommand ShowLicensesWindowCommand { get; private set; }

        private void ExecuteShowLicensesWindow(object param)
        {
            var LicensesWindow = new LicensesWindow()
            {
                Owner = Application.Current.MainWindow
            };
            LicensesWindow.ShowDialog();
        }

        public ICommand OpenRecentFileCommand { get; private set; }

        private void ExecuteOpenRecentFile(object param)
        {
            if (!(param is RecentFileItem recentFile))
            {
                return;
            }

            AnnoCanvas.OpenFile(recentFile.Path);

            _recentFilesHelper.AddFile(new RecentFile(recentFile.Path, DateTime.UtcNow));
        }

        #endregion

        #region view models

        public StatisticsViewModel StatisticsViewModel { get; set; }

        public BuildingSettingsViewModel BuildingSettingsViewModel { get; set; }

        public PresetsTreeViewModel PresetsTreeViewModel { get; set; }

        public PresetsTreeSearchViewModel PresetsTreeSearchViewModel { get; set; }

        public WelcomeViewModel WelcomeViewModel { get; set; }

        public AboutViewModel AboutViewModel { get; set; }

        #endregion    
    }
}


