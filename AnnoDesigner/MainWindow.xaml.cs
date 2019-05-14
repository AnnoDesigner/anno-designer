using AnnoDesigner.Presets;
using AnnoDesigner.UI;
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

namespace AnnoDesigner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
        : Window
    {
        private readonly WebClient _webClient;
        private IconImage _noIconItem;
        private static MainWindow _instance;
        private TreeViewSearch<AnnoObject> _treeViewSearch;
        private List<bool> _treeViewState;

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
                _instance.SelectedLanguageChanged();
            }
        }

        private static Localization.MainWindow _mainWindowLocalization;
        //About window does not need to be called, as it get's instantiated and used when the about window is created

        private void SelectedLanguageChanged()
        {
            _mainWindowLocalization.UpdateLanguage();
            _instance.RepopulateTreeView();
            foreach (MenuItem item in LanguageMenu.Items)
            {
                if (item.Header.ToString() == SelectedLanguage)
                {
                    item.IsChecked = true;
                }
                else
                {
                    item.IsChecked = false;
                }
            }

            //refresh localized influence types in combo box
            comboxBoxInfluenceType.Items.Clear();
            string[] rangeTypes = Enum.GetNames(typeof(BuildingInfluenceType));
            string language = Localization.Localization.GetLanguageCodeFromName(SelectedLanguage);

            foreach (string rangeType in rangeTypes)
            {
                comboxBoxInfluenceType.Items.Add(new KeyValuePair<BuildingInfluenceType, string>((BuildingInfluenceType)Enum.Parse(typeof(BuildingInfluenceType), rangeType), Localization.Localization.Translations[language][rangeType]));
            }
            comboxBoxInfluenceType.SelectedIndex = 0;

            //Force a language update on the clipboard status item.
            if (StatusBarItemClipboardStatus.Content != null) ClipboardChanged(annoCanvas.ObjectClipboard);

            //update settings
            Settings.Default.SelectedLanguage = SelectedLanguage;

            _mainWindowLocalization.StatisticsViewModel.UpdateStatistics(annoCanvas.PlacedObjects,
                annoCanvas.SelectedObjects,
                annoCanvas.BuildingPresets);

            _mainWindowLocalization.TreeViewSearchText = string.Empty;
        }

        private readonly ICommons _commons;

        #region Initialization

        public MainWindow()
        {
            InitializeComponent();
        }

        public MainWindow(ICommons commonsToUse) : this()
        {
            _commons = commonsToUse;

            App.DpiScale = VisualTreeHelper.GetDpi(this);

            _instance = this;
            // initialize web client
            _webClient = new WebClient();
            _webClient.DownloadStringCompleted += WebClientDownloadStringCompleted;
            // add event handlers
            annoCanvas.OnCurrentObjectChanged += UpdateUIFromObject;
            annoCanvas.OnStatusMessageChanged += StatusMessageChanged;
            annoCanvas.OnLoadedFileChanged += LoadedFileChanged;
            annoCanvas.OnClipboardChanged += ClipboardChanged;
            annoCanvas.StatisticsUpdated += AnnoCanvas_StatisticsUpdated;

            DpiChanged += MainWindow_DpiChanged;

            //Get a reference an instance of Localization.MainWindow, so we can call UpdateLanguage() in the SelectedLanguage setter
            var dependencyObject = LogicalTreeHelper.FindLogicalNode(this, "Menu");
            _mainWindowLocalization = (Localization.MainWindow)((Menu)dependencyObject).DataContext;

            //If language is not recognized, bring up the language selection screen
            if (!Localization.Localization.LanguageCodeMap.ContainsKey(Settings.Default.SelectedLanguage))
            {
                var w = new Welcome();
                w.ShowDialog();
            }
            else
            {
                SelectedLanguage = Settings.Default.SelectedLanguage;
            }
            foreach (MenuItem item in LanguageMenu.Items)
            {
                if (item.Header.ToString() == SelectedLanguage)
                {
                    item.IsChecked = true;
                }
            }

            LoadSettings();
        }

        private void AnnoCanvas_StatisticsUpdated(object sender, EventArgs e)
        {
            _mainWindowLocalization.StatisticsViewModel.UpdateStatistics(annoCanvas.PlacedObjects,
                annoCanvas.SelectedObjects,
                annoCanvas.BuildingPresets);
        }

        private void LoadSettings()
        {
            annoCanvas.RenderGrid = Settings.Default.ShowGrid;
            annoCanvas.RenderIcon = Settings.Default.ShowIcons;
            annoCanvas.RenderLabel = Settings.Default.ShowLabels;
            ToggleStatisticsView(Settings.Default.StatsShowStats);
            ToggleBuildingList(Settings.Default.StatsShowBuildingCount);
            AutomaticUpdateCheck.IsChecked = Settings.Default.EnableAutomaticUpdateCheck;
            ShowGrid.IsChecked = Settings.Default.ShowGrid;
            ShowIcons.IsChecked = Settings.Default.ShowIcons;
            ShowLabels.IsChecked = Settings.Default.ShowLabels;
            _treeViewState = Settings.Default.TreeViewState ?? null;
            _mainWindowLocalization.TreeViewSearchText = Settings.Default.TreeViewSearchText ?? "";
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            _mainWindowLocalization.BuildingSettingsViewModel.AnnoCanvasToUse = annoCanvas;

            // add icons to the combobox
            comboBoxIcon.Items.Clear();
            _noIconItem = new IconImage("None");
            comboBoxIcon.Items.Add(_noIconItem);
            foreach (var icon in annoCanvas.Icons)
            {
                comboBoxIcon.Items.Add(icon.Value);
            }
            comboBoxIcon.SelectedIndex = 0;

            string language = Localization.Localization.GetLanguageCodeFromName(SelectedLanguage);
            //add localized influence types to combo box
            comboxBoxInfluenceType.Items.Clear();
            string[] rangeTypes = Enum.GetNames(typeof(BuildingInfluenceType));
            foreach (string rangeType in rangeTypes)
            {
                comboxBoxInfluenceType.Items.Add(new KeyValuePair<BuildingInfluenceType, string>((BuildingInfluenceType)Enum.Parse(typeof(BuildingInfluenceType), rangeType), Localization.Localization.Translations[language][rangeType]));
            }
            comboxBoxInfluenceType.SelectedIndex = 0;

            // check for updates on startup            
            _mainWindowLocalization.VersionValue = Constants.Version.ToString("0.0#", CultureInfo.InvariantCulture);
            _mainWindowLocalization.FileVersionValue = Constants.FileVersion.ToString("0.#", CultureInfo.InvariantCulture);

            CheckForUpdates(false);

            // load color presets
            colorPicker.StandardColors.Clear();
            //This is currently disabled
            colorPicker.ShowStandardColors = false;
            //try
            //{
            //    ColorPresets colorPresets = DataIO.LoadFromFile<ColorPresets>(Path.Combine(App.ApplicationPath, Constants.ColorPresetsFile));
            //    foreach (ColorScheme colorScheme in colorPresets.ColorSchemes)
            //    {
            //        foreach (ColorInfo colorInfo in colorScheme.ColorInfos)
            //        {
            //            colorPicker.StandardColors.Add(new ColorItem(colorInfo.Color, string.Format("{0} ({1})", colorInfo.ColorTarget, colorScheme.Name)));
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Message, "Loading of the color presets failed");
            //}

            // load presets
            treeViewPresets.Items.Clear();

            // manually add a road tile preset
            AddRoadTiles();
            BuildingPresets presets = annoCanvas.BuildingPresets;
            if (presets != null)
            {
                presets.AddToTree(treeViewPresets);
                GroupBoxPresets.Header = string.Format("Building presets - loaded v{0}", presets.Version);
                _mainWindowLocalization.PresetsVersionValue = presets.Version;
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

        #endregion

        #region Version check

        private async Task DownloadNewPresetsAsync()
        {
            var isnewPresetAvailable = await _commons.UpdateHelper.IsNewPresetFileAvailableAsync(new Version(annoCanvas.BuildingPresets.Version));
            if (isnewPresetAvailable)
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

                    var newLocation = await _commons.UpdateHelper.DownloadLatestPresetFileAsync().ConfigureAwait(false);

                    busyIndicator.IsBusy = false;

                    Commons.RestartApplication(false, null, App.ExecutablePath);

                    Environment.Exit(-1);
                }
            }
        }

        private void CheckForUpdates(bool forcedCheck)
        {
            if (Settings.Default.EnableAutomaticUpdateCheck || forcedCheck)
            {
                DownloadNewPresetsAsync();

                _webClient.DownloadStringAsync(new Uri("https://raw.githubusercontent.com/AgmasGold/anno-designer/master/version.txt"), forcedCheck);
            }
        }

        private void WebClientDownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null)
                {
                    MessageBox.Show(e.Error.Message, "Version check failed");
                    return;
                }
                if (Double.Parse(e.Result, CultureInfo.InvariantCulture) > Constants.Version)
                {
                    // new version found
                    if (MessageBox.Show("A newer version was found, do you want to visit the releases page?\nhttps://github.com/AgmasGold/anno-designer/releases\n\n Clicking 'Yes' will open a new tab in your web browser.", "Update available", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.OK) == MessageBoxResult.Yes)
                    {
                        System.Diagnostics.Process.Start("https://github.com/AgmasGold/anno-designer/releases");
                    }
                }
                else
                {
                    StatusMessageChanged("Version is up to date.");
                    if ((bool)e.UserState)
                    {
                        MessageBox.Show("This version is up to date.", "No updates found");
                    }
                }
                //If not already prompted
                if (Settings.Default.PromptedForAutoUpdateCheck == false)
                {
                    Settings.Default.PromptedForAutoUpdateCheck = true;
                    if (MessageBox.Show("Do you want to continue checking for a new version on startup?\n\nThis option can be changed from the help menu.", "Continue checking for updates?", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.No)
                    {
                        Settings.Default.EnableAutomaticUpdateCheck = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error checking version. \n\nAdded error to error log.", "Version check failed");
                App.WriteToErrorLog("Error Checking Version", ex.Message, ex.StackTrace);
                return;
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
            _mainWindowLocalization.BuildingSettingsViewModel.BuildingWidth = (int)obj.Size.Width;
            _mainWindowLocalization.BuildingSettingsViewModel.BuildingHeight = (int)obj.Size.Height;
            // color
            _mainWindowLocalization.BuildingSettingsViewModel.SelectedColor = obj.Color;
            // label
            _mainWindowLocalization.BuildingSettingsViewModel.BuildingName = obj.Label;
            // Identifier
            _mainWindowLocalization.BuildingSettingsViewModel.BuildingIdentifier = obj.Identifier;
            // Template
            _mainWindowLocalization.BuildingSettingsViewModel.BuildingTemplate = obj.Template;
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
            _mainWindowLocalization.BuildingSettingsViewModel.BuildingRadius = obj.Radius;
            //InfluenceRadius
            _mainWindowLocalization.BuildingSettingsViewModel.BuildingInfluenceRange = obj.InfluenceRange;
            _mainWindowLocalization.BuildingSettingsViewModel.IsPavedStreet = false;

            //Set Influence Type combo box
            if (obj.Radius > 0 && obj.InfluenceRange > 0)
            {
                //Building uses both a radius and an influence
                //Has to be set manually 
                comboxBoxInfluenceType.SelectedValue = BuildingInfluenceType.Both;
            }
            else if (obj.Radius > 0)
            {
                comboxBoxInfluenceType.SelectedValue = BuildingInfluenceType.Radius;
            }
            else if (obj.InfluenceRange > 0)
            {
                comboxBoxInfluenceType.SelectedValue = BuildingInfluenceType.Distance;
                if (obj.PavedStreet)
                {
                    _mainWindowLocalization.BuildingSettingsViewModel.IsPavedStreet = obj.PavedStreet;
                } else
                {
                    _mainWindowLocalization.BuildingSettingsViewModel.IsPavedStreet = false;
                }
            }
            else
            {
                comboxBoxInfluenceType.SelectedValue = BuildingInfluenceType.None;
            }

            // flags            
            //_mainWindowLocalization.BuildingSettingsViewModel.IsEnableLabelChecked = !string.IsNullOrEmpty(obj.Label);
            _mainWindowLocalization.BuildingSettingsViewModel.IsBorderlessChecked = obj.Borderless;
            _mainWindowLocalization.BuildingSettingsViewModel.IsRoadChecked = obj.Road;
        }

        private void StatusMessageChanged(string message)
        {
            StatusBarItemStatus.Content = message;
            System.Diagnostics.Debug.WriteLine(string.Format("Status message changed: {0}", message));
        }

        private void LoadedFileChanged(string filename)
        {
            Title = string.IsNullOrEmpty(filename) ? "Anno Designer" : string.Format("{0} - Anno Designer", Path.GetFileName(filename));
            System.Diagnostics.Debug.WriteLine(string.Format("Loaded file: {0}", string.IsNullOrEmpty(filename) ? "(none)" : filename));
        }

        private void ClipboardChanged(List<AnnoObject> l)
        {
            StatusBarItemClipboardStatus.Content = _mainWindowLocalization.StatusBarItemsOnClipboard + ": " + l.Count;
        }

        #endregion

        #region Main methods

        private static bool IsChecked(ToggleButton checkBox)
        {
            return checkBox.IsChecked ?? false;
        }

        private void ApplyCurrentObject()
        {
            // parse user inputs and create new object
            var obj = new AnnoObject
            {
                Size = new Size(_mainWindowLocalization.BuildingSettingsViewModel.BuildingWidth, _mainWindowLocalization.BuildingSettingsViewModel.BuildingHeight),
                Color = _mainWindowLocalization.BuildingSettingsViewModel.SelectedColor ?? Colors.Red,
                Label = _mainWindowLocalization.BuildingSettingsViewModel.IsEnableLabelChecked ? _mainWindowLocalization.BuildingSettingsViewModel.BuildingName : string.Empty,
                Icon = comboBoxIcon.SelectedItem == _noIconItem ? null : ((IconImage)comboBoxIcon.SelectedItem).Name,
                Radius = _mainWindowLocalization.BuildingSettingsViewModel.BuildingRadius,
                InfluenceRange = _mainWindowLocalization.BuildingSettingsViewModel.BuildingInfluenceRange,
                PavedStreet = _mainWindowLocalization.BuildingSettingsViewModel.IsPavedStreet,
                Borderless = _mainWindowLocalization.BuildingSettingsViewModel.IsBorderlessChecked,
                Road = _mainWindowLocalization.BuildingSettingsViewModel.IsRoadChecked,
                Identifier = _mainWindowLocalization.BuildingSettingsViewModel.BuildingIdentifier,
                Template = _mainWindowLocalization.BuildingSettingsViewModel.BuildingTemplate
            };

            var objIconFileName = "";
            //Parse the Icon path into something we can check.
            if (!string.IsNullOrEmpty(obj.Icon))
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
                if (!string.IsNullOrEmpty(obj.Icon) && obj.Icon.Contains(IconFieldNamesCheck) == false)
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
                            obj.Identifier = buildingInfo.Identifier;
                        }
                        else
                        {
                            //Sizes and icon do not match
                            obj.Identifier = "Unknown Object";
                        }
                    }
                    else if (_mainWindowLocalization.BuildingSettingsViewModel.BuildingTemplate.ToLower().Contains("field") == false) //check if the icon is removed from a template field
                    {
                        obj.Identifier = "Unknown Object";
                    }
                }
                else if (!string.IsNullOrEmpty(obj.Icon) && obj.Icon.Contains(IconFieldNamesCheck) == true)
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
                if (string.IsNullOrEmpty(obj.Icon) && _mainWindowLocalization.BuildingSettingsViewModel.BuildingTemplate.ToLower().Contains("field") == false)
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

        private void ApplyPreset()
        {
            try
            {
                var selectedItem = treeViewPresets.SelectedItem as AnnoObject;
                if (selectedItem != null)
                {
                    UpdateUIFromObject(new AnnoObject(selectedItem)
                    {
                        Color = _mainWindowLocalization.BuildingSettingsViewModel.SelectedColor ?? Colors.Red,
                    });
                    ApplyCurrentObject();
                }
            }
            catch (Exception ex)
            {
                App.WriteToErrorLog("Error in ApplyPreset()", ex.Message, ex.StackTrace);
                MessageBox.Show("Something went wrong while applying the preset.");
            }
        }
        /// <summary>
        /// Called when localisation is changed, to repopulate the tree view
        /// </summary>
        private void RepopulateTreeView()
        {
            treeViewPresets.Items.Clear();
            if (annoCanvas.BuildingPresets != null)
            {
                // manually add a road tile preset
                AddRoadTiles();
                annoCanvas.BuildingPresets.AddToTree(treeViewPresets);
            }
        }

        private void AddRoadTiles()
        {
            treeViewPresets.Items.Add(new AnnoObject { Label = TreeLocalization.TreeLocalization.GetTreeLocalization("RoadTile"), Size = new Size(1, 1), Radius = 0, Road = true, Identifier = "Road" });
            treeViewPresets.Items.Add(new AnnoObject { Label = TreeLocalization.TreeLocalization.GetTreeLocalization("BorderlessRoadTile"), Size = new Size(1, 1), Radius = 0, Borderless = true, Road = true, Identifier = "Road" });
        }

        #endregion

        #region UI events
        private void MainWindow_DpiChanged(object sender, DpiChangedEventArgs e)
        {
            App.DpiScale = e.NewDpi;
            //TODO: Redraw statistics when change is merged.
        }

        private void MenuItemCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
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
            //annoCanvas.ExportImage(MenuItemExportZoom.IsChecked, MenuItemExportSelection.IsChecked);
            ExportImage(MenuItemExportZoom.IsChecked, MenuItemExportSelection.IsChecked);
        }

        private void MenuItemGridClick(object sender, RoutedEventArgs e)
        {
            annoCanvas.RenderGrid = !annoCanvas.RenderGrid;
        }

        private void MenuItemLabelClick(object sender, RoutedEventArgs e)
        {
            annoCanvas.RenderLabel = !annoCanvas.RenderLabel;
        }

        private void MenuItemIconClick(object sender, RoutedEventArgs e)
        {
            annoCanvas.RenderIcon = !annoCanvas.RenderIcon;
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
            ToggleBuildingList(((MenuItem)sender).IsChecked);
        }

        private void ToggleBuildingList(bool showBuildingList)
        {
            _mainWindowLocalization.StatisticsViewModel.ShowBuildingList = showBuildingList;
            if (showBuildingList)
            {
                _mainWindowLocalization.StatisticsViewModel.UpdateStatistics(annoCanvas.PlacedObjects,
                    annoCanvas.SelectedObjects,
                    annoCanvas.BuildingPresets);
            }
        }

        private void MenuItemVersionCheckImageClick(object sender, RoutedEventArgs e)
        {
            CheckForUpdates(true);
        }

        private void MenuItemResetZoomClick(object sender, RoutedEventArgs e)
        {
            annoCanvas.ResetZoom();
        }

        private void MenuItemNormalizeClick(object sender, RoutedEventArgs e)
        {
            annoCanvas.Normalize(1);
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

        private void MenuItemHomepageClick(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/AgmasGold/anno-designer");
        }

        private void MenuItemOpenWelcomeClick(object sender, RoutedEventArgs e)
        {
            var w = new Welcome
            {
                Owner = this
            };
            w.Show();
        }

        private void MenuItemAboutClick(object sender, RoutedEventArgs e)
        {
            new About { Owner = this }.ShowDialog();
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
        private void LanguageMenuSubmenuClosed(object sender, RoutedEventArgs e)
        {
            SelectedLanguageChanged();
        }

        #endregion

        private void ComboxBoxInfluenceType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cbx = sender as ComboBox;
            if (cbx.SelectedValue != null)
            {
                var influenceType = (BuildingInfluenceType)((KeyValuePair<BuildingInfluenceType, string>)cbx.SelectedItem).Key;
                switch (influenceType)
                {
                    case BuildingInfluenceType.None:
                        dockPanelInfluenceRadius.Visibility = Visibility.Collapsed;
                        dockPanelInfluenceRange.Visibility = Visibility.Collapsed;
                        dockPanelPavedStreet.Visibility = Visibility.Collapsed;
                        break;
                    case BuildingInfluenceType.Radius:
                        dockPanelInfluenceRadius.Visibility = Visibility.Visible;
                        dockPanelInfluenceRange.Visibility = Visibility.Collapsed;
                        dockPanelPavedStreet.Visibility = Visibility.Collapsed;
                        break;
                    case BuildingInfluenceType.Distance:
                        dockPanelInfluenceRadius.Visibility = Visibility.Collapsed;
                        dockPanelInfluenceRange.Visibility = Visibility.Visible;
                        dockPanelPavedStreet.Visibility = Visibility.Visible;
                        break;
                    case BuildingInfluenceType.Both:
                        dockPanelInfluenceRadius.Visibility = Visibility.Visible;
                        dockPanelInfluenceRange.Visibility = Visibility.Visible;
                        dockPanelPavedStreet.Visibility = Visibility.Visible;
                        break;
                    default:
                        dockPanelInfluenceRadius.Visibility = Visibility.Collapsed;
                        dockPanelInfluenceRange.Visibility = Visibility.Collapsed;
                        dockPanelPavedStreet.Visibility = Visibility.Collapsed;
                        break;
                }
            }
        }

        //CheckBox PavedStreet : calculate distance range
        private void CheckBoxPavedStreetClick(object sender, RoutedEventArgs o)
        {
            MessageBox.Show("Checking this option will change the Influence Range for buildings, representing the increased range they receive when using paved streets.","Paved Street Selecion");
            if (!GetDistanceRange(_mainWindowLocalization.BuildingSettingsViewModel.IsPavedStreet))
            { Debug.WriteLine("$Calculate Paved Street/Dirt Street Error: Can not obtain new Distance Value");} else
            {
                //Trying to see of Mouse has an object or not is mouse has Object then Renew Object, withouth placnig.
                ApplyCurrentObject();
            }
        }
        
        public bool GetDistanceRange(bool value)
        {
            if (_mainWindowLocalization.BuildingSettingsViewModel.BuildingInfluenceRange>0)
            {
                var buildingInfo = annoCanvas.BuildingPresets.Buildings.FirstOrDefault(_ => _.Identifier == _mainWindowLocalization.BuildingSettingsViewModel.BuildingIdentifier);
                if (value)
                {
                    //sum for range on paved street for Public Service Building = x*1.43
                    //sum for range on paved street for City Institution Building = x*1.38
                    if (buildingInfo.InfluenceRange > 0 && buildingInfo.Template == "CityInstitutionBuilding")
                    {
                        _mainWindowLocalization.BuildingSettingsViewModel.BuildingInfluenceRange = Math.Round((buildingInfo.InfluenceRange - 2) * 1.38);
                    }
                    else if (buildingInfo.InfluenceRange > 0)
                    {
                        _mainWindowLocalization.BuildingSettingsViewModel.BuildingInfluenceRange = Math.Round((buildingInfo.InfluenceRange - 2) *1.43 );
                    }
                    return true;
                }
                else
                { 
                    if (buildingInfo.InfluenceRange > 0)
                    {
                        _mainWindowLocalization.BuildingSettingsViewModel.BuildingInfluenceRange = buildingInfo.InfluenceRange - 2;
                        return true;
                    }
                }
            }
            _mainWindowLocalization.BuildingSettingsViewModel.BuildingInfluenceRange = 0;
            return false;
        }

        private void TextBoxSearchPresetsGotFocus(object sender, RoutedEventArgs e)
        {
            if (e.Source is TextBox)
            {
                if (_mainWindowLocalization.TreeViewSearchText.Length == 0)
                {
                    _treeViewState = treeViewPresets.GetTreeViewState();
                }
            }
        }

        private void TextBoxSearchPresetsKeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Escape)
                {
                    _mainWindowLocalization.TreeViewSearchText = string.Empty;
                    TextBoxSearchPresets.UpdateLayout();
                }

                if (_mainWindowLocalization.TreeViewSearchText.Length == 0)
                {
                    _treeViewSearch.Reset();
                    treeViewPresets.SetTreeViewState(_treeViewState);
                }
                else
                {
                    _treeViewSearch.Search(_mainWindowLocalization.TreeViewSearchText);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to execute search successfully.");
                App.WriteToErrorLog("Failed to execute search successfully", ex.Message, ex.StackTrace);
            }
        }

        private void TreeViewPresetsMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ApplyPreset();
        }

        private void TreeViewPresetsKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                ApplyPreset();
            }
        }

        private void TreeViewPresetsLoaded(object sender, RoutedEventArgs e)
        {
            //Intialise tree view and ensure that item containers are generated.
            _treeViewSearch = new TreeViewSearch<AnnoObject>(treeViewPresets, _ => _.Label)
            {
                MatchFullWordOnly = false,
                IsCaseSensitive = false
            };
            _treeViewSearch.EnsureItemContainersGenerated();

            var isSearchState = false;
            if (!string.IsNullOrWhiteSpace(Settings.Default.TreeViewSearchText))
            {
                //Then apply the search **before** reloading state
                _treeViewSearch.Search(Settings.Default.TreeViewSearchText);
                isSearchState = true;
            }

            if (_treeViewState != null && _treeViewState.Count > 0)
            {
                try
                {
                    treeViewPresets.SetTreeViewState(_treeViewState);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to restore previous preset menu settings.");
                    App.WriteToErrorLog("TreeView SetTreeViewState Error", ex.Message, ex.StackTrace);
                }
            }

            if (isSearchState)
            {
                //if the application was last closed in the middle of a search, set the previous state
                //to an empty value, so that we don't just expand the results of the search as the 
                //previous state

                _treeViewState = new List<bool>();

            }
        }

        #endregion
        private void WindowClosing(object sender, CancelEventArgs e)
        {
            Settings.Default.TreeViewState = treeViewPresets.GetTreeViewState();
            Settings.Default.TreeViewSearchText = _mainWindowLocalization.TreeViewSearchText;
            if (WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
            }
            Settings.Default.Save();
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
                catch (Exception e)
                {
                    App.WriteToErrorLog("Error exporting image", e.Message, e.StackTrace);
                    MessageBox.Show(e.Message, "Something went wrong while exporting the image.");
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
            var allObjects = annoCanvas.PlacedObjects.Select(_ => new AnnoObject(_)).ToList();
            // copy selected objects
            // note: should be references to the correct copied objects from allObjects
            var selectedObjects = annoCanvas.SelectedObjects.Select(_ => new AnnoObject(_)).ToList();

            Debug.WriteLine($"UI thread: {Thread.CurrentThread.ManagedThreadId} ({Thread.CurrentThread.Name})");
            void renderThread()
            {
                Debug.WriteLine($"Render thread: {Thread.CurrentThread.ManagedThreadId} ({Thread.CurrentThread.Name})");

                Stopwatch sw = new Stopwatch();
                sw.Start();

                var icons = new Dictionary<string, IconImage>();
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
                var width = target.GridToScreen(target.PlacedObjects.Max(_ => _.Position.X + _.Size.Width) + border);//if +1 then there are weird black lines next to the statistics view
                var height = target.GridToScreen(target.PlacedObjects.Max(_ => _.Position.Y + _.Size.Height) + border) + 1;//+1 for black grid line at bottom

                if (renderStatistics)
                {
                    var exportStatisticsView = new StatisticsView
                    {
                        Margin = new Thickness(5, 0, 0, 0)
                    };
                    exportStatisticsView.statisticsViewModel.UpdateStatistics(target.PlacedObjects, target.SelectedObjects, target.BuildingPresets);
                    exportStatisticsView.statisticsViewModel.CopyLocalization(_mainWindowLocalization.StatisticsViewModel);
                    exportStatisticsView.statisticsViewModel.ShowBuildingList = _mainWindowLocalization.StatisticsViewModel.ShowBuildingList;

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
    }
}
