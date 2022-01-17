using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Properties;

namespace AnnoDesigner.Models
{
    public class AppSettings : IAppSettings
    {
        public event EventHandler SettingsChanged;

        #region ctor

        private static readonly Lazy<AppSettings> lazy = new Lazy<AppSettings>(() => new AppSettings());

        public static AppSettings Instance
        {
            get { return lazy.Value; }
        }

        private AppSettings()
        {
        }

        #endregion

        public void Reload()
        {
            Settings.Default.Reload();
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Reset()
        {
            Settings.Default.Reset();
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Save()
        {
            Settings.Default.Save();
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Upgrade()
        {
            Settings.Default.Upgrade();
        }

        public bool SettingsUpgradeNeeded
        {
            get => Settings.Default.SettingsUpgradeNeeded;
            set => Settings.Default.SettingsUpgradeNeeded = value;
        }

        public bool PromptedForAutoUpdateCheck
        {
            get => Settings.Default.PromptedForAutoUpdateCheck;
            set => Settings.Default.PromptedForAutoUpdateCheck = value;
        }

        public string SelectedLanguage
        {
            get => Settings.Default.SelectedLanguage;
            set => Settings.Default.SelectedLanguage = value;
        }

        public bool StatsShowStats
        {
            get => Settings.Default.StatsShowStats;
            set => Settings.Default.StatsShowStats = value;
        }

        public bool StatsShowBuildingCount
        {
            get => Settings.Default.StatsShowBuildingCount;
            set => Settings.Default.StatsShowBuildingCount = value;
        }

        public bool ShowPavedRoadsWarning
        {
            get => Settings.Default.ShowPavedRoadsWarning;
            set => Settings.Default.ShowPavedRoadsWarning = value;
        }

        public bool EnableAutomaticUpdateCheck
        {
            get => Settings.Default.EnableAutomaticUpdateCheck;
            set => Settings.Default.EnableAutomaticUpdateCheck = value;
        }

        public bool UseCurrentZoomOnExportedImageValue
        {
            get => Settings.Default.UseCurrentZoomOnExportedImageValue;
            set => Settings.Default.UseCurrentZoomOnExportedImageValue = value;
        }

        public bool RenderSelectionHighlightsOnExportedImageValue
        {
            get => Settings.Default.RenderSelectionHighlightsOnExportedImageValue;
            set => Settings.Default.RenderSelectionHighlightsOnExportedImageValue = value;
        }

        public bool ShowLabels
        {
            get => Settings.Default.ShowLabels;
            set => Settings.Default.ShowLabels = value;
        }

        public bool ShowIcons
        {
            get => Settings.Default.ShowIcons;
            set => Settings.Default.ShowIcons = value;
        }

        public bool ShowGrid
        {
            get => Settings.Default.ShowGrid;
            set => Settings.Default.ShowGrid = value;
        }

        public bool ShowTrueInfluenceRange
        {
            get => Settings.Default.ShowTrueInfluenceRange;
            set => Settings.Default.ShowTrueInfluenceRange = value;
        }

        public bool ShowInfluences
        {
            get => Settings.Default.ShowInfluences;
            set => Settings.Default.ShowInfluences = value;
        }

        public bool ShowHarborBlockedArea
        {
            get => Settings.Default.ShowHarborBlockedArea;
            set => Settings.Default.ShowHarborBlockedArea = value;
        }

        public bool ShowPanorama
        {
            get => Settings.Default.ShowPanorama;
            set => Settings.Default.ShowPanorama = value;
        }

        public bool IsPavedStreet
        {
            get => Settings.Default.IsPavedStreet;
            set => Settings.Default.IsPavedStreet = value;
        }

        public string TreeViewSearchText
        {
            get => Settings.Default.TreeViewSearchText;
            set => Settings.Default.TreeViewSearchText = value;
        }

        public string PresetsTreeGameVersionFilter
        {
            get => Settings.Default.PresetsTreeGameVersionFilter;
            set => Settings.Default.PresetsTreeGameVersionFilter = value;
        }

        public string PresetsTreeExpandedState
        {
            get => Settings.Default.PresetsTreeExpandedState;
            set => Settings.Default.PresetsTreeExpandedState = value;
        }

        public string PresetsTreeLastVersion
        {
            get => Settings.Default.PresetsTreeLastVersion;
            set => Settings.Default.PresetsTreeLastVersion = value;
        }

        public double MainWindowHeight
        {
            get => Settings.Default.MainWindowHeight;
            set => Settings.Default.MainWindowHeight = value;
        }

        public double MainWindowWidth
        {
            get => Settings.Default.MainWindowWidth;
            set => Settings.Default.MainWindowWidth = value;
        }

        public double MainWindowLeft
        {
            get => Settings.Default.MainWindowLeft;
            set => Settings.Default.MainWindowLeft = value;
        }

        public double MainWindowTop
        {
            get => Settings.Default.MainWindowTop;
            set => Settings.Default.MainWindowTop = value;
        }

        public WindowState MainWindowWindowState
        {
            get => Settings.Default.MainWindowWindowState;
            set => Settings.Default.MainWindowWindowState = value;
        }

        public bool UpdateSupportsPrerelease
        {
            get => Settings.Default.UpdateSupportsPrerelease;
            set => Settings.Default.UpdateSupportsPrerelease = value;
        }

        /// <summary>
        /// Serialized <see cref="IDictionary{string, HotkeyInformation}"/>.
        /// </summary>
        public string HotkeyMappings
        {
            get => Settings.Default.HotkeyMappings;
            set => Settings.Default.HotkeyMappings = value;
        }

        public string RecentFiles
        {
            get => Settings.Default.RecentFiles;
            set => Settings.Default.RecentFiles = value;
        }

        public int MaxRecentFiles
        {
            get => Settings.Default.MaxRecentFiles;
            set => Settings.Default.MaxRecentFiles = value;
        }

        public string ColorGridLines
        {
            get => Settings.Default.ColorGridLines;
            set => Settings.Default.ColorGridLines = value;
        }

        public string ColorObjectBorderLines
        {
            get => Settings.Default.ColorObjectBorderLines;
            set => Settings.Default.ColorObjectBorderLines = value;
        }

        public bool UseZoomToPoint
        {
            get => Settings.Default.UseZoomToPoint;
            set => Settings.Default.UseZoomToPoint = value;
        }

        public bool HideInfluenceOnSelection
        {
            get => Settings.Default.HideInfluenceOnSelection;
            set => Settings.Default.HideInfluenceOnSelection = value;
        }

        public double ZoomSensitivityPercentage
        {
            get => Settings.Default.ZoomSensitivityPercentage;
            set => Settings.Default.ZoomSensitivityPercentage = value;
        }

        public bool InvertPanningDirection
        {
            get => Settings.Default.InvertPanningDirection;
            set => Settings.Default.InvertPanningDirection = value;
        }

        public bool InvertScrollingDirection
        {
            get => Settings.Default.InvertScrollingDirection;
            set => Settings.Default.InvertScrollingDirection = value;
        }

        public bool ShowScrollbars
        {
            get => Settings.Default.ShowScrollbars;
            set => Settings.Default.ShowScrollbars = value;
        }

        public bool IncludeRoadsInStatisticCalculation
        {
            get => Settings.Default.IncludeRoadsInStatisticCalculation;
            set => Settings.Default.IncludeRoadsInStatisticCalculation = value;
        }
    }
}

