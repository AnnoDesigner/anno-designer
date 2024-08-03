using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AnnoDesigner.Core.Interop;

namespace AnnoDesigner.Core.Helper;

//Modified from https://stackoverflow.com/a/5826175

/// <summary>
/// Used to map a <see cref="Key"/> to the OEM or OS specific unicode character that it produces.
/// </summary>
public static class KeyboardInteropHelper
{
    /// <summary>
    /// Returns the character represented by the <see cref="Key"/> given. This could be non-visible characters such
    /// as control keys. To get a display string that handles these, such as "Space" for <see cref="Key.Space"/>, or "Tab" for 
    /// <see cref="Key.Tab"/>, use the <see cref="GetDisplayString(Key)"/> method instead.
    /// <para>
    /// If <paramref name="useKeyboardState"/> is set to <see langword="true"/>, then the character retrieved is affected by modifier keys. 
    /// For example, <see cref="Key.L"/> could return either "L" or "l", <see cref="Key.OemSemicolon"/> could return ";" or ":" 
    /// depending on if the shift key is pressed.
    /// </para>
    /// <para>
    /// If the character can not be translated, this method returns null.
    /// </para>
    /// </summary>
    /// <param name="key">The key to retrieve the character for.</param>
    /// <param name="useKeyboardState">Use the current keyboard state when retrieving the character.</param>
    /// <returns></returns>
    public static char? GetCharacter(Key key, bool useKeyboardState = false)
    {
        char? c = null;

        var virtualKey = KeyInterop.VirtualKeyFromKey(key);
        var keyboardState = new byte[256];
        if (useKeyboardState)
        {
            NativeMethods.GetKeyboardState(keyboardState);
        }

        var scanCode = NativeMethods.MapVirtualKey((uint)virtualKey, NativeMethods.MapType.MAPVK_VK_TO_VSC);
        var stringBuilder = new StringBuilder(2);

        var result = (NativeMethods.ToUnicodeReturnValues)NativeMethods.ToUnicode((uint)virtualKey, scanCode, keyboardState, stringBuilder, stringBuilder.Capacity, 0);
        switch (result)
        {
            case NativeMethods.ToUnicodeReturnValues.DEAD_KEY:
            case NativeMethods.ToUnicodeReturnValues.NO_TRANSLATION:
                break;
            case NativeMethods.ToUnicodeReturnValues.SINGLE_CHARACTER:
                c = stringBuilder[0];
                break;
            case NativeMethods.ToUnicodeReturnValues.TWO_OR_MORE_CHARACTERS:
                c = stringBuilder[0];
                break;
            default:
                break;
        }
        return c;
    }

    /// <summary>
    /// Returns the a display string for the given <paramref name="key"/>. For example, <see cref="Key.A"/> returns "A", 
    /// <see cref="Key.LeftAlt"/> returns "Alt" and <see cref="Key.Space"/> returns "Space". This method returns
    /// <paramref name="key"/>.ToString() if the key could not be translated. For example, <see cref="Key.Delete"/> has no 
    /// valid translation and would return "Delete".
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string GetDisplayString(Key key)
    {
        //Special handling for numpad keys.
        if (IsNumPadKey(key))
        {
            return GetFriendlyDisplayString(key);
        }
        var c = GetCharacter(key, false);
        if (c.HasValue)
        {
            if (IsVisibleCharacter(c.Value))
            {
                var str = char.ToUpper(c.Value).ToString();
                if (!string.IsNullOrWhiteSpace(str))
                {
                    return str;
                }
            }
            return GetFriendlyDisplayString(key);
        }
        else
        {
            return key.ToString();
        }
    }

    private static bool IsVisibleCharacter(char c)
    {
        return !(char.IsWhiteSpace(c) || char.IsControl(c));
    }

    /// <summary>
    /// Returns the friendly display string for a <see cref="Key"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    private static string GetFriendlyDisplayString(Key key)
    {
        return key switch
        {
            //Also defined as Key.Return. Return Enter explicitly
            Key.Enter => "Enter",
            //Also defined as Key.Capital. Return CapsLock explicitly
            Key.CapsLock => "CapsLock",
            //Also defined as Key.Prior. Return PageUp explicitly
            Key.PageUp => "PageUp",
            //Also defined as Key.Next. Return PageDown explicitly.
            Key.PageDown => "PageDown",
            //Also defined as Key.Snapshot. Return PrintScreen explicitly
            Key.PrintScreen => "PrintScreen",
            Key.LWin or Key.RWin => "Windows",
            Key.LeftShift or Key.RightShift => "Shift",
            Key.LeftCtrl or Key.RightCtrl => "Ctrl",
            Key.LeftAlt or Key.RightAlt => "Alt",
            Key.Decimal => "NumPadDecimal",
            Key.Subtract => "NumPadSubtract",
            Key.Multiply => "NumPadMultiply",
            Key.Divide => "NumPadDivide",
            _ => key.ToString(),
        };
    }

    /// <summary>
    /// Special handling for numpad keys, as these produce a number when pressed (a valid translation), but still have a unique key code,
    /// (Numpad0 - 9).
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    private static bool IsNumPadKey(Key key)
    {
        return key switch
        {
            Key.NumPad0 or Key.NumPad1 or Key.NumPad2 or Key.NumPad3 or Key.NumPad4 or Key.NumPad5 or Key.NumPad6 or Key.NumPad7 or Key.NumPad8 or Key.NumPad9 or Key.Decimal or Key.Subtract or Key.Multiply or Key.Divide => true,
            _ => false,
        };
    }
}
