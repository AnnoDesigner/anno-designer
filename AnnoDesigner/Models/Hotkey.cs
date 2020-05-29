using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AnnoDesigner.Core.Models;

namespace AnnoDesigner.Models
{
    /// <summary>
    /// Acts as a wrapper for a named <see cref="InputBinding"/>, and allows updating if the current Binding is replaced with a fresh reference.
    /// </summary>
    public class Hotkey : Notify
    {
        private Hotkey() { }
        public Hotkey(string hotkeyName, InputBinding binding):this(hotkeyName, binding, null) { }
        public Hotkey(string hotkeyName, InputBinding binding, string description)
        {
            Name = hotkeyName;
            Binding = binding;
            Description = description;
        }

        private InputBinding _binding;
        public InputBinding Binding
        {
            get { return _binding; }
            set { UpdateProperty(ref _binding, value); }
        }

        private string _name;
        /// <summary>
        /// An identifier for the <see cref="Hotkey"/>, usually required to be unique.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { UpdateProperty(ref _name, value); }
        }

        private string _description;
        public string Description
        {
            get { return _description; }
            set { UpdateProperty(ref _description, value); }
        }

    }
}
