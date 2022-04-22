using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
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

            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(10);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            static IEnumerable<PresetLayout> getRecursively(IPresetLayout preset)
            {
                if (preset is PresetLayout presetLayout)
                {
                    yield return presetLayout;
                }
                if (preset is PresetLayoutDirectory presetLayoutDirectory)
                {
                    foreach (var item in presetLayoutDirectory.Presets.SelectMany(getRecursively))
                    {
                        yield return item;
                    }
                }
            }

            foreach (var item in Context.Presets.SelectMany(getRecursively))
            {
                if (item != Context.SelectedPreset)
                {
                    item.UnloadImages();
                }
            }
        }

        private async void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            Context.SelectedPreset = e.NewValue as PresetLayout;
            if (Context.SelectedPreset != null)
            {
                if (Context.SelectedPreset.Images == null)
                {
                    await Context.SelectedPreset.LoadImages();
                }
            }
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
            Context.SelectedPreset = null;
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
