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
        }

        private void ButtonProjectHomepageClick(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://code.google.com/p/anno-designer/");
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
            System.Diagnostics.Process.Start("http://anno2070.wikia.com/wiki/Anno_2070_Wiki");
        }
    }
}
