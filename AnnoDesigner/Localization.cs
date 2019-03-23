using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace AnnoDesigner.Localization
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

    //This static classes cannot be nested as they are used for data binding

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
        static MainWindow()
        {
            File = "File";
            New = "New";
            Open = "Open";
            Save = "save";
            SaveAs = "SaveAs";
            //etc
            //TODO: Add rest
            //TODO: create method to run when lang is changed
        }

        //File Menu
        public static string File { get; set; }
        public static string New { get; set; }
        public static string Open { get; set; }
        public static string Save { get; set; }
        public static string SaveAs { get; set; }
        public static string Exit { get; set; }

        //Extras Menu
        public static string Extras { get; set; }
        public static string Normalize { get; set; }
        public static string ResetZoom { get; set; }
        public static string RegisterFileExtension { get; set; }
        public static string UnregisterFileExtension { get; set; }

        //Export Menu
        public static string Export { get; set; }
        public static string ExportImage { get; set; }
        public static string UseCurrentZoomOnExportedImage { get; set; }
        public static string RenderSelectionHighlightsOnExportedImage { get; set; }

        //Help Menu
        public static string Help { get; set; }
        public static string Version { get; set; }
        public static string FileVersion { get; set; }
        public static string PresetsVersion { get; set; }
        public static string CheckForUpdates { get; set; }
        public static string GoToProjectHomepage { get; set; }
        public static string AboutAnnoDesigner { get; set; }

        //Other
        public static string ShowGrid { get; set; }
        public static string ShowLabel { get; set; }
        public static string ShowIcon { get; set; }
        public static string ShowStat { get; set; }

        //DockPanel
        public static string BuildingSettings { get; set; }
        public static string Size { get; set; }
        public static string Color { get; set; }
        public static string Label { get; set; }
        public static string Icon { get; set; }
        public static string Radius { get; set; }
        public static string Options { get; set; }
        public static string EnableLabel { get; set; }
        public static string Borderless { get; set; }
        public static string Road { get; set; }
        public static string PlaceBuilding { get; set; }


    }

    public static class Welcome
    {

    }
}


