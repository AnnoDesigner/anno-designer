using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnoDesigner.Core.Extensions
{
    public static class StringExtensions
    {
        public static bool Contains(this string s, string token, StringComparison stringComparison)
        {
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
