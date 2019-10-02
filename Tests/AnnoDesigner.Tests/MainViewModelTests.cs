using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnnoDesigner.viewmodel;
using Xunit;
using Moq;
using AnnoDesigner.Core.Models;
using AnnoDesigner.model;

namespace AnnoDesigner.Tests
{
    public class MainViewModelTests
    {
        private readonly ICommons _mockedCommons;
        private readonly IAppSettings _mockedAppSettings;

        public MainViewModelTests()
        {
            var commonsMock = new Mock<ICommons>();
            commonsMock.SetupGet(x => x.SelectedLanguage).Returns(() => "English");
            _mockedCommons = commonsMock.Object;

            _mockedAppSettings = new Mock<IAppSettings>().Object;
        }

        private MainViewModel GetViewModel(ICommons commonsToUse = null, IAppSettings appSettingsToUse = null)
        {
            return new MainViewModel(commonsToUse ?? _mockedCommons, appSettingsToUse ?? _mockedAppSettings);
        }

        #region ctor tests

        [Fact]
        public void Ctor_ShouldSetDefaultValues()
        {
            // Arrange/Act
            var viewModel = GetViewModel();

            // Assert
            Assert.NotNull(viewModel.OpenProjectHomepageCommand);
            Assert.NotNull(viewModel.CloseWindowCommand);
            Assert.NotNull(viewModel.CanvasResetZoomCommand);
            Assert.NotNull(viewModel.CanvasNormalizeCommand);
            Assert.NotNull(viewModel.LoadLayoutFromJsonCommand);
            Assert.NotNull(viewModel.UnregisterExtensionCommand);
            Assert.NotNull(viewModel.RegisterExtensionCommand);
            Assert.NotNull(viewModel.ExportImageCommand);
            Assert.NotNull(viewModel.CopyLayoutToClipboardCommand);
            Assert.NotNull(viewModel.LanguageSelectedCommand);
            Assert.NotNull(viewModel.ShowAboutWindowCommand);
            Assert.NotNull(viewModel.ShowWelcomeWindowCommand);
            Assert.NotNull(viewModel.CheckForUpdatesCommand);
            Assert.NotNull(viewModel.ShowStatisticsCommand);
            Assert.NotNull(viewModel.ShowStatisticsBuildingCountCommand);
            Assert.NotNull(viewModel.PlaceBuildingCommand);

            Assert.NotNull(viewModel.StatisticsViewModel);
            Assert.NotNull(viewModel.BuildingSettingsViewModel);
            Assert.NotNull(viewModel.PresetsTreeViewModel);
            Assert.NotNull(viewModel.PresetsTreeSearchViewModel);
            Assert.NotNull(viewModel.WelcomeViewModel);
            Assert.NotNull(viewModel.AboutViewModel);

            Assert.False(viewModel.CanvasShowGrid);
            Assert.False(viewModel.CanvasShowIcons);
            Assert.False(viewModel.CanvasShowLabels);
            Assert.False(viewModel.AutomaticUpdateCheck);
            Assert.False(viewModel.UseCurrentZoomOnExportedImageValue);
            Assert.False(viewModel.RenderSelectionHighlightsOnExportedImageValue);
            Assert.False(viewModel.IsLanguageChange);
            Assert.False(viewModel.IsBusy);

            Assert.Null(viewModel.PresetsVersionValue);
            Assert.Null(viewModel.StatusMessage);
            Assert.Null(viewModel.StatusMessageClipboard);

            Assert.NotNull(viewModel.VersionValue);
            Assert.NotNull(viewModel.FileVersionValue);
            Assert.NotNull(viewModel.AvailableIcons);
            Assert.NotNull(viewModel.SelectedIcon);
            Assert.NotNull(viewModel.Languages);
            Assert.NotNull(viewModel.MainWindowTitle);
            Assert.NotNull(viewModel.PresetsSectionHeader);
        }

