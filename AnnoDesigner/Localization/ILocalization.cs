using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnoDesigner.Localization
{
    public interface ILocalization
    {
        IDictionary<string, string> InstanceTranslations { get; }
    }
}
