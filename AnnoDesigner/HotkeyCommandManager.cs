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
        private readonly Dictionary<string, Hotkey> hotkeys;

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
            hotkeys = new Dictionary<string, Hotkey>();
            _observableCollection = new ObservableCollection<Hotkey>();
            ObservableCollection = _observableCollection;
        }

        public void HandleCommand(InputEventArgs e)
        {
            IEnumerable<Hotkey> values = hotkeys.Values;
            foreach (var item in values)
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
        /// Registers a hotkey with the hotkey manager. Creates a <see cref="Hotkey"/> from the provided parameters
        /// and adds it to the <see cref="HotkeyCommandManager"/>.
        /// </summary>
        /// <param name="hotkeyId">A unique identifier for the hotkey. Also acts as the key for localizing the hotkey description.</param>
        /// <param name="binding">A <see cref="KeyBinding"/> or <see cref="MouseBinding"/></param>
        public void AddBinding(string hotkeyId, InputBinding binding)
        {
            AddHotkey(new Hotkey(hotkeyId, binding));
        }

        /// <summary>
        /// Registers a <see cref="Hotkey"/> with the hotkey manager.
        /// </summary>
        /// <param name="hotkey"></param>
        public void AddHotkey(Hotkey hotkey)
        {
            if (!hotkeys.ContainsKey(hotkey.Name))
            {
                hotkey.PropertyChanged += Hotkey_PropertyChanged;
                hotkeys.Add(hotkey.Name, hotkey);
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
                throw new ArgumentException($"Key {hotkey.Name} already exists in collection.", "hotkey");
            }
        }

        private void Hotkey_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Debug.WriteLine($"Changed {sender}, Property Name: {e.PropertyName}. Updating Collection");
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary>
        /// Removes the hotkey that matches the specified hotkeyId
        /// </summary>
        /// <param name="hotkeyId"></param>
        public void RemoveHotkey(string hotkeyId)
        {
            if (hotkeys.ContainsKey(hotkeyId))
            {
                hotkeys[hotkeyId].PropertyChanged -= Hotkey_PropertyChanged;
                _observableCollection.Remove(hotkeys[hotkeyId]);
                hotkeys.Remove(hotkeyId);
            }
            else
            {
                throw new KeyNotFoundException($"Key {hotkeyId} does not exist");
            }
        }

        public IEnumerable<Hotkey> GetHotkeys()
        {
            return hotkeys.Values;
        }

        /// <summary>
        /// Resets all hotkeys to their defaults
        /// </summary>
        public void ResetHotkeys()
        {
            foreach (var hotkey in hotkeys.Values)
            {
                hotkey.Reset();
            }
        }

        /// <summary>
        /// Returns true if the specified hotkey exists in this <see cref="HotkeyCommandManager"/>
        /// </summary>
        /// <param name="hotkeyId">A unique identifier for the hotkey</param>
        /// <returns></returns>
        public bool ContainsHotkey(string hotkeyId)
        {
            return hotkeys.ContainsKey(hotkeyId);
        }

        /// <summary>
        /// Retrieves a <see cref="Hotkey"/>
        /// </summary>
        /// <param name="hotkeyId"></param>
        /// <returns></returns>
        public Hotkey GetHotkey(string hotkeyId)
        {
            if (!hotkeys.TryGetValue(hotkeyId, out var hotkey))
            {
                throw new KeyNotFoundException($"Key {hotkeyId} does not exist");
            }
            return hotkey;
        }

        public void UpdateLanguage()
        {
            var language = Localization.Localization.GetLanguageCodeFromName(Commons.Instance.SelectedLanguage);
            foreach (var kvp in hotkeys)
            {
                if (Localization.Localization.Translations[language].TryGetValue(kvp.Key, out var description))
                {
                    kvp.Value.Description = description;
                }
            }
        }
    }
}
