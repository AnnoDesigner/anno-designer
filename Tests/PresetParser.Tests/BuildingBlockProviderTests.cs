using System;
using System.IO;
using System.Text;
using System.Xml;
using AnnoDesigner.Core.Presets.Models;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace PresetParser.Tests
{
    //disable parallel execution of tests, because of culture awareness in some of the tests
    [CollectionDefinition(nameof(BuildingBlockProviderTests), DisableParallelization = true)]
    public class BuildingBlockProviderTests
    {
        #region testdata

        private static readonly string testData_Bakery;
        private readonly ITestOutputHelper _out;

        #endregion

        static BuildingBlockProviderTests()
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;

            testData_Bakery = File.ReadAllText(Path.Combine(basePath, "Testdata", "1404_Bakery.txt"), Encoding.UTF8);
        }

        public BuildingBlockProviderTests(ITestOutputHelper outputHelper)
        {
            _out = outputHelper;
        }

        #region ctor tests

        [Fact]
        public void ctor_IfoProviderIsNull_ShouldThrow()
        {
            // Arrange/Act
            var ex = Assert.Throws<ArgumentNullException>(() =>
            {
                var provider = new BuildingBlockProvider(null);
            });

            // Assert
            Assert.NotNull(ex);
        }

        #endregion

        #region GetBuildingBlocker Anno1404 tests

        [Fact]
        public void GetBuildingBlocker_Anno1404BuildBlockerNotFound_ShouldReturnFalse()
        {
            // Arrange
            var mockedDocument = new XmlDocument();
            mockedDocument.LoadXml("<Info><Dummy></Dummy></Info>");

            var mockedIfoProvider = new Mock<IIfoFileProvider>();
            mockedIfoProvider.Setup(x => x.GetIfoFileContent(It.IsAny<string>(), It.IsAny<string>())).Returns(() => mockedDocument);

            var provider = new BuildingBlockProvider(mockedIfoProvider.Object);

            var mockedBuilding = new Mock<IBuildingInfo>();
            mockedBuilding.SetupAllProperties();

            // Act
            var result = provider.GetBuildingBlocker("basePath", mockedBuilding.Object, "variationFilename", Constants.ANNO_VERSION_1404);

            // Assert
            Assert.False(result);
            Assert.NotNull(mockedBuilding.Object.BuildBlocker);
        }

        [Fact]
        public void GetBuildingBlocker_Anno1404BuildBlockerHasNoChildNode_ShouldReturnFalse()
        {
            // Arrange
            var mockedDocument = new XmlDocument();
            mockedDocument.LoadXml("<Info><BuildBlocker></BuildBlocker></Info>");

            var mockedIfoProvider = new Mock<IIfoFileProvider>();
            mockedIfoProvider.Setup(x => x.GetIfoFileContent(It.IsAny<string>(), It.IsAny<string>())).Returns(() => mockedDocument);

            var provider = new BuildingBlockProvider(mockedIfoProvider.Object);

            var mockedBuilding = new Mock<IBuildingInfo>();
            mockedBuilding.SetupAllProperties();

            // Act
            var result = provider.GetBuildingBlocker("basePath", mockedBuilding.Object, "variationFilename", Constants.ANNO_VERSION_1404);

            // Assert
            Assert.False(result);            
            Assert.NotNull(mockedBuilding.Object.BuildBlocker);
        }

        [Fact]
        public void GetBuildingBlocker_Anno1404BothValuesZero_ShouldReturnFalse()
        {
            // Arrange
            var mockedDocument = new XmlDocument();
            mockedDocument.LoadXml("<Info><BuildBlocker><Position><x>300</x><z>300</z></Position></BuildBlocker></Info>");

            var mockedIfoProvider = new Mock<IIfoFileProvider>();
            mockedIfoProvider.Setup(x => x.GetIfoFileContent(It.IsAny<string>(), It.IsAny<string>())).Returns(() => mockedDocument);

            var provider = new BuildingBlockProvider(mockedIfoProvider.Object);

            var mockedBuilding = new Mock<IBuildingInfo>();
            mockedBuilding.SetupAllProperties();

            // Act
            var result = provider.GetBuildingBlocker("basePath", mockedBuilding.Object, "variationFilename", Constants.ANNO_VERSION_1404);

            // Assert
            Assert.False(result);
            Assert.NotNull(mockedBuilding.Object.BuildBlocker);
            Assert.Empty(mockedBuilding.Object.BuildBlocker.Dict);
        }

        [Fact]
        public void GetBuildingBlocker_Anno1404ValueForxIsZero_ShouldSetxToOne()
        {
            // Arrange
            var mockedDocument = new XmlDocument();
            mockedDocument.LoadXml("<Info><BuildBlocker><Position><x>300</x><z>8192</z></Position></BuildBlocker></Info>");

            var mockedIfoProvider = new Mock<IIfoFileProvider>();
            mockedIfoProvider.Setup(x => x.GetIfoFileContent(It.IsAny<string>(), It.IsAny<string>())).Returns(() => mockedDocument);

            var provider = new BuildingBlockProvider(mockedIfoProvider.Object);

            var mockedBuilding = new Mock<IBuildingInfo>();
            mockedBuilding.SetupAllProperties();

            // Act
            var result = provider.GetBuildingBlocker("basePath", mockedBuilding.Object, "variationFilename", Constants.ANNO_VERSION_1404);

            // Assert
            Assert.True(result);
            Assert.NotNull(mockedBuilding.Object.BuildBlocker);
            Assert.Equal(1, mockedBuilding.Object.BuildBlocker["x"]);
            Assert.Equal(4, mockedBuilding.Object.BuildBlocker["z"]);
        }

        [Fact]
        public void GetBuildingBlocker_Anno1404ValueForzIsZero_ShouldSetzToOne()
        {
            // Arrange
            var mockedDocument = new XmlDocument();
            mockedDocument.LoadXml("<Info><BuildBlocker><Position><x>8192</x><z>300</z></Position></BuildBlocker></Info>");

            var mockedIfoProvider = new Mock<IIfoFileProvider>();
            mockedIfoProvider.Setup(x => x.GetIfoFileContent(It.IsAny<string>(), It.IsAny<string>())).Returns(() => mockedDocument);

            var provider = new BuildingBlockProvider(mockedIfoProvider.Object);

            var mockedBuilding = new Mock<IBuildingInfo>();
            mockedBuilding.SetupAllProperties();

            // Act
            var result = provider.GetBuildingBlocker("basePath", mockedBuilding.Object, "variationFilename", Constants.ANNO_VERSION_1404);

            // Assert
            Assert.True(result);
            Assert.NotNull(mockedBuilding.Object.BuildBlocker);
            Assert.Equal(4, mockedBuilding.Object.BuildBlocker["x"]);
            Assert.Equal(1, mockedBuilding.Object.BuildBlocker["z"]);
        }

        [Fact]
        public void GetBuildingBlocker_Anno1404VariationWaterMillEcos_ShouldReturnCorrectValue()
        {
            // Arrange
            var mockedDocument = new XmlDocument();
            mockedDocument.LoadXml("<Info><BuildBlocker><Position><x>-8192</x><z>8192</z></Position></BuildBlocker></Info>");

            var mockedIfoProvider = new Mock<IIfoFileProvider>();
            mockedIfoProvider.Setup(x => x.GetIfoFileContent(It.IsAny<string>(), It.IsAny<string>())).Returns(() => mockedDocument);

            var provider = new BuildingBlockProvider(mockedIfoProvider.Object);

            var mockedBuilding = new Mock<IBuildingInfo>();
            mockedBuilding.SetupAllProperties();

            // Act
            var result = provider.GetBuildingBlocker("basePath", mockedBuilding.Object, "water_mill_ecos.txt", Constants.ANNO_VERSION_1404);

            // Assert
            Assert.True(result);
            Assert.NotNull(mockedBuilding.Object.BuildBlocker);
            Assert.Equal(3, mockedBuilding.Object.BuildBlocker["x"]);
            Assert.Equal(7, mockedBuilding.Object.BuildBlocker["z"]);
        }

        [Fact]
        public void GetBuildingBlocker_Anno1404VariationOrnamentalPost09_ShouldReturnCorrectValue()
        {
            // Arrange
            var mockedDocument = new XmlDocument();
            mockedDocument.LoadXml("<Info><BuildBlocker><Position><x>-8192</x><z>8192</z></Position></BuildBlocker></Info>");

            var mockedIfoProvider = new Mock<IIfoFileProvider>();
            mockedIfoProvider.Setup(x => x.GetIfoFileContent(It.IsAny<string>(), It.IsAny<string>())).Returns(() => mockedDocument);

            var provider = new BuildingBlockProvider(mockedIfoProvider.Object);

            var mockedBuilding = new Mock<IBuildingInfo>();
            mockedBuilding.SetupAllProperties();

            // Act
            var result = provider.GetBuildingBlocker("basePath", mockedBuilding.Object, "ornamental_post_09.txt", Constants.ANNO_VERSION_1404);

            // Assert
            Assert.True(result);
            Assert.NotNull(mockedBuilding.Object.BuildBlocker);
            Assert.Equal(7, mockedBuilding.Object.BuildBlocker["x"]);
            Assert.Equal(7, mockedBuilding.Object.BuildBlocker["z"]);
        }

        [Fact]
        public void GetBuildingBlocker_Anno1404Bakery_ShouldReturnCorrectValue()
        {
            // Arrange
            var mockedDocument = new XmlDocument();
            mockedDocument.LoadXml(testData_Bakery);

            var mockedIfoProvider = new Mock<IIfoFileProvider>();
            mockedIfoProvider.Setup(x => x.GetIfoFileContent(It.IsAny<string>(), It.IsAny<string>())).Returns(() => mockedDocument);

            var provider = new BuildingBlockProvider(mockedIfoProvider.Object);

            var mockedBuilding = new Mock<IBuildingInfo>();
            mockedBuilding.SetupAllProperties();

            // Act
            var result = provider.GetBuildingBlocker("basePath", mockedBuilding.Object, "variationFilename", Constants.ANNO_VERSION_1404);

            // Assert
            Assert.True(result);
            Assert.NotNull(mockedBuilding.Object.BuildBlocker);
            Assert.Equal(3, mockedBuilding.Object.BuildBlocker["x"]);
            Assert.Equal(3, mockedBuilding.Object.BuildBlocker["z"]);
        }

        #endregion

        #region GetBuildingBlocker Anno1800 tests

        [Theory]
        [InlineData("<Info><Dummy></Dummy></Info>")]
        [InlineData("<Info></Info>")]
        public void GetBuildingBlocker_Anno1800BuildBlockerNotFound_ShouldReturnFalse(string ifodocument)
        {
            // Arrange
            var mockedDocument = new XmlDocument();
            mockedDocument.LoadXml(ifodocument);

            var mockedIfoProvider = new Mock<IIfoFileProvider>();
            mockedIfoProvider.Setup(x => x.GetIfoFileContent(It.IsAny<string>(), It.IsAny<string>())).Returns(() => mockedDocument);

            var provider = new BuildingBlockProvider(mockedIfoProvider.Object);

            var mockedBuilding = new Mock<IBuildingInfo>();
            mockedBuilding.SetupAllProperties();

            // Act
            var result = provider.GetBuildingBlocker("basePath", mockedBuilding.Object, "variationFilename", Constants.ANNO_VERSION_1800);

            // Assert
            Assert.False(result);
            Assert.Null(mockedBuilding.Object.BuildBlocker);
        }

        [Fact]
        public void GetBuildingBlocker_Anno1800BlockerFoundWitEmptyChild_ShouldReturnFalse()
        {
            // Arrange
            var mockedDocument = new XmlDocument();
            mockedDocument.LoadXml("<Info><BuildBlocker><Position></Position><Position></Position><Position></Position><Position></Position></BuildBlocker></Info>");

            var mockedIfoProvider = new Mock<IIfoFileProvider>();
            mockedIfoProvider.Setup(x => x.GetIfoFileContent(It.IsAny<string>(), It.IsAny<string>())).Returns(() => mockedDocument);

            var provider = new BuildingBlockProvider(mockedIfoProvider.Object);

            var mockedBuilding = new Mock<IBuildingInfo>();
            mockedBuilding.SetupAllProperties();

            // Act
            var result = provider.GetBuildingBlocker("basePath", mockedBuilding.Object, "variationFilename", Constants.ANNO_VERSION_1800);

            // Assert
            Assert.False(result);
            Assert.Null(mockedBuilding.Object.BuildBlocker);
        }

        [Fact]
        public void GetBuildingBlocker_Anno1800BlockerFoundButEmpty_ShouldReturnFalse()
        {
            // Arrange
            var mockedDocument = new XmlDocument();
            mockedDocument.LoadXml("<Info><BuildBlocker></BuildBlocker></Info>");

            var mockedIfoProvider = new Mock<IIfoFileProvider>();
            mockedIfoProvider.Setup(x => x.GetIfoFileContent(It.IsAny<string>(), It.IsAny<string>())).Returns(() => mockedDocument);

            var provider = new BuildingBlockProvider(mockedIfoProvider.Object);

            var mockedBuilding = new Mock<IBuildingInfo>();
            mockedBuilding.SetupAllProperties();

            // Act
            var result = provider.GetBuildingBlocker("basePath", mockedBuilding.Object, "variationFilename", Constants.ANNO_VERSION_1800);

            // Assert
            Assert.False(result);
            Assert.Null(mockedBuilding.Object.BuildBlocker);
        }

        [Fact]
        public void GetBuildingBlocker_Anno1800BothValuesZero_ShouldReturnFalse()
        {
            // Arrange
            var mockedDocument = new XmlDocument();
            mockedDocument.LoadXml("<Info><BuildBlocker><Position><xf>0.2</xf><zf>0.2</zf></Position><Position><xf>0.2</xf><zf>-0.2</zf></Position><Position><xf>-0.2</xf><zf>0.2</zf></Position><Position><xf>-0.2</xf><zf>-0.2</zf></Position></BuildBlocker></Info>");

            var mockedIfoProvider = new Mock<IIfoFileProvider>();
            mockedIfoProvider.Setup(x => x.GetIfoFileContent(It.IsAny<string>(), It.IsAny<string>())).Returns(() => mockedDocument);

            var provider = new BuildingBlockProvider(mockedIfoProvider.Object);

            var mockedBuilding = new Mock<IBuildingInfo>();
            mockedBuilding.SetupAllProperties();

            // Act
            var result = provider.GetBuildingBlocker("basePath", mockedBuilding.Object, "variationFilename", Constants.ANNO_VERSION_1800);

            // Assert
            Assert.False(result);
            Assert.NotNull(mockedBuilding.Object.BuildBlocker);
            Assert.Empty(mockedBuilding.Object.BuildBlocker.Dict);
        }

        [CulturedFact]
        public void GetBuildingBlocker_Anno1800ValueForxIsZero_ShouldSetxToOne()
        {
            // Arrange
            var mockedDocument = new XmlDocument();
            mockedDocument.LoadXml("<Info><BuildBlocker><Position><xf>0.2</xf><zf>2</zf></Position><Position><xf>0.2</xf><zf>-2</zf></Position><Position><xf>-0.2</xf><zf>2</zf></Position><Position><xf>-0.2</xf><zf>-2</zf></Position></BuildBlocker></Info>");

            var mockedIfoProvider = new Mock<IIfoFileProvider>();
            mockedIfoProvider.Setup(x => x.GetIfoFileContent(It.IsAny<string>(), It.IsAny<string>())).Returns(() => mockedDocument);

            var provider = new BuildingBlockProvider(mockedIfoProvider.Object);

            var mockedBuilding = new Mock<IBuildingInfo>();
            mockedBuilding.SetupAllProperties();

            var building = mockedBuilding.Object;

            // Act
            var result = provider.GetBuildingBlocker("basePath", building, "variationFilename", Constants.ANNO_VERSION_1800);

            // Assert
            Assert.True(result);
            Assert.NotNull(building.BuildBlocker);
            Assert.Equal(1, building.BuildBlocker["x"]);
            Assert.Equal(4, building.BuildBlocker["z"]);
        }

        [CulturedFact]
        public void GetBuildingBlocker_Anno1800ValueForzIsZero_ShouldSetzToOne()
        {
            // Arrange
            var mockedDocument = new XmlDocument();
            mockedDocument.LoadXml("<Info><BuildBlocker><Position><xf>2</xf><zf>0.2</zf></Position><Position><xf>2</xf><zf>-0.2</zf></Position><Position><xf>-2</xf><zf>0.2</zf></Position><Position><xf>-2</xf><zf>-0.2</zf></Position></BuildBlocker></Info>");

            var mockedIfoProvider = new Mock<IIfoFileProvider>();
            mockedIfoProvider.Setup(x => x.GetIfoFileContent(It.IsAny<string>(), It.IsAny<string>())).Returns(() => mockedDocument);

            var provider = new BuildingBlockProvider(mockedIfoProvider.Object);

            var mockedBuilding = new Mock<IBuildingInfo>();
            mockedBuilding.SetupAllProperties();

            // Act
            var result = provider.GetBuildingBlocker("basePath", mockedBuilding.Object, "variationFilename", Constants.ANNO_VERSION_1800);

            // Assert
            Assert.True(result);
            Assert.NotNull(mockedBuilding.Object.BuildBlocker);
            Assert.Equal(4, mockedBuilding.Object.BuildBlocker["x"]);
            Assert.Equal(1, mockedBuilding.Object.BuildBlocker["z"]);
        }

        [CulturedFact]
        public void GetBuildingBlocker_Anno1800BuildingIsPalaceGate_ShouldSetCorrectSize()
        {
            // Arrange
            var mockedDocument = new XmlDocument();
            mockedDocument.LoadXml("<Info><BuildBlocker><Position><xf>1.5</xf><zf>1.5</zf></Position><Position><xf>1.5</xf><zf>-1.5</zf></Position><Position><xf>-1.5</xf><zf>0.5</zf></Position><Position><xf>-1.5</xf><zf>-1.5</zf></Position></BuildBlocker></Info>");

            var mockedIfoProvider = new Mock<IIfoFileProvider>();
            mockedIfoProvider.Setup(x => x.GetIfoFileContent(It.IsAny<string>(), It.IsAny<string>())).Returns(() => mockedDocument);

            var provider = new BuildingBlockProvider(mockedIfoProvider.Object);

            var mockedBuilding = new Mock<IBuildingInfo>();
            mockedBuilding.SetupAllProperties();
            mockedBuilding.SetupGet(x => x.Identifier).Returns("Palace_Module_05 (gate)");

            // Act
            var result = provider.GetBuildingBlocker("basePath", mockedBuilding.Object, "variationFilename", Constants.ANNO_VERSION_1800);

            // Assert
            Assert.True(result);
            Assert.NotNull(mockedBuilding.Object.BuildBlocker);
            Assert.Equal(3, mockedBuilding.Object.BuildBlocker["x"]);
            Assert.Equal(3, mockedBuilding.Object.BuildBlocker["z"]);
        }

        #endregion
    }
}
