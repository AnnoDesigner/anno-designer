using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace AnnoDesigner.Localization
{
    public static class Localization
    {
        public static Dictionary<string, Dictionary<string, string>> Translations;

        static Localization()
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
                        { "RegisterFileExtensionSuccessful" , "Registration of file extension was successful." },
                        { "UnregisterFileExtension" , "Unregister File Extension" },
                        { "UnregisterFileExtensionSuccessful" , "Deregistration of file extension was successful." },
                        { "Successful" , "Successful" },
                        { "Export" , "Export" },
                        { "ExportImage" , "Export Image" },
                        { "UseCurrentZoomOnExportedImage" , "Use current zoom on exported image" },
                        { "RenderSelectionHighlightsOnExportedImage" , "Render selection highlights on exported image" },
                        { "Language" , "Language" },
                        { "ManageStats" , "Manage Stats" },
                        { "ShowStats" , "Show Stats" },
                        { "BuildingCount" , "Building Count" },
                        { "Help" , "Help" },
                        { "Version" , "Version" },
                        { "FileVersion" , "File Version" },
                        { "PresetsVersion" , "Presets Version" },
                        { "CheckForUpdates" , "Check For Updates" },
                        { "EnableAutomaticUpdateCheck" , "Enable Automatic Update Check on Startup" },
                        { "GoToProjectHomepage" , "Go to Project Homepage" },
                        { "OpenWelcomePage" , "Open Welcome page" },
                        { "AboutAnnoDesigner" , "About Anno Designer" },
                        { "ShowGrid" , "Show Grid" },
                        { "ShowLabels" , "Show Labels" },
                        { "ShowIcons" , "Show Icons" },
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
                        { "BuildingLayoutDesigner" , "A building layout designer for Ubisofts Anno-series" },
                        { "Credits" , "Credits" },
                        { "OriginalApplicationBy" , "Original application by" },
                        { "BuildingPresets" , "Building presets" },
                        { "CombinedForAnnoVersions" , "Combined building presets for" },
                        { "AdditionalChanges" , "Additional changes by" },
                        { "ManyThanks" , "Many thanks to all the users who contributed to this project!" },
                        { "VisitTheFandom" , "Be sure to visit the Fandom pages for Anno!" },
                        { "OriginalHomepage" , "Original Homepage" },
                        { "ProjectHomepage" , "Project Homepage" },
                        { "GoToFandom" , "Go to Fandom" },
                        { "Close" , "Close" },
                        { "StatusBarControls" , "Mouse controls: left - place, select, move // right - stop placement, remove // both - move all // double click - copy // wheel - zoom // wheel-click - rotate." },
                        { "StatNothingPlaced" , "Nothing Placed" },
                        { "StatBoundingBox" , "Bounding Box" },
                        { "StatMinimumArea" , "Minimum Area" },
                        { "StatSpaceEfficiency" , "Space Efficiency" },
                        { "StatBuildings" , "Buildings" },
                        { "StatBuildingsSelected" , "Buildings Selected" },
                        { "UnknownObject" , "Unknown Object" },
                        { "PresetsLoaded" , "Building presets loaded" }
                    }
                 },
                {
                    "ger", new Dictionary<string, string>() {
                        { "File" , "Datei" },
                        { "NewCanvas" , "Neue Oberfläche" },
                        { "Open" , "Öffnen" },
                        { "Save" , "Speichern" },
                        { "SaveAs" , "Speichern unter" },
                        { "Exit" , "Beenden" },
                        { "Extras" , "Extras" },
                        { "Normalize" , "Normalisieren" },
                        { "ResetZoom" , "Zoom zurücksetzen" },
                        { "RegisterFileExtension" , "Dateierweiterung registrieren" },
                        { "RegisterFileExtensionSuccessful" , "Dateierweiterung wurde erfolgreich registriert." },
                        { "UnregisterFileExtension" , "Dateierweiterung entfernen" },
                        { "UnregisterFileExtensionSuccessful" , "Registrierung der Dateierweiterung wurde entfernt." },
                        { "Successful" , "Erfolg" },
                        { "Export" , "Exportieren" },
                        { "ExportImage" , "Exportiere Bild / Speichere als Bild" },
                        { "UseCurrentZoomOnExportedImage" , "Wende aktuellen Zomm auf exportiertes Bild an" },
                        { "RenderSelectionHighlightsOnExportedImage" , "Exportiere Bild mit Selektionen" },
                        { "Language" , "Sprache" },
                        { "ManageStats" , "Statistiken verwalten" },
                        { "ShowStats" , "Statistiken (an)zeigen" },
                        { "BuildingCount" , "Anzahl der Gebäude" },
                        { "Help" , "Hilfe" },
                        { "Version" , "Version" },
                        { "FileVersion" , "Dateiversion" },
                        { "PresetsVersion" , "Preset-Version" },
                        { "CheckForUpdates" , "Auf Updates prüfen" },
                        { "EnableAutomaticUpdateCheck" , "Automatische Updateprüfung beim Start aktivieren" },
                        { "GoToProjectHomepage" , "Gehe zu Projekt Startseite" },
                        { "OpenWelcomePage" , "Willkommensseite öffnen" },
                        { "AboutAnnoDesigner" , "über Anno Designer" },
                        { "ShowGrid" , "Raster/Gitter (an)zeigen" },
                        { "ShowLabels" , "Bezeichnungen (an)zeigen" },
                        { "ShowIcons" , "Symbol (an)zeigen" },
                        { "BuildingSettings" , "Gebäude Optionen" },
                        { "Size" , "Größe" },
                        { "Color" , "Farbe" },
                        { "Label" , "Bezeichnung" },
                        { "Icon" , "Zeichen/Icon" },
                        { "Radius" , "Radius" },
                        { "Options" , "Optionen" },
                        { "EnableLabel" , "Bezeichnung aktivieren" },
                        { "Borderless" , "Randlos" },
                        { "Road" , "Straße" },
                        { "PlaceBuilding" , "Gebäude platzieren" },
                        { "TitleAbout" , "über" },
                        { "Title" , "überarbeiteter" },
                        { "BuildingLayoutDesigner" , "Ein Gebäudelayout Designer für Ubisofts Anno Reihe" },
                        { "Credits" , "Credits" },
                        { "OriginalApplicationBy" , "Ursprüngliche Anwendung von" },
                        { "BuildingPresets" , "Gebäudevorlagen" },
                        { "CombinedForAnnoVersions" , "Zusammengefügte Gebäudevorlagen für" },
                        { "AdditionalChanges" , "Zusätzliche Änderungen von" },
                        { "ManyThanks" , "Vielen Dank an alle, die an diesem Projekt mitgeholfen haben!" },
                        { "VisitTheFandom" , "Besuche auch die Fandom Seiten von Anno!" },
                        { "OriginalHomepage" , "Original Startseite" },
                        { "ProjectHomepage" , "Projekt Startsteite" },
                        { "GoToFandom" , "Fandom Seite" },
                        { "Close" , "Schließen" },
                        { "StatusBarControls" , "Maussteuerung: Links - Platzieren, Auswählen, Verschieben // Rechts - Platzieren stoppen // Beide - Alle verschieben // Doppelklicken - Kopieren // Rad Zoom // Klickrad - Drehen" },
                        { "StatNothingPlaced" , "Nichts platziert" },
                        { "StatBoundingBox" , "Begrenzungsbox" },
                        { "StatMinimumArea" , "Minimale Fläche" },
                        { "StatSpaceEfficiency" , "Raumeffizienz" },
                        { "StatBuildings" , "Gebäude" },
                        { "StatBuildingsSelected" , "Ausgewählte Gebäude" },
                        { "UnknownObject" , "Unbekanntes Objekt" },
                        { "PresetsLoaded" , "Gebäudevorlagen geladen" }
                    }
                 },
                {
                    "pol", new Dictionary<string, string>() {
                        { "File" , "Plik" },
                        { "NewCanvas" , "Nowy projekt" },
                        { "Open" , "Otwórz" },
                        { "Save" , "Zapisz" },
                        { "SaveAs" , "Zapisz jako" },
                        { "Exit" , "Zamknij" },
                        { "Extras" , "Dodatki" },
                        { "Normalize" , "Znormalizuj" },
                        { "ResetZoom" , "Resetuj powiększenie" },
                        { "RegisterFileExtension" , "Zarejestruj rozszerzenie pliku" },
                        { "RegisterFileExtensionSuccessful" , "Rejestracja rozszerzenia pliku zakończyła się sukcesem." },
                        { "UnregisterFileExtension" , "Wyrejestruj rozszerzenie pliku" },
                        { "UnregisterFileExtensionSuccessful" , "Wyrejestrowanie rozszerzenia pliku zakończyło się sukcesem." },
                        { "Successful" , "Udało się." },
                        { "Export" , "Eksportuj" },
                        { "ExportImage" , "Eksportuj obraz" },
                        { "UseCurrentZoomOnExportedImage" , "Użyj obecnego powiększenia na eksportowanym obrazie" },
                        { "RenderSelectionHighlightsOnExportedImage" , "Pokaż podświetlenie wybranych elementów na eksportowanym obrazie" },
                        { "Language" , "Język" },
                        { "ManageStats" , "Zarządzanie statystyki" },
                        { "ShowStats" , "Pokaż statystyki" },
                        { "BuildingCount" , "Licznik budynków" },
                        { "Help" , "Pomoc" },
                        { "Version" , "Wersja" },
                        { "FileVersion" , "Wersja pliku" },
                        { "PresetsVersion" , "Presety-wersja" },
                        { "CheckForUpdates" , "Sprawdź aktualizacje" },
                        { "EnableAutomaticUpdateCheck" , "Włączyć automatyczne sprawdzanie aktualizacji przy uruchamianiu" },
                        { "GoToProjectHomepage" , "Przejdź do strony projektu" },
                        { "OpenWelcomePage" , "Otwórz stronę powitalną" },
                        { "AboutAnnoDesigner" , "O Anno Designerze" },
                        { "ShowGrid" , "Pokaż siatkę" },
                        { "ShowLabels" , "Pokaż etykiety" },
                        { "ShowIcons" , "Pokaż ikony" },
                        { "BuildingSettings" , "Ustawienia budynku" },
                        { "Size" , "Wymiary" },
                        { "Color" , "Kolor" },
                        { "Label" , "Podpis" },
                        { "Icon" , "Ikona" },
                        { "Radius" , "Promień" },
                        { "Options" , "Opcje" },
                        { "EnableLabel" , "Pokaż etykietę" },
                        { "Borderless" , "Bez obramowania" },
                        { "Road" , "Droga / Ulica" },
                        { "PlaceBuilding" , "Postaw budynek" },
                        { "TitleAbout" , "Na temat / O" },
                        { "Title" , "zmodyfikowany" },
                        { "BuildingLayoutDesigner" , "Program do planowania zabudowy w serii Anno Ubisoftu" },
                        { "Credits" , "Autorzy" },
                        { "OriginalApplicationBy" , "Oryginalna aplikacja napisana przez" },
                        { "BuildingPresets" , "Presety dla budynków" },
                        { "CombinedForAnnoVersions" , "Połączone presety budynków dla" },
                        { "AdditionalChanges" , "Dodatkowe zmiany wprowadzone przez" },
                        { "ManyThanks" , "Dziękujemy wszystkim użytkownikom, którzy wsparli ten projekt!" },
                        { "VisitTheFandom" , "Odwiedź fanowskie strony o Anno!" },
                        { "OriginalHomepage" , "Oryginalna strona" },
                        { "ProjectHomepage" , "Strona projektu" },
                        { "GoToFandom" , "Przejdź do strony Fandom" },
                        { "Close" , "Zamknij" },
                        { "StatusBarControls" , "Sterowanie myszą: w lewo - miejsce, zaznacz, przesuń // zatrzymanie w prawo, usuń // oba - przenieś wszystko // podwójne kliknięcie - kopiuj // kółko - powiększ // kółko - obracaj." },
                        { "StatNothingPlaced" , "Nic nie postawiono" },
                        { "StatBoundingBox" , "Ramka Ograniczająca" },
                        { "StatMinimumArea" , "Minimalna Powierzchnia" },
                        { "StatSpaceEfficiency" , "Wykorzystanie Przestrzeni" },
                        { "StatBuildings" , "Budynki" },
                        { "StatBuildingsSelected" , "Wybrane Budynki" },
                        { "UnknowObject" , "Obiekt nieznany" },
                        { "PresetsLoaded" , "Presety budynków załadowano" }
                    }
                 },
                {
                    "rus", new Dictionary<string, string>() {
                        { "File" , "Файл" },
                        { "NewCanvas" , "Новый файл" },
                        { "Open" , "Открыть" },
                        { "Save" , "Сохранить" },
                        { "SaveAs" , "Сохранить как" },
                        { "Exit" , "Выход" },
                        { "Extras" , "Дополнительно" },
                        { "Normalize" , "Нормализация" },
                        { "ResetZoom" , "Сбросить масштаб" },
                        { "RegisterFileExtension" , "Зарегистрировать расширение файла" },
                        { "RegisterFileExtensionSuccessful" , "Регистрация расширения файла прошла успешно." },
                        { "UnregisterFileExtension" , "Отмена регистрации расширения файла" },
                        { "UnregisterFileExtensionSuccessful" , "Отмена регистрации продления файлов прошла успешно." },
                        { "Successful" , "Успешный" },
                        { "Export" , "Экспорт" },
                        { "ExportImage" , "Экспортировать изображение" },
                        { "UseCurrentZoomOnExportedImage" , "Использовать текущее масштабирование экспортируемого изображения" },
                        { "RenderSelectionHighlightsOnExportedImage" , "Выделение выделенного фрагмента на экспортируемом изображении" },
                        { "Language" , "язык" },
                        { "ManageStats" , "Управление статистикой" },
                        { "ShowStats" , "Показывать параметры" },
                        { "BuildingCount" , "Строительный граф" },
                        { "Help" , "Помощь" },
                        { "Version" , "Версия" },
                        { "FileVersion" , "Версия файла" },
                        { "PresetsVersion" , "Версия пресета" },
                        { "CheckForUpdates" , "Проверить наличие обновлений" },
                        { "EnableAutomaticUpdateCheck" , "Включить автоматическую проверку обновлений при запуске" },
                        { "GoToProjectHomepage" , "Перейти на главную страницу" },
                        { "OpenWelcomePage" , "Открыть страницу приветствия" },
                        { "AboutAnnoDesigner" , "О Anno Дизайнер" },
                        { "ShowGrid" , "Показать сетку" },
                        { "ShowLabels" , "Показывать название" },
                        { "ShowIcons" , "Показывать значок" },
                        { "BuildingSettings" , "Параметры здания" },
                        { "Size" , "Размер" },
                        { "Color" , "Цвет" },
                        { "Label" , "Название" },
                        { "Icon" , "Значок" },
                        { "Radius" , "Радиус" },
                        { "Options" , "Параметры" },
                        { "EnableLabel" , "Показывать название" },
                        { "Borderless" , "Без полей" },
                        { "Road" , "Дорогa" },
                        { "PlaceBuilding" , "Выбрать здание" },
                        { "TitleAbout" , "О программе" },
                        { "Title" , "обновлено" },
                        { "BuildingLayoutDesigner" , "Конструктор макета здания для Ubisofts Anno-серии" },
                        { "Credits" , "Авторы" },
                        { "OriginalApplicationBy" , "Оригинальное приложение" },
                        { "BuildingPresets" , "Строительные пресеты" },
                        { "CombinedForAnnoVersions" , "Комбинированные строительные пресеты для" },
                        { "AdditionalChanges" , "Дополнительные изменения от" },
                        { "ManyThanks" , "Большое спасибо всем пользователям, которые внесли свой вклад в этот проект!" },
                        { "VisitTheFandom" , "Обязательно посетите страницы Фэндома для Anno!" },
                        { "OriginalHomepage" , "Оригинальная домашняя страница" },
                        { "ProjectHomepage" , "Домашняя страница проекта" },
                        { "GoToFandom" , "Перейти к Фэндом" },
                        { "Close" , "Закрыть" },
                        { "StatusBarControls" , "Управление мышью: влево - разместить, выбрать, переместить // вправо - прекратить размещение, удалить // оба - переместить все // двойной щелчок - копировать // колесо - масштабировать // колесико щелкнуть - повернуть." },
                        { "StatNothingPlaced" , "Ничего не размещено" },
                        { "StatBoundingBox" , "Ограничительная рамка" },
                        { "StatMinimumArea" , "Минимальная площадь" },
                        { "StatSpaceEfficiency" , "Космическая эффективность" },
                        { "StatBuildings" , "Здания" },
                        { "StatBuildingsSelected" , "Выбранные здания" },
                        { "UnknownObject" , "Неизвестный объект" },
                        { "PresetsLoaded" , "Загружаются пресеты зданий" }
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


    //These classes cannot be nested as they are used for data binding

    public class About : Notify
    {
        public About()
        {
            UpdateLanguage();
        }

        public void UpdateLanguage()
        {
            string language = Localization.GetLanguageCodeFromName(AnnoDesigner.MainWindow.SelectedLanguage);

            TitleAbout = Localization.Translations[language]["TitleAbout"];
            Title = Localization.Translations[language]["Title"];
            //Title = "Modified", ModifiedAnnoDesigner = "Modified" + " Anno Designer"
            ModifiedAnnoDesigner = Localization.Translations[language]["Title"];
            BuildingLayoutDesigner = Localization.Translations[language]["BuildingLayoutDesigner"];

            //Credits
            Credits = Localization.Translations[language]["Credits"];
            OriginalApplicationBy = Localization.Translations[language]["OriginalApplicationBy"];
            BuildingPresets = Localization.Translations[language]["BuildingPresets"];
            CombinedForAnnoVersions = Localization.Translations[language]["CombinedForAnnoVersions"];
            AdditionalChanges = Localization.Translations[language]["AdditionalChanges"];
            ManyThanks = Localization.Translations[language]["ManyThanks"];
            VisitTheFandom = Localization.Translations[language]["VisitTheFandom"];
            OriginalHomepage = Localization.Translations[language]["OriginalHomepage"];
            ProjectHomepage = Localization.Translations[language]["ProjectHomepage"];
            GoToFandom = Localization.Translations[language]["GoToFandom"];
            Close = Localization.Translations[language]["Close"];
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

        private string _titleAbout;
        public string TitleAbout
        {
            get { return _titleAbout; }
            set
            {
                UpdateProperty(ref _titleAbout, value);
            }
        }
        private string _title;
        public string Title
        {
            get { return TitleAbout + " Anno Designer (" + _title + ")"; }
            set
            {
                UpdateProperty(ref _title, value);
            }
        }

        //Setter not needed
        private string _modifiedAnnoDesigner;
        public string ModifiedAnnoDesigner
        {
            get { return _modifiedAnnoDesigner + " Anno Designer"; }
            set
            {
                UpdateProperty(ref _modifiedAnnoDesigner, value);
            }
        }
        private string _buildingLayoutDesigner;
        public string BuildingLayoutDesigner
        {
            get { return _buildingLayoutDesigner; }
            set
            {
                UpdateProperty(ref _buildingLayoutDesigner, value);
            }
        }

        //Credits
        private string _credits;
        public string Credits
        {
            get { return _credits; }
            set
            {
                UpdateProperty(ref _credits, value);
            }
        }
        private string _originalApplicationBy;
        public string OriginalApplicationBy
        {
            get { return _originalApplicationBy; }
            set
            {
                UpdateProperty(ref _originalApplicationBy, value);
            }
        }
        private string _buildingPresets;
        public string BuildingPresets
        {
            get { return _buildingPresets; }
            set
            {
                UpdateProperty(ref _buildingPresets, value);
            }
        }
        private string _combinedForAnnoVersions;
        public string CombinedForAnnoVersions
        {
            get { return _combinedForAnnoVersions; }
            set
            {
                UpdateProperty(ref _combinedForAnnoVersions, value);
            }
        }
        private string _additionalChanges;
        public string AdditionalChanges
        {
            get { return _additionalChanges; }
            set
            {
                UpdateProperty(ref _additionalChanges, value);
            }
        }
        private string _manyThanks;
        public string ManyThanks
        {
            get { return _manyThanks; }
            set
            {
                UpdateProperty(ref _manyThanks, value);
            }
        }
        private string _visitTheFandom;
        public string VisitTheFandom
        {
            get { return _visitTheFandom; }
            set
            {
                UpdateProperty(ref _visitTheFandom, value);
            }
        }
        private string _originalHomepage;
        public string OriginalHomepage
        {
            get { return _originalHomepage; }
            set
            {
                UpdateProperty(ref _originalHomepage, value);
            }
        }
        private string _projectHomepage;
        public string ProjectHomepage
        {
            get { return _projectHomepage; }
            set
            {
                UpdateProperty(ref _projectHomepage, value);
            }
        }
        private string _goToFandom;
        public string GoToFandom
        {
            get { return _goToFandom; }
            set
            {
                UpdateProperty(ref _goToFandom, value);
            }
        }
        private string _close;
        public string Close
        {
            get { return _close; }
            set
            {
                UpdateProperty(ref _close, value);
            }
        }


    }

    public class AnnoCanvas : Notify
    {
        public AnnoCanvas()
        {
            UpdateLanguage();
        }

        public void UpdateLanguage()
        {
            string language = Localization.GetLanguageCodeFromName(AnnoDesigner.MainWindow.SelectedLanguage);

            //Statistics Section
            NothingPlaced = Localization.Translations[language]["StatNothingPlaced"];
            BoundingBox = Localization.Translations[language]["StatBoundingBox"];
            MinimumArea = Localization.Translations[language]["StatMinimumArea"];
            SpaceEfficiency = Localization.Translations[language]["StatSpaceEfficiency"];

        }
        //Statistics Section
        private string _nothingPlaced;
        public string NothingPlaced
        {
            get { return _nothingPlaced; }
            set
            {
                UpdateProperty(ref _nothingPlaced, value);
            }
        }
        private string _boundingBox;
        public string BoundingBox
        {
            get { return _boundingBox; }
            set
            {
                UpdateProperty(ref _boundingBox, value);
            }
        }
        private string _minimumArea;
        public string MinimumArea
        {
            get { return _minimumArea; }
            set
            {
                UpdateProperty(ref _minimumArea, value);
            }
        }
        private string _spaceEfficiency;
        public string SpaceEfficiency
        {
            get { return _spaceEfficiency; }
            set
            {
                UpdateProperty(ref _spaceEfficiency, value);
            }
        }
    }

    ////Probably nothing to add in here
    //public class App : Notify
    //{
    //}


    /// <summary>
    /// Holds information about the current localized symbols for the "MainWindow.xaml" window
    /// </summary>
    public class MainWindow : Notify
    {
        public MainWindow()
        {
            UpdateLanguage();
        }

        public void UpdateLanguage()
        {
            string language = Localization.GetLanguageCodeFromName(AnnoDesigner.MainWindow.SelectedLanguage);
            //File Menu
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
            Radius = Localization.Translations[language]["Radius"];
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
        private string _radius;
        public string Radius
        {
            get { return _radius; }
            set
            {
                UpdateProperty(ref _radius, value);
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

        public class Welcome : Notify
        {
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
    }

    public class SupportedLanguage
    {
        public string Name { get; set; }
        public string FlagPath { get; set; }
    }
}


