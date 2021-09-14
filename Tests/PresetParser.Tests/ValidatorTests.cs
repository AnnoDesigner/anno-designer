using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnnoDesigner.Core.Presets.Models;
using Xunit;
using Xunit.Abstractions;

namespace PresetParser.Tests
{
    public class ValidatorTests
    {
        private readonly ITestOutputHelper _out;

        private readonly Validator _validator;
        private readonly List<IBuildingInfo> _testData_valid_buildingInfo;
        private readonly List<IBuildingInfo> _testData_invalid_buildingInfo;

        public ValidatorTests(ITestOutputHelper outputHelperToUse)
        {
            _out = outputHelperToUse;
            _validator = new Validator();

            _testData_valid_buildingInfo = new List<IBuildingInfo>
            {
                new BuildingInfo { Identifier = "A4_building 1" },
                new BuildingInfo { Identifier = "A4_building 2" },
                new BuildingInfo { Identifier = "A4_building 3" },
                new BuildingInfo { Identifier = "A7_building 3" },
            };

            _testData_invalid_buildingInfo = new List<IBuildingInfo>
            {
                new BuildingInfo { Identifier = "A4_building 1" },
                new BuildingInfo { Identifier = "A4_building 2" },
                new BuildingInfo { Identifier = "A4_building 1" },
                new BuildingInfo { Identifier = "A4_building 3" },
            };
        }

        [Fact]
        public void CheckForUniqueIdentifiers_NoDuplicateIdentifiers_ShouldReturnIsValid()
        {
            // Arrange/Act
            var result = _validator.CheckForUniqueIdentifiers(_testData_valid_buildingInfo);

            // Assert
            Assert.True(result.isValid);
            Assert.Empty(result.duplicateIdentifiers);
        }

        [Fact]
        public void CheckForUniqueIdentifiers_DuplicateIdentifiers_ShouldReturnNotValidAndListOfIdentifiers()
        {
            // Arrange/Act
            var result = _validator.CheckForUniqueIdentifiers(_testData_invalid_buildingInfo);

            // Assert
            Assert.False(result.isValid);
            Assert.NotEmpty(result.duplicateIdentifiers);
            _out.WriteLine(string.Join(" ,", result.duplicateIdentifiers));
        }
    }
}
