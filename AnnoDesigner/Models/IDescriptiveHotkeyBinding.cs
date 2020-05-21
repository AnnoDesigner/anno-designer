using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AnnoDesigner.Models
{
    public interface IDescriptiveHotkeyBinding
    {
        InputGesture Gesture { get; set; }
        ICommand Command { get; set; }
        /// <summary>
        /// A phrase describing what the hotkey does. e.g "Copy", "Paste", "Cut", "Rotate an object"
        /// </summary>
        string Description { get; set; }
    }
}
