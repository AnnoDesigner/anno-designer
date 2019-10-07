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
        [InlineData(Constants.ANNO_VERSION_2070, 4)]
        [InlineData(Constants.ANNO_VERSION_1800, 24)]
        [InlineData(Constants.ANNO_VERSION_2205, 0)]
        public void GetExtraPresets_AnnoVersionIsKnown_ShouldReturnCorrectExtraPresetsCount(string annoVersion, int expectedCount)
        {
            // Arrange/Act
            var result = ExtraPresets.GetExtraPresets(annoVersion).ToList();

            // Assert
            Assert.Equal(expectedCount, result.Count);
        }
    }
}
