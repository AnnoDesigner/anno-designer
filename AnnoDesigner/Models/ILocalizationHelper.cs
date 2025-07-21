namespace AnnoDesigner.Models;

/// <summary>
/// This interface is used to help to tranlate texts of the application.
/// </summary>
public interface ILocalizationHelper
{
    /// <summary>
    /// Returns a translation of a given key.
    /// </summary>
    /// <param name="translationKey">The key to search the translation for.</param>
    /// <returns>The translated value, if found. Otherwise the translation in english or the <paramref name="translationKey"/>.</returns>
    string GetLocalization(string translationKey);

    /// <summary>
    /// Returns a translation of a given key for a given languagecode.
    /// </summary>
    /// <param name="translationKey">The key to search the translation for.</param>
    /// <param name="languageCode">The languagecode (ISO 639-2; three-letter code) to use.</param>
    /// <returns>The translated value, if found. Otherwise the translation in english or the <paramref name="translationKey"/>.</returns>
    string GetLocalization(string translationKey, string languageCode);
}
