using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnnoDesigner.ViewModels;
using Xunit;

namespace AnnoDesigner.Tests
{
    public class LayoutSettingsViewModelTests
    {
        private LayoutSettingsViewModel GetViewModel(Version layoutVersionToUse = null)
        {
            return new LayoutSettingsViewModel
            {
                LayoutVersion = layoutVersionToUse ?? new Version(99, 99, 99, 99)
            };
        }

        #region ctor tests

        [Fact]
        public void Ctor_ShouldSetDefaultValues()
        {
            // Arrange/Act
            var viewModel = new LayoutSettingsViewModel();

            // Assert
            Assert.Equal(new Version(1, 0, 0, 0), viewModel.LayoutVersion);
            Assert.NotNull(viewModel.LayoutVersionDisplayValue);
        }

        #endregion

        #region LayoutVersion tests        

        [Fact]
        public void LayoutVersion_ValueIsChanged_ShouldRaisePropertyChangedEvent()
        {
            // Arrange
            var viewModel = GetViewModel();

            // Act/Assert
            Assert.PropertyChanged(viewModel, nameof(viewModel.LayoutVersion), () => viewModel.LayoutVersion = new Version(42, 42, 42, 42));
        }

        [Fact]
        public void LayoutVersion_ValueIsChanged_ShouldRaisePropertyChangedEventForLayoutVersionDisplayValue()
        {
            // Arrange
            var viewModel = GetViewModel();

            // Act/Assert
            Assert.PropertyChanged(viewModel, nameof(viewModel.LayoutVersionDisplayValue), () => viewModel.LayoutVersion = new Version(42, 42, 42, 42));
        }

        #endregion

        #region LayoutVersionDisplayValue tests

        [Fact]
        public void LayoutVersionDisplayValue_ValueIsChangedToValid_ShouldRaisePropertyChangedEvent()
        {
            // Arrange
            var viewModel = GetViewModel();

            // Act/Assert
            Assert.PropertyChanged(viewModel, nameof(viewModel.LayoutVersionDisplayValue), () => viewModel.LayoutVersionDisplayValue = "42.42.42.42");
        }

        [Fact]
        public void LayoutVersionDisplayValue_ValueIsChangedToValid_ShouldRaisePropertyChangedEventForLayoutVersion()
        {
            // Arrange
            var viewModel = GetViewModel();

            // Act/Assert
            Assert.PropertyChanged(viewModel, nameof(viewModel.LayoutVersion), () => viewModel.LayoutVersionDisplayValue = "42.42.42.42");
        }

        [Fact]
        public void LayoutVersionDisplayValue_ValueIsChangedToValid_ShouldSetLayoutVersion()
        {
            // Arrange
            var viewModel = GetViewModel();

            // Act
            viewModel.LayoutVersionDisplayValue = "42.42.42.42";

            // Assert
            Assert.Equal(new Version(42, 42, 42, 42), viewModel.LayoutVersion);
        }

        [Fact]
        public void LayoutVersionDisplayValue_ValueIsChangedToInvalid_ShouldNotSetLayoutVersionAndNotThrow()
        {
            // Arrange
            var viewModel = GetViewModel();
            var oldVersion = viewModel.LayoutVersion;

            // Act
            viewModel.LayoutVersionDisplayValue = "not a valid version";

            // Assert
            Assert.Equal(oldVersion, viewModel.LayoutVersion);
        }

        #endregion
    }
}
