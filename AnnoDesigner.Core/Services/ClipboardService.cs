using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using AnnoDesigner.Core.Layout;
using AnnoDesigner.Core.Layout.Models;
using AnnoDesigner.Core.Models;
using Newtonsoft.Json;

namespace AnnoDesigner.Core.Services
{
    public class ClipboardService : IClipboardService
    {
        public void Copy(IEnumerable<AnnoObject> objects)
        {
            if (!objects.Any())
            {
                using var memoryStream = new MemoryStream();
                new LayoutLoader().SaveLayout(new LayoutFile(objects), memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);
                Clipboard.SetData(CoreConstants.AnnoDesignerClipboardFormat, memoryStream);
            }
        }

        public ICollection<AnnoObject> Paste()
        {
            var loader = new LayoutLoader();

            var files = Clipboard.GetFileDropList();
            if (files.Count == 1)
            {
                try
                {
                    return loader.LoadLayout(files[0], true).Objects;
                }
                catch (JsonReaderException) { }
            }

            if (Clipboard.ContainsData(CoreConstants.AnnoDesignerClipboardFormat))
            {
                var stream = Clipboard.GetData(CoreConstants.AnnoDesignerClipboardFormat) as Stream;
                try
                {
                    return loader.LoadLayout(stream, true).Objects;
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
                    return loader.LoadLayout(memoryStream, true).Objects;
                }
                catch (JsonReaderException) { }
            }

            return Array.Empty<AnnoObject>();
        }
    }
}
