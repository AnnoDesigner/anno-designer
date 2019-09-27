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

        public MainViewModelTests()
        {
            var commonsMock = new Mock<ICommons>();
            commonsMock.SetupGet(x => x.SelectedLanguage).Returns(() => "English");
            _mockedCommons = commonsMock.Object;
        }

        #region ctor tests

        [Fact]
        public void Ctor_ShouldSetDefaultValues()
        {
            // Arrange/Act
            var viewModel = new MainViewModel(_mockedCommons);

            // Assert
            Assert.NotNull(viewModel.OpenProjectHomepageCommand);
            Assert.NotNull(viewModel.CloseWindowCommand);
            Assert.NotNull(viewModel.CanvasResetZoomCommand);
            Assert.NotNull(viewModel.CanvasNormalizeCommand);

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

            Assert.Null(viewModel.VersionValue);
            Assert.Null(viewModel.FileVersionValue);
            Assert.Null(viewModel.PresetsVersionValue);

            Assert.False(viewModel.UseCurrentZoomOnExportedImageValue);
            Assert.False(viewModel.RenderSelectionHighlightsOnExportedImageValue);
        }

        #endregion

        #region CloseWindowCommand tests

        [Fact]
        public void CloseWindowCommand_ShouldCanExecute()
        {
            // Arrange
            var viewModel = new MainViewModel(_mockedCommons);

            // Act
            var result = viewModel.CloseWindowCommand.CanExecute(null);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CloseWindowCommand_IsExecutedWithNull_ShouldNotThrow()
        {
            // Arrange
            var viewModel = new MainViewModel(_mockedCommons);

            // Act
            var ex = Record.Exception(() => viewModel.CloseWindowCommand.Execute(null));

            // Assert
            Assert.Null(ex);
        }

        [Fact]
        public void CloseWindowCommand_IsExecutedWithICloseable_ShouldCallClose()
        {
            // Arrange
            var viewModel = new MainViewModel(_mockedCommons);
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
            var viewModel = new MainViewModel(_mockedCommons);
            viewModel.PresetsTreeViewModel.FilterText = "Lorem";

            var textToSet = "dummy";

            // Act
            viewModel.PresetsTreeSearchViewModel.SearchText = textToSet;

            // Assert
            Assert.Equal(textToSet, viewModel.PresetsTreeViewModel.FilterText);
        }

        #endregion
    }
}
