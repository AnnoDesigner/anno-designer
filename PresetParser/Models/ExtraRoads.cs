namespace PresetParser.Models
{
    public class ExtraRoads
    {
        public string Header { get; set; }
        public string Group { get; set; }
        public string Faction { get; set; }
        public string Identifier { get; set; }
        public string IconFileName { get; set; }
        public bool Borderless { get; set; }
        public bool Road { get; set; }
        public int BuildBlockerX { get; set; }
        public int BuildBlockerZ { get; set; }
        public int InfluenceRadius { get; set; }
        public int InfluenceRange { get; set; }
        public string Template { get; set; }
        public string LocaEng { get; set; }
        public string LocaGer { get; set; }
        public string LocaFra { get; set; }
        public string LocaPol { get; set; }
        public string LocaRus { get; set; }
    }
}
