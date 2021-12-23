using System.Collections.Generic;

namespace AnnoDesigner.Core.Layout.Presets
{
    public class PresetLayoutDirectory : IPresetLayout
    {
        public MultilangInfo Name { get; set; }

        public List<IPresetLayout> Presets { get; set; }
    }
}
