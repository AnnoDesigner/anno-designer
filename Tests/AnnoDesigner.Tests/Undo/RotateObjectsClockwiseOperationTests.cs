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
    public class RotateObjectsClockwiseOperationTests
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
        public void Undo_SingleObject_ShouldRotateObject()
        {
            // Arrange
            var collection = Collection;
            var obj = CreateLayoutObject(1, 2, 3, 4);
            collection.Insert(obj, obj.GridRect);

            var operation = new RotateObjectsClockwiseOperation()
            {
                Collection = collection,
                Objects = new List<LayoutObject>()
                {
                    obj
                }
            };

            // Act
            operation.Undo();

            // Assert
            Assert.Equal(new Rect(2, -4, 4, 3), obj.GridRect);
        }

        [Fact]
        public void Undo_MultipleObjects_ShouldRotateObjects()
        {
            // Arrange
            var collection = Collection;
            var obj1 = CreateLayoutObject(1, 2, 3, 4);
            var obj2 = CreateLayoutObject(5, 6, 7, 8);
            collection.Insert(obj1, obj1.GridRect);
            collection.Insert(obj2, obj2.GridRect);

            var operation = new RotateObjectsClockwiseOperation()
            {
                Collection = collection,
                Objects = new List<LayoutObject>()
                {
                    obj1,
                    obj2
                }
            };

            // Act
            operation.Undo();

            // Assert
            Assert.Equal(new Rect(2, -4, 4, 3), obj1.GridRect);
            Assert.Equal(new Rect(6, -12, 8, 7), obj2.GridRect);
        }

        #endregion

        #region Redo tests

        [Fact]
        public void Redo_SingleObject_ShouldRotateObject()
        {
            // Arrange
            var collection = Collection;
            var obj = CreateLayoutObject(1, 2, 3, 4);
            collection.Insert(obj, obj.GridRect);

            var operation = new RotateObjectsClockwiseOperation()
            {
                Collection = collection,
                Objects = new List<LayoutObject>()
                {
                    obj
                }
            };

            // Act
            operation.Redo();

            // Assert
            Assert.Equal(new Rect(-6, 1, 4, 3), obj.GridRect);
        }

        [Fact]
        public void Redo_MultipleObjects_ShouldRotateObjects()
        {
            // Arrange
            var collection = Collection;
            var obj1 = CreateLayoutObject(1, 2, 3, 4);
            var obj2 = CreateLayoutObject(5, 6, 7, 8);
            collection.Insert(obj1, obj1.GridRect);
            collection.Insert(obj2, obj2.GridRect);

            var operation = new RotateObjectsClockwiseOperation()
            {
                Collection = collection,
                Objects = new List<LayoutObject>()
                {
                    obj1,
                    obj2
                }
            };

            // Act
            operation.Redo();

            // Assert
            Assert.Equal(new Rect(-6, 1, 4, 3), obj1.GridRect);
            Assert.Equal(new Rect(-14, 5, 8, 7), obj2.GridRect);
        }

        #endregion
    }
}
