using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AnnoDesigner.Core.Layout.Presets;
using AnnoDesigner.Models;

namespace AnnoDesigner
{
    /// <summary>
    /// Interaction logic for PresetLayoutWindow.xaml
    /// </summary>
    public partial class PresetLayoutWindow : Window
    {
        public PresetLayoutViewModel Context
        {
            get => DataContext as PresetLayoutViewModel;
            set => DataContext = value;
        }

        public PresetLayoutWindow()
        {
            InitializeComponent();
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            Context.SelectedPreset = e.NewValue as PresetLayout;
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            if (Context.BeforeLayoutOpen?.Invoke() ?? true)
            {
                DialogResult = true;
                Close();
            }
        }

        private void TreeViewItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2 && e.ChangedButton == MouseButton.Left && (sender as FrameworkElement).DataContext is PresetLayout layout)
            {
                Context.SelectedPreset = layout;
                LoadButton_Click(null, null);
            }
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2 && e.ChangedButton == MouseButton.Left)
            {
                new ImageWindow()
                {
                    DataContext = (sender as Image).Source
                }.ShowDialog();
            }
        }

        private async void ReloadLayoutPresets_Click(object sender, RoutedEventArgs e)
        {
            await Context.LoadLayoutsAsync();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Context.Presets == null)
            {
                await Context.LoadLayoutsAsync();
            }
        }
    }
}
