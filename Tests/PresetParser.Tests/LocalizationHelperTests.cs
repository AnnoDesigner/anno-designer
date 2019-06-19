using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnnoDesigner.Core.Models;
using PresetParser.Anno1404_Anno2070;
using PresetParser.Models;
using Xunit;

namespace PresetParser.Tests
{
    public class LocalizationHelperTests
    {
        #region testdata

        private static readonly Dictionary<string, Dictionary<string, PathRef[]>> testData_versionSpecificPaths_Anno1404;
        private static readonly Dictionary<string, Dictionary<string, PathRef[]>> testData_versionSpecificPaths_Anno2070;

        static LocalizationHelperTests()
        {
            testData_versionSpecificPaths_Anno1404 = new Dictionary<string, Dictionary<string, PathRef[]>>();
            testData_versionSpecificPaths_Anno1404.Add(Constants.ANNO_VERSION_1404, new Dictionary<string, PathRef[]>());
            testData_versionSpecificPaths_Anno1404[Constants.ANNO_VERSION_1404].Add("localisation", new PathRef[]
            {
                new PathRef("data/loca"),
                new PathRef("addondata/loca")
            });

            testData_versionSpecificPaths_Anno2070 = new Dictionary<string, Dictionary<string, PathRef[]>>();
            testData_versionSpecificPaths_Anno2070.Add(Constants.ANNO_VERSION_2070, new Dictionary<string, PathRef[]>());
            testData_versionSpecificPaths_Anno2070[Constants.ANNO_VERSION_2070].Add("localisation", new PathRef[]
            {
                new PathRef("data/loca"),
            });
        }

        #endregion

        [Fact]
        public void Ctor_FileSystemIsNull_ShouldThrow()
        {
            // Arrange
            LocalizationHelper helper;

            // Act/Assert
            var ex = Assert.Throws<ArgumentNullException>(() => helper = new LocalizationHelper(null));
        }

        #region common tests

        [Theory]
        [InlineData(Constants.ANNO_VERSION_1800)]
        [InlineData(Constants.ANNO_VERSION_2205)]
        public void GetLocalization_AnnoVersionIsNot1404Or2070_ShouldReturnNull(string annoVersionToTest)
        {
            // Arrange
            var helper = new LocalizationHelper(new MockFileSystem());

            // Act
            var result = helper.GetLocalization(annoVersionToTest,
                addPrefix: false,
                versionSpecificPaths: testData_versionSpecificPaths_Anno1404,
                languages: new string[0],
                basePath: "dummy");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetLocalization_VersionSpecificPathsAreNull_ShouldThrow()
        {
            // Arrange
            var helper = new LocalizationHelper(new MockFileSystem());
            var languages = new string[] { "dummy" };

            // Act/Assert
            var ex = Assert.Throws<ArgumentNullException>(() => helper.GetLocalization(Constants.ANNO_VERSION_1404, addPrefix: false, null, languages, basePath: "dummy"));

            // Assert
            Assert.NotNull(ex);
        }

        [Fact]
        public void GetLocalization_LanguagesAreNull_ShouldThrow()
        {
            // Arrange
            var helper = new LocalizationHelper(new MockFileSystem());

            // Act/Assert
            var ex = Assert.Throws<ArgumentNullException>(() => helper.GetLocalization(Constants.ANNO_VERSION_1404, addPrefix: false, testData_versionSpecificPaths_Anno1404, null, basePath: "dummy"));
        }

        [Fact]
        public void GetLocalization_VersionSpecificPathsDoesNotContainAnnoVersion_ShouldThrow()
        {
            // Arrange
            var helper = new LocalizationHelper(new MockFileSystem());

            var languages = new string[] { "dummy" };

            var versionSpecificPaths = new Dictionary<string, Dictionary<string, PathRef[]>>();
            versionSpecificPaths.Add("dummy", new Dictionary<string, PathRef[]>());
            versionSpecificPaths.Add(Constants.ANNO_VERSION_2205, new Dictionary<string, PathRef[]>());

            // Act/Assert
            var ex = Assert.Throws<ArgumentException>(() => helper.GetLocalization(Constants.ANNO_VERSION_1404, addPrefix: false, versionSpecificPaths, languages, basePath: "dummy"));
        }

        [Fact]
        public void GetLocalization_NoFilesFound_ShouldReturnEmpty()
        {
            // Arrange
            var helper = new LocalizationHelper(new MockFileSystem());
            var languages = new string[] { "dummy" };

            // Act
            var result = helper.GetLocalization(Constants.ANNO_VERSION_1404, addPrefix: false, testData_versionSpecificPaths_Anno1404, languages, basePath: "dummy");

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetLocalization_FilesContainOnlyComments_ShouldReturnEmpty()
        {
            // Arrange
            var basePath = "dummy";
            var languages = new string[] { "eng" };

            var mockedFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { $@"{basePath}\data\loca\{languages[0]}\txt\guids.txt", new MockFileData($"#first comment{Environment.NewLine}#second comment") },
                { $@"{basePath}\addondata\loca\{languages[0]}\txt\guids.txt", new MockFileData($"#first addon comment{Environment.NewLine}#second addon comment") },
            });

            var helper = new LocalizationHelper(mockedFileSystem);

            // Act
            var result = helper.GetLocalization(Constants.ANNO_VERSION_1404, addPrefix: false, testData_versionSpecificPaths_Anno1404, languages, basePath);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetLocalization_FilesContainOnlyEmptyLines_ShouldReturnEmpty()
        {
            // Arrange
            var basePath = "dummy";
            var languages = new string[] { "eng" };

            var mockedFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { $@"{basePath}\data\loca\{languages[0]}\txt\guids.txt", new MockFileData($"{string.Empty}{Environment.NewLine}  ") },
                { $@"{basePath}\addondata\loca\{languages[0]}\txt\guids.txt", new MockFileData($"{string.Empty}{Environment.NewLine}  ") },
            });

