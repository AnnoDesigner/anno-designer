using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace AnnoDesigner.UI
{
    public class IconImage
    {
        private readonly Dictionary<string, string> _localizations;

        public BitmapImage Icon
        {
            get;
            private set;
        }

        public string Name
        {
            get;
            private set;
        }

        public string DisplayName
        {
            get
            {
                return _localizations == null ? Name : _localizations["eng"];
            }
        }

        public IconImage(string name)
        {
            _localizations = null;
            Name = name;
        }

        public IconImage(string name, Dictionary<string,string> localizations, BitmapImage icon)
        {
            _localizations = localizations;
            Name = name;
            Icon = icon;
        }
    }
}