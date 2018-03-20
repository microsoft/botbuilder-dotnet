using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Classic.Resource
{
    #region Documentation
    /// <summary>   Extensions for resources. </summary>
    #endregion
    public static partial class Extensions
    {
        /// <summary>   The separator character between elements in a string list. </summary>
        public const string SEPARATOR = ";";

        /// <summary>   When the <see cref="SEPARATOR"/> is found in a string list, the escaped replacement.</summary>
        public const string ESCAPED_SEPARATOR = "__semi";

        #region Documentation
        /// <summary>   Makes a string list. </summary>
        /// <param name="elements">     The elements to combine into a list. </param>
        /// <param name="separator">    The separator character between elements in a string list. </param>
        /// <param name="escape">       The escape string for separator characters. </param>
        /// <returns>   A string. </returns>
        #endregion
        public static string MakeList(IEnumerable<string> elements, string separator = SEPARATOR, string escape = ESCAPED_SEPARATOR)
        {
            return string.Join(separator, from elt in elements select elt.Replace(separator, escape));
        }

        #region Documentation
        /// <summary>   Makes a list from parameters. </summary>
        /// <param name="elements"> The elements to combine into a list. </param>
        /// <returns>   A string. </returns>
        #endregion
        public static string MakeList(params string[] elements)
        {
            return MakeList(elements.AsEnumerable());
        }

        #region Documentation
        /// <summary>   A string extension method that splits a list. </summary>
        /// <param name="str">          The str to act on. </param>
        /// <param name="separator">    The separator character between elements in a string list. </param>
        /// <param name="escape">       The escape string for separator characters. </param>
        /// <returns>   A string[]. </returns>
        #endregion
        public static string[] SplitList(this string str, string separator = SEPARATOR, string escape = ESCAPED_SEPARATOR)
        {
            var elements = str.Split(separator[0]);
            return (from elt in elements select elt.Replace(escape, separator)).ToArray();
        }
    }
}
