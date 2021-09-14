using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnnoDesigner.Core.Presets.Models;

namespace PresetParser
{
    internal class Validator
    {
        public (bool isValid, List<string> duplicateIdentifiers) CheckForUniqueIdentifiers(List<IBuildingInfo> buildingsToCheck)
        {
            var result = (true, new List<string>());

            var duplicates = buildingsToCheck.GroupBy(x => x.Identifier).Where(x => x.Count() > 1).Select(x => x.Key).ToList();
            if (duplicates.Count > 0)
            {
                result = (false, duplicates);
            }

            return result;
        }
    }
}
