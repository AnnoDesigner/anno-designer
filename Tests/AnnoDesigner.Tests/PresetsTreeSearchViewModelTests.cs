using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnnoDesigner.viewmodel;
using Xunit;

namespace AnnoDesigner.Tests
{
    public class PresetsTreeSearchViewModelTests
    {
        #region ctor tests

        [Fact]
        public void Ctor_ShouldSetDefaultValues()
        {
            // Arrange/Act
            var viewModel = new PresetsTreeSearchViewModel();

            // Assert
            Assert.False(viewModel.HasFocus);
            Assert.Empty(viewModel.SearchText);
            Assert.NotNull(viewModel.ClearSearchTextCommand);
            Assert.NotNull(viewModel.GotFocusCommand);
            Assert.NotNull(viewModel.LostFocusCommand);
        }

        #endregion

        #region GotFocusCommand tests

        [Fact]
        public void GotFocusCommand_ShouldCanExecute()
        {
            // Arrange
            var viewModel = new PresetsTreeSearchViewModel();

            // Act
            var result = viewModel.GotFocusCommand.CanExecute(null);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void GotFocusCommand_IsExecuted_ShouldSetHasFocusTrue()
        {
            // Arrange
            var viewModel = new PresetsTreeSearchViewModel();
            viewModel.HasFocus = false;

            // Act
            viewModel.GotFocusCommand.Execute(null);

            // Assert
            Assert.True(viewModel.HasFocus);
        }

        #endregion

        #region LostFocusCommand tests

        [Fact]
        public void LostFocusCommand_ShouldCanExecute()
        {
            // Arrange
            var viewModel = new PresetsTreeSearchViewModel();

            // Act
            var result = viewModel.LostFocusCommand.CanExecute(null);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void LostFocusCommand_IsExecuted_ShouldSetHasFocusFalse()
        {
            // Arrange
            var viewModel = new PresetsTreeSearchViewModel();
            viewModel.HasFocus = true;

            // Act
            viewModel.LostFocusCommand.Execute(null);

            // Assert
            Assert.False(viewModel.HasFocus);
        }

        #endregion

        #region ClearSearchTextCommand tests

        [Fact]
        public void ClearSearchTextCommand_ShouldCanExecute()
        {
            // Arrange
            var viewModel = new PresetsTreeSearchViewModel();

            // Act
            var result = viewModel.ClearSearchTextCommand.CanExecute(null);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ClearSearchTextCommand_IsExecuted_ShouldSetSearchTextEmpty()
        {
            // Arrange
            var viewModel = new PresetsTreeSearchViewModel();
            viewModel.SearchText = "dummy";

            // Act
            viewModel.ClearSearchTextCommand.Execute(null);

            // Assert
            Assert.Empty(viewModel.SearchText);
        }

        #endregion
    }
}
