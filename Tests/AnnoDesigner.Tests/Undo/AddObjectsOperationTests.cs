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
    public class AddObjectsOperationTests
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
        public void Undo_AddSingleObject_ShouldRemoveObjectFromCollection()
        {
            // Arrange
            var collection = Collection;
            var obj = CreateLayoutObject(5, 5, 2, 2);
            collection.Add(obj);
            var operation = new AddObjectsOperation<LayoutObject>()
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
            Assert.Empty(collection);
        }

        [Fact]
        public void Undo_AddMultipleObjects_ShouldRemoveObjectsFromCollection()
        {
            // Arrange
            var collection = Collection;
            var obj1 = CreateLayoutObject(5, 5, 2, 2);
            var obj2 = CreateLayoutObject(0, 0, 2, 2);
            collection.Add(obj1);
            collection.Add(obj2);
            var operation = new AddObjectsOperation<LayoutObject>()
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
            Assert.Empty(collection);
        }

        #endregion

        #region Redo tests

        [Fact]
        public void Redo_AddSingleObject_ShouldAddObjectToCollection()
        {
            // Arrange
            var collection = Collection;
            Assert.Empty(collection);
            var obj = CreateLayoutObject(5, 5, 2, 2);
            var operation = new AddObjectsOperation<LayoutObject>()
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
            Assert.Single(collection);
        }

        [Fact]
        public void Redo_AddMultipleObjects_ShouldAddObjectsToCollection()
        {
            // Arrange
            var collection = Collection;
            Assert.Empty(collection);
            var obj1 = CreateLayoutObject(5, 5, 2, 2);
            var obj2 = CreateLayoutObject(0, 0, 2, 2);
            var operation = new AddObjectsOperation<LayoutObject>()
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
            Assert.Equal(2, collection.Count);
        }

        #endregion
    }
}
