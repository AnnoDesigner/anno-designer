using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FandomParser.Core.Helper
{
    public static class SerializationHelper
    {
        public static void SaveToFile<T>(T obj, string filename, bool prettyPrint = false)
        {
            Console.WriteLine($"saving file: {Path.GetFullPath(filename)}");

            var formatting = prettyPrint ? Formatting.Indented : Formatting.None;

            var json = JsonConvert.SerializeObject(obj, formatting, new VersionConverter(), new StringEnumConverter());
            File.WriteAllText(filename, json, Encoding.UTF8);
        }

        public static T LoadFromFile<T>(string filename)
        {
            Console.WriteLine($"loading file: {Path.GetFullPath(filename)}");

            var json = File.ReadAllText(filename, Encoding.UTF8);
            return JsonConvert.DeserializeObject<T>(json, new VersionConverter(), new StringEnumConverter());
        }
    }
}
