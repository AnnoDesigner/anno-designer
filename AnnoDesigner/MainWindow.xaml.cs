using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using MessageBox = Xceed.Wpf.Toolkit.MessageBox;
using System.Windows.Media;
using System.ComponentModel;
using AnnoDesigner.Properties;
using System.Globalization;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AnnoDesigner.model;
using AnnoDesigner.Core.Extensions;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Core;
using AnnoDesigner.Core.Presets.Models;
using AnnoDesigner.Core.Presets.Helper;
using System.Text;
using System.Configuration;
using AnnoDesigner.Core.Helper;
using AnnoDesigner.viewmodel;
using System.Windows.Threading;
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

        //for identifier checking process
        private static readonly List<string> IconFieldNamesCheck = new List<string> { "icon_116_22", "icon_27_6", "field", "general_module" };

        public BuildingPresets BuildingPresets { get; }

        private static MainViewModel _mainViewModel;

        #region Initialization

        public MainWindow()
        {
            InitializeComponent();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            _mainViewModel = DataContext as MainViewModel;
            _mainViewModel.AnnoCanvas = annoCanvas;
            _mainViewModel.ShowStatisticsChanged += MainViewModel_ShowStatisticsChanged;

            App.DpiScale = VisualTreeHelper.GetDpi(this);

            // add event handlers
            annoCanvas.OnCurrentObjectChanged += UpdateUIFromObject;
            annoCanvas.OnStatusMessageChanged += StatusMessageChanged;
            annoCanvas.OnLoadedFileChanged += LoadedFileChanged;

            DpiChanged += MainWindow_DpiChanged;

            ToggleStatisticsView(_mainViewModel.StatisticsViewModel.IsVisible);

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
            _ = _mainViewModel.CheckForUpdatesSub(false);//just fire and forget

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

        private void MainViewModel_ShowStatisticsChanged(object sender, EventArgs e)
        {
            ToggleStatisticsView(_mainViewModel.StatisticsViewModel.IsVisible);
        }

        private void LoadSettings()
        {
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
            _mainViewModel.StatusMessage = message;
            logger.Trace($"Status message changed: {message}");
        }

        private void LoadedFileChanged(string filename)
        {
            Title = string.IsNullOrEmpty(filename) ? "Anno Designer" : string.Format("{0} - Anno Designer", Path.GetFileName(filename));
            logger.Info($"Loaded file: {(string.IsNullOrEmpty(filename) ? "(none)" : filename)}");
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

        private void ToggleStatisticsView(bool showStatisticsView)
        {
            colStatisticsView.MinWidth = showStatisticsView ? 100 : 0;
            colStatisticsView.Width = showStatisticsView ? GridLength.Auto : new GridLength(0);

            statisticsView.Visibility = showStatisticsView ? Visibility.Visible : Visibility.Collapsed;
            statisticsView.MinWidth = showStatisticsView ? 100 : 0;

            splitterStatisticsView.Visibility = showStatisticsView ? Visibility.Visible : Visibility.Collapsed;
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

            Settings.Default.StatsShowStats = _mainViewModel.StatisticsViewModel.IsVisible;
            Settings.Default.StatsShowBuildingCount = _mainViewModel.StatisticsViewModel.ShowStatisticsBuildingCount;

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
    }
}