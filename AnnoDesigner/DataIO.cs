using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MessageBox = Microsoft.Windows.Controls.MessageBox;

namespace AnnoDesigner
{
    /// <summary>
    /// Provides I/O methods
    /// </summary>
    public static class DataIO
    {
        #region Serialization and Deserialization

        /// <summary>
        /// Serializes the given object to JSON and writes it to the given file.
        /// </summary>
        /// <typeparam name="T">type of the object being serialized</typeparam>
        /// <param name="obj">object to serialize</param>
        /// <param name="filename">output JSON filename</param>
        public static void SaveToFile<T>(T obj, string filename)
        {
            using (var stream = File.Open(filename, FileMode.Create))
            {
                var serializer = new DataContractJsonSerializer(typeof(T));
                serializer.WriteObject(stream, obj);
            }
        }

        /// <summary>
        /// Deserializes the given JSON file to an object of type T.
        /// </summary>
        /// <typeparam name="T">type of object being deserialized</typeparam>
        /// <param name="filename">input JSON filename</param>
        /// <returns>deserialized object</returns>
        public static T LoadFromFile<T>(string filename)
        {
            T obj;
            LoadFromFile(out obj, filename);
            return obj;
        }

        /// <summary>
        /// Deserializes the given JSON file to an object.
        /// </summary>
        /// <typeparam name="T">type of the object being deserialized</typeparam>
        /// <param name="obj">output object</param>
        /// <param name="filename">input JSON filename</param>
        public static void LoadFromFile<T>(out T obj, string filename)
        {
            using (var stream = File.Open(filename, FileMode.Open))
            {
                var serializer = new DataContractJsonSerializer(typeof(T));
                obj = (T)serializer.ReadObject(stream);
            }
        }

        #endregion

        #region Render to file

        /// <summary>
        /// Renders the given target to an image file, png encoded.
        /// </summary>
        /// <param name="target">target to be rendered</param>
        /// <param name="filename">output filename</param>
        public static void RenderToFile(FrameworkElement target, string filename)
        {
            // render control
            const int dpi = 96;
            var rtb = new RenderTargetBitmap((int)target.ActualWidth, (int)target.ActualHeight, dpi, dpi, PixelFormats.Default);
            rtb.Render(target);
            // put result into bitmap
            var encoder = Constants.GetExportImageEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));
            // save file
            using (var file = new FileStream(filename, FileMode.Create))
            {
                encoder.Save(file);
            }
        }

        #endregion

        #region Layout saving and loading

        /// <summary>
        /// Container with just the file version number
        /// </summary>
        [DataContract]
        private class LayoutVersionContainer
        {
            [DataMember]
            public int FileVersion;
        }

        /// <summary>
        /// Container with file version and all objects
        /// </summary>
        [DataContract]
        private class SavedLayout
            : LayoutVersionContainer
        {
            [DataMember]
            public List<AnnoObject> Objects;

            public SavedLayout(List<AnnoObject> objects)
            {
                FileVersion = Constants.FileVersion;
                Objects = objects;
            }
        }

        /// <summary>
        /// Saves the given objects to the given file and includes the current file version number.
        /// </summary>
        /// <param name="objects">objects to save</param>
        /// <param name="filename">filename to save to</param>
        public static void SaveLayout(List<AnnoObject> objects, string filename)
        {
            SaveToFile(new SavedLayout(objects), filename);
        }

        /// <summary>
        /// Loads all objects from the given file, taking care of file version mismatches.
        /// </summary>
        /// <param name="filename">file to load</param>
        /// <returns>list of loaded objects</returns>
        public static List<AnnoObject> LoadLayout(string filename)
        {
            // try to load file version
            var layoutVersion = LoadFromFile<LayoutVersionContainer>(filename);
            // show message if file versions don't match or if loading of the file version failed
            if (layoutVersion.FileVersion != Constants.FileVersion)
            {
                if (MessageBox.Show(
                        "Try loading anyway?\nThis is very likely to fail or result in strange things happening.",
                        "File version mismatch", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                {
                    return null;
                }
            }
            // try to load layout
            var layout = LoadFromFile<SavedLayout>(filename);
            // use fallback for old layouts
            return layout.Objects ?? LoadFromFile<List<AnnoObject>>(filename);
        }

        #endregion
    }
}