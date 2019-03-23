using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace AnnoDesigner.UI
{
    public static class Localization
    {
        public static readonly Dictionary<string, string> LanguageCodeMap = new Dictionary<string, string>()
        {
            { "English", "eng" },
            { "Deutsch", "ger" },
            { "Français","fra" },
            { "Español", "esp" },
            { "Italiano", "ita" },
            { "Polski", "pol" },
            { "Русский", "rus" },
            { "český", "cze" },
        };

        public static string GetLanguageCodeFromName(string s)
        {
            return LanguageCodeMap[s];
        }
    }

    public static class About
    {
        public static string Title;
    }

    //Probably nothing to add in here
    public static class AnnoCanvas
    {

    }

    //Probably nothing to add in here
    public static class App
    {

    }

    public static class MainWindow
    {

    }

    public static class Welcome
    {

    }
}


