using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using AnnoDesigner.Core.Models;
using AnnoDesigner.Core.Presets.Loader;
using AnnoDesigner.Core.Presets.Models;

namespace AnnoDesigner.Core.Presets.Helper
{
    public class ColorPresetsHelper
    {
        private readonly ColorPresetsLoader _colorPresetsLoader;
        private readonly BuildingPresetsLoader _buildingPresetsLoader;
        private ColorPresets _loadedColorPresets;
        private ColorScheme _loadedDefaultColorScheme;
        private BuildingPresets _loadedBuildingPresets;

        #region ctor

        private static readonly Lazy<ColorPresetsHelper> lazy = new Lazy<ColorPresetsHelper>(() => new ColorPresetsHelper());

        public static ColorPresetsHelper Instance
        {
            get { return lazy.Value; }
        }

        private ColorPresetsHelper()
        {
            _colorPresetsLoader = new ColorPresetsLoader();
            _buildingPresetsLoader = new BuildingPresetsLoader();
        }

        #endregion

        private ColorPresets LoadedColorPresets
        {
            get { return _loadedColorPresets ?? (_loadedColorPresets = _colorPresetsLoader.Load(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), CoreConstants.ColorPresetsFile))); }
        }

        private ColorScheme LoadedDefaultColorScheme
        {
            get { return _loadedDefaultColorScheme ?? (_loadedDefaultColorScheme = _colorPresetsLoader.LoadDefaultScheme(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), CoreConstants.ColorPresetsFile))); }
        }

        private BuildingPresets LoadedBuildingPresets
        {
            get { return _loadedBuildingPresets ?? (_loadedBuildingPresets = _buildingPresetsLoader.Load(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), CoreConstants.BuildingPresetsFile))); }
        }

        public Color? GetPredefinedColor(AnnoObject annoObject)
        {
            Color? result = null;

            var templateName = annoObject.Template;

            //template name defined?
            if (string.IsNullOrWhiteSpace(annoObject.Template))
            {
                var foundTemplate = FindTemplateByIdentifier(annoObject.Identifier);
                if (string.IsNullOrWhiteSpace(foundTemplate))
                {
                    return result;
                }

                templateName = foundTemplate;
                //set template so it is saved when the layout is saved again
                annoObject.Template = templateName;
            }

            //colors for template defined?
            var colorsForTemplate = LoadedDefaultColorScheme.Colors.Where(x => x.TargetTemplate.Equals(templateName, StringComparison.OrdinalIgnoreCase)).ToList();
            if (!colorsForTemplate.Any())
            {
                return result;
            }

            //specific color for identifier defined?
            var colorForTemplateContainingIdentifier = colorsForTemplate.FirstOrDefault(x => x.TargetIdentifiers.Contains(annoObject.Identifier, StringComparer.OrdinalIgnoreCase));
            if (colorForTemplateContainingIdentifier != null)
            {
                result = colorForTemplateContainingIdentifier.Color;
            }
            else
            {
                result = colorsForTemplate.First().Color;
            }

            return result;
        }

        private string FindTemplateByIdentifier(string identifier)
        {
            string result = null;

            result = LoadedBuildingPresets.Buildings.FirstOrDefault(x => x.Identifier.Equals(identifier, StringComparison.OrdinalIgnoreCase))?.Template;

            return result;
        }
    }
}
