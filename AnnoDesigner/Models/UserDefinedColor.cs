using AnnoDesigner.Core.Models;
using System.Diagnostics;

namespace AnnoDesigner.Models;

[DebuggerDisplay("{" + nameof(Type) + ",nq}")]
public class UserDefinedColor : Notify
{
    private readonly ILocalizationHelper _localizationHelper;

    private UserDefinedColorType _type;
    private SerializableColor _color;

    /// <summary>
    /// This constructor is only used for Serialization.
    /// </summary>
    public UserDefinedColor()
    { }

    public UserDefinedColor(ILocalizationHelper localizationHelperToUse)
    {
        _localizationHelper = localizationHelperToUse;
    }

    public UserDefinedColorType Type
    {
        get => _type;
        set => UpdateProperty(ref _type, value);
    }

    public string DisplayName()
    {
        return _localizationHelper is null ? Type.ToString() : _localizationHelper.GetLocalization("ColorType" + Type.ToString());
    }

    public SerializableColor Color
    {
        get => _color;
        set => UpdateProperty(ref _color, value);
    }
}
