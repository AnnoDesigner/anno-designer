using System;
using AnnoDesigner.Core.Models;

namespace AnnoDesigner.ViewModels
{
    public class LayoutSettingsViewModel : Notify
    {
        public LayoutSettingsViewModel()
        {
            _layoutVersion = new Version(1, 0, 0, 0);
        }

        private Version _layoutVersion;

        public Version LayoutVersion
        {
            get { return _layoutVersion; }
            set
            {
                UpdateProperty(ref _layoutVersion, value);
                OnPropertyChanged(nameof(LayoutVersionDisplayValue));
            }
        }

        public string LayoutVersionDisplayValue
        {
            get { return _layoutVersion.ToString(); }
            set
            {
                if (Version.TryParse(value, out var parsedVersion))
                {
                    LayoutVersion = parsedVersion;
                }
            }
        }
    }
}
