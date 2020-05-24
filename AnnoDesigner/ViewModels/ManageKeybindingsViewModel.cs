using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public ManageKeybindingsViewModel(HotkeyCommandManager hotkeyCommandManager)
        {

        }

        /// <summary>
        /// This is here for testing only - hotkeys will be bound via an ItemsSource eventually
        /// </summary>
        private KeyBinding _rotateKeybinding;
        public KeyBinding RotateKeybinding
        {
            get { return _rotateKeybinding; }
            set { UpdateProperty(ref _rotateKeybinding, value);  }
        }

        public ICommand RotateCommand { get; set; }
    }
}
