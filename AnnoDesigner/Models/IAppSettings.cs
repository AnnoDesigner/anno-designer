using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AnnoDesigner.Models
{
    public interface IAppSettings
    {
        void Reload();
        void Reset();
        void Save();
        void Upgrade();
        bool SettingsUpgradeNeeded { get; set; }

        bool PromptedForAutoUpdateCheck { get; set; }
        string SelectedLanguage { get; set; }
        bool StatsShowStats { get; set; }
        bool StatsShowBuildingCount { get; set; }
        bool ShowPavedRoadsWarning { get; set; }
        bool EnableAutomaticUpdateCheck { get; set; }
        bool UseCurrentZoomOnExportedImageValue { get; set; }
        bool RenderSelectionHighlightsOnExportedImageValue { get; set; }
        bool ShowLabels { get; set; }
        bool ShowIcons { get; set; }
        bool ShowGrid { get; set; }
        bool ShowInfluences { get; set; }
        bool IsPavedStreet { get; set; }
        string TreeViewSearchText { get; set; }
        string PresetsTreeGameVersionFilter { get; set; }
        string PresetsTreeExpandedState { get; set; }
        string PresetsTreeLastVersion { get; set; }
        double MainWindowHeight { get; set; }
        double MainWindowWidth { get; set; }
        double MainWindowLeft { get; set; }
        double MainWindowTop { get; set; }
        WindowState MainWindowWindowState { get; set; }
        bool UpdateSupportsPrerelease { get; set; }
    }
}
