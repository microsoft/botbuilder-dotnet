// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using AdaptiveExpressions.BuiltinFunctions;
using AdaptiveExpressions.Memory;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AdaptiveExpressions
{
    /// <summary>
    /// Definition of default built-in functions for expressions.
    /// </summary>
    /// <remarks>
    /// These functions are largely from WDL https://docs.microsoft.com/en-us/azure/logic-apps/workflow-definition-language-functions-reference
    /// with a few extensions like infix operators for math, logic and comparisons.
    ///
    /// This class also has some methods that are useful to use when defining custom functions.
    /// You can always construct a <see cref="ExpressionEvaluator"/> directly which gives the maximum amount of control over validation and evaluation.
    /// Validators are static checkers that should throw an exception if something is not valid statically.
    /// Evaluators are called to evaluate an expression and should try not to throw.
    /// There are some evaluators in this file that take in a verifier that is called at runtime to verify arguments are proper.
    /// </remarks>
    public static class ExpressionFunctions
    {

        private static IDictionary<string, ExpressionEvaluator> GetStandardFunctions()
        {
            var functions = new List<ExpressionEvaluator>
            {
                // Math
                new ExpressionEvaluator(ExpressionType.Element, ExtractElement, ReturnType.Object, ValidateBinary),
                MultivariateNumeric(ExpressionType.Subtract, args => Mod(args[0], args[1])),
                MultivariateNumeric(ExpressionType.Multiply, args => Multiply(args[0], args[1])),
                MultivariateNumeric(
                    ExpressionType.Divide,
                    args => Divide(args[0], args[1]),
                    (val, expression, pos) =>
                    {
                        var error = VerifyNumber(val, expression, pos);
                        if (error == null && (pos > 0 && Convert.ToSingle(val) == 0.0))
                        {
                            error = $"Cannot divide by 0 from {expression}";
                        }

                        return error;
                    }),
                NumericOrCollection(ExpressionType.Min, (args) =>
                {
                    object result = double.MaxValue;
                    if (args.Count == 1)
                    {
                        if (TryParseList(args[0], out IList ilist))
                        {
                            foreach (var value in ilist)
                            {
                                result = Min(result, value);
                            }
                        }
                        else
                        {
                            result = Min(result, args[0]);
                        }
                    }
                    else
                    {
                        foreach (var arg in args)
                        {
                            if (TryParseList(arg, out IList ilist))
                            {
                                foreach (var value in ilist)
                                {
                                    result = Min(result, value);
                                }
                            }
                            else
                            {
                                result = Min(result, arg);
                            }
                        }
                    }

                    return result;
                }),
                NumericOrCollection(ExpressionType.Max, args =>
                {
                    object result = double.MinValue;
                    if (args.Count == 1)
                    {
                        if (TryParseList(args[0], out IList ilist))
                        {
                            foreach (var value in ilist)
                            {
                                result = Max(result, value);
                            }
                        }
                        else
                        {
                            result = Max(result, args[0]);
                        }
                    }
                    else
                    {
                        foreach (var arg in args)
                        {
                            if (TryParseList(arg, out IList ilist))
                            {
                                foreach (var value in ilist)
                                {
                                    result = Max(result, value);
                                }
                            }
                            else
                            {
                                result = Max(result, arg);
                            }
                        }
                    }

                    return result;
                }),
                MultivariateNumeric(ExpressionType.Power, args => Math.Pow(CultureInvariantDoubleConvert(args[0]), CultureInvariantDoubleConvert(args[1]))),
                new ExpressionEvaluator(
                    ExpressionType.Mod,
                    ApplyWithError(
                        args =>
                        {
                            object value = null;
                            string error;
                            if (Convert.ToInt64(args[1]) == 0)
                            {
                                error = $"Cannot mod by 0";
                            }
                            else
                            {
                                error = null;
                                value = Mod(args[0], args[1]);
                            }

                            return (value, error);
                        },
                        VerifyInteger),
                    ReturnType.Number,
                    ValidateBinaryNumber),
                new ExpressionEvaluator(
                    ExpressionType.Average,
                    Apply(
                        args =>
                        {
                            var operands = ResolveListValue(args[0]).OfType<object>().ToList();
                            return operands.Average(u => Convert.ToSingle(u));
                        },
                        VerifyNumericList),
                    ReturnType.Number,
                    ValidateUnary),
                new ExpressionEvaluator(
                    ExpressionType.Add,
                    ApplySequenceWithError(
                        args =>
                        {
                            object result = null;
                            string error = null;
                            var firstItem = args[0];
                            var secondItem = args[1];
                            var stringConcat = !firstItem.IsNumber() || !secondItem.IsNumber();

                            if ((firstItem == null && secondItem.IsNumber())
                                || (secondItem == null && firstItem.IsNumber()))
                            {
                                error = "Operator '+' or add cannot be applied to operands of type 'number' and null object.";
                            }
                            else
                            {
                                if (stringConcat)
                                {
                                    result = $"{firstItem?.ToString()}{secondItem?.ToString()}";
                                }
                                else
                                {
                                    result = Add(args[0], args[1]);
                                }
                            }

                            return (result, error);
                        }, VerifyNumberOrStringOrNull),
                    ReturnType.String | ReturnType.Number,
                    (expression) => ValidateArityAndAnyType(expression, 2, int.MaxValue, ReturnType.String | ReturnType.Number)),
                new ExpressionEvaluator(
                    ExpressionType.Sum,
                    Apply(
                        args =>
                        {
                            var operands = ResolveListValue(args[0]).OfType<object>().ToList();
                            return operands.All(u => u.IsInteger()) ? operands.Sum(u => Convert.ToInt64(u)) : operands.Sum(u => Convert.ToSingle(u));
                        },
                        VerifyNumericList),
                    ReturnType.Number,
                    (expression) => ValidateOrder(expression, null, ReturnType.Array)),
                new ExpressionEvaluator(
                    ExpressionType.Range,
                    ApplyWithError(
                        args =>
                        {
                            string error = null;
                            IList result = null;
                            var count = Convert.ToInt32(args[1]);
                            if (count <= 0)
                            {
                                error = $"The second parameter {args[1]} should be more than zero";
                            }
                            else
                            {
                                result = Enumerable.Range(Convert.ToInt32(args[0]), count).ToList();
                            }

                            return (result, error);
                        },
                        VerifyInteger),
                    ReturnType.Array,
                    ValidateBinaryNumber),
                NumberTransform(
                    ExpressionType.Floor,
                    args => Math.Floor(Convert.ToDouble(args[0]))),
                NumberTransform(
                    ExpressionType.Ceiling,
                    args => Math.Ceiling(Convert.ToDouble(args[0]))),
                new ExpressionEvaluator(
                    ExpressionType.Round,
                    ApplyWithError(
                        args =>
                        {
                        string error = null;
                        object result = null;
                        if (args.Count == 2 && !args[1].IsInteger())
                        {
                            error = $"The second parameter {args[1]} must be an integer.";
                        }

                        if (error == null)
                        {
                            var digits = args.Count == 2 ? Convert.ToInt32(args[1]) : 0;
                            if (digits < 0 || digits > 15)
                            {
                                error = $"The second parameter {args[1]} must be an integer between 0 and 15.";
                            }
                            else
                            {
                                result = Math.Round(Convert.ToDouble(args[0]), digits);
                            }
                        }

                        return (result, error);
                    }, VerifyNumber),
                    ReturnType.Number,
                    ValidateUnaryOrBinaryNumber),

                // Collection Functions
                new ExpressionEvaluator(
                    ExpressionType.Count,
                    Apply(
                        args =>
                        {
                            object count = null;
                            if (args[0] is string string0)
                            {
                                count = string0.Length;
                            }
                            else if (args[0] is IList list)
                            {
                                count = list.Count;
                            }

                            return count;
                        }, VerifyContainer),
                    ReturnType.Number,
                    (expression) => ValidateOrder(expression, null, ReturnType.String | ReturnType.Array)),
                new ExpressionEvaluator(
                    ExpressionType.Union,
                    Apply(
                        args =>
                        {
                        var result = (IEnumerable<object>)args[0];
                        for (var i = 1; i < args.Count; i++)
                        {
                            var nextItem = (IEnumerable<object>)args[i];
                            result = result.Union(nextItem);
                        }

                        return result.ToList();
                        }, VerifyList),
                    ReturnType.Array,
                    (expression) => ValidateArityAndAnyType(expression, 1, int.MaxValue, ReturnType.Array)),
                new ExpressionEvaluator(
                    ExpressionType.Intersection,
                    Apply(
                        args =>
                        {
                        var result = (IEnumerable<object>)args[0];
                        for (var i = 1; i < args.Count; i++)
                        {
                            var nextItem = (IEnumerable<object>)args[i];
                            result = result.Intersect(nextItem);
                        }

                        return result.ToList();
                        }, VerifyList),
                    ReturnType.Array,
                    (expression) => ValidateArityAndAnyType(expression, 1, int.MaxValue, ReturnType.Array)),
                new ExpressionEvaluator(
                    ExpressionType.Skip,
                    Skip,
                    ReturnType.Array,
                    (expression) => ValidateOrder(expression, null, ReturnType.Array, ReturnType.Number)),
                new ExpressionEvaluator(
                    ExpressionType.Take,
                    Take,
                    ReturnType.Array,
                    (expression) => ValidateOrder(expression, null, ReturnType.Array, ReturnType.Number)),
                new ExpressionEvaluator(
                    ExpressionType.SubArray,
                    SubArray,
                    ReturnType.Array,
                    (expression) => ValidateOrder(expression, new[] { ReturnType.Number }, ReturnType.Array, ReturnType.Number)),
                new ExpressionEvaluator(
                    ExpressionType.SortBy,
                    SortBy(false),
                    ReturnType.Array,
                    (expression) => ValidateOrder(expression, new[] { ReturnType.String }, ReturnType.Array)),
                new ExpressionEvaluator(
                    ExpressionType.SortByDescending,
                    SortBy(true),
                    ReturnType.Array,
                    (expression) => ValidateOrder(expression, new[] { ReturnType.String }, ReturnType.Array)),
                new ExpressionEvaluator(ExpressionType.IndicesAndValues, IndicesAndValues, ReturnType.Array, ValidateUnary),
                new ExpressionEvaluator(
                    ExpressionType.Flatten,
                    Apply(
                        args =>
                        {
                            var depth = args.Count > 1 ? Convert.ToInt32(args[1]) : 100;
                            return Flatten((IEnumerable<object>)args[0], depth);
                        }),
                    ReturnType.Array,
                    (expression) => ValidateOrder(expression, new[] { ReturnType.Number }, ReturnType.Array)),
                new ExpressionEvaluator(
                    ExpressionType.Unique,
                    Apply(
                        args =>
                        {
                            return ((IEnumerable<object>)args[0]).Distinct().ToList();
                        }, VerifyList),
                    ReturnType.Array,
                    (expression) => ValidateOrder(expression, null, ReturnType.Array)),

                // Booleans
                Comparison(ExpressionType.LessThan, args => CultureInvariantDoubleConvert(args[0]) < CultureInvariantDoubleConvert(args[1]), ValidateBinaryNumberOrString, VerifyNumberOrString),
                Comparison(ExpressionType.LessThanOrEqual, args => CultureInvariantDoubleConvert(args[0]) <= CultureInvariantDoubleConvert(args[1]), ValidateBinaryNumberOrString, VerifyNumberOrString),

                Comparison(ExpressionType.Equal, IsEqual, ValidateBinary),
                Comparison(ExpressionType.NotEqual, args => !IsEqual(args), ValidateBinary),
                Comparison(ExpressionType.GreaterThan, args => CultureInvariantDoubleConvert(args[0]) > CultureInvariantDoubleConvert(args[1]), ValidateBinaryNumberOrString, VerifyNumberOrString),
                Comparison(ExpressionType.GreaterThanOrEqual, args => CultureInvariantDoubleConvert(args[0]) >= CultureInvariantDoubleConvert(args[1]), ValidateBinaryNumberOrString, VerifyNumberOrString),
                Comparison(ExpressionType.Exists, args => args[0] != null, ValidateUnary, VerifyNotNull),
                new ExpressionEvaluator(
                    ExpressionType.Contains,
                    (expression, state, options) =>
                    {
                        var found = false;
                        var (args, error) = EvaluateChildren(expression, state, options);
                        if (error == null)
                        {
                            if (args[0] is string string0 && args[1] is string string1)
                            {
                                found = string0.Contains(string1);
                            }
                            else if (TryParseList(args[0], out IList ilist))
                            {
                                // list to find a value
                                var operands = ResolveListValue(ilist);
                                found = operands.Contains(args[1]);
                            }
                            else if (args[1] is string string2)
                            {
                                found = TryAccessProperty((object)args[0], string2, out var _);
                            }
                        }

                        return (found, null);
                    },
                    ReturnType.Boolean,
                    ValidateBinary),
                Comparison(ExpressionType.Empty, args => IsEmpty(args[0]), ValidateUnary, VerifyContainer),
                new ExpressionEvaluator(ExpressionType.And, (expression, state, options) => And(expression, state, options), ReturnType.Boolean, ValidateAtLeastOne),
                new ExpressionEvaluator(ExpressionType.Or, (expression, state, options) => Or(expression, state, options), ReturnType.Boolean, ValidateAtLeastOne),
                new ExpressionEvaluator(ExpressionType.Not, (expression, state, options) => Not(expression, state, options), ReturnType.Boolean, ValidateUnary),

                // String
                new ExpressionEvaluator(
                    ExpressionType.Concat,
                    ApplySequence(
                        args =>
                        {
                            var firstItem = args[0];
                            var secondItem = args[1];
                            var isFirstList = TryParseList(firstItem, out var firstList);
                            var isSecondList = TryParseList(secondItem, out var secondList);

                            if (firstItem == null && secondItem == null)
                            {
                                return null;
                            }
                            else if (firstItem == null && isSecondList)
                            {
                                return secondList;
                            }
                            else if (secondItem == null && isFirstList)
                            {
                                return firstList;
                            }
                            else if (isFirstList && isSecondList)
                            {
                                return firstList.OfType<object>().Concat(secondList.OfType<object>()).ToList();
                            }
                            else
                            {
                                return $"{firstItem?.ToString()}{secondItem?.ToString()}";
                            }
                        }),
                    ReturnType.Array | ReturnType.String,
                    ValidateAtLeastOne),
                new ExpressionEvaluator(
                    ExpressionType.Length,
                    Apply(
                        args =>
                        {
                            var result = 0;
                            if (args[0] is string str)
                                {
                                    result = str.Length;
                                }
                            else
                                {
                                    result = 0;
                                }

                            return result;
                        }, VerifyStringOrNull),
                    ReturnType.Number,
                    ValidateUnaryString),
                new ExpressionEvaluator(
                    ExpressionType.Replace,
                    ApplyWithError(
                        args =>
                        {
                            string error = null;
                            string result = null;
                            string inputStr = ParseStringOrNull(args[0]);
                            string oldStr = ParseStringOrNull(args[1]);
                            if (oldStr.Length == 0)
                            {
                                error = $"{args[1]} the oldValue in replace function should be a string with at least length 1.";
                            }

                            string newStr = ParseStringOrNull(args[2]);
                            if (error == null)
                            {
                                result = inputStr.Replace(oldStr, newStr);
                            }

                            return (result, error);
                        }, VerifyStringOrNull),
                    ReturnType.String,
                    (expression) => ValidateArityAndAnyType(expression, 3, 3, ReturnType.String)),
                new ExpressionEvaluator(
                    ExpressionType.ReplaceIgnoreCase,
                    ApplyWithError(
                        args =>
                        {
                            string error = null;
                            string result = null;
                            string inputStr = ParseStringOrNull(args[0]);
                            string oldStr = ParseStringOrNull(args[1]);
                            if (oldStr.Length == 0)
                            {
                                error = $"{args[1]} the oldValue in replace function should be a string with at least length 1.";
                            }

                            string newStr = ParseStringOrNull(args[2]);
                            if (error == null)
                            {
                                result = Regex.Replace(inputStr, oldStr, newStr, RegexOptions.IgnoreCase);
                            }

                            return (result, error);
                        }, VerifyStringOrNull),
                    ReturnType.String,
                    (expression) => ValidateArityAndAnyType(expression, 3, 3, ReturnType.String)),
                new ExpressionEvaluator(
                    ExpressionType.Split,
                    Apply(
                        args =>
                        {
                            var inputStr = string.Empty;
                            var seperator = string.Empty;
                            if (args.Count == 1)
                            {
                                inputStr = ParseStringOrNull(args[0]);
                            }
                            else
                            {
                                inputStr = ParseStringOrNull(args[0]);
                                seperator = ParseStringOrNull(args[1]);
                            }

                            if (seperator == string.Empty)
                            {
                                return inputStr.Select(c => c.ToString()).ToArray();
                            }

                            return inputStr.Split(seperator.ToCharArray());
                        }, VerifyStringOrNull),
                    ReturnType.Array,
                    (expression) => ValidateArityAndAnyType(expression, 1, 2, ReturnType.String)),
                new ExpressionEvaluator(
                    ExpressionType.Substring,
                    Substring,
                    ReturnType.String,
                    (expression) => ValidateOrder(expression, new[] { ReturnType.Number }, ReturnType.String, ReturnType.Number)),
                StringTransform(
                                ExpressionType.ToLower,
                                args =>
                                {
                                    if (args[0] == null)
                                    {
                                        return string.Empty;
                                    }
                                    else
                                    {
                                        return args[0].ToString().ToLowerInvariant();
                                    }
                                }),
                StringTransform(
                                ExpressionType.ToUpper,
                                args =>
                                {
                                    if (args[0] == null)
                                    {
                                        return string.Empty;
                                    }
                                    else
                                    {
                                        return args[0].ToString().ToUpperInvariant();
                                    }
                                }),
                StringTransform(
                                ExpressionType.Trim,
                                args =>
                                {
                                    if (args[0] == null)
                                    {
                                        return string.Empty;
                                    }
                                    else
                                    {
                                        return args[0].ToString().Trim();
                                    }
                                }),
                new ExpressionEvaluator(
                    ExpressionType.StartsWith,
                    Apply(
                        args =>
                        {
                            string rawStr = ParseStringOrNull(args[0]);
                            string seekStr = ParseStringOrNull(args[1]);
                            return rawStr.StartsWith(seekStr);
                        }, VerifyStringOrNull),
                    ReturnType.Boolean,
                    (expression) => ValidateArityAndAnyType(expression, 2, 2, ReturnType.String)),
                new ExpressionEvaluator(
                    ExpressionType.EndsWith,
                    Apply(
                        args =>
                        {
                            string rawStr = ParseStringOrNull(args[0]);
                            string seekStr = ParseStringOrNull(args[1]);
                            return rawStr.EndsWith(seekStr);
                        }, VerifyStringOrNull),
                    ReturnType.Boolean,
                    (expression) => ValidateArityAndAnyType(expression, 2, 2, ReturnType.String)),
                new ExpressionEvaluator(
                    ExpressionType.CountWord,
                    Apply(
                        args =>
                        {
                            if (args[0] is string)
                            {
                                return Regex.Split(args[0].ToString().Trim(), @"\s{1,}").Length;
                            }
                            else
                            {
                                return 0;
                            }
                        }, VerifyStringOrNull),
                    ReturnType.Number,
                    ValidateUnaryString),
                new ExpressionEvaluator(
                    ExpressionType.AddOrdinal,
                    Apply(args => AddOrdinal(Convert.ToInt32(args[0])), VerifyInteger),
                    ReturnType.String,
                    (expression) => ValidateArityAndAnyType(expression, 1, 1, ReturnType.Number)),
                new ExpressionEvaluator(
                    ExpressionType.Join,
                    (expression, state, options) =>
                    {
                        object result = null;
                        var (args, error) = EvaluateChildren(expression, state, options);
                        if (error == null)
                        {
                            if (!TryParseList(args[0], out IList list))
                            {
                                error = $"{expression.Children[0]} evaluates to {args[0]} which is not a list.";
                            }
                            else
                            {
                                if (args.Count == 2)
                                {
                                    result = string.Join(args[1].ToString(), list.OfType<object>().Select(x => x.ToString()));
                                }
                                else
                                {
                                    if (list.Count < 3)
                                    {
                                        result = string.Join(args[2].ToString(), list.OfType<object>().Select(x => x.ToString()));
                                    }
                                    else
                                    {
                                        var firstPart = string.Join(args[1].ToString(), list.OfType<object>().TakeWhile(o => o != null && o != list.OfType<object>().LastOrDefault()));
                                        result = firstPart + args[2] + list.OfType<object>().Last().ToString();
                                    }
                                }
                            }
                        }

                        return (result, error);
                    },
                    ReturnType.String,
                    expr => ValidateOrder(expr, new[] { ReturnType.String }, ReturnType.Array, ReturnType.String)),
                new NewGuid(),
                new ExpressionEvaluator(
                    ExpressionType.EOL,
                    Apply(args => Environment.NewLine),
                    ReturnType.String,
                    (exprssion) => ValidateArityAndAnyType(exprssion, 0, 0)),
                new ExpressionEvaluator(
                    ExpressionType.IndexOf,
                    (expression, state, options) =>
                    {
                        object result = -1;
                        var (args, error) = EvaluateChildren(expression, state, options);
                        if (error == null)
                        {
                            if (args[0] is string || args[0] == null)
                            {
                                if (args[1] is string || args[1] == null)
                                {
                                    result = ParseStringOrNull(args[0]).IndexOf(ParseStringOrNull(args[1]));
                                }
                                else
                                {
                                    error = $"Can only look for indexof string in {expression}";
                                }
                            }
                            else if (TryParseList(args[0], out IList list))
                            {
                                result = ResolveListValue(list).IndexOf(args[1]);
                            }
                            else
                            {
                                error = $"{expression} works only on string or list.";
                            }
                        }

                        return (result, error);
                    },
                    ReturnType.Number,
                    expr => ValidateOrder(expr, null, ReturnType.Array | ReturnType.String, ReturnType.Object)),
                new ExpressionEvaluator(
                    ExpressionType.LastIndexOf,
                    (expression, state, options) =>
                    {
                        object result = -1;
                        var (args, error) = EvaluateChildren(expression, state, options);
                        if (error == null)
                        {
                            if (args[0] is string || args[0] == null)
                            {
                                if (args[1] is string || args[1] == null)
                                {
                                    result = ParseStringOrNull(args[0]).LastIndexOf(ParseStringOrNull(args[1]));
                                }
                                else
                                {
                                    error = $"Can only look for indexof string in {expression}";
                                }
                            }
                            else if (TryParseList(args[0], out IList list))
                            {
                                result = ResolveListValue(list).OfType<object>().ToList().LastIndexOf(args[1]);
                            }
                            else
                            {
                                error = $"{expression} works only on string or list.";
                            }
                        }

                        return (result, error);
                    },
                    ReturnType.Number,
                    expr => ValidateOrder(expr, null, ReturnType.Array | ReturnType.String, ReturnType.Object)),
                StringTransform(
                                ExpressionType.SentenceCase,
                                args =>
                                {
                                    var inputStr = (string)args[0];
                                    if (string.IsNullOrEmpty(inputStr))
                                    {
                                        return string.Empty;
                                    }
                                    else
                                    {
                                        return inputStr.Substring(0, 1).ToUpperInvariant() + inputStr.Substring(1).ToLowerInvariant();
                                    }
                                }),
                StringTransform(
                                ExpressionType.TitleCase,
                                args =>
                                {
                                    var inputStr = (string)args[0];
                                    if (string.IsNullOrEmpty(inputStr))
                                    {
                                        return string.Empty;
                                    }
                                    else
                                    {
                                        var ti = CultureInfo.InvariantCulture.TextInfo;
                                        return ti.ToTitleCase(inputStr);
                                    }
                                }),

                // Date and time
                TimeTransform(ExpressionType.AddDays, (ts, add) => ts.AddDays(add)),
                TimeTransform(ExpressionType.AddHours, (ts, add) => ts.AddHours(add)),
                TimeTransform(ExpressionType.AddMinutes, (ts, add) => ts.AddMinutes(add)),
                TimeTransform(ExpressionType.AddSeconds, (ts, add) => ts.AddSeconds(add)),
                new ExpressionEvaluator(
                    ExpressionType.DayOfMonth,
                    ApplyWithError(args => NormalizeToDateTime(args[0], dt => dt.Day)),
                    ReturnType.Number,
                    ValidateUnary),
                new ExpressionEvaluator(
                    ExpressionType.DayOfWeek,
                    ApplyWithError(args => NormalizeToDateTime(args[0], dt => Convert.ToInt32(dt.DayOfWeek))),
                    ReturnType.Number,
                    ValidateUnary),
                new ExpressionEvaluator(
                    ExpressionType.DayOfYear,
                    ApplyWithError(args => NormalizeToDateTime(args[0], dt => dt.DayOfYear)),
                    ReturnType.Number,
                    ValidateUnary),
                new ExpressionEvaluator(
                    ExpressionType.Month,
                    ApplyWithError(args => NormalizeToDateTime(args[0], dt => dt.Month)),
                    ReturnType.Number,
                    ValidateUnary),
                new ExpressionEvaluator(
                    ExpressionType.Date,
                    ApplyWithError(args => NormalizeToDateTime(args[0], dt => dt.Date.ToString("M/dd/yyyy", CultureInfo.InvariantCulture))),
                    ReturnType.String,
                    ValidateUnary),
                new ExpressionEvaluator(
                    ExpressionType.Year,
                    ApplyWithError(args => NormalizeToDateTime(args[0], dt => dt.Year)),
                    ReturnType.Number,
                    ValidateUnary),
                new ExpressionEvaluator(
                    ExpressionType.UtcNow,
                    Apply(args => DateTime.UtcNow.ToString(args.Count() == 1 ? args[0].ToString() : DefaultDateTimeFormat)),
                    ReturnType.String),
                new ExpressionEvaluator(
                    ExpressionType.FormatDateTime,
                    ApplyWithError(
                        args =>
                        {
                            object result = null;
                            string error = null;
                            var timestamp = args[0];
                            if (timestamp is string tsString)
                            {
                                (result, error) = ParseTimestamp(tsString, dt => dt.ToString(args.Count() == 2 ? args[1].ToString() : DefaultDateTimeFormat, CultureInfo.InvariantCulture));
                            }
                            else if (timestamp is DateTime dt)
                            {
                                result = dt.ToString(args.Count() == 2 ? args[1].ToString() : DefaultDateTimeFormat, CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                error = $"formatDateTime has invalid first argument {timestamp}";
                            }

                            return (result, error);
                        }),
                    ReturnType.String,
                    (expr) => ValidateOrder(expr, new[] { ReturnType.String }, ReturnType.Object)),
                new ExpressionEvaluator(
                    ExpressionType.FormatEpoch,
                    ApplyWithError(
                        args =>
                        {
                            object result = null;
                            string error = null;
                            var timestamp = args[0];
                            if (timestamp.IsNumber())
                            {
                                var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                                dateTime = dateTime.AddSeconds(Convert.ToDouble(timestamp));
                                result = dateTime.ToString(args.Count() == 2 ? args[1].ToString() : DefaultDateTimeFormat, CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                error = $"formatEpoch first argument {timestamp} is not a number";
                            }

                            return (result, error);
                        }),
                    ReturnType.String,
                    (expr) => ValidateOrder(expr, new[] { ReturnType.String }, ReturnType.Number)),
                new ExpressionEvaluator(
                    ExpressionType.FormatTicks,
                    ApplyWithError(
                        args =>
                        {
                            object result = null;
                            string error = null;
                            var timestamp = args[0];
                            if (timestamp.IsInteger())
                            {
                                var ticks = Convert.ToInt64(timestamp);
                                var dateTime = new DateTime(ticks);
                                result = dateTime.ToString(args.Count() == 2 ? args[1].ToString() : DefaultDateTimeFormat, CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                error = $"formatTicks first arugment {timestamp} must be an integer";
                            }

                            return (result, error);
                        }),
                    ReturnType.String,
                    (expr) => ValidateOrder(expr, new[] { ReturnType.String }, ReturnType.Number)),
                new ExpressionEvaluator(
                    ExpressionType.SubtractFromTime,
                    (expr, state, options) =>
                    {
                        object value = null;
                        string error = null;
                        IReadOnlyList<object> args;
                        (args, error) = EvaluateChildren(expr, state, options);
                        if (error == null)
                        {
                            if (args[1].IsInteger() && args[2] is string string2)
                            {
                                var format = (args.Count() == 4) ? (string)args[3] : DefaultDateTimeFormat;
                                Func<DateTime, DateTime> timeConverter;
                                (timeConverter, error) = DateTimeConverter(Convert.ToInt64(args[1]), string2);
                                if (error == null)
                                {
                                    (value, error) = NormalizeToDateTime(args[0], dt => timeConverter(dt).ToString(format));
                                }
                            }
                            else
                            {
                                error = $"{expr} should contain an ISO format timestamp, a time interval integer, a string unit of time and an optional output format string.";
                            }
                        }

                        return (value, error);
                    },
                    ReturnType.String,
                    (expr) => ValidateOrder(expr, new[] { ReturnType.String }, ReturnType.Object, ReturnType.Number, ReturnType.String)),
                new ExpressionEvaluator(
                    ExpressionType.DateReadBack,
                    ApplyWithError(
                        args =>
                        {
                            object result = null;
                            string error;
                            (result, error) = NormalizeToDateTime(args[0]);
                            if (error == null)
                            {
                                var timestamp1 = (DateTime)result;
                                (result, error) = NormalizeToDateTime(args[1]);
                                if (error == null)
                                {
                                    var timestamp2 = (DateTime)result;
                                    var timex = new TimexProperty(timestamp2.ToString("yyyy-MM-dd"));
                                    result = TimexRelativeConvert.ConvertTimexToStringRelative(timex, timestamp1);
                                }
                            }

                            return (result, error);
                        }),
                    ReturnType.String,
                    expr => ValidateOrder(expr, null, ReturnType.String, ReturnType.String)),
                new ExpressionEvaluator(
                    ExpressionType.GetTimeOfDay,
                    ApplyWithError(
                        args =>
                        {
                            object value = null;
                            string error = null;
                            (value, error) = NormalizeToDateTime(args[0]);
                            if (error == null)
                            {
                                var timestamp = (DateTime)value;
                                if (timestamp.Hour == 0 && timestamp.Minute == 0)
                                {
                                    value = "midnight";
                                }
                                else if (timestamp.Hour >= 0 && timestamp.Hour < 12)
                                {
                                    value = "morning";
                                }
                                else if (timestamp.Hour == 12 && timestamp.Minute == 0)
                                {
                                    value = "noon";
                                }
                                else if (timestamp.Hour < 18)
                                {
                                    value = "afternoon";
                                }
                                else if (timestamp.Hour < 22 || (timestamp.Hour == 22 && timestamp.Minute == 0))
                                {
                                    value = "evening";
                                }
                                else
                                {
                                    value = "night";
                                }
                            }

                            return (value, error);
                        }),
                    ReturnType.String,
                    ValidateUnary),
                new ExpressionEvaluator(
                    ExpressionType.GetFutureTime,
                    (expr, state, options) =>
                    {
                        object value = null;
                        string error = null;
                        IReadOnlyList<object> args;
                        (args, error) = EvaluateChildren(expr, state, options);
                        if (error == null)
                        {
                            if (args[0].IsInteger() && args[1] is string string1)
                            {
                                var format = (args.Count() == 3) ? (string)args[2] : DefaultDateTimeFormat;
                                Func<DateTime, DateTime> timeConverter;
                                (timeConverter, error) = DateTimeConverter(Convert.ToInt64(args[0]), string1, false);
                                if (error == null)
                                {
                                    value = timeConverter(DateTime.UtcNow).ToString(format);
                                }
                            }
                            else
                            {
                                error = $"{expr} should contain a time interval integer, a string unit of time and an optional output format string.";
                            }
                        }

                        return (value, error);
                    },
                    ReturnType.String,
                    (expr) => ValidateOrder(expr, new[] { ReturnType.String }, ReturnType.Number, ReturnType.String)),
                new ExpressionEvaluator(
                    ExpressionType.GetPastTime,
                    (expr, state, options) =>
                    {
                        object value = null;
                        string error = null;
                        IReadOnlyList<object> args;
                        (args, error) = EvaluateChildren(expr, state, options);
                        if (error == null)
                        {
                            if (args[0].IsInteger() && args[1] is string string1)
                            {
                                var format = (args.Count() == 3) ? (string)args[2] : DefaultDateTimeFormat;
                                Func<DateTime, DateTime> timeConverter;
                                (timeConverter, error) = DateTimeConverter(Convert.ToInt64(args[0]), string1);
                                if (error == null)
                                {
                                    value = timeConverter(DateTime.UtcNow).ToString(format);
                                }
                            }
                            else
                            {
                                error = $"{expr} should contain a time interval integer, a string unit of time and an optional output format string.";
                            }
                        }

                        return (value, error);
                    },
                    ReturnType.String,
                    (expr) => ValidateOrder(expr, new[] { ReturnType.String }, ReturnType.Number, ReturnType.String)),
                new ExpressionEvaluator(
                    ExpressionType.ConvertFromUtc,
                    (expr, state, options) =>
                    {
                        object value = null;
                        string error = null;
                        IReadOnlyList<object> args;
                        (args, error) = EvaluateChildren(expr, state, options);
                        if (error == null)
                        {
                            var format = (args.Count() == 3) ? (string)args[2] : DefaultDateTimeFormat;
                            if (args[1] is string targetTimeZone)
                            {
                                (value, error) = ConvertFromUTC(args[0], targetTimeZone, format);
                            }
                            else
                            {
                                error = $"{expr} should contain an ISO format timestamp, a destination time zone string and an optional output format string.";
                            }
                        }

                        return (value, error);
                    },
                    ReturnType.String,
                    expr => ValidateArityAndAnyType(expr, 2, 3, ReturnType.String)),
                new ExpressionEvaluator(
                    ExpressionType.ConvertToUtc,
                    (expr, state, options) =>
                    {
                        object value = null;
                        string error = null;
                        IReadOnlyList<object> args;
                        (args, error) = EvaluateChildren(expr, state, options);
                        if (error == null)
                        {
                            var format = (args.Count() == 3) ? (string)args[2] : DefaultDateTimeFormat;
                            if (args[1] is string sourceTimeZone)
                            {
                                (value, error) = ConvertToUTC(args[0], sourceTimeZone, format);
                            }
                            else
                            {
                                error = $"{expr} should contain an ISO format timestamp, a origin time zone string and an optional output format string.";
                            }
                        }

                        return (value, error);
                    },
                    ReturnType.String,
                    expr => ValidateArityAndAnyType(expr, 2, 3, ReturnType.String)),
                new ExpressionEvaluator(
                    ExpressionType.AddToTime,
                    (expr, state, options) =>
                    {
                        object value = null;
                        string error = null;
                        IReadOnlyList<object> args;
                        (args, error) = EvaluateChildren(expr, state, options);
                        if (error == null)
                        {
                            var format = (args.Count() == 4) ? (string)args[3] : DefaultDateTimeFormat;
                            if (args[1].IsInteger() && args[2] is string timeUnit)
                            {
                                (value, error) = AddToTime(args[0], Convert.ToInt64(args[1]), timeUnit, format);
                            }
                            else
                            {
                                error = $"{expr} should contain an ISO format timestamp, a time interval integer, a string unit of time and an optional output format string.";
                            }
                        }

                        return (value, error);
                    },
                    ReturnType.String,
                    expr => ValidateOrder(expr, new[] { ReturnType.String }, ReturnType.Object, ReturnType.Number, ReturnType.String)),
                new ExpressionEvaluator(
                    ExpressionType.StartOfDay,
                    (expr, state, options) =>
                    {
                        object value = null;
                        string error = null;
                        IReadOnlyList<object> args;
                        (args, error) = EvaluateChildren(expr, state, options);
                        if (error == null)
                        {
                            var format = (args.Count() == 2) ? (string)args[1] : DefaultDateTimeFormat;
                            (value, error) = StartOfDay(args[0], format);
                        }

                        return (value, error);
                    },
                    ReturnType.String,
                    expr => ValidateArityAndAnyType(expr, 1, 2, ReturnType.String)),
                new ExpressionEvaluator(
                    ExpressionType.StartOfHour,
                    (expr, state, options) =>
                    {
                        object value = null;
                        string error = null;
                        IReadOnlyList<object> args;
                        (args, error) = EvaluateChildren(expr, state, options);
                        if (error == null)
                        {
                            var format = (args.Count() == 2) ? (string)args[1] : DefaultDateTimeFormat;
                            (value, error) = StartOfHour(args[0], format);
                        }

                        return (value, error);
                    },
                    ReturnType.String,
                    expr => ValidateArityAndAnyType(expr, 1, 2, ReturnType.String)),
                new ExpressionEvaluator(
                    ExpressionType.StartOfMonth,
                    (expr, state, options) =>
                    {
                        object value = null;
                        string error = null;
                        IReadOnlyList<object> args;
                        (args, error) = EvaluateChildren(expr, state, options);
                        if (error == null)
                        {
                            var format = (args.Count() == 2) ? (string)args[1] : DefaultDateTimeFormat;
                            (value, error) = StartOfMonth(args[0], format);
                        }

                        return (value, error);
                    },
                    ReturnType.String,
                    expr => ValidateArityAndAnyType(expr, 1, 2, ReturnType.String)),
                new ExpressionEvaluator(
                    ExpressionType.Ticks,
                    (expr, state, options) =>
                    {
                        object value = null;
                        string error = null;
                        IReadOnlyList<object> args;
                        (args, error) = EvaluateChildren(expr, state, options);
                        if (error == null)
                        {
                            (value, error) = Ticks(args[0]);
                        }

                        return (value, error);
                    },
                    ReturnType.Number,
                    expr => ValidateArityAndAnyType(expr, 1, 1, ReturnType.String)),
                new ExpressionEvaluator(
                    ExpressionType.TicksToDays,
                    (expr, state, options) =>
                    {
                        object value = null;
                        string error = null;
                        IReadOnlyList<object> args;
                        (args, error) = EvaluateChildren(expr, state, options);
                        if (error == null)
                        {
                            if (args[0].IsInteger())
                            {
                                value = Convert.ToDouble(args[0]) / TicksPerDay;
                            }
                            else
                            {
                                error = $"{expr} should contain an integer of ticks";
                            }
                        }

                        return (value, error);
                    },
                    ReturnType.Number,
                    ValidateUnaryNumber),
                new ExpressionEvaluator(
                    ExpressionType.TicksToHours,
                    (expr, state, options) =>
                    {
                        object value = null;
                        string error = null;
                        IReadOnlyList<object> args;
                        (args, error) = EvaluateChildren(expr, state, options);
                        if (error == null)
                        {
                            if (args[0].IsInteger())
                            {
                                value = Convert.ToDouble(args[0]) / TicksPerHour;
                            }
                            else
                            {
                                error = $"{expr} should contain an integer of ticks";
                            }
                        }

                        return (value, error);
                    },
                    ReturnType.Number,
                    ValidateUnaryNumber),
                new ExpressionEvaluator(
                    ExpressionType.TicksToMinutes,
                    (expr, state, options) =>
                    {
                        object value = null;
                        string error = null;
                        IReadOnlyList<object> args;
                        (args, error) = EvaluateChildren(expr, state, options);
                        if (error == null)
                        {
                            if (args[0].IsInteger())
                            {
                                value = Convert.ToDouble(args[0]) / TicksPerMinute;
                            }
                            else
                            {
                                error = $"{expr} should contain an integer of ticks";
                            }
                        }

                        return (value, error);
                    },
                    ReturnType.Number,
                    ValidateUnaryNumber),
                new ExpressionEvaluator(
                    ExpressionType.DateTimeDiff,
                    (expr, state, options) =>
                    {
                        object dateTimeStart = null;
                        object dateTimeEnd = null;
                        object value = null;
                        string error = null;
                        IReadOnlyList<object> args;
                        (args, error) = EvaluateChildren(expr, state, options);
                        if (error == null)
                        {
                            (dateTimeStart, error) = Ticks(args[0]);
                            if (error == null)
                            {
                                (dateTimeEnd, error) = Ticks(args[1]);
                            }
                            else
                            {
                                error = $"{expr} must have two ISO timestamps.";
                            }
                        }

                        if (error == null)
                        {
                            value = (long)dateTimeStart - (long)dateTimeEnd;
                        }

                        return (value, error);
                    },
                    ReturnType.Number,
                    expr => ValidateArityAndAnyType(expr, 2, 2, ReturnType.String)),
                new ExpressionEvaluator(
                    ExpressionType.IsDefinite,
                    (expr, state, options) =>
                    {
                        TimexProperty parsed = null;
                        bool? value = null;
                        string error = null;
                        IReadOnlyList<object> args;
                        (args, error) = EvaluateChildren(expr, state, options);
                        if (error == null)
                        {
                            (parsed, error) = ParseTimexProperty(args[0]);
                        }

                        if (error == null)
                        {
                            value = parsed != null && parsed.Year != null && parsed.Month != null && parsed.DayOfMonth != null;
                        }

                        return (value, error);
                    },
                    ReturnType.Boolean,
                    expr => ValidateArityAndAnyType(expr, 1, 1, ReturnType.Object)),
                new ExpressionEvaluator(
                    ExpressionType.IsTime,
                    (expr, state, options) =>
                    {
                        TimexProperty parsed = null;
                        bool? value = null;
                        string error = null;
                        IReadOnlyList<object> args;
                        (args, error) = EvaluateChildren(expr, state, options);
                        if (error == null)
                        {
                            (parsed, error) = ParseTimexProperty(args[0]);
                        }

                        if (error == null)
                        {
                            value = parsed.Hour != null && parsed.Minute != null && parsed.Second != null;
                        }

                        return (value, error);
                    },
                    ReturnType.Boolean,
                    expr => ValidateArityAndAnyType(expr, 1, 1, ReturnType.Object)),
                new ExpressionEvaluator(
                    ExpressionType.IsDuration,
                    (expr, state, options) =>
                    {
                        TimexProperty parsed = null;
                        bool? value = null;
                        string error = null;
                        IReadOnlyList<object> args;
                        (args, error) = EvaluateChildren(expr, state, options);
                        if (error == null)
                        {
                            (parsed, error) = ParseTimexProperty(args[0]);
                        }

                        if (error == null)
                        {
                            value = parsed.Years != null || parsed.Months != null || parsed.Weeks != null || parsed.Days != null ||
                   parsed.Hours != null || parsed.Minutes != null || parsed.Seconds != null;
                        }

                        return (value, error);
                    },
                    ReturnType.Boolean,
                    expr => ValidateArityAndAnyType(expr, 1, 1, ReturnType.Object)),
                new ExpressionEvaluator(
                    ExpressionType.IsDate,
                    (expr, state, options) =>
                    {
                        TimexProperty parsed = null;
                        bool? value = null;
                        string error = null;
                        IReadOnlyList<object> args;
                        (args, error) = EvaluateChildren(expr, state, options);
                        if (error == null)
                        {
                            (parsed, error) = ParseTimexProperty(args[0]);
                        }

                        if (error == null)
                        {
                            value = (parsed.Month != null && parsed.DayOfMonth != null) || parsed.DayOfWeek != null;
                        }

                        return (value, error);
                    },
                    ReturnType.Boolean,
                    expr => ValidateArityAndAnyType(expr, 1, 1, ReturnType.Object)),
                new ExpressionEvaluator(
                    ExpressionType.IsTimeRange,
                    (expr, state, options) =>
                    {
                        TimexProperty parsed = null;
                        bool? value = null;
                        string error = null;
                        IReadOnlyList<object> args;
                        (args, error) = EvaluateChildren(expr, state, options);
                        if (error == null)
                        {
                            (parsed, error) = ParseTimexProperty(args[0]);
                        }

                        if (error == null)
                        {
                            value = parsed.PartOfDay != null;
                        }

                        return (value, error);
                    },
                    ReturnType.Boolean,
                    expr => ValidateArityAndAnyType(expr, 1, 1, ReturnType.Object)),
                new ExpressionEvaluator(
                    ExpressionType.IsDateRange,
                    (expr, state, options) =>
                    {
                        TimexProperty parsed = null;
                        bool? value = null;
                        string error = null;
                        IReadOnlyList<object> args;
                        (args, error) = EvaluateChildren(expr, state, options);
                        if (error == null)
                        {
                            (parsed, error) = ParseTimexProperty(args[0]);
                        }

                        if (error == null)
                        {
                            value = (parsed.Year != null && parsed.DayOfMonth == null) ||
                                    (parsed.Year != null && parsed.Month != null && parsed.DayOfMonth == null) ||
                                    (parsed.Month != null && parsed.DayOfMonth == null) ||
                                    parsed.Season != null || parsed.WeekOfYear != null || parsed.WeekOfMonth != null;
                        }

                        return (value, error);
                    },
                    ReturnType.Boolean,
                    expr => ValidateArityAndAnyType(expr, 1, 1, ReturnType.Object)),
                new ExpressionEvaluator(
                    ExpressionType.IsPresent,
                    (expr, state, options) =>
                    {
                        TimexProperty parsed = null;
                        bool? value = null;
                        string error = null;
                        IReadOnlyList<object> args;
                        (args, error) = EvaluateChildren(expr, state, options);
                        if (error == null)
                        {
                            (parsed, error) = ParseTimexProperty(args[0]);
                        }

                        if (error == null)
                        {
                            value = parsed.Now != null;
                        }

                        return (value, error);
                    },
                    ReturnType.Boolean,
                    expr => ValidateArityAndAnyType(expr, 1, 1, ReturnType.Object)),

                // URI Parsing
                new ExpressionEvaluator(
                    ExpressionType.UriHost,
                    (expr, state, options) =>
                    {
                        object value = null;
                        string error = null;
                        IReadOnlyList<object> args;
                        (args, error) = EvaluateChildren(expr, state, options);
                        if (error == null)
                        {
                            if (args[0] is string uri)
                            {
                                (value, error) = UriHost(uri);
                            }
                            else
                            {
                                error = $"{expr} should contain a URI string.";
                            }
                        }

                        return (value, error);
                    },
                    ReturnType.String,
                    ValidateUnary),
                new ExpressionEvaluator(
                    ExpressionType.UriPath,
                    (expr, state, options) =>
                    {
                        object value = null;
                        string error = null;
                        IReadOnlyList<object> args;
                        (args, error) = EvaluateChildren(expr, state, options);
                        if (error == null)
                        {
                            if (args[0] is string uri)
                            {
                                (value, error) = UriPath(uri);
                            }
                            else
                            {
                                error = $"{expr} should contain a URI string.";
                            }
                        }

                        return (value, error);
                    },
                    ReturnType.String,
                    ValidateUnary),
                new ExpressionEvaluator(
                    ExpressionType.UriPathAndQuery,
                    (expr, state, options) =>
                    {
                        object value = null;
                        string error = null;
                        IReadOnlyList<object> args;
                        (args, error) = EvaluateChildren(expr, state, options);
                        if (error == null)
                        {
                            if (args[0] is string uri)
                            {
                                (value, error) = UriPathAndQuery(uri);
                            }
                            else
                            {
                                error = $"{expr} should contain a URI string.";
                            }
                        }

                        return (value, error);
                    },
                    ReturnType.String,
                    ValidateUnary),
                new ExpressionEvaluator(
                    ExpressionType.UriPort,
                    (expr, state, options) =>
                    {
                        object value = null;
                        string error = null;
                        IReadOnlyList<object> args;
                        (args, error) = EvaluateChildren(expr, state, options);
                        if (error == null)
                        {
                            if (args[0] is string uri)
                            {
                                (value, error) = UriPort(uri);
                            }
                            else
                            {
                                error = $"{expr} should contain a URI string.";
                            }
                        }

                        return (value, error);
                    },
                    ReturnType.Number,
                    ValidateUnary),
                new ExpressionEvaluator(
                    ExpressionType.UriQuery,
                    (expr, state, options) =>
                    {
                        object value = null;
                        string error = null;
                        IReadOnlyList<object> args;
                        (args, error) = EvaluateChildren(expr, state, options);
                        if (error == null)
                        {
                            if (args[0] is string uri)
                            {
                                (value, error) = UriQuery(uri);
                            }
                            else
                            {
                                error = $"{expr} should contain a URI string .";
                            }
                        }

                        return (value, error);
                    },
                    ReturnType.String,
                    ValidateUnary),
                new ExpressionEvaluator(
                    ExpressionType.UriScheme,
                    (expr, state, options) =>
                    {
                        object value = null;
                        string error = null;
                        IReadOnlyList<object> args;
                        (args, error) = EvaluateChildren(expr, state, options);
                        if (error == null)
                        {
                            if (args[0] is string uri)
                            {
                                (value, error) = UriScheme(uri);
                            }
                            else
                            {
                                error = $"{expr} should contain a URI string.";
                            }
                        }

                        return (value, error);
                    },
                    ReturnType.String,
                    ValidateUnary),

                // Conversions
                new ExpressionEvaluator(ExpressionType.Float, Apply(args => CultureInvariantDoubleConvert(args[0])), ReturnType.Number, ValidateUnary),
                new ExpressionEvaluator(ExpressionType.Int, Apply(args => Convert.ToInt64(args[0])), ReturnType.Number, ValidateUnary),
                new ExpressionEvaluator(ExpressionType.Binary, Apply(args => ToBinary(args[0].ToString()), VerifyString), ReturnType.String, ValidateUnary),
                new ExpressionEvaluator(
                    ExpressionType.Base64,
                    Apply(
                        (args) =>
                        {
                            byte[] byteArray;
                            if (args[0] is byte[] byteArr)
                            {
                                byteArray = byteArr;
                            }
                            else
                            {
                                byteArray = System.Text.Encoding.UTF8.GetBytes(args[0].ToString());
                            }

                            return Convert.ToBase64String(byteArray);
                        }),
                    ReturnType.String,
                    ValidateUnary),
                new ExpressionEvaluator(ExpressionType.Base64ToBinary, Apply(args => Convert.FromBase64String(args[0].ToString()), VerifyString), ReturnType.Object, ValidateUnary),
                new ExpressionEvaluator(ExpressionType.Base64ToString, Apply(args => System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(args[0].ToString())), VerifyString), ReturnType.String, ValidateUnary),
                new ExpressionEvaluator(ExpressionType.UriComponent, Apply(args => Uri.EscapeDataString(args[0].ToString()), VerifyString), ReturnType.String, ValidateUnary),
                new ExpressionEvaluator(ExpressionType.DataUri, Apply(args => "data:text/plain;charset=utf-8;base64," + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(args[0].ToString())), VerifyString), ReturnType.String, ValidateUnary),
                new ExpressionEvaluator(ExpressionType.DataUriToBinary, Apply(args => ToBinary(args[0].ToString()), VerifyString), ReturnType.String, ValidateUnary),
                new ExpressionEvaluator(ExpressionType.DataUriToString, Apply(args => System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(args[0].ToString().Substring(args[0].ToString().IndexOf(",") + 1))), VerifyString), ReturnType.String, ValidateUnary),
                new ExpressionEvaluator(ExpressionType.UriComponentToString, Apply(args => Uri.UnescapeDataString(args[0].ToString()), VerifyString), ReturnType.String, ValidateUnary),

                // TODO: Is this really the best way?
                new ExpressionEvaluator(ExpressionType.String, Apply(args => JsonConvert.SerializeObject(args[0]).TrimStart('"').TrimEnd('"')), ReturnType.String, ValidateUnary),
                Comparison(ExpressionType.Bool, args => IsLogicTrue(args[0]), ValidateUnary),
                new ExpressionEvaluator(ExpressionType.Xml, ApplyWithError(args => ToXml(args[0])), ReturnType.String, ValidateUnary),
                new ExpressionEvaluator(
                    ExpressionType.FormatNumber,
                    ApplyWithError(
                        args =>
                        {
                            string result = null;
                            string error = null;
                            if (!args[0].IsNumber())
                            {
                                error = $"formatNumber first argument {args[0]} must be number";
                            }
                            else if (!args[1].IsInteger())
                            {
                                error = $"formatNumber second argument {args[1]} must be number";
                            }
                            else if (args.Count == 3 && !(args[2] is string))
                            {
                                error = $"formatNumber third agument {args[2]} must be a locale";
                            }
                            else
                            {
                                try
                                {
                                    var number = Convert.ToDouble(args[0]);
                                    var precision = Convert.ToInt32(args[1]);
                                    var locale = args.Count == 3 ? new CultureInfo(args[2] as string) : CultureInfo.InvariantCulture;
                                    result = number.ToString("N" + precision.ToString(), locale);
                                }
                                catch
                                {
                                    error = $"{args[3]} is not a valid locale for formatNumber";
                                }
                            }

                            return (result, error);
                        }),
                    ReturnType.String,
                    (expr) => ValidateOrder(expr, new[] { ReturnType.String }, ReturnType.Number, ReturnType.Number)),

                // Misc
                new ExpressionEvaluator(ExpressionType.Accessor, Accessor, ReturnType.Object, ValidateAccessor),
                new ExpressionEvaluator(ExpressionType.GetProperty, GetProperty, ReturnType.Object, (expr) => ValidateOrder(expr, new[] { ReturnType.String }, ReturnType.Object)),
                new ExpressionEvaluator(ExpressionType.If, (expression, state, options) => If(expression, state, options), ReturnType.Object, (expression) => ValidateArityAndAnyType(expression, 3, 3)),
                new ExpressionEvaluator(
                    ExpressionType.Rand,
                    ApplyWithError(
                        args =>
                        {
                            object value = null;
                            string error = null;
                            var min = Convert.ToInt32(args[0]);
                            var max = Convert.ToInt32(args[1]);
                            if (min >= max)
                            {
                                error = $"{min} is not < {max} for rand";
                            }
                            else
                            {
                                lock (_randomizerLock)
                                {
                                    value = Randomizer.Next(min, max);
                                }
                            }

                            return (value, error);
                        },
                        VerifyInteger),
                    ReturnType.Number,
                    ValidateBinaryNumber),
                new ExpressionEvaluator(ExpressionType.CreateArray, Apply(args => new List<object>(args)), ReturnType.Array),
                new ExpressionEvaluator(
                    ExpressionType.First,
                    Apply(
                        args =>
                        {
                            object first = null;
                            if (args[0] is string string0 && string0.Length > 0)
                            {
                                first = string0.First().ToString();
                            }
                            else if (TryParseList(args[0], out IList list) && list.Count > 0)
                            {
                                first = AccessIndex(list, 0).value;
                            }

                            return first;
                        }),
                    ReturnType.Object,
                    ValidateUnary),
                new ExpressionEvaluator(
                    ExpressionType.Last,
                    Apply(
                        args =>
                        {
                            object last = null;
                            if (args[0] is string string0 && string0.Length > 0)
                            {
                                last = string0.Last().ToString();
                            }
                            else if (TryParseList(args[0], out IList list) && list.Count > 0)
                            {
                                last = AccessIndex(list, list.Count - 1).value;
                            }

                            return last;
                        }),
                    ReturnType.Object,
                    ValidateUnary),

                // Object manipulation and construction functions
                new ExpressionEvaluator(
                    ExpressionType.Json,
                    Apply(
                        args =>
                        {
                            using (var textReader = new StringReader(args[0].ToString()))
                            {
                                using (var jsonReader = new JsonTextReader(textReader) { DateParseHandling = DateParseHandling.None })
                                {
                                    return JToken.ReadFrom(jsonReader);
                                }
                            }
                        }),
                    ReturnType.Object,
                    (expr) => ValidateOrder(expr, null, ReturnType.String)),
                new ExpressionEvaluator(
                    ExpressionType.AddProperty,
                    ApplyWithError(args =>
                        {
                            var newJobj = (IDictionary<string, JToken>)args[0];
                            var prop = args[1].ToString();
                            string error = null;
                            if (newJobj.ContainsKey(prop))
                            {
                                error = $"{prop} already exists";
                            }
                            else
                            {
                                newJobj[prop] = ConvertToJToken(args[2]);
                            }

                            return (newJobj, error);
                        }),
                    ReturnType.Object,
                    (expr) => ValidateOrder(expr, null, ReturnType.Object, ReturnType.String, ReturnType.Object)),
                new ExpressionEvaluator(
                    ExpressionType.SetProperty,
                    Apply(args =>
                        {
                            var newJobj = (IDictionary<string, JToken>)args[0];
                            newJobj[args[1].ToString()] = ConvertToJToken(args[2]);
                            return newJobj;
                        }),
                    ReturnType.Object,
                    (expr) => ValidateOrder(expr, null, ReturnType.Object, ReturnType.String, ReturnType.Object)),
                new ExpressionEvaluator(
                    ExpressionType.RemoveProperty,
                    Apply(args =>
                        {
                            var newJobj = (JObject)args[0];
                            newJobj.Property(args[1].ToString()).Remove();
                            return newJobj;
                        }),
                    ReturnType.Object,
                    (expr) => ValidateOrder(expr, null, ReturnType.Object, ReturnType.String)),
                new ExpressionEvaluator(
                    ExpressionType.SetPathToValue,
                    SetPathToValue,
                    ReturnType.Object,
                    ValidateBinary),
                new ExpressionEvaluator(ExpressionType.Select, Foreach, ReturnType.Array, ValidateForeach),
                new ExpressionEvaluator(ExpressionType.Foreach, Foreach, ReturnType.Array, ValidateForeach),
                new ExpressionEvaluator(ExpressionType.Where, Where, ReturnType.Array, ValidateWhere),
                new ExpressionEvaluator(ExpressionType.Coalesce, Apply(args => Coalesce(args.ToArray())), ReturnType.Object, ValidateAtLeastOne),
                new ExpressionEvaluator(ExpressionType.XPath, ApplyWithError(args => XPath(args[0], args[1])), ReturnType.Object, (expr) => ValidateOrder(expr, null, ReturnType.Object, ReturnType.String)),
                new ExpressionEvaluator(ExpressionType.JPath, ApplyWithError(args => JPath(args[0], args[1].ToString())), ReturnType.Object, (expr) => ValidateOrder(expr, null, ReturnType.Object, ReturnType.String)),
                new ExpressionEvaluator(
                    ExpressionType.Merge,
                    ApplySequenceWithError(args =>
                                {
                                    object result = null;
                                    string error = null;
                                    if (args[0] is JObject && args[1] is JObject)
                                    {
                                        (args[0] as JObject).Merge(args[1] as JObject, new JsonMergeSettings
                                        {
                                            MergeArrayHandling = MergeArrayHandling.Replace
                                        });

                                        result = args[0];
                                    }
                                    else
                                    {
                                        error = $"The arguments {args[0]} and {args[1]} must be a JSON objects.";
                                    }

                                    return (result, error);
                                }), 
                    ReturnType.Object,
                    (expression) => ValidateArityAndAnyType(expression, 2, int.MaxValue)),

                // Regex expression
                new ExpressionEvaluator(
                    ExpressionType.IsMatch,
                    ApplyWithError(
                        args =>
                        {
                            var value = false;
                            string error = null;

                            string inputString = args[0]?.ToString();
                            if (string.IsNullOrEmpty(inputString))
                            {
                                value = false;
                                error = "regular expression is empty.";
                            }
                            else
                            {
                                var regex = CommonRegex.CreateRegex(args[1].ToString());
                                value = regex.IsMatch(inputString);
                            }

                            return (value, error);
                        }, VerifyStringOrNull),
                    ReturnType.Boolean,
                    ValidateIsMatch),

                //Type Checking Functions
                new ExpressionEvaluator(
                    ExpressionType.IsString,
                    Apply(args => args[0] != null && args[0].GetType() == typeof(string)),
                    ReturnType.Boolean,
                    ValidateUnary),
                new ExpressionEvaluator(
                    ExpressionType.IsInteger,
                    Apply(args => Extensions.IsNumber(args[0]) && CultureInvariantDoubleConvert(args[0]) % 1 == 0),
                    ReturnType.Boolean,
                    ValidateUnary),
                new ExpressionEvaluator(
                    ExpressionType.IsFloat,
                    Apply(args => Extensions.IsNumber(args[0]) && CultureInvariantDoubleConvert(args[0]) % 1 != 0),
                    ReturnType.Boolean,
                    ValidateUnary),
                new ExpressionEvaluator(
                    ExpressionType.IsArray,
                    Apply(args => TryParseList(args[0], out IList _)),
                    ReturnType.Boolean,
                    ValidateUnary),
                new ExpressionEvaluator(
                    ExpressionType.IsObject,
                    Apply(args => args[0] != null && !(args[0] is JValue) && args[0].GetType().IsValueType == false && args[0].GetType() != typeof(string)),
                    ReturnType.Boolean,
                    ValidateUnary),
                new ExpressionEvaluator(
                    ExpressionType.IsBoolean,
                    Apply(args => args[0] is bool),
                    ReturnType.Boolean,
                    ValidateUnary),
                new ExpressionEvaluator(
                    ExpressionType.IsDateTime,
                    Apply(
                        args =>
                        {
                            object value = null;
                            string error = null;
                            (value, error) = NormalizeToDateTime(args[0]);
                            if (error == null)
                            {
                                return true;
                            }

                            return false;
                        }),
                    ReturnType.Boolean,
                    ValidateUnary),
            };

            var eval = new ExpressionEvaluator(ExpressionType.Optional, (expression, state, options) => throw new NotImplementedException(), ReturnType.Boolean, ValidateUnaryBoolean);
            eval.Negation = eval;
            functions.Add(eval);

            eval = new ExpressionEvaluator(ExpressionType.Ignore, (expression, state, options) => expression.Children[0].TryEvaluate(state, options), ReturnType.Boolean, ValidateUnaryBoolean);
            eval.Negation = eval;
            functions.Add(eval);

            var lookup = new Dictionary<string, ExpressionEvaluator>();
            foreach (var function in functions)
            {
                lookup.Add(function.Type, function);
            }

            // Attach negations
            lookup[ExpressionType.LessThan].Negation = lookup[ExpressionType.GreaterThanOrEqual];
            lookup[ExpressionType.LessThanOrEqual].Negation = lookup[ExpressionType.GreaterThan];
            lookup[ExpressionType.Equal].Negation = lookup[ExpressionType.NotEqual];

            // Math aliases
            lookup.Add("add", lookup[ExpressionType.Add]); // more than 1 params
            lookup.Add("div", lookup[ExpressionType.Divide]); // more than 1 params
            lookup.Add("mul", lookup[ExpressionType.Multiply]); // more than 1 params
            lookup.Add("sub", lookup[ExpressionType.Subtract]); // more than 1 params
            lookup.Add("exp", lookup[ExpressionType.Power]); // more than 1 params
            lookup.Add("mod", lookup[ExpressionType.Mod]);

            // Comparison aliases
            lookup.Add("and", lookup[ExpressionType.And]);
            lookup.Add("equals", lookup[ExpressionType.Equal]);
            lookup.Add("greater", lookup[ExpressionType.GreaterThan]);
            lookup.Add("greaterOrEquals", lookup[ExpressionType.GreaterThanOrEqual]);
            lookup.Add("less", lookup[ExpressionType.LessThan]);
            lookup.Add("lessOrEquals", lookup[ExpressionType.LessThanOrEqual]);
            lookup.Add("not", lookup[ExpressionType.Not]);
            lookup.Add("or", lookup[ExpressionType.Or]);

            lookup.Add("&", lookup[ExpressionType.Concat]);
            return new ReadOnlyDictionary<string, ExpressionEvaluator>(lookup);
        }
    }
}
