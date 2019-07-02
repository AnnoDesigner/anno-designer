using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnnoDesigner.Core.Models;

namespace AnnoDesigner.model
{
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
