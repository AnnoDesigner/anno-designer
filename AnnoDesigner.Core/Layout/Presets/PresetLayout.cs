using System.Collections.Generic;
using System.Windows.Media;
using AnnoDesigner.Core.Layout.Models;

namespace AnnoDesigner.Core.Layout.Presets
{
    public class PresetLayout : IPresetLayout
    {
        public MultilangInfo Name => Info.Name;

        public LayoutPresetInfo Info { get; set; }

        public string Author { get; set; }

        public string AuthorContact { get; set; }

        public LayoutFile Layout { get; set; }

        public List<ImageSource> Images { get; set; }
    }
}
