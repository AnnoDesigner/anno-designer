using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AnnoDesigner.Core.Helper;
using AnnoDesigner.Core.Layout.Exceptions;
using AnnoDesigner.Core.Layout.Models;
using AnnoDesigner.Core.Models;
using Newtonsoft.Json;

namespace AnnoDesigner.Core.Layout
{
    public class LayoutLoader : ILayoutLoader
    {
        /// <summary>
        /// Saves the given objects to the given file and includes the current file version number.
        /// </summary>
        /// <param name="objects">objects to save</param>
        /// <param name="pathToLayoutFile">filepath to save to</param>
        public void SaveLayout(SavedLayout layout, string pathToLayoutFile)
        {
            SerializationHelper.SaveToFile(layout, pathToLayoutFile);
        }

        /// <summary>
        /// Saves the given objects to the given stream and includes the current file version number.
        /// </summary>
        /// <param name="objects">objects to save</param>
        /// <param name="streamWithLayout">stream to save to</param>
        public void SaveLayout(SavedLayout layout, Stream streamWithLayout)
        {
            SerializationHelper.SaveToStream(layout, streamWithLayout);
        }

        /// <summary>
        /// Loads all objects from the given file, taking care of file version mismatches.
        /// </summary>
        /// <param name="pathToLayoutFile">path to file to load</param>
        /// <param name="forceLoad">ignore version of layout file and load anyway</param>
        /// <exception cref="LayoutFileUnsupportedFormatException">indicates the given layout file is in an unsupported format.</exception>
        /// <returns>list of loaded objects</returns>
        public SavedLayout LoadLayout(string pathToLayoutFile, bool forceLoad = false)
        {
            if (string.IsNullOrWhiteSpace(pathToLayoutFile))
            {
                throw new ArgumentNullException(nameof(pathToLayoutFile));
            }
            var jsonString = File.ReadAllText(pathToLayoutFile);
            return Load(jsonString, forceLoad);
        }

        public SavedLayout LoadLayout(Stream streamWithLayout, bool forceLoad = false)
        {
            if (streamWithLayout == null)
            {
                throw new ArgumentNullException(nameof(streamWithLayout));
            }
            using var sr = new StreamReader(streamWithLayout);
            var jsonString = sr.ReadToEnd();
            return Load(jsonString, forceLoad);
        }

        private SavedLayout Load(string jsonString, bool forceLoad)
        {
            var layoutVersion = new LayoutVersionContainer() { FileVersion = 0 };
            try
            {
                layoutVersion = SerializationHelper.LoadFromJsonString<LayoutVersionContainer>(jsonString);
            }
            catch (JsonSerializationException) { } //No file version, old layout file.

            //Only throw an exception if we do not support the layout
            var isLayoutVersionSupported = layoutVersion.FileVersion >= CoreConstants.LayoutFileVersionSupportedMinimum && layoutVersion.FileVersion <= CoreConstants.LayoutFileVersion;
            if (!isLayoutVersionSupported && !forceLoad)
            {
                throw new LayoutFileUnsupportedFormatException($"loaded version: {layoutVersion.FileVersion} | expected version: {CoreConstants.LayoutFileVersionSupportedMinimum} <= file version <= {CoreConstants.LayoutFileVersion}");
            }

            return layoutVersion.FileVersion switch
            {
                var version when version >= 4 => SerializationHelper.LoadFromJsonString<SavedLayout>(jsonString), //file version 4+, Newtonsoft.Json format json
                var version when version > 0 => SerializationHelper.LoadFromJsonStringLegacy<SavedLayout>(jsonString), //file version 1-3, DataContractJsonSerializer format json 
                var version when version == 0 => new SavedLayout(SerializationHelper.LoadFromJsonStringLegacy<List<AnnoObject>>(jsonString)), //no file version, DataContractJsonSerializer format json
                _ => throw new NotImplementedException()
            };
        }
    }
}
