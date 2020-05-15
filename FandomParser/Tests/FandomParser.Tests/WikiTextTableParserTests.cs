using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FandomParser.WikiText;
using Xunit;
using Xunit.Abstractions;

namespace FandomParser.Tests
{
    public class WikiTextTableParserTests
    {
        private static readonly string testDataBuildings_Complete_20200514;

        private readonly WikiTextTableParser _parser;
        private readonly ITestOutputHelper _output;

        static WikiTextTableParserTests()
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;

            testDataBuildings_Complete_20200514 = File.ReadAllText(Path.Combine(basePath, "Testdata", "20200514_wikitext_buildings_complete.txt"));
        }

        public WikiTextTableParserTests(ITestOutputHelper testOutputHelperToUse)
        {
            _output = testOutputHelperToUse;
            _parser = new WikiTextTableParser();
        }

        #region test data

        public static TheoryData<string> BuildingsCompleteTestData
        {
            get
            {
                return new TheoryData<string>
                {
                    { testDataBuildings_Complete_20200514}
                };
            }
        }

        #endregion

        [Theory]
        [MemberData(nameof(BuildingsCompleteTestData))]
        public void GetTables_IsCalledWithTestdata_ShouldReturnExpectedResultCount(string testdata)
        {
            // Act
            var result = _parser.GetTables(testdata);

            // Assert
            Assert.Equal(293, result.Entries.Count);
        }
    }
}
