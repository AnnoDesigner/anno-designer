using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AnnoDesigner.Models;

namespace AnnoDesigner
{
    public class HotkeyCommandManager<T> : IHotkeyCommandManager<T> where T: InputBinding
    {
        private readonly Dictionary<string, T> hotkeyBindings;

        /// <summary>
        /// Represents a data-bindable collection of hotkey bindings.
        /// </summary>
        public ObservableCollection<T> ObservableCollection { get; private set; }

        public HotkeyCommandManager()
        {
            hotkeyBindings = new Dictionary<string, T>();
        }

        public void HandleCommand(KeyEventArgs e)
        {
            IEnumerable<T> hotkeys = hotkeyBindings.Values;
            foreach (var item in hotkeys)
            {
                if (item.Command.CanExecute(item.CommandParameter))
                {
                    if (item.Gesture.Matches(e.Source, e))
                    {
                        item.Command.Execute(item.CommandParameter);
                        e.Handled = true;
                    }
                }
            }
        }

        public void HandleCommand(MouseButtonEventArgs e)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Registers a binding with the hotkey manager
        /// </summary>
        /// <param name="bindingId">A unique identifier for the hotkey. Also acts as the key for managing localization if adding an IDescriptiveHotkeyBinding</param>
        /// <param name="binding"></param>
        public void AddBinding(string bindingId, T binding)
        {
            if (!hotkeyBindings.ContainsKey(bindingId))
            {
                hotkeyBindings.Add(bindingId, binding);
            }
            else
            {
                hotkeyBindings[bindingId] = binding;
            }
        }

        public void RemoveBinding(string hotkeyId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> GetBindings()
        {
            return hotkeyBindings.Values;
        }

        /// <summary>
        /// Returns true if the specified binding exists in this <see cref="HotkeyCommandManager{T}"/>
        /// </summary>
        /// <param name="bindingId"></param>
        /// <returns></returns>
        public bool ContainsBinding(string bindingId)
        {
            return hotkeyBindings.ContainsKey(bindingId);
        }

        public T GetBinding(string bindingId)
        {
            if (!hotkeyBindings.ContainsKey(bindingId))
            {
                throw new ArgumentException($"Specified binding {bindingId} does not exist.", nameof(bindingId));
            }
            return hotkeyBindings[bindingId];
        }

    }

    public class HotkeyCommandManager : HotkeyCommandManager<InputBinding>, IHotkeyCommandManager<InputBinding>
    {

    }
}
