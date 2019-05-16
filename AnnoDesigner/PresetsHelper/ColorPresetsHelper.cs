using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using AnnoDesigner.Presets;
using AnnoDesigner.PresetsLoader;

namespace AnnoDesigner.PresetsHelper
{
    public class ColorPresetsHelper
    {
        private readonly ColorPresetsLoader _colorPresetsLoader;
        private ColorPresets _loadedColorPresets;
        private ColorScheme _loadedDefaultColorScheme;

        #region ctor

        private static readonly Lazy<ColorPresetsHelper> lazy = new Lazy<ColorPresetsHelper>(() => new ColorPresetsHelper());

        public static ColorPresetsHelper Instance
        {
            get { return lazy.Value; }
        }

        private ColorPresetsHelper()
        {
            _colorPresetsLoader = new ColorPresetsLoader();
        }

        #endregion

        private ColorPresets LoadedColorPresets
        {
            get
            {
                if (_loadedColorPresets == null)
                {
                    _loadedColorPresets = _colorPresetsLoader.Load();
                }

                return _loadedColorPresets;
            }
        }

        private ColorScheme LoadedDefaultColorScheme
        {
            get
            {
                if (_loadedDefaultColorScheme == null)
                {
                    _loadedDefaultColorScheme = _colorPresetsLoader.LoadDefaultScheme();
                }

                return _loadedDefaultColorScheme;
            }
        }


        public Color? GetPredefinedColor(AnnoObject annoObject)
        {
            Color? result = null;

            if (string.IsNullOrWhiteSpace(annoObject.Template))
            {
                return result;
            }

            var colorsForTemplate = LoadedDefaultColorScheme.Colors.Where(x => x.TargetTemplate.Equals(annoObject.Template, StringComparison.OrdinalIgnoreCase)).ToList();
            if (!colorsForTemplate.Any())
            {
                return result;
            }

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
    }
}
