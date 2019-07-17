using System;
using System.Linq;

namespace Stringification
{
    public static class StringExtensions
    {
        public static string RemoveWhitespace(this string input)
        {
            return new string(
                input
                    .Where(c => !Char.IsWhiteSpace(c))
                    .ToArray());
        }

        public static string TrimEndString(this string sourceStr, string strToTrim)
        {
            if (sourceStr.EndsWith(strToTrim))
            {
                sourceStr = sourceStr.Substring(0, sourceStr.Length - strToTrim.Length);
            }
            return sourceStr;
        }
    }
}