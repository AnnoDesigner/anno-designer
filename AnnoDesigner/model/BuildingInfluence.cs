using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnnoDesigner.Core.Models;

namespace AnnoDesigner.model
{
    [DebuggerDisplay("{" + nameof(Type) + ",nq} - {" + nameof(Name) + "}")]
    public class BuildingInfluence : Notify
    {
        private BuildingInfluenceType _type;
        private string _name;

        public BuildingInfluenceType Type
        {
            get { return _type; }
            set { UpdateProperty(ref _type, value); }
        }

        public string Name
        {
            get { return _name; }
            set { UpdateProperty(ref _name, value); }
        }
    }
}
