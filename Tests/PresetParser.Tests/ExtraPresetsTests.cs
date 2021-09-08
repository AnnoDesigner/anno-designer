using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PresetParser.Tests
{
    public class ExtraPresetsTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData(" ")]
        [InlineData("")]
        public void GetExtraPresets_AnnoVersionIsNullOrWhiteSpace_ShouldReturnEmptyList(string annoVersion)
        {
            // Arrange/Act
            var result = ExtraPresets.GetExtraPresets(annoVersion);

            // Assert
            Assert.Empty(result);
        }

        [Theory]
        [InlineData(Constants.ANNO_VERSION_1404, 4)]
        [InlineData(Constants.ANNO_VERSION_2070, 5)]
        [InlineData(Constants.ANNO_VERSION_1800, 25)]
        [InlineData(Constants.ANNO_VERSION_2205, 0)]
        public void GetExtraPresets_AnnoVersionIsKnown_ShouldReturnCorrectExtraPresetsCount(string annoVersion, int expectedCount)
        {
            // Arrange/Act
            var result = ExtraPresets.GetExtraPresets(annoVersion).ToList();

            // Assert
            Assert.Equal(expectedCount, result.Count);
        }

        [Theory]
        [InlineData(Constants.ANNO_VERSION_1404)]
        [InlineData(Constants.ANNO_VERSION_2070)]
        [InlineData(Constants.ANNO_VERSION_1800)]
        [InlineData(Constants.ANNO_VERSION_2205)]
        public void GetExtraPresets_EveryElement_ShouldContainValuesForAllLocalizations(string annoVersion)
        {
            // Arrange/Act
            var result = ExtraPresets.GetExtraPresets(annoVersion).ToList();

            // Assert
            Assert.All(result, x =>
            {
                Assert.False(string.IsNullOrWhiteSpace(x.LocaEng));
                Assert.False(string.IsNullOrWhiteSpace(x.LocaEsp));
                Assert.False(string.IsNullOrWhiteSpace(x.LocaFra));
                Assert.False(string.IsNullOrWhiteSpace(x.LocaGer));
                Assert.False(string.IsNullOrWhiteSpace(x.LocaPol));
                Assert.False(string.IsNullOrWhiteSpace(x.LocaRus));
            });
        }

        [Fact]
        public void GetExtraRoads_ShouldReturnCorrectExtraRoadsCount()
        {
            // Arrange
            var expectedCount = 17;

            // Act
            var result = ExtraPresets.GetExtraRoads().ToList();

            // Assert
            Assert.Equal(expectedCount, result.Count);
        }

        [Fact]
        public void GetExtraRoads_EveryElement_ShouldContainValuesForAllLocalizations()
        {
            // Arrange/Act
            var result = ExtraPresets.GetExtraRoads().ToList();

            // Assert
            Assert.All(result, x =>
            {
                Assert.False(string.IsNullOrWhiteSpace(x.LocaEng));
                Assert.False(string.IsNullOrWhiteSpace(x.LocaEsp));
                Assert.False(string.IsNullOrWhiteSpace(x.LocaFra));
                Assert.False(string.IsNullOrWhiteSpace(x.LocaGer));
                Assert.False(string.IsNullOrWhiteSpace(x.LocaPol));
                Assert.False(string.IsNullOrWhiteSpace(x.LocaRus));
            });
        }
    }
}
