using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AnnoDesigner.Core;
using AnnoDesigner.Core.DataStructures;
using AnnoDesigner.Core.Extensions;
using AnnoDesigner.Core.Helper;
using AnnoDesigner.Core.Layout;
using AnnoDesigner.Core.Layout.Exceptions;
using AnnoDesigner.Core.Layout.Helper;
using AnnoDesigner.Core.Layout.Models;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Core.Presets.Helper;
using AnnoDesigner.Core.Presets.Loader;
using AnnoDesigner.Core.Presets.Models;
using AnnoDesigner.Core.Services;
using AnnoDesigner.CustomEventArgs;
using AnnoDesigner.Helper;
using AnnoDesigner.Localization;
using AnnoDesigner.Models;
using AnnoDesigner.PreferencesPages;
using AnnoDesigner.Undo.Operations;
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
        private readonly IAdjacentCellGrouper _adjacentCellGrouper;
        private readonly IRecentFilesHelper _recentFilesHelper;
        private readonly IMessageBoxService _messageBoxService;
        private readonly IUpdateHelper _updateHelper;
        private readonly ILocalizationHelper _localizationHelper;
        private readonly IFileSystem _fileSystem;

        public event EventHandler<EventArgs> ShowStatisticsChanged;

        private IAnnoCanvas _annoCanvas;
        private Dictionary<int, bool> _treeViewState;
        private bool _canvasShowGrid;
        private bool _canvasShowIcons;
        private bool _canvasShowLabels;
        private bool _canvasShowTrueInfluenceRange;
        private bool _canvasShowInfluences;
        private bool _canvasShowHarborBlockedArea;
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
        private TreeLocalizationContainer _treeLocalizationContainer;

        //for identifier checking process
        private static readonly List<string> IconFieldNamesCheck = new List<string> { "icon_116_22", "icon_27_6", "field", "general_module" };
        private readonly IconImage _noIconItem;

        public MainViewModel(ICommons commonsToUse,
            IAppSettings appSettingsToUse,
            IRecentFilesHelper recentFilesHelperToUse,
            IMessageBoxService messageBoxServiceToUse,
            IUpdateHelper updateHelperToUse,
            ILocalizationHelper localizationHelperToUse,
            IFileSystem fileSystemToUse,
            ILayoutLoader layoutLoaderToUse = null,
            ICoordinateHelper coordinateHelperToUse = null,
            IBrushCache brushCacheToUse = null,
            IPenCache penCacheToUse = null,
            IAdjacentCellGrouper adjacentCellGrouper = null,
            ITreeLocalizationLoader treeLocalizationLoader = null)
        {
            _commons = commonsToUse;
            _commons.SelectedLanguageChanged += Commons_SelectedLanguageChanged;

            _appSettings = appSettingsToUse;
            _recentFilesHelper = recentFilesHelperToUse;
            _messageBoxService = messageBoxServiceToUse;
            _updateHelper = updateHelperToUse;
            _localizationHelper = localizationHelperToUse;
            _fileSystem = fileSystemToUse;

            _layoutLoader = layoutLoaderToUse ?? new LayoutLoader();
            _coordinateHelper = coordinateHelperToUse ?? new CoordinateHelper();
            _brushCache = brushCacheToUse ?? new BrushCache();
            _penCache = penCacheToUse ?? new PenCache();
            _adjacentCellGrouper = adjacentCellGrouper ?? new AdjacentCellGrouper();

            HotkeyCommandManager = new HotkeyCommandManager(_localizationHelper);

            StatisticsViewModel = new StatisticsViewModel(_localizationHelper, _commons);
            StatisticsViewModel.IsVisible = _appSettings.StatsShowStats;
            StatisticsViewModel.ShowStatisticsBuildingCount = _appSettings.StatsShowBuildingCount;

            BuildingSettingsViewModel = new BuildingSettingsViewModel(_appSettings, _messageBoxService, _localizationHelper);

            // load tree localization            
            try
            {
                _treeLocalizationContainer = treeLocalizationLoader.LoadFromFile(Path.Combine(App.ApplicationPath, CoreConstants.PresetsFiles.TreeLocalizationFile));
            }
            catch (Exception ex)
            {
                _messageBoxService.ShowError(ex.Message,
                      _localizationHelper.GetLocalization("LoadingTreeLocalizationFailed"));
            }

            PresetsTreeViewModel = new PresetsTreeViewModel(new TreeLocalization(_commons, _treeLocalizationContainer), _commons);
            PresetsTreeViewModel.ApplySelectedItem += PresetTreeViewModel_ApplySelectedItem;

            PresetsTreeSearchViewModel = new PresetsTreeSearchViewModel();
            PresetsTreeSearchViewModel.PropertyChanged += PresetsTreeSearchViewModel_PropertyChanged;

            WelcomeViewModel = new WelcomeViewModel(_commons, _appSettings);

            AboutViewModel = new AboutViewModel();

            PreferencesUpdateViewModel = new UpdateSettingsViewModel(_commons, _appSettings, _messageBoxService, _updateHelper, _localizationHelper);
            PreferencesKeyBindingsViewModel = new ManageKeybindingsViewModel(HotkeyCommandManager, _commons, _messageBoxService, _localizationHelper);
            PreferencesGeneralViewModel = new GeneralSettingsViewModel(_appSettings, _commons, _recentFilesHelper);

            LayoutSettingsViewModel = new LayoutSettingsViewModel();

            OpenProjectHomepageCommand = new RelayCommand(OpenProjectHomepage);
            CloseWindowCommand = new RelayCommand<ICloseable>(CloseWindow);
            CanvasResetZoomCommand = new RelayCommand(CanvasResetZoom);
            CanvasNormalizeCommand = new RelayCommand(CanvasNormalize);
            MergeRoadsCommand = new RelayCommand(MergeRoads);
            LoadLayoutFromJsonCommand = new RelayCommand(ExecuteLoadLayoutFromJson);
            UnregisterExtensionCommand = new RelayCommand(UnregisterExtension);
            RegisterExtensionCommand = new RelayCommand(RegisterExtension);
            ExportImageCommand = new RelayCommand(ExecuteExportImage);
            CopyLayoutToClipboardCommand = new RelayCommand(ExecuteCopyLayoutToClipboard);
            LanguageSelectedCommand = new RelayCommand(ExecuteLanguageSelected);
            ShowAboutWindowCommand = new RelayCommand(ExecuteShowAboutWindow);
            ShowWelcomeWindowCommand = new RelayCommand(ExecuteShowWelcomeWindow);
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
            Languages.Add(new SupportedLanguage("Español")
            {
                FlagPath = "Flags/Spain.png"
            });
            //Languages.Add(new SupportedLanguage("Italiano"));
            //Languages.Add(new SupportedLanguage("český"));

            MainWindowTitle = "Anno Designer";
            PresetsSectionHeader = "Building presets - not loaded";

            PreferencesUpdateViewModel.VersionValue = Constants.Version.ToString();
            PreferencesUpdateViewModel.FileVersionValue = CoreConstants.LayoutFileVersion.ToString("0.#", CultureInfo.InvariantCulture);

            RecentFilesHelper_Updated(this, EventArgs.Empty);
        }

        private IconImage GenerateNoIconItem()
        {
            var localizations = new Dictionary<string, string>();

            foreach (var curLanguageCode in _commons.LanguageCodeMap.Values)
            {
                var curTranslationOfNone = _localizationHelper.GetLocalization("NoIcon", curLanguageCode);
                localizations.Add(curLanguageCode, curTranslationOfNone);
            }

            return new IconImage("NoIcon") { Localizations = localizations };
        }

        private void Commons_SelectedLanguageChanged(object sender, EventArgs e)
        {
            try
            {
                InitLanguageMenu(_commons.CurrentLanguage);

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
                _appSettings.SelectedLanguage = _commons.CurrentLanguage;

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
                _messageBoxService.ShowError(
                    _localizationHelper.GetLocalization("LayoutLoadingError"),
                   _localizationHelper.GetLocalization("Error"));
            }
        }

        private void ApplyCurrentObject()
        {
            // parse user inputs and create new object
            string RenameBuildingIdentifier = BuildingSettingsViewModel.BuildingName;
            string TextBoxText = "UnknownObject";
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
                Template = BuildingSettingsViewModel.BuildingTemplate,
                BlockedAreaLength = BuildingSettingsViewModel.BuildingBlockedAreaLength,
                BlockedAreaWidth = BuildingSettingsViewModel.BuildingBlockedAreaWidth,
                Direction = BuildingSettingsViewModel.BuildingDirection
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
                //gets icons and origin building info
                var buildingInfo = AnnoCanvas.BuildingPresets.Buildings.FirstOrDefault(_ => _.IconFileName?.Equals(objIconFileName, StringComparison.OrdinalIgnoreCase) ?? false);
                if (buildingInfo != null)
                {
                    //Put in the Blocked Area if there is one
                    if (buildingInfo.BlockedAreaLength > 0)
                    {
                        obj.BlockedAreaLength = buildingInfo.BlockedAreaLength;
                        obj.BlockedAreaWidth = buildingInfo.BlockedAreaWidth;
                        obj.Direction = buildingInfo.Direction;
                    }

                    //if user entered a new Label Name (as in renaming existing building or naming own building) then name and identifier will be renamed
                    if (BuildingSettingsViewModel.BuildingRealName != BuildingSettingsViewModel.BuildingName)
                    {
                        obj.Identifier = RenameBuildingIdentifier;
                        obj.Template = RenameBuildingIdentifier;
                    }
                }
                else
                {
                    //if no Identifier is found or if user entered a new Label Name (as in renaming existing building or naming own building) then name and identifier will be renamed
                    if (string.IsNullOrWhiteSpace(BuildingSettingsViewModel.BuildingIdentifier) || BuildingSettingsViewModel.BuildingRealName != BuildingSettingsViewModel.BuildingName)
                    {
                        if (!string.IsNullOrWhiteSpace(RenameBuildingIdentifier))
                        {
                            obj.Identifier = RenameBuildingIdentifier;
                            obj.Template = RenameBuildingIdentifier;
                        }
                        else
                        {
                            obj.Identifier = TextBoxText;
                        }
                    }
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
            BuildingSettingsViewModel.BuildingRealName = obj.Label;
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
            StatusMessageClipboard = _localizationHelper.GetLocalization("StatusBarItemsOnClipboard") + ": " + itemsOnClipboard.Count;
        }

        private void AnnoCanvas_StatusMessageChanged(string message)
        {
            StatusMessage = message;
        }

        private void AnnoCanvas_LoadedFileChanged(object sender, FileLoadedEventArgs args)
        {
            var fileName = string.Empty;
            if (!string.IsNullOrWhiteSpace(args.FilePath) && args.Layout?.LayoutVersion != default)
            {
                fileName = $"{Path.GetFileName(args.FilePath)} ({args.Layout.LayoutVersion})";
                LayoutSettingsViewModel.LayoutVersion = args.Layout.LayoutVersion;
            }
            else if (!string.IsNullOrWhiteSpace(args.FilePath))
            {
                fileName = Path.GetFileName(args.FilePath);
            }

            MainWindowTitle = string.IsNullOrEmpty(fileName) ? "Anno Designer" : string.Format("{0} - Anno Designer", fileName);

            logger.Info($"Loaded file: {(string.IsNullOrEmpty(args.FilePath) ? "(none)" : args.FilePath)}");

            _recentFilesHelper.AddFile(new RecentFile(args.FilePath, DateTime.UtcNow));
        }

        private void AnnoCanvas_OpenFileRequested(object sender, OpenFileEventArgs e)
        {
            OpenFile(e.FilePath);
        }

        private void AnnoCanvas_SaveFileRequested(object sender, SaveFileEventArgs e)
        {
            SaveFile(e.FilePath);
        }

        public Task UpdateStatisticsAsync(UpdateMode mode)
        {
            return StatisticsViewModel.UpdateStatisticsAsync(mode,
                AnnoCanvas.PlacedObjects.ToList(),
                AnnoCanvas.SelectedObjects,
                AnnoCanvas.BuildingPresets);
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
            foreach (var icon in AnnoCanvas.Icons.OrderBy(x => x.Value.NameForLanguage(_commons.CurrentLanguageCode)))
            {
                AvailableIcons.Add(icon.Value);
            }
        }

        public void LoadSettings()
        {
            StatisticsViewModel.ToggleBuildingList(_appSettings.StatsShowBuildingCount, AnnoCanvas.PlacedObjects.ToList(), AnnoCanvas.SelectedObjects, AnnoCanvas.BuildingPresets);

            PreferencesUpdateViewModel.AutomaticUpdateCheck = _appSettings.EnableAutomaticUpdateCheck;
            PreferencesUpdateViewModel.UpdateSupportsPrerelease = _appSettings.UpdateSupportsPrerelease;

            UseCurrentZoomOnExportedImageValue = _appSettings.UseCurrentZoomOnExportedImageValue;
            RenderSelectionHighlightsOnExportedImageValue = _appSettings.RenderSelectionHighlightsOnExportedImageValue;

            CanvasShowGrid = _appSettings.ShowGrid;
            CanvasShowIcons = _appSettings.ShowIcons;
            CanvasShowLabels = _appSettings.ShowLabels;
            CanvasShowTrueInfluenceRange = _appSettings.ShowTrueInfluenceRange;
            CanvasShowInfluences = _appSettings.ShowInfluences;
            CanvasShowHarborBlockedArea = _appSettings.ShowHarborBlockedArea;

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
            _appSettings.ShowHarborBlockedArea = CanvasShowHarborBlockedArea;

            _appSettings.StatsShowStats = StatisticsViewModel.IsVisible;
            _appSettings.StatsShowBuildingCount = StatisticsViewModel.ShowStatisticsBuildingCount;

            _appSettings.EnableAutomaticUpdateCheck = PreferencesUpdateViewModel.AutomaticUpdateCheck;
            _appSettings.UpdateSupportsPrerelease = PreferencesUpdateViewModel.UpdateSupportsPrerelease;

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

            PreferencesUpdateViewModel.PresetsVersionValue = presets.Version;
            PreferencesUpdateViewModel.ColorPresetsVersionValue = ColorPresetsHelper.Instance.PresetsVersion;
            PreferencesUpdateViewModel.TreeLocalizationVersionValue = _treeLocalizationContainer.Version;

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

        /// <summary>
        /// Loads a new layout from file.
        /// </summary>
        public void OpenFile(string filePath, bool forceLoad = false)
        {
            try
            {
                var layout = _layoutLoader.LoadLayout(filePath, forceLoad);
                if (layout != null)
                {
                    AnnoCanvas.SelectedObjects.Clear();
                    AnnoCanvas.PlacedObjects.Clear();
                    AnnoCanvas.UndoManager.Clear();

                    var layoutObjects = new List<LayoutObject>(layout.Objects.Count);
                    foreach (var curObj in layout.Objects)
                    {
                        layoutObjects.Add(new LayoutObject(curObj, _coordinateHelper, _brushCache, _penCache));
                    }

                    var bounds = AnnoCanvas.ComputeBoundingRect(layoutObjects);
                    AnnoCanvas.PlacedObjects.AddRange(layoutObjects);

                    AnnoCanvas.LoadedFile = filePath;
                    AnnoCanvas.Normalize(1);

                    AnnoCanvas_LoadedFileChanged(this, new FileLoadedEventArgs(filePath, layout));

                    AnnoCanvas.RaiseStatisticsUpdated(UpdateStatisticsEventArgs.All);
                    AnnoCanvas.RaiseColorsInLayoutUpdated();
                    AnnoCanvas.UndoManager.Clear();
                }
            }
            catch (LayoutFileUnsupportedFormatException layoutEx)
            {
                logger.Warn(layoutEx, "Version of layout file is not supported.");

                if (_messageBoxService.ShowQuestion(
                        _localizationHelper.GetLocalization("FileVersionUnsupportedMessage"),
                        _localizationHelper.GetLocalization("FileVersionUnsupportedTitle")))
                {
                    OpenFile(filePath, true);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error loading layout from JSON.");

                IOErrorMessageBox(ex);
            }
        }

        /// <summary>
        /// Writes layout to file.
        /// </summary>
        public void SaveFile(string filePath)
        {
            try
            {
                AnnoCanvas.Normalize(1);
                var layoutToSave = new LayoutFile(AnnoCanvas.PlacedObjects.Select(x => x.WrappedAnnoObject).ToList());
                layoutToSave.LayoutVersion = LayoutSettingsViewModel.LayoutVersion;
                _layoutLoader.SaveLayout(layoutToSave, filePath);
                AnnoCanvas.UndoManager.IsDirty = false;
            }
            catch (Exception e)
            {
                IOErrorMessageBox(e);
            }
        }

        /// <summary>
        /// Displays a message box containing some error information.
        /// </summary>
        /// <param name="e">exception containing error information</param>
        private void IOErrorMessageBox(Exception e)
        {
            _messageBoxService.ShowError(e.Message, _localizationHelper.GetLocalization("IOErrorMessage"));
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
                _annoCanvas.OpenFileRequested += AnnoCanvas_OpenFileRequested;
                _annoCanvas.SaveFileRequested += AnnoCanvas_SaveFileRequested;
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

        public bool CanvasShowHarborBlockedArea
        {
            get { return _canvasShowHarborBlockedArea; }
            set
            {
                UpdateProperty(ref _canvasShowHarborBlockedArea, value);
                if (AnnoCanvas != null)
                {
                    AnnoCanvas.RenderHarborBlockedArea = _canvasShowHarborBlockedArea;
                }
            }
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

        public ICommand MergeRoadsCommand { get; private set; }

        /// <summary>
        /// Filters all roads in current layout, finds largest groups of them and replaces them with merged variants.
        /// Respects road color during merging.
        /// </summary>
        public void MergeRoads(object param)
        {
            var roadColorGroups = AnnoCanvas.PlacedObjects.Where(p => p.WrappedAnnoObject.Road).GroupBy(p => (p.WrappedAnnoObject.Borderless, p.Color));
            foreach (var roadColorGroup in roadColorGroups)
            {
                if (roadColorGroup.Count() <= 1) continue;

                var bounds = (Rect)new StatisticsCalculationHelper().CalculateStatistics(roadColorGroup.Select(p => p.WrappedAnnoObject));

                var cells = Enumerable.Range(0, (int)bounds.Width).Select(i => new LayoutObject[(int)bounds.Height]).ToArray();
                foreach (var item in roadColorGroup)
                {
                    for (var i = 0; i < item.Size.Width; i++)
                    {
                        for (var j = 0; j < item.Size.Height; j++)
                        {
                            cells[(int)(item.Position.X + i - bounds.Left)][(int)(item.Position.Y + j - bounds.Top)] = item;
                        }
                    }
                }

                var groups = _adjacentCellGrouper.GroupAdjacentCells(cells).ToList();
                AnnoCanvas.UndoManager.AsSingleUndoableOperation(() =>
                {
                    var oldObjects = groups.SelectMany(g => g.Items).ToList();
                    foreach (var item in oldObjects)
                    {
                        AnnoCanvas.PlacedObjects.Remove(item);
                    }
                    var newObjects = groups
                        .Select(g => new LayoutObject(
                            new AnnoObject(g.Items.First().WrappedAnnoObject)
                            {
                                Position = g.Bounds.TopLeft + (Vector)bounds.TopLeft,
                                Size = g.Bounds.Size
                            },
                            _coordinateHelper,
                            _brushCache,
                            _penCache
                        ))
                        .ToList();
                    AnnoCanvas.PlacedObjects.AddRange(newObjects);

                    AnnoCanvas.UndoManager.RegisterOperation(new RemoveObjectsOperation<LayoutObject>()
                    {
                        Objects = oldObjects,
                        Collection = AnnoCanvas.PlacedObjects
                    });
                    AnnoCanvas.UndoManager.RegisterOperation(new AddObjectsOperation<LayoutObject>()
                    {
                        Objects = newObjects,
                        Collection = AnnoCanvas.PlacedObjects
                    });
                });
            }
        }

        public ICommand LoadLayoutFromJsonCommand { get; private set; }

        private void ExecuteLoadLayoutFromJson(object param)
        {
            if (!AnnoCanvas.CheckUnsavedChanges())
            {
                return;
            }

            var input = InputWindow.Prompt(this, _localizationHelper.GetLocalization("LoadLayoutMessage"),
                _localizationHelper.GetLocalization("LoadLayoutHeader"));

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
                            AnnoCanvas.PlacedObjects.AddRange(loadedLayout.Objects.Select(x => new LayoutObject(x, _coordinateHelper, _brushCache, _penCache)));

                            AnnoCanvas.UndoManager.Clear();

                            AnnoCanvas.LoadedFile = string.Empty;
                            AnnoCanvas.Normalize(1);

                            _ = UpdateStatisticsAsync(UpdateMode.All);
                        }
                    }
                }
            }
            catch (LayoutFileUnsupportedFormatException layoutEx)
            {
                logger.Warn(layoutEx, "Version of layout does not match.");

                if (_messageBoxService.ShowQuestion(
                        _localizationHelper.GetLocalization("FileVersionMismatchMessage"),
                        _localizationHelper.GetLocalization("FileVersionMismatchTitle")))
                {
                    ExecuteLoadLayoutFromJsonSub(jsonString, true);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error loading layout from JSON.");
                _messageBoxService.ShowError(_localizationHelper.GetLocalization("LayoutLoadingError"),
                        _localizationHelper.GetLocalization("Error"));
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
            var message = isDeregistration ? _localizationHelper.GetLocalization("UnregisterFileExtensionSuccessful") : _localizationHelper.GetLocalization("RegisterFileExtensionSuccessful");

            _messageBoxService.ShowMessage(message, _localizationHelper.GetLocalization("Successful"));
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
                       _localizationHelper.GetLocalization("ExportImageSuccessful"),
                       _localizationHelper.GetLocalization("Successful"));
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Error exporting image.");
                    _messageBoxService.ShowError(Application.Current.MainWindow,
                        _localizationHelper.GetLocalization("ExportImageError"),
                        _localizationHelper.GetLocalization("Error"));
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
        private void RenderToFile(string filename, int border, bool exportZoom, bool exportSelection, bool renderStatistics, bool renderVersion = true)
        {
            if (AnnoCanvas.PlacedObjects.Count() == 0)
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

                var quadTree = new QuadTree<LayoutObject>(AnnoCanvas.PlacedObjects.Extent);
                quadTree.AddRange(allObjects);
                // initialize output canvas
                var target = new AnnoCanvas(AnnoCanvas.BuildingPresets, icons, _appSettings, _coordinateHelper, _brushCache, _penCache, _messageBoxService)
                {
                    PlacedObjects = quadTree,
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

                if (renderVersion)
                {
                    var versionView = new VersionView()
                    {
                        Context = LayoutSettingsViewModel
                    };

                    target.DockPanel.Children.Insert(0, versionView);
                    DockPanel.SetDock(versionView, Dock.Bottom);

                    versionView.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                    height += versionView.DesiredSize.Height;
                }

                if (renderStatistics)
                {
                    var exportStatisticsViewModel = new StatisticsViewModel(_localizationHelper, _commons);
                    exportStatisticsViewModel.UpdateStatisticsAsync(UpdateMode.All, target.PlacedObjects.ToList(), target.SelectedObjects, target.BuildingPresets).GetAwaiter().GetResult(); ;
                    exportStatisticsViewModel.ShowBuildingList = StatisticsViewModel.ShowBuildingList;

                    var exportStatisticsView = new StatisticsView()
                    {
                        Context = exportStatisticsViewModel
                    };

                    target.DockPanel.Children.Insert(0, exportStatisticsView);
                    DockPanel.SetDock(exportStatisticsView, Dock.Right);

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
                    var layoutToSave = new LayoutFile(AnnoCanvas.PlacedObjects.Select(x => x.WrappedAnnoObject).ToList());
                    _layoutLoader.SaveLayout(layoutToSave, ms);

                    var jsonString = Encoding.UTF8.GetString(ms.ToArray());

                    Clipboard.SetText(jsonString, TextDataFormat.UnicodeText);

                    _messageBoxService.ShowMessage(_localizationHelper.GetLocalization("ClipboardContainsLayoutAsJson"),
                        _localizationHelper.GetLocalization("Successful"));
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error saving layout to JSON.");
                _messageBoxService.ShowError(ex.Message, _localizationHelper.GetLocalization("LayoutSavingError"));
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

                _commons.CurrentLanguage = selectedLanguage.Name;
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

        public ICommand ShowStatisticsCommand { get; private set; }

        private void ExecuteShowStatistics(object param)
        {
            ShowStatisticsChanged?.Invoke(this, EventArgs.Empty);
        }

        public ICommand ShowStatisticsBuildingCountCommand { get; private set; }

        private void ExecuteShowStatisticsBuildingCount(object param)
        {
            StatisticsViewModel.ToggleBuildingList(StatisticsViewModel.ShowStatisticsBuildingCount, AnnoCanvas.PlacedObjects.ToList(), AnnoCanvas.SelectedObjects, AnnoCanvas.BuildingPresets);
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
                _messageBoxService.ShowError(
                    _localizationHelper.GetLocalization("InvalidBuildingConfiguration"),
                   _localizationHelper.GetLocalization("Error"));
            }
        }

        public ICommand ShowPreferencesWindowCommand { get; private set; }

        private void ExecuteShowPreferencesWindow(object param)
        {
            var preferencesWindow = new PreferencesWindow()
            {
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            var vm = new PreferencesViewModel();
            preferencesWindow.DataContext = vm;

            vm.Pages.Add(new PreferencePage
            {
                Name = nameof(GeneralSettingsPage),
                ViewModel = PreferencesGeneralViewModel,
                HeaderKeyForTranslation = "GeneralSettings"
            });
            vm.Pages.Add(new PreferencePage
            {
                Name = nameof(ManageKeybindingsPage),
                ViewModel = PreferencesKeyBindingsViewModel,
                HeaderKeyForTranslation = "ManageKeybindings"
            });
            vm.Pages.Add(new PreferencePage
            {
                Name = nameof(UpdateSettingsPage),
                ViewModel = PreferencesUpdateViewModel,
                HeaderKeyForTranslation = "UpdateSettings"
            });

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

            if (_fileSystem.File.Exists(recentFile.Path))
            {
                if (!AnnoCanvas.CheckUnsavedChanges())
                {
                    return;
                }

                OpenFile(recentFile.Path);

                _recentFilesHelper.AddFile(new RecentFile(recentFile.Path, DateTime.UtcNow));
            }
            else
            {
                _recentFilesHelper.RemoveFile(new RecentFile(recentFile.Path, recentFile.LastUsed));
                _messageBoxService.ShowWarning(Application.Current.MainWindow,
                    _localizationHelper.GetLocalization("FileNotFound"),
                    _localizationHelper.GetLocalization("Warning"));
            }
        }

        #endregion

        #region view models

        public StatisticsViewModel StatisticsViewModel { get; set; }

        public BuildingSettingsViewModel BuildingSettingsViewModel { get; set; }

        public PresetsTreeViewModel PresetsTreeViewModel { get; set; }

        public PresetsTreeSearchViewModel PresetsTreeSearchViewModel { get; set; }

        public WelcomeViewModel WelcomeViewModel { get; set; }

        public AboutViewModel AboutViewModel { get; set; }

        public UpdateSettingsViewModel PreferencesUpdateViewModel { get; set; }

        public ManageKeybindingsViewModel PreferencesKeyBindingsViewModel { get; set; }

        public GeneralSettingsViewModel PreferencesGeneralViewModel { get; set; }

        public LayoutSettingsViewModel LayoutSettingsViewModel { get; set; }

        #endregion    
    }
}


