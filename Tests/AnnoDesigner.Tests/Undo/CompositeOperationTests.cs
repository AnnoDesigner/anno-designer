using System.Collections.Generic;
using AnnoDesigner.Undo.Operations;
using Moq;
using Xunit;

namespace AnnoDesigner.Tests
{
    public class CompositeOperationTests
    {
        [Fact]
        public void Undo_OperationUndoneInCorrectOrder()
        {
            var order = new List<IOperation>();
            var op1 = new Mock<IOperation>();
            var op2 = new Mock<IOperation>();
            op1.Setup(op => op.Undo()).Callback(() => order.Add(op1.Object));
            op2.Setup(op => op.Undo()).Callback(() => order.Add(op2.Object));
            var operation = new CompositeOperation()
            {
                Operations = new List<IOperation>()
                {
                    op1.Object,
                    op2.Object
                }
            };

            operation.Undo();

            Assert.Equal(new[] { op2.Object, op1.Object }, order);
        }

        [Fact]
        public void Redo_OperationRedoneInCorrectOrder()
        {
            var order = new List<IOperation>();
            var op1 = new Mock<IOperation>();
            var op2 = new Mock<IOperation>();
            op1.Setup(op => op.Redo()).Callback(() => order.Add(op1.Object));
            op2.Setup(op => op.Redo()).Callback(() => order.Add(op2.Object));
            var operation = new CompositeOperation()
            {
                Operations = new List<IOperation>()
                {
                    op1.Object,
                    op2.Object
                }
            };

            operation.Redo();

            Assert.Equal(new[] { op1.Object, op2.Object }, order);
        }
    }
}
