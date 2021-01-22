// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Globalization;
using System.Threading;
using Newtonsoft.Json;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return the string version of a value.
    /// String function takes an object as the first argument and an optional locale string.
    /// </summary>
#pragma warning disable CA1716 // Identifiers should not match keywords (by design and can't break binary compat, excluding)
#pragma warning disable CA1720 // Identifier contains type name (by design and can't change this because of backward compat)
    internal class String : ExpressionEvaluator
#pragma warning restore CA1720 // Identifier contains type name
#pragma warning restore CA1716 // Identifiers should not match keywords
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="String"/> class.
        /// </summary>
        public String()
            : base(ExpressionType.String, Evaluator(), ReturnType.String, expr => FunctionUtils.ValidateOrder(expr, new[] { ReturnType.String }, ReturnType.Object))
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.ApplyWithOptionsAndError(
                (args, options) => 
                {
                    string error = null;
                    string result = null;
                    var locale = options.Locale != null ? new CultureInfo(options.Locale) : Thread.CurrentThread.CurrentCulture;
                    if (error == null)
                    {
                        (locale, error) = FunctionUtils.DetermineLocale(args, locale, 2);
                    }

                    if (error == null)
                    {
                        if (args[0].IsNumber())
                        {
                            result = Convert.ToDouble(args[0], CultureInfo.InvariantCulture).ToString(locale);
                        }
                        else if (args[0] is DateTime dt)
                        {
                            result = dt.ToString(FunctionUtils.DefaultDateTimeFormat, locale);
                        }
                        else if (args[0] is string str)
                        {
                            result = str;
                        }
                        else if (args[0] is byte[] byteArr)
                        {
                            result = System.Text.Encoding.UTF8.GetString(byteArr);
                        }
                        else
                        {
                            result = JsonConvert.SerializeObject(args[0]).TrimStart('"').TrimEnd('"');
                        }
                    }

                    return (result, error);
                });
        }
    }
}
