namespace PresetParser.Models
{
    public class PathRef
    {
        public string Path { get; }
        public string XPath { get; }
        public string YPath { get; } //A secondary path used match xml within a secondary file
        public string InnerNameTag { get; }

        public PathRef(string path) : this(path, null, null, null)
        {
            Path = path;
        }

        public PathRef(string path, string xPath) : this(path, xPath, null, null)
        {
            Path = path;
            XPath = xPath;
        }

        public PathRef(string path, string xPath, string yPath, string innerNameTag)
        {
            Path = path;
            XPath = xPath;
            YPath = yPath;
            InnerNameTag = innerNameTag;
        }
    }
}
