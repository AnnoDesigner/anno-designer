using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using AnnoDesigner.Core.Layout.Models;
using AnnoDesigner.Core.Models;
using Newtonsoft.Json;

namespace AnnoDesigner.Core.Services
{
    public class ClipboardService : IClipboardService
    {
        private readonly ILayoutLoader _layoutLoader;

        public ClipboardService(ILayoutLoader layoutLoaderToUse)
        {
            _layoutLoader = layoutLoaderToUse;
        }

        public void Copy(IEnumerable<AnnoObject> objects)
        {
            if (objects.Any())
            {
                using var memoryStream = new MemoryStream();
                _layoutLoader.SaveLayout(new LayoutFile(objects), memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);
                Clipboard.Clear();
                Clipboard.SetData(CoreConstants.AnnoDesignerClipboardFormat, memoryStream);
                Clipboard.Flush();
            }
        }

        public ICollection<AnnoObject> Paste()
        {
            var files = Clipboard.GetFileDropList();
            if (files?.Count == 1)
            {
                try
                {
                    return _layoutLoader.LoadLayout(files[0], forceLoad: true).Objects;
                }
                catch (JsonReaderException) { }
            }

            if (Clipboard.ContainsData(CoreConstants.AnnoDesignerClipboardFormat))
            {
                var stream = Clipboard.GetData(CoreConstants.AnnoDesignerClipboardFormat) as Stream;
                try
                {
                    return _layoutLoader.LoadLayout(stream, forceLoad: true).Objects;
                }
                catch (JsonReaderException) { }
            }

            if (Clipboard.ContainsText())
            {
                using var memoryStream = new MemoryStream();
                using var streamWriter = new StreamWriter(memoryStream);
                streamWriter.Write(Clipboard.GetText());
                streamWriter.Flush();
                memoryStream.Seek(0, SeekOrigin.Begin);
                try
                {
                    return _layoutLoader.LoadLayout(memoryStream, forceLoad: true).Objects;
                }
                catch (JsonReaderException) { }
            }

            return Array.Empty<AnnoObject>();
        }
    }
}
