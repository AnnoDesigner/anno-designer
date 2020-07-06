using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FandomParser.Core.Helper;
using FandomParser.WikiText;
using Xunit;
using Xunit.Abstractions;

namespace FandomParser.Tests
{
    public class WikiBuildingInfoProviderTests
    {
        private static readonly WikiTextTableContainer testData_BasicInfo_20200514;

        private readonly WikiBuildingInfoProvider _provider;
        private readonly ITestOutputHelper _output;

        static WikiBuildingInfoProviderTests()
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;

            testData_BasicInfo_20200514 = SerializationHelper.LoadFromFile<WikiTextTableContainer>(Path.Combine(basePath, "Testdata", "20200514_wiki_basic_info.json"));
        }

        public WikiBuildingInfoProviderTests(ITestOutputHelper testOutputHelperToUse)
        {
            _output = testOutputHelperToUse;
            _provider = new WikiBuildingInfoProvider();
        }

        #region test data

        public static TheoryData<WikiTextTableContainer> BasicInfoTestData
        {
            get
            {
                return new TheoryData<WikiTextTableContainer>
                {
                    { testData_BasicInfo_20200514}
                };
            }
        }

        #endregion

        [Theory]
        [MemberData(nameof(BasicInfoTestData))]
        public void GetWikiBuildingInfos_IsCalledWithTestdata_ShouldReturnExpectedResultCount(WikiTextTableContainer testdata)
        {
            // Act
            var result = _provider.GetWikiBuildingInfos(testdata);

            // Assert
            Assert.Equal(293, result.Infos.Count);
        }
    }
}
