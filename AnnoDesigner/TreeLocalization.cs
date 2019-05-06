using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace AnnoDesigner.TreeLocalization
{
    public static class TreeLocalization
    {
        public static Dictionary<string, Dictionary<string, string>> Translations;

        static TreeLocalization()
        {
            //This dictionary initialisation was auto-generated from:
            //https://docs.google.com/spreadsheets/d/1CjECty43mkkm1waO4yhQl1rzZ-ZltrBgj00aq-WJX4w/edit?usp=sharing 
            //Steps to format:
            //Run CreateDictionary Script
            //Copy Output
            //Replace the escaped characters (\t\r\n) with the actual characters from within an editor of your choice
            Translations = new Dictionary<string, Dictionary<string, string>>()
            {
                {
                    "eng", new Dictionary<string, string>() {
                        {"Military" , "Military" },
                        {"Camps" , "Camps" },
                        {"Production" , "Production" },
                        {"Ornament" , "Ornament" },
                        {"ManorialPalace" , "ManorialPalace" },
                        {"PlayerBuildings" , "PlayerBuildings" },
                        {"Harbour" , "Harbor" },
                        {"Residence" , "Residence" },
                        {"Animalfarm" , "Animalfarm" },
                        {"Factory" , "Factory" },
                        {"Farm" , "Farm" },
                        {"FarmFields" , "Farm Fields" },
                        {"Plantation" , "Plantation" },
                        {"Resource" , "Resource" },
                        {"Public" , "Public" },
                        {"Demand" , "Demand" },
                        {"Marine" , "Marine" },
                        {"Special" , "Special" },
                        {"Trade" , "Trade" },
                    }
                    },
                {
                    "ger", new Dictionary<string, string>() {
                        {"Military" , "Military" },
                        {"Camps" , "Camps" },
                        {"Production" , "Production" },
                        {"Ornament" , "Ornament" },
                        {"ManorialPalace" , "ManorialPalace" },
                        {"PlayerBuildings" , "PlayerBuildings" },
                        {"Harbour" , "Harbor" },
                        {"Residence" , "Residence" },
                        {"Animalfarm" , "Animalfarm" },
                        {"Factory" , "Factory" },
                        {"Farm" , "Farm" },
                        {"FarmFields" , "Farm Fields" },
                        {"Plantation" , "Plantation" },
                        {"Resource" , "Resource" },
                        {"Public" , "Public" },
                        {"Demand" , "Demand" },
                        {"Marine" , "Marine" },
                        {"Special" , "Special" },
                        {"Trade" , "Trade" },
                    }
                    },
                {
                    "pol", new Dictionary<string, string>() {
                        {"Military" , "Military" },
                        {"Camps" , "Camps" },
                        {"Production" , "Production" },
                        {"Ornament" , "Ornament" },
                        {"ManorialPalace" , "ManorialPalace" },
                        {"PlayerBuildings" , "PlayerBuildings" },
                        {"Harbour" , "Harbor" },
                        {"Residence" , "Residence" },
                        {"Animalfarm" , "Animalfarm" },
                        {"Factory" , "Factory" },
                        {"Farm" , "Farm" },
                        {"FarmFields" , "Farm Fields" },
                        {"Plantation" , "Plantation" },
                        {"Resource" , "Resource" },
                        {"Public" , "Public" },
                        {"Demand" , "Demand" },
                        {"Marine" , "Marine" },
                        {"Special" , "Special" },
                        {"Trade" , "Trade" },
                    }
                    },
                {
                    "rus", new Dictionary<string, string>() {
                        {"Military" , "Military" },
                        {"Camps" , "Camps" },
                        {"Production" , "Production" },
                        {"Ornament" , "Ornament" },
                        {"ManorialPalace" , "ManorialPalace" },
                        {"PlayerBuildings" , "PlayerBuildings" },
                        {"Harbour" , "Harbor" },
                        {"Residence" , "Residence" },
                        {"Animalfarm" , "Animalfarm" },
                        {"Factory" , "Factory" },
                        {"Farm" , "Farm" },
                        {"FarmFields" , "Farm Fields" },
                        {"Plantation" , "Plantation" },
                        {"Resource" , "Resource" },
                        {"Public" , "Public" },
                        {"Demand" , "Demand" },
                        {"Marine" , "Marine" },
                        {"Special" , "Special" },
                        {"Trade" , "Trade" },
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

        public static void Update()
        {

        }

    }

    /// <summary>
    /// Holds the base INotifyPropertyChanged implementation plus helper methods
    /// //https://stackoverflow.com/questions/1315621/implementing-inotifypropertychanged-does-a-better-way-exist
    /// </summary>
    public class Notify : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            //Invoke event if not null
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        protected bool UpdateProperty<T>(ref T field, T value, [CallerMemberName] string name = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            OnPropertyChanged(name);
            return true;
        }
    }


    //Preset Localization Process;

    /// <summary>
    /// Holds information about the current localized symbols for the "Preset Tree View"
    /// </summary>
    public class LocaPresetTree : Notify
    {
        public LocaPresetTree()
        {
            UpdateLanguage();
        }

        public void UpdateLanguage()
        {
            string language = TreeLocalization.GetLanguageCodeFromName(AnnoDesigner.MainWindow.SelectedLanguage);
            
            File = Localization.Translations[language]["File"];
            NewCanvas = Localization.Translations[language]["NewCanvas"];
            Open = Localization.Translations[language]["Open"];
            Save = Localization.Translations[language]["Save"];
            SaveAs = Localization.Translations[language]["SaveAs"];
            Exit = Localization.Translations[language]["Exit"];

            //Extras Menu
            Extras = Localization.Translations[language]["Extras"];
            Normalize = Localization.Translations[language]["Normalize"];
            ResetZoom = Localization.Translations[language]["ResetZoom"];
            RegisterFileExtension = Localization.Translations[language]["RegisterFileExtension"];
            UnregisterFileExtension = Localization.Translations[language]["UnregisterFileExtension"];

            //Export Menu
            Export = Localization.Translations[language]["Export"];
            ExportImage = Localization.Translations[language]["ExportImage"];
            UseCurrentZoomOnExportedImage = Localization.Translations[language]["UseCurrentZoomOnExportedImage"];
            RenderSelectionHighlightsOnExportedImage = Localization.Translations[language]["RenderSelectionHighlightsOnExportedImage"];

            //Manage Stats Menu
            ManageStats = Localization.Translations[language]["ManageStats"];
            ShowStats = Localization.Translations[language]["ShowStats"];
            BuildingCount = Localization.Translations[language]["BuildingCount"];

            //Language Menu
            Language = Localization.Translations[language]["Language"];

            //Help Menu
            Help = Localization.Translations[language]["Help"];
            Version = Localization.Translations[language]["Version"];
            FileVersion = Localization.Translations[language]["FileVersion"];
            PresetsVersion = Localization.Translations[language]["PresetsVersion"];
            CheckForUpdates = Localization.Translations[language]["CheckForUpdates"];
            EnableAutomaticUpdateCheck = Localization.Translations[language]["EnableAutomaticUpdateCheck"];
            GoToProjectHomepage = Localization.Translations[language]["GoToProjectHomepage"];
            OpenWelcomePage = Localization.Translations[language]["OpenWelcomePage"];
            AboutAnnoDesigner = Localization.Translations[language]["AboutAnnoDesigner"];

            //Other
            ShowGrid = Localization.Translations[language]["ShowGrid"];
            ShowLabels = Localization.Translations[language]["ShowLabels"];
            ShowIcons = Localization.Translations[language]["ShowIcons"];

            //DockPanel
            BuildingSettings = Localization.Translations[language]["BuildingSettings"];
            Size = Localization.Translations[language]["Size"];
            Color = Localization.Translations[language]["Color"];
            Label = Localization.Translations[language]["Label"];
            Icon = Localization.Translations[language]["Icon"];
            InfluenceType = Localization.Translations[language]["InfluenceType"];
            None = Localization.Translations[language]["None"];
            Radius = Localization.Translations[language]["Radius"];
            Distance = Localization.Translations[language]["Distance"];
            Both = Localization.Translations[language]["Both"];
            Options = Localization.Translations[language]["Options"];
            EnableLabel = Localization.Translations[language]["EnableLabel"];
            Borderless = Localization.Translations[language]["Borderless"];
            Road = Localization.Translations[language]["Road"];
            PlaceBuilding = Localization.Translations[language]["PlaceBuilding"];

            //Status Bar
            StatusBarControls = Localization.Translations[language]["StatusBarControls"];
        }

        //Generated from:
        //...
        //public string Prop1 {get; set;}
        //public string Prop2 {get; set;}
        //...
        //find expr: public (string) (.+?) {.+
        //With the following regex (in a compatible editor that supports lowercasing of values
        //within regex expressions):
        //private $1 _\l$2; \r\n public $1 $2 \r\n { \r\n get { return _\l$2; } \r\n set \r\n { \r\n UpdateProperty\(ref _\l$2, value\); \r\n}\r\n}

        //File Menu
        private string _file;
        public string File
        {
            get { return _file; }
            set
            {
                UpdateProperty(ref _file, value);
            }
        }
        private string _newCanvas;
        public string NewCanvas
        {
            get { return _newCanvas; }
            set
            {
                UpdateProperty(ref _newCanvas, value);
            }
        }
        private string _open;
        public string Open
        {
            get { return _open; }
            set
            {
                UpdateProperty(ref _open, value);
            }
        }
        private string _save;
        public string Save
        {
            get { return _save; }
            set
            {
                UpdateProperty(ref _save, value);
            }
        }
        private string _saveAs;
        public string SaveAs
        {
            get { return _saveAs; }
            set
            {
                UpdateProperty(ref _saveAs, value);
            }
        }
        private string _exit;
        public string Exit
        {
            get { return _exit; }
            set
            {
                UpdateProperty(ref _exit, value);
            }
        }

        //Extras Menu
        private string _extras;
        public string Extras
        {
            get { return _extras; }
            set
            {
                UpdateProperty(ref _extras, value);
            }
        }
        private string _normalize;
        public string Normalize
        {
            get { return _normalize; }
            set
            {
                UpdateProperty(ref _normalize, value);
            }
        }
        private string _resetZoom;
        public string ResetZoom
        {
            get { return _resetZoom; }
            set
            {
                UpdateProperty(ref _resetZoom, value);
            }
        }
        private string _registerFileExtension;
        public string RegisterFileExtension
        {
            get { return _registerFileExtension; }
            set
            {
                UpdateProperty(ref _registerFileExtension, value);
            }
        }
        private string _unregisterFileExtension;
        public string UnregisterFileExtension
        {
            get { return _unregisterFileExtension; }
            set
            {
                UpdateProperty(ref _unregisterFileExtension, value);
            }
        }

        //Export Menu
        private string _export;
        public string Export
        {
            get { return _export; }
            set
            {
                UpdateProperty(ref _export, value);
            }
        }
        private string _exportImage;
        public string ExportImage
        {
            get { return _exportImage; }
            set
            {
                UpdateProperty(ref _exportImage, value);
            }
        }
        private string _useCurrentZoomOnExportedImage;
        public string UseCurrentZoomOnExportedImage
        {
            get { return _useCurrentZoomOnExportedImage; }
            set
            {
                UpdateProperty(ref _useCurrentZoomOnExportedImage, value);
            }
        }
        private string _renderSelectionHighlightsOnExportedImage;
        public string RenderSelectionHighlightsOnExportedImage
        {
            get { return _renderSelectionHighlightsOnExportedImage; }
            set
            {
                UpdateProperty(ref _renderSelectionHighlightsOnExportedImage, value);
            }
        }

        //Language Menu
        private string _language;
        public string Language
        {
            get { return _language; }
            set
            {
                UpdateProperty(ref _language, value);
            }
        }

        //Managa Stats Menu
        private string _ManageStats;
        public string ManageStats
        {
            get { return _ManageStats; }
            set
            {
                UpdateProperty(ref _ManageStats, value);
            }
        }
        private string _showStats;
        public string ShowStats
        {
            get { return _showStats; }
            set
            {
                UpdateProperty(ref _showStats, value);
            }
        }
        private string _BuildingCount;
        public string BuildingCount
        {
            get { return _BuildingCount; }
            set
            {
                UpdateProperty(ref _BuildingCount, value);
            }
        }

        //Help Menu
        private string _help;
        public string Help
        {
            get { return _help; }
            set
            {
                UpdateProperty(ref _help, value);
            }
        }
        private string _version;
        public string Version
        {
            get { return _version; }
            set
            {
                UpdateProperty(ref _version, value);
            }
        }
        private string _fileVersion;
        public string FileVersion
        {
            get { return _fileVersion; }
            set
            {
                UpdateProperty(ref _fileVersion, value);
            }
        }
        private string _presetsVersion;
        public string PresetsVersion
        {
            get { return _presetsVersion; }
            set
            {
                UpdateProperty(ref _presetsVersion, value);
            }
        }
        private string _checkForUpdates;
        public string CheckForUpdates
        {
            get { return _checkForUpdates; }
            set
            {
                UpdateProperty(ref _checkForUpdates, value);
            }
        }

        private string _enableAutomaticUpdateCheck;
        public string EnableAutomaticUpdateCheck
        {
            get { return _enableAutomaticUpdateCheck; }
            set
            {
                UpdateProperty(ref _enableAutomaticUpdateCheck, value);
            }
        }

        private string _goToProjectHomepage;
        public string GoToProjectHomepage
        {
            get { return _goToProjectHomepage; }
            set
            {
                UpdateProperty(ref _goToProjectHomepage, value);
            }
        }

        private string _openWelcomePage;
        public string OpenWelcomePage
        {
            get { return _openWelcomePage; }
            set
            {
                UpdateProperty(ref _openWelcomePage, value);
            }
        }

        private string _aboutAnnoDesigner;
        public string AboutAnnoDesigner
        {
            get { return _aboutAnnoDesigner; }
            set
            {
                UpdateProperty(ref _aboutAnnoDesigner, value);
            }
        }

        //Other
        private string _showGrid;
        public string ShowGrid
        {
            get { return _showGrid; }
            set
            {
                UpdateProperty(ref _showGrid, value);
            }
        }
        private string _showLabels;
        public string ShowLabels
        {
            get { return _showLabels; }
            set
            {
                UpdateProperty(ref _showLabels, value);
            }
        }
        private string _showIcons;
        public string ShowIcons
        {
            get { return _showIcons; }
            set
            {
                UpdateProperty(ref _showIcons, value);
            }
        }

        //DockPanel
        private string _buildingSettings;
        public string BuildingSettings
        {
            get { return _buildingSettings; }
            set
            {
                UpdateProperty(ref _buildingSettings, value);
            }
        }
        private string _size;
        public string Size
        {
            get { return _size; }
            set
            {
                UpdateProperty(ref _size, value);
            }
        }
        private string _color;
        public string Color
        {
            get { return _color; }
            set
            {
                UpdateProperty(ref _color, value);
            }
        }
        private string _label;
        public string Label
        {
            get { return _label; }
            set
            {
                UpdateProperty(ref _label, value);
            }
        }

        private string _icon;
        public string Icon
        {
            get { return _icon; }
            set
            {
                UpdateProperty(ref _icon, value);
            }
        }
        private string _influenceType;
        public string InfluenceType
        {
            get { return _influenceType; }
            set
            {
                UpdateProperty(ref _influenceType, value);
            }
        }
        private string _none;
        public string None
        {
            get { return _none; }
            set
            {
                UpdateProperty(ref _none, value);
            }
        }
        private string _radius;
        public string Radius
        {
            get { return _radius; }
            set
            {
                UpdateProperty(ref _radius, value);
            }
        }
        private string _distance;
        public string Distance
        {
            get { return _distance; }
            set
            {
                UpdateProperty(ref _distance, value);
            }
        }
        private string _both;
        public string Both
        {
            get { return _both; }
            set
            {
                UpdateProperty(ref _both, value);
            }
        }
        private string _options;
        public string Options
        {
            get { return _options; }
            set
            {
                UpdateProperty(ref _options, value);
            }
        }
        private string _enableLabel;
        public string EnableLabel
        {
            get { return _enableLabel; }
            set
            {
                UpdateProperty(ref _enableLabel, value);
            }
        }
        private string _borderless;
        public string Borderless
        {
            get { return _borderless; }
            set
            {
                UpdateProperty(ref _borderless, value);
            }
        }
        private string _road;
        public string Road
        {
            get { return _road; }
            set
            {
                UpdateProperty(ref _road, value);
            }
        }
        private string _placeBuilding;
        public string PlaceBuilding
        {
            get { return _placeBuilding; }
            set
            {
                UpdateProperty(ref _placeBuilding, value);
            }
        }

        //Status Bar
        private string _statusBarControls;
        public string StatusBarControls
        {
            get { return _statusBarControls; }
            set
            {
                UpdateProperty(ref _statusBarControls, value);
            }
        }
    }

    public class Welcome : Notify
    {
        //Generated from:
        //...
        //public string Prop1 {get; set;}
        //public string Prop2 {get; set;}
        //...
        //find expr: public (string) (.+?) {.+
        //With the following regex (in a compatible editor that supports lowercasing of values
        //within regex expressions):
        //private $1 _\l$2; \r\n public $1 $2 \r\n { \r\n get { return _\l$2; } \r\n set \r\n { \r\n UpdateProperty\(ref _\l$2, value\); \r\n}\r\n}


        private string _continue;
        public string Continue
        {
            get { return _continue; }
            set
            {
                UpdateProperty(ref _continue, value);
            }
        }

        private string _selectALanguageWarning;
        public string SelectALanguageWarning
        {
            get { return _selectALanguageWarning; }
            set
            {
                UpdateProperty(ref _selectALanguageWarning, value);
            }
        }
    }

    public class SupportedLanguage
    {
        public string Name { get; set; }
        public string FlagPath { get; set; }
    }
}


