using System;
using AnnoDesigner.Undo.Operations;
using AnnoDesigner.ViewModels;
using Xunit;

namespace AnnoDesigner.Tests.Undo
{
    public class ModifyLayoutVersionOperationTests
    {
        #region Undo tests

        [Fact]
        public void Undo_LayoutVersion_ShouldBeUpdated()
        {
            // Arrange
            var viewModel = new LayoutSettingsViewModel();
            var operation = new ModifyLayoutVersionOperation()
            {
                LayoutSettingsViewModel = viewModel,
                OldValue = new Version(1, 0, 0, 0),
                NewValue = new Version(42, 42, 42, 42)
            };

            // Act
            operation.Undo();

            // Assert
            Assert.Equal(new Version(1, 0, 0, 0), viewModel.LayoutVersion);
        }

        #endregion

        #region Undo tests

        [Fact]
        public void Rodo_LayoutVersion_ShouldBeUpdated()
        {
            // Arrange
            var viewModel = new LayoutSettingsViewModel();
            var operation = new ModifyLayoutVersionOperation()
            {
                LayoutSettingsViewModel = viewModel,
                OldValue = new Version(1, 0, 0, 0),
                NewValue = new Version(42, 42, 42, 42)
            };

            // Act
            operation.Redo();

            // Assert
            Assert.Equal(new Version(42, 42, 42, 42), viewModel.LayoutVersion);
        }

        #endregion
    }
}
