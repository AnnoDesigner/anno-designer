using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace AnnoDesigner.Localization
{
    public static class Localization
    {
        static Localization()
        {
            //This is auto-generated from:
            //https://docs.google.com/spreadsheets/d/1CjECty43mkkm1waO4yhQl1rzZ-ZltrBgj00aq-WJX4w/edit?usp=sharing 
            Translations = new Dictionary<string, Dictionary<string, string>>()
{
{
"English", new Dictionary<string, string>() {
{ "File" , "File" },
{ "NewCanvas" , "New Canvas" },
{ "Open" , "Open" },
{ "Save" , "Save" },
{ "SaveAs" , "Save As" },
{ "Exit" , "Exit" },
{ "Extras" , "Extras" },
{ "Normalize" , "Normalize" },
{ "ResetZoom" , "Reset Zoom" },
{ "RegisterFileExtension" , "Register File Extension" },
{ "UnregisterFileExtension" , "Unregister File Extension" },
{ "Export" , "Export" },
{ "ExportImage" , "Export Image" },
{ "UseCurrentZoomOnExportedImage" , "Use current zoom on exported image" },
{ "RenderSelectionHighlightsOnExportedImage" , "Render selection highlights on exported image" },
{ "Help" , "Help" },
{ "Version" , "Version" },
{ "FileVersion" , "File Version" },
{ "PresetsVersion" , "Presets Version" },
{ "CheckForUpdates" , "Check For Updates" },
{ "GoToProjectHomepage" , "Go to Project Homepage" },
{ "AboutAnnoDesigner" , "About Anno Designer" },
{ "ShowGrid" , "Show Grid" },
{ "ShowLabels" , "Show Labels" },
{ "ShowIcon" , "Show Icon" },
{ "ShowStats" , "Show Stats" },
{ "BuildingSettings" , "Building Settings" },
{ "Size" , "Size" },
{ "Color" , "Color" },
{ "Label" , "Label" },
{ "Icon" , "Icon" },
{ "Radius" , "Radius" },
{ "Options" , "Options" },
{ "EnableLabel" , "Enable label" },
{ "Borderless" , "Borderless" },
{ "Road" , "Road" },
{ "PlaceBuilding" , "Place building" },
{ "TitleAbout" , "About" },
{ "Title" , "Modified" },
{ "ModifiedAnnoDesigner" , "Modified" },
{ "BuildingLayoutDesigner" , "A building layout designer for Ubisofts Anno-series" },
{ "Credits" , "Credits" },
{ "OriginalApplicationBy" , "Original application by [name]" },
{ "BuildingPresets" , "Building presets" },
{ "CombinedForAnnoVersions" , "Combined building presets for" },
{ "AdditionalChanges" , "Additional changes by" },
{ "ManyThanks" , "Many thanks to all the users who contributed to this project!" },
{ "VisitTheFandom" , "Be sure to visit the Fandom pages for Anno!" },
{ "OriginalHomepage" , "Original Homepage" },
{ "ProjectHomepage" , "Project Homepage" },
{ "GoToFandom" , "Go to Fandom" },
{ "Close" , "Close" }
}
},
{
"German", new Dictionary<string, string>() {
{ "File" , "Neu" },
{ "NewCanvas" , "Datei" },
{ "Open" , "Öffnen" },
{ "Save" , "Speichern" },
{ "SaveAs" , "Speichern unter" },
{ "Exit" , "Beenden" },
{ "Extras" , "Extras" },
{ "Normalize" , "normalisieren/normieren" },
{ "ResetZoom" , "Zoom zurücksetzen" },
{ "RegisterFileExtension" , "Register File Extension" },
{ "UnregisterFileExtension" , "Unregister File Extension" },
{ "Export" , "Exportiere" },
{ "ExportImage" , "Exportiere Bild / Speichere als Bild" },
{ "UseCurrentZoomOnExportedImage" , "Exportiere Aktuellen Zoom als Bild" },
{ "RenderSelectionHighlightsOnExportedImage" , "Render selection highlights on exported image" },
{ "Help" , "Hilfe" },
{ "Version" , "Version" },
{ "FileVersion" , "Dateiversion" },
{ "PresetsVersion" , "Vergegebene Versionen" },
{ "CheckForUpdates" , "Auf Updates prüfen" },
{ "GoToProjectHomepage" , "gehe zu Projekt Startseite" },
{ "AboutAnnoDesigner" , "über Anno Designer" },
{ "ShowGrid" , "Raster/Gitter (an)zeigen" },
{ "ShowLabels" , "Bezeichnungen (an)zeigen" },
{ "ShowIcon" , "Symbol (an)zeigen" },
{ "ShowStats" , "Statistiken (an)zeigen" },
{ "BuildingSettings" , "Gebäude Optionen" },
{ "Size" , "Größe" },
{ "Color" , "Farbe" },
{ "Label" , "Bezeichnung" },
{ "Icon" , "Zeichen/Icon" },
{ "Radius" , "Radius" },
{ "Options" , "Optionen" },
{ "EnableLabel" , "Bezeichnung aktivieren" },
{ "Borderless" , "randlos" },
{ "Road" , "Straße" },
{ "PlaceBuilding" , "Gebäude platzieren" },
{ "TitleAbout" , "über" },
{ "Title" , "Modified" },
{ "ModifiedAnnoDesigner" , "bearbeitet/geändert" },
{ "BuildingLayoutDesigner" , "Ein Gebäude-layout Designer für Ubisofts Anno Reihe" },
{ "Credits" , "Credits" },
{ "OriginalApplicationBy" , "ursprüngliche Anwendung von [name]" },
{ "BuildingPresets" , "Gebäude Vorlagen" },
{ "CombinedForAnnoVersions" , "Gebäude Vorlagen zusammenfügen zu" },
{ "AdditionalChanges" , "zusätzliche Änderungen von" },
{ "ManyThanks" , "vielen Dank an alle (Leute) die an diesem Projekt mitgeholfen haben" },
{ "VisitTheFandom" , "Schau dir mal die Anno Fanpages an!" },
{ "OriginalHomepage" , "Original Startseite" },
{ "ProjectHomepage" , "Projekt Startsteite" },
{ "GoToFandom" , "zu Fandom gehen/Fandom besuchen" },
{ "Close" , "schließen" }
}
},
{
"French", new Dictionary<string, string>() {
{ "File" , "File" },
{ "NewCanvas" , "New Canvas" },
{ "Open" , "Open" },
{ "Save" , "Save" },
{ "SaveAs" , "Save As" },
{ "Exit" , "Exit" },
{ "Extras" , "Extras" },
{ "Normalize" , "Normalize" },
{ "ResetZoom" , "Reset Zoom" },
{ "RegisterFileExtension" , "Register File Extension" },
{ "UnregisterFileExtension" , "Unregister File Extension" },
{ "Export" , "Export" },
{ "ExportImage" , "Export Image" },
{ "UseCurrentZoomOnExportedImage" , "Use current zoom on exported image" },
{ "RenderSelectionHighlightsOnExportedImage" , "Render selection highlights on exported image" },
{ "Help" , "Help" },
{ "Version" , "Version" },
{ "FileVersion" , "File Version" },
{ "PresetsVersion" , "Presets Version" },
{ "CheckForUpdates" , "Check For Updates" },
{ "GoToProjectHomepage" , "Go to Project Homepage" },
{ "AboutAnnoDesigner" , "About Anno Designer" },
{ "ShowGrid" , "Show Grid" },
{ "ShowLabels" , "Show Labels" },
{ "ShowIcon" , "Show Icon" },
{ "ShowStats" , "Show Stats" },
{ "BuildingSettings" , "Building Settings" },
{ "Size" , "Size" },
{ "Color" , "Color" },
{ "Label" , "Label" },
{ "Icon" , "Icon" },
{ "Radius" , "Radius" },
{ "Options" , "Options" },
{ "EnableLabel" , "Enable label" },
{ "Borderless" , "Borderless" },
{ "Road" , "Road" },
{ "PlaceBuilding" , "Place building" },
{ "TitleAbout" , "About" },
{ "Title" , "Modified" },
{ "ModifiedAnnoDesigner" , "Modified" },
{ "BuildingLayoutDesigner" , "A building layout designer for Ubisofts Anno-series" },
{ "Credits" , "Credits" },
{ "OriginalApplicationBy" , "Original application by [name]" },
{ "BuildingPresets" , "Building presets" },
{ "CombinedForAnnoVersions" , "Combined building presets for" },
{ "AdditionalChanges" , "Additional changes by" },
{ "ManyThanks" , "Many thanks to all the users who contributed to this project!" },
{ "VisitTheFandom" , "Be sure to visit the Fandom pages for Anno!" },
{ "OriginalHomepage" , "Original Homepage" },
{ "ProjectHomepage" , "Project Homepage" },
{ "GoToFandom" , "Go to Fandom" },
{ "Close" , "Close" }
}
},
{
"Spanish", new Dictionary<string, string>() {
{ "File" , "File" },
{ "NewCanvas" , "New Canvas" },
{ "Open" , "Open" },
{ "Save" , "Save" },
{ "SaveAs" , "Save As" },
{ "Exit" , "Exit" },
{ "Extras" , "Extras" },
{ "Normalize" , "Normalize" },
{ "ResetZoom" , "Reset Zoom" },
{ "RegisterFileExtension" , "Register File Extension" },
{ "UnregisterFileExtension" , "Unregister File Extension" },
{ "Export" , "Export" },
{ "ExportImage" , "Export Image" },
{ "UseCurrentZoomOnExportedImage" , "Use current zoom on exported image" },
{ "RenderSelectionHighlightsOnExportedImage" , "Render selection highlights on exported image" },
{ "Help" , "Help" },
{ "Version" , "Version" },
{ "FileVersion" , "File Version" },
{ "PresetsVersion" , "Presets Version" },
{ "CheckForUpdates" , "Check For Updates" },
{ "GoToProjectHomepage" , "Go to Project Homepage" },
{ "AboutAnnoDesigner" , "About Anno Designer" },
{ "ShowGrid" , "Show Grid" },
{ "ShowLabels" , "Show Labels" },
{ "ShowIcon" , "Show Icon" },
{ "ShowStats" , "Show Stats" },
{ "BuildingSettings" , "Building Settings" },
{ "Size" , "Size" },
{ "Color" , "Color" },
{ "Label" , "Label" },
{ "Icon" , "Icon" },
{ "Radius" , "Radius" },
{ "Options" , "Options" },
{ "EnableLabel" , "Enable label" },
{ "Borderless" , "Borderless" },
{ "Road" , "Road" },
{ "PlaceBuilding" , "Place building" },
{ "TitleAbout" , "About" },
{ "Title" , "Modified" },
{ "ModifiedAnnoDesigner" , "Modified" },
{ "BuildingLayoutDesigner" , "A building layout designer for Ubisofts Anno-series" },
{ "Credits" , "Credits" },
{ "OriginalApplicationBy" , "Original application by [name]" },
{ "BuildingPresets" , "Building presets" },
{ "CombinedForAnnoVersions" , "Combined building presets for" },
{ "AdditionalChanges" , "Additional changes by" },
{ "ManyThanks" , "Many thanks to all the users who contributed to this project!" },
{ "VisitTheFandom" , "Be sure to visit the Fandom pages for Anno!" },
{ "OriginalHomepage" , "Original Homepage" },
{ "ProjectHomepage" , "Project Homepage" },
{ "GoToFandom" , "Go to Fandom" },
{ "Close" , "Close" }
}
},
{
"Italian", new Dictionary<string, string>() {
{ "File" , "File" },
{ "NewCanvas" , "New Canvas" },
{ "Open" , "Open" },
{ "Save" , "Save" },
{ "SaveAs" , "Save As" },
{ "Exit" , "Exit" },
{ "Extras" , "Extras" },
{ "Normalize" , "Normalize" },
{ "ResetZoom" , "Reset Zoom" },
{ "RegisterFileExtension" , "Register File Extension" },
{ "UnregisterFileExtension" , "Unregister File Extension" },
{ "Export" , "Export" },
{ "ExportImage" , "Export Image" },
{ "UseCurrentZoomOnExportedImage" , "Use current zoom on exported image" },
{ "RenderSelectionHighlightsOnExportedImage" , "Render selection highlights on exported image" },
{ "Help" , "Help" },
{ "Version" , "Version" },
{ "FileVersion" , "File Version" },
{ "PresetsVersion" , "Presets Version" },
{ "CheckForUpdates" , "Check For Updates" },
{ "GoToProjectHomepage" , "Go to Project Homepage" },
{ "AboutAnnoDesigner" , "About Anno Designer" },
{ "ShowGrid" , "Show Grid" },
{ "ShowLabels" , "Show Labels" },
{ "ShowIcon" , "Show Icon" },
{ "ShowStats" , "Show Stats" },
{ "BuildingSettings" , "Building Settings" },
{ "Size" , "Size" },
{ "Color" , "Color" },
{ "Label" , "Label" },
{ "Icon" , "Icon" },
{ "Radius" , "Radius" },
{ "Options" , "Options" },
{ "EnableLabel" , "Enable label" },
{ "Borderless" , "Borderless" },
{ "Road" , "Road" },
{ "PlaceBuilding" , "Place building" },
{ "TitleAbout" , "About" },
{ "Title" , "Modified" },
{ "ModifiedAnnoDesigner" , "Modified" },
{ "BuildingLayoutDesigner" , "A building layout designer for Ubisofts Anno-series" },
{ "Credits" , "Credits" },
{ "OriginalApplicationBy" , "Original application by [name]" },
{ "BuildingPresets" , "Building presets" },
{ "CombinedForAnnoVersions" , "Combined building presets for" },
{ "AdditionalChanges" , "Additional changes by" },
{ "ManyThanks" , "Many thanks to all the users who contributed to this project!" },
{ "VisitTheFandom" , "Be sure to visit the Fandom pages for Anno!" },
{ "OriginalHomepage" , "Original Homepage" },
{ "ProjectHomepage" , "Project Homepage" },
{ "GoToFandom" , "Go to Fandom" },
{ "Close" , "Close" }
}
},
{
"Polish", new Dictionary<string, string>() {
{ "File" , "File" },
{ "NewCanvas" , "New Canvas" },
{ "Open" , "Open" },
{ "Save" , "Save" },
{ "SaveAs" , "Save As" },
{ "Exit" , "Exit" },
{ "Extras" , "Extras" },
{ "Normalize" , "Normalize" },
{ "ResetZoom" , "Reset Zoom" },
{ "RegisterFileExtension" , "Register File Extension" },
{ "UnregisterFileExtension" , "Unregister File Extension" },
{ "Export" , "Export" },
{ "ExportImage" , "Export Image" },
{ "UseCurrentZoomOnExportedImage" , "Use current zoom on exported image" },
{ "RenderSelectionHighlightsOnExportedImage" , "Render selection highlights on exported image" },
{ "Help" , "Help" },
{ "Version" , "Version" },
{ "FileVersion" , "File Version" },
{ "PresetsVersion" , "Presets Version" },
{ "CheckForUpdates" , "Check For Updates" },
{ "GoToProjectHomepage" , "Go to Project Homepage" },
{ "AboutAnnoDesigner" , "About Anno Designer" },
{ "ShowGrid" , "Show Grid" },
{ "ShowLabels" , "Show Labels" },
{ "ShowIcon" , "Show Icon" },
{ "ShowStats" , "Show Stats" },
{ "BuildingSettings" , "Building Settings" },
{ "Size" , "Size" },
{ "Color" , "Color" },
{ "Label" , "Label" },
{ "Icon" , "Icon" },
{ "Radius" , "Radius" },
{ "Options" , "Options" },
{ "EnableLabel" , "Enable label" },
{ "Borderless" , "Borderless" },
{ "Road" , "Road" },
{ "PlaceBuilding" , "Place building" },
{ "TitleAbout" , "About" },
{ "Title" , "Modified" },
{ "ModifiedAnnoDesigner" , "Modified" },
{ "BuildingLayoutDesigner" , "A building layout designer for Ubisofts Anno-series" },
{ "Credits" , "Credits" },
{ "OriginalApplicationBy" , "Original application by [name]" },
{ "BuildingPresets" , "Building presets" },
{ "CombinedForAnnoVersions" , "Combined building presets for" },
{ "AdditionalChanges" , "Additional changes by" },
{ "ManyThanks" , "Many thanks to all the users who contributed to this project!" },
{ "VisitTheFandom" , "Be sure to visit the Fandom pages for Anno!" },
{ "OriginalHomepage" , "Original Homepage" },
{ "ProjectHomepage" , "Project Homepage" },
{ "GoToFandom" , "Go to Fandom" },
{ "Close" , "Close" }
}
},
{
"Russian", new Dictionary<string, string>() {
{ "File" , "File" },
{ "NewCanvas" , "New Canvas" },
{ "Open" , "Open" },
{ "Save" , "Save" },
{ "SaveAs" , "Save As" },
{ "Exit" , "Exit" },
{ "Extras" , "Extras" },
{ "Normalize" , "Normalize" },
{ "ResetZoom" , "Reset Zoom" },
{ "RegisterFileExtension" , "Register File Extension" },
{ "UnregisterFileExtension" , "Unregister File Extension" },
{ "Export" , "Export" },
{ "ExportImage" , "Export Image" },
{ "UseCurrentZoomOnExportedImage" , "Use current zoom on exported image" },
{ "RenderSelectionHighlightsOnExportedImage" , "Render selection highlights on exported image" },
{ "Help" , "Help" },
{ "Version" , "Version" },
{ "FileVersion" , "File Version" },
{ "PresetsVersion" , "Presets Version" },
{ "CheckForUpdates" , "Check For Updates" },
{ "GoToProjectHomepage" , "Go to Project Homepage" },
{ "AboutAnnoDesigner" , "About Anno Designer" },
{ "ShowGrid" , "Show Grid" },
{ "ShowLabels" , "Show Labels" },
{ "ShowIcon" , "Show Icon" },
{ "ShowStats" , "Show Stats" },
{ "BuildingSettings" , "Building Settings" },
{ "Size" , "Size" },
{ "Color" , "Color" },
{ "Label" , "Label" },
{ "Icon" , "Icon" },
{ "Radius" , "Radius" },
{ "Options" , "Options" },
{ "EnableLabel" , "Enable label" },
{ "Borderless" , "Borderless" },
{ "Road" , "Road" },
{ "PlaceBuilding" , "Place building" },
{ "TitleAbout" , "About" },
{ "Title" , "Modified" },
{ "ModifiedAnnoDesigner" , "Modified" },
{ "BuildingLayoutDesigner" , "A building layout designer for Ubisofts Anno-series" },
{ "Credits" , "Credits" },
{ "OriginalApplicationBy" , "Original application by [name]" },
{ "BuildingPresets" , "Building presets" },
{ "CombinedForAnnoVersions" , "Combined building presets for" },
{ "AdditionalChanges" , "Additional changes by" },
{ "ManyThanks" , "Many thanks to all the users who contributed to this project!" },
{ "VisitTheFandom" , "Be sure to visit the Fandom pages for Anno!" },
{ "OriginalHomepage" , "Original Homepage" },
{ "ProjectHomepage" , "Project Homepage" },
{ "GoToFandom" , "Go to Fandom" },
{ "Close" , "Close" }
}
},
{
"Czech", new Dictionary<string, string>() {
{ "File" , "File" },
{ "NewCanvas" , "New Canvas" },
{ "Open" , "Open" },
{ "Save" , "Save" },
{ "SaveAs" , "Save As" },
{ "Exit" , "Exit" },
{ "Extras" , "Extras" },
{ "Normalize" , "Normalize" },
{ "ResetZoom" , "Reset Zoom" },
{ "RegisterFileExtension" , "Register File Extension" },
{ "UnregisterFileExtension" , "Unregister File Extension" },
{ "Export" , "Export" },
{ "ExportImage" , "Export Image" },
{ "UseCurrentZoomOnExportedImage" , "Use current zoom on exported image" },
{ "RenderSelectionHighlightsOnExportedImage" , "Render selection highlights on exported image" },
{ "Help" , "Help" },
{ "Version" , "Version" },
{ "FileVersion" , "File Version" },
{ "PresetsVersion" , "Presets Version" },
{ "CheckForUpdates" , "Check For Updates" },
{ "GoToProjectHomepage" , "Go to Project Homepage" },
{ "AboutAnnoDesigner" , "About Anno Designer" },
{ "ShowGrid" , "Show Grid" },
{ "ShowLabels" , "Show Labels" },
{ "ShowIcon" , "Show Icon" },
{ "ShowStats" , "Show Stats" },
{ "BuildingSettings" , "Building Settings" },
{ "Size" , "Size" },
{ "Color" , "Color" },
{ "Label" , "Label" },
{ "Icon" , "Icon" },
{ "Radius" , "Radius" },
{ "Options" , "Options" },
{ "EnableLabel" , "Enable label" },
{ "Borderless" , "Borderless" },
{ "Road" , "Road" },
{ "PlaceBuilding" , "Place building" },
{ "TitleAbout" , "About" },
{ "Title" , "Modified" },
{ "ModifiedAnnoDesigner" , "Modified" },
{ "BuildingLayoutDesigner" , "A building layout designer for Ubisofts Anno-series" },
{ "Credits" , "Credits" },
{ "OriginalApplicationBy" , "Original application by [name]" },
{ "BuildingPresets" , "Building presets" },
{ "CombinedForAnnoVersions" , "Combined building presets for" },
{ "AdditionalChanges" , "Additional changes by" },
{ "ManyThanks" , "Many thanks to all the users who contributed to this project!" },
{ "VisitTheFandom" , "Be sure to visit the Fandom pages for Anno!" },
{ "OriginalHomepage" , "Original Homepage" },
{ "ProjectHomepage" , "Project Homepage" },
{ "GoToFandom" , "Go to Fandom" },
{ "Close" , "Close" }
}
},
};
        }

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
        //TODO: Populate this;
        public static Dictionary<string, Dictionary<string, string>> Translations;


    }

    //This static classes cannot be nested as they are used for data binding

    public static class About
    {
        static About()
        {
            TitleAbout = Localization.Translations[AnnoDesigner.MainWindow.SelectedLanguage]["TitleAbout"];
            Title = Localization.Translations[AnnoDesigner.MainWindow.SelectedLanguage]["Title"];
            //Setter not needed as this value is derived from others
            //ModifiedAnnoDesigner = Localization.Translations[AnnoDesigner.MainWindow.SelectedLanguage]["ModifiedAnnoDesigner"];
            BuildingLayoutDesigner = Localization.Translations[AnnoDesigner.MainWindow.SelectedLanguage]["BuildingLayoutDesigner"];

            //Credits
            Credits = Localization.Translations[AnnoDesigner.MainWindow.SelectedLanguage]["Credits"];
            OriginalApplicationBy = Localization.Translations[AnnoDesigner.MainWindow.SelectedLanguage]["OriginalApplicationBy"];
            BuildingPresets = Localization.Translations[AnnoDesigner.MainWindow.SelectedLanguage]["BuildingPresets"];
            CombinedForAnnoVersions = Localization.Translations[AnnoDesigner.MainWindow.SelectedLanguage]["CombinedForAnnoVersions"];
            AdditionalChanges = Localization.Translations[AnnoDesigner.MainWindow.SelectedLanguage]["AdditionalChanges"];
            ManyThanks = Localization.Translations[AnnoDesigner.MainWindow.SelectedLanguage]["ManyThanks"];
            VisitTheFandom = Localization.Translations[AnnoDesigner.MainWindow.SelectedLanguage]["VisitTheFandom"];
            OriginalHomepage = Localization.Translations[AnnoDesigner.MainWindow.SelectedLanguage]["OriginalHomepage"];
            ProjectHomepage = Localization.Translations[AnnoDesigner.MainWindow.SelectedLanguage]["ProjectHomepage"];
            GoToFandom = Localization.Translations[AnnoDesigner.MainWindow.SelectedLanguage]["GoToFandom"];
            Close = Localization.Translations[AnnoDesigner.MainWindow.SelectedLanguage]["Close"];
        }

        private static string _title;
        public static string TitleAbout { get; set; }
        public static string Title { get => TitleAbout + " Anno Designer (" + _title + ")"; set => _title = value; }

        //Setter not needed
        public static string ModifiedAnnoDesigner { get => _title + "Anno Designer"; }
        public static string BuildingLayoutDesigner { get; set; }

        //Credits
        public static string Credits { get; set; }
        public static string OriginalApplicationBy { get; set; }
        public static string BuildingPresets { get; set; }
        public static string CombinedForAnnoVersions { get; set; }
        public static string AdditionalChanges { get; set; }
        public static string ManyThanks { get; set; }
        public static string VisitTheFandom { get; set; }
        public static string OriginalHomepage { get; set; }
        public static string ProjectHomepage { get; set; }
        public static string GoToFandom { get; set; }
        public static string Close { get; set; }


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
            var a = Localization.Translations[AnnoDesigner.MainWindow.SelectedLanguage]["File"];
            //File Menu
            File = Localization.Translations[AnnoDesigner.MainWindow.SelectedLanguage]["File"];
            NewCanvas = Localization.Translations[AnnoDesigner.MainWindow.SelectedLanguage]["NewCanvas"];
            Open = Localization.Translations[AnnoDesigner.MainWindow.SelectedLanguage]["Open"];
            Save = Localization.Translations[AnnoDesigner.MainWindow.SelectedLanguage]["Save"];
            SaveAs = Localization.Translations[AnnoDesigner.MainWindow.SelectedLanguage]["SaveAs"];
            Exit = Localization.Translations[AnnoDesigner.MainWindow.SelectedLanguage]["Exit"];

            //Extras Menu
            Extras = Localization.Translations[AnnoDesigner.MainWindow.SelectedLanguage]["Extras"];
            Normalize = Localization.Translations[AnnoDesigner.MainWindow.SelectedLanguage]["Normalize"];
            ResetZoom = Localization.Translations[AnnoDesigner.MainWindow.SelectedLanguage]["ResetZoom"];
            RegisterFileExtension = Localization.Translations[AnnoDesigner.MainWindow.SelectedLanguage]["RegisterFileExtension"];
            UnregisterFileExtension = Localization.Translations[AnnoDesigner.MainWindow.SelectedLanguage]["UnregisterFileExtension"];

            //Export Menu
            Export = Localization.Translations[AnnoDesigner.MainWindow.SelectedLanguage]["Export"];
            ExportImage = Localization.Translations[AnnoDesigner.MainWindow.SelectedLanguage]["ExportImage"];
            UseCurrentZoomOnExportedImage = Localization.Translations[AnnoDesigner.MainWindow.SelectedLanguage]["UseCurrentZoomOnExportedImage"];
            RenderSelectionHighlightsOnExportedImage = Localization.Translations[AnnoDesigner.MainWindow.SelectedLanguage]["RenderSelectionHighlightsOnExportedImage"];

            //Help Menu
            Help = Localization.Translations[AnnoDesigner.MainWindow.SelectedLanguage]["Help"];
            Version = Localization.Translations[AnnoDesigner.MainWindow.SelectedLanguage]["Version"];
            FileVersion = Localization.Translations[AnnoDesigner.MainWindow.SelectedLanguage]["FileVersion"];
            PresetsVersion = Localization.Translations[AnnoDesigner.MainWindow.SelectedLanguage]["PresetsVersion"];
            CheckForUpdates = Localization.Translations[AnnoDesigner.MainWindow.SelectedLanguage]["CheckForUpdates"];
            GoToProjectHomepage = Localization.Translations[AnnoDesigner.MainWindow.SelectedLanguage]["GoToProjectHomepage"];
            AboutAnnoDesigner = Localization.Translations[AnnoDesigner.MainWindow.SelectedLanguage]["AboutAnnoDesigner"];

            //Other
            ShowGrid = Localization.Translations[AnnoDesigner.MainWindow.SelectedLanguage]["ShowGrid"];
            ShowLabels = Localization.Translations[AnnoDesigner.MainWindow.SelectedLanguage]["ShowLabels"];
            ShowIcon = Localization.Translations[AnnoDesigner.MainWindow.SelectedLanguage]["ShowIcon"];
            ShowStats = Localization.Translations[AnnoDesigner.MainWindow.SelectedLanguage]["ShowStats"];

            //DockPanel
            BuildingSettings = Localization.Translations[AnnoDesigner.MainWindow.SelectedLanguage]["BuildingSettings"];
            Size = Localization.Translations[AnnoDesigner.MainWindow.SelectedLanguage]["Size"];
            Color = Localization.Translations[AnnoDesigner.MainWindow.SelectedLanguage]["Color"];
            Label = Localization.Translations[AnnoDesigner.MainWindow.SelectedLanguage]["Label"];
            Icon = Localization.Translations[AnnoDesigner.MainWindow.SelectedLanguage]["Icon"];
            Radius = Localization.Translations[AnnoDesigner.MainWindow.SelectedLanguage]["Radius"];
            Options = Localization.Translations[AnnoDesigner.MainWindow.SelectedLanguage]["Options"];
            EnableLabel = Localization.Translations[AnnoDesigner.MainWindow.SelectedLanguage]["EnableLabel"];
            Borderless = Localization.Translations[AnnoDesigner.MainWindow.SelectedLanguage]["Borderless"];
            Road = Localization.Translations[AnnoDesigner.MainWindow.SelectedLanguage]["Road"];
            PlaceBuilding = Localization.Translations[AnnoDesigner.MainWindow.SelectedLanguage]["PlaceBuilding"];

            //TODO: Create method as language is changed



        }

        //File Menu
        public static string File { get; set; }
        public static string NewCanvas { get; set; }
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
        public static string ShowLabels { get; set; }
        public static string ShowIcon { get; set; }
        public static string ShowStats { get; set; }

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


