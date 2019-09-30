using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.ComponentModel;
using AnnoDesigner.Properties;
using System.Globalization;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Core;
using AnnoDesigner.Core.Presets.Models;
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

            DpiChanged += MainWindow_DpiChanged;

            ToggleStatisticsView(_mainViewModel.StatisticsViewModel.IsVisible);

            _mainViewModel.LoadSettings();

            _mainViewModel.LoadAvailableIcons();

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

        #endregion

        #region UI events

        private void MainWindow_DpiChanged(object sender, DpiChangedEventArgs e)
        {
            App.DpiScale = e.NewDpi;
            //TODO: Redraw statistics when change is merged.
        }

        private void ToggleStatisticsView(bool showStatisticsView)
        {
            colStatisticsView.MinWidth = showStatisticsView ? 100 : 0;
            colStatisticsView.Width = showStatisticsView ? GridLength.Auto : new GridLength(0);

            statisticsView.Visibility = showStatisticsView ? Visibility.Visible : Visibility.Collapsed;
            statisticsView.MinWidth = showStatisticsView ? 100 : 0;

            splitterStatisticsView.Visibility = showStatisticsView ? Visibility.Visible : Visibility.Collapsed;
        }

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