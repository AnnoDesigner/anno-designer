using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PresetParser
{
    public static class StingExtensions
    {
        public static bool Contains(this string s, IEnumerable<string> tokens)
        {
            foreach (string token in tokens)
            {
                if (s.Contains(token))
                {
                    return true;
                }
            }
            return false;
        }
        public static bool IsPartOff(this string s, IEnumerable<string> tokens)
        {
            foreach (string token in tokens)
            {
                if (token.Contains(s))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
