using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace PresetParser;

public class IfoFileProvider : IIfoFileProvider
{
    private readonly FileSystem _fileSystem;

    public IfoFileProvider()
    {
        _fileSystem = new FileSystem();
    }

    public XmlDocument GetIfoFileContent(string basePath, string variationFilename)
    {
        XmlDocument result = new();

        string pathToFileDirectory = _fileSystem.Path.Combine(_fileSystem.Path.GetDirectoryName(variationFilename), _fileSystem.Path.GetFileNameWithoutExtension(variationFilename));
        string pathToFile = _fileSystem.Path.Combine(basePath + "/", string.Format("{0}.ifo", pathToFileDirectory));

        if (_fileSystem.File.Exists(pathToFile))
        {
            result.Load(pathToFile);
            return result;
        }

        return result;
    }
}