            var helper = new LocalizationHelper(mockedFileSystem);

            // Act
            var result = helper.GetLocalization(Constants.ANNO_VERSION_1404, addPrefix: false, testData_versionSpecificPaths_Anno1404, languages, basePath);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetLocalization_LinesContainNoDelimiter_ShouldReturnEmpty()
        {
            // Arrange
            var basePath = "dummy";
            var languages = new string[] { "eng" };

            var mockedFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { $@"{basePath}\data\loca\{languages[0]}\txt\guids.txt", new MockFileData($"this is {Environment.NewLine} a test ") },
                { $@"{basePath}\addondata\loca\{languages[0]}\txt\guids.txt", new MockFileData($"this is {Environment.NewLine} a test ") },
            });

            var helper = new LocalizationHelper(mockedFileSystem);

            // Act
            var result = helper.GetLocalization(Constants.ANNO_VERSION_1404, addPrefix: false, testData_versionSpecificPaths_Anno1404, languages, basePath);

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region Anno 1404 tests

        [Fact]
        public void GetLocalization_IsAnno1404AndLocalizationIsFoundAndNoPrefix_ShouldReturnCorrectResult()
        {
            // Arrange
            var basePath = "dummy";
            var languages = new string[] { "eng" };
            var expectedGuid1 = "12612";
            var expectedLocalization1 = "Knight";
            var expectedGuid2 = "15926";
            var expectedLocalization2 = "Warship";

            var mockedFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { $@"{basePath}\data\loca\{languages[0]}\txt\guids.txt", new MockFileData($"{expectedGuid1}={expectedLocalization1}{Environment.NewLine}{expectedGuid2}={expectedLocalization2}") },
            });

            var expectedResult = new Dictionary<string, SerializableDictionary<string>>();
            expectedResult.Add(expectedGuid1, new SerializableDictionary<string>());
            expectedResult[expectedGuid1][languages[0]] = expectedLocalization1;
            expectedResult.Add(expectedGuid2, new SerializableDictionary<string>());
            expectedResult[expectedGuid2][languages[0]] = expectedLocalization2;

            var helper = new LocalizationHelper(mockedFileSystem);

            // Act
            var result = helper.GetLocalization(Constants.ANNO_VERSION_1404, addPrefix: false, testData_versionSpecificPaths_Anno1404, languages, basePath);

            // Assert
            Assert.NotEmpty(result);
            Assert.Equal(expectedResult[expectedGuid1][languages[0]], result[expectedGuid1][languages[0]]);
            Assert.Equal(expectedResult[expectedGuid2][languages[0]], result[expectedGuid2][languages[0]]);
        }

        [Fact]
        public void GetLocalization_IsAnno1404AndLocalizationIsFoundAndPrefix_ShouldReturnCorrectResult()
        {
            // Arrange
            var basePath = "dummy";
            var languages = new string[] { "eng" };
            var expectedGuid1 = "12612";
            var expectedLocalization1 = "Knight";
            var expectedGuid2 = "15926";
            var expectedLocalization2 = "Warship";
            var expectedPrefix = "A4_";

            var mockedFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { $@"{basePath}\data\loca\{languages[0]}\txt\guids.txt", new MockFileData($"{expectedGuid1}={expectedLocalization1}{Environment.NewLine}{expectedGuid2}={expectedLocalization2}") },
            });

            var expectedResult = new Dictionary<string, SerializableDictionary<string>>();
            expectedResult.Add(expectedGuid1, new SerializableDictionary<string>());
            expectedResult[expectedGuid1][languages[0]] = expectedLocalization1;
            expectedResult.Add(expectedGuid2, new SerializableDictionary<string>());
            expectedResult[expectedGuid2][languages[0]] = expectedLocalization2;

            var helper = new LocalizationHelper(mockedFileSystem);

            // Act
            var result = helper.GetLocalization(Constants.ANNO_VERSION_1404, addPrefix: true, testData_versionSpecificPaths_Anno1404, languages, basePath);

            // Assert
            Assert.NotEmpty(result);
            Assert.Equal(expectedPrefix + expectedResult[expectedGuid1][languages[0]], result[expectedGuid1][languages[0]]);
            Assert.Equal(expectedPrefix + expectedResult[expectedGuid2][languages[0]], result[expectedGuid2][languages[0]]);
        }

        #endregion

        #region Anno 2070 tests

        [Fact]
        public void GetLocalization_IsAnno2070AndLocalizationIsFoundAndNoPrefix_ShouldReturnCorrectResult()
        {
            // Arrange
            var basePath = "dummy";
            var languages = new string[] { "eng" };
            var expectedGuid1 = "12612";
            var expectedLocalization1 = "Knight";
            var expectedGuid2 = "15926";
            var expectedLocalization2 = "Warship";

            var mockedFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { $@"{basePath}\data\loca\{languages[0]}\txt\guids.txt", new MockFileData($"{expectedGuid1}={expectedLocalization1}{Environment.NewLine}{expectedGuid2}={expectedLocalization2}") },
            });

            var expectedResult = new Dictionary<string, SerializableDictionary<string>>();
            expectedResult.Add(expectedGuid1, new SerializableDictionary<string>());
            expectedResult[expectedGuid1][languages[0]] = expectedLocalization1;
            expectedResult.Add(expectedGuid2, new SerializableDictionary<string>());
            expectedResult[expectedGuid2][languages[0]] = expectedLocalization2;

            var helper = new LocalizationHelper(mockedFileSystem);

            // Act
            var result = helper.GetLocalization(Constants.ANNO_VERSION_2070, addPrefix: false, testData_versionSpecificPaths_Anno2070, languages, basePath);

            // Assert
            Assert.NotEmpty(result);
            Assert.Equal(expectedResult[expectedGuid1][languages[0]], result[expectedGuid1][languages[0]]);
            Assert.Equal(expectedResult[expectedGuid2][languages[0]], result[expectedGuid2][languages[0]]);
        }

        [Fact]
        public void GetLocalization_IsAnno2070AndLocalizationIsFoundAndPrefix_ShouldReturnCorrectResult()
        {
            // Arrange
            var basePath = "dummy";
            var languages = new string[] { "eng" };
            var expectedGuid1 = "12612";
            var expectedLocalization1 = "Knight";
            var expectedGuid2 = "15926";
            var expectedLocalization2 = "Warship";
            var expectedPrefix = "A5_";

            var mockedFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { $@"{basePath}\data\loca\{languages[0]}\txt\guids.txt", new MockFileData($"{expectedGuid1}={expectedLocalization1}{Environment.NewLine}{expectedGuid2}={expectedLocalization2}") },
            });

            var expectedResult = new Dictionary<string, SerializableDictionary<string>>();
            expectedResult.Add(expectedGuid1, new SerializableDictionary<string>());
            expectedResult[expectedGuid1][languages[0]] = expectedLocalization1;
            expectedResult.Add(expectedGuid2, new SerializableDictionary<string>());
            expectedResult[expectedGuid2][languages[0]] = expectedLocalization2;

            var helper = new LocalizationHelper(mockedFileSystem);

            // Act
            var result = helper.GetLocalization(Constants.ANNO_VERSION_2070, addPrefix: true, testData_versionSpecificPaths_Anno2070, languages, basePath);

            // Assert
            Assert.NotEmpty(result);
            Assert.Equal(expectedPrefix + expectedResult[expectedGuid1][languages[0]], result[expectedGuid1][languages[0]]);
            Assert.Equal(expectedPrefix + expectedResult[expectedGuid2][languages[0]], result[expectedGuid2][languages[0]]);
        }

        [Fact]
        public void GetLocalization_IsAnno2070AndIsSpecialGuidAndNoPrefix_ShouldReturnCorrectResult()
        {
            // Arrange
            var basePath = "dummy";
            var languages = new string[] { "eng", "ger", "fra", "pol", "rus", "non_existing" };
            var expectedGuid = "10239";
            var expectedLocalizations = new string[] { "Black Smoker", "Black Smoker", "Convertisseur de métal", "Komin hydrotermalny", "Черный курильщик" };

            var mockedFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { $@"{basePath}\data\loca\{languages[0]}\txt\guids.txt", new MockFileData($"{expectedGuid}=dummy") },
                { $@"{basePath}\data\loca\{languages[1]}\txt\guids.txt", new MockFileData($"{expectedGuid}=dummy") },
                { $@"{basePath}\data\loca\{languages[2]}\txt\guids.txt", new MockFileData($"{expectedGuid}=dummy") },
                { $@"{basePath}\data\loca\{languages[3]}\txt\guids.txt", new MockFileData($"{expectedGuid}=dummy") },
                { $@"{basePath}\data\loca\{languages[4]}\txt\guids.txt", new MockFileData($"{expectedGuid}=dummy") },
                { $@"{basePath}\data\loca\{languages[5]}\txt\guids.txt", new MockFileData($"{expectedGuid}=dummy") },
            });

            var expectedResult = new Dictionary<string, SerializableDictionary<string>>();
            expectedResult.Add(expectedGuid, new SerializableDictionary<string>());
            expectedResult[expectedGuid][languages[0]] = expectedLocalizations[0];
            expectedResult[expectedGuid][languages[1]] = expectedLocalizations[1];
            expectedResult[expectedGuid][languages[2]] = expectedLocalizations[2];
            expectedResult[expectedGuid][languages[3]] = expectedLocalizations[3];
            expectedResult[expectedGuid][languages[4]] = expectedLocalizations[4];
            expectedResult[expectedGuid][languages[5]] = "dummy";

            var helper = new LocalizationHelper(mockedFileSystem);

            // Act
            var result = helper.GetLocalization(Constants.ANNO_VERSION_2070, addPrefix: false, testData_versionSpecificPaths_Anno2070, languages, basePath);

            // Assert
            Assert.NotEmpty(result);
            Assert.Equal(expectedResult[expectedGuid][languages[0]], result[expectedGuid][languages[0]]);
            Assert.Equal(expectedResult[expectedGuid][languages[1]], result[expectedGuid][languages[1]]);
            Assert.Equal(expectedResult[expectedGuid][languages[2]], result[expectedGuid][languages[2]]);
            Assert.Equal(expectedResult[expectedGuid][languages[3]], result[expectedGuid][languages[3]]);
            Assert.Equal(expectedResult[expectedGuid][languages[4]], result[expectedGuid][languages[4]]);
            Assert.Equal(expectedResult[expectedGuid][languages[5]], result[expectedGuid][languages[5]]);
        }

        [Fact]
        public void GetLocalization_IsAnno2070AndIsSpecialGuidAndPrefix_ShouldReturnCorrectResult()
        {
            // Arrange
            var basePath = "dummy";
            var languages = new string[] { "eng" };
            var expectedGuid = "10239";
            var expectedLocalization = "dummy";
            var expectedPrefix = "A5_";

            var mockedFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { $@"{basePath}\data\loca\{languages[0]}\txt\guids.txt", new MockFileData($"{expectedGuid}={expectedLocalization}") },
            });

            var expectedResult = new Dictionary<string, SerializableDictionary<string>>();
            expectedResult.Add(expectedGuid, new SerializableDictionary<string>());
            expectedResult[expectedGuid][languages[0]] = expectedLocalization;

            var helper = new LocalizationHelper(mockedFileSystem);

            // Act
            var result = helper.GetLocalization(Constants.ANNO_VERSION_2070, addPrefix: true, testData_versionSpecificPaths_Anno2070, languages, basePath);

            // Assert
            Assert.NotEmpty(result);
            Assert.Equal(expectedPrefix + expectedResult[expectedGuid][languages[0]], result[expectedGuid][languages[0]]);
        }

        #endregion
    }
}
