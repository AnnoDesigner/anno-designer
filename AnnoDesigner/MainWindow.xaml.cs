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

        private static string _selectedLanguage;
        //for identifier checking process
        private static readonly List<string> IconFieldNamesCheck = new List<string> { "icon_116_22", "icon_27_6", "field", "general_module" };
        public string iconFileNameCheck = "";
        public BuildingPresets BuildingPreset { get; }
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

        private static Localization.MainWindow mainWindowLocalization;
        //About window does not need to be called, as it get's instantiated and used when the about window is created

        private void SelectedLanguageChanged()
        {
            mainWindowLocalization.UpdateLanguage();
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
            Settings.Default.SelectedLanguage = SelectedLanguage;
        }

        #region Initialization

        public MainWindow()
        {
            InitializeComponent();
            _instance = this;
            // initialize web client
            _webClient = new WebClient();
            _webClient.DownloadStringCompleted += WebClientDownloadStringCompleted;
            // add event handlers
            annoCanvas.OnCurrentObjectChanged += UpdateUIFromObject;
            annoCanvas.OnStatusMessageChanged += StatusMessageChanged;
            annoCanvas.OnLoadedFileChanged += LoadedFileChanged;
            annoCanvas.OnClipboardChanged += ClipboardChanged;

            //Get a reference an instance of Localization.MainWindow, so we can call UpdateLanguage() in the SelectedLanguage setter
            DependencyObject dependencyObject = LogicalTreeHelper.FindLogicalNode(this, "Menu");
            mainWindowLocalization = (Localization.MainWindow)((Menu)dependencyObject).DataContext;

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

            // check for updates on startup
            MenuItemVersion.Header = "Version: " + Constants.Version;
            MenuItemFileVersion.Header = "File version: " + Constants.FileVersion;
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
            treeViewPresets.Items.Add(new AnnoObject { Label = "Road tile", Size = new Size(1, 1), Radius = 0, Road = true, Identifier = "Road" });
            treeViewPresets.Items.Add(new AnnoObject { Label = "Borderless road tile", Size = new Size(1, 1), Radius = 0, Borderless = true, Road = true, Identifier = "Road" });
            BuildingPresets presets = annoCanvas.BuildingPresets;
            if (presets != null)
            {
                presets.AddToTree(treeViewPresets);
                GroupBoxPresets.Header = string.Format("Building presets - loaded v{0}", presets.Version);
                MenuItemPresetsVersion.Header = "Presets version: " + presets.Version;
            }
            else
            {
                GroupBoxPresets.Header = "Building presets - load failed";
            }

            if (Settings.Default.TreeViewState != null && Settings.Default.TreeViewState.Count > 0)
            {
                try
                {
                    treeViewPresets.SetTreeViewState(Settings.Default.TreeViewState);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to restore previous preset menu settings.");
                    App.WriteToErrorLog("TreeView SetTreeViewState Error", ex.Message, ex.StackTrace);
                }
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
            textBoxWidth.Text = obj.Size.Width.ToString();
            textBoxHeight.Text = obj.Size.Height.ToString();
            // color
            colorPicker.SelectedColor = obj.Color;
            // label
            textBoxLabel.Text = obj.Label;
            // Ident
            textBoxIdentifier.Text = obj.Identifier;
            // templatename 
            textBoxTemlateName.Text = obj.Template;
            // icon
            try
            {
                comboBoxIcon.SelectedItem = string.IsNullOrEmpty(obj.Icon) ? _noIconItem : comboBoxIcon.Items.Cast<IconImage>().Single(_ => _.Name == Path.GetFileNameWithoutExtension(obj.Icon));
            }
            catch (Exception)
            {
                comboBoxIcon.SelectedItem = _noIconItem;
            }
            // radius
            textBoxRadius.Text = obj.Radius.ToString();
            //InfluenceRadius
            textBoxInfluenceRange.Text = obj.InfluenceRange.ToString();
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
            StatusBarItemClipboardStatus.Content = "Items on clipboard: " + l.Count;
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
                Size = new Size(int.Parse(textBoxWidth.Text), int.Parse(textBoxHeight.Text)),
                Color = colorPicker.SelectedColor.HasValue ? colorPicker.SelectedColor.Value : Colors.Red,
                Label = IsChecked(checkBoxLabel) ? textBoxLabel.Text : "",
                Icon = comboBoxIcon.SelectedItem == _noIconItem ? null : ((IconImage)comboBoxIcon.SelectedItem).Name,
                Radius = string.IsNullOrEmpty(textBoxRadius.Text) ? 0 : double.Parse(textBoxRadius.Text, CultureInfo.InvariantCulture),
                InfluenceRange = string.IsNullOrEmpty(textBoxInfluenceRange.Text) ? 0 : double.Parse(textBoxInfluenceRange.Text, CultureInfo.InvariantCulture),
                Borderless = IsChecked(checkBoxBorderless),
                Road = IsChecked(checkBoxRoad),
                Identifier = textBoxIdentifier.Text,
                Template = textBoxTemlateName.Text
            };
            // Turn obj.Icon into a checkable filename *iconFileNameCheck*
            if (!string.IsNullOrEmpty(obj.Icon))
            {
                if (obj.Icon.StartsWith("A5_"))
                {
                    iconFileNameCheck = obj.Icon.Remove(0, 3) + ".png"; //when Anno 2070, it use not A5_ in the original naming.
                }
                else
                {
                    iconFileNameCheck = obj.Icon + ".png";
                }
            }
            // do some sanity checks
            if (obj.Size.Width > 0 && obj.Size.Height > 0 && obj.Radius >= 0)
            {
                if (!string.IsNullOrEmpty(obj.Icon) && obj.Icon.Contains(IconFieldNamesCheck) == false)
                {
                    //gets icons origin building info
                    var buildingsIconCheck = annoCanvas.BuildingPresets.Buildings.FirstOrDefault(_ => _.IconFileName == iconFileNameCheck);
                    if (buildingsIconCheck != null)
                    {
                        // Check X and Z Sizes if the Building Info, if one of both not right, the Object will be Unknown
                        if ((obj.Size.Width != buildingsIconCheck.BuildBlocker["x"] && obj.Size.Width != buildingsIconCheck.BuildBlocker["z"]) || (obj.Size.Height != buildingsIconCheck.BuildBlocker["x"] && obj.Size.Height != buildingsIconCheck.BuildBlocker["z"]))
                        {
                            //Size X is not correct on Building Info, call it Unknown Object
                            obj.Identifier = "Unknown Object";
                        }
                        else
                        {
                            //if sizes and icon is a existing building in the presets, call it that onject
                            obj.Identifier = buildingsIconCheck.Identifier;
                        }
                    }
                    else if (textBoxTemlateName.Text.ToLower().Contains("field") == false) //check if the icon is removed from a template field
                    {
                        obj.Identifier = "Unknown Object";
                    }
                    //annoCanvas.SetCurrentObject(obj);
                }
                else if (!string.IsNullOrEmpty(obj.Icon) && obj.Icon.Contains(IconFieldNamesCheck) == true)
                {
                    //Check if Field Icon belongs to the field identifier, else set the official icon
                    var buildingsIconCheck = annoCanvas.BuildingPresets.Buildings.FirstOrDefault(_ => _.Identifier == obj.Identifier);
                    if (buildingsIconCheck != null)
                    {
                        if (iconFileNameCheck != buildingsIconCheck.IconFileName)
                        {
                            obj.Icon = buildingsIconCheck.IconFileName.Remove(buildingsIconCheck.IconFileName.Length - 4, 4); //rmeove the .png for the comboBoxIcon
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
                        //when it is a not existing building, then call it Unknown Object
                        obj.Identifier = "Unknown Object";
                    }
                    //annoCanvas.SetCurrentObject(obj);
                }
                if (textBoxTemlateName.Text.ToLower().Contains("field") == false && string.IsNullOrEmpty(obj.Icon))
                {
                    obj.Identifier = "Unknown Object";
                }
                // set current object to mouse (original line)
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
                        Color = colorPicker.SelectedColor.HasValue ? colorPicker.SelectedColor.Value : Colors.Red,
                    });
                    ApplyCurrentObject();
                }
            }
            catch (Exception whaterror)
           {
               MessageBox.Show("Something went wrong while applying the preset." + Environment.NewLine + whaterror.Message);
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
                treeViewPresets.Items.Add(new AnnoObject { Label = "Road tile", Size = new Size(1, 1), Radius = 0, Road = true, Identifier = "Road" });
                treeViewPresets.Items.Add(new AnnoObject { Label = "Borderless road tile", Size = new Size(1, 1), Radius = 0, Borderless = true, Road = true, Identifier = "Road" });
                annoCanvas.BuildingPresets.AddToTree(treeViewPresets);
            }
        }

        #endregion

        #region UI events

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

            showRegistrationMessageBox(isDeregistration: false);
        }

        private void MenuItemUnregisterExtensionClick(object sender, RoutedEventArgs e)
        {
            // removes the registry entries            

            Registry.CurrentUser.DeleteSubKeyTree(@"Software\Classes\anno_designer",true);
            Registry.CurrentUser.DeleteSubKeyTree(@"Software\Classes\.ad",true);

            showRegistrationMessageBox(isDeregistration: true);

        }

        private void showRegistrationMessageBox(bool isDeregistration)
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
        #endregion

        private void WindowClosing(object sender, CancelEventArgs e)
        {
            Settings.Default.TreeViewState = treeViewPresets.GetTreeViewState();
            Settings.Default.Save();
        }

        private void LanguageMenuSubmenuClosed(object sender, RoutedEventArgs e)
        {
            SelectedLanguageChanged();
        }

    }
}