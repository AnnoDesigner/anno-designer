using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace PresetParser
{
    public class IfoFileProvider : IIfoFileProvider
    {
        public XmlDocument GetIfoFileContent(string basePath, string variationFilename)
        {
            var result = new XmlDocument();

            var pathToFileDirectory = Path.Combine(Path.GetDirectoryName(variationFilename), Path.GetFileNameWithoutExtension(variationFilename));
            var pathToFile = Path.Combine(basePath + "/", string.Format("{0}.ifo", pathToFileDirectory));

            if (File.Exists(pathToFile))
            {
                result.Load(pathToFile);
                return result;
            }

            return result;
        }
    }
}
