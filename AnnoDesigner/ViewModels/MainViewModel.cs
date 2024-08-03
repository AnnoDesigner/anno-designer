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
using AnnoDesigner.Extensions;
using AnnoDesigner.Helper;
using AnnoDesigner.Localization;
using AnnoDesigner.Models;
using AnnoDesigner.PreferencesPages;
using AnnoDesigner.Undo.Operations;
using Microsoft.Win32;
using NLog;
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

namespace AnnoDesigner.ViewModels;

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
    private bool _canvasShowPanorama;
    private bool _useCurrentZoomOnExportedImageValue;
    private bool _renderSelectionHighlightsOnExportedImageValue;
    private bool _renderVersionOnExportedImageValue;
    private bool _isLanguageChange;
    private bool _isBusy;
    private string _statusMessage;
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
    private string statusMessageClipboard;
    private readonly TreeLocalizationContainer _treeLocalizationContainer;

    //for identifier checking process
    private static readonly List<string> IconFieldNamesCheck = ["icon_116_22", "icon_27_6", "field", "general_module"];
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
        _appSettings.SettingsChanged += AppSettings_SettingsChanged;
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

        StatisticsViewModel = new StatisticsViewModel(_localizationHelper, _commons, appSettingsToUse)
        {
            IsVisible = _appSettings.StatsShowStats,
            ShowStatisticsBuildingCount = _appSettings.StatsShowBuildingCount
        };

        BuildingSettingsViewModel = new BuildingSettingsViewModel(_appSettings, _messageBoxService, _localizationHelper);

        // load tree localization            
        try
        {
            _treeLocalizationContainer = treeLocalizationLoader.LoadFromFile(_fileSystem.Path.Combine(App.ApplicationPath, CoreConstants.PresetsFiles.TreeLocalizationFile));
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
        LayoutSettingsViewModel.PropertyChangedWithValues += LayoutSettingsViewModel_PropertyChangedWithValues;

        OpenProjectHomepageCommand = new RelayCommand(OpenProjectHomepage);
        CloseWindowCommand = new RelayCommand<ICloseable>(CloseWindow);
        CanvasResetZoomCommand = new RelayCommand(CanvasResetZoom);
        CanvasNormalizeCommand = new RelayCommand(CanvasNormalize);
        MergeRoadsCommand = new RelayCommand(MergeRoads);
        LoadLayoutFromJsonCommand = new RelayCommand(async (obj) => await ExecuteLoadLayoutFromJsonAsync(obj));
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
        OpenRecentFileCommand = new RelayCommand(async (obj) => await ExecuteOpenRecentFileAsync(obj));

        AvailableIcons = [];
        _noIconItem = GenerateNoIconItem();
        AvailableIcons.Add(_noIconItem);
        SelectedIcon = _noIconItem;

        RecentFiles = [];
        _recentFilesHelper.Updated += RecentFilesHelper_Updated;

        Languages =
        [
            new SupportedLanguage("English")
            {
                FlagPath = "Flags/United Kingdom.png"
            },
            new SupportedLanguage("Deutsch")
            {
                FlagPath = "Flags/Germany.png"
            },
            new SupportedLanguage("Français")
            {
                FlagPath = "Flags/France.png"
            },
            new SupportedLanguage("Polski")
            {
                FlagPath = "Flags/Poland.png"
            },
            new SupportedLanguage("Русский")
            {
                FlagPath = "Flags/Russia.png"
            },
            new SupportedLanguage("Español")
            {
                FlagPath = "Flags/Spain.png"
            },
        ];
        //Languages.Add(new SupportedLanguage("Italiano"));
        //Languages.Add(new SupportedLanguage("český"));

        MainWindowTitle = "Anno Designer";
        PresetsSectionHeader = "Building presets - not loaded";

        PreferencesUpdateViewModel.VersionValue = Constants.Version.ToString();
        PreferencesUpdateViewModel.FileVersionValue = CoreConstants.LayoutFileVersion.ToString("0.#", CultureInfo.InvariantCulture);

        RecentFilesHelper_Updated(this, EventArgs.Empty);
    }

    private void LayoutSettingsViewModel_PropertyChangedWithValues(object sender, PropertyChangedWithValuesEventArgs<object> e)
    {
        if (string.Equals(e.PropertyName, nameof(LayoutSettingsViewModel.LayoutVersion), StringComparison.OrdinalIgnoreCase))
        {
            AnnoCanvas.UndoManager.RegisterOperation(new ModifyLayoutVersionOperation()
            {
                LayoutSettingsViewModel = sender as LayoutSettingsViewModel,
                OldValue = e.OldValue as Version,
                NewValue = e.NewValue as Version,
            });
        }
    }

    private IconImage GenerateNoIconItem()
    {
        Dictionary<string, string> localizations = [];

        foreach (string curLanguageCode in _commons.LanguageCodeMap.Values)
        {
            string curTranslationOfNone = _localizationHelper.GetLocalization("NoIcon", curLanguageCode);
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

    private void AppSettings_SettingsChanged(object sender, EventArgs e)
    {
        _ = UpdateStatisticsAsync(UpdateMode.All);
    }

    private void RecentFilesHelper_Updated(object sender, EventArgs e)
    {
        RecentFiles.Clear();

        foreach (RecentFile curRecentFile in _recentFilesHelper.RecentFiles)
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
            CoreConstants.GameVersion filterGameVersion = CoreConstants.GameVersion.Unknown;

            foreach (GameVersionFilter curSelectedFilter in PresetsTreeSearchViewModel.SelectedGameVersionFilters)
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
                AnnoObject copySelectedItem = new(selectedItem);
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
        AnnoObject obj = new()
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

        string objIconFileName = "";
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
            BuildingInfo buildingInfo = AnnoCanvas.BuildingPresets.Buildings.FirstOrDefault(_ => _.IconFileName?.Equals(objIconFileName, StringComparison.OrdinalIgnoreCase) ?? false);
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
    /// <param name="layoutObject"></param>
    private void UpdateUIFromObject(LayoutObject layoutObject)
    {
        AnnoObject obj = layoutObject?.WrappedAnnoObject;
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
                IconImage foundIconImage = AvailableIcons.SingleOrDefault(x => x.Name.Equals(_fileSystem.Path.GetFileNameWithoutExtension(obj.Icon), StringComparison.OrdinalIgnoreCase));
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
            _ = BuildingSettingsViewModel.GetDistanceRange(true, AnnoCanvas.BuildingPresets.Buildings.FirstOrDefault(_ => _.Identifier == BuildingSettingsViewModel.BuildingIdentifier));
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

    private void AnnoCanvas_StatusMessageChanged(string message)
    {
        StatusMessage = message;
    }

    private void AnnoCanvas_LoadedFileChanged(object sender, FileLoadedEventArgs args)
    {
        string fileName = string.Empty;
        Version layoutVersion = args.Layout?.LayoutVersion ?? LayoutSettingsViewModel.LayoutVersion;
        if (!string.IsNullOrWhiteSpace(args.FilePath) && layoutVersion != default)
        {
            fileName = $"{_fileSystem.Path.GetFileName(args.FilePath)} ({layoutVersion})";
            LayoutSettingsViewModel.LayoutVersion = layoutVersion;
        }
        else if (!string.IsNullOrWhiteSpace(args.FilePath))
        {
            fileName = _fileSystem.Path.GetFileName(args.FilePath);
        }

        MainWindowTitle = string.IsNullOrEmpty(fileName) ? "Anno Designer" : string.Format("{0} - Anno Designer", fileName);

        if (!string.IsNullOrWhiteSpace(args.FilePath))
        {
            logger.Info($"Loaded file: {(string.IsNullOrEmpty(args.FilePath) ? "(none)" : args.FilePath)}");

            _recentFilesHelper.AddFile(new RecentFile(args.FilePath, DateTime.UtcNow));
        }
    }

    private void AnnoCanvas_OpenFileRequested(object sender, OpenFileEventArgs e)
    {
        _ = OpenFileAsync(e.FilePath);
    }

    private void AnnoCanvas_SaveFileRequested(object sender, SaveFileEventArgs e)
    {
        SaveFile(e.FilePath);
    }

    public Task UpdateStatisticsAsync(UpdateMode mode)
    {
        return StatisticsViewModel is null || AnnoCanvas is null
            ? Task.CompletedTask
            : StatisticsViewModel.UpdateStatisticsAsync(mode,
            [.. AnnoCanvas.PlacedObjects],
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
            Dictionary<int, bool> treeState = PresetsTreeViewModel.GetCondensedTreeState();

            PresetsTreeViewModel.LoadItems(AnnoCanvas.BuildingPresets);

            PresetsTreeViewModel.SetCondensedTreeState(treeState, AnnoCanvas.BuildingPresets.Version);
        }
    }

    public void LoadAvailableIcons()
    {
        foreach (KeyValuePair<string, IconImage> icon in AnnoCanvas.Icons.OrderBy(x => x.Value.NameForLanguage(_commons.CurrentLanguageCode)))
        {
            AvailableIcons.Add(icon.Value);
        }
    }

    public void LoadSettings()
    {
        StatisticsViewModel.ToggleBuildingList(_appSettings.StatsShowBuildingCount, [.. AnnoCanvas.PlacedObjects], AnnoCanvas.SelectedObjects, AnnoCanvas.BuildingPresets);

        PreferencesUpdateViewModel.AutomaticUpdateCheck = _appSettings.EnableAutomaticUpdateCheck;
        PreferencesUpdateViewModel.UpdateSupportsPrerelease = _appSettings.UpdateSupportsPrerelease;
        PreferencesUpdateViewModel.ShowMultipleInstanceWarning = _appSettings.ShowMultipleInstanceWarning;

        UseCurrentZoomOnExportedImageValue = _appSettings.UseCurrentZoomOnExportedImageValue;
        RenderSelectionHighlightsOnExportedImageValue = _appSettings.RenderSelectionHighlightsOnExportedImageValue;
        RenderVersionOnExportedImageValue = _appSettings.RenderVersionOnExportedImageValue;

        CanvasShowGrid = _appSettings.ShowGrid;
        CanvasShowIcons = _appSettings.ShowIcons;
        CanvasShowLabels = _appSettings.ShowLabels;
        CanvasShowTrueInfluenceRange = _appSettings.ShowTrueInfluenceRange;
        CanvasShowInfluences = _appSettings.ShowInfluences;
        CanvasShowHarborBlockedArea = _appSettings.ShowHarborBlockedArea;
        CanvasShowPanorama = _appSettings.ShowPanorama;

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
        _appSettings.ShowPanorama = CanvasShowPanorama;

        _appSettings.StatsShowStats = StatisticsViewModel.IsVisible;
        _appSettings.StatsShowBuildingCount = StatisticsViewModel.ShowStatisticsBuildingCount;

        _appSettings.EnableAutomaticUpdateCheck = PreferencesUpdateViewModel.AutomaticUpdateCheck;
        _appSettings.UpdateSupportsPrerelease = PreferencesUpdateViewModel.UpdateSupportsPrerelease;
        _appSettings.ShowMultipleInstanceWarning = PreferencesUpdateViewModel.ShowMultipleInstanceWarning;

        _appSettings.UseCurrentZoomOnExportedImageValue = UseCurrentZoomOnExportedImageValue;
        _appSettings.RenderSelectionHighlightsOnExportedImageValue = RenderSelectionHighlightsOnExportedImageValue;
        _appSettings.RenderVersionOnExportedImageValue = RenderVersionOnExportedImageValue;

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

        Dictionary<string, HotkeyInformation> remappedHotkeys = HotkeyCommandManager.GetRemappedHotkeys();
        _appSettings.HotkeyMappings = SerializationHelper.SaveToJsonString(remappedHotkeys);

        _appSettings.Save();
    }

    public void LoadPresets()
    {
        BuildingPresets presets = AnnoCanvas.BuildingPresets;
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
        bool isFiltered = false;

        //apply saved search before restoring state
        if (!string.IsNullOrWhiteSpace(_appSettings.TreeViewSearchText))
        {
            PresetsTreeSearchViewModel.SearchText = _appSettings.TreeViewSearchText;
            isFiltered = true;
        }

        if (Enum.TryParse<CoreConstants.GameVersion>(_appSettings.PresetsTreeGameVersionFilter, ignoreCase: true, out CoreConstants.GameVersion parsedValue))
        {
            //if all games were deselected on last app run, now select all
            if (parsedValue == CoreConstants.GameVersion.Unknown)
            {
                foreach (CoreConstants.GameVersion curGameVersion in Enum.GetValues<CoreConstants.GameVersion>())
                {
                    if (curGameVersion is CoreConstants.GameVersion.Unknown or CoreConstants.GameVersion.All)
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

            foreach (CoreConstants.GameVersion curGameVersion in Enum.GetValues<CoreConstants.GameVersion>())
            {
                if (curGameVersion is CoreConstants.GameVersion.Unknown or CoreConstants.GameVersion.All)
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
    public async Task OpenFileAsync(string filePath, bool forceLoad = false)
    {
        try
        {
            LayoutFile layout = _layoutLoader.LoadLayout(filePath, forceLoad);
            if (layout != null)
            {
                OpenLayout(layout);
            }

            AnnoCanvas.LoadedFile = filePath;

            AnnoCanvas.ForceRendering();

            AnnoCanvas_LoadedFileChanged(this, new FileLoadedEventArgs(filePath, layout));
        }
        catch (LayoutFileUnsupportedFormatException layoutEx)
        {
            logger.Warn(layoutEx, "Version of layout file is not supported.");

            if (await _messageBoxService.ShowQuestion(
                    _localizationHelper.GetLocalization("FileVersionUnsupportedMessage"),
                    _localizationHelper.GetLocalization("FileVersionUnsupportedTitle")))
            {
                await OpenFileAsync(filePath, true);
            }
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error loading layout from JSON.");

            IOErrorMessageBox(ex);
        }
    }

    /// <summary>
    /// Opens new layout from memory.
    /// </summary>
    public void OpenLayout(LayoutFile layout)
    {
        AnnoCanvas.SelectedObjects.Clear();
        AnnoCanvas.PlacedObjects.Clear();
        AnnoCanvas.UndoManager.Clear();

        List<LayoutObject> layoutObjects = new(layout.Objects.Count);
        foreach (AnnoObject curObj in layout.Objects)
        {
            layoutObjects.Add(new LayoutObject(curObj, _coordinateHelper, _brushCache, _penCache));
        }
        LayoutSettingsViewModel.LayoutVersion = layout.LayoutVersion;

        _ = AnnoCanvas.ComputeBoundingRect(layoutObjects);
        AnnoCanvas.PlacedObjects.AddRange(layoutObjects);

        AnnoCanvas.Normalize(1);
        AnnoCanvas.ResetViewport();

        AnnoCanvas.RaiseStatisticsUpdated(UpdateStatisticsEventArgs.All);
        AnnoCanvas.RaiseColorsInLayoutUpdated();
        AnnoCanvas.UndoManager.Clear();
    }

    /// <summary>
    /// Writes layout to file.
    /// </summary>
    public void SaveFile(string filePath)
    {
        try
        {
            AnnoCanvas.Normalize(1);
            LayoutFile layoutToSave = new(AnnoCanvas.PlacedObjects.Select(x => x.WrappedAnnoObject))
            {
                LayoutVersion = LayoutSettingsViewModel.LayoutVersion
            };
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
        get => _annoCanvas;
        set
        {
            if (_annoCanvas != null)
            {
                _annoCanvas.StatisticsUpdated -= AnnoCanvas_StatisticsUpdated;
            }

            _annoCanvas = value;
            _annoCanvas.StatisticsUpdated += AnnoCanvas_StatisticsUpdated;
            _annoCanvas.OnCurrentObjectChanged += UpdateUIFromObject;
            _annoCanvas.OnStatusMessageChanged += AnnoCanvas_StatusMessageChanged;
            _annoCanvas.OnLoadedFileChanged += AnnoCanvas_LoadedFileChanged;
            _annoCanvas.OpenFileRequested += AnnoCanvas_OpenFileRequested;
            _annoCanvas.SaveFileRequested += AnnoCanvas_SaveFileRequested;
            BuildingSettingsViewModel.AnnoCanvasToUse = _annoCanvas;

            _annoCanvas.RenderGrid = CanvasShowGrid;
            _annoCanvas.RenderIcon = CanvasShowIcons;
            _annoCanvas.RenderLabel = CanvasShowLabels;
            _annoCanvas.RenderTrueInfluenceRange = CanvasShowTrueInfluenceRange;
            _annoCanvas.RenderInfluences = CanvasShowInfluences;
            _annoCanvas.RenderHarborBlockedArea = CanvasShowHarborBlockedArea;
            _annoCanvas.RenderPanorama = CanvasShowPanorama;
        }
    }

    public bool CanvasShowGrid
    {
        get => _canvasShowGrid;
        set
        {
            _ = UpdateProperty(ref _canvasShowGrid, value);
            if (AnnoCanvas != null)
            {
                AnnoCanvas.RenderGrid = _canvasShowGrid;
            }
        }
    }

    public bool CanvasShowIcons
    {
        get => _canvasShowIcons;
        set
        {
            _ = UpdateProperty(ref _canvasShowIcons, value);
            if (AnnoCanvas != null)
            {
                AnnoCanvas.RenderIcon = _canvasShowIcons;
            }
        }
    }

    public bool CanvasShowLabels
    {
        get => _canvasShowLabels;
        set
        {
            _ = UpdateProperty(ref _canvasShowLabels, value);
            if (AnnoCanvas != null)
            {
                AnnoCanvas.RenderLabel = _canvasShowLabels;
            }
        }
    }

    public bool CanvasShowTrueInfluenceRange
    {
        get => _canvasShowTrueInfluenceRange;
        set
        {
            _ = UpdateProperty(ref _canvasShowTrueInfluenceRange, value);
            if (AnnoCanvas != null)
            {
                AnnoCanvas.RenderTrueInfluenceRange = _canvasShowTrueInfluenceRange;
            }
        }
    }

    public bool CanvasShowInfluences
    {
        get => _canvasShowInfluences;
        set
        {
            _ = UpdateProperty(ref _canvasShowInfluences, value);
            if (AnnoCanvas != null)
            {
                AnnoCanvas.RenderInfluences = _canvasShowInfluences;
            }
        }
    }

    public bool CanvasShowHarborBlockedArea
    {
        get => _canvasShowHarborBlockedArea;
        set
        {
            _ = UpdateProperty(ref _canvasShowHarborBlockedArea, value);
            if (AnnoCanvas != null)
            {
                AnnoCanvas.RenderHarborBlockedArea = _canvasShowHarborBlockedArea;
            }
        }
    }

    public bool CanvasShowPanorama
    {
        get => _canvasShowPanorama;
        set
        {
            _ = UpdateProperty(ref _canvasShowPanorama, value);
            if (AnnoCanvas != null)
            {
                AnnoCanvas.RenderPanorama = _canvasShowPanorama;
            }
        }
    }

    public bool UseCurrentZoomOnExportedImageValue
    {
        get => _useCurrentZoomOnExportedImageValue;
        set => UpdateProperty(ref _useCurrentZoomOnExportedImageValue, value);
    }

    public bool RenderSelectionHighlightsOnExportedImageValue
    {
        get => _renderSelectionHighlightsOnExportedImageValue;
        set => UpdateProperty(ref _renderSelectionHighlightsOnExportedImageValue, value);
    }

    public bool RenderVersionOnExportedImageValue
    {
        get => _renderVersionOnExportedImageValue;
        set => UpdateProperty(ref _renderVersionOnExportedImageValue, value);
    }

    public bool IsLanguageChange
    {
        get => _isLanguageChange;
        set => UpdateProperty(ref _isLanguageChange, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set => UpdateProperty(ref _isBusy, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => UpdateProperty(ref _statusMessage, value);
    }

    public string StatusMessageClipboard
    {
        get => statusMessageClipboard;
        set => UpdateProperty(ref statusMessageClipboard,value);
    }
    public ObservableCollection<SupportedLanguage> Languages
    {
        get => _languages;
        set => UpdateProperty(ref _languages, value);
    }

    private void InitLanguageMenu(string selectedLanguage)
    {
        //unselect all other languages
        foreach (SupportedLanguage curLanguage in Languages)
        {
            curLanguage.IsSelected = string.Equals(curLanguage.Name, selectedLanguage, StringComparison.OrdinalIgnoreCase);
        }
    }

    public ObservableCollection<IconImage> AvailableIcons
    {
        get => _availableIcons;
        set => UpdateProperty(ref _availableIcons, value);
    }

    public IconImage SelectedIcon
    {
        get => _selectedIcon;
        set => UpdateProperty(ref _selectedIcon, value);
    }

    public string MainWindowTitle
    {
        get => _mainWindowTitle;
        set => UpdateProperty(ref _mainWindowTitle, value);
    }

    public string PresetsSectionHeader
    {
        get => _presetsSectionHeader;
        set => UpdateProperty(ref _presetsSectionHeader, value);
    }

    public double MainWindowHeight
    {
        get => _mainWindowHeight;
        set => UpdateProperty(ref _mainWindowHeight, value);
    }

    public double MainWindowWidth
    {
        get => _mainWindowWidth;
        set => UpdateProperty(ref _mainWindowWidth, value);
    }

    public double MainWindowLeft
    {
        get => _mainWindowLeft;
        set => UpdateProperty(ref _mainWindowLeft, value);
    }

    public double MainWindowTop
    {
        get => _mainWindowTop;
        set => UpdateProperty(ref _mainWindowTop, value);
    }

    public WindowState MainWindowWindowState
    {
        get => _minWindowWindowState;
        set => UpdateProperty(ref _minWindowWindowState, value);
    }

    public HotkeyCommandManager HotkeyCommandManager
    {
        get => _hotkeyCommandManager;
        set => UpdateProperty(ref _hotkeyCommandManager, value);
    }

    public ObservableCollection<RecentFileItem> RecentFiles
    {
        get => _recentFiles;
        set
        {
            if (UpdateProperty(ref _recentFiles, value))
            {
                OnPropertyChanged(nameof(HasRecentFiles));
            }
        }
    }

    public bool HasRecentFiles => RecentFiles.Count > 0;

    #endregion

    #region Commands

    public ICommand OpenProjectHomepageCommand { get; private set; }

    private void OpenProjectHomepage(object param)
    {
        _ = Process.Start("https://github.com/AnnoDesigner/anno-designer/");
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
        AnnoCanvas.ResetViewport();
    }

    public ICommand MergeRoadsCommand { get; private set; }

    /// <summary>
    /// Filters all roads in current layout, finds largest groups of them and replaces them with merged variants.
    /// Respects road color during merging.
    /// </summary>
    public void MergeRoads(object param)
    {
        IEnumerable<IGrouping<(bool Borderless, SerializableColor Color), LayoutObject>> roadColorGroups = AnnoCanvas.PlacedObjects.Where(p => p.WrappedAnnoObject.Road).GroupBy(p => (p.WrappedAnnoObject.Borderless, p.Color));
        foreach (IGrouping<(bool Borderless, SerializableColor Color), LayoutObject> roadColorGroup in roadColorGroups)
        {
            if (roadColorGroup.Count() <= 1)
            {
                continue;
            }

            Rect bounds = (Rect)new StatisticsCalculationHelper().CalculateStatistics(roadColorGroup.Select(p => p.WrappedAnnoObject));

            LayoutObject[][] cells = Enumerable.Range(0, (int)bounds.Width).Select(i => new LayoutObject[(int)bounds.Height]).ToArray();
            foreach (LayoutObject item in roadColorGroup)
            {
                for (int i = 0; i < item.Size.Width; i++)
                {
                    for (int j = 0; j < item.Size.Height; j++)
                    {
                        cells[(int)(item.Position.X + i - bounds.Left)][(int)(item.Position.Y + j - bounds.Top)] = item;
                    }
                }
            }

            List<CellGroup<LayoutObject>> groups = _adjacentCellGrouper.GroupAdjacentCells(cells).ToList();
            AnnoCanvas.UndoManager.AsSingleUndoableOperation(() =>
            {
                List<LayoutObject> oldObjects = groups.SelectMany(g => g.Items).ToList();
                foreach (LayoutObject item in oldObjects)
                {
                    _ = AnnoCanvas.PlacedObjects.Remove(item);
                }
                List<LayoutObject> newObjects = groups
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

    private async Task ExecuteLoadLayoutFromJsonAsync(object param)
    {
        ArgumentNullException.ThrowIfNull(param);

        if (!await AnnoCanvas.CheckUnsavedChanges())
        {
            return;
        }

        string input = InputWindow.Prompt(this, _localizationHelper.GetLocalization("LoadLayoutMessage"),
            _localizationHelper.GetLocalization("LoadLayoutHeader"));

        await ExecuteLoadLayoutFromJsonSubAsync(input, false);
    }

    private async Task ExecuteLoadLayoutFromJsonSubAsync(string jsonString, bool forceLoad = false)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(jsonString))
            {
                byte[] jsonArray = Encoding.UTF8.GetBytes(jsonString);
                using MemoryStream ms = new(jsonArray);
                LayoutFile loadedLayout = _layoutLoader.LoadLayout(ms, forceLoad);
                if (loadedLayout != null)
                {
                    AnnoCanvas.SelectedObjects.Clear();
                    AnnoCanvas.PlacedObjects.Clear();
                    AnnoCanvas.PlacedObjects.AddRange(loadedLayout.Objects.Select(x => new LayoutObject(x, _coordinateHelper, _brushCache, _penCache)));

                    AnnoCanvas.UndoManager.Clear();

                    AnnoCanvas.LoadedFile = string.Empty;
                    AnnoCanvas.Normalize(1);
                    AnnoCanvas.ResetViewport();

                    _ = UpdateStatisticsAsync(UpdateMode.All);
                }
            }
        }
        catch (LayoutFileUnsupportedFormatException layoutEx)
        {
            logger.Warn(layoutEx, "Version of layout does not match.");

            if (await _messageBoxService.ShowQuestion(
                    _localizationHelper.GetLocalization("FileVersionMismatchMessage"),
                    _localizationHelper.GetLocalization("FileVersionMismatchTitle")))
            {
                await ExecuteLoadLayoutFromJsonSubAsync(jsonString, true);
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
        RegistryKey regCheckADFileExtension = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default).OpenSubKey(@"Software\Classes\anno_designer", false);
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
        Registry.SetValue(Constants.FileAssociationRegistryKey, null, string.Format("\"{0}\" open \"%1\"", App.ExecutablePath));
        Registry.SetValue(@"HKEY_CURRENT_USER\Software\Classes\anno_designer\DefaultIcon", null, string.Format("\"{0}\",0", App.ExecutablePath));
        // registers the .ad file extension to the anno_designer class
        Registry.SetValue(@"HKEY_CURRENT_USER\Software\Classes\.ad", null, "anno_designer");

        ShowRegistrationMessageBox(isDeregistration: false);
    }

    public static void UpdateRegisteredExtension()
    {
        if (string.Format("\"{0}\" \"%1\"", App.ExecutablePath).Equals(Registry.GetValue(Constants.FileAssociationRegistryKey, null, null)))
        {
            Registry.SetValue(Constants.FileAssociationRegistryKey, null, string.Format("\"{0}\" open \"%1\"", App.ExecutablePath));
        }
    }

    private void ShowRegistrationMessageBox(bool isDeregistration)
    {
        string message = isDeregistration ? _localizationHelper.GetLocalization("UnregisterFileExtensionSuccessful") : _localizationHelper.GetLocalization("RegisterFileExtensionSuccessful");

        _messageBoxService.ShowMessage(message, _localizationHelper.GetLocalization("Successful"));
    }

    public ICommand ExportImageCommand { get; private set; }

    private void ExecuteExportImage(object param)
    {
        ExecuteExportImageSub(UseCurrentZoomOnExportedImageValue, RenderSelectionHighlightsOnExportedImageValue, RenderVersionOnExportedImageValue);
    }

    /// <summary>
    /// Renders the current layout to file.
    /// </summary>
    /// <param name="exportZoom">indicates whether the current zoom level should be applied, if false the default zoom is used</param>
    /// <param name="exportSelection">indicates whether selection and influence highlights should be rendered</param>
    private void ExecuteExportImageSub(bool exportZoom, bool exportSelection, bool exportVersion)
    {
        SaveFileDialog dialog = new()
        {
            DefaultExt = Constants.ExportedImageExtension,
            Filter = Constants.ExportDialogFilter
        };

        if (!string.IsNullOrEmpty(AnnoCanvas.LoadedFile))
        {
            // default the filename to the same name as the saved layout
            dialog.FileName = _fileSystem.Path.GetFileNameWithoutExtension(AnnoCanvas.LoadedFile);
        }

        if (dialog.ShowDialog() == true)
        {
            try
            {
                RenderToFile(dialog.FileName, 1, exportZoom, exportSelection, StatisticsViewModel.IsVisible, exportVersion);

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
    private void RenderToFile(string filename, int border, bool exportZoom, bool exportSelection, bool renderStatistics, bool renderVersion)
    {
        if (AnnoCanvas.PlacedObjects.Count == 0)
        {
            return;
        }

        logger.Trace($"UI thread: {Environment.CurrentManagedThreadId} ({Thread.CurrentThread.Name})");
        void renderThread()
        {
            FrameworkElement target = PrepareCanvasForRender(
                AnnoCanvas.PlacedObjects.Select(o => o.WrappedAnnoObject),
                exportSelection ? AnnoCanvas.SelectedObjects.Select(o => o.WrappedAnnoObject) : [],
                border,
                new CanvasRenderSetting()
                {
                    GridSize = exportZoom ? AnnoCanvas.GridSize : null,
                    RenderGrid = AnnoCanvas.RenderGrid,
                    RenderHarborBlockedArea = AnnoCanvas.RenderHarborBlockedArea,
                    RenderIcon = AnnoCanvas.RenderIcon,
                    RenderInfluences = AnnoCanvas.RenderInfluences,
                    RenderLabel = AnnoCanvas.RenderLabel,
                    RenderPanorama = AnnoCanvas.RenderPanorama,
                    RenderTrueInfluenceRange = AnnoCanvas.RenderTrueInfluenceRange,
                    RenderStatistics = renderStatistics,
                    RenderVersion = renderVersion
                }
            );

            // render canvas to file
            target.RenderToFile(filename);
        }

        Thread thread = new(renderThread)
        {
            IsBackground = true,
            Name = "exportImage"
        };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        _ = thread.Join(TimeSpan.FromSeconds(10));
    }

    public FrameworkElement PrepareCanvasForRender(
        IEnumerable<AnnoObject> placedObjects,
        IEnumerable<AnnoObject> selectedObjects,
        int border,
        CanvasRenderSetting renderSettings = null)
    {
        renderSettings ??= new CanvasRenderSetting()
        {
            RenderGrid = true,
            RenderIcon = true
        };

        Stopwatch sw = new();
        sw.Start();

        Dictionary<string, IconImage> icons = new(StringComparer.OrdinalIgnoreCase);
        foreach (KeyValuePair<string, IconImage> curIcon in AnnoCanvas.Icons)
        {
            icons.Add(curIcon.Key, new IconImage(curIcon.Value.Name, curIcon.Value.Localizations, curIcon.Value.IconPath));
        }

        StatisticsCalculationResult statistics = new StatisticsCalculationHelper().CalculateStatistics(placedObjects, true, true);

        QuadTree<LayoutObject> quadTree = new((Rect)statistics);
        quadTree.AddRange(placedObjects.Select(o => new LayoutObject(o, _coordinateHelper, _brushCache, _penCache)));
        // initialize output canvas
        AnnoCanvas target = new(AnnoCanvas.BuildingPresets, icons, _appSettings, _coordinateHelper, _brushCache, _penCache, _messageBoxService)
        {
            PlacedObjects = quadTree,
            RenderGrid = renderSettings.RenderGrid,
            RenderIcon = renderSettings.RenderIcon,
            RenderLabel = renderSettings.RenderLabel,
            RenderHarborBlockedArea = renderSettings.RenderHarborBlockedArea,
            RenderPanorama = renderSettings.RenderPanorama,
            RenderTrueInfluenceRange = renderSettings.RenderTrueInfluenceRange,
            RenderInfluences = renderSettings.RenderInfluences,
        };

        sw.Stop();
        logger.Trace($"creating canvas took: {sw.ElapsedMilliseconds}ms");

        // normalize layout
        target.Normalize(border);

        // set zoom level
        if (renderSettings.GridSize.HasValue)
        {
            target.GridSize = renderSettings.GridSize.Value;
        }

        // set selection
        target.SelectedObjects.UnionWith(selectedObjects.Select(o => new LayoutObject(o, _coordinateHelper, _brushCache, _penCache)));

        // calculate output size
        double width = _coordinateHelper.GridToScreen(target.PlacedObjects.Max(_ => _.Position.X + _.Size.Width) + border, target.GridSize);//if +1 then there are weird black lines next to the statistics view
        double height = _coordinateHelper.GridToScreen(target.PlacedObjects.Max(_ => _.Position.Y + _.Size.Height) + border, target.GridSize) + 1;//+1 for black grid line at bottom

        if (renderSettings.RenderVersion)
        {
            VersionView versionView = new()
            {
                Context = LayoutSettingsViewModel
            };

            target.DockPanel.Children.Insert(0, versionView);
            DockPanel.SetDock(versionView, Dock.Bottom);

            versionView.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            height += versionView.DesiredSize.Height;
        }

        if (renderSettings.RenderStatistics)
        {
            StatisticsViewModel exportStatisticsViewModel = new(_localizationHelper, _commons, _appSettings);
            _ = exportStatisticsViewModel.UpdateStatisticsAsync(UpdateMode.All, [.. target.PlacedObjects], target.SelectedObjects, target.BuildingPresets);
            exportStatisticsViewModel.ShowBuildingList = StatisticsViewModel.ShowBuildingList;

            StatisticsView exportStatisticsView = new()
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
        Size outputSize = new(width, height);
        target.Measure(outputSize);
        target.Arrange(new Rect(outputSize));

        return target;
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
            using MemoryStream ms = new();
            AnnoCanvas.Normalize(1);
            LayoutFile layoutToSave = new(AnnoCanvas.PlacedObjects.Select(x => x.WrappedAnnoObject));
            _layoutLoader.SaveLayout(layoutToSave, ms);

            string jsonString = Encoding.UTF8.GetString(ms.ToArray());

            Clipboard.SetText(jsonString, TextDataFormat.UnicodeText);

            _messageBoxService.ShowMessage(_localizationHelper.GetLocalization("ClipboardContainsLayoutAsJson"),
                _localizationHelper.GetLocalization("Successful"));
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

            if (param is not SupportedLanguage selectedLanguage)
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
        About aboutWindow = new()
        {
            Owner = Application.Current.MainWindow,
            DataContext = AboutViewModel
        };
        _ = aboutWindow.ShowDialog();
    }

    public ICommand ShowWelcomeWindowCommand { get; private set; }

    private void ExecuteShowWelcomeWindow(object param)
    {
        Welcome welcomeWindow = new()
        {
            Owner = Application.Current.MainWindow,
            DataContext = WelcomeViewModel
        };
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
        StatisticsViewModel.ToggleBuildingList(StatisticsViewModel.ShowStatisticsBuildingCount, [.. AnnoCanvas.PlacedObjects], AnnoCanvas.SelectedObjects, AnnoCanvas.BuildingPresets);
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
        PreferencesWindow preferencesWindow = new()
        {
            Owner = Application.Current.MainWindow,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        PreferencesViewModel vm = new();

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

        preferencesWindow.DataContext = vm;
        preferencesWindow.ShowDialog();
    }

    public ICommand ShowLicensesWindowCommand { get; private set; }

    private void ExecuteShowLicensesWindow(object param)
    {
        LicensesWindow LicensesWindow = new()
        {
            Owner = Application.Current.MainWindow
        };
        _ = LicensesWindow.ShowDialog();
    }

    public ICommand OpenRecentFileCommand { get; private set; }

    private async Task ExecuteOpenRecentFileAsync(object param)
    {
        if (param is not RecentFileItem recentFile)
        {
            return;
        }

        if (_fileSystem.File.Exists(recentFile.Path))
        {
            if (!await AnnoCanvas.CheckUnsavedChanges())
            {
                return;
            }

            await OpenFileAsync(recentFile.Path);

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


