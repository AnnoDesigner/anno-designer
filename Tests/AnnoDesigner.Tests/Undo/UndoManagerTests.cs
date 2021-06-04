using System;
using AnnoDesigner.Undo;
using AnnoDesigner.Undo.Operations;
using Moq;
using Xunit;

namespace AnnoDesigner.Tests
{
    public class UndoManagerTests
    {
        public UndoManager UndoManager => new UndoManager();

        private void DoNothing()
        {

        }

        [Fact]
        public void Undo_EmptyStack_DoesNotFail()
        {
            var manager = UndoManager;

            manager.Undo();

            // didn't throw exception
        }

        [Fact]
        public void Undo_NotEmptyStack_MovesOperationToRedoStack()
        {
            var manager = UndoManager;
            manager.UndoStack.Push(Mock.Of<IOperation>());

            manager.Undo();

            Assert.Empty(manager.UndoStack);
            Assert.NotEmpty(manager.RedoStack);
        }

        [Fact]
        public void Undo_NotEmptyStack_CallsUndoOnOperation()
        {
            var manager = UndoManager;
            var operationMock = new Mock<IOperation>();
            manager.UndoStack.Push(operationMock.Object);

            manager.Undo();

            operationMock.Verify(op => op.Undo(), Times.Once());
        }

        [Fact]
        public void Redo_EmptyStack_DoesNotFail()
        {
            var manager = UndoManager;

            manager.Redo();

            // didn't throw exception
        }

        [Fact]
        public void Redo_NotEmptyStack_MovesOperationToUndoStack()
        {
            var manager = UndoManager;
            manager.RedoStack.Push(Mock.Of<IOperation>());

            manager.Redo();

            Assert.Empty(manager.RedoStack);
            Assert.NotEmpty(manager.UndoStack);
        }

        [Fact]
        public void Redo_NotEmptyStack_CallsRedoOnOperation()
        {
            var manager = UndoManager;
            var operationMock = new Mock<IOperation>();
            manager.RedoStack.Push(operationMock.Object);

            manager.Redo();

            operationMock.Verify(op => op.Redo(), Times.Once());
        }

        [Fact]
        public void Clear_ClearsUndoStack()
        {
            var manager = UndoManager;
            manager.UndoStack.Push(Mock.Of<IOperation>());

            manager.Clear();

            Assert.Empty(manager.UndoStack);
        }

        [Fact]
        public void Clear_ClearsRedoStack()
        {
            var manager = UndoManager;
            manager.RedoStack.Push(Mock.Of<IOperation>());

            manager.Clear();

            Assert.Empty(manager.RedoStack);
        }

        [Fact]
        public void RegisterOperation_NotGatheringOperations_AddsToUndoStack()
        {
            var manager = UndoManager;

            manager.RegisterOperation(Mock.Of<IOperation>());

            Assert.NotEmpty(manager.UndoStack);
        }

        [Fact]
        public void RegisterOperation_NotGatheringOperations_ClearsRedoStack()
        {
            var manager = UndoManager;
            manager.RedoStack.Push(Mock.Of<IOperation>());

            manager.RegisterOperation(Mock.Of<IOperation>());

            Assert.Empty(manager.RedoStack);
        }

        [Fact]
        public void RegisterOperation_GatheringOperations_DoesNotAddToUndoStack()
        {
            var manager = UndoManager;
            manager.GatheredOperation = new CompositeOperation();

            manager.RegisterOperation(Mock.Of<IOperation>());

            Assert.Empty(manager.UndoStack);
        }

        [Fact]
        public void RegisterOperation_GatheringOperations_AddsToGatheredOperation()
        {
            var manager = UndoManager;
            manager.GatheredOperation = new CompositeOperation();

            manager.RegisterOperation(Mock.Of<IOperation>());

            Assert.NotEmpty(manager.GatheredOperation.Operations);
        }

        [Fact]
        public void AsSingleUndoableOperation_NoOperationRegistered_DoesNotAddToUndoStack()
        {
            var manager = UndoManager;

            manager.AsSingleUndoableOperation(DoNothing);

            Assert.Empty(manager.UndoStack);
        }

        [Fact]
        public void AsSingleUndoableOperation_NoOperationRegistered_CleansAfterItself()
        {
            var manager = UndoManager;

            manager.AsSingleUndoableOperation(DoNothing);

            Assert.Null(manager.GatheredOperation);
        }

        [Fact]
        public void AsSingleUndoableOperation_OperationRegistered_AddsToUndoStack()
        {
            var manager = UndoManager;

            manager.AsSingleUndoableOperation(() =>
            {
                manager.RegisterOperation(Mock.Of<IOperation>());
            });

            Assert.NotEmpty(manager.UndoStack);
        }

        [Fact]
        public void AsSingleUndoableOperation_OperationRegistered_CleansAfterItself()
        {
            var manager = UndoManager;

            manager.AsSingleUndoableOperation(() => {
                manager.RegisterOperation(Mock.Of<IOperation>());
            });

            Assert.Null(manager.GatheredOperation);
        }

        [Fact]
        public void AsSingleUndoableOperation_ExceptionThrown_ExceptionPassedThrough()
        {
            var manager = UndoManager;

            Assert.Throws<Exception>(() => {
                manager.AsSingleUndoableOperation(() =>
                {
                    throw new Exception();
                });
            });
        }

        [Fact]
        public void AsSingleUndoableOperation_ExceptionThrown_DoesNotAddToUndoStack()
        {
            var manager = UndoManager;

            try
            {
                manager.AsSingleUndoableOperation(() =>
                {
                    manager.RegisterOperation(Mock.Of<IOperation>());
                    throw new Exception();
                });
            }
            catch
            {

            }

            Assert.Empty(manager.UndoStack);
        }

        [Fact]
        public void AsSingleUndoableOperation_ExceptionThrown_CleansAfterItself()
        {
            var manager = UndoManager;

            try
            {
                manager.AsSingleUndoableOperation(() =>
                {
                    throw new Exception();
                });
            }
            catch
            {

            }

            Assert.Null(manager.GatheredOperation);
        }
    }
}
