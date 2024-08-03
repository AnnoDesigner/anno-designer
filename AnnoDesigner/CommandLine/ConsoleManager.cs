using System;
using System.CommandLine;
using System.CommandLine.IO;
using System.Runtime.InteropServices;
using System.Security;

namespace AnnoDesigner.CommandLine;

/// <summary>
/// Console manager class for showing console when help text should be shown.
/// </summary>
/// <remarks>
/// Source: https://stackoverflow.com/a/718505
/// </remarks>
[SuppressUnmanagedCodeSecurity]
public static class ConsoleManager
{
    public class LazyConsole : IConsole
    {
        private readonly SystemConsole systemConsole = new();

        public IStandardStreamWriter Out
        {
            get
            {
                Show();
                return systemConsole.Out;
            }
        }

        public bool IsOutputRedirected
        {
            get
            {
                Show();
                return systemConsole.IsOutputRedirected;
            }
        }

        public IStandardStreamWriter Error
        {
            get
            {
                Show();
                return systemConsole.Error;
            }
        }

        public bool IsErrorRedirected
        {
            get
            {
                Show();
                return systemConsole.IsErrorRedirected;
            }
        }

        public bool IsInputRedirected
        {
            get
            {
                Show();
                return systemConsole.IsInputRedirected;
            }
        }
    }

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

    public static bool HasConsole => GetConsoleWindow() != IntPtr.Zero;

    public static bool StartedWithoutConsole { get; private set; }

    /// <summary>
    /// Creates a new console instance if the process is not attached to a console already.
    /// </summary>
    public static void Show()
    {
        if (HasConsole)
        {
            return;
        }

        if (!AttachConsole(ATTACH_PARENT_PROCESS))
        {
            _ = AllocConsole();

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
            _ = FreeConsole();
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
