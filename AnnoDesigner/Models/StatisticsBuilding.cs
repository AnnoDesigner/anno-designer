using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnnoDesigner.Core.Models;

namespace AnnoDesigner.Models
{
    [DebuggerDisplay("{" + nameof(Count) + ",nq} x {" + nameof(Name) + "}")]
    public class StatisticsBuilding : Notify
    {
        private int _count;
        private string _name;

        public int Count
        {
            get { return _count; }
            set { UpdateProperty(ref _count, value); }
        }

        public string Name
        {
            get { return _name; }
            set { UpdateProperty(ref _name, value); }
        }
    }
}
