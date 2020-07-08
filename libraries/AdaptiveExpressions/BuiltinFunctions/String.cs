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
    /// </summary>
    public class String : ExpressionEvaluator
    {
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
                        if (args.Count == 2 && !(args[1] is string))
                        {
                            error = $"the second argument {args[1]} should be a locale string.";
                        }
                        else
                        {
                            (locale, error) = FunctionUtils.DetermineLocale(args, locale, 2);
                        }
                    }

                    if (error == null)
                    {
                        if (args[0].IsNumber())
                        {
                            result = Convert.ToDouble(args[0]).ToString(locale);
                        }
                        else if (args[0] is DateTime dt)
                        {
                            result = dt.ToString(FunctionUtils.DefaultDateTimeFormat, locale);
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
