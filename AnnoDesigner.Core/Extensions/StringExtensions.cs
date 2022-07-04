using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnnoDesigner.Core.Presets.Models;

namespace AnnoDesigner.Core.Extensions
{
    public static class StringExtensions
    {
        //private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static bool Contains(this string s, string token, StringComparison stringComparison)
        {

            if (string.IsNullOrWhiteSpace(s) || string.IsNullOrWhiteSpace(token))
            {
                //logger.Trace($"{nameof(s)}: {s} | {nameof(token)}: {token}");

                return false;//or throw error?
            }

            if (s.IndexOf(token, stringComparison) != -1)
            {
                return true;
            }

            return false;
        }

        public static bool Contains(this string s, IEnumerable<string> tokens)
        {
            foreach (var token in tokens)
            {
                if (s.Contains(token, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsPartOf(this string s, IEnumerable<string> tokens)
        {
            foreach (var token in tokens)
            {
                if (token.Contains(s, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        // This checker is made so it is a 100% sure same Identifiers in name and length of the string.
        // i discovered that .Contains .startWith and .IsPartOff was not good enough to check this.
        // (11-06-2022)
        public static bool IsMatch(this string IdentifierToCheck, List<IBuildingInfo> buildingsToCheck)
        {
            foreach (var building in buildingsToCheck)
            {
                var match = building.Identifier.StartsWith(IdentifierToCheck);
                var isSameLenght = building.Identifier.Length;
                if (match && (IdentifierToCheck.Length == isSameLenght))
                {
                    return true;
                } 
            }

            return false;
        }
        public static bool IsMatchString(this string IdentifierToCheck, List<string> StringListtoCheck)
        {
            foreach (var StringValeu in StringListtoCheck)
            {
                var match = StringValeu.StartsWith(IdentifierToCheck);
                var isSameLenght = StringValeu.Length;
                if (match && (IdentifierToCheck.Length == isSameLenght))
                {
                    return true;
                }
            }

            return false;
        }

        public static string FirstCharToUpper(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            var tempCharArray = input.ToCharArray();
            tempCharArray[0] = char.ToUpper(tempCharArray[0]);

            return new string(tempCharArray);
        }
    }
}
