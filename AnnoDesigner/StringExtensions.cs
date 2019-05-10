using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnoDesigner
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
    }
}
