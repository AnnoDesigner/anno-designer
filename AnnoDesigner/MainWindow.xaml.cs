using AnnoDesigner.Presets;
using AnnoDesigner.UI;
using Microsoft.Win32;
using Microsoft.Windows.Controls;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using MessageBox = Microsoft.Windows.Controls.MessageBox;
using System.ComponentModel;

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

        private static string _selectedLanguage;
        public static string SelectedLanguage
        {
            get { return _selectedLanguage == null ? "English" : _selectedLanguage; }
            set {
                _selectedLanguage = value == null ? "English" : value;
                mainWindowLocalization.UpdateLanguage();
            }
        }

        private static Localization.MainWindow mainWindowLocalization;
        //About window does not need to be called, as it get's instantiated and used when the about window is created

        #region Initialization

        public MainWindow()
        {
            InitializeComponent();
            // initialize web client
            _webClient = new WebClient();
            _webClient.DownloadStringCompleted += WebClientDownloadStringCompleted;
            // add event handlers
            annoCanvas.OnCurrentObjectChanged += UpdateUIFromObject;
            annoCanvas.OnStatusMessageChanged += StatusMessageChanged;
            annoCanvas.OnLoadedFileChanged += LoadedFileChanged;

            //Get a reference an instance of Localization.MainWindow, so we can call UpdateLanguage() in the SelectedLanguage setter
            DependencyObject dependencyObject = LogicalTreeHelper.FindLogicalNode(this, "Menu");
            mainWindowLocalization = (Localization.MainWindow)((Menu)dependencyObject).DataContext;
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            // add icons to the combobox
            comboBoxIcon.Items.Clear();
            _noIconItem = new IconImage("None");
            comboBoxIcon.Items.Add(_noIconItem);
            foreach (System.Collections.Generic.KeyValuePair<string, IconImage> icon in annoCanvas.Icons)
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
            try
            {
                ColorPresets colorPresets = DataIO.LoadFromFile<ColorPresets>(Path.Combine(App.ApplicationPath, Constants.ColorPresetsFile));
                foreach (ColorScheme colorScheme in colorPresets.ColorSchemes)
                {
                    foreach (ColorInfo colorInfo in colorScheme.ColorInfos)
                    {
                        colorPicker.StandardColors.Add(new ColorItem(colorInfo.Color, string.Format("{0} ({1})", colorInfo.ColorTarget, colorScheme.Name)));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Loading of the color presets failed");
            }
            // load presets
            treeViewPresets.Items.Clear();
            // manually add a road tile preset
            treeViewPresets.Items.Add(new AnnoObject { Label = "Road tile", Size = new Size(1, 1), Radius = 0, Road = true });
            treeViewPresets.Items.Add(new AnnoObject { Label = "Borderless road tile", Size = new Size(1, 1), Radius = 0, Borderless = true, Road = true });
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
            _webClient.DownloadStringAsync(new Uri("http://anno-designer.googlecode.com/svn/trunk/version.txt"), forcedCheck);
        }

        private void WebClientDownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message, "Version check failed");
                return;
            }
            if (int.Parse(e.Result) > Constants.Version)
            {
                // new version found
                if (MessageBox.Show("A newer version was found, do you want to visit the project page?\nhttp://anno-designer.googlecode.com/", "Update available", MessageBoxButton.YesNo, MessageBoxImage.Asterisk, MessageBoxResult.OK) == MessageBoxResult.Yes)
                {
                    System.Diagnostics.Process.Start("http://code.google.com/p/anno-designer/downloads/list");
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
                Color = colorPicker.SelectedColor,
                Label = IsChecked(checkBoxLabel) ? textBoxLabel.Text : "",
                Icon = comboBoxIcon.SelectedItem == _noIconItem ? null : ((IconImage)comboBoxIcon.SelectedItem).Name,
                Radius = string.IsNullOrEmpty(textBoxRadius.Text) ? 0 : double.Parse(textBoxRadius.Text),
                Borderless = IsChecked(checkBoxBorderless),
                Road = IsChecked(checkBoxRoad)
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
                AnnoObject selectedItem = treeViewPresets.SelectedItem as AnnoObject;
                if (selectedItem != null)
                {
                    UpdateUIFromObject(new AnnoObject(selectedItem) { Color = colorPicker.SelectedColor });
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
                treeViewPresets.Items.Add(new AnnoObject { Label = "Road tile", Size = new Size(1, 1), Radius = 0, Road = true });
                treeViewPresets.Items.Add(new AnnoObject { Label = "Borderless road tile", Size = new Size(1, 1), Radius = 0, Borderless = true, Road = true });
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

        private void MenuItemStatsClick(object sender, RoutedEventArgs e)
        {
            annoCanvas.RenderStats = !annoCanvas.RenderStats;
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
            ShowRegistrationMessageBox();
        }

        private void MenuItemUnregisterExtensionClick(object sender, RoutedEventArgs e)
        {
            // removes the registry entries
            Registry.CurrentUser.DeleteSubKeyTree(@"Software\Classes\anno_designer");
            Registry.CurrentUser.DeleteSubKeyTree(@"Software\Classes\.ad");
            ShowRegistrationMessageBox();
        }

        private void ShowRegistrationMessageBox()
        {
            MessageBox.Show("You may need to reboot or relog for changes to take effect.", "Successful", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MenuItemHomepageClick(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://code.google.com/p/anno-designer/");
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
                //And re populate the tree view if the language has changed
                if (language != currentLanguage)
                {
                    SelectedLanguage = language;
                    RepopulateTreeView();
                }
            }
        }
        #endregion
    }
}