using AnnoDesigner.Core.Models;
using System;
using System.Windows.Input;

namespace AnnoDesigner.Models;

/// <summary>
/// Acts as a wrapper for a named <see cref="InputBinding"/>, and allows updating if the currently wrapped <see cref="InputBinding"/> 
/// is replaced with a fresh reference, whilst still maintaining the same <see cref="HotkeyId"/>.
/// </summary>
public class Hotkey : Notify
{
    private Hotkey() { }
    public Hotkey(string hotkeyId, InputBinding binding) : this(hotkeyId, binding, null, null) { }
    public Hotkey(string hotkeyId, InputBinding binding, string localizationKey) : this(hotkeyId, binding, localizationKey, null) { }
    public Hotkey(string hotkeyId, InputBinding binding, string localizationKey, string description)
    {
        HotkeyId = hotkeyId;
        Binding = binding;
        LocalizationKey = localizationKey;
        Description = description;

        if (binding.Gesture is PolyGesture gesture)
        {
            defaultKey = gesture.Key;
            defaultModifiers = gesture.ModifierKeys;
            defaultMouseAction = gesture.MouseAction;
            defaultType = gesture.Type;
        }
        else
        {
            throw new ArgumentException($"{nameof(binding)} must use a {nameof(PolyGesture)}");
        }
    }

    private InputBinding _binding;
    public InputBinding Binding
    {
        get => _binding;
        set
        {
            _ = UpdateProperty(ref _binding, value);
            //Check that a PolyGesture is still being used
            _ = GetGestureOrThrow();
        }
    }

    private string _name;
    /// <summary>
    /// An identifier for the <see cref="Hotkey"/>, usually required to be unique.
    /// </summary>
    public string HotkeyId
    {
        get => _name;
        set => UpdateProperty(ref _name, value);
    }

    private string _description;
    public string Description
    {
        get => _description;
        set => UpdateProperty(ref _description, value);
    }

    private string _localizationKey;
    public string LocalizationKey
    {
        get => _localizationKey;
        set => UpdateProperty(ref _localizationKey, value);
    }

    /// <summary>
    /// Resets a hotkey to its defaults.
    /// </summary>
    public void Reset()
    {
        SynchronizeProperties(defaultKey, defaultMouseAction, defaultModifiers, defaultType);
    }

    private void SynchronizeProperties(Key key, ExtendedMouseAction mouseAction, ModifierKeys modifiers, GestureType type)
    {
        PolyGesture gesture = GetGestureOrThrow();

        gesture.Type = type;
        gesture.Key = key;
        gesture.MouseAction = mouseAction;
        gesture.ModifierKeys = modifiers;
        OnPropertyChanged(nameof(Binding.Gesture));
    }

    /// <summary>
    /// Returns <see langword="true"/> if the current mappings for this <see cref="Hotkey"/> 
    /// do not match the default mappings it was created with.
    /// </summary>
    /// <returns></returns>
    public bool IsRemapped()
    {
        PolyGesture gesture = GetGestureOrThrow();
        return !(gesture.Type == defaultType && gesture.Key == defaultKey && gesture.ModifierKeys == defaultModifiers && gesture.MouseAction == defaultMouseAction);
    }

    public HotkeyInformation GetHotkeyInformation()
    {
        PolyGesture gesture = GetGestureOrThrow();
        return new HotkeyInformation()
        {
            Key = gesture.Key,
            Modifiers = gesture.ModifierKeys,
            MouseAction = gesture.MouseAction,
            Type = gesture.Type
        };
    }

    /// <summary>
    /// Updates a hotkey and based on the given HotkeyInformation
    /// </summary>
    /// <param name="information"></param>
    public void UpdateHotkey(HotkeyInformation information)
    {
        UpdateHotkey(information.Key, information.MouseAction, information.Modifiers, information.Type);
    }

    /// <summary>
    /// Updates a hotkey and based on the given information
    /// </summary>
    /// <param name="key"></param>
    /// <param name="modifiers"></param>
    /// <param name="mouseAction"></param>
    /// <param name="type"></param>
    public void UpdateHotkey(Key key, ExtendedMouseAction mouseAction, ModifierKeys modifiers, GestureType type)
    {
        if (PolyGesture.IsDefinedGestureType(type))
        {
            SynchronizeProperties(key, mouseAction, modifiers, type);
        }
        else
        {
            throw new ArgumentException($"Value provided is not valid for enum {nameof(GestureType)}", nameof(type));
        }
    }

    /// <summary>
    /// Checks that the Hotkey is using a PolyGesture
    /// </summary>
    /// <returns></returns>
    private PolyGesture GetGestureOrThrow()
    {
        return Binding.Gesture as PolyGesture ?? throw new InvalidOperationException($"{nameof(Hotkey)} must use a {nameof(PolyGesture)}");
    }

    private readonly Key defaultKey = default;
    private readonly ExtendedMouseAction defaultMouseAction = default;
    private readonly ModifierKeys defaultModifiers = default;
    private readonly GestureType defaultType = default;
}
