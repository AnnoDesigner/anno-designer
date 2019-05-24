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
            using (var stream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var serializer = new DataContractJsonSerializer(typeof(T), new List<Type> { typeof(AnnoObject) });
                obj = (T)serializer.ReadObject(stream);
            }
        }
    }
}
