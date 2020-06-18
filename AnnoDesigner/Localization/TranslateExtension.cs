using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

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

    public class TranslateKeyBinding : Binding
    {
        private static TranslateKeyConverter TranslateConverter { get; } = new TranslateKeyConverter();

        public string Key
        {
            set
            {
                ConverterParameter = value;
            }
        }

        public TranslateKeyBinding() : base("InstanceTranslations")
        {
            Source = Localization.Instance;
            Converter = TranslateConverter;
        }

        public TranslateKeyBinding(string key) : this()
        {
            Key = key;
        }
    }

    public class TranslateExtension : MarkupExtension
    {
        public string Key { get; set; }

        public string StringFormat { get; set; }

        public TranslateExtension()
        {

        }

        public TranslateExtension(string key)
        {
            Key = key;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var binding = new TranslateKeyBinding(Key)
            {
                StringFormat = StringFormat
            };
            return binding.ProvideValue(serviceProvider);
        }
    }
}
