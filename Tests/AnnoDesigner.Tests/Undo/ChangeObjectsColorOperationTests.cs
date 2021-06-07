using System;
using System.Collections.Generic;
using System.Windows.Media;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Models;
using AnnoDesigner.Undo.Operations;
using Moq;
using Xunit;

namespace AnnoDesigner.Tests.Undo
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

        #region Undo tests

        [Fact]
        public void Undo_SingleObject_ShouldSetColor()
        {
            // Arrange
            var expectedColor = Colors.Black;
            var obj = CreateLayoutObject(Colors.White);
            var operation = new ChangeObjectsColorOperation()
            {
                ObjectColors = new List<(LayoutObject, SerializableColor, Color?)>()
                {
                    (obj, expectedColor, obj.Color)
                }
            };

            // Act
            operation.Undo();

            // Assert
            Assert.Equal<SerializableColor>(expectedColor, obj.Color);
        }

        [Fact]
        public void Undo_ShouldInvokeRedrawAction()
        {
            // Arrange
            var actionMock = new Mock<Action>();
            var operation = new ChangeObjectsColorOperation()
            {
                ObjectColors = new List<(LayoutObject, SerializableColor, Color?)>(),
                RedrawAction = actionMock.Object
            };

            // Act
            operation.Undo();

            // Assert
            actionMock.Verify(action => action(), Times.Once());
        }

        [Fact]
        public void Undo_MultipleObjects_ShouldSetColors()
        {
            // Arrange
            var expectedColor1 = Colors.Black;
            var expectedColor2 = Colors.Blue;
            var obj1 = CreateLayoutObject(Colors.White);
            var obj2 = CreateLayoutObject(Colors.Red);
            var operation = new ChangeObjectsColorOperation()
            {
                ObjectColors = new List<(LayoutObject, SerializableColor, Color?)>()
                {
                    (obj1, expectedColor1, obj1.Color),
                    (obj2, expectedColor2, obj2.Color),
                }
            };

            // Act
            operation.Undo();

            // Assert
            Assert.Equal<SerializableColor>(expectedColor1, obj1.Color);
            Assert.Equal<SerializableColor>(expectedColor2, obj2.Color);
        }

        #endregion

        #region Redo tests

        [Fact]
        public void Redo_SingleObject_ShouldSetColor()
        {
            // Arrange
            var expectedColor = Colors.Black;
            var obj = CreateLayoutObject(Colors.White);
            var operation = new ChangeObjectsColorOperation()
            {
                ObjectColors = new List<(LayoutObject, SerializableColor, Color?)>()
                {
                    (obj, obj.Color, expectedColor)
                }
            };

            // Act
            operation.Redo();

            // Assert
            Assert.Equal<SerializableColor>(expectedColor, obj.Color);
        }

        [Fact]
        public void Redo_MultipleObjects_ShouldSetColors()
        {
            // Arrange
            var expectedColor1 = Colors.Black;
            var expectedColor2 = Colors.Blue;
            var obj1 = CreateLayoutObject(Colors.White);
            var obj2 = CreateLayoutObject(Colors.Red);
            var operation = new ChangeObjectsColorOperation()
            {
                ObjectColors = new List<(LayoutObject, SerializableColor, Color?)>()
                {
                    (obj1, obj1.Color, expectedColor1),
                    (obj2, obj2.Color, expectedColor2),
                }
            };

            // Act
            operation.Redo();

            // Assert
            Assert.Equal<SerializableColor>(expectedColor1, obj1.Color);
            Assert.Equal<SerializableColor>(expectedColor2, obj2.Color);
        }

        [Fact]
        public void Redo_ShouldInvokeRedrawAction()
        {
            // Arrange
            var actionMock = new Mock<Action>();
            var operation = new ChangeObjectsColorOperation()
            {
                ObjectColors = new List<(LayoutObject, SerializableColor, Color?)>(),
                RedrawAction = actionMock.Object
            };

            // Act
            operation.Redo();

            // Assert
            actionMock.Verify(action => action(), Times.Once());
        }

        #endregion
    }
}
