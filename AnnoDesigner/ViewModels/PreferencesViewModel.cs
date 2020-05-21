using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Models;
using NLog;

namespace AnnoDesigner.ViewModels
{
    public class PreferencesViewModel : Notify
    {
        public PreferencesViewModel(ICommons commons, IAppSettings appSettings)
        {
            this.commons = commons;
            KeyBindings = new ObservableCollection<PolyGesture>();
            KeyBindings.Add(new PolyGesture(Key.A, ModifierKeys.Control));
            ChangeBindingCommand = new RelayCommand(ExecuteChangeBinding);
            ShowKeybindingView = new RelayCommand<bool>(ExecuteShowKeybindingView);

        }

        private ICommons commons;

        private ObservableCollection<PolyGesture> _keyBindings;
        public ObservableCollection<PolyGesture> KeyBindings
        {
            get { return _keyBindings; }
            set { UpdateProperty(ref _keyBindings, value); }
        }

        public ICommand ChangeBindingCommand { get; set; }
        private void ExecuteChangeBinding(object param)
        {
            KeyBindings[0] = new PolyGesture(Key.V, ModifierKeys.Shift);
        }

        public ICommand ShowKeybindingView { get; set; }
        private void ExecuteShowKeybindingView (bool IsSelected)
        {
            //For testing
            Debug.WriteLine($"Is Selected: {IsSelected}");            
        }
    }


}
