using System;
using System.ComponentModel;
using System.Configuration;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
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

        #region Initialization

        public MainWindow(IAppSettings appSettingsToUse)
        {
            InitializeComponent();

            _appSettings = appSettingsToUse;
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            _mainViewModel = DataContext as MainViewModel;
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
            if (!string.IsNullOrEmpty(App.FilenameArgument))
            {
                _mainViewModel.OpenFile(App.FilenameArgument);
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