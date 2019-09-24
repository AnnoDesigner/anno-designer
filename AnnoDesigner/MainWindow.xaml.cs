using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using MessageBox = Xceed.Wpf.Toolkit.MessageBox;
using System.Windows.Media;
using System.ComponentModel;
using AnnoDesigner.Properties;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Generic;
using System.Threading;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AnnoDesigner.model;
using AnnoDesigner.Core.Extensions;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Core;
using AnnoDesigner.Core.Presets.Models;
using AnnoDesigner.Core.Presets.Helper;
using AnnoDesigner.Core.Layout.Models;
using AnnoDesigner.Core.Layout;
using System.Text;
using AnnoDesigner.Core.Layout.Exceptions;
using System.Configuration;
using AnnoDesigner.Core.Helper;
using AnnoDesigner.viewmodel;
using System.Windows.Threading;
using AnnoDesigner.Helper;
using NLog;

namespace AnnoDesigner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, ICloseable
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private IconImage _noIconItem;
        private static MainWindow _instance;

        private static string _selectedLanguage;
        //for identifier checking process
        private static readonly List<string> IconFieldNamesCheck = new List<string> { "icon_116_22", "icon_27_6", "field", "general_module" };

        public BuildingPresets BuildingPresets { get; }

        public static string SelectedLanguage
        {
            get
            {
                if (_selectedLanguage != null && Localization.Localization.LanguageCodeMap.ContainsKey(_selectedLanguage))
                {
                    return _selectedLanguage;
                }
                else
                {
                    _selectedLanguage = "English";
                    return _selectedLanguage;
                }
            }
            set
            {
                _selectedLanguage = value ?? "English";
                _instance?.SelectedLanguageChanged();
            }
        }

        private static MainViewModel _mainViewModel;

        private void SelectedLanguageChanged()
        {
            try
            {
                _mainViewModel.IsLanguageChange = true;

                _mainViewModel.UpdateLanguage();
                _instance.RepopulateTreeView();

                foreach (MenuItem item in LanguageMenu.Items)
                {
                    item.IsChecked = item.Header.ToString() == SelectedLanguage;
                }

                _mainViewModel.BuildingSettingsViewModel.UpdateLanguageBuildingInfluenceType();

                //Force a language update on the clipboard status item.
                if (StatusBarItemClipboardStatus.Content != null) ClipboardChanged(annoCanvas.ObjectClipboard);

                //update settings
                Settings.Default.SelectedLanguage = SelectedLanguage;

                _mainViewModel.UpdateStatistics();

                _mainViewModel.PresetsTreeSearchViewModel.SearchText = string.Empty;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error when changing the language.");
            }
            finally
            {
                _mainViewModel.IsLanguageChange = false;
            }
        }

        private readonly ICommons _commons;
        private readonly ILayoutLoader _layoutLoader;
        private readonly ICoordinateHelper _coordinateHelper;

        #region Initialization

        public MainWindow()
        {
            InitializeComponent();
        }

        public MainWindow(ICommons commonsToUse, ICoordinateHelper coordinateHelperToUse = null) : this()
        {
            _commons = commonsToUse;
            _layoutLoader = new LayoutLoader();
            _coordinateHelper = coordinateHelperToUse ?? new CoordinateHelper();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            _mainViewModel = DataContext as MainViewModel;
            _mainViewModel.AnnoCanvas = annoCanvas;

            App.DpiScale = VisualTreeHelper.GetDpi(this);

            _instance = this;
            // add event handlers
            annoCanvas.OnCurrentObjectChanged += UpdateUIFromObject;
            annoCanvas.OnStatusMessageChanged += StatusMessageChanged;
            annoCanvas.OnLoadedFileChanged += LoadedFileChanged;
            annoCanvas.OnClipboardChanged += ClipboardChanged;

            DpiChanged += MainWindow_DpiChanged;

            foreach (MenuItem item in LanguageMenu.Items)
            {
                if (item.Header.ToString() == SelectedLanguage)
                {
                    item.IsChecked = true;
                }
            }

            LoadSettings();

            // add icons to the combobox
            comboBoxIcon.Items.Clear();
            _noIconItem = new IconImage("None");
            comboBoxIcon.Items.Add(_noIconItem);
            foreach (var icon in annoCanvas.Icons)
            {
                comboBoxIcon.Items.Add(icon.Value);
            }
            comboBoxIcon.SelectedIndex = 0;

            _mainViewModel.VersionValue = Constants.Version.ToString("0.0#", CultureInfo.InvariantCulture);
            _mainViewModel.FileVersionValue = CoreConstants.LayoutFileVersion.ToString("0.#", CultureInfo.InvariantCulture);

            // check for updates on startup            
            _ = CheckForUpdates(false);//just fire and forget

            // load color presets
            colorPicker.StandardColors.Clear();
            //This is currently disabled
            colorPicker.ShowStandardColors = false;
            //try
            //{
            //    ColorPresetsLoader loader = new ColorPresetsLoader();
            //    var defaultScheme = loader.LoadDefaultScheme();
            //    foreach (var curPredefinedColor in defaultScheme.Colors.GroupBy(x => x.Color).Select(x => x.Key))
            //    {
            //        //colorPicker.StandardColors.Add(new Xceed.Wpf.Toolkit.ColorItem(curPredefinedColor.Color, $"{curPredefinedColor.TargetTemplate}"));
            //        colorPicker.StandardColors.Add(new Xceed.Wpf.Toolkit.ColorItem(curPredefinedColor, curPredefinedColor.ToHex()));
            //    }
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Message, "Loading of the color presets failed");
            //}

            // load presets
            BuildingPresets presets = annoCanvas.BuildingPresets;
            if (presets != null)
            {
                GroupBoxPresets.Header = string.Format("Building presets - loaded v{0}", presets.Version);

                _mainViewModel.PresetsVersionValue = presets.Version;
                _mainViewModel.PresetsTreeViewModel.ApplySelectedItem += PresetTreeViewModel_ApplySelectedItem;
                _mainViewModel.PresetsTreeViewModel.LoadItems(annoCanvas.BuildingPresets);

                var isFiltered = false;

                //apply saved search before restoring state
                if (!string.IsNullOrWhiteSpace(Settings.Default.TreeViewSearchText))
                {
                    _mainViewModel.PresetsTreeSearchViewModel.SearchText = Settings.Default.TreeViewSearchText;
                    isFiltered = true;
                }

                if (Enum.TryParse<CoreConstants.GameVersion>(Settings.Default.PresetsTreeGameVersionFilter, ignoreCase: true, out var parsedValue))
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

                    _mainViewModel.PresetsTreeSearchViewModel.SelectedGameVersions = parsedValue;
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

                    _mainViewModel.PresetsTreeSearchViewModel.SelectedGameVersions = parsedValue;
                }

                //if not filtered, then restore tree state
                if (!isFiltered && !string.IsNullOrWhiteSpace(Settings.Default.PresetsTreeExpandedState))
                {
                    Dictionary<int, bool> savedTreeState = null;
                    using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(Settings.Default.PresetsTreeExpandedState)))
                    {
                        savedTreeState = SerializationHelper.LoadFromStream<Dictionary<int, bool>>(ms);
                    }

                    _mainViewModel.PresetsTreeViewModel.SetCondensedTreeState(savedTreeState, Settings.Default.PresetsTreeLastVersion);
                }
            }
            else
            {
                GroupBoxPresets.Header = "Building presets - load failed";
            }

            // load file given by argument
            if (!string.IsNullOrEmpty(App.FilenameArgument))
            {
                annoCanvas.OpenFile(App.FilenameArgument);
            }
        }

        private void LoadSettings()
        {
            ToggleStatisticsView(Settings.Default.StatsShowStats);
            _mainViewModel.StatisticsViewModel.ToggleBuildingList(Settings.Default.StatsShowBuildingCount, annoCanvas.PlacedObjects, annoCanvas.SelectedObjects, annoCanvas.BuildingPresets);

            _mainViewModel.AutomaticUpdateCheck = Settings.Default.EnableAutomaticUpdateCheck;

            _mainViewModel.UseCurrentZoomOnExportedImageValue = Settings.Default.UseCurrentZoomOnExportedImageValue;
            _mainViewModel.RenderSelectionHighlightsOnExportedImageValue = Settings.Default.RenderSelectionHighlightsOnExportedImageValue;

            _mainViewModel.CanvasShowGrid = Settings.Default.ShowGrid;
            _mainViewModel.CanvasShowIcons = Settings.Default.ShowIcons;
            _mainViewModel.CanvasShowLabels = Settings.Default.ShowLabels;

            _mainViewModel.BuildingSettingsViewModel.IsPavedStreet = Settings.Default.IsPavedStreet;
        }

        #endregion

        private void PresetTreeViewModel_ApplySelectedItem(object sender, EventArgs e)
        {
            ApplyPreset(_mainViewModel.PresetsTreeViewModel.SelectedItem.AnnoObject);
        }

        #region Version check

        private async Task CheckForUpdates(bool forcedCheck)
        {
            if (_mainViewModel.AutomaticUpdateCheck || forcedCheck)
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
                    StatusMessageChanged("Version is up to date.");

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
                        _mainViewModel.AutomaticUpdateCheck = false;
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

            var isNewReleaseAvailable = foundRelease.Version > new Version(annoCanvas.BuildingPresets.Version);
            if (isNewReleaseAvailable)
            {
                string language = Localization.Localization.GetLanguageCodeFromName(SelectedLanguage);

                if (MessageBox.Show(Localization.Localization.Translations[language]["UpdateAvailablePresetMessage"],
                    Localization.Localization.Translations[language]["UpdateAvailableHeader"],
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Asterisk,
                    MessageBoxResult.OK) == MessageBoxResult.Yes)
                {
                    busyIndicator.IsBusy = true;

                    if (!Commons.CanWriteInFolder())
                    {
                        //already asked for admin rights?
                        if (Environment.GetCommandLineArgs().Any(x => x.Trim().Equals(Constants.Argument_Ask_For_Admin, StringComparison.OrdinalIgnoreCase)))
                        {
                            MessageBox.Show($"You have no write access to the folder.{Environment.NewLine}The update can not be installed.",
                                Localization.Localization.Translations[language]["Error"],
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);

                            busyIndicator.IsBusy = false;
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

                    busyIndicator.IsBusy = false;

                    Commons.RestartApplication(false, null, App.ExecutablePath);

                    Environment.Exit(-1);
                }
            }
        }

        #endregion

        #region AnnoCanvas events

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
            _mainViewModel.BuildingSettingsViewModel.BuildingWidth = (int)obj.Size.Width;
            _mainViewModel.BuildingSettingsViewModel.BuildingHeight = (int)obj.Size.Height;
            // color
            _mainViewModel.BuildingSettingsViewModel.SelectedColor = ColorPresetsHelper.Instance.GetPredefinedColor(obj) ?? obj.Color;
            // label
            _mainViewModel.BuildingSettingsViewModel.BuildingName = obj.Label;
            // Identifier
            _mainViewModel.BuildingSettingsViewModel.BuildingIdentifier = obj.Identifier;
            // Template
            _mainViewModel.BuildingSettingsViewModel.BuildingTemplate = obj.Template;
            // icon
            try
            {
                if (string.IsNullOrWhiteSpace(obj.Icon))
                {
                    comboBoxIcon.SelectedItem = _noIconItem;
                }
                else
                {
                    var foundIconImage = comboBoxIcon.Items.Cast<IconImage>().SingleOrDefault(x => x.Name.Equals(Path.GetFileNameWithoutExtension(obj.Icon), StringComparison.OrdinalIgnoreCase));
                    comboBoxIcon.SelectedItem = foundIconImage ?? _noIconItem;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error finding {nameof(IconImage)} for value \"{obj.Icon}\".{Environment.NewLine}{ex}");

                comboBoxIcon.SelectedItem = _noIconItem;
            }

            // radius
            _mainViewModel.BuildingSettingsViewModel.BuildingRadius = obj.Radius;
            //InfluenceRange
            if (!_mainViewModel.BuildingSettingsViewModel.IsPavedStreet)
            {
                _mainViewModel.BuildingSettingsViewModel.BuildingInfluenceRange = obj.InfluenceRange;
            }
            else
            {
                _mainViewModel.BuildingSettingsViewModel.GetDistanceRange(true, annoCanvas.BuildingPresets.Buildings.FirstOrDefault(_ => _.Identifier == _mainViewModel.BuildingSettingsViewModel.BuildingIdentifier));
            }

            //Set Influence Type
            if (obj.Radius > 0 && obj.InfluenceRange > 0)
            {
                //Building uses both a radius and an influence
                //Has to be set manually 
                _mainViewModel.BuildingSettingsViewModel.SelectedBuildingInfluence = _mainViewModel.BuildingSettingsViewModel.BuildingInfluences.Single(x => x.Type == BuildingInfluenceType.Both);
            }
            else if (obj.Radius > 0)
            {
                _mainViewModel.BuildingSettingsViewModel.SelectedBuildingInfluence = _mainViewModel.BuildingSettingsViewModel.BuildingInfluences.Single(x => x.Type == BuildingInfluenceType.Radius);
            }
            else if (obj.InfluenceRange > 0)
            {
                _mainViewModel.BuildingSettingsViewModel.SelectedBuildingInfluence = _mainViewModel.BuildingSettingsViewModel.BuildingInfluences.Single(x => x.Type == BuildingInfluenceType.Distance);

                if (obj.PavedStreet)
                {
                    _mainViewModel.BuildingSettingsViewModel.IsPavedStreet = obj.PavedStreet;
                }
            }
            else
            {
                _mainViewModel.BuildingSettingsViewModel.SelectedBuildingInfluence = _mainViewModel.BuildingSettingsViewModel.BuildingInfluences.Single(x => x.Type == BuildingInfluenceType.None);
            }

            // flags            
            //_mainWindowLocalization.BuildingSettingsViewModel.IsEnableLabelChecked = !string.IsNullOrEmpty(obj.Label);
            _mainViewModel.BuildingSettingsViewModel.IsBorderlessChecked = obj.Borderless;
            _mainViewModel.BuildingSettingsViewModel.IsRoadChecked = obj.Road;
        }

        private void StatusMessageChanged(string message)
        {
            StatusBarItemStatus.Content = message;
            Debug.WriteLine($"Status message changed: {message}");
        }

        private void LoadedFileChanged(string filename)
        {
            Title = string.IsNullOrEmpty(filename) ? "Anno Designer" : string.Format("{0} - Anno Designer", Path.GetFileName(filename));
            logger.Info($"Loaded file: {(string.IsNullOrEmpty(filename) ? "(none)" : filename)}");
        }

        private void ClipboardChanged(List<AnnoObject> l)
        {
            StatusBarItemClipboardStatus.Content = _mainViewModel.StatusBarItemsOnClipboard + ": " + l.Count;
        }

        #endregion

        #region Main methods

        private void ApplyCurrentObject()
        {
            // parse user inputs and create new object
            var obj = new AnnoObject
            {
                Size = new Size(_mainViewModel.BuildingSettingsViewModel.BuildingWidth, _mainViewModel.BuildingSettingsViewModel.BuildingHeight),
                Color = _mainViewModel.BuildingSettingsViewModel.SelectedColor ?? Colors.Red,
                Label = _mainViewModel.BuildingSettingsViewModel.IsEnableLabelChecked ? _mainViewModel.BuildingSettingsViewModel.BuildingName : string.Empty,
                Icon = comboBoxIcon.SelectedItem == _noIconItem ? null : ((IconImage)comboBoxIcon.SelectedItem).Name,
                Radius = _mainViewModel.BuildingSettingsViewModel.BuildingRadius,
                InfluenceRange = _mainViewModel.BuildingSettingsViewModel.BuildingInfluenceRange,
                PavedStreet = _mainViewModel.BuildingSettingsViewModel.IsPavedStreet,
                Borderless = _mainViewModel.BuildingSettingsViewModel.IsBorderlessChecked,
                Road = _mainViewModel.BuildingSettingsViewModel.IsRoadChecked,
                Identifier = _mainViewModel.BuildingSettingsViewModel.BuildingIdentifier,
                Template = _mainViewModel.BuildingSettingsViewModel.BuildingTemplate
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
                    var buildingInfo = annoCanvas.BuildingPresets.Buildings.FirstOrDefault(_ => _.IconFileName?.Equals(objIconFileName, StringComparison.OrdinalIgnoreCase) ?? false);
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
                    else if (!_mainViewModel.BuildingSettingsViewModel.BuildingTemplate.Contains("field", StringComparison.OrdinalIgnoreCase)) //check if the icon is removed from a template field
                    {
                        obj.Identifier = "Unknown Object";
                    }
                }
                else if (!string.IsNullOrWhiteSpace(obj.Icon) && obj.Icon.Contains(IconFieldNamesCheck))
                {
                    //Check if Field Icon belongs to the field identifier, else set the official icon
                    var buildingInfo = annoCanvas.BuildingPresets.Buildings.FirstOrDefault(_ => _.Identifier == obj.Identifier);
                    if (buildingInfo != null)
                    {
                        if (!string.Equals(objIconFileName, buildingInfo.IconFileName, StringComparison.OrdinalIgnoreCase))
                        {
                            obj.Icon = buildingInfo.IconFileName.Remove(buildingInfo.IconFileName.Length - 4, 4); //rmeove the .png for the comboBoxIcon
                            try
                            {
                                comboBoxIcon.SelectedItem = string.IsNullOrEmpty(obj.Icon) ? _noIconItem : comboBoxIcon.Items.Cast<IconImage>().Single(_ => _.Name == Path.GetFileNameWithoutExtension(obj.Icon));
                            }
                            catch (Exception)
                            {
                                comboBoxIcon.SelectedItem = _noIconItem;
                            }
                        }
                    }
                    else
                    {
                        obj.Identifier = "Unknown Object";
                    }
                }
                if (string.IsNullOrEmpty(obj.Icon) && !_mainViewModel.BuildingSettingsViewModel.BuildingTemplate.Contains("field", StringComparison.OrdinalIgnoreCase))
                {
                    obj.Identifier = "Unknown Object";
                }

                annoCanvas.SetCurrentObject(obj);
            }
            else
            {
                throw new Exception("Invalid building configuration.");
            }
        }

        private void ApplyPreset(AnnoObject selectedItem)
        {
            try
            {
                if (selectedItem != null)
                {
                    UpdateUIFromObject(new AnnoObject(selectedItem)
                    {
                        Color = _mainViewModel.BuildingSettingsViewModel.SelectedColor ?? Colors.Red,
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

        /// <summary>
        /// Called when localisation is changed, to repopulate the tree view
        /// </summary>
        private void RepopulateTreeView()
        {
            if (annoCanvas.BuildingPresets != null)
            {
                var treeState = _mainViewModel.PresetsTreeViewModel.GetCondensedTreeState();

                _mainViewModel.PresetsTreeViewModel.LoadItems(annoCanvas.BuildingPresets);

                _mainViewModel.PresetsTreeViewModel.SetCondensedTreeState(treeState, annoCanvas.BuildingPresets.Version);
            }
        }

        #endregion

        #region UI events
        private void MainWindow_DpiChanged(object sender, DpiChangedEventArgs e)
        {
            App.DpiScale = e.NewDpi;
            //TODO: Redraw statistics when change is merged.
        }

        //When changing 'SelectBox'

        private void ButtonPlaceBuildingClick(object sender, RoutedEventArgs e)
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

        #region Menu Events

        private void MenuItemExportImageClick(object sender, RoutedEventArgs e)
        {
            ExportImage(_mainViewModel.UseCurrentZoomOnExportedImageValue, _mainViewModel.RenderSelectionHighlightsOnExportedImageValue);
        }

        private void MenuItemStatsShowStatsClick(object sender, RoutedEventArgs e)
        {
            ToggleStatisticsView(((MenuItem)sender).IsChecked);
        }

        private void ToggleStatisticsView(bool showStatisticsView)
        {
            colStatisticsView.MinWidth = showStatisticsView ? 100 : 0;
            colStatisticsView.Width = showStatisticsView ? GridLength.Auto : new GridLength(0);

            statisticsView.Visibility = showStatisticsView ? Visibility.Visible : Visibility.Collapsed;
            statisticsView.MinWidth = showStatisticsView ? 100 : 0;

            splitterStatisticsView.Visibility = showStatisticsView ? Visibility.Visible : Visibility.Collapsed;
        }

        private void MenuItemStatsBuildingCountClick(object sender, RoutedEventArgs e)
        {
            _mainViewModel.StatisticsViewModel.ToggleBuildingList(((MenuItem)sender).IsChecked, annoCanvas.PlacedObjects, annoCanvas.SelectedObjects, annoCanvas.BuildingPresets);
        }

        private async void MenuItemVersionCheckImageClick(object sender, RoutedEventArgs e)
        {
            await CheckForUpdates(true);
        }

        private void MenuItemRegisterExtensionClick(object sender, RoutedEventArgs e)
        {
            // registers the anno_designer class type and adds the correct command string to pass a file argument to the application
            Registry.SetValue(@"HKEY_CURRENT_USER\Software\Classes\anno_designer\shell\open\command", null, string.Format("\"{0}\" \"%1\"", App.ExecutablePath));
            Registry.SetValue(@"HKEY_CURRENT_USER\Software\Classes\anno_designer\DefaultIcon", null, string.Format("\"{0}\",0", App.ExecutablePath));
            // registers the .ad file extension to the anno_designer class
            Registry.SetValue(@"HKEY_CURRENT_USER\Software\Classes\.ad", null, "anno_designer");

            ShowRegistrationMessageBox(isDeregistration: false);
        }

        private void MenuItemUnregisterExtensionClick(object sender, RoutedEventArgs e)
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

        private void ShowRegistrationMessageBox(bool isDeregistration)
        {
            var language = AnnoDesigner.Localization.Localization.GetLanguageCodeFromName(SelectedLanguage);
            var message = isDeregistration ? AnnoDesigner.Localization.Localization.Translations[language]["UnregisterFileExtensionSuccessful"] : AnnoDesigner.Localization.Localization.Translations[language]["RegisterFileExtensionSuccessful"];

            MessageBox.Show(message,
                AnnoDesigner.Localization.Localization.Translations[language]["Successful"],
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void MenuItemOpenWelcomeClick(object sender, RoutedEventArgs e)
        {
            var welcomeWindow = new Welcome
            {
                Owner = this
            };
            welcomeWindow.DataContext = _mainViewModel.WelcomeViewModel;
            welcomeWindow.Show();
        }

        private void MenuItemAboutClick(object sender, RoutedEventArgs e)
        {
            var aboutWindow = new About
            {
                Owner = this
            };
            aboutWindow.DataContext = _mainViewModel.AboutViewModel;
            aboutWindow.ShowDialog();
        }

        /// <summary>
        /// Event fired when a language is selected from the "Language" submenu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemLanguageClick(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var languageChecked = false;
            string language = null;
            foreach (MenuItem m in menuItem.Items)
            {
                if (m.IsChecked && !languageChecked && m.Header.ToString() != SelectedLanguage)
                {
                    languageChecked = true;
                    language = m.Header.ToString();
                }
                else
                {
                    m.IsChecked = false;
                }
            }
            if (!languageChecked)
            {
                return;
            }
            else
            {
                var currentLanguage = SelectedLanguage;
                if (language != currentLanguage)
                {
                    SelectedLanguage = language;
                }

            }
        }

        #endregion

        private void TextBoxSearchPresetsKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                //used to fix issue with misplaced caret in TextBox
                TextBoxSearchPresets.UpdateLayout();
                TextBoxSearchPresets.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() => { }));
                //TextBoxSearchPresets.InvalidateVisual();                
            }
        }

        #endregion

        private void WindowClosing(object sender, CancelEventArgs e)
        {
            Settings.Default.IsPavedStreet = _mainViewModel.BuildingSettingsViewModel.IsPavedStreet;

            Settings.Default.ShowGrid = _mainViewModel.CanvasShowGrid;
            Settings.Default.ShowIcons = _mainViewModel.CanvasShowIcons;
            Settings.Default.ShowLabels = _mainViewModel.CanvasShowLabels;

            Settings.Default.EnableAutomaticUpdateCheck = _mainViewModel.AutomaticUpdateCheck;

            Settings.Default.UseCurrentZoomOnExportedImageValue = _mainViewModel.UseCurrentZoomOnExportedImageValue;
            Settings.Default.RenderSelectionHighlightsOnExportedImageValue = _mainViewModel.RenderSelectionHighlightsOnExportedImageValue;

            string savedTreeState = null;
            using (var ms = new MemoryStream())
            {
                SerializationHelper.SaveToStream(_mainViewModel.PresetsTreeViewModel.GetCondensedTreeState(), ms);

                savedTreeState = Encoding.UTF8.GetString(ms.ToArray());
            }
            Settings.Default.PresetsTreeExpandedState = savedTreeState;
            Settings.Default.PresetsTreeLastVersion = _mainViewModel.PresetsTreeViewModel.BuildingPresetsVersion;

            Settings.Default.TreeViewSearchText = _mainViewModel.PresetsTreeSearchViewModel.SearchText;
            Settings.Default.PresetsTreeGameVersionFilter = _mainViewModel.PresetsTreeViewModel.FilterGameVersion.ToString();

            if (WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
            }

            Settings.Default.MainWindowWindowState = WindowState;

            Settings.Default.Save();

#if DEBUG
            var userConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath;
            logger.Trace($"saving settings: \"{userConfig}\"");
#endif
        }

        private void MenuCopyLayoutToClipboardClick(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    annoCanvas.Normalize(1);
                    _layoutLoader.SaveLayout(annoCanvas.PlacedObjects, ms);

                    var jsonString = Encoding.UTF8.GetString(ms.ToArray());

                    Clipboard.SetText(jsonString, TextDataFormat.UnicodeText);

                    string language = Localization.Localization.GetLanguageCodeFromName(SelectedLanguage);

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

        /// <summary>
        /// Renders the current layout to file.
        /// </summary>
        /// <param name="exportZoom">indicates whether the current zoom level should be applied, if false the default zoom is used</param>
        /// <param name="exportSelection">indicates whether selection and influence highlights should be rendered</param>
        public void ExportImage(bool exportZoom, bool exportSelection)
        {
            var dialog = new SaveFileDialog
            {
                DefaultExt = Constants.ExportedImageExtension,
                Filter = Constants.ExportDialogFilter
            };

            if (!string.IsNullOrEmpty(annoCanvas.LoadedFile))
            {
                // default the filename to the same name as the saved layout
                dialog.FileName = Path.GetFileNameWithoutExtension(annoCanvas.LoadedFile);
            }

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    RenderToFile(dialog.FileName, 1, exportZoom, exportSelection, statisticsView.IsVisible);

                    MessageBox.Show(this,
                        Localization.Localization.Translations[Localization.Localization.GetLanguageCodeFromName(SelectedLanguage)]["ExportImageSuccessful"],
                        Localization.Localization.Translations[Localization.Localization.GetLanguageCodeFromName(SelectedLanguage)]["Successful"],
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Error exporting image.");
                    MessageBox.Show("Something went wrong while exporting the image.",
                        Localization.Localization.Translations[Localization.Localization.GetLanguageCodeFromName(SelectedLanguage)]["Error"],
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
            if (annoCanvas.PlacedObjects.Count == 0)
            {
                return;
            }

            // copy all objects
            var allObjects = annoCanvas.PlacedObjects.Select(_ => new AnnoObject(_)).Cast<AnnoObject>().ToList();
            // copy selected objects
            // note: should be references to the correct copied objects from allObjects
            var selectedObjects = annoCanvas.SelectedObjects.Select(_ => new AnnoObject(_)).ToList();

            Debug.WriteLine($"UI thread: {Thread.CurrentThread.ManagedThreadId} ({Thread.CurrentThread.Name})");
            void renderThread()
            {
                Debug.WriteLine($"Render thread: {Thread.CurrentThread.ManagedThreadId} ({Thread.CurrentThread.Name})");

                Stopwatch sw = new Stopwatch();
                sw.Start();

                var icons = new Dictionary<string, IconImage>(StringComparer.OrdinalIgnoreCase);
                foreach (var curIcon in annoCanvas.Icons)
                {
                    icons.Add(curIcon.Key, new IconImage(curIcon.Value.Name, curIcon.Value.Localizations, curIcon.Value.IconPath));
                }

                // initialize output canvas
                var target = new AnnoCanvas(annoCanvas.BuildingPresets, icons)
                {
                    PlacedObjects = allObjects,
                    RenderGrid = annoCanvas.RenderGrid,
                    RenderIcon = annoCanvas.RenderIcon,
                    RenderLabel = annoCanvas.RenderLabel
                };

                sw.Stop();
                Debug.WriteLine($"creating canvas took: {sw.ElapsedMilliseconds}ms");

                // normalize layout
                target.Normalize(border);
                // set zoom level
                if (exportZoom)
                {
                    target.GridSize = annoCanvas.GridSize;
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
                    var exportStatisticsView = new StatisticsView
                    {
                        Margin = new Thickness(5, 0, 0, 0)
                    };
                    exportStatisticsView.statisticsViewModel.UpdateStatistics(target.PlacedObjects, target.SelectedObjects, target.BuildingPresets);
                    exportStatisticsView.statisticsViewModel.CopyLocalization(_mainViewModel.StatisticsViewModel);
                    exportStatisticsView.statisticsViewModel.ShowBuildingList = _mainViewModel.StatisticsViewModel.ShowBuildingList;

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

        private void MenuLoadLayoutFromJsonClick(object sender, RoutedEventArgs e)
        {
            string language = Localization.Localization.GetLanguageCodeFromName(SelectedLanguage);

            var input = InputWindow.Prompt(Localization.Localization.Translations[language]["LoadLayoutMessage"],
                Localization.Localization.Translations[language]["LoadLayoutHeader"]);

            LoadLayoutFromJson(input, false);
        }

        private void LoadLayoutFromJson(string jsonString, bool forceLoad = false)
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
                            annoCanvas.SelectedObjects.Clear();
                            annoCanvas.PlacedObjects = loadedLayout;
                            annoCanvas.LoadedFile = string.Empty;
                            annoCanvas.Normalize(1);

                            _mainViewModel.UpdateStatistics();
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
                    LoadLayoutFromJson(jsonString, true);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error loading layout from JSON.");
                MessageBox.Show("Something went wrong while loading the layout.",
                        Localization.Localization.Translations[Localization.Localization.GetLanguageCodeFromName(SelectedLanguage)]["Error"],
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
            }
        }
    }
}