using System.Collections.Generic;
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
        public void CheckForUniqueIdentifiers_KnownDuplicatesIsNull_ShouldNotThrow()
        {
            // Arrange/Act
            var result = _validator.CheckForUniqueIdentifiers(_testData_valid_buildingInfo, null);

            // Assert
            Assert.True(result.isValid);
            Assert.Empty(result.duplicateIdentifiers);
        }

        [Fact]
        public void CheckForUniqueIdentifiers_KnownDuplicatesIsEmpty_ShouldNotThrow()
        {
            // Arrange/Act
            var result = _validator.CheckForUniqueIdentifiers(_testData_valid_buildingInfo, new List<string>());

            // Assert
            Assert.True(result.isValid);
            Assert.Empty(result.duplicateIdentifiers);
        }

        [Fact]
        public void CheckForUniqueIdentifiers_NoDuplicateIdentifiers_ShouldReturnIsValid()
        {
            // Arrange/Act
            var result = _validator.CheckForUniqueIdentifiers(_testData_valid_buildingInfo, new List<string>());

            // Assert
            Assert.True(result.isValid);
            Assert.Empty(result.duplicateIdentifiers);
        }

        [Fact]
        public void CheckForUniqueIdentifiers_DuplicateIdentifiers_ShouldReturnNotValidAndListOfIdentifiers()
        {
            // Arrange/Act
            var result = _validator.CheckForUniqueIdentifiers(_testData_invalid_buildingInfo, new List<string>());

            // Assert
            Assert.False(result.isValid);
            Assert.NotEmpty(result.duplicateIdentifiers);
            _out.WriteLine(string.Join(" ,", result.duplicateIdentifiers));
        }

        [Fact]
        public void CheckForUniqueIdentifiers_DuplicateIdentifiersAndKnownDuplicates_ShouldReturnNotValidAndListOfIdentifiers()
        {
            // Arrange
            var knownDuplicate = "A99_known duplicate";
            _testData_invalid_buildingInfo.Add(new BuildingInfo { Identifier = knownDuplicate });
            _testData_invalid_buildingInfo.Insert(0, new BuildingInfo { Identifier = knownDuplicate });

            // Arrange
            var result = _validator.CheckForUniqueIdentifiers(_testData_invalid_buildingInfo, new List<string> { knownDuplicate });

            // Assert
            Assert.False(result.isValid);
            Assert.NotEmpty(result.duplicateIdentifiers);
            _out.WriteLine(string.Join(" ,", result.duplicateIdentifiers));
        }

        [Fact]
        public void CheckForUniqueIdentifiers_KnownDuplicatesFound_ShouldReturnIsValid()
        {
            // Arrange
            var knownDuplicate = "A99_known duplicate";
            _testData_valid_buildingInfo.Add(new BuildingInfo { Identifier = knownDuplicate });
            _testData_valid_buildingInfo.Insert(0, new BuildingInfo { Identifier = knownDuplicate });

            // Act
            var result = _validator.CheckForUniqueIdentifiers(_testData_valid_buildingInfo, new List<string> { knownDuplicate });

            // Assert
            Assert.True(result.isValid);
            Assert.Empty(result.duplicateIdentifiers);
        }
    }
}
