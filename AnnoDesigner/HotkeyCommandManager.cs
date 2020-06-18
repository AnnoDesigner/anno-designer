using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Models;

namespace AnnoDesigner
{
    public class HotkeyCommandManager : Notify, INotifyCollectionChanged
    {
        private readonly Dictionary<string, Hotkey> bindings;

        /// <summary>
        /// Represents a read-only data-bindable collection of hotkeys.
        /// </summary>
        //public ReadOnlyObservableCollection<Hotkey> ObservableCollection { get; }
        public ObservableCollection<Hotkey> ObservableCollection { get; }
        /// <summary>
        /// Backing collection for the ObservableCollection property.
        /// </summary>
        private readonly ObservableCollection<Hotkey> _observableCollection;

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public HotkeyCommandManager()
        {
            bindings = new Dictionary<string, Hotkey>();
            _observableCollection = new ObservableCollection<Hotkey>();
            //ObservableCollection = new ReadOnlyObservableCollection<Hotkey>(_observableCollection);
            ObservableCollection = _observableCollection;
        }

        public void HandleCommand(InputEventArgs e)
        {
            IEnumerable<Hotkey> hotkeys = bindings.Values;
            foreach (var item in hotkeys)
            {
                
                if (item?.Binding?.Command?.CanExecute(item.Binding.CommandParameter) ?? false)
                {
                    if (item.Binding.Gesture.Matches(e.Source, e))
                    {
                        item.Binding.Command.Execute(item.Binding.CommandParameter);
                        e.Handled = true;
                    }
                }
            }
        }

        /// <summary>
        /// Registers a binding with the hotkey manager. Creates a <see cref="Hotkey"/> and adds it to the <see cref="HotkeyCommandManager"/>.
        /// </summary>
        /// <param name="bindingId">A unique identifier for the hotkey. Also acts as the key for managing localization if adding an IDescriptiveHotkeyBinding</param>
        /// <param name="binding"></param>
        public void AddBinding(string bindingId, InputBinding binding)
        {
            AddBinding(new Hotkey(bindingId, binding));
        }

        /// <summary>
        /// Registers a binding with the hotkey manager.
        /// </summary>
        /// <param name="bindingId">A unique identifier for the hotkey. Also acts as the key for managing localization if adding an IDescriptiveHotkeyBinding</param>
        /// <param name="binding"></param>
        public void AddBinding(Hotkey hotkey)
        {
            if (!bindings.ContainsKey(hotkey.Name))
            {
                hotkey.PropertyChanged += Hotkey_PropertyChanged;
                bindings.Add(hotkey.Name, hotkey);
                _observableCollection.Add(hotkey);
                //Check for localization
                var language = Localization.Localization.GetLanguageCodeFromName(Commons.Instance.SelectedLanguage);
                if (Localization.Localization.Translations[language].TryGetValue(hotkey.Name, out var description))
                {
                    hotkey.Description = description;
                }
            }
            else
            {
                throw new ArgumentException($"Key {hotkey.Name} already exists in collection.", "bindingId");
            }
        }

        private void Hotkey_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Debug.WriteLine($"Changed {sender}, Property Name: {e.PropertyName}. Updating Collection");
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public void RemoveBinding(string bindingId)
        {
            if (bindings.ContainsKey(bindingId))
            {
                bindings[bindingId].PropertyChanged -= Hotkey_PropertyChanged;
                _observableCollection.Remove(bindings[bindingId]);
                bindings.Remove(bindingId);
            }
            else
            {
                throw new KeyNotFoundException($"Key {bindingId} does not exist");
            }
        }

        public IEnumerable<Hotkey> GetBindings()
        {
            return bindings.Values;
        }

        /// <summary>
        /// Returns true if the specified binding exists in this <see cref="HotkeyCommandManager{T}"/>
        /// </summary>
        /// <param name="bindingId">A unique identifier for the hotkey</param>
        /// <returns></returns>
        public bool ContainsBinding(string bindingId)
        {
            return bindings.ContainsKey(bindingId);
        }

        public Hotkey GetBinding(string bindingId)
        {
            if (!bindings.ContainsKey(bindingId))
            {
                throw new KeyNotFoundException($"Key {bindingId} does not exist");
            }
            return bindings[bindingId];
        }

        public void UpdateLanguage()
        {
            var language = Localization.Localization.GetLanguageCodeFromName(Commons.Instance.SelectedLanguage);
            foreach (var kvp in bindings)
            {
                if (Localization.Localization.Translations[language].TryGetValue(kvp.Key, out var description))
                {
                    kvp.Value.Description = description;
                }
            }
        }
    }
}
