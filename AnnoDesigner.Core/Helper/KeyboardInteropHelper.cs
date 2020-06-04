using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AnnoDesigner.Core.Interop;

namespace AnnoDesigner.Core.Helper
{
    //Modified from https://stackoverflow.com/a/5826175

    /// <summary>
    /// Used to map a <see cref="Key"/> to the OEM or OS specific unicode character that it produces.
    /// </summary>
    public static class KeyboardInteropHelper
    {
        /// <summary>
        /// Returns the character represented by the <see cref="Key"/> given. This could be non-visible characters such
        /// as control keys. To get a display string, for example "Space" for <see cref="Key.Space"/>, use the 
        /// <see cref="GetDisplayString(Key)"/> method instead.
        /// 
        /// <para>
        /// If <paramref name="useKeyboardState"/> is set to <see langword="true"/>, then the character retrieved is affected by modifier keys. For example, <see cref="Key.L"/> could return either "L" or "l",
        /// <see cref="Key.OemSemicolon"/> could return ";" or ":" depending on if the shift key is pressed.
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
#if DEBUG
                case NativeMethods.ToUnicodeReturnValues.SINGLE_CHARACTER:
                    c = stringBuilder[0];
                    break;
                case NativeMethods.ToUnicodeReturnValues.TWO_OR_MORE_CHARACTERS:
                    var c1 = stringBuilder[0];
                    var c2 = stringBuilder[1];
                    Debugger.Break(); //See what this might look like.
                    c = stringBuilder[0];
                    break;
#else
                case NativeMethods.ToUnicodeReturnValues.SINGLE_CHARACTER:
                case NativeMethods.ToUnicodeReturnValues.TWO_OR_MORE_CHARACTERS:
                    c = stringBuilder[0];
#endif
                default:
                    break;
            }
            return c;
        }

        /// <summary>
        /// Returns the a display string for the given <paramref name="key"/>. For example, <see cref="Key.A"/> returns "A", 
        /// <see cref="Key.LeftAlt"/> returns "Alt" and <see cref="Key.Space"/> returns "Space". This method returns
        /// <see langword="null"/> if the given <see cref="Key"/> could not be translated.
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
                if (IsVisible(c.Value))
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
                return null;
            }
        }

        private static bool IsVisible(char c)
        {
            return !(char.IsWhiteSpace(c) || char.IsControl(c));
        }

        /// <summary>
        /// Returns the friendly display string for a <see cref="Key"/>.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private static string GetFriendlyDisplayString(Key key)
        {
            switch (key)
            {
                //Also defined as Key.Return. Return Enter explicitly
                case Key.Enter:
                    return "Enter";
                //Also defined as Key.Capital. Return CapsLock explicitly
                case Key.CapsLock:
                    return "CapsLock";
                //Also defined as Key.Prior. Return PageUp explicitly
                case Key.PageUp:
                    return "PageUp";
                //Also defined as Key.Next. Return PageDown explicitly.
                case Key.PageDown:
                    return "PageDown";
                //Also defined as Key.Snapshot. Return PrintScreen explicitly
                case Key.PrintScreen:
                    return "PrintScreen";
                case Key.LWin:
                case Key.RWin:
                    return "Windows";
                case Key.LeftShift:
                case Key.RightShift:
                    return "Shift";
                case Key.LeftCtrl:
                case Key.RightCtrl:
                    return "Ctrl";
                case Key.LeftAlt:
                case Key.RightAlt:
                    return "Alt";
                case Key.Decimal:
                    return "NumPadDecimal";
                case Key.Subtract:
                    return "NumPadSubtract";
                case Key.Multiply:
                    return "NumPadMultiply";
                case Key.Divide:
                    return "NumPadDivide";
                default:
                    return key.ToString();
            }
        }

        /// <summary>
        /// Special handling for numpad keys, as these produce a number when pressed, but still have a unique key code.
        /// (Numpad0 - 9)
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private static bool IsNumPadKey(Key key)
        {
            switch (key)
            {
                case Key.NumPad0:
                case Key.NumPad1:
                case Key.NumPad2:
                case Key.NumPad3:
                case Key.NumPad4:
                case Key.NumPad5:
                case Key.NumPad6:
                case Key.NumPad7:
                case Key.NumPad8:
                case Key.NumPad9:
                case Key.Decimal:
                case Key.Subtract:
                case Key.Multiply:
                case Key.Divide:
                    return true;
                default: return false;
            }
        }
    }
}
