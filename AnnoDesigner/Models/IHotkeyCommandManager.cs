using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AnnoDesigner.Models
{
    public interface IHotkeyCommandManager
    {
        /// <summary>
        /// Registers a binding with the hotkey manager
        /// </summary>
        /// <param name="hotkeyId">A unique identifier for the hotkey</param>
        /// <param name="binding"></param>
        void AddBinding(string hotkeyId, InputBinding binding);
        void RemoveBinding(string hotkeyId);
        InputBinding GetBinding(string hotkeyId);

        void HandleCommand(InputEventArgs e);
    }



}
