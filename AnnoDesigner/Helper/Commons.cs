using AnnoDesigner.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Abstractions;

namespace AnnoDesigner.Helper;

public class Commons : ICommons
{
    private readonly FileSystem _fileSystem;
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private static string _currentLanguage;

    public event EventHandler SelectedLanguageChanged;

    #region ctor

    private static readonly Lazy<Commons> lazy = new(() => new Commons());

    public static Commons Instance => lazy.Value;
    private Commons()
    {
        _fileSystem = new FileSystem();
    }
    #endregion

    public string CurrentLanguage
    {
        get
        {
            if (_currentLanguage != null && LanguageCodeMap.ContainsKey(_currentLanguage))
            {
                return _currentLanguage;
            }

            _currentLanguage = "English";
            return _currentLanguage;
        }
        set
        {
            _currentLanguage = value ?? "English";
            SelectedLanguageChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public string CurrentLanguageCode => LanguageCodeMap[CurrentLanguage];

    public Dictionary<string, string> LanguageCodeMap => new()
    {
        { "English", "eng" },
        { "Deutsch", "ger" },
        { "Français","fra" },
        { "Polski", "pol" },
        { "Русский", "rus" },
        { "Español", "esp" },

        /* We currently do not support these languages */
        //{ "Italiano", "ita" },
        //{ "český", "cze" },
    };

    public bool CanWriteInFolder(string folderPathToCheck = null)
    {
        bool result = false;

        try
        {
            if (string.IsNullOrWhiteSpace(folderPathToCheck))
            {
                folderPathToCheck = App.ApplicationPath;
            }

            string testFile = _fileSystem.Path.Combine(folderPathToCheck, "test.test");
            _fileSystem.File.WriteAllText(testFile, "test");

            if (_fileSystem.File.Exists(testFile))
            {
                _fileSystem.File.Delete(testFile);
            }

            result = true;
        }
        catch (Exception ex)
        {
            logger.Error(ex, $"Cannot write to folder (\"{folderPathToCheck}\").");
        }

        return result;
    }

    public void RestartApplication(bool asAdmin, string parameters, string path = null)
    {
        string arguments = string.Empty;
        if (!string.IsNullOrWhiteSpace(parameters))
        {
            arguments = parameters;
        }

        arguments = arguments.Trim();

        if (string.IsNullOrWhiteSpace(path))
        {
            path = App.ExecutablePath;
        }

        ProcessStartInfo psi = new()
        {
            FileName = path,
            Arguments = arguments
        };

        if (asAdmin)
        {
            psi.Verb = "runas";
        }

        logger.Trace($"{nameof(RestartApplication)} {nameof(asAdmin)}: {asAdmin}{Environment.NewLine}Path: \"{psi.FileName}\"{Environment.NewLine}Arguments: {psi.Arguments}");

        Process process = new()
        {
            StartInfo = psi
        };
        _ = process.Start();

        Environment.Exit(-1);
        //Application.Current.Shutdown();//sometimes hangs
    }
}
