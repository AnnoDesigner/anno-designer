using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using AnnoDesigner.Core.Helper;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Core.Presets.Models;

namespace PresetParser.Anno1404_Anno2070;

public class IconNameMapper
{
    public void WriteIconNameMapping(IconFileNameHelper iconFileNameHelper, bool isTest, IEnumerable<XmlNode> iconNodes, Dictionary<string, SerializableDictionary<string>> localizations, string annoVersion, string BUILDING_PRESETS_VERSION)
    {
        IconMappingPresets iconNameMappings = new()
        {
            Version = BUILDING_PRESETS_VERSION
        };

        foreach (XmlNode iconNode in iconNodes)
        {
            string guid = iconNode["GUID"].InnerText;
            string iconFilename = iconFileNameHelper.GetIconFilename(iconNode["Icons"].FirstChild, annoVersion);
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
            string fileName = "icons-Anno" + annoVersion + "-v" + BUILDING_PRESETS_VERSION + ".json";
            SerializationHelper.SaveToFile(iconNameMappings, fileName);
            Console.WriteLine($"saved icon name mapping file: {fileName}");
        }
        else
        {
            Console.WriteLine("THIS IS A TEST: No icons.json file is writen");
        }

    }
}
