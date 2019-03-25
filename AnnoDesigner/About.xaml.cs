using System.Windows;
using System.Windows.Input;

namespace AnnoDesigner
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About
        : Window
    {
        public About()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void ButtonOriginalHomepageClick(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://code.google.com/p/anno-designer/");
        }

        private void ButtonProjectHomepageClick(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/AgmasGold/anno-designer/");
        }

        private void ButtonCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void WindowKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void ButtonWikiaClick(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://anno1800.fandom.com/wiki/Anno_Designer");
        }
    }
}
