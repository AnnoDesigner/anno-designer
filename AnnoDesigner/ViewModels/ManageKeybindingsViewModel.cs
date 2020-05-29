using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Models;

namespace AnnoDesigner.ViewModels
{
    public class ManageKeybindingsViewModel : Notify
    {
        public HotkeyCommandManager HotkeyCommandManager { get; set; }

        public ManageKeybindingsViewModel()
        {

        }

        public ManageKeybindingsViewModel(HotkeyCommandManager hotkeyCommandManager)
        {
            Manager = hotkeyCommandManager;
            RebindCommand = new RelayCommand<Hotkey>(ExecuteRebind);
        }

        private HotkeyCommandManager _manager;
        public HotkeyCommandManager Manager
        {
            get { return _manager; }
            set { UpdateProperty(ref _manager, value); }
        }

        private ICommand _rebindCommand;
        public ICommand RebindCommand
        {
            get { return _rebindCommand; }
            set { UpdateProperty(ref _rebindCommand, value); }
        }

        private void ExecuteRebind(Hotkey hotkey)
        {

        }
    }
}
