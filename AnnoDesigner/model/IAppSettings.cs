using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnoDesigner.model
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
        bool IsPavedStreet { get; set; }
    }
}
