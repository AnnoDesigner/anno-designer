using System;
using System.Collections.Generic;
using System.Windows;
using AnnoDesigner.Core.DataStructures;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Models;
using AnnoDesigner.Undo.Operations;
using Moq;
using Xunit;

namespace AnnoDesigner.Tests.Undo
{
    public class MoveObjectsOperationTests
    {
        private QuadTree<LayoutObject> Collection => new QuadTree<LayoutObject>(new Rect(-16, -16, 32, 32));

        private LayoutObject CreateLayoutObject(double x, double y, double width, double height)
        {
            return new LayoutObject(new AnnoObject()
            {
                Position = new Point(x, y),
                Size = new Size(width, height)
            }, Mock.Of<ICoordinateHelper>(), Mock.Of<IBrushCache>(), Mock.Of<IPenCache>());
        }

        #region Undo tests

        [Fact]
        public void Undo_SingleObject_ShouldMoveObject()
        {
            // Arrange
            var expectedRect = new Rect(-1, -2, 4, 3);

            var collection = Collection;
            var obj = CreateLayoutObject(1, 2, 3, 4);
            collection.Add(obj);

            var operation = new MoveObjectsOperation<LayoutObject>()
            {
                QuadTree = collection,
                ObjectPropertyValues = new List<(LayoutObject, Rect, Rect)>()
                {
                    (obj, expectedRect, obj.Bounds)
                }
            };

            // Act
            operation.Undo();

            // Assert
            Assert.Equal(expectedRect, obj.Bounds);
        }

        [Fact]
        public void Undo_MultipleObjects_ShouldMoveObjects()
        {
            // Arrange
            var expectedRect1 = new Rect(-1, -2, 4, 3);
            var expectedRect2 = new Rect(-3, -4, 8, 7);

            var collection = Collection;
            var obj1 = CreateLayoutObject(1, 2, 3, 4);
            var obj2 = CreateLayoutObject(5, 6, 7, 8);
            collection.Add(obj1);
            collection.Add(obj2);

            var operation = new MoveObjectsOperation<LayoutObject>()
            {
                QuadTree = collection,
                ObjectPropertyValues = new List<(LayoutObject, Rect, Rect)>()
                {
                    (obj1, expectedRect1, obj1.Bounds),
                    (obj2, expectedRect2, obj2.Bounds)
                }
            };

            // Act
            operation.Undo();

            // Assert
            Assert.Equal(expectedRect1, obj1.Bounds);
            Assert.Equal(expectedRect2, obj2.Bounds);
        }

        [Fact]
        public void Undo_QuadTreeReindex_ShouldBeCalled()
        {
            // Arrange
            var collection = new Mock<IQuadTree<LayoutObject>>();
            var obj = CreateLayoutObject(1, 2, 3, 4);
            var operation = new MoveObjectsOperation<LayoutObject>()
            {
                QuadTree = collection.Object,
                ObjectPropertyValues = new List<(LayoutObject, Rect, Rect)>()
                {
                    (obj, new Rect(-1, -2, 4, 3), obj.Bounds)
                }
            };

            // Act
            operation.Undo();

            // Assert
            collection.Verify(c => c.ReIndex(It.IsAny<LayoutObject>(), It.IsAny<Rect>()), Times.Once());
        }

        #endregion

        #region Redo tests

        [Fact]
        public void Redo_SingleObject_ShouldMoveObject()
        {
            // Arrange
            var expectedRect = new Rect(-1, -2, 4, 3);

            var collection = Collection;
            var obj = CreateLayoutObject(1, 2, 3, 4);
            collection.Add(obj);

            var operation = new MoveObjectsOperation<LayoutObject>()
            {
                QuadTree = collection,
                ObjectPropertyValues = new List<(LayoutObject, Rect, Rect)>()
                {
                    (obj, obj.Bounds, expectedRect)
                }
            };

            // Act
            operation.Redo();

            // Assert
            Assert.Equal(expectedRect, obj.Bounds);
        }

        [Fact]
        public void Redo_MultipleObjects_ShouldMoveObjects()
        {
            // Arrange
            var expectedRect1 = new Rect(-1, -2, 4, 3);
            var expectedRect2 = new Rect(-3, -4, 8, 7);

            var collection = Collection;
            var obj1 = CreateLayoutObject(1, 2, 3, 4);
            var obj2 = CreateLayoutObject(5, 6, 7, 8);
            collection.Add(obj1);
            collection.Add(obj2);

            var operation = new MoveObjectsOperation<LayoutObject>()
            {
                QuadTree = collection,
                ObjectPropertyValues = new List<(LayoutObject, Rect, Rect)>()
                {
                    (obj1, obj1.Bounds, expectedRect1),
                    (obj2, obj2.Bounds, expectedRect2)
                }
            };

            // Act
            operation.Redo();

            // Assert
            Assert.Equal(expectedRect1, obj1.Bounds);
            Assert.Equal(expectedRect2, obj2.Bounds);
        }

        [Fact]
        public void Redo_QuadTreeReindex_ShouldBeCalled()
        {
            // Arrange
            var collection = new Mock<IQuadTree<LayoutObject>>();
            var obj = CreateLayoutObject(1, 2, 3, 4);
            var operation = new MoveObjectsOperation<LayoutObject>()
            {
                QuadTree = collection.Object,
                ObjectPropertyValues = new List<(LayoutObject, Rect, Rect)>()
                {
                    (obj, obj.Bounds, new Rect(-1, -2, 4, 3))
                }
            };

            // Act
            operation.Redo();

            // Assert
            collection.Verify(c => c.ReIndex(It.IsAny<LayoutObject>(), It.IsAny<Rect>()), Times.Once());
        }

        #endregion
    }
}
