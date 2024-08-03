using System.Collections.Generic;

namespace AnnoDesigner.Core.Models;

public interface IClipboard
{
    /// <summary>
    /// Clears any data from the Clipboard.
    /// </summary>
    void Clear();

    /// <summary>
    /// Queries the Clipboard for the presence of data in a specified data format.
    /// </summary>
    /// <param name="format">The format of the data to look for.</param>
    /// <returns><c>true</c> if the Clipboard contains data in the specified <paramref name="format"/>, otherwise <c>false</c>.</returns>
    /// <exception cref="System.ArgumentNullException"><paramref name="format"/> is <c>null</c></exception>
    bool ContainsData(string format);

    /// <summary>
    /// Returns a list of strings that contains a list of dropped files available on the Clipboard.
    /// </summary>
    /// <returns>A list of strings, where each string specifies the name of a file in the list of dropped files on the Clipboard, or <c>null</c> if the data is unavailable in this format.</returns>
    IReadOnlyList<string> GetFileDropList();

    /// <summary>
    /// Stores the specified data on the Clipboard in the specified format.
    /// </summary>
    /// <param name="format">A string that specifies the format to use to store the data.</param>
    /// <param name="data">An object representing the data to store on the Clipboard.</param>
    void SetData(string format, object data);

    /// <summary>
    /// Retrieves data in a specified format from the Clipboard.
    /// </summary>
    /// <param name="format">A string that specifies the format of the data to retrieve.</param>
    /// <returns>An object that contains the data in the specified <paramref name="format"/>, or <c>null</c> if the data is unavailable in the specified <paramref name="format"/>.</returns>
    /// <exception cref="System.ArgumentNullException"><paramref name="format"/> is <c>null</c></exception>
    object GetData(string format);

    /// <summary>
    /// Permanently adds the data that is on the Clipboard so that it is available after the data's original application closes.
    /// </summary>
    void Flush();

    /// <summary>
    /// Queries the Clipboard for the presence of data in text format.
    /// </summary>
    /// <returns><c>true</c> if the Clipboard contains data in the text format, otherwise <c>false</c>.</returns>
    bool ContainsText();

    /// <summary>
    /// Returns a string containing the text data on the Clipboard.
    /// </summary>
    /// <returns>A string containing the text data, or an empty string if no text data is available on the Clipboard.</returns>
    string GetText();
}
