using System.Collections.Generic;
using System.Windows;
using AnnoDesigner.Core.DataStructures;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Models;
using AnnoDesigner.Undo.Operations;
using Moq;
using Xunit;

namespace AnnoDesigner.Tests
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

        [Fact]
        public void Undo_SingleObject_RemovedObjectsFromCollection()
        {
            var collection = Collection;
            var obj = CreateLayoutObject(5, 5, 2, 2);
            collection.Insert(obj, obj.GridRect);
            var operation = new AddObjectsOperation()
            {
                Collection = collection,
                Objects = new List<LayoutObject>()
                {
                    obj
                }
            };

            operation.Undo();

            Assert.Empty(collection);
        }

        [Fact]
        public void Undo_MultipleObject_RemovedObjectsFromCollection()
        {
            var collection = Collection;
            var obj1 = CreateLayoutObject(5, 5, 2, 2);
            var obj2 = CreateLayoutObject(0, 0, 2, 2);
            collection.Insert(obj1, obj1.GridRect);
            collection.Insert(obj2, obj2.GridRect);
            var operation = new AddObjectsOperation()
            {
                Collection = collection,
                Objects = new List<LayoutObject>()
                {
                    obj1,
                    obj2
                }
            };

            operation.Undo();

            Assert.Empty(collection);
        }

        [Fact]
        public void Redo_SingleObject_AddedObjectsToCollection()
        {
            var collection = Collection;
            var obj = CreateLayoutObject(5, 5, 2, 2);
            var operation = new AddObjectsOperation()
            {
                Collection = collection,
                Objects = new List<LayoutObject>()
                {
                    obj
                }
            };

            operation.Redo();

            Assert.Equal(1, collection.Count());
        }

        [Fact]
        public void Redo_MultipleObject_AddedObjectsToCollection()
        {
            var collection = Collection;
            var obj1 = CreateLayoutObject(5, 5, 2, 2);
            var obj2 = CreateLayoutObject(0, 0, 2, 2);
            var operation = new AddObjectsOperation()
            {
                Collection = collection,
                Objects = new List<LayoutObject>()
                {
                    obj1,
                    obj2
                }
            };

            operation.Redo();

            Assert.Equal(2, collection.Count());
        }
    }
}
