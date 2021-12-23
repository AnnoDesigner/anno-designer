namespace AnnoDesigner.Core.Layout.Presets
{
    public class LayoutPresetInfo
    {
        public MultilangInfo Name { get; set; }

        public MultilangInfo Description { get; set; }

        public string Author { get; set; }

        public string AuthorContact { get; set; }

        public LayoutPresetInfo() { }

        public LayoutPresetInfo(string name)
        {
            Name = name;
        }
    }
}
