using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Models;
using AnnoDesigner.ViewModels;
using Moq;
using Xunit;

namespace AnnoDesigner.Tests
{
    public class AboutViewModelTests
    {
        private AboutViewModel GetViewModel()
        {
            return new AboutViewModel();
        }

        #region CloseWindowCommand tests

        [Fact(Skip = "needs abstraction for localization")]
        public void CloseWindowCommand_IsExecutedWithICloseable_ShouldCallClose()
        {
            // Arrange
            var viewModel = GetViewModel();

            var closeable = new Mock<ICloseable>();

            // Act
            viewModel.CloseWindowCommand.Execute(closeable.Object);

            // Assert
            closeable.Verify(x => x.Close(), Times.Once);
        }

        [Fact(Skip = "needs abstraction for localization")]
        public void CloseWindowCommand_IsExecutedWithoutICloseable_ShouldNotThrow()
        {
            // Arrange
            var viewModel = GetViewModel();

            // Act
            var ex = Record.Exception(() => viewModel.CloseWindowCommand.Execute(null));

            // Assert
            Assert.Null(ex);
        }

        #endregion
    }
}
