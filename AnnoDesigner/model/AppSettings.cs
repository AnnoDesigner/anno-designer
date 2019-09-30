using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnnoDesigner.Properties;

namespace AnnoDesigner.model
{
    public class AppSettings : IAppSettings
    {
        public void Reload()
        {
            Settings.Default.Reload();
        }

        public void Reset()
        {
            Settings.Default.Reset();
        }

        public void Save()
        {
            Settings.Default.Save();
        }

        public void Upgrade()
        {
            Settings.Default.Upgrade();
        }

        public bool SettingsUpgradeNeeded
        {
            get { return Settings.Default.SettingsUpgradeNeeded; }
            set { Settings.Default.SettingsUpgradeNeeded = value; }
        }

        public bool PromptedForAutoUpdateCheck
        {
            get { return Settings.Default.PromptedForAutoUpdateCheck; }
            set { Settings.Default.PromptedForAutoUpdateCheck = value; }
        }

        public string SelectedLanguage
        {
            get { return Settings.Default.SelectedLanguage; }
            set { Settings.Default.SelectedLanguage = value; }
        }

        public bool StatsShowStats
        {
            get { return Settings.Default.StatsShowStats; }
            set { Settings.Default.StatsShowStats = value; }
        }

        public bool StatsShowBuildingCount
        {
            get { return Settings.Default.StatsShowBuildingCount; }
            set { Settings.Default.StatsShowBuildingCount = value; }
        }

        public bool ShowPavedRoadsWarning
        {
            get { return Settings.Default.ShowPavedRoadsWarning; }
            set { Settings.Default.ShowPavedRoadsWarning = value; }
        }

        public bool EnableAutomaticUpdateCheck
        {
            get { return Settings.Default.EnableAutomaticUpdateCheck; }
            set { Settings.Default.EnableAutomaticUpdateCheck = value; }
        }

        public bool UseCurrentZoomOnExportedImageValue
        {
            get { return Settings.Default.UseCurrentZoomOnExportedImageValue; }
            set { Settings.Default.UseCurrentZoomOnExportedImageValue = value; }
        }

        public bool RenderSelectionHighlightsOnExportedImageValue
        {
            get { return Settings.Default.RenderSelectionHighlightsOnExportedImageValue; }
            set { Settings.Default.RenderSelectionHighlightsOnExportedImageValue = value; }
        }

        public bool ShowLabels
        {
            get { return Settings.Default.ShowLabels; }
            set { Settings.Default.ShowLabels = value; }
        }

        public bool ShowIcons
        {
            get { return Settings.Default.ShowIcons; }
            set { Settings.Default.ShowIcons = value; }
        }

        public bool ShowGrid
        {
            get { return Settings.Default.ShowGrid; }
            set { Settings.Default.ShowGrid = value; }
        }

        public bool IsPavedStreet
        {
            get { return Settings.Default.IsPavedStreet; }
            set { Settings.Default.IsPavedStreet = value; }
        }

        public string TreeViewSearchText
        {
            get { return Settings.Default.TreeViewSearchText; }
            set { Settings.Default.TreeViewSearchText = value; }
        }

        public string PresetsTreeGameVersionFilter
        {
            get { return Settings.Default.PresetsTreeGameVersionFilter; }
            set { Settings.Default.PresetsTreeGameVersionFilter = value; }
        }

        public string PresetsTreeExpandedState
        {
            get { return Settings.Default.PresetsTreeExpandedState; }
            set { Settings.Default.PresetsTreeExpandedState = value; }
        }

        public string PresetsTreeLastVersion
        {
            get { return Settings.Default.PresetsTreeLastVersion; }
            set { Settings.Default.PresetsTreeLastVersion = value; }
        }
    }
}

