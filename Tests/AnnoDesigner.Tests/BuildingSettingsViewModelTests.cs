using System;
using System.Linq;
using AnnoDesigner.model;
using AnnoDesigner.viewmodel;
using System.Windows.Media;
using Xunit;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Core.Presets.Models;
using Moq;

namespace AnnoDesigner.Tests
{
    public class BuildingSettingsViewModelTests
    {
        private readonly ICommons _mockedCommons;

        public BuildingSettingsViewModelTests()
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
            var viewModel = new BuildingSettingsViewModel(_mockedCommons);

            // Assert
            Assert.NotNull(viewModel.ApplyColorToSelectionCommand);
            Assert.NotNull(viewModel.ApplyPredefinedColorToSelectionCommand);
            Assert.NotNull(viewModel.UseColorInLayoutCommand);
            Assert.Equal(Colors.Red, viewModel.SelectedColor);
            Assert.NotNull(viewModel.ColorsInLayout);
            Assert.NotNull(viewModel.BuildingInfluences);
            Assert.Equal(BuildingInfluenceType.None, viewModel.SelectedBuildingInfluence.Type);
        }

        [Fact]
        public void Ctor_ShouldSetCorrectNumberOfBuildingInfluences()
        {
            // Arrange/Act
            var viewModel = new BuildingSettingsViewModel(_mockedCommons);
            var expectedCount = Enum.GetValues(typeof(BuildingInfluenceType)).Length;

            // Assert
            Assert.Equal(expectedCount, viewModel.BuildingInfluences.Count);
        }

        #endregion

        #region BuildingInfluence tests

        [Theory]
        [InlineData(BuildingInfluenceType.None, false, false)]
        [InlineData(BuildingInfluenceType.Radius, true, false)]
        [InlineData(BuildingInfluenceType.Distance, false, true)]
        [InlineData(BuildingInfluenceType.Both, true, true)]
        public void SelectedBuildingInfluence_TypeIsGiven_ShouldAdjustInputVisibility(BuildingInfluenceType typeToSet, bool expectedRadiusVisible, bool expectedRangeVisible)
        {
            // Arrange
            var viewModel = new BuildingSettingsViewModel(_mockedCommons);
            //to trigger PropertyChanged
            if (typeToSet != BuildingInfluenceType.None)
            {
                viewModel.SelectedBuildingInfluence = viewModel.BuildingInfluences.Single(x => x.Type == BuildingInfluenceType.None);
            }
            else
            {
                viewModel.SelectedBuildingInfluence = viewModel.BuildingInfluences.Single(x => x.Type == BuildingInfluenceType.Both);
            }
            viewModel.IsBuildingInfluenceInputRadiusVisible = !expectedRadiusVisible;
            viewModel.IsBuildingInfluenceInputRangeVisible = !expectedRangeVisible;

            // Act
            viewModel.SelectedBuildingInfluence = viewModel.BuildingInfluences.Single(x => x.Type == typeToSet);

            // Assert
            Assert.Equal(expectedRadiusVisible, viewModel.IsBuildingInfluenceInputRadiusVisible);
            Assert.Equal(expectedRangeVisible, viewModel.IsBuildingInfluenceInputRangeVisible);
        }

        #endregion

        #region GetDistanceRange tests

