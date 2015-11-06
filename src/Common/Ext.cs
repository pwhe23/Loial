using System.Text.RegularExpressions;

namespace Common
{
    public static class Ext
    {
        //REF: http://www.henrikbrinch.dk/Blog/2013/03/07/Wildcard-Matching-In-C-Using-Regular-Expressions
        public static bool LikeString(string pattern, string text, bool caseSensitive = false)
        {
            if (!pattern.StartsWith("^")) pattern = "^" + pattern;
            pattern = pattern.Replace(".", @"\.");
            pattern = pattern.Replace("?", ".");
            pattern = pattern.Replace("*", ".*?");
            pattern = pattern.Replace(@"\", @"\\");
            pattern = pattern.Replace(" ", @"\s");
            if (!pattern.EndsWith("$")) pattern = pattern + "$";

            return new Regex(pattern, caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase)
                .IsMatch(text);
        }
    };
}
