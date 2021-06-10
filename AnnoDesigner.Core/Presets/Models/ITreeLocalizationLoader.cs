using AnnoDesigner.Core.Presets.Models;

namespace AnnoDesigner.Core.Presets.Loader
{
    public interface ITreeLocalizationLoader
    {
        /// <summary>
        /// Load localization for the presets tree from a file path.
        /// </summary>
        /// <param name="pathToTreeLocalizationFile">path containing the json encoded localization</param>
        /// <returns>A <see cref="TreeLocalizationContainer"/> containing all information about the localization for the presets tree.</returns>
        TreeLocalizationContainer LoadFromFile(string pathToTreeLocalizationFile);

        /// <summary>
        /// Load localization for the presets tree from json string.
        /// </summary>
        /// <param name="jsonString">string with json encoded localization</param>
        /// <returns>A <see cref="TreeLocalizationContainer"/> containing all information about the localization for the presets tree.</returns>
        TreeLocalizationContainer Load(string jsonString);
    }
}