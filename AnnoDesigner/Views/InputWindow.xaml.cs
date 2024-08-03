using AnnoDesigner.ViewModels;
using System.Windows;

namespace AnnoDesigner;

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
        _ = input.Focus();
    }

    public static string Prompt(MainViewModel context, string message, string title, string defaultValue = "")
    {
        InputWindow inputWindow = new(context, message, title, defaultValue);
        _ = inputWindow.ShowDialog();

        return inputWindow.DialogResult == true ? inputWindow.ResponseText : null;
    }

    public string ResponseText => input.Text;

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
