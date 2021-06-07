using System;
using AnnoDesigner.Undo;
using AnnoDesigner.Undo.Operations;
using Moq;
using Xunit;

namespace AnnoDesigner.Tests.Undo
{
    public class UndoManagerTests
    {
        private UndoManager _undoManager;

        public UndoManagerTests()
        {
            _undoManager = new UndoManager();
        }

        //private IUndoManager UndoManager => new UndoManager();

        private void DoNothing()
        { }

        #region Undo tests

        [Fact]
        public void Undo_EmptyStack_ShouldNotThrow()
        {
            // Arrange/Act
            var ex = Record.Exception(() => _undoManager.Undo());

            // Assert
            Assert.Null(ex);
        }

        [Fact]
        public void Undo_StackHasSingleOperation_ShouldMoveOperationToRedoStack()
        {
            // Arrange
            _undoManager.UndoStack.Push(Mock.Of<IOperation>());

            // Act
            _undoManager.Undo();

            // Assert
            Assert.Empty(_undoManager.UndoStack);
            Assert.NotEmpty(_undoManager.RedoStack);
        }

        [Fact]
        public void Undo_StackHasSingleOperation_ShouldInvokeUndoOnOperation()
        {
            // Arrange
            var operationMock = new Mock<IOperation>();
            _undoManager.UndoStack.Push(operationMock.Object);

            // Act
            _undoManager.Undo();

            // Assert
            operationMock.Verify(operation => operation.Undo(), Times.Once());
        }

        #endregion

        #region Redo tests

        [Fact]
        public void Redo_EmptyStack_ShouldNotThrow()
        {
            // Arrange/Act
            var ex = Record.Exception(() => _undoManager.Redo());

            // Assert
            Assert.Null(ex);
        }

        [Fact]
        public void Redo_StackHasSingleOperation_ShouldMoveOperationToUndoStack()
        {
            // Arrange
            _undoManager.RedoStack.Push(Mock.Of<IOperation>());

            // Act
            _undoManager.Redo();

            // Assert
            Assert.Empty(_undoManager.RedoStack);
            Assert.NotEmpty(_undoManager.UndoStack);
        }

        [Fact]
        public void Redo_StackHasSingleOperation_ShouldInvokeRedoOnOperation()
        {
            // Arrange
            var operationMock = new Mock<IOperation>();
            _undoManager.RedoStack.Push(operationMock.Object);

            // Act
            _undoManager.Redo();

            // Assert
            operationMock.Verify(operation => operation.Redo(), Times.Once());
        }

        #endregion

        #region Clear tests

        [Fact]
        public void Clear_ShouldClearUndoStack()
        {
            // Arrange
            _undoManager.UndoStack.Push(Mock.Of<IOperation>());

            // Act
            _undoManager.Clear();

            // Assert
            Assert.Empty(_undoManager.UndoStack);
        }

        [Fact]
        public void Clear_ShouldClearRedoStack()
        {
            // Arrange
            _undoManager.RedoStack.Push(Mock.Of<IOperation>());

            // Act
            _undoManager.Clear();

            // Assert
            Assert.Empty(_undoManager.RedoStack);
        }

        #endregion

        #region RegisterOperation tests

        [Fact]
        public void RegisterOperation_NotGatheringOperations_ShouldAddToUndoStack()
        {
            // Arrange/Act
            _undoManager.RegisterOperation(Mock.Of<IOperation>());

            // Assert
            Assert.NotEmpty(_undoManager.UndoStack);
        }

        [Fact]
        public void RegisterOperation_NotGatheringOperations_ShouldClearRedoStack()
        {
            // Arrange
            _undoManager.RedoStack.Push(Mock.Of<IOperation>());
            Assert.NotEmpty(_undoManager.RedoStack);

            // Act
            _undoManager.RegisterOperation(Mock.Of<IOperation>());

            // Assert
            Assert.Empty(_undoManager.RedoStack);
        }

        [Fact]
        public void RegisterOperation_GatheringOperations_ShouldNotAddToUndoStack()
        {
            // Arrange
            _undoManager.GatheredOperation = new CompositeOperation();

            // Act
            _undoManager.RegisterOperation(Mock.Of<IOperation>());

            // Assert
            Assert.Empty(_undoManager.UndoStack);
        }

        [Fact]
        public void RegisterOperation_GatheringOperations_ShouldAddToGatheredOperation()
        {
            // Arrange
            _undoManager.GatheredOperation = new CompositeOperation();

            // Act
            _undoManager.RegisterOperation(Mock.Of<IOperation>());

            // Assert
            Assert.NotEmpty(_undoManager.GatheredOperation.Operations);
        }

        #endregion

        #region AsSingleUndoableOperation tests

        [Fact]
        public void AsSingleUndoableOperation_NoOperationRegistered_ShouldNotAddToUndoStack()
        {
            // Arrange/Act
            _undoManager.AsSingleUndoableOperation(DoNothing);

            // Assert
            Assert.Empty(_undoManager.UndoStack);
        }

        [Fact]
        public void AsSingleUndoableOperation_NoOperationRegistered_ShouldCleanGatheredOperation()
        {
            // Arrange/Act
            _undoManager.AsSingleUndoableOperation(DoNothing);

            // Assert
            Assert.Null(_undoManager.GatheredOperation);
        }

        [Fact]
        public void AsSingleUndoableOperation_OperationRegistered_ShouldAddToUndoStack()
        {
            // Arrange/Act
            _undoManager.AsSingleUndoableOperation(() =>
            {
                _undoManager.RegisterOperation(Mock.Of<IOperation>());
            });

            // Assert
            Assert.NotEmpty(_undoManager.UndoStack);
        }

        [Fact]
        public void AsSingleUndoableOperation_OperationRegistered_ShouldCleanGatheredOperation()
        {
            // Arrange/Act
            _undoManager.AsSingleUndoableOperation(() =>
            {
                _undoManager.RegisterOperation(Mock.Of<IOperation>());
            });

            // Assert
            Assert.Null(_undoManager.GatheredOperation);
        }

        [Fact]
        public void AsSingleUndoableOperation_ExceptionIsThrown_ShouldPassThroughException()
        {
            // Arrange/Act/Assert
            Assert.Throws<Exception>(() =>
            {
                _undoManager.AsSingleUndoableOperation(() =>
                {
                    throw new Exception();
                });
            });
        }

        [Fact]
        public void AsSingleUndoableOperation_ExceptionIsThrown_ShouldNotAddToUndoStack()
        {
            // Arrange/Act
            try
            {
                _undoManager.AsSingleUndoableOperation(() =>
                {
                    _undoManager.RegisterOperation(Mock.Of<IOperation>());
                    throw new Exception();
                });
            }
            catch
            {

            }

            // Assert
            Assert.Empty(_undoManager.UndoStack);
        }

        [Fact]
        public void AsSingleUndoableOperation_ExceptionIsThrown_ShouldCleanGatheredOperation()
        {
            // Arrange/Act
            try
            {
                _undoManager.AsSingleUndoableOperation(() =>
                {
                    throw new Exception();
                });
            }
            catch
            {

            }

            // Assert
            Assert.Null(_undoManager.GatheredOperation);
        }

        #endregion
    }
}
