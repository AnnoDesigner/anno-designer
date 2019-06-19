using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using AnnoDesigner.Core.Helper;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Core.Presets.Models;

namespace PresetParser.Anno1404_Anno2070
{
    public class IconNameMapper
    {
        public void WriteIconNameMapping(IconFileNameHelper iconFileNameHelper, bool isTest, IEnumerable<XmlNode> iconNodes, Dictionary<string, SerializableDictionary<string>> localizations, string annoVersion, string BUILDING_PRESETS_VERSION)
        {
            var iconNameMappings = new IconMappingPresets()
            {
                Version = BUILDING_PRESETS_VERSION
            };

            foreach (var iconNode in iconNodes)
            {
                var guid = iconNode["GUID"].InnerText;
                var iconFilename = iconFileNameHelper.GetIconFilename(iconNode["Icons"].FirstChild, annoVersion);
                if (!localizations.ContainsKey(guid) || iconNameMappings.IconNameMappings.Exists(_ => _.IconFilename == iconFilename))
                {
                    continue;
                }

                iconNameMappings.IconNameMappings.Add(new IconNameMap
                {
                    IconFilename = iconFilename,
                    Localizations = localizations[guid]
                });
            }

            if (!isTest)
            {
                var fileName = "icons-Anno" + annoVersion + "-v" + BUILDING_PRESETS_VERSION + ".json";
                SerializationHelper.SaveToFile(iconNameMappings, fileName);
                Console.WriteLine($"saved icon name mapping file: {fileName}");
            }
            else
            {
                Console.WriteLine("THIS IS A TEST: No icons.json file is writen");
            }

        }
    }
}
