using System;
using System.Collections.Generic;
using AnnoDesigner.Core.Models;

namespace AnnoDesigner.Core.Tests.Mocks
{
    public class MockedClipboard : IClipboard
    {
        private List<string> _files = new List<string>();
        private Dictionary<string, object> _data = new Dictionary<string, object>();
        private string _text;

        public void AddFilesToClipboard(List<string> filesToAdd)
        {
            _files.AddRange(filesToAdd);
        }

        public void Clear()
        {
            _files.Clear();
            _data.Clear();
            _text = null;
        }

        public bool ContainsData(string format)
        {
            return _data.ContainsKey(format);
        }

        public bool ContainsText()
        {
            return _text is not null;
        }

        public void Flush()
        {
            //It is a no-op because how should it be implemented/tested?
        }

        public object GetData(string format)
        {
            if (format is null)
            {
                throw new ArgumentNullException(nameof(format));
            }

            if (!_data.ContainsKey(format))
            {
                return null;
            }

            return _data[format];
        }

        public IReadOnlyList<string> GetFileDropList()
        {
            return _files.AsReadOnly();
        }

        public string GetText()
        {
            if (string.IsNullOrEmpty(_text))
            {
                return string.Empty;
            }

            return _text;
        }

        public void SetData(string format, object data)
        {
            if (data is System.IO.Stream stream)
            {
                var copiedData = new System.IO.MemoryStream();
                stream.CopyTo(copiedData);
                copiedData.Seek(0, System.IO.SeekOrigin.Begin);
                _data[format] = copiedData;
                return;
            }

            _data[format] = data;
        }
    }
}
