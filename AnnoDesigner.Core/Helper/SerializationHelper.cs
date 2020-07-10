using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using AnnoDesigner.Core.Models;
using Newtonsoft.Json;

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
            File.WriteAllText(filename, SaveToJsonString(obj));
        }

        /// <summary>
        /// Serializes the given object to JSON and writes it to the given <see cref="Stream"/>.
        /// </summary>
        /// <typeparam name="T">type of the object being serialized</typeparam>
        /// <param name="obj">object to serialize</param>
        /// <param name="stream">output JSON stream</param>
        public static void SaveToStream<T>(T obj, Stream stream) where T : class
        {
            var serializer = new JsonSerializer();
            using var sw = new StreamWriter(stream, Encoding.UTF8, 1024, true);//use constructor that does not close base stream
            using var jsonWriter = new JsonTextWriter(sw);
            serializer.Serialize(jsonWriter, obj, typeof(T));
            jsonWriter.Flush();
        }


        /// <summary>
        /// Serializes the given object to JSON and returns it as a <see cref="string"/>.
        /// </summary>
        /// <typeparam name="T">type of the object being serialized</typeparam>
        /// <param name="obj">object to serialize</param>
        public static string SaveToJsonString<T>(T obj, Formatting formatting = Formatting.None) where T : class
        {
            return JsonConvert.SerializeObject(obj, formatting);
        }

        /// <summary>
        /// Deserializes the given JSON file to an object of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">type of object being deserialized</typeparam>
        /// <param name="filename">input JSON filename</param>
        /// <returns>deserialized object</returns>
        public static T LoadFromFile<T>(string filename) where T : class
        {
            var fileContents = File.ReadAllText(filename);
            return LoadFromJsonString<T>(fileContents);
        }

        /// <summary>
        /// Deserializes the given JSON stream to an object of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">type of object being deserialized</typeparam>
        /// <param name="stream">input JSON stream</param>
        /// <returns>deserialized object</returns>
        public static T LoadFromStream<T>(Stream stream) where T : class
        {
            var serializer = new JsonSerializer();
            using var sr = new StreamReader(stream);
            using var jsonReader = new JsonTextReader(sr);
            return serializer.Deserialize<T>(jsonReader);
        }



        /// <summary>
        /// Deserializes the given JSON string to an object of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">type of object being deserialized</typeparam>
        /// <param name="s">JSON string to deserialize</param>
        /// <exception cref="Newtonsoft.Json.JsonSerializationException">If <paramref name="jsonString"/> 
        /// is null or empty, or the json is not valid for the given object.</exception>
        /// <returns>deserialized object</returns>
        public static T LoadFromJsonString<T>(string jsonString) where T : class
        {
            if (string.IsNullOrWhiteSpace(jsonString))
            {
                return default;
            }
            return JsonConvert.DeserializeObject<T>(jsonString);
        }

        /// <summary>
        /// Legacy deserialization method for deserializing layout files <see cref="CoreConstants.LayoutFileVersion"/> 3 or older.
        /// </summary>
        /// <typeparam name="T">type of object being deserialized</typeparam>
        /// <param name="s">JSON string to deserialize</param>
        /// <returns>deserialized object</returns>
        public static T LoadFromJsonStringLegacy<T>(string jsonString) where T : class
        {
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));
            var serializer = new DataContractJsonSerializer(typeof(T));
            return (T)serializer.ReadObject(ms);
        }
    }
}
