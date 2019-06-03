using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using AnnoDesigner.Core.Models;

namespace AnnoDesigner.Core.Helper
{
    public static class SerializationHelper
    {
        /// <summary>
        /// Serializes the given object to JSON and writes it to the given file.
        /// </summary>
        /// <typeparam name="T">type of the object being serialized</typeparam>
        /// <param name="obj">object to serialize</param>
        /// <param name="filename">output JSON filename</param>
        public static void SaveToFile<T>(T obj, string filename) where T : class
        {
            using (var stream = File.Open(filename, FileMode.Create))
            {
                SaveToStream(obj, stream);
            }
        }

        /// <summary>
        /// Serializes the given object to JSON and writes it to the given stream.
        /// </summary>
        /// <typeparam name="T">type of the object being serialized</typeparam>
        /// <param name="obj">object to serialize</param>
        /// <param name="stream">output JSON stream</param>
        public static void SaveToStream<T>(T obj, Stream stream) where T : class
        {
            var serializer = new DataContractJsonSerializer(typeof(T));
            serializer.WriteObject(stream, obj);
        }

        /// <summary>
        /// Deserializes the given JSON file to an object of type T.
        /// </summary>
        /// <typeparam name="T">type of object being deserialized</typeparam>
        /// <param name="filename">input JSON filename</param>
        /// <returns>deserialized object</returns>
        public static T LoadFromFile<T>(string filename) where T : class
        {
            T result = null;

            using (var stream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                result = LoadFromStream<T>(stream);
            }

            return result;
        }

        /// <summary>
        /// Deserializes the given JSON file to an object of type T.
        /// </summary>
        /// <typeparam name="T">type of object being deserialized</typeparam>
        /// <param name="stream">input JSON stream</param>
        /// <returns>deserialized object</returns>
        public static T LoadFromStream<T>(Stream stream) where T : class
        {
            T result = null;

            var serializer = new DataContractJsonSerializer(typeof(T));//, new List<Type> { typeof(AnnoObject) });
            result = (T)serializer.ReadObject(stream);

            return result;
        }
    }
}
