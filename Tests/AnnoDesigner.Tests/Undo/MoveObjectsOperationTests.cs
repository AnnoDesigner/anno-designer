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

        [Fact]
        public void Undo_SingleObject_ObjectsMoved()
        {
            var collection = Collection;
            var obj = CreateLayoutObject(1, 2, 3, 4);
            collection.Insert(obj, obj.GridRect);
            var operation = new MoveObjectsOperation()
            {
                Collection = collection,
                ObjectPositions = new List<(LayoutObject, Point, Point)>()
                {
                    (obj, new Point(-1, -2), obj.Position)
                }
            };

            operation.Undo();

            Assert.Equal(new Point(-1, -2), obj.Position);
        }

        [Fact]
        public void Undo_MultipleObjects_ObjectsMoved()
        {
            var collection = Collection;
            var obj1 = CreateLayoutObject(1, 2, 3, 4);
            var obj2 = CreateLayoutObject(5, 6, 7, 8);
            collection.Insert(obj1, obj1.GridRect);
            collection.Insert(obj2, obj2.GridRect);
            var operation = new MoveObjectsOperation()
            {
                Collection = collection,
                ObjectPositions = new List<(LayoutObject, Point, Point)>()
                {
                    (obj1, new Point(-1, -2), obj1.Position),
                    (obj2, new Point(-3, -4), obj2.Position)
                }
            };

            operation.Undo();

            Assert.Equal(new Point(-1, -2), obj1.Position);
            Assert.Equal(new Point(-3, -4), obj2.Position);
        }

        [Fact]
        public void Redo_SingleObject_ObjectsMoved()
        {
            var collection = Collection;
            var obj = CreateLayoutObject(1, 2, 3, 4);
            collection.Insert(obj, obj.GridRect);
            var operation = new MoveObjectsOperation()
            {
                Collection = collection,
                ObjectPositions = new List<(LayoutObject, Point, Point)>()
                {
                    (obj, obj.Position, new Point(-1, -2))
                }
            };

            operation.Redo();

            Assert.Equal(new Point(-1, -2), obj.Position);
        }

        [Fact]
        public void Redo_MultipleObjects_ObjectsMoved()
        {
            var collection = Collection;
            var obj1 = CreateLayoutObject(1, 2, 3, 4);
            var obj2 = CreateLayoutObject(5, 6, 7, 8);
            collection.Insert(obj1, obj1.GridRect);
            collection.Insert(obj2, obj2.GridRect);
            var operation = new MoveObjectsOperation()
            {
                Collection = collection,
                ObjectPositions = new List<(LayoutObject, Point, Point)>()
                {
                    (obj1, obj1.Position, new Point(-1, -2)),
                    (obj2, obj2.Position, new Point(-3, -4))
                }
            };

            operation.Redo();

            Assert.Equal(new Point(-1, -2), obj1.Position);
            Assert.Equal(new Point(-3, -4), obj2.Position);
        }
    }
}
