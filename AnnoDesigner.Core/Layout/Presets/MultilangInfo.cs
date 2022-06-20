using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace AnnoDesigner.Core.Layout.Presets
{
    [JsonConverter(typeof(MultilangInfoConverter))]
    public class MultilangInfo
    {
        private class MultilangInfoConverter : JsonConverter<MultilangInfo>
        {
            public override MultilangInfo ReadJson(JsonReader reader, Type objectType, MultilangInfo existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                switch (reader.TokenType)
                {
                    case JsonToken.String:
                        return reader.Value as string;
                    case JsonToken.StartObject:
                        return serializer.Deserialize<Dictionary<string, string>>(reader);
                    default:
                        throw new JsonSerializationException($"Unexpected token during deserialization of {nameof(MultilangInfo)}");
                }
            }

            public override void WriteJson(JsonWriter writer, MultilangInfo value, JsonSerializer serializer)
            {
                serializer.Serialize(writer, (object)value.Default ?? value.Translations);
            }
        }

        private Dictionary<string, string> Translations { get; set; }

        private string Default { get; set; }

        public string this[string language]
        {
            set
            {
                Translations ??= new Dictionary<string, string>();
                Translations[language] = value;
            }
        }

        public string Translate(string language)
        {
            return Default ?? (
                Translations.TryGetValue(language, out var translation)
                    ? translation
                    : Translations.Count > 0
                        ? $"{Translations.FirstOrDefault().Value} ({Translations.FirstOrDefault().Key})"
                        : string.Empty
                );
        }

        public static implicit operator MultilangInfo(string value)
        {
            return new MultilangInfo()
            {
                Default = value
            };
        }

        public static implicit operator MultilangInfo(Dictionary<string, string> value)
        {
            return new MultilangInfo()
            {
                Translations = value
            };
        }

        public static explicit operator string(MultilangInfo info)
        {
            var first = info.Translations.FirstOrDefault();
            return info.Default ?? (info.Translations.Count > 0 ? $"{first.Value} ({first.Key})" : string.Empty);
        }
    }
}
