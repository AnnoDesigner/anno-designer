using AnnoDesigner.Core.Models;
using System;

namespace AnnoDesigner.ViewModels;

public class LayoutSettingsViewModel : Notify
{
    public LayoutSettingsViewModel()
    {
        _layoutVersion = new Version(1, 0, 0, 0);
    }

    private Version _layoutVersion;

    public Version LayoutVersion
    {
        get => _layoutVersion;
        set
        {
            if (value is null)
            {
                return;
            }

            _ = UpdateProperty(ref _layoutVersion, value);
            OnPropertyChanged(nameof(LayoutVersionDisplayValue));
        }
    }

    public string LayoutVersionDisplayValue
    {
        get => _layoutVersion.ToString();
        set
        {
            if (Version.TryParse(value, out Version parsedVersion))
            {
                LayoutVersion = parsedVersion;
            }
        }
    }
}
