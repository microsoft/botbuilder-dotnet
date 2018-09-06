using System.Text.RegularExpressions;

namespace Chronic
{
    public static class StringExtensions
    {
        public static string ReplaceAll(this string @this, string pattern, string replacement)
        {
            return Regex.Replace(@this, pattern, replacement);            
        }

        public static Regex Compile(this string @this)
        {
            return new Regex(@this, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        public static string LastCharacters(this string @this, int numberOfCharsToTake)
        {
            if (@this == null)
            {
                return null;
            }

            if (@this.Length <= numberOfCharsToTake)
            {
                return @this;
            }

            return @this.Substring(@this.Length - numberOfCharsToTake);
        }

        public static string Numerize(this string @this)
        {
            return Numerizer.Numerize(@this);
        }
    }
}
