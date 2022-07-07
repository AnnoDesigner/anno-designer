using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Core.Presets.Models;
using AnnoDesigner.Core.Services;
using AnnoDesigner.Models;
using AnnoDesigner.Undo;
using Moq;
using Xunit;

namespace AnnoDesigner.Tests
{
    public class AnnoCanvasTests
    {
        private readonly IAppSettings _appSettings;
        private readonly ICoordinateHelper _coordinateHelper;
        private readonly IBrushCache _brushCache;
        private readonly IPenCache _penCache;
        private readonly IMessageBoxService _messageBoxService;
        private readonly ILocalizationHelper _localizationHelper;
        private readonly IUndoManager _undoManager;
        private readonly IClipboardService _clipboardService;

        public AnnoCanvasTests()
        {
            var mockedAppSettings = new Mock<IAppSettings>();
            mockedAppSettings.Setup(x => x.ColorGridLines).Returns(() => "{\"Type\":\"Default\",\"Color\":{\"A\":255,\"R\":0,\"G\":0,\"B\":0}}");
            mockedAppSettings.Setup(x => x.ColorObjectBorderLines).Returns(() => "{\"Type\":\"Default\",\"Color\":{\"A\":255,\"R\":0,\"G\":0,\"B\":0}}");
            _appSettings = mockedAppSettings.Object;

            var mockedBrushCache = new Mock<IBrushCache>();
            mockedBrushCache.Setup(x => x.GetSolidBrush(It.IsAny<Color>())).Returns(() => new SolidColorBrush(Colors.Black));
            _brushCache = mockedBrushCache.Object;

            var mockedPenCache = new Mock<IPenCache>();
            mockedPenCache.Setup(x => x.GetPen(It.IsAny<Brush>(), It.IsAny<double>())).Returns(() => new Pen(new SolidColorBrush(Colors.Black), 2));
            _penCache = mockedPenCache.Object;

            _coordinateHelper = new Mock<ICoordinateHelper>().Object;
            _messageBoxService = new Mock<IMessageBoxService>().Object;
            _localizationHelper = new Mock<ILocalizationHelper>().Object;
            _undoManager = new Mock<IUndoManager>().Object;
            _clipboardService = new Mock<IClipboardService>().Object;
        }

        #region helper methods

        private LayoutObject CreateLayoutObject(double x, double y, double width, double height)
        {
            return new LayoutObject(new AnnoObject
            {
                Position = new Point(x, y),
                Size = new Size(width, height)
            }, _coordinateHelper, _brushCache, _penCache);
        }

        private IAnnoCanvas GetCanvas()
        {
            return new AnnoCanvas(new BuildingPresets(),
                new Dictionary<string, IconImage>(),
                _appSettings,
                _coordinateHelper,
                _brushCache,
                _penCache,
                _messageBoxService,
                _localizationHelper,
                _undoManager,
                _clipboardService);
        }

        #endregion

        #region Rotate tests

        [StaFact]
        public void Rotate_NoCurrentObjectsButSelectedObject_ShouldCloneSelectedObject()
        {
            // Arrange
            var canvas = GetCanvas();

            var selectedObject = CreateLayoutObject(0, 0, 2, 2);
            canvas.SelectedObjects.Add(selectedObject);

            Assert.Empty(canvas.CurrentObjects);

            // Act
            canvas.RotateCommand.Execute(null);

            // Assert
            Assert.Single(canvas.CurrentObjects);

            selectedObject.Size = new Size(4, 4);
            Assert.NotEqual(selectedObject.Size, canvas.CurrentObjects[0].Size);
        }

        #endregion
    }
}
