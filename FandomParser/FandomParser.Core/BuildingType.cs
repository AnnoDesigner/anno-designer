using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FandomParser.Core
{
    public enum BuildingType
    {
        Unknown,
        Production,
        [Description("Public Service")]
        PublicService,
        Residence,
        Infrastructure,
        Institution,
        Ornament,
        Monument,
        Administration,
        Harbour,
        Street
    }
}
