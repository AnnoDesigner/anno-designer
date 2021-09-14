using System;
using System.Collections.Generic;
using System.Linq;
using AnnoDesigner.Core.Presets.Models;

namespace PresetParser
{
    internal class Validator
    {
        public (bool isValid, List<string> duplicateIdentifiers) CheckForUniqueIdentifiers(List<IBuildingInfo> buildingsToCheck, List<string> knownDuplicates)
        {
            var result = (true, new List<string>());

            knownDuplicates ??= new List<string>();

            var duplicates = buildingsToCheck.GroupBy(x => x.Identifier).Where(x => x.Count() > 1).Select(x => x.Key).ToList();
            //remove known duplicates from result
            duplicates = duplicates.Except(knownDuplicates, StringComparer.Ordinal).ToList();

            if (duplicates.Count > 0)
            {
                result = (false, duplicates);
            }

            return result;
        }
    }
}
