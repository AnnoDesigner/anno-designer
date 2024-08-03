namespace PresetParser.Models;

public class ExtraRoads : ExtraPreset
{
    public ExtraRoads()
    {
        Road = true;
        Borderless = true;
        Group = null;
        Header = "- Road Presets";
        IconFileName = null;
        Template = "Road";
    }

    public bool Borderless { get; set; }
    public bool Road { get; set; }
}
