using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AnnoDesigner
{
    /// <summary>
    /// Interaction logic for MainControl.xaml
    /// </summary>
    public partial class MainControl : UserControl
    {
        public MainControl()
        {
            InitializeComponent();
        }

        public AnnoCanvas Canvas
        {
            get { return (AnnoCanvas)GetValue(CanvasProperty); }
            set { SetValue(CanvasProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Canvas.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CanvasProperty =
            DependencyProperty.Register("Canvas", typeof(AnnoCanvas), typeof(MainControl), new PropertyMetadata());

        public StatisticsView Statistics
        {
            get { return (StatisticsView)GetValue(StatisticsProperty); }
            set { SetValue(StatisticsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Statistics.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StatisticsProperty =
            DependencyProperty.Register("Statistics", typeof(StatisticsView), typeof(MainControl), new PropertyMetadata());

        public void ToggleStatisticsView(bool showStatisticsView)
        {
            colStatisticsView.MinWidth = showStatisticsView ? 100 : 0;
            colStatisticsView.Width = showStatisticsView ? GridLength.Auto : new GridLength(0);

            statisticsView.Visibility = showStatisticsView ? Visibility.Visible : Visibility.Collapsed;
            statisticsView.MinWidth = showStatisticsView ? 100 : 0;

            splitterStatisticsView.Visibility = showStatisticsView ? Visibility.Visible : Visibility.Collapsed;
        }


    }
}
