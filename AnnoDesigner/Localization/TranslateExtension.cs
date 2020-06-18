using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace AnnoDesigner.Localization
{
    public class TranslateKeyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var translations = value as IDictionary<string, string>;
            var key = parameter as string;

            if (translations.ContainsKey(key))
            {
                return translations[key];
            }
            return $"!{key}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class Translate : Binding
    {
        private static TranslateKeyConverter TranslateConverter { get; } = new TranslateKeyConverter();

        public string Key
        {
            set
            {
                ConverterParameter = value;
            }
        }

        public Translate() : base("InstanceTranslations")
        {
            Source = Localization.Instance;
            Converter = TranslateConverter;
        }

        public Translate(string key) : this()
        {
            Key = key;
        }
    }
}
