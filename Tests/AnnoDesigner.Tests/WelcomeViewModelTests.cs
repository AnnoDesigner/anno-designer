using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnnoDesigner.Core.Models;
using AnnoDesigner.model;
using AnnoDesigner.viewmodel;
using Moq;
using Xunit;

namespace AnnoDesigner.Tests
{
    public class WelcomeViewModelTests
    {
        private readonly ICommons _mockedCommons;
        private readonly IAppSettings _mockedAppSettings;

        public WelcomeViewModelTests()
        {
            _mockedCommons = new Mock<ICommons>().Object;
            _mockedAppSettings = new Mock<IAppSettings>().Object;
        }

        private WelcomeViewModel GetViewModel(ICommons commonsToUse = null, IAppSettings appSettingsToUse = null)
        {
            return new WelcomeViewModel(commonsToUse ?? _mockedCommons, appSettingsToUse ?? _mockedAppSettings);
        }

        #region ctor tests

        [Fact]
        public void Ctor_ShouldSetDefaultValues()
        {
            // Arrange/Act
            var viewModel = GetViewModel();

            // Assert
            Assert.Null(viewModel.SelectedItem);
            Assert.NotNull(viewModel.ContinueCommand);
            Assert.NotNull(viewModel.Languages);
        }

        [Fact]
        public void Ctor_ShouldSetCorrectNumberOfLanguages()
        {
            // Arrange/Act
            var viewModel = GetViewModel();

            // Assert
            Assert.Equal(5, viewModel.Languages.Count);
        }

        #endregion

        #region ContinueCommand tests

        [Fact]
        public void ContinueCommand_SelectedItemIsNull_ShouldNotCanExecute()
        {
            // Arrange
            var viewModel = GetViewModel();

            // Act
            var result = viewModel.ContinueCommand.CanExecute(null);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ContinueCommand_SelectedItemIsNotNull_ShouldCanExecute()
        {
            // Arrange
            var viewModel = GetViewModel();
            viewModel.SelectedItem = viewModel.Languages[0];

            // Act
            var result = viewModel.ContinueCommand.CanExecute(null);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ContinueCommand_IsExecuted_ShouldSetSelectedLanguageInCommons()
        {
            // Arrange
            var commons = new Mock<ICommons>();
            commons.SetupAllProperties();

            var viewModel = GetViewModel(commons.Object);
            viewModel.SelectedItem = viewModel.Languages[1];

            var expectedLanguage = viewModel.SelectedItem.Name;

            // Act
            viewModel.ContinueCommand.Execute(null);

            // Assert
            Assert.Equal(expectedLanguage, commons.Object.SelectedLanguage);
        }

        [Fact]
        public void ContinueCommand_IsExecuted_ShouldSetSelectedLanguageInAppSettings()
        {
            // Arrange            
            var commons = new Mock<ICommons>();
            commons.SetupAllProperties();

            var appSettings = new Mock<IAppSettings>();
            appSettings.SetupAllProperties();

            var viewModel = GetViewModel(commons.Object, appSettings.Object);
            viewModel.SelectedItem = viewModel.Languages[1];

            var expectedLanguage = viewModel.SelectedItem.Name;

            // Act
            viewModel.ContinueCommand.Execute(null);

            // Assert
            Assert.Equal(expectedLanguage, appSettings.Object.SelectedLanguage);
        }

        [Fact]
        public void ContinueCommand_IsExecuted_ShouldSaveAppSettings()
        {
            // Arrange            
            var appSettings = new Mock<IAppSettings>();
            appSettings.SetupAllProperties();

            var viewModel = GetViewModel(_mockedCommons, appSettings.Object);
            viewModel.SelectedItem = viewModel.Languages[1];

            var expectedLanguage = viewModel.SelectedItem.Name;

            // Act
            viewModel.ContinueCommand.Execute(null);

            // Assert
            appSettings.Verify(x => x.Save(), Times.Once);
        }

        [Fact]
        public void ContinueCommand_IsExecutedWithICloseable_ShouldCallClose()
        {
            // Arrange
            var viewModel = GetViewModel();
            viewModel.SelectedItem = viewModel.Languages[1];

            var closeable = new Mock<ICloseable>();

            // Act
            viewModel.ContinueCommand.Execute(closeable.Object);

            // Assert
            closeable.Verify(x => x.Close(), Times.Once);
        }

        [Fact]
        public void ContinueCommand_IsExecutedWithoutICloseable_ShouldNotThrow()
        {
            // Arrange
            var viewModel = GetViewModel();
            viewModel.SelectedItem = viewModel.Languages[1];

            // Act
            var ex = Record.Exception(() => viewModel.ContinueCommand.Execute(null));

            // Assert
            Assert.Null(ex);
        }

        #endregion
    }
}
