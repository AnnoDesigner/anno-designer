using System.Diagnostics;
using AnnoDesigner.Core.Models;

namespace AnnoDesigner.Models
{
    [DebuggerDisplay("{" + nameof(Type) + ",nq}")]
    public class UserDefinedColor : Notify
    {
        private UserDefinedColorType _type;
        private SerializableColor _color;

        public UserDefinedColorType Type
        {
            get { return _type; }
            set { UpdateProperty(ref _type, value); }
        }

        public string DisplayName()
        {
            return Localization.Localization.Translations["ColorType" + Type.ToString()];
        }

        public SerializableColor Color
        {
            get { return _color; }
            set { UpdateProperty(ref _color, value); }
        }
    }
}
