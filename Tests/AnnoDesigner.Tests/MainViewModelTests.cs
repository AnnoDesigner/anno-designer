using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Windows.Input;
using AnnoDesigner.Core;
using AnnoDesigner.Core.Helper;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Core.Presets.Models;
using AnnoDesigner.Core.RecentFiles;
using AnnoDesigner.Core.Services;
using AnnoDesigner.Models;
using AnnoDesigner.ViewModels;
using Moq;
using Xunit;

namespace AnnoDesigner.Tests
{
    public class MainViewModelTests
    {
        private readonly ICommons _mockedCommons;
        private readonly IAppSettings _mockedAppSettings;
        private readonly IAnnoCanvas _mockedAnnoCanvas;
        private readonly IRecentFilesHelper _inMemoryRecentFilesHelper;
        private readonly IMessageBoxService _mockedMessageBoxService;
        private readonly ILocalizationHelper _mockedLocalizationHelper;
        private readonly IUpdateHelper _mockedUpdateHelper;
        private readonly IFileSystem _mockedFileSystem;

        public MainViewModelTests()
        {
            _mockedFileSystem = new MockFileSystem();

            var commonsMock = new Mock<ICommons>();
            commonsMock.SetupGet(x => x.CurrentLanguage).Returns(() => "English");
            commonsMock.SetupGet(x => x.CurrentLanguageCode).Returns(() => "eng");
            commonsMock.SetupGet(x => x.LanguageCodeMap).Returns(() => new Dictionary<string, string>());
            _mockedCommons = commonsMock.Object;

            var mockedLocalizationHelper = new Mock<ILocalizationHelper>();
            mockedLocalizationHelper.Setup(x => x.GetLocalization(It.IsAny<string>())).Returns<string>(x => x);
            mockedLocalizationHelper.Setup(x => x.GetLocalization(It.IsAny<string>(), It.IsAny<string>())).Returns((string value, string langauge) => value);
            _mockedLocalizationHelper = mockedLocalizationHelper.Object;

            Localization.Localization.Init(_mockedCommons);

            _mockedAppSettings = new Mock<IAppSettings>().Object;

            var annoCanvasMock = new Mock<IAnnoCanvas>();
            annoCanvasMock.SetupAllProperties();
            _mockedAnnoCanvas = annoCanvasMock.Object;

            _inMemoryRecentFilesHelper = new RecentFilesHelper(new RecentFilesInMemorySerializer(), new MockFileSystem());

            _mockedMessageBoxService = new Mock<IMessageBoxService>().Object;
            _mockedUpdateHelper = new Mock<IUpdateHelper>().Object;
        }

