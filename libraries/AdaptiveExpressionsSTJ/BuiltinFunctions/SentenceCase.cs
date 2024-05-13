// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Converts the specified string to sentence case.
    /// SentenceCase function takes a string as the first argument 
    /// and an optional locale string whose default value is Thread.CurrentThread.CurrentCulture.Name.
    /// </summary>
    internal class SentenceCase : StringTransformEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SentenceCase"/> class.
        /// </summary>
        public SentenceCase()
            : base(ExpressionType.SentenceCase, Function)
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
                var inputStr = (string)args[0];
                if (string.IsNullOrEmpty(inputStr))
                {
                    result = string.Empty;
                }
                else
                {
                    result = inputStr.Substring(0, 1).ToUpper(locale) + inputStr.Substring(1).ToLower(locale);
                }
            }

            return (result, error);
        }
    }
}
