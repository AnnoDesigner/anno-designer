using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FandomParser.Core;
using InfoboxParser.Tests.Attributes;
using Xunit;

namespace InfoboxParser.Tests
{
    public class ParserTests
    {
        private static readonly ICommons mockedCommons;

        static ParserTests()
        {
            mockedCommons = Commons.Instance;
        }

        [Theory]
        [InlineData(null)]
        [InlineData(" ")]
        [InlineData("")]
        public void GetInfobox_InputIsNullOrWhiteSpace_ShouldReturnNull(string input)
        {
            // Arrange
            var parser = new Parser(mockedCommons);

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetInfobox_ProducesAmountElectricityIsParseable_ShouldSetAmountElectricity()
        {
            // Arrange
            var input = "|Produces Amount Electricity = 42";

            var parser = new Parser(mockedCommons);

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(42, result[0].ProductionInfos.EndProduct.AmountElectricity);
        }

        [Fact]
        [UseCulture("en-US")]
        public void GetInfobox_ProducesAmountElectricityIsDouble_ShouldSetAmountElectricity()
        {
            // Arrange
            var input = "|Produces Amount Electricity = 42,21";

            var parser = new Parser(mockedCommons);

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(42.21, result[0].ProductionInfos.EndProduct.AmountElectricity);
        }

        [Fact]
        public void GetInfobox_ProducesAmountElectricityIsNotParseable_ShouldNotSetAmountElectricity()
        {
            // Arrange
            var input = "|Produces Amount Electricity = no_number";

            var parser = new Parser(mockedCommons);

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(0, result[0].ProductionInfos.EndProduct.AmountElectricity);
        }

        [Fact]
        public void GetInfobox_ProducesAmountIsParseable_ShouldSetAmount()
        {
            // Arrange
            var input = "|Produces Amount = 42";

            var parser = new Parser(mockedCommons);

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(42, result[0].ProductionInfos.EndProduct.Amount);
        }

        [Fact]
        [UseCulture("en-US")]
        public void GetInfobox_ProducesAmountIsDouble_ShouldSetAmount()
        {
            // Arrange
            var input = "|Produces Amount = 42,21";

            var parser = new Parser(mockedCommons);

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(42.21, result[0].ProductionInfos.EndProduct.Amount);
        }

        [Fact]
        public void GetInfobox_ProducesAmountIsNotParseable_ShouldNotSetAmount()
        {
            // Arrange
            var input = "|Produces Amount = no_number";

            var parser = new Parser(mockedCommons);

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal(0, result[0].ProductionInfos.EndProduct.Amount);
        }

        [Fact]
        public void GetInfobox_ProducesIconIsPresent_ShouldSetIcon()
        {
            // Arrange
            var input = $"|Produces Amount {Environment.NewLine}|Produces Icon = dummy.png";

            var parser = new Parser(mockedCommons);

            // Act
            var result = parser.GetInfobox(input);

            // Assert
            Assert.Equal("dummy.png", result[0].ProductionInfos.EndProduct.Icon);
        }

        [Fact]
        public void GetInfobox_InputCounterIsNotParseable_ShouldThrow()
        {
            // Arrange
            var input = $"|Produces Amount = 1{Environment.NewLine}|Input {int.MaxValue + 1L} Amount = 42";

            var parser = new Parser(mockedCommons);

            // Act/Assert
            var ex = Assert.Throws<Exception>(() => parser.GetInfobox(input));
        }

        [Fact]
        public void GetInfobox_InputElectricityCounterIsNotParseable_ShouldThrow()
        {
            // Arrange
            var input = $"|Produces Amount = 1{Environment.NewLine}|Input {int.MaxValue + 1L} Amount Electricity = 42";

            var parser = new Parser(mockedCommons);

            // Act/Assert
            var ex = Assert.Throws<Exception>(() => parser.GetInfobox(input));
        }

        [Fact]
        public void GetInfobox_InputIconCounterIsNotParseable_ShouldThrow()
        {
            // Arrange
            var input = $"|Produces Amount = 1{Environment.NewLine}|Input {int.MaxValue + 1L} Icon = dummy.png";

            var parser = new Parser(mockedCommons);

            // Act/Assert
            var ex = Assert.Throws<Exception>(() => parser.GetInfobox(input));
        }

        [Fact]
        public void GetInfobox_SupplyAmountCounterIsNotParseable_ShouldThrow()
        {
            // Arrange
            var input = $"|Supplies {int.MaxValue + 1L} Amount = 42";

            var parser = new Parser(mockedCommons);

            // Act/Assert
            var ex = Assert.Throws<Exception>(() => parser.GetInfobox(input));
        }

        [Fact]
        public void GetInfobox_SupplyAmountElectricityCounterIsNotParseable_ShouldThrow()
        {
            // Arrange
            var input = $"|Supplies {int.MaxValue + 1L} Amount Electricity = 42";

            var parser = new Parser(mockedCommons);

            // Act/Assert
            var ex = Assert.Throws<Exception>(() => parser.GetInfobox(input));
        }

        [Fact]
        public void GetInfobox_SupplyTypeCounterIsNotParseable_ShouldThrow()
        {
            // Arrange
            var input = $"|Supplies {int.MaxValue + 1L} Type = Farmer";

            var parser = new Parser(mockedCommons);

            // Act/Assert
            var ex = Assert.Throws<Exception>(() => parser.GetInfobox(input));
        }

        [Fact]
        public void GetInfobox_UnlockAmountCounterIsNotParseable_ShouldThrow()
        {
            // Arrange
            var input = $"|Unlock Condition {int.MaxValue + 1L} Amount = 42";

            var parser = new Parser(mockedCommons);

            // Act/Assert
            var ex = Assert.Throws<Exception>(() => parser.GetInfobox(input));
        }

        [Fact]
        public void GetInfobox_UnlockTypeCounterIsNotParseable_ShouldThrow()
        {
            // Arrange
            var input = $"|Unlock Condition {int.MaxValue + 1L} Type = Farmers";

            var parser = new Parser(mockedCommons);

            // Act/Assert
            var ex = Assert.Throws<Exception>(() => parser.GetInfobox(input));
        }
    }
}
