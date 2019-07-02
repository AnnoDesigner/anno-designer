using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnnoDesigner.viewmodel;
using Xunit;

namespace AnnoDesigner.Tests
{
    public class WelcomeViewModelTests
    {
        #region ctor tests

        [Fact]
        public void Ctor_ShouldSetDefaultValues()
        {
            // Arrange/Act
            var viewModel = new WelcomeViewModel();

            // Assert
            Assert.Null(viewModel.SelectedItem);
            Assert.NotNull(viewModel.ContinueCommand);
            Assert.NotNull(viewModel.Languages);
        }

        [Fact]
        public void Ctor_ShouldSetCorrectNumberOfLanguages()
        {
            // Arrange/Act
            var viewModel = new WelcomeViewModel();

            // Assert
            Assert.Equal(5, viewModel.Languages.Count);
        }

        #endregion

        #region ContinueCommand tests

        [Fact]
        public void ContinueCommand_SelectedItemIsNull_ShouldNotCanExecute()
        {
            // Arrange
            var viewModel = new WelcomeViewModel();

            // Act
            var result = viewModel.ContinueCommand.CanExecute(null);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ContinueCommand_SelectedItemIsNotNull_ShouldCanExecute()
        {
            // Arrange
            var viewModel = new WelcomeViewModel();
            viewModel.SelectedItem = viewModel.Languages[0];

            // Act
            var result = viewModel.ContinueCommand.CanExecute(null);

            // Assert
            Assert.True(result);
        }

        #endregion
    }
}
