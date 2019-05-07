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
using System.Collections.ObjectModel;

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
        }

        #region Initialization

        public MainWindow()
        {
            InitializeComponent();

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
            DpiChanged += MainWindow_DpiChanged;

            //Get a reference an instance of Localization.MainWindow, so we can call UpdateLanguage() in the SelectedLanguage setter
            DependencyObject dependencyObject = LogicalTreeHelper.FindLogicalNode(this, "Menu");
            _mainWindowLocalization = (Localization.MainWindow)((Menu)dependencyObject).DataContext;

            //If language is not recognized, bring up the language selection screen
            if (!Localization.Localization.LanguageCodeMap.ContainsKey(Settings.Default.SelectedLanguage))
            {
                Welcome w = new Welcome();
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

        private void LoadSettings()
        {
            annoCanvas.RenderGrid = Settings.Default.ShowGrid;
            annoCanvas.RenderIcon = Settings.Default.ShowIcons;
            annoCanvas.RenderLabel = Settings.Default.ShowLabels;
            annoCanvas.RenderStats = Settings.Default.StatsShowStats;
            annoCanvas.RenderBuildingCount = Settings.Default.StatsShowBuildingCount;
            AutomaticUpdateCheck.IsChecked = Settings.Default.EnableAutomaticUpdateCheck;
            ShowGrid.IsChecked = Settings.Default.ShowGrid;
            ShowIcons.IsChecked = Settings.Default.ShowIcons;
            ShowLabels.IsChecked = Settings.Default.ShowLabels;
            _treeViewState = Settings.Default.TreeViewState ?? null;
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
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

        private void CheckForUpdates(bool forcedCheck)
        {
            if (Settings.Default.EnableAutomaticUpdateCheck || forcedCheck)
            {
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
            textBoxWidth.Value = (int)obj.Size.Width;
            textBoxHeight.Value = (int)obj.Size.Height;
            // color
            colorPicker.SelectedColor = obj.Color;
            // label
            textBoxLabel.Text = obj.Label;
            // Ident
            textBoxIdentifier.Text = obj.Identifier;
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
            textBoxRadius.Value = obj.Radius;
            //InfluenceRadius
            textBoxInfluenceRange.Text = obj.InfluenceRange.ToString();

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
            }
            else
            {
                comboxBoxInfluenceType.SelectedValue = BuildingInfluenceType.None;
            }

            // flags
            //checkBoxLabel.IsChecked = !string.IsNullOrEmpty(obj.Label);
            checkBoxBorderless.IsChecked = obj.Borderless;
            checkBoxRoad.IsChecked = obj.Road;
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
            AnnoObject obj = new AnnoObject
            {
                Size = new Size(textBoxWidth?.Value ?? 1, textBoxHeight?.Value ?? 1),
                Color = colorPicker.SelectedColor ?? Colors.Red,
                Label = IsChecked(checkBoxLabel) ? textBoxLabel.Text : "",
                Icon = comboBoxIcon.SelectedItem == _noIconItem ? null : ((IconImage)comboBoxIcon.SelectedItem).Name,
                Radius = textBoxRadius?.Value ?? 0,
                InfluenceRange = string.IsNullOrEmpty(textBoxInfluenceRange.Text) ? 0 : double.Parse(textBoxInfluenceRange.Text, CultureInfo.InvariantCulture),
                Borderless = IsChecked(checkBoxBorderless),
                Road = IsChecked(checkBoxRoad),
                Identifier = textBoxIdentifier.Text,
            };
            // do some sanity checks
            if (obj.Size.Width > 0 && obj.Size.Height > 0 && obj.Radius >= 0)
            {
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
                //TDOD: Rewrite ApplyPreset();
                AnnoObject selectedItem = treeViewPresets.SelectedItem as AnnoObject;
                if (selectedItem != null)
                {
                    UpdateUIFromObject(new AnnoObject(selectedItem)
                    {
                        Color = colorPicker.SelectedColor ?? Colors.Red,
                    });
                    ApplyCurrentObject();
                }
            }
            catch (Exception)
            {
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
            annoCanvas.ExportImage(MenuItemExportZoom.IsChecked, MenuItemExportSelection.IsChecked);
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
            annoCanvas.RenderStats = ((MenuItem)sender).IsChecked;
        }

        private void MenuItemStatsBuildingCountClick(object sender, RoutedEventArgs e)
        {
            annoCanvas.RenderBuildingCount = ((MenuItem)sender).IsChecked;
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
            string language = AnnoDesigner.Localization.Localization.GetLanguageCodeFromName(SelectedLanguage);
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
            Welcome w = new Welcome();
            w.Owner = this;
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
            MenuItem menuItem = sender as MenuItem;
            bool languageChecked = false;
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
                string currentLanguage = SelectedLanguage;
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
                        break;
                    case BuildingInfluenceType.Radius:
                        dockPanelInfluenceRadius.Visibility = Visibility.Visible;
                        dockPanelInfluenceRange.Visibility = Visibility.Collapsed;
                        break;
                    case BuildingInfluenceType.Distance:
                        dockPanelInfluenceRadius.Visibility = Visibility.Collapsed;
                        dockPanelInfluenceRange.Visibility = Visibility.Visible;
                        break;
                    case BuildingInfluenceType.Both:
                        dockPanelInfluenceRadius.Visibility = Visibility.Visible;
                        dockPanelInfluenceRange.Visibility = Visibility.Visible;
                        break;
                    default:
                        dockPanelInfluenceRadius.Visibility = Visibility.Collapsed;
                        dockPanelInfluenceRange.Visibility = Visibility.Collapsed;
                        break;
                }
            }
        }

        private void TextBoxSearchPresetsGotFocus(object sender, RoutedEventArgs e)
        {
            if (e.Source is TextBox textBox)
            {
                if (textBox.Text == "")
                {
                    _treeViewState = treeViewPresets.GetTreeViewState();
                }
            }
        }

        private void TextBoxSearchPresetsKeyUp(object sender, KeyEventArgs e)
        {
            var txt = sender as TextBox;
            try
            {
                if (txt.Text == "")
                {
                    _treeViewSearch.Reset();
                    treeViewPresets.SetTreeViewState(_treeViewState);
                }
                else
                {
                    _treeViewSearch.Search(txt.Text);
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

        private void TreeViewPresets_Loaded(object sender, RoutedEventArgs e)
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
            Settings.Default.TreeViewSearchText = TextBoxSearchPresets.Text; //Set explicity despite the data binding as UpdateProperty is only called on LostFocus
            Settings.Default.Save();
        }


    }
}