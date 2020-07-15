using System.Diagnostics;
using AnnoDesigner.Core.Models;

namespace AnnoDesigner.Models
{
    [DebuggerDisplay("{" + nameof(Type) + ",nq} - {" + nameof(Name) + "}")]
    public class UserDefinedColor : Notify
    {
        private UserDefinedColorType _type;
        private string _name;
        private SerializableColor _color;

        public UserDefinedColorType Type
        {
            get { return _type; }
            set { UpdateProperty(ref _type, value); }
        }

        public string Name
        {
            get { return _name; }
            set { UpdateProperty(ref _name, value); }
        }

        public SerializableColor Color
        {
            get { return _color; }
            set { UpdateProperty(ref _color, value); }
        }
    }
}
