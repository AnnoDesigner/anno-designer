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

            var filePath = Path.Combine(Path.GetDirectoryName(variationFilename), Path.GetFileNameWithoutExtension(variationFilename));

            if (File.Exists(Path.Combine(basePath + "/", string.Format("{0}.ifo", filePath)))) { 
                result.Load(Path.Combine(basePath + "/", string.Format("{0}.ifo", filePath)));
                return result;
            }

            return (result);
        }
    }
}
