// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return a string in lowercase format.
    /// ToLower function takes a string as the first argument 
    /// and an optional locale string whose default value is Thread.CurrentThread.CurrentCulture.Name.
    /// If a character in the string doesn't have a lowercase version, that character stays unchanged in the returned string.
    /// </summary>
    internal class ToLower : StringTransformEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ToLower"/> class.
        /// </summary>
        public ToLower()
            : base(ExpressionType.ToLower, Function)
        {
        }

        private static (object, string) Function(IReadOnlyList<object> args, Options options)
        {
            string result = null;
            string error = null;
            var locale = options.Locale != null ? new CultureInfo(options.Locale) : Thread.CurrentThread.CurrentCulture;
            (locale, error) = FunctionUtils.DetermineLocale(args, locale, 2);

            if (error == null)
            {
                if (args[0] == null)
                {
                    result = string.Empty;
                }
                else
                {
                    result = args[0].ToString().ToLower(locale);
                }
            }

            return (result, error);
        }
    }
}
