using System;
using System.Collections.Generic;
using System.Windows.Media;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Models;
using AnnoDesigner.Undo.Operations;
using Moq;
using Xunit;

namespace AnnoDesigner.Tests
{
    public class ChangeObjectsColorOperationTests
    {
        private LayoutObject CreateLayoutObject(Color color)
        {
            return new LayoutObject(new AnnoObject()
            {
                Color = color
            }, Mock.Of<ICoordinateHelper>(), Mock.Of<IBrushCache>(), Mock.Of<IPenCache>());
        }

        [Fact]
        public void Undo_SingleObject_ObjectsRecolored()
        {
            var obj = CreateLayoutObject(Colors.White);
            var operation = new ChangeObjectsColorOperation()
            {
                ObjectColors = new List<(LayoutObject, SerializableColor, Color?)>()
                {
                    (obj, Colors.Black, obj.Color)
                }
            };

            operation.Undo();

            Assert.Equal<SerializableColor>(Colors.Black, obj.Color);
        }

        [Fact]
        public void Undo_RedrawActionCalled()
        {
            var actionMock = new Mock<Action>();
            var operation = new ChangeObjectsColorOperation()
            {
                ObjectColors = new List<(LayoutObject, SerializableColor, Color?)>(),
                RedrawAction = actionMock.Object
            };

            operation.Undo();

            actionMock.Verify(a => a());
        }

        [Fact]
        public void Undo_MultipleObjects_ObjectsRecolored()
        {
            var obj1 = CreateLayoutObject(Colors.White);
            var obj2 = CreateLayoutObject(Colors.Red);
            var operation = new ChangeObjectsColorOperation()
            {
                ObjectColors = new List<(LayoutObject, SerializableColor, Color?)>()
                {
                    (obj1, Colors.Black, obj1.Color),
                    (obj2, Colors.Blue, obj2.Color),
                }
            };

            operation.Undo();

            Assert.Equal<SerializableColor>(Colors.Black, obj1.Color);
            Assert.Equal<SerializableColor>(Colors.Blue, obj2.Color);
        }

        [Fact]
        public void Redo_SingleObject_ObjectsRecolored()
        {
            var obj = CreateLayoutObject(Colors.White);
            var operation = new ChangeObjectsColorOperation()
            {
                ObjectColors = new List<(LayoutObject, SerializableColor, Color?)>()
                {
                    (obj, obj.Color, Colors.Black)
                }
            };

            operation.Redo();

            Assert.Equal<SerializableColor>(Colors.Black, obj.Color);
        }

        [Fact]
        public void Redo_MultipleObjects_ObjectsRecolored()
        {
            var obj1 = CreateLayoutObject(Colors.White);
            var obj2 = CreateLayoutObject(Colors.Red);
            var operation = new ChangeObjectsColorOperation()
            {
                ObjectColors = new List<(LayoutObject, SerializableColor, Color?)>()
                {
                    (obj1, obj1.Color, Colors.Black),
                    (obj2, obj2.Color, Colors.Blue),
                }
            };

            operation.Redo();

            Assert.Equal<SerializableColor>(Colors.Black, obj1.Color);
            Assert.Equal<SerializableColor>(Colors.Blue, obj2.Color);
        }

        [Fact]
        public void Redo_RedrawActionCalled()
        {
            var actionMock = new Mock<Action>();
            var operation = new ChangeObjectsColorOperation()
            {
                ObjectColors = new List<(LayoutObject, SerializableColor, Color?)>(),
                RedrawAction = actionMock.Object
            };

            operation.Redo();

            actionMock.Verify(a => a());
        }
    }
}
