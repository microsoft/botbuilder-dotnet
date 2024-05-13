// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Collections.ObjectModel;

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
        /// <summary>
        /// Read only Dictionary of built in functions.
        /// </summary>
        public static readonly IDictionary<string, ExpressionEvaluator> StandardFunctions = GetStandardFunctions();

        private static IDictionary<string, ExpressionEvaluator> GetStandardFunctions()
        {
            var functions = new List<ExpressionEvaluator>
            {
                new BuiltinFunctions.Abs(),
                new BuiltinFunctions.Accessor(),
                new BuiltinFunctions.Add(),
                new BuiltinFunctions.AddDays(),
                new BuiltinFunctions.AddHours(),
                new BuiltinFunctions.AddMinutes(),
                new BuiltinFunctions.AddOrdinal(),
                new BuiltinFunctions.AddProperty(),
                new BuiltinFunctions.AddSeconds(),
                new BuiltinFunctions.AddToTime(),
                new BuiltinFunctions.All(),
                new BuiltinFunctions.And(),
                new BuiltinFunctions.Any(),
                new BuiltinFunctions.Average(),
                new BuiltinFunctions.Base64(),
                new BuiltinFunctions.Base64ToBinary(),
                new BuiltinFunctions.Base64ToString(),
                new BuiltinFunctions.Binary(),
                new BuiltinFunctions.Bool(),
                new BuiltinFunctions.Ceiling(),
                new BuiltinFunctions.Coalesce(),
                new BuiltinFunctions.Concat(),
                new BuiltinFunctions.Contains(),
                new BuiltinFunctions.ConvertFromUtc(),
                new BuiltinFunctions.ConvertToUtc(),
                new BuiltinFunctions.Count(),
                new BuiltinFunctions.CountWord(),
                new BuiltinFunctions.CreateArray(),
                new BuiltinFunctions.DataUri(),
                new BuiltinFunctions.DataUriToBinary(),
                new BuiltinFunctions.DataUriToString(),
                new BuiltinFunctions.Date(),
                new BuiltinFunctions.DateReadBack(),
                new BuiltinFunctions.DateTimeDiff(),
                new BuiltinFunctions.DayOfMonth(),
                new BuiltinFunctions.DayOfWeek(),
                new BuiltinFunctions.DayOfYear(),
                new BuiltinFunctions.Divide(),
                new BuiltinFunctions.Element(),
                new BuiltinFunctions.Empty(),
                new BuiltinFunctions.EndsWith(),
                new BuiltinFunctions.EOL(),
                new BuiltinFunctions.Equal(),
                new BuiltinFunctions.Exists(),
                new BuiltinFunctions.First(),
                new BuiltinFunctions.Flatten(),
                new BuiltinFunctions.Float(),
                new BuiltinFunctions.Floor(),
                new BuiltinFunctions.Foreach(),
                new BuiltinFunctions.FormatDateTime(),
                new BuiltinFunctions.FormatEpoch(),
                new BuiltinFunctions.FormatNumber(),
                new BuiltinFunctions.FormatTicks(),
                new BuiltinFunctions.GetFutureTime(),
                new BuiltinFunctions.GetNextViableDate(),
                new BuiltinFunctions.GetNextViableTime(),
                new BuiltinFunctions.GetPastTime(),
                new BuiltinFunctions.GetPreviousViableDate(),
                new BuiltinFunctions.GetPreviousViableTime(),
                new BuiltinFunctions.GetProperty(),
                new BuiltinFunctions.GetTimeOfDay(),
                new BuiltinFunctions.GreaterThan(),
                new BuiltinFunctions.GreaterThanOrEqual(),
                new BuiltinFunctions.If(),
                new BuiltinFunctions.Ignore(),
                new BuiltinFunctions.IndexOf(),
                new BuiltinFunctions.IndicesAndValues(),
                new BuiltinFunctions.Int(),
                new BuiltinFunctions.Intersection(),
                new BuiltinFunctions.IsArray(),
                new BuiltinFunctions.IsBoolean(),
                new BuiltinFunctions.IsDate(),
                new BuiltinFunctions.IsDateRange(),
                new BuiltinFunctions.IsDateTime(),
                new BuiltinFunctions.IsDefinite(),
                new BuiltinFunctions.IsDuration(),
                new BuiltinFunctions.IsFloat(),
                new BuiltinFunctions.IsInteger(),
                new BuiltinFunctions.IsMatch(),
                new BuiltinFunctions.IsObject(),
                new BuiltinFunctions.IsPresent(),
                new BuiltinFunctions.IsString(),
                new BuiltinFunctions.IsTime(),
                new BuiltinFunctions.IsTimeRange(),
                new BuiltinFunctions.Join(),
                new BuiltinFunctions.JPath(),
                new BuiltinFunctions.Json(),
                new BuiltinFunctions.JsonStringify(),
                new BuiltinFunctions.Last(),
                new BuiltinFunctions.LastIndexOf(),
                new BuiltinFunctions.Length(),
                new BuiltinFunctions.LessThan(),
                new BuiltinFunctions.LessThanOrEqual(),
                new BuiltinFunctions.Max(),
                new BuiltinFunctions.Merge(),
                new BuiltinFunctions.Min(),
                new BuiltinFunctions.Mod(),
                new BuiltinFunctions.Month(),
                new BuiltinFunctions.Multiply(),
                new BuiltinFunctions.NewGuid(),
                new BuiltinFunctions.Not(),
                new BuiltinFunctions.NotEqual(),
                new BuiltinFunctions.Optional(),
                new BuiltinFunctions.Or(),
                new BuiltinFunctions.Power(),
                new BuiltinFunctions.Rand(),
                new BuiltinFunctions.Range(),
                new BuiltinFunctions.RemoveProperty(),
                new BuiltinFunctions.Replace(),
                new BuiltinFunctions.ReplaceIgnoreCase(),
                new BuiltinFunctions.Reverse(),
                new BuiltinFunctions.Round(),
                new BuiltinFunctions.Select(),
                new BuiltinFunctions.SentenceCase(),
                new BuiltinFunctions.SetPathToValue(),
                new BuiltinFunctions.SetProperty(),
                new BuiltinFunctions.Skip(),
                new BuiltinFunctions.SortBy(),
                new BuiltinFunctions.SortByDescending(),
                new BuiltinFunctions.Split(),
                new BuiltinFunctions.Sqrt(),
                new BuiltinFunctions.StartOfDay(),
                new BuiltinFunctions.StartOfHour(),
                new BuiltinFunctions.StartOfMonth(),
                new BuiltinFunctions.StartsWith(),
                new BuiltinFunctions.String(),
                new BuiltinFunctions.StringOrValue(),
                new BuiltinFunctions.SubArray(),
                new BuiltinFunctions.Substring(),
                new BuiltinFunctions.Subtract(),
                new BuiltinFunctions.SubtractFromTime(),
                new BuiltinFunctions.Sum(),
                new BuiltinFunctions.Take(),
                new BuiltinFunctions.Ticks(),
                new BuiltinFunctions.TicksToDays(),
                new BuiltinFunctions.TicksToHours(),
                new BuiltinFunctions.TicksToMinutes(),
                new BuiltinFunctions.TimexResolve(),
                new BuiltinFunctions.TitleCase(),
                new BuiltinFunctions.ToLower(),
                new BuiltinFunctions.ToUpper(),
                new BuiltinFunctions.Trim(),
                new BuiltinFunctions.Union(),
                new BuiltinFunctions.Unique(),
                new BuiltinFunctions.UriComponent(),
                new BuiltinFunctions.UriComponentToString(),
                new BuiltinFunctions.UriHost(),
                new BuiltinFunctions.UriPath(),
                new BuiltinFunctions.UriPathAndQuery(),
                new BuiltinFunctions.UriPort(),
                new BuiltinFunctions.UriQuery(),
                new BuiltinFunctions.UriScheme(),
                new BuiltinFunctions.UtcNow(),
                new BuiltinFunctions.Where(),
                new BuiltinFunctions.Xml(),
                new BuiltinFunctions.XPath(),
                new BuiltinFunctions.Year(),
            };

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
            lookup.Add("??", lookup[ExpressionType.Coalesce]);

            return new ReadOnlyDictionary<string, ExpressionEvaluator>(lookup);
        }
    }
}