        [Fact]
        public void GetDistanceRange_BuildingInfoIsNull_ShouldSetInfluenceRangeToZeroAndReturnFalse()
        {
            // Arrange
            var viewModel = new BuildingSettingsViewModel(_mockedCommons);
            viewModel.BuildingInfluenceRange = -1;

            // Act
            var result = viewModel.GetDistanceRange(true, null);

            // Assert
            Assert.False(result);
            Assert.Equal(0, viewModel.BuildingInfluenceRange);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        public void GetDistanceRange_BuildingInfluenceRangeIsZeroOrLower_ShouldSetInfluenceRangeToZeroAndReturnFalse(int influenceRangeToSet)
        {
            // Arrange
            var viewModel = new BuildingSettingsViewModel(_mockedCommons);
            viewModel.BuildingInfluenceRange = int.MaxValue;

            var mockedBuildingInfo = new Mock<IBuildingInfo>();
            mockedBuildingInfo.SetupGet(x => x.InfluenceRange).Returns(() => influenceRangeToSet);

            // Act
            var result = viewModel.GetDistanceRange(true, mockedBuildingInfo.Object);

            // Assert
            Assert.False(result);
            Assert.Equal(0, viewModel.BuildingInfluenceRange);
        }

        [Theory]
        [InlineData(0, 0, false)]
        [InlineData(1, 0, false)]
        [InlineData(2, 0, false)]
        [InlineData(3, 1, true)]
        [InlineData(10, 8, true)]
        public void GetDistanceRange_IsNotPavedStreet_ShouldSetInfluenceRangeCorrect(int influenceRangeToSet, int expectedInfluenceRange, bool expectedResult)
        {
            // Arrange
            var viewModel = new BuildingSettingsViewModel(_mockedCommons);
            viewModel.BuildingInfluenceRange = int.MaxValue;

            var mockedBuildingInfo = new Mock<IBuildingInfo>();
            mockedBuildingInfo.SetupGet(x => x.InfluenceRange).Returns(() => influenceRangeToSet);

            // Act
            var result = viewModel.GetDistanceRange(false, mockedBuildingInfo.Object);

            // Assert
            Assert.Equal(expectedResult, result);
            Assert.Equal(expectedInfluenceRange, viewModel.BuildingInfluenceRange);
        }

        [Theory]
        [InlineData(0, 0, false)]
        [InlineData(1, 0, false)]
        [InlineData(2, 1, true)]
        [InlineData(3, 3, true)]
        [InlineData(10, 13, true)]
        [InlineData(26, 36, true)]
        [InlineData(30, 41, true)]
        [InlineData(35, 49, true)]
        [InlineData(40, 56, true)]
        [InlineData(45, 63, true)]
        public void GetDistanceRange_IsPavedStreet_ShouldSetInfluenceRangeCorrect(int influenceRangeToSet, int expectedInfluenceRange, bool expectedResult)
        {
            // Arrange
            var viewModel = new BuildingSettingsViewModel(_mockedCommons);
            viewModel.BuildingInfluenceRange = int.MaxValue;

            var mockedBuildingInfo = new Mock<IBuildingInfo>();
            mockedBuildingInfo.SetupGet(x => x.InfluenceRange).Returns(() => influenceRangeToSet);
            mockedBuildingInfo.SetupGet(x => x.Template).Returns(() => "dummy");

            // Act
            var result = viewModel.GetDistanceRange(true, mockedBuildingInfo.Object);

            // Assert
            Assert.Equal(expectedResult, result);
            Assert.Equal(expectedInfluenceRange, viewModel.BuildingInfluenceRange);
        }

        [Theory]
        [InlineData(0, 0, false)]
        [InlineData(1, 0, false)]
        [InlineData(2, 1, true)]
        [InlineData(3, 3, true)]
        [InlineData(10, 12, true)]
        [InlineData(26, 34, true)]
        [InlineData(30, 40, true)]
        [InlineData(35, 47, true)]
        [InlineData(40, 54, true)]
        [InlineData(45, 61, true)]
        public void GetDistanceRange_IsPavedStreetAndBuildingIsCityInstitutionBuilding_ShouldSetInfluenceRangeCorrect(int influenceRangeToSet, int expectedInfluenceRange, bool expectedResult)
        {
            // Arrange
            var viewModel = new BuildingSettingsViewModel(_mockedCommons);
            viewModel.BuildingInfluenceRange = int.MaxValue;

            var mockedBuildingInfo = new Mock<IBuildingInfo>();
            mockedBuildingInfo.SetupGet(x => x.InfluenceRange).Returns(() => influenceRangeToSet);
            mockedBuildingInfo.SetupGet(x => x.Template).Returns(() => "CityInstitutionBuilding");

            // Act
            var result = viewModel.GetDistanceRange(true, mockedBuildingInfo.Object);

            // Assert
            Assert.Equal(expectedResult, result);
            Assert.Equal(expectedInfluenceRange, viewModel.BuildingInfluenceRange);
        }

        [Theory]
        [InlineData(0, 0, false)]
        [InlineData(1, 0, false)]
        [InlineData(2, 1, true)]
        [InlineData(3, 3, true)]
        [InlineData(10, 12, true)]
        [InlineData(26, 34, true)]
        [InlineData(30, 40, true)]
        [InlineData(35, 47, true)]
        [InlineData(40, 54, true)]
        [InlineData(45, 61, true)]
        public void GetDistanceRange_IsPavedStreetAndBuildingIsCityInstitutionBuilding_ShouldIgnoreCase(int influenceRangeToSet, int expectedInfluenceRange, bool expectedResult)
        {
            // Arrange
            var viewModel = new BuildingSettingsViewModel(_mockedCommons);
            viewModel.BuildingInfluenceRange = int.MaxValue;

            var mockedBuildingInfo = new Mock<IBuildingInfo>();
            mockedBuildingInfo.SetupGet(x => x.InfluenceRange).Returns(() => influenceRangeToSet);
            mockedBuildingInfo.SetupGet(x => x.Template).Returns(() => "citYInsTitutIOnbuiLding");

            // Act
            var result = viewModel.GetDistanceRange(true, mockedBuildingInfo.Object);

            // Assert
            Assert.Equal(expectedResult, result);
            Assert.Equal(expectedInfluenceRange, viewModel.BuildingInfluenceRange);
        }

        #endregion

        #region UseColorInLayoutCommand tests        

        [Fact]
        public void UseColorInLayoutCommand_ShouldCanExecute()
        {
            // Arrange
            var viewModel = new BuildingSettingsViewModel(_mockedCommons);

            // Act
            var result = viewModel.UseColorInLayoutCommand.CanExecute(null);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void UseColorInLayoutCommand_IsExecutedWithNull_ShouldNotThrow()
        {
            // Arrange
            var viewModel = new BuildingSettingsViewModel(_mockedCommons);
            var expectedColor = viewModel.SelectedColor;

            // Act
            viewModel.UseColorInLayoutCommand.Execute(null);

            // Assert
            Assert.Equal(expectedColor, viewModel.SelectedColor);
        }

        [Fact]
        public void UseColorInLayoutCommand_IsExecutedWithColor_ShouldSetSelectedColor()
        {
            // Arrange
            var viewModel = new BuildingSettingsViewModel(_mockedCommons);
            var expectedColor = Colors.LimeGreen;

            // Act
            viewModel.UseColorInLayoutCommand.Execute(expectedColor);

            // Assert
            Assert.Equal(expectedColor, viewModel.SelectedColor);
        }

        [Fact]
        public void UseColorInLayoutCommand_IsExecutedWithSerializableColor_ShouldSetSelectedColor()
        {
            // Arrange
            var viewModel = new BuildingSettingsViewModel(_mockedCommons);
            var expectedColor = new SerializableColor(Colors.LimeGreen);

            // Act
            viewModel.UseColorInLayoutCommand.Execute(expectedColor);

            // Assert
            Assert.Equal(expectedColor, viewModel.SelectedColor);
        }

        #endregion
    }
}
