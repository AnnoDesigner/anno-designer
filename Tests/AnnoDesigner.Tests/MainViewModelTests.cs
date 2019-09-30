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

            Assert.Null(viewModel.VersionValue);
            Assert.Null(viewModel.FileVersionValue);
            Assert.Null(viewModel.PresetsVersionValue);
            Assert.Null(viewModel.StatusMessage);
            Assert.Null(viewModel.StatusMessageClipboard);

            Assert.NotNull(viewModel.AvailableIcons);
            Assert.NotNull(viewModel.SelectedIcon);
            Assert.NotNull(viewModel.Languages);
            Assert.NotNull(viewModel.MainWindowTitle);
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
    }
}
