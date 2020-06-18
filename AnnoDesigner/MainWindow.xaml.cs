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
using System.Text;
using System.Configuration;
using AnnoDesigner.Core.Helper;
using AnnoDesigner.ViewModels;
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
            _mainViewModel.AnnoCanvas.RegisterHotkeys(_mainViewModel.HotkeyCommandManager);

            _mainViewModel.ShowStatisticsChanged += MainViewModel_ShowStatisticsChanged;

            App.DpiScale = VisualTreeHelper.GetDpi(this);

            DpiChanged += MainWindow_DpiChanged;

            ToggleStatisticsView(_mainViewModel.StatisticsViewModel.IsVisible);

            _mainViewModel.LoadSettings();

            _mainViewModel.LoadAvailableIcons();

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

            _mainViewModel.LoadPresets();

            // load file given by argument
            if (!string.IsNullOrEmpty(App.FilenameArgument))
            {
                annoCanvas.OpenFile(App.FilenameArgument);
            }
            
            //For testing only
            //TODO: remove this before PR
            _mainViewModel.ShowPreferencesWindowCommand.Execute(null);
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