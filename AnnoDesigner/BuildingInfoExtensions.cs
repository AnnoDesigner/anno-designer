using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Core.Presets.Models;

namespace AnnoDesigner
{
    public static class BuildingInfoExtensions
    {
        public static AnnoObject ToAnnoObject(this IBuildingInfo buildingInfo, string selectedLanguage)
        {
            var labelLocalization = buildingInfo.Localization == null ? buildingInfo.Identifier : buildingInfo.Localization[AnnoDesigner.Localization.Localization.GetLanguageCodeFromName(selectedLanguage)];
            if (string.IsNullOrEmpty(labelLocalization))
            {
                labelLocalization = buildingInfo.Localization["eng"];
            }

            return new AnnoObject
            {
                //Label = (buildingInfo.Localization == null ? buildingInfo.Identifier : buildingInfo.Localization[AnnoDesigner.Localization.Localization.GetLanguageCodeFromName(selectedLanguage)]),
                Label = labelLocalization,
                Icon = buildingInfo.IconFileName,
                Radius = buildingInfo.InfluenceRadius,
                InfluenceRange = buildingInfo.InfluenceRange - 2,
                Identifier = buildingInfo.Identifier,
                Size = buildingInfo.BuildBlocker == null ? new Size() : new Size(buildingInfo.BuildBlocker["x"], buildingInfo.BuildBlocker["z"]),
                Template = buildingInfo.Template
                //BuildCosts = BuildCost
            };
        }

        public static string GetOrderParameter(this IBuildingInfo buildingInfo, string selectedLanguage)
        {
            var labelLocalization = buildingInfo.Localization == null ? buildingInfo.Identifier : buildingInfo.Localization[AnnoDesigner.Localization.Localization.GetLanguageCodeFromName(selectedLanguage)];
            if (string.IsNullOrEmpty(labelLocalization))
            {
                labelLocalization = buildingInfo.Localization["eng"];
            }

            //return buildingInfo.Localization == null ? buildingInfo.Identifier : buildingInfo.Localization[AnnoDesigner.Localization.Localization.GetLanguageCodeFromName(selectedLanguage)];
            return labelLocalization;
        }
    }
}
