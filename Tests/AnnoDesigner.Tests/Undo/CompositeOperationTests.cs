using System.Collections.Generic;
using AnnoDesigner.Undo.Operations;
using Moq;
using Xunit;

namespace AnnoDesigner.Tests.Undo
{
    public class CompositeOperationTests
    {
        #region Undo tests

        [Fact]
        public void Undo_ShouldUndoOperationsInCorrectOrder()
        {
            // Arrange
            var order = new List<IOperation>();

            var op1 = new Mock<IOperation>();
            _ = op1.Setup(op => op.Undo()).Callback(() => order.Add(op1.Object));
            var op2 = new Mock<IOperation>();
            _ = op2.Setup(op => op.Undo()).Callback(() => order.Add(op2.Object));

            var operation = new CompositeOperation(new List<IOperation>()
            {
                op1.Object,
                op2.Object
            });

            // Act
            operation.Undo();

            // Assert
            Assert.Equal(new[] { op2.Object, op1.Object }, order);
        }

        #endregion

        #region Redo tests

        [Fact]
        public void Redo_ShouldRedoOperationsInCorrectOrder()
        {
            // Arrange
            var order = new List<IOperation>();

            var op1 = new Mock<IOperation>();
            _ = op1.Setup(op => op.Redo()).Callback(() => order.Add(op1.Object));
            var op2 = new Mock<IOperation>();
            _ = op2.Setup(op => op.Redo()).Callback(() => order.Add(op2.Object));

            var operation = new CompositeOperation(new List<IOperation>()
            {
                op1.Object,
                op2.Object
            });

            // Act
            operation.Redo();

            // Assert
            Assert.Equal(new[] { op1.Object, op2.Object }, order);
        }

        #endregion
    }
}
