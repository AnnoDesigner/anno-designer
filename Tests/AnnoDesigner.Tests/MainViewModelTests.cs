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
using AnnoDesigner.Core;
using System.Globalization;

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

            Assert.NotNull(viewModel.AvailableIcons);
            Assert.NotNull(viewModel.SelectedIcon);
            Assert.NotNull(viewModel.Languages);
            Assert.NotNull(viewModel.MainWindowTitle);
            Assert.NotNull(viewModel.PresetsSectionHeader);

            Assert.Equal(Constants.Version.ToString("0.0#", CultureInfo.InvariantCulture), viewModel.VersionValue);
            Assert.Equal(CoreConstants.LayoutFileVersion.ToString("0.#", CultureInfo.InvariantCulture), viewModel.FileVersionValue);
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

        [Theory]
        [InlineData(System.Windows.WindowState.Maximized)]
        [InlineData(System.Windows.WindowState.Normal)]
        public void SaveSettings_IsCalled_ShouldSaveMainWindowWindowState(System.Windows.WindowState expectedMainMainWindowWindowState)
        {
            // Arrange            
            var appSettings = new Mock<IAppSettings>();
            appSettings.SetupAllProperties();

            var viewModel = GetViewModel(null, appSettings.Object);
            viewModel.MainWindowWindowState = expectedMainMainWindowWindowState;

            // Act
            viewModel.SaveSettings();

            // Assert
            Assert.Equal(expectedMainMainWindowWindowState, appSettings.Object.MainWindowWindowState);
            appSettings.Verify(x => x.Save(), Times.Once);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void SaveSettings_IsCalled_ShouldSaveShowGrid(bool expectedShowGrid)
        {
            // Arrange            
            var appSettings = new Mock<IAppSettings>();
            appSettings.SetupAllProperties();

            var viewModel = GetViewModel(null, appSettings.Object);
            viewModel.CanvasShowGrid = expectedShowGrid;

            // Act
            viewModel.SaveSettings();

            // Assert
            Assert.Equal(expectedShowGrid, appSettings.Object.ShowGrid);
            appSettings.Verify(x => x.Save(), Times.Once);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void SaveSettings_IsCalled_ShouldSaveShowIcons(bool expectedShowIcons)
        {
            // Arrange            
            var appSettings = new Mock<IAppSettings>();
            appSettings.SetupAllProperties();

            var viewModel = GetViewModel(null, appSettings.Object);
            viewModel.CanvasShowIcons = expectedShowIcons;

            // Act
            viewModel.SaveSettings();

            // Assert
            Assert.Equal(expectedShowIcons, appSettings.Object.ShowIcons);
            appSettings.Verify(x => x.Save(), Times.Once);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void SaveSettings_IsCalled_ShouldSaveShowLabels(bool expectedShowLabels)
        {
            // Arrange            
            var appSettings = new Mock<IAppSettings>();
            appSettings.SetupAllProperties();

            var viewModel = GetViewModel(null, appSettings.Object);
            viewModel.CanvasShowLabels = expectedShowLabels;

            // Act
            viewModel.SaveSettings();

            // Assert
            Assert.Equal(expectedShowLabels, appSettings.Object.ShowLabels);
            appSettings.Verify(x => x.Save(), Times.Once);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void SaveSettings_IsCalled_ShouldSaveEnableAutomaticUpdateCheck(bool expectedEnableAutomaticUpdateCheck)
        {
            // Arrange            
            var appSettings = new Mock<IAppSettings>();
            appSettings.SetupAllProperties();

            var viewModel = GetViewModel(null, appSettings.Object);
            viewModel.AutomaticUpdateCheck = expectedEnableAutomaticUpdateCheck;

            // Act
            viewModel.SaveSettings();

            // Assert
            Assert.Equal(expectedEnableAutomaticUpdateCheck, appSettings.Object.EnableAutomaticUpdateCheck);
            appSettings.Verify(x => x.Save(), Times.Once);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void SaveSettings_IsCalled_ShouldSaveUseCurrentZoomOnExportedImageValue(bool expectedUseCurrentZoomOnExportedImageValue)
        {
            // Arrange            
            var appSettings = new Mock<IAppSettings>();
            appSettings.SetupAllProperties();

            var viewModel = GetViewModel(null, appSettings.Object);
            viewModel.UseCurrentZoomOnExportedImageValue = expectedUseCurrentZoomOnExportedImageValue;

            // Act
            viewModel.SaveSettings();

            // Assert
            Assert.Equal(expectedUseCurrentZoomOnExportedImageValue, appSettings.Object.UseCurrentZoomOnExportedImageValue);
            appSettings.Verify(x => x.Save(), Times.Once);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void SaveSettings_IsCalled_ShouldSaveRenderSelectionHighlightsOnExportedImageValue(bool expectedRenderSelectionHighlightsOnExportedImageValue)
        {
            // Arrange            
            var appSettings = new Mock<IAppSettings>();
            appSettings.SetupAllProperties();

            var viewModel = GetViewModel(null, appSettings.Object);
            viewModel.RenderSelectionHighlightsOnExportedImageValue = expectedRenderSelectionHighlightsOnExportedImageValue;

            // Act
            viewModel.SaveSettings();

            // Assert
            Assert.Equal(expectedRenderSelectionHighlightsOnExportedImageValue, appSettings.Object.RenderSelectionHighlightsOnExportedImageValue);
            appSettings.Verify(x => x.Save(), Times.Once);
        }

        [Theory(Skip = "needs abstraction of 'MessageBox.Show' in BuildingSettingsViewModel")]
        [InlineData(true)]
        [InlineData(false)]
        public void SaveSettings_IsCalled_ShouldSaveIsPavedStreet(bool expectedIsPavedStreet)
        {
            // Arrange            
            var appSettings = new Mock<IAppSettings>();
            appSettings.SetupAllProperties();

            var viewModel = GetViewModel(null, appSettings.Object);
            viewModel.BuildingSettingsViewModel.IsPavedStreet = expectedIsPavedStreet;

            // Act
            viewModel.SaveSettings();

            // Assert
            Assert.Equal(expectedIsPavedStreet, appSettings.Object.IsPavedStreet);
            appSettings.Verify(x => x.Save(), Times.Once);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void SaveSettings_IsCalled_ShouldSaveStatsShowStats(bool expectedStatsShowStats)
        {
            // Arrange            
            var appSettings = new Mock<IAppSettings>();
            appSettings.SetupAllProperties();

            var viewModel = GetViewModel(null, appSettings.Object);
            viewModel.StatisticsViewModel.IsVisible = expectedStatsShowStats;

            // Act
            viewModel.SaveSettings();

            // Assert
            Assert.Equal(expectedStatsShowStats, appSettings.Object.StatsShowStats);
            appSettings.Verify(x => x.Save(), Times.Once);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void SaveSettings_IsCalled_ShouldSaveStatsShowBuildingCount(bool expectedStatsShowBuildingCount)
        {
            // Arrange            
            var appSettings = new Mock<IAppSettings>();
            appSettings.SetupAllProperties();

            var viewModel = GetViewModel(null, appSettings.Object);
            viewModel.StatisticsViewModel.ShowStatisticsBuildingCount = expectedStatsShowBuildingCount;

            // Act
            viewModel.SaveSettings();

            // Assert
            Assert.Equal(expectedStatsShowBuildingCount, appSettings.Object.StatsShowBuildingCount);
            appSettings.Verify(x => x.Save(), Times.Once);
        }

        [Theory]
        //[InlineData("")] Skip = "needs abstraction of 'AnnoCanvas' in MainViewModel"
        //[InlineData(" ")] Skip = "needs abstraction of 'AnnoCanvas' in MainViewModel"
        //[InlineData(null)] Skip = "needs abstraction of 'AnnoCanvas' in MainViewModel"
        [InlineData("lorem")]
        public void SaveSettings_IsCalled_ShouldSaveTreeViewSearchText(string expectedTreeViewSearchText)
        {
            // Arrange            
            var appSettings = new Mock<IAppSettings>();
            appSettings.SetupAllProperties();

            var viewModel = GetViewModel(null, appSettings.Object);
            viewModel.PresetsTreeSearchViewModel.SearchText = expectedTreeViewSearchText;

            // Act
            viewModel.SaveSettings();

            // Assert
            Assert.Equal(expectedTreeViewSearchText, appSettings.Object.TreeViewSearchText);
            appSettings.Verify(x => x.Save(), Times.Once);
        }

        [Theory]
        [InlineData(CoreConstants.GameVersion.All)]
        [InlineData(CoreConstants.GameVersion.Anno1404)]
        [InlineData(CoreConstants.GameVersion.Anno1800)]
        [InlineData(CoreConstants.GameVersion.Anno2070)]
        [InlineData(CoreConstants.GameVersion.Anno2205)]
        [InlineData(CoreConstants.GameVersion.Unknown)]
        [InlineData(CoreConstants.GameVersion.Anno1404 | CoreConstants.GameVersion.Anno1800)]
        [InlineData(CoreConstants.GameVersion.Anno2070 | CoreConstants.GameVersion.Anno2205 | CoreConstants.GameVersion.Anno1800)]
        public void SaveSettings_IsCalled_ShouldSavePresetsTreeGameVersionFilter(CoreConstants.GameVersion expectedPresetsTreeGameVersionFilter)
        {
            // Arrange            
            var appSettings = new Mock<IAppSettings>();
            appSettings.SetupAllProperties();

            var viewModel = GetViewModel(null, appSettings.Object);
            viewModel.PresetsTreeViewModel.FilterGameVersion = expectedPresetsTreeGameVersionFilter;

            // Act
            viewModel.SaveSettings();

            // Assert
            Assert.Equal(expectedPresetsTreeGameVersionFilter.ToString(), appSettings.Object.PresetsTreeGameVersionFilter);
            appSettings.Verify(x => x.Save(), Times.Once);
        }

        #endregion

        #region LanguageSelectedCommand tests

        [Fact]
        public void LanguageSelectedCommand_IsExecutedWithNull_ShouldNotThrow()
        {
            // Arrange
            var viewModel = GetViewModel();

            // Act
            var ex = Record.Exception(() => viewModel.LanguageSelectedCommand.Execute(null));

            // Assert
            Assert.Null(ex);
        }

        [Fact]
        public void LanguageSelectedCommand_IsExecutedWithUnknownDataType_ShouldNotThrow()
        {
            // Arrange
            var viewModel = GetViewModel();

            // Act
            var ex = Record.Exception(() => viewModel.LanguageSelectedCommand.Execute(42));

            // Assert
            Assert.Null(ex);
        }

        [Fact]
        public void LanguageSelectedCommand_AlreadyIsLanguageChange_ShouldNotSetLanguage()
        {
            // Arrange
            var languageBeforeChange = "English";

            var commons = new Mock<ICommons>();
            commons.SetupAllProperties();
            commons.Object.SelectedLanguage = languageBeforeChange;

            var viewModel = GetViewModel(commons.Object, null);
            viewModel.IsLanguageChange = true;

            var languageToSet = new SupportedLanguage("Deutsch");

            // Act
            var ex = Record.Exception(() => viewModel.LanguageSelectedCommand.Execute(languageToSet));

            // Assert
            Assert.Null(ex);
            Assert.Equal(languageBeforeChange, commons.Object.SelectedLanguage);
        }

        [Fact]
        public void LanguageSelectedCommand_IsExecutedWithSupportedLanguage_ShouldSetLanguage()
        {
            // Arrange
            var languageBeforeChange = "English";

            var commons = new Mock<ICommons>();
            commons.SetupAllProperties();
            commons.Object.SelectedLanguage = languageBeforeChange;

            var viewModel = GetViewModel(commons.Object, null);

            var languageToSet = new SupportedLanguage("Deutsch");

            // Act
            viewModel.LanguageSelectedCommand.Execute(languageToSet);

            // Assert
            Assert.Equal(languageToSet.Name, commons.Object.SelectedLanguage);
        }

        [Fact]
        public void LanguageSelectedCommand_IsExecutedWithSupportedLanguage_ShouldSetIsLanguageChangeToFalse()
        {
            // Arrange
            var languageBeforeChange = "English";

            var commons = new Mock<ICommons>();
            commons.SetupAllProperties();
            commons.Object.SelectedLanguage = languageBeforeChange;

            var viewModel = GetViewModel(commons.Object, null);

            var languageToSet = new SupportedLanguage("Deutsch");

            // Act
            viewModel.LanguageSelectedCommand.Execute(languageToSet);

            // Assert
            Assert.Equal(languageToSet.Name, commons.Object.SelectedLanguage);
            Assert.False(viewModel.IsLanguageChange);
        }

        #endregion
    }
}
