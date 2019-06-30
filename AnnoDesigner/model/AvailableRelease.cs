using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnoDesigner.model
{
    public class AvailableRelease
    {
        public int Id { get; set; }

        public ReleaseType Type { get; set; }

        public Version Version { get; set; }
    }
}
