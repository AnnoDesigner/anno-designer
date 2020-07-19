using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Core.Presets.Models;
using AnnoDesigner.Core.Services;
using AnnoDesigner.Models;
using AnnoDesigner.ViewModels;
using Moq;
using Xunit;

namespace AnnoDesigner.Tests
{
    public class BuildingSettingsViewModelTests
    {
        private readonly IAppSettings _mockedAppSettings;
        private readonly IMessageBoxService _mockedMessageBoxService;
        private static ILocalizationHelper _mockedLocalization;
        private readonly ICommons _mockedCommons;

        public BuildingSettingsViewModelTests()
        {
            var commonsMock = new Mock<ICommons>();
            commonsMock.SetupGet(x => x.CurrentLanguage).Returns(() => "English");
            commonsMock.SetupGet(x => x.CurrentLanguageCode).Returns(() => "eng");
            _mockedCommons = commonsMock.Object;

            var mockedLocalizationHelper = new Mock<ILocalizationHelper>();
            mockedLocalizationHelper.Setup(x => x.GetLocalization(It.IsAny<string>())).Returns<string>(x => x);
            mockedLocalizationHelper.Setup(x => x.GetLocalization(It.IsAny<string>(), It.IsAny<string>())).Returns((string value, string langauge) => value);
            _mockedLocalization = mockedLocalizationHelper.Object;

            Localization.Localization.Init(commonsMock.Object);

            _mockedAppSettings = new Mock<IAppSettings>().Object;
            _mockedMessageBoxService = new Mock<IMessageBoxService>().Object;
        }

        private BuildingSettingsViewModel GetViewModel(IAppSettings appSettingsToUse = null)
        {
            return new BuildingSettingsViewModel(appSettingsToUse ?? _mockedAppSettings,
                _mockedMessageBoxService,
                _mockedLocalization);
        }

        #region ctor tests

        [Fact]
        public void Ctor_ShouldSetDefaultValues()
        {
            // Arrange/Act
            var viewModel = GetViewModel();

            // Assert
            Assert.NotNull(viewModel.ApplyColorToSelectionCommand);
            Assert.NotNull(viewModel.ApplyPredefinedColorToSelectionCommand);
            Assert.NotNull(viewModel.UseColorInLayoutCommand);
            Assert.NotNull(viewModel.ColorsInLayout);
            Assert.NotNull(viewModel.BuildingInfluences);

            Assert.Equal(BuildingInfluenceType.None, viewModel.SelectedBuildingInfluence.Type);
            Assert.Equal(Colors.Red, viewModel.SelectedColor);
            Assert.Equal(4, viewModel.BuildingHeight);
            Assert.Equal(4, viewModel.BuildingWidth);
            Assert.Equal(string.Empty, viewModel.BuildingTemplate);
            Assert.Equal(string.Empty, viewModel.BuildingName);
            Assert.Equal(string.Empty, viewModel.BuildingIdentifier);
            Assert.Equal(0, viewModel.BuildingRadius);
            Assert.Equal(0, viewModel.BuildingInfluenceRange);

            Assert.False(viewModel.IsEnableLabelChecked);
            Assert.False(viewModel.IsBorderlessChecked);
            Assert.False(viewModel.IsRoadChecked);
        }

        [Fact]
        public void Ctor_ShouldSetCorrectNumberOfBuildingInfluences()
        {
            // Arrange/Act
            var viewModel = GetViewModel();
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
            var viewModel = GetViewModel();
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
            var viewModel = GetViewModel();
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
            var viewModel = GetViewModel();
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
            var viewModel = GetViewModel();
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
            var viewModel = GetViewModel();
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
            var viewModel = GetViewModel();
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
            var viewModel = GetViewModel();
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
            var viewModel = GetViewModel();

            // Act
            var result = viewModel.UseColorInLayoutCommand.CanExecute(null);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void UseColorInLayoutCommand_IsExecutedWithNull_ShouldNotThrow()
        {
            // Arrange
            var viewModel = GetViewModel();
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
            var viewModel = GetViewModel();
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
            var viewModel = GetViewModel();
            var expectedColor = new SerializableColor(Colors.LimeGreen);

            // Act
            viewModel.UseColorInLayoutCommand.Execute(expectedColor);

            // Assert
            Assert.Equal(expectedColor, viewModel.SelectedColor);
        }

        #endregion

        #region ShowColorsInLayout tests

        [Fact]
        public void ShowColorsInLayout_ColorsInLayoutIsNull_ShouldReturnFalse()
        {
            // Arrange
            var viewModel = GetViewModel();
            viewModel.ColorsInLayout = null;

            // Act/Assert
            Assert.False(viewModel.ShowColorsInLayout);
        }

        [Fact]
        public void ShowColorsInLayout_ColorsInLayoutIsEmpty_ShouldReturnFalse()
        {
            // Arrange
            var viewModel = GetViewModel();
            viewModel.ColorsInLayout = new ObservableCollection<SerializableColor>();

            // Act/Assert
            Assert.False(viewModel.ShowColorsInLayout);
        }

        [Fact]
        public void ShowColorsInLayout_ColorsInLayoutContainsItem_ShouldReturnTrue()
        {
            // Arrange
            var viewModel = GetViewModel();
            var colorsInLayout = new ObservableCollection<SerializableColor>();
            colorsInLayout.Add(new SerializableColor());

            viewModel.ColorsInLayout = colorsInLayout;

            // Act/Assert
            Assert.True(viewModel.ShowColorsInLayout);
        }

        #endregion

        #region ColorsInLayout tests

        [Fact]
        public void ColorsInLayout_IsSet_ShouldNotifyShowColorsInLayout()
        {
            // Arrange
            var viewModel = GetViewModel();

            // Act/Assert
            Assert.PropertyChanged(viewModel,
                nameof(BuildingSettingsViewModel.ShowColorsInLayout),
                () => viewModel.ColorsInLayout = new ObservableCollection<SerializableColor>());
        }

        #endregion
    }
}
