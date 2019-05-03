using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Media.Imaging;

namespace AnnoDesigner.UI
{
    [DebuggerDisplay("{Name}")]
    public class IconImage
    {
        private readonly Dictionary<string, string> _localizations;

        #region ctor

        public IconImage(string name)
        {
            Name = name;
            _localizations = null;
        }

        public IconImage(string name, Dictionary<string, string> localizations, BitmapImage icon) : this(name)
        {
            _localizations = localizations;
            Icon = icon;
        }

        #endregion        

        public string Name { get; }

        public string DisplayName
        {
            get { return _localizations == null ? Name : _localizations["eng"]; }
        }

        public BitmapImage Icon { get; }
    }
}