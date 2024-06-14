﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;

namespace Microsoft.AdaptiveExpressions.Core.BuiltinFunctions
{
    /// <summary>
    /// Converts the specified string to title case.
    /// TitleCase function takes a string as the first argument 
    /// and an optional locale string whose default value is Thread.CurrentThread.CurrentCulture.Name.
    /// </summary>
    internal class TitleCase : StringTransformEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TitleCase"/> class.
        /// </summary>
        public TitleCase()
            : base(ExpressionType.TitleCase, Function)
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
                    var ti = locale.TextInfo;
                    result = ti.ToTitleCase(inputStr);

                    // Case that starts with number.
                    result = Regex.Replace(
                        result,
                        "([0-9]+[a-zA-Z]+)",
                        new MatchEvaluator((m) =>
                        {
                            return m.Captures[0].Value.ToLower(locale);
                        }),
                        RegexOptions.IgnoreCase);
                }
            }

            return (result, error);
        }
    }
}
