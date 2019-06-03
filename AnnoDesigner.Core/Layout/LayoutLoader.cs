using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnnoDesigner.Core.Helper;
using AnnoDesigner.Core.Layout.Exceptions;
using AnnoDesigner.Core.Layout.Models;
using AnnoDesigner.Core.Models;

namespace AnnoDesigner.Core.Layout
{
    public class LayoutLoader : ILayoutLoader
    {
        /// <summary>
        /// Saves the given objects to the given file and includes the current file version number.
        /// </summary>
        /// <param name="objects">objects to save</param>
        /// <param name="pathToLayoutFile">filepath to save to</param>
        public void SaveLayout(List<AnnoObject> objects, string pathToLayoutFile)
        {
            SerializationHelper.SaveToFile(new SavedLayout(objects), pathToLayoutFile);
        }

        /// <summary>
        /// Saves the given objects to the given stream and includes the current file version number.
        /// </summary>
        /// <param name="objects">objects to save</param>
        /// <param name="streamWithLayout">stream to save to</param>
        public void SaveLayout(List<AnnoObject> objects, Stream streamWithLayout)
        {
            SerializationHelper.SaveToStream(new SavedLayout(objects), streamWithLayout);
        }

        /// <summary>
        /// Loads all objects from the given file, taking care of file version mismatches.
        /// </summary>
        /// <param name="pathToLayoutFile">path to file to load</param>
        /// <param name="forceLoad">ignore version of layout file and load anyway</param>
        /// <exception cref="LayoutFileVersionMismatchException">indicates a version mismatch of the layout file</exception>
        /// <returns>list of loaded objects</returns>
        public List<AnnoObject> LoadLayout(string pathToLayoutFile, bool forceLoad = false)
        {
            if (string.IsNullOrWhiteSpace(pathToLayoutFile))
            {
                throw new ArgumentNullException(nameof(pathToLayoutFile));
            }

            using (var streamWithLayout = File.Open(pathToLayoutFile, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return LoadLayout(streamWithLayout, forceLoad);
            }
        }

        public List<AnnoObject> LoadLayout(Stream streamWithLayout, bool forceLoad = false)
        {
            if (streamWithLayout == null)
            {
                throw new ArgumentNullException(nameof(streamWithLayout));
            }

            // try to load file version
            var layoutVersion = SerializationHelper.LoadFromStream<LayoutVersionContainer>(streamWithLayout);
            // show message if file versions don't match or if loading of the file version failed
            if (layoutVersion.FileVersion != CoreConstants.LayoutFileVersion && !forceLoad)
            {
                throw new LayoutFileVersionMismatchException($"loaded version: {layoutVersion.FileVersion} | expected version: {CoreConstants.LayoutFileVersion}");
            }

            if (streamWithLayout.CanSeek)
            {
                streamWithLayout.Position = 0;
            }

            // try to load layout
            var layout = SerializationHelper.LoadFromStream<SavedLayout>(streamWithLayout);

            if (streamWithLayout.CanSeek)
            {
                streamWithLayout.Position = 0;
            }

            // use fallback for old layouts
            return layout.Objects ?? SerializationHelper.LoadFromStream<List<AnnoObject>>(streamWithLayout);
        }
    }
}
