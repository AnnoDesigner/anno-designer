using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace AnnoDesigner.Localization
{
    public class LocalizeConverter : IValueConverter, IMultiValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var translations = value as IDictionary<string, string>;

            if (parameter is string key && translations.TryGetValue(key, out var translation))
            {
                return translation;
            }
#if DEBUG
            throw new Exception($"Missing translation or not string translation key \"{parameter}\"");
#else
            return $"!{parameter}";
#endif
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(values[0], targetType, values[1], culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class Localize : Binding
    {
        private static LocalizeConverter LocalizeConverter { get; } = new LocalizeConverter();

        public string Key
        {
            set
            {
                ConverterParameter = value;
            }
        }

        public Localize() : base(nameof(Localization.Instance.InstanceTranslations))
        {
            Source = Localization.Instance;
            Converter = LocalizeConverter;
        }

        public Localize(string key) : this()
        {
            Key = key;
        }
    }

    public class DynamicLocalize : MultiBinding
    {
        private static LocalizeConverter LocalizeConverter { get; } = new LocalizeConverter();

        public string KeyPath
        {
            set
            {
                Bindings.Add(new Binding(value));
            }
        }

        public DynamicLocalize()
        {
            Bindings.Add(new Binding(nameof(Localization.Instance.InstanceTranslations))
            {
                Source = Localization.Instance
            });
            Converter = LocalizeConverter;
        }

        public DynamicLocalize(string keyPath) : this()
        {
            KeyPath = keyPath;
        }
    }
}
