using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace PresetParser;

public class IconFileNameHelper
{
    public string GetIconFilename(XmlNode iconNode, string annoVersion)
    {
        string annoIndexNumber = string.Empty;

        if (annoVersion == Constants.ANNO_VERSION_1404)
        {
            annoIndexNumber = "A4_";
        }

        /* For Anno 2070, we use the normal icon names, without the AnnoIndexNUmber ('A5_'),
            because anno 2070 has already the right names in previous Anno Designer versions. */

        //TODO: check this icon format is consistent between Anno versions
        string iconFileId = iconNode["IconFileID"].InnerText;
        string iconIndex = iconNode["IconIndex"] != null ? iconNode["IconIndex"].InnerText : "0";

        return string.Format("{0}icon_{1}_{2}.png", annoIndexNumber, iconFileId, iconIndex);
    }
}
