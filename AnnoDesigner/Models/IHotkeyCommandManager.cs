using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AnnoDesigner.Models
{
    public interface IHotkeyCommandManager<T> where T: InputBinding
    {
        /// <summary>
        /// Registers a binding with the hotkey manager
        /// </summary>
        /// <param name="hotkeyId">A unique identifier for the hotkey</param>
        /// <param name="binding"></param>
        void AddBinding(string hotkeyId, T binding);
        void RemoveBinding(string hotkeyId);
        T GetBinding(string hotkeyId);

        void HandleCommand(KeyEventArgs e);
        void HandleCommand(MouseButtonEventArgs e);
    }



}
