using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnoDesigner.Models
{
    public class AvailableRelease
    {
        public long Id { get; set; }

        public ReleaseType Type { get; set; }

        public Version Version { get; set; }
    }
}
