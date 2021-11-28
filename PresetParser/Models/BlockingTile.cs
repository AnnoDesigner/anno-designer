namespace PresetParser.Models
{
    public class BlockingTile : ExtraPreset
    {
        public BlockingTile()
        {
            Faction = "Blocker";
            Group = null;
            Header = "- Blocking Presets";
            IconFileName = null;
            Template = "Blocker";
            Borderless = true;
            Road = false;
        }

        public bool Borderless { get; set; }
        public bool Road { get; set; }
    }
}
