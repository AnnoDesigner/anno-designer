using AnnoDesigner.Localization;
using AnnoDesigner.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AnnoDesigner
{
    /// <summary>
    /// Interaction logic for Welcome.xaml
    /// </summary>
    public partial class Welcome : Window
    {
        public Welcome()
        {
            InitializeComponent();
            var languages = new List<SupportedLanguage>()
            {
                new SupportedLanguage()
                {
                    Name = "English",
                    FlagPath = "Flags/United Kingdom.png"
                },
                new SupportedLanguage()
                {
                    Name = "Deutsch",
                    FlagPath = "Flags/Germany.png"
                },
                new SupportedLanguage()
                {
                    Name = "Polski",
                    FlagPath = "Flags/Poland.png"
                },
                new SupportedLanguage()
                {
                    Name = "Русский",
                    FlagPath = "Flags/Russia.png"
                }
            };
            LanguageSelection.ItemsSource = languages;
        }

        private static int _selectedIndex = -1;

        private void LanguageSelection_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var lb = (ListBox)sender;
            if (lb.SelectedItem != null)
            {
                LoadSelectedLanguage();
            }
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedIndex != -1)
            {
                LoadSelectedLanguage();
            }
            else
            {
                //Show a message;
                Microsoft.Windows.Controls.MessageBox.Show(this, "Please select a langauge before continuing");
            }
        }

        private void LoadSelectedLanguage()
        {
            MainWindow.SelectedLanguage = ((SupportedLanguage)LanguageSelection.SelectedItem).Name;
            Settings.Default.SelectedLanguage = MainWindow.SelectedLanguage;
            Settings.Default.Save();
            this.Close();
        }

        private void LanguageSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var lb = (ListBox)sender;
            _selectedIndex = lb.SelectedIndex;
        }
    }
}
