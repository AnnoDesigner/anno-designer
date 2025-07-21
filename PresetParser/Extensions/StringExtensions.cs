using System.Collections.Generic;
using AnnoDesigner.Core.Presets.Models;

namespace PresetParser.Extensions;

public static class StringExtensions
{
    /// <summary>
    /// Checks if a given identifier matches a list of identifiers by lenght and name.
    /// </summary>
    /// <param name="identifierToCheck">The identifier to check.</param>
    /// <param name="buildingsToCheck">The list of buildings to check against.</param>
    /// <returns><c>true</c> if the identifier matches; otherwise <c>false</c>.</returns>
    public static bool IsMatch(this string identifierToCheck, List<IBuildingInfo> buildingsToCheck)
    {
        if (string.IsNullOrWhiteSpace(identifierToCheck) || buildingsToCheck is null)
        {
            return false;
        }

        foreach (IBuildingInfo curBuilding in buildingsToCheck)
        {
            bool match = curBuilding.Identifier.StartsWith(identifierToCheck);
            int isSameLenght = curBuilding.Identifier.Length;
            if (match && identifierToCheck.Length == isSameLenght)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks if a given identifier matches a list of identifiers by lenght and name.
    /// </summary>
    /// <param name="identifierToCheck">The identifier to check.</param>
    /// <param name="stringListToCheck">The list of identifiers to check against.</param>
    /// <returns><c>true</c> if the identifier matches; otherwise <c>false</c>.</returns>
    public static bool IsMatch(this string identifierToCheck, List<string> stringListToCheck)
    {
        if (string.IsNullOrWhiteSpace(identifierToCheck) || stringListToCheck is null)
        {
            return false;
        }

        foreach (string curValue in stringListToCheck)
        {
            bool match = curValue.StartsWith(identifierToCheck);
            int isSameLenght = curValue.Length;
            if (match && identifierToCheck.Length == isSameLenght)
            {
                return true;
            }
        }

        return false;
    }
}
