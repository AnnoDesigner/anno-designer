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
using System.Windows.Shapes;
using AnnoDesigner.viewmodel;

namespace AnnoDesigner
{
    /// <summary>
    /// Interaction logic for InputWindow.xaml
    /// </summary>
    public partial class InputWindow : Window
    {
        public InputWindow()
        {
            InitializeComponent();
        }

        public InputWindow(MainViewModel context, string message, string title, string defaultValue = "") : this()
        {
            InitializeComponent();

            DataContext = context;

            Loaded += new RoutedEventHandler(InputWindow_Loaded);

            this.message.Text = message;
            Title = title;
            input.Text = defaultValue;
        }

        private void InputWindow_Loaded(object sender, RoutedEventArgs e)
        {
            input.Focus();
        }

        public static string Prompt(MainViewModel context, string message, string title, string defaultValue = "")
        {
            var inputWindow = new InputWindow(context, message, title, defaultValue);
            inputWindow.ShowDialog();

            if (inputWindow.DialogResult == true)
            {
                return inputWindow.ResponseText;
            }

            return null;
        }

        public string ResponseText
        {
            get { return input.Text; }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
