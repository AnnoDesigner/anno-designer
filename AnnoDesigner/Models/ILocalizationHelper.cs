using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnoDesigner.Models
{
    public interface ILocalizationHelper
    {
        string GetLocalization(string valueToTranslate);

        string GetLocalization(string valueToTranslate, string languageCode);
    }
}
