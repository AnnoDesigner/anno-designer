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
    }
}
