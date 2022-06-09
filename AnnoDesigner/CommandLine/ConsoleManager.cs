using System;
using System.Runtime.InteropServices;
using System.Security;

namespace AnnoDesigner.CommandLine
{
    /// <summary>
    /// Console manager class for showing console when help text should be shown.
    /// </summary>
    /// <remarks>
    /// Source: https://stackoverflow.com/a/718505
    /// </remarks>
    [SuppressUnmanagedCodeSecurity]
    public static class ConsoleManager
    {
        private const string Kernel32_DllName = "kernel32.dll";

        [DllImport(Kernel32_DllName)]
        private static extern bool AllocConsole();

        [DllImport(Kernel32_DllName)]
        private static extern bool FreeConsole();

        [DllImport(Kernel32_DllName)]
        private static extern IntPtr GetConsoleWindow();

        [DllImport(Kernel32_DllName)]
        private static extern bool AttachConsole(int dwProcessId);
        private const int ATTACH_PARENT_PROCESS = -1;

        public static bool HasConsole
        {
            get { return GetConsoleWindow() != IntPtr.Zero; }
        }

        public static bool StartedWithoutConsole { get; private set; }

        /// <summary>
        /// Creates a new console instance if the process is not attached to a console already.
        /// </summary>
        public static void Show()
        {
            if (!AttachConsole(ATTACH_PARENT_PROCESS))
            {
                AllocConsole();

                StartedWithoutConsole = true;
            }
            else
            {
                StartedWithoutConsole = false;
            }
        }

        /// <summary>
        /// If the process has a console attached to it, it will be detached and no longer visible. Writing to the System.Console is still possible, but no output will be shown.
        /// </summary>
        public static void Hide()
        {
            if (HasConsole)
            {
                FreeConsole();
            }
        }

        public static void Toggle()
        {
            if (HasConsole)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }
    }
}
