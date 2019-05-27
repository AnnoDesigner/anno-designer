using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnnoDesigner.Core.Presets.Comparer;
using AnnoDesigner.Core.Presets.Models;
using Moq;
using Xunit;

namespace AnnoDesigner.Core.Tests
{
    public class BuildingInfoComparerTests
    {
        private const string GROUP_FIRST = "Production";
        private const string GROUP_SECOND = "Public Service";

        #region Equals tests

        [Fact]
        public void Implements_Interface_IEqualityComparer()
        {
            Assert.True(typeof(IEqualityComparer<IBuildingInfo>).IsAssignableFrom(typeof(BuildingInfoComparer)));
        }

        [Fact]
        public void Equals_BothEqual_ShouldReturnTrue()
        {
            // Arrange
            var mockedElement1 = new Mock<IBuildingInfo>();
            mockedElement1.SetupGet(x => x.Group).Returns(GROUP_FIRST);

            var mockedElement2 = new Mock<IBuildingInfo>();
            mockedElement2.SetupGet(x => x.Group).Returns(GROUP_FIRST);

            BuildingInfoComparer comparer = new BuildingInfoComparer();

            // Act/Assert
            Assert.True(comparer.Equals(mockedElement1.Object, mockedElement2.Object));
        }

        [Fact]
        public void Equals_OneIsNull_ShouldReturnFalse()
        {
            // Arrange
            var mockedElement = new Mock<IBuildingInfo>();
            mockedElement.SetupGet(x => x.Group).Returns(GROUP_FIRST);

            BuildingInfoComparer comparer = new BuildingInfoComparer();

            // Act/Assert
            Assert.False(comparer.Equals(mockedElement.Object, null));
            Assert.False(comparer.Equals(null, mockedElement.Object));
        }

        [Fact]
        public void Equals_BothAreNull_ShouldReturnTrue()
        {
            // Arrange
            BuildingInfoComparer comparer = new BuildingInfoComparer();

            // Act/Assert
            Assert.True(comparer.Equals(null, null));
        }

        [Fact]
        public void Equals_DifferentGroup_ShouldReturnFalse()
        {
            // Arrange
            var mockedElement1 = new Mock<IBuildingInfo>();
            mockedElement1.SetupGet(x => x.Group).Returns(GROUP_FIRST);

            var mockedElement2 = new Mock<IBuildingInfo>();
            mockedElement2.SetupGet(x => x.Group).Returns(GROUP_SECOND);

            BuildingInfoComparer comparer = new BuildingInfoComparer();

            // Act/Assert
            Assert.False(comparer.Equals(mockedElement1.Object, mockedElement2.Object));
        }

        #endregion

        #region GetHashCode tests

        [Fact]
        public void GetHashCode_BothEqual_ShouldBeEqual()
        {
            // Arrange
            var mockedElement1 = new Mock<IBuildingInfo>();
            mockedElement1.SetupGet(x => x.Group).Returns(GROUP_FIRST);

            var mockedElement2 = new Mock<IBuildingInfo>();
            mockedElement2.SetupGet(x => x.Group).Returns(GROUP_FIRST);

            BuildingInfoComparer comparer = new BuildingInfoComparer();

            // Act
            var hashCode1 = comparer.GetHashCode(mockedElement1.Object);
            var hashCode2 = comparer.GetHashCode(mockedElement2.Object);

            // Assert
            Assert.Equal(hashCode1, hashCode2);
        }

        [Fact]
        public void GetHashCode_DifferentGroup_ShouldNotBeEqual()
        {
            // Arrange
            var mockedElement1 = new Mock<IBuildingInfo>();
            mockedElement1.SetupGet(x => x.Group).Returns(GROUP_FIRST);

            var mockedElement2 = new Mock<IBuildingInfo>();
            mockedElement2.SetupGet(x => x.Group).Returns(GROUP_SECOND);

            BuildingInfoComparer comparer = new BuildingInfoComparer();

            // Act
            var hashCode1 = comparer.GetHashCode(mockedElement1.Object);
            var hashCode2 = comparer.GetHashCode(mockedElement2.Object);

            // Assert
            Assert.NotEqual(hashCode1, hashCode2);
        }

        [Fact]
        public void GetHashCode_ElementIsNull_ShouldNotThrow()
        {
            // Arrange
            BuildingInfoComparer comparer = new BuildingInfoComparer();

            // Act
            var hashCode = comparer.GetHashCode(null);

            // Assert
            Assert.Equal(-1, hashCode);
        }

        #endregion
    }
}
