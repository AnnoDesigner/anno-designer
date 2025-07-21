using System.Xml;

namespace PresetParser;

public interface IIfoFileProvider
{
    XmlDocument GetIfoFileContent(string basePath, string variationFilename);
}