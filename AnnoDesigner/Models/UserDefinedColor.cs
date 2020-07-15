using System.Diagnostics;
using System.Runtime.Serialization;
using AnnoDesigner.Core.Models;

namespace AnnoDesigner.Models
{
    [DebuggerDisplay("{" + nameof(Type) + ",nq}")]
    [DataContract]
    public class UserDefinedColor : Notify
    {
        private UserDefinedColorType _type;
        private SerializableColor _color;

        [DataMember(Order = 0)]
        public UserDefinedColorType Type
        {
            get { return _type; }
            set { UpdateProperty(ref _type, value); }
        }

        public string DisplayName()
        {
            return Localization.Localization.Translations["ColorType" + Type.ToString()];
        }

        [DataMember(Order = 1)]
        public SerializableColor Color
        {
            get { return _color; }
            set { UpdateProperty(ref _color, value); }
        }
    }
}
