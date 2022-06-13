﻿using System;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using AnnoDesigner.CommandLine;
using AnnoDesigner.CommandLine.Arguments;
using AnnoDesigner.Core.Extensions;
using AnnoDesigner.Core.Layout;
using AnnoDesigner.Core.Models;
using AnnoDesigner.ViewModels;
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
        private readonly IAppSettings _appSettings;

        public new MainViewModel DataContext { get => base.DataContext as MainViewModel; set => base.DataContext = value; }

        #region Initialization

        public MainWindow(IAppSettings appSettingsToUse)
        {
            InitializeComponent();

            _appSettings = appSettingsToUse;
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            _mainViewModel = DataContext;
            _mainViewModel.AnnoCanvas = annoCanvas;
            _mainViewModel.AnnoCanvas.RegisterHotkeys(_mainViewModel.HotkeyCommandManager);

            _mainViewModel.ShowStatisticsChanged += MainViewModel_ShowStatisticsChanged;

            App.DpiScale = VisualTreeHelper.GetDpi(this);

            DpiChanged += MainWindow_DpiChanged;

            ToggleStatisticsView(_mainViewModel.StatisticsViewModel.IsVisible);

            _mainViewModel.LoadSettings();

            _mainViewModel.LoadAvailableIcons();

            //load presets before checking for updates
            _mainViewModel.LoadPresets();

            // check for updates on startup
            if (_appSettings.EnableAutomaticUpdateCheck)
            {
                //just fire and forget
                _ = _mainViewModel.PreferencesUpdateViewModel.CheckForUpdates(isAutomaticUpdateCheck: true);
            }

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

            // load file given by argument
            if (App.StartupArguments is OpenArgs startupArgs && !string.IsNullOrEmpty(startupArgs.Filename))
            {
                _mainViewModel.OpenFile(startupArgs.Filename);
            }

            if (App.StartupArguments is ExportArgs exportArgs && !string.IsNullOrEmpty(exportArgs.Filename) && !string.IsNullOrEmpty(exportArgs.ExportedFilename))
            {
                var layout = new LayoutLoader().LoadLayout(exportArgs.Filename);
                _mainViewModel.PrepareCanvasForRender(layout.Objects, Enumerable.Empty<AnnoObject>(), Math.Max(exportArgs.Border, 0), new Models.CanvasRenderSetting()
                {
                    GridSize = Math.Max(Math.Min(exportArgs.GridSize, Constants.GridStepMax), Constants.GridStepMin),
                    RenderGrid = exportArgs.UseUserSetting ? _appSettings.ShowGrid : !exportArgs.HideGrid,
                    RenderHarborBlockedArea = exportArgs.UseUserSetting ? _appSettings.ShowHarborBlockedArea : exportArgs.RenderHarborBlockedArea,
                    RenderIcon = exportArgs.UseUserSetting ? _appSettings.ShowIcons : !exportArgs.HideIcon,
                    RenderInfluences = exportArgs.UseUserSetting ? _appSettings.ShowInfluences : exportArgs.RenderInfluences,
                    RenderLabel = exportArgs.UseUserSetting ? _appSettings.ShowLabels : !exportArgs.HideLabel,
                    RenderPanorama = exportArgs.UseUserSetting ? _appSettings.ShowPanorama : exportArgs.RenderPanorama,
                    RenderTrueInfluenceRange = exportArgs.UseUserSetting ? _appSettings.ShowTrueInfluenceRange : exportArgs.RenderTrueInfluenceRange,
                    RenderStatistics = exportArgs.UseUserSetting ? _appSettings.StatsShowStats : !exportArgs.HideStatistics,
                    RenderVersion = !exportArgs.HideVersion
                }).RenderToFile(exportArgs.ExportedFilename);

                ConsoleManager.Show();
                Console.WriteLine($"Export of \"{exportArgs.Filename}\" completed");
                ConsoleManager.Hide();

                Close();
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
            if (!annoCanvas.CheckUnsavedChanges())
            {
                e.Cancel = true;
                return;
            }

            if (WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
            }

            _mainViewModel.MainWindowWindowState = WindowState;

            _mainViewModel.SaveSettings();

#if DEBUG
            var userConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath;
            logger.Trace($"saving settings: \"{userConfig}\"");
#endif
        }
    }
}