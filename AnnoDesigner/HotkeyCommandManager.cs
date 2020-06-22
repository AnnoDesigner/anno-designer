using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
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

        /// <summary>
        /// Stores hotkey information loaded from user settings. Use <see cref="EnsureMappedHotkeys"/>
        /// </summary>
        private IDictionary<string, HotkeyInformation> hotkeyUserMappings;

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
        public void AddHotkey(string hotkeyId, InputBinding binding)
        {
            AddHotkey(new Hotkey(hotkeyId, binding));
        }

        /// <summary>
        /// Registers a <see cref="Hotkey"/> with the hotkey manager.
        /// </summary>
        /// <param name="hotkey"></param>
        public void AddHotkey(Hotkey hotkey)
        {
            if (!hotkeys.ContainsKey(hotkey.HotkeyId))
            {
                hotkey.PropertyChanged += Hotkey_PropertyChanged;
                hotkeys.Add(hotkey.HotkeyId, hotkey);
                _observableCollection.Add(hotkey);
                //Check for localization
                hotkey.Description = hotkey.HotkeyId;
                var language = Localization.Localization.GetLanguageCodeFromName(Commons.Instance.SelectedLanguage);
                if (Localization.Localization.Translations.TryGetValue(hotkey.HotkeyId, out var description))
                {
                    hotkey.Description = description;
                }
                CheckHotkeyUserMappings();
            }
            else
            {
                throw new ArgumentException($"Key {hotkey.HotkeyId} already exists in collection.", "hotkey");
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
            foreach (var kvp in hotkeys)
            {
                if (Localization.Localization.Translations.TryGetValue(kvp.Key, out var description))
                {
                    kvp.Value.Description = description;
                }
            }
        }

        /// <summary>
        /// Retrieves a <see cref="Dictionary{string, HotkeyInformation}"/> of Hotkeys that have been remapped from their defaults.
        /// Hotkeys that have not been changed are ignored.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, HotkeyInformation> GetRemappedHotkeys()
        {
            var remapped = new Dictionary<string, HotkeyInformation>();
            foreach (var h in hotkeys.Values)
            {
                if (h.IsRemapped())
                {
                    remapped.Add(h.HotkeyId, h.GetHotkeyInformation());
                }
            }
            return remapped;
        }

        /// <summary>
        /// Loads the given hotkey mappings. The hotkey mappings are lazily applied so that hotkeys registered after this
        /// method is called can still be updated with the information provided. See the remarks section for more info.
        /// </summary>
        /// <remarks>
        /// As we don't know when in the lifetime of the HotkeyCommandManager that this method will be called, the mappings 
        /// must be "lazily" loaded - this means that hotkeys can be registered with the manager after this method has been
        /// called, and the mappings can still be loaded. This is done by checking against the hotkeyId property - if we find
        /// a match, we update the hotkey with the new information.
        ///
        /// We can run the matching process over all existing hotkeys when this method is initially called, to update the
        /// mappings for hotkeys that are already registered.
        /// </remarks>
        public void LoadHotkeyMappings(IDictionary<string, HotkeyInformation> mappings)
        {
            if (mappings is null || mappings.Count == 0)
            {
                return;
            }
            hotkeyUserMappings = mappings;
            CheckHotkeyUserMappings();
        }

        /// <summary>
        /// Attempts to map hotkey information loaded from user settings to existing hotkeys set with defaults.
        /// A value is removed from the <see cref="hotkeyUserMappings"/> dictionary once it is mapped.
        /// </summary>
        private void CheckHotkeyUserMappings()
        {
            if (hotkeyUserMappings is null || hotkeyUserMappings.Count == 0)
            {
                return;
            }
            foreach (var kvp in hotkeyUserMappings.ToDictionary(_ => _.Key, _ => _.Value)) //Copy so that we can modify the original collection
            {
                if (hotkeys.TryGetValue(kvp.Key, out var hotkey))
                {
                    hotkeyUserMappings.Remove(kvp.Key);
                    hotkey.UpdateHotkey(kvp.Value);
                }
            }
        }

    }
}
