namespace PresetParser.Models
{
    public class BlockingTile : ExtraPreset
    {
        public BlockingTile()
        {
            Faction = "Common";
            Group = null;
            Header = "(a0)- Blocking Presets";
            IconFileName = null;
            Template = "Blocker";
            Borderless = true;
            Road = false;
        }

        public bool Borderless { get; set; }
        public bool Road { get; set; }
    }
}
