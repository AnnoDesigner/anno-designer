using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AnnoDesigner.Core.Interop
{
    //From https://stackoverflow.com/a/5826175

    public static class NativeMethods
    {
        #region MapVirtualKey
        public enum MapType : uint
        {
            /// <summary>
            /// uCode is a virtual-key code and is translated into a scan code. If it is a virtual-key code that does
            /// not distinguish between left- and right-hand keys, the left-hand scan code is returned. If there is no
            /// translation, the function returns 0. 
            /// </summary>
            MAPVK_VK_TO_VSC = 0x0,
            /// <summary>
            /// uCode is a scan code and is translated into a virtual-key code that does not distinguish between left-
            /// and right-hand keys. If there is no translation, the function returns 0. 
            /// </summary>
            MAPVK_VSC_TO_VK = 0x1,
            /// <summary>
            /// uCode is a virtual-key code and is translated into an unshifted character value in the low-order word of
            /// the return value. Dead keys (diacritics) are indicated by setting the top bit of the return value. If
            /// there is no translation, the function returns 0. 
            /// </summary>
            MAPVK_VK_TO_CHAR = 0x2,
            /// <summary>
            /// uCode is a scan code and is translated into a virtual-key code that distinguishes between left- and
            /// right-hand keys. If there is no translation, the function returns 0. 
            /// </summary>
            MAPVK_VSC_TO_VK_EX = 0x3,
        }

        /// <summary>
        /// <para>
        /// Translates (maps) a virtual-key code into a scan code or character value, or translates a scan code into a
        /// virtual-key code.
        /// </para>
        /// <para>
        /// Documentation: https://docs.microsoft.com/en-gb/windows/win32/api/winuser/nf-winuser-mapvirtualkeya
        /// </para>
        /// </summary>
        /// <param name="uCode"></param>
        /// <param name="uMapType">A <see cref="MapType"/>.</param>
        /// <returns>The return value is either a scan code, a virtual-key code, or a character value, depending on the
        /// value of uCode and <see cref="MapType"/>. If there is no translation, the return value is zero.</returns>
        [DllImport("user32.dll")]
        public static extern uint MapVirtualKey(uint uCode, MapType uMapType);
        #endregion

        #region ToUnicode
        public enum ToUnicodeReturnValues : int
        {
            /// <summary>
            /// The specified virtual key is a dead-key character (accent or diacritic). This value is returned
            /// regardless of the keyboard layout, even if several characters have been typed and are stored in the
            /// keyboard state. If possible, even with Unicode keyboard layouts, the function has written a spacing
            /// version of the dead-key character to the buffer specified by pwszBuff. For example, the function writes
            /// the character SPACING ACUTE (0x00B4), rather than the character NON_SPACING ACUTE (0x0301). 
            /// </summary>
            DEAD_KEY = -1,
            /// <summary>
            /// The specified virtual key has no translation for the current state of the keyboard. Nothing was written
            /// to the buffer specified by pwszBuff. 
            /// </summary>
            NO_TRANSLATION = 0,
            /// <summary>
            /// One character was written to the buffer specified by pwszBuff. 
            /// </summary>
            SINGLE_CHARACTER = 1,
            /// <summary>
            /// Two or more characters were written to the buffer specified by pwszBuff. The most common cause for this
            /// is that a dead-key character (accent or diacritic) stored in the keyboard layout could not be combined
            /// with the specified virtual key to form a single character. However, the buffer may contain more
            /// characters than the return value specifies. When this happens, any extra characters are invalid and
            /// should be ignored. 
            /// </summary>
            TWO_OR_MORE_CHARACTERS = 2
        }


        /// <summary>
        /// <para>
        /// Translates the specified virtual-key code and keyboard state to the corresponding Unicode character or
        /// characters.
        /// </para>
        /// <para>
        /// Documentation: https://docs.microsoft.com/en-gb/windows/win32/api/winuser/nf-winuser-tounicode
        /// </para>
        /// </summary>
        /// <param name="wVirtKey">The virtual-key code to be translated. See <see
        /// href="https://docs.microsoft.com/en-gb/windows/win32/inputdev/virtual-key-codes">Virtual-Key Codes</see>
        /// </param>
        /// <param name="wScanCode">The hardware scan code of the key to be translated. The high-order bit of this value
        /// is set if the key is up.</param>
        /// <param name="lpKeyState">A pointer to a 256-byte array that contains the current keyboard state. Each
        /// element (byte) in the array contains the state of one key. If the high-order bit of a byte is set, the key
        /// is down.</param>
        /// <param name="pwszBuff">The buffer that receives the translated Unicode character or characters. However,
        /// this buffer may be returned without being null-terminated even though the variable name suggests that it is
        /// null-terminated.</param>
        /// <param name="cchBuff">The size, in characters, of the buffer pointed to by the pwszBuff parameter.</param>
        /// <param name="wFlags">The behavior of the function. If bit 0 is set, a menu is active. If bit 2 is set,
        /// keyboard state is not changed(Windows 10, version 1607 and newer) All other bits(through 31) are
        /// reserved.</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern int ToUnicode(
          uint wVirtKey,
          uint wScanCode,
          byte[] lpKeyState,
          [Out, MarshalAs(UnmanagedType.LPWStr, SizeParamIndex = 4)]
            StringBuilder pwszBuff,
          int cchBuff,
          uint wFlags);
        #endregion

        #region GetKeyboardState
        /// <summary>
        /// <para>
        /// Copies the status of the 256 virtual keys to the specified buffer.
        /// </para>
        /// <para>
        /// Documentation: https://docs.microsoft.com/en-gb/windows/win32/api/winuser/nf-winuser-getkeyboardstate
        /// </para>
        /// </summary>
        /// <param name="lpKeyState">The 256-byte array that receives the status data for each virtual key.</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern bool GetKeyboardState(byte[] lpKeyState);
        #endregion
    }
}
