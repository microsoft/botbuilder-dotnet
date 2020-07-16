// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Converts the specified string to sentence case.
    /// </summary>
    public class SentenceCase : StringTransformEvaluator
    {
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
