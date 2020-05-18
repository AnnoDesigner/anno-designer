using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FandomParser.Core;
using Xunit;
using Xunit.Abstractions;

namespace InfoboxParser.Tests
{
    public class InfoboxExtractorTests
    {
        private static readonly ICommons _mockedCommons;
        private readonly ITestOutputHelper _output;

        private static readonly string input_Bakery;
        private static readonly string result_Bakery;
        private static readonly string input_Bank;
        private static readonly string result_Bank;
        private static readonly string input_Fishery;
        private static readonly string result_Fishery;
        private static readonly string input_Hospital;
        private static readonly string result_Hospital;
        private static readonly string input_Lumberjack;
        private static readonly string result_Lumberjack;
        private static readonly string input_Sawmill;
        private static readonly string result_Sawmill;

        static InfoboxExtractorTests()
        {
            _mockedCommons = Commons.Instance;

            string basePath = AppDomain.CurrentDomain.BaseDirectory;

            input_Bakery = File.ReadAllText(Path.Combine(basePath, "Testdata", "Extractor", "input_Bakery.txt"));
            result_Bakery = File.ReadAllText(Path.Combine(basePath, "Testdata", "Extractor", "result_Bakery.infobox"));

            input_Bank = File.ReadAllText(Path.Combine(basePath, "Testdata", "Extractor", "input_Bank.txt"));
            result_Bank = File.ReadAllText(Path.Combine(basePath, "Testdata", "Extractor", "result_Bank.infobox"));

            input_Fishery = File.ReadAllText(Path.Combine(basePath, "Testdata", "Extractor", "input_Fishery.txt"));
            result_Fishery = File.ReadAllText(Path.Combine(basePath, "Testdata", "Extractor", "result_Fishery.infobox"));

            input_Hospital = File.ReadAllText(Path.Combine(basePath, "Testdata", "Extractor", "input_Hospital.txt"));
            result_Hospital = File.ReadAllText(Path.Combine(basePath, "Testdata", "Extractor", "result_Hospital.infobox"));

            input_Lumberjack = File.ReadAllText(Path.Combine(basePath, "Testdata", "Extractor", "input_Lumberjack's_Hut.txt"));
            result_Lumberjack = File.ReadAllText(Path.Combine(basePath, "Testdata", "Extractor", "result_Lumberjack's_Hut.infobox"));

            input_Sawmill = File.ReadAllText(Path.Combine(basePath, "Testdata", "Extractor", "input_Sawmill.txt"));
            result_Sawmill = File.ReadAllText(Path.Combine(basePath, "Testdata", "Extractor", "result_Sawmill.infobox"));
        }

        public InfoboxExtractorTests(ITestOutputHelper testOutputHelperToUse)
        {
            _output = testOutputHelperToUse;
        }

        private IInfoboxExtractor GetExtractor(ICommons commonsToUse = null)
        {
            return new InfoboxExtractor(commonsToUse ?? _mockedCommons);
        }

        #region test data

        public static TheoryData<string, string> ExtractorTestData
        {
            get
            {
                return new TheoryData<string, string>
                {
                    { input_Bakery, result_Bakery },
                    { input_Bank, result_Bank },
                    { input_Fishery, result_Fishery },
                    { input_Hospital, result_Hospital },
                    { input_Lumberjack, result_Lumberjack },
                    { input_Sawmill, result_Sawmill },
                };
            }
        }

        #endregion

        [Theory]
        [InlineData(null)]
        [InlineData(" ")]
        [InlineData("")]
        public void ExtractInfobox_ContentIsNullOrWhiteSpace_ShouldReturnNull(string content)
        {
            // Arrange
            var extractor = GetExtractor();

            // Act
            var result = extractor.ExtractInfobox(content);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void ExtractInfobox_ContentContainsNoInfoboxTemplateStart_ShouldReturnNull()
        {
            // Arrange
            var content = "{{Some other template";
            var extractor = GetExtractor();

            // Act
            var result = extractor.ExtractInfobox(content);

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [MemberData(nameof(ExtractorTestData))]
        public void ExtractInfobox_ContentIsKnown_ShouldReturnExpectedResult(string content, string expectedResult)
        {
            // Arrange
            _output.WriteLine($"{nameof(content)}: {content}");
            _output.WriteLine($"{nameof(expectedResult)}: {expectedResult}");

            var extractor = GetExtractor();

            // Act
            var result = extractor.ExtractInfobox(content);

            // Assert
            Assert.Equal(expectedResult, result);
        }

    }
}
