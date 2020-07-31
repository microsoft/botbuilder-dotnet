using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Parsers
{
    /// <summary>
    /// A custom expression parser.
    /// </summary>
    public static class TemplateExpressionParser
    {
        private static readonly Regex FuncRegex = new Regex(@"{\b[^()]+\((.*)\)}$", RegexOptions.Compiled);
        private static readonly Regex ArgsRegex = new Regex(@"([^,]+\(.+?\))|([^,]+)", RegexOptions.Compiled);

        /// <summary>
        /// Parses a string template and retrieves a collection of arg values.
        /// </summary>
        /// <param name="template">The string template of the form {&lt;func&gt;(&lt;arg1&gt;, &lt;arg2&gt;, &lt;argn&gt;)}.</param>
        /// <returns>An object containing the values &lt;func&gt; and a collection of arg values.</returns>
        public static List<string> Parse(string template)
        {
            if (string.IsNullOrEmpty(template))
            {
                throw new ArgumentNullException(nameof(template));
            }

            var func = FuncRegex.Match(template);

            string innerArgs = func?.Groups?[1]?.Value;

            if (innerArgs == null)
            {
                throw new ArgumentException("Expected function format {<func>(<arg1>, <arg2>, <argn>)}", nameof(template));
            }

            var paramTags = ArgsRegex.Matches(innerArgs);

            var paramsList = new List<string>();

            foreach (var param in paramTags)
            {
                paramsList.Add(param.ToString());
            }

            return paramsList;
        }
    }
}