        private MainViewModel GetViewModel(ICommons commonsToUse = null,
            IAppSettings appSettingsToUse = null,
            IRecentFilesHelper recentFilesHelperToUse = null,
            IMessageBoxService messageBoxServiceToUse = null,
            IUpdateHelper updateHelperToUse = null,
            ILocalizationHelper localizationHelperToUse = null,
            IAnnoCanvas annoCanvasToUse = null,
            IFileSystem fileSystemToUse = null)
        {
            return new MainViewModel(commonsToUse ?? _mockedCommons,
                appSettingsToUse ?? _mockedAppSettings,
                recentFilesHelperToUse ?? _inMemoryRecentFilesHelper,
                messageBoxServiceToUse ?? _mockedMessageBoxService,
                updateHelperToUse ?? _mockedUpdateHelper,
                localizationHelperToUse ?? _mockedLocalizationHelper,
                fileSystemToUse ?? _mockedFileSystem)
            {
                AnnoCanvas = annoCanvasToUse ?? _mockedAnnoCanvas
            };
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
            Assert.NotNull(viewModel.PreferencesUpdateViewModel.CheckForUpdatesCommand);
            Assert.NotNull(viewModel.ShowStatisticsCommand);
            Assert.NotNull(viewModel.ShowStatisticsBuildingCountCommand);
            Assert.NotNull(viewModel.PlaceBuildingCommand);

            Assert.NotNull(viewModel.StatisticsViewModel);
            Assert.NotNull(viewModel.BuildingSettingsViewModel);
            Assert.NotNull(viewModel.PresetsTreeViewModel);
            Assert.NotNull(viewModel.PresetsTreeSearchViewModel);
            Assert.NotNull(viewModel.WelcomeViewModel);
            Assert.NotNull(viewModel.AboutViewModel);
            Assert.NotNull(viewModel.PreferencesUpdateViewModel);

            Assert.False(viewModel.CanvasShowGrid);
            Assert.False(viewModel.CanvasShowIcons);
            Assert.False(viewModel.CanvasShowLabels);
            Assert.False(viewModel.UseCurrentZoomOnExportedImageValue);
            Assert.False(viewModel.RenderSelectionHighlightsOnExportedImageValue);
            Assert.False(viewModel.IsLanguageChange);
            Assert.False(viewModel.IsBusy);

            Assert.Null(viewModel.StatusMessage);
            Assert.Null(viewModel.StatusMessageClipboard);

            Assert.NotNull(viewModel.AvailableIcons);
            Assert.NotNull(viewModel.SelectedIcon);
            Assert.NotNull(viewModel.Languages);
            Assert.NotNull(viewModel.MainWindowTitle);
            Assert.NotNull(viewModel.PresetsSectionHeader);

            Assert.Equal(Constants.Version.ToString("0.0#", CultureInfo.InvariantCulture), viewModel.PreferencesUpdateViewModel.VersionValue);
            Assert.Equal(CoreConstants.LayoutFileVersion.ToString("0.#", CultureInfo.InvariantCulture), viewModel.PreferencesUpdateViewModel.FileVersionValue);
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
        public void PresetsTreeSearchViewModelPropertyChanged_SearchTextChanged_ShouldSetFilterTextOnPresetsTreeViewModel()
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
            appSettings.Verify(x => x.Save(), Times.AtLeastOnce);
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
            appSettings.Verify(x => x.Save(), Times.AtLeastOnce);
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
            appSettings.Verify(x => x.Save(), Times.AtLeastOnce);
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
            appSettings.Verify(x => x.Save(), Times.AtLeastOnce);
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
            appSettings.Verify(x => x.Save(), Times.AtLeastOnce);
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
            appSettings.Verify(x => x.Save(), Times.AtLeastOnce);
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
            appSettings.Verify(x => x.Save(), Times.AtLeastOnce);
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
            appSettings.Verify(x => x.Save(), Times.AtLeastOnce);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void SaveSettings_IsCalled_ShouldSaveShowInfluences(bool expectedShowInfluences)
        {
            // Arrange            
            var appSettings = new Mock<IAppSettings>();
            appSettings.SetupAllProperties();

            var viewModel = GetViewModel(null, appSettings.Object);
            viewModel.CanvasShowInfluences = expectedShowInfluences;

            // Act
            viewModel.SaveSettings();

            // Assert
            Assert.Equal(expectedShowInfluences, appSettings.Object.ShowInfluences);
            appSettings.Verify(x => x.Save(), Times.AtLeastOnce);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void SaveSettings_IsCalled_ShouldSaveShowTrueInfluenceRange(bool expectedShowTrueInfluenceRange)
        {
            // Arrange            
            var appSettings = new Mock<IAppSettings>();
            appSettings.SetupAllProperties();

            var viewModel = GetViewModel(null, appSettings.Object);
            viewModel.CanvasShowTrueInfluenceRange = expectedShowTrueInfluenceRange;

            // Act
            viewModel.SaveSettings();

            // Assert
            Assert.Equal(expectedShowTrueInfluenceRange, appSettings.Object.ShowTrueInfluenceRange);
            appSettings.Verify(x => x.Save(), Times.AtLeastOnce);
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
            viewModel.PreferencesUpdateViewModel.AutomaticUpdateCheck = expectedEnableAutomaticUpdateCheck;

            // Act
            viewModel.SaveSettings();

            // Assert
            Assert.Equal(expectedEnableAutomaticUpdateCheck, appSettings.Object.EnableAutomaticUpdateCheck);
            appSettings.Verify(x => x.Save(), Times.AtLeastOnce);
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
            appSettings.Verify(x => x.Save(), Times.AtLeastOnce);
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
            appSettings.Verify(x => x.Save(), Times.AtLeastOnce);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void SaveSettings_IsCalled_ShouldSaveIsPavedStreet(bool expectedIsPavedStreet)
        {
            // Arrange            
            var appSettings = new Mock<IAppSettings>();
            appSettings.SetupAllProperties();

            var presets = new BuildingPresets
            {
                Buildings = new List<BuildingInfo>()
            };

            var canvas = new Mock<IAnnoCanvas>();
            canvas.SetupGet(x => x.BuildingPresets).Returns(() => presets);

            var viewModel = GetViewModel(null, appSettings.Object, annoCanvasToUse: canvas.Object);
            viewModel.BuildingSettingsViewModel.IsPavedStreet = expectedIsPavedStreet;

            // Act
            viewModel.SaveSettings();

            // Assert
            Assert.Equal(expectedIsPavedStreet, appSettings.Object.IsPavedStreet);
            appSettings.Verify(x => x.Save(), Times.AtLeastOnce);
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
            appSettings.Verify(x => x.Save(), Times.AtLeastOnce);
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
            appSettings.Verify(x => x.Save(), Times.AtLeastOnce);
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
            appSettings.Verify(x => x.Save(), Times.AtLeastOnce);
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
            appSettings.Verify(x => x.Save(), Times.AtLeastOnce);
        }

        [Theory]
        [InlineData("id", Key.A, ModifierKeys.Alt, Key.S, ModifierKeys.Control | ModifierKeys.Shift, @"{""id"":{""Key"":""S"",""MouseAction"":""None"",""Modifiers"":""Control, Shift"",""Type"":""KeyGesture""}}")]
        [InlineData("id", Key.A, ModifierKeys.Alt, Key.A, ModifierKeys.Alt, "{}")]
        public void SaveSettings_IsCalled_ShouldSaveRemappedHotkeys(string id, Key key, ModifierKeys modifiers, Key newKey, ModifierKeys newModifiers, string expectedJsonString)
        {
            // Arrange            
            var appSettings = new Mock<IAppSettings>();
            appSettings.SetupAllProperties();

            var command = Mock.Of<ICommand>(c => c.CanExecute(It.IsAny<object>()) == true);

            var viewModel = GetViewModel(null, appSettings.Object);
            viewModel.HotkeyCommandManager.AddHotkey(id, new InputBinding(command, new PolyGesture(key, modifiers)));
            var hotkey = viewModel.HotkeyCommandManager.GetHotkey("id");
            var gesture = hotkey.Binding.Gesture as PolyGesture;
            gesture.Key = newKey;
            gesture.ModifierKeys = newModifiers;

            // Act
            viewModel.SaveSettings();

            // Assert
            Assert.Equal(expectedJsonString, appSettings.Object.HotkeyMappings);
            appSettings.Verify(x => x.Save(), Times.AtLeastOnce);
        }
        #endregion

        #region LoadSettings tests

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void LoadSettings_IsCalled_ShouldLoadShowInfluences(bool expectedShowInfluences)
        {
            // Arrange            
            var appSettings = new Mock<IAppSettings>();
            appSettings.SetupAllProperties();
            appSettings.Setup(x => x.ShowInfluences).Returns(() => expectedShowInfluences);

            var viewModel = GetViewModel(null, appSettings.Object);
            viewModel.CanvasShowInfluences = !expectedShowInfluences;

            // Act
            viewModel.LoadSettings();

            // Assert
            Assert.Equal(expectedShowInfluences, viewModel.CanvasShowInfluences);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void LoadSettings_IsCalled_ShouldLoadShowGrid(bool expectedShowGrid)
        {
            // Arrange            
            var appSettings = new Mock<IAppSettings>();
            appSettings.SetupAllProperties();
            appSettings.Setup(x => x.ShowGrid).Returns(() => expectedShowGrid);

            var viewModel = GetViewModel(null, appSettings.Object);
            viewModel.CanvasShowGrid = !expectedShowGrid;

            // Act
            viewModel.LoadSettings();

            // Assert
            Assert.Equal(expectedShowGrid, viewModel.CanvasShowGrid);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void LoadSettings_IsCalled_ShouldLoadShowIcons(bool expectedShowIcons)
        {
            // Arrange            
            var appSettings = new Mock<IAppSettings>();
            appSettings.SetupAllProperties();
            appSettings.Setup(x => x.ShowIcons).Returns(() => expectedShowIcons);

            var viewModel = GetViewModel(null, appSettings.Object);
            viewModel.CanvasShowIcons = !expectedShowIcons;

            // Act
            viewModel.LoadSettings();

            // Assert
            Assert.Equal(expectedShowIcons, viewModel.CanvasShowIcons);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void LoadSettings_IsCalled_ShouldLoadShowLabels(bool expectedShowLabels)
        {
            // Arrange            
            var appSettings = new Mock<IAppSettings>();
            appSettings.SetupAllProperties();
            appSettings.Setup(x => x.ShowLabels).Returns(() => expectedShowLabels);

            var viewModel = GetViewModel(null, appSettings.Object);
            viewModel.CanvasShowLabels = !expectedShowLabels;

            // Act
            viewModel.LoadSettings();

            // Assert
            Assert.Equal(expectedShowLabels, viewModel.CanvasShowLabels);
        }

        [Fact]
        public void LoadSettings_IsCalled_ShouldLoadMainWindowHeight()
        {
            // Arrange            
            var expectedMainWindowHeight = 42.4;

            var appSettings = new Mock<IAppSettings>();
            appSettings.SetupAllProperties();
            appSettings.Setup(x => x.MainWindowHeight).Returns(() => expectedMainWindowHeight);

            var viewModel = GetViewModel(null, appSettings.Object);

            // Act
            viewModel.LoadSettings();

            // Assert
            Assert.Equal(expectedMainWindowHeight, viewModel.MainWindowHeight);
        }

        [Fact]
        public void LoadSettings_IsCalled_ShouldLoadMainWindowWidth()
        {
            // Arrange            
            var expectedMainWindowWidth = 42.4;

            var appSettings = new Mock<IAppSettings>();
            appSettings.SetupAllProperties();
            appSettings.Setup(x => x.MainWindowWidth).Returns(() => expectedMainWindowWidth);

            var viewModel = GetViewModel(null, appSettings.Object);

            // Act
            viewModel.LoadSettings();

            // Assert
            Assert.Equal(expectedMainWindowWidth, viewModel.MainWindowWidth);
        }

        [Fact]
        public void LoadSettings_IsCalled_ShouldLoadMainWindowLeft()
        {
            // Arrange            
            var expectedMainWindowLeft = 42.4;

            var appSettings = new Mock<IAppSettings>();
            appSettings.SetupAllProperties();
            appSettings.Setup(x => x.MainWindowLeft).Returns(() => expectedMainWindowLeft);

            var viewModel = GetViewModel(null, appSettings.Object);

            // Act
            viewModel.LoadSettings();

            // Assert
            Assert.Equal(expectedMainWindowLeft, viewModel.MainWindowLeft);
        }

        [Fact]
        public void LoadSettings_IsCalled_ShouldLoadMainWindowTop()
        {
            // Arrange            
            var expectedMainWindowTop = 42.4;

            var appSettings = new Mock<IAppSettings>();
            appSettings.SetupAllProperties();
            appSettings.Setup(x => x.MainWindowTop).Returns(() => expectedMainWindowTop);

            var viewModel = GetViewModel(null, appSettings.Object);

            // Act
            viewModel.LoadSettings();

            // Assert
            Assert.Equal(expectedMainWindowTop, viewModel.MainWindowTop);
        }

        [Theory]
        [InlineData(System.Windows.WindowState.Maximized)]
        [InlineData(System.Windows.WindowState.Normal)]
        public void LoadSettings_IsCalled_ShouldLoadMainWindowWindowState(System.Windows.WindowState expectedMainMainWindowWindowState)
        {
            // Arrange            
            var appSettings = new Mock<IAppSettings>();
            appSettings.SetupAllProperties();
            appSettings.Setup(x => x.MainWindowWindowState).Returns(() => expectedMainMainWindowWindowState);

            var viewModel = GetViewModel(null, appSettings.Object);

            // Act
            viewModel.LoadSettings();

            // Assert
            Assert.Equal(expectedMainMainWindowWindowState, viewModel.MainWindowWindowState);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void LoadSettings_IsCalled_ShouldLoadEnableAutomaticUpdateCheck(bool expectedEnableAutomaticUpdateCheck)
        {
            // Arrange            
            var appSettings = new Mock<IAppSettings>();
            appSettings.SetupAllProperties();
            appSettings.Setup(x => x.EnableAutomaticUpdateCheck).Returns(() => expectedEnableAutomaticUpdateCheck);

            var viewModel = GetViewModel(null, appSettings.Object);
            viewModel.PreferencesUpdateViewModel.AutomaticUpdateCheck = !expectedEnableAutomaticUpdateCheck;

            // Act
            viewModel.LoadSettings();

            // Assert
            Assert.Equal(expectedEnableAutomaticUpdateCheck, viewModel.PreferencesUpdateViewModel.AutomaticUpdateCheck);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void LoadSettings_IsCalled_ShouldLoadUseCurrentZoomOnExportedImageValue(bool expectedUseCurrentZoomOnExportedImageValue)
        {
            // Arrange            
            var appSettings = new Mock<IAppSettings>();
            appSettings.SetupAllProperties();
            appSettings.Setup(x => x.UseCurrentZoomOnExportedImageValue).Returns(() => expectedUseCurrentZoomOnExportedImageValue);

            var viewModel = GetViewModel(null, appSettings.Object);
            viewModel.UseCurrentZoomOnExportedImageValue = !expectedUseCurrentZoomOnExportedImageValue;

            // Act
            viewModel.LoadSettings();

            // Assert
            Assert.Equal(expectedUseCurrentZoomOnExportedImageValue, viewModel.UseCurrentZoomOnExportedImageValue);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void LoadSettings_IsCalled_ShouldLoadRenderSelectionHighlightsOnExportedImageValue(bool expectedRenderSelectionHighlightsOnExportedImageValue)
        {
            // Arrange            
            var appSettings = new Mock<IAppSettings>();
            appSettings.SetupAllProperties();
            appSettings.Setup(x => x.RenderSelectionHighlightsOnExportedImageValue).Returns(() => expectedRenderSelectionHighlightsOnExportedImageValue);

            var viewModel = GetViewModel(null, appSettings.Object);
            viewModel.RenderSelectionHighlightsOnExportedImageValue = !expectedRenderSelectionHighlightsOnExportedImageValue;

            // Act
            viewModel.LoadSettings();

            // Assert
            Assert.Equal(expectedRenderSelectionHighlightsOnExportedImageValue, viewModel.RenderSelectionHighlightsOnExportedImageValue);
        }

        [Theory]
        [InlineData("id", Key.A, ModifierKeys.Alt, Key.S, ModifierKeys.Control | ModifierKeys.Shift, ExtendedMouseAction.None, GestureType.KeyGesture, @"{""id"":{""Key"":62,""MouseAction"":0,""Modifiers"":6,""Type"":1}}")]
        [InlineData("id", Key.A, ModifierKeys.Alt, Key.None, ModifierKeys.Shift, ExtendedMouseAction.LeftDoubleClick, GestureType.MouseGesture, @"{""id"":{""Key"":0,""MouseAction"":5,""Modifiers"":4,""Type"":0}}")]
        [InlineData("id", Key.A, ModifierKeys.Alt, Key.A, ModifierKeys.Alt, ExtendedMouseAction.None, GestureType.KeyGesture, "{}")]
        public void LoadSettings_IsCalled_ShouldLoadRemappedHotkeys(string id, Key key, ModifierKeys modifiers, Key expectedKey, ModifierKeys expectedModifiers, ExtendedMouseAction expectedMouseAction, GestureType expectedType, string settingsString)
        {
            // Arrange            
            var appSettings = new Mock<IAppSettings>();
            appSettings.SetupAllProperties();

            var command = Mock.Of<ICommand>(c => c.CanExecute(It.IsAny<object>()) == true);

            var viewModel = GetViewModel(null, appSettings.Object);
            appSettings.Setup(x => x.HotkeyMappings).Returns(settingsString);

            // Act
            viewModel.LoadSettings();

            viewModel.HotkeyCommandManager.AddHotkey(id, new InputBinding(command, new PolyGesture(key, modifiers)));
            var gesture = viewModel.HotkeyCommandManager.GetHotkey(id).Binding.Gesture as PolyGesture;
            // Assert
            Assert.Equal(expectedKey, gesture.Key);
            Assert.Equal(expectedModifiers, gesture.ModifierKeys);
            Assert.Equal(expectedMouseAction, gesture.MouseAction);
            Assert.Equal(expectedType, gesture.Type);
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
            commons.SetupGet(x => x.LanguageCodeMap).Returns(() => new Dictionary<string, string>());
            commons.Object.CurrentLanguage = languageBeforeChange;

            var viewModel = GetViewModel(commons.Object, null);
            viewModel.IsLanguageChange = true;

            var languageToSet = new SupportedLanguage("Deutsch");

            // Act
            var ex = Record.Exception(() => viewModel.LanguageSelectedCommand.Execute(languageToSet));

            // Assert
            Assert.Null(ex);
            Assert.Equal(languageBeforeChange, commons.Object.CurrentLanguage);
        }

        [Fact]
        public void LanguageSelectedCommand_IsExecutedWithSupportedLanguage_ShouldSetLanguage()
        {
            // Arrange
            var languageBeforeChange = "English";

            var commons = new Mock<ICommons>();
            commons.SetupAllProperties();
            commons.SetupGet(x => x.LanguageCodeMap).Returns(() => new Dictionary<string, string>());
            commons.Object.CurrentLanguage = languageBeforeChange;

            var viewModel = GetViewModel(commons.Object, null);

            var languageToSet = new SupportedLanguage("Deutsch");

            // Act
            viewModel.LanguageSelectedCommand.Execute(languageToSet);

            // Assert
            Assert.Equal(languageToSet.Name, commons.Object.CurrentLanguage);
        }

        [Fact]
        public void LanguageSelectedCommand_IsExecutedWithSupportedLanguage_ShouldSetIsLanguageChangeToFalse()
        {
            // Arrange
            var languageBeforeChange = "English";

            var commons = new Mock<ICommons>();
            commons.SetupAllProperties();
            commons.SetupGet(x => x.LanguageCodeMap).Returns(() => new Dictionary<string, string>());
            commons.Object.CurrentLanguage = languageBeforeChange;

            var viewModel = GetViewModel(commons.Object, null);

            var languageToSet = new SupportedLanguage("Deutsch");

            // Act
            viewModel.LanguageSelectedCommand.Execute(languageToSet);

            // Assert
            Assert.Equal(languageToSet.Name, commons.Object.CurrentLanguage);
            Assert.False(viewModel.IsLanguageChange);
        }

        #endregion
    }
}