        #endregion

        #region CloseWindowCommand tests

        [Fact]
        public void CloseWindowCommand_ShouldCanExecute()
        {
            // Arrange
            var viewModel = GetViewModel();

            // Act
            var result = viewModel.CloseWindowCommand.CanExecute(null);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CloseWindowCommand_IsExecutedWithNull_ShouldNotThrow()
        {
            // Arrange
            var viewModel = GetViewModel();

            // Act
            var ex = Record.Exception(() => viewModel.CloseWindowCommand.Execute(null));

            // Assert
            Assert.Null(ex);
        }

        [Fact]
        public void CloseWindowCommand_IsExecutedWithICloseable_ShouldCallClose()
        {
            // Arrange
            var viewModel = GetViewModel();
            var mockedCloseable = new Mock<ICloseable>();

            // Act
            viewModel.CloseWindowCommand.Execute(mockedCloseable.Object);

            // Assert
            mockedCloseable.Verify(x => x.Close(), Times.Once);
        }

        #endregion

        #region SearchText tests

        [Fact]
        public void PresetsTreeSearchViewModelPropertyChanged_SeachTextChanged_ShouldSetFilterTextOnPresetsTreeViewModel()
        {
            // Arrange
            var viewModel = GetViewModel();
            viewModel.PresetsTreeViewModel.FilterText = "Lorem";

            var textToSet = "dummy";

            // Act
            viewModel.PresetsTreeSearchViewModel.SearchText = textToSet;

            // Assert
            Assert.Equal(textToSet, viewModel.PresetsTreeViewModel.FilterText);
        }

        #endregion

        #region ShowStatisticsCommand tests

        [Fact]
        public void ShowStatisticsCommand_IsExecuted_ShouldRaiseShowStatisticsChangedEvent()
        {
            // Arrange
            var viewModel = GetViewModel();

            // Act/Assert
            Assert.Raises<EventArgs>(
                x => viewModel.ShowStatisticsChanged += x,
                x => viewModel.ShowStatisticsChanged -= x,
                () => viewModel.ShowStatisticsCommand.Execute(null));
        }

        #endregion

        #region SaveSettings tests

        [Fact]
        public void SaveSettings_IsCalled_ShouldSaveMainWindowHeight()
        {
            // Arrange            
            var appSettings = new Mock<IAppSettings>();
            appSettings.SetupAllProperties();

            var expectedMainWindowHeight = 42.4;

            var viewModel = GetViewModel(null, appSettings.Object);
            viewModel.MainWindowHeight = expectedMainWindowHeight;

            // Act
            viewModel.SaveSettings();

            // Assert
            Assert.Equal(expectedMainWindowHeight, appSettings.Object.MainWindowHeight);
            appSettings.Verify(x => x.Save(), Times.Once);
        }

        [Fact]
        public void SaveSettings_IsCalled_ShouldSaveMainWindowWidth()
        {
            // Arrange            
            var appSettings = new Mock<IAppSettings>();
            appSettings.SetupAllProperties();

            var expectedMainMainWindowWidth = 42.4;

            var viewModel = GetViewModel(null, appSettings.Object);
            viewModel.MainWindowWidth = expectedMainMainWindowWidth;

            // Act
            viewModel.SaveSettings();

            // Assert
            Assert.Equal(expectedMainMainWindowWidth, appSettings.Object.MainWindowWidth);
            appSettings.Verify(x => x.Save(), Times.Once);
        }

        [Fact]
        public void SaveSettings_IsCalled_ShouldSaveMainWindowLeft()
        {
            // Arrange            
            var appSettings = new Mock<IAppSettings>();
            appSettings.SetupAllProperties();

            var expectedMainMainWindowLeft = 42.4;

            var viewModel = GetViewModel(null, appSettings.Object);
            viewModel.MainWindowLeft = expectedMainMainWindowLeft;

            // Act
            viewModel.SaveSettings();

            // Assert
            Assert.Equal(expectedMainMainWindowLeft, appSettings.Object.MainWindowLeft);
            appSettings.Verify(x => x.Save(), Times.Once);
        }

        [Fact]
        public void SaveSettings_IsCalled_ShouldSaveMainWindowTop()
        {
            // Arrange            
            var appSettings = new Mock<IAppSettings>();
            appSettings.SetupAllProperties();

            var expectedMainMainWindowTop = 42.4;

            var viewModel = GetViewModel(null, appSettings.Object);
            viewModel.MainWindowTop = expectedMainMainWindowTop;

            // Act
            viewModel.SaveSettings();

            // Assert
            Assert.Equal(expectedMainMainWindowTop, appSettings.Object.MainWindowTop);
            appSettings.Verify(x => x.Save(), Times.Once);
        }

        [Fact]
        public void SaveSettings_IsCalled_ShouldSaveMainWindowWindowState()
        {
            // Arrange            
            var appSettings = new Mock<IAppSettings>();
            appSettings.SetupAllProperties();

            var expectedMainMainWindowWindowState = System.Windows.WindowState.Normal;

            var viewModel = GetViewModel(null, appSettings.Object);
            viewModel.MainWindowWindowState = expectedMainMainWindowWindowState;

            // Act
            viewModel.SaveSettings();

            // Assert
            Assert.Equal(expectedMainMainWindowWindowState, appSettings.Object.MainWindowWindowState);
            appSettings.Verify(x => x.Save(), Times.Once);
        }

        [Fact]
        public void SaveSettings_IsCalled_ShouldSaveShowGrid()
        {
            // Arrange            
            var appSettings = new Mock<IAppSettings>();
            appSettings.SetupAllProperties();

            var expectedShowGrid = true;

            var viewModel = GetViewModel(null, appSettings.Object);
            viewModel.CanvasShowGrid = expectedShowGrid;

            // Act
            viewModel.SaveSettings();

            // Assert
            Assert.Equal(expectedShowGrid, appSettings.Object.ShowGrid);
            appSettings.Verify(x => x.Save(), Times.Once);
        }

        [Fact]
        public void SaveSettings_IsCalled_ShouldSaveShowIcons()
        {
            // Arrange            
            var appSettings = new Mock<IAppSettings>();
            appSettings.SetupAllProperties();

            var expectedShowIcons = true;

            var viewModel = GetViewModel(null, appSettings.Object);
            viewModel.CanvasShowIcons = expectedShowIcons;

            // Act
            viewModel.SaveSettings();

            // Assert
            Assert.Equal(expectedShowIcons, appSettings.Object.ShowIcons);
            appSettings.Verify(x => x.Save(), Times.Once);
        }

        [Fact]
        public void SaveSettings_IsCalled_ShouldSaveShowLabels()
        {
            // Arrange            
            var appSettings = new Mock<IAppSettings>();
            appSettings.SetupAllProperties();

            var expectedShowLabels = true;

            var viewModel = GetViewModel(null, appSettings.Object);
            viewModel.CanvasShowLabels = expectedShowLabels;

            // Act
            viewModel.SaveSettings();

            // Assert
            Assert.Equal(expectedShowLabels, appSettings.Object.ShowLabels);
            appSettings.Verify(x => x.Save(), Times.Once);
        }

        [Fact]
        public void SaveSettings_IsCalled_ShouldSaveEnableAutomaticUpdateCheck()
        {
            // Arrange            
            var appSettings = new Mock<IAppSettings>();
            appSettings.SetupAllProperties();

            var expectedEnableAutomaticUpdateCheck = true;

            var viewModel = GetViewModel(null, appSettings.Object);
            viewModel.AutomaticUpdateCheck = expectedEnableAutomaticUpdateCheck;

            // Act
            viewModel.SaveSettings();

            // Assert
            Assert.Equal(expectedEnableAutomaticUpdateCheck, appSettings.Object.EnableAutomaticUpdateCheck);
            appSettings.Verify(x => x.Save(), Times.Once);
        }

        [Fact]
        public void SaveSettings_IsCalled_ShouldSaveUseCurrentZoomOnExportedImageValue()
        {
            // Arrange            
            var appSettings = new Mock<IAppSettings>();
            appSettings.SetupAllProperties();

            var expectedUseCurrentZoomOnExportedImageValue = true;

            var viewModel = GetViewModel(null, appSettings.Object);
            viewModel.UseCurrentZoomOnExportedImageValue = expectedUseCurrentZoomOnExportedImageValue;

            // Act
            viewModel.SaveSettings();

            // Assert
            Assert.Equal(expectedUseCurrentZoomOnExportedImageValue, appSettings.Object.UseCurrentZoomOnExportedImageValue);
            appSettings.Verify(x => x.Save(), Times.Once);
        }

        [Fact]
        public void SaveSettings_IsCalled_ShouldSaveRenderSelectionHighlightsOnExportedImageValue()
        {
            // Arrange            
            var appSettings = new Mock<IAppSettings>();
            appSettings.SetupAllProperties();

            var expectedRenderSelectionHighlightsOnExportedImageValue = true;

            var viewModel = GetViewModel(null, appSettings.Object);
            viewModel.RenderSelectionHighlightsOnExportedImageValue = expectedRenderSelectionHighlightsOnExportedImageValue;

            // Act
            viewModel.SaveSettings();

            // Assert
            Assert.Equal(expectedRenderSelectionHighlightsOnExportedImageValue, appSettings.Object.RenderSelectionHighlightsOnExportedImageValue);
            appSettings.Verify(x => x.Save(), Times.Once);
        }

        [Fact(Skip = "needs abstraction of 'MessageBox.Show' in BuildingSettingsViewModel")]
        public void SaveSettings_IsCalled_ShouldSaveIsPavedStreet()
        {
            // Arrange            
            var appSettings = new Mock<IAppSettings>();
            appSettings.SetupAllProperties();

            var expectedIsPavedStreet = true;

            var viewModel = GetViewModel(null, appSettings.Object);
            viewModel.BuildingSettingsViewModel.IsPavedStreet = expectedIsPavedStreet;

            // Act
            viewModel.SaveSettings();

            // Assert
            Assert.Equal(expectedIsPavedStreet, appSettings.Object.IsPavedStreet);
            appSettings.Verify(x => x.Save(), Times.Once);
        }

        [Fact]
        public void SaveSettings_IsCalled_ShouldSaveStatsShowStats()
        {
            // Arrange            
            var appSettings = new Mock<IAppSettings>();
            appSettings.SetupAllProperties();

            var expectedStatsShowStats = true;

            var viewModel = GetViewModel(null, appSettings.Object);
            viewModel.StatisticsViewModel.IsVisible = expectedStatsShowStats;

            // Act
            viewModel.SaveSettings();

            // Assert
            Assert.Equal(expectedStatsShowStats, appSettings.Object.StatsShowStats);
            appSettings.Verify(x => x.Save(), Times.Once);
        }

        [Fact]
        public void SaveSettings_IsCalled_ShouldSaveStatsShowBuildingCount()
        {
            // Arrange            
            var appSettings = new Mock<IAppSettings>();
            appSettings.SetupAllProperties();

            var expectedStatsShowBuildingCount = true;

            var viewModel = GetViewModel(null, appSettings.Object);
            viewModel.StatisticsViewModel.ShowStatisticsBuildingCount = expectedStatsShowBuildingCount;

            // Act
            viewModel.SaveSettings();

            // Assert
            Assert.Equal(expectedStatsShowBuildingCount, appSettings.Object.StatsShowBuildingCount);
            appSettings.Verify(x => x.Save(), Times.Once);
        }

        #endregion
    }
}
