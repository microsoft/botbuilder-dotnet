﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.AdaptiveExpressions.Core
{
    /// <summary>
    /// Built-in expression types.
    /// </summary>
    public static class ExpressionType
    {
        // Math
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public const string Add = "+";
        public const string Subtract = "-";
        public const string Multiply = "*";
        public const string Divide = "/";
        public const string Min = "min";
        public const string Max = "max";
        public const string Power = "^";
        public const string Mod = "%";
        public const string Average = "average";
        public const string Sum = "sum";
        public const string Range = "range";
        public const string Floor = "floor";
        public const string Ceiling = "ceiling";
        public const string Round = "round";
        public const string Abs = "abs";
        public const string Sqrt = "sqrt";

        // Comparisons
        public const string LessThan = "<";
        public const string LessThanOrEqual = "<=";
        public const string Equal = "==";
        public const string NotEqual = "!=";
        public const string GreaterThan = ">";
        public const string GreaterThanOrEqual = ">=";
        public const string Exists = "exists";

        // Logic
        public const string And = "&&";
        public const string Or = "||";
        public const string Not = "!";

        // String
        public const string Concat = "concat";
        public const string Length = "length";
        public const string Replace = "replace";
        public const string ReplaceIgnoreCase = "replaceIgnoreCase";
        public const string Split = "split";
        public const string Substring = "substring";
        public const string ToLower = "toLower";
        public const string ToUpper = "toUpper";
        public const string Trim = "trim";
        public const string EndsWith = "endsWith";
        public const string StartsWith = "startsWith";
        public const string CountWord = "countWord";
        public const string AddOrdinal = "addOrdinal";
        public const string NewGuid = "newGuid";
        public const string IndexOf = "indexOf";
        public const string LastIndexOf = "lastIndexOf";
        public const string EOL = "EOL";
        public const string SentenceCase = "sentenceCase";
        public const string TitleCase = "titleCase";

        // Collection
        public const string Count = "count";
        public const string Contains = "contains";
        public const string Empty = "empty";
        public const string Join = "join";
        public const string First = "first";
        public const string Last = "last";
        public const string Foreach = "foreach";
        public const string Select = "select";
        public const string Where = "where";
        public const string Union = "union";
        public const string Intersection = "intersection";
        public const string Skip = "skip";
        public const string Take = "take";
        public const string SubArray = "subArray";
        public const string SortBy = "sortBy";
        public const string SortByDescending = "sortByDescending";
        public const string IndicesAndValues = "indicesAndValues";
        public const string Flatten = "flatten";
        public const string Unique = "unique";
        public const string Reverse = "reverse";
        public const string Any = "any";
        public const string All = "all";

        // DateTime
        public const string AddDays = "addDays";
        public const string AddHours = "addHours";
        public const string AddMinutes = "addMinutes";
        public const string AddSeconds = "addSeconds";
        public const string DayOfMonth = "dayOfMonth";
        public const string DayOfWeek = "dayOfWeek";
        public const string DayOfYear = "dayOfYear";
        public const string Month = "month";
        public const string Date = "date";
        public const string Year = "year";
        public const string UtcNow = "utcNow";
        public const string FormatDateTime = "formatDateTime";
        public const string FormatEpoch = "formatEpoch";
        public const string FormatTicks = "formatTicks";
        public const string SubtractFromTime = "subtractFromTime";
        public const string DateReadBack = "dateReadBack";
        public const string GetTimeOfDay = "getTimeOfDay";
        public const string GetFutureTime = "getFutureTime";
        public const string GetPastTime = "getPastTime";
        public const string ConvertFromUtc = "convertFromUTC";
        public const string ConvertToUtc = "convertToUTC";
        public const string AddToTime = "addToTime";
        public const string StartOfDay = "startOfDay";
        public const string StartOfHour = "startOfHour";
        public const string StartOfMonth = "startOfMonth";
        public const string Ticks = "ticks";
        public const string TicksToDays = "ticksToDays";
        public const string TicksToHours = "ticksToHours";
        public const string TicksToMinutes = "ticksToMinutes";
        public const string DateTimeDiff = "dateTimeDiff";

        // Timex 
        public const string IsDefinite = "isDefinite";
        public const string IsTime = "isTime";
        public const string IsDuration = "isDuration";
        public const string IsDate = "isDate";
        public const string IsTimeRange = "isTimeRange";
        public const string IsDateRange = "isDateRange";
        public const string IsPresent = "isPresent";
        public const string GetNextViableDate = "getNextViableDate";
        public const string GetPreviousViableDate = "getPreviousViableDate";
        public const string GetNextViableTime = "getNextViableTime";
        public const string GetPreviousViableTime = "getPreviousViableTime";
        public const string TimexResolve = "resolve";

        // Conversions
#pragma warning disable CA1720 // Identifier contains type name (by design and can't change this because of backward compat)
        public const string Float = "float";
#pragma warning restore CA1720 // Identifier contains type name
#pragma warning disable CA1720 // Identifier contains type name (by design and can't change this because of backward compat)
        public const string Int = "int";
#pragma warning restore CA1720 // Identifier contains type name
#pragma warning disable CA1720 // Identifier contains type name (by design and can't change this because of backward compat)
        public const string String = "string";
#pragma warning restore CA1720 // Identifier contains type name
        public const string JsonStringify = "jsonStringify";
        public const string Bool = "bool";
        public const string Binary = "binary";
        public const string Base64 = "base64";
        public const string Base64ToBinary = "base64ToBinary";
        public const string Base64ToString = "base64ToString";
        public const string DataUri = "dataUri";
        public const string DataUriToBinary = "dataUriToBinary";
        public const string DataUriToString = "dataUriToString";
        public const string UriComponent = "uriComponent";
        public const string UriComponentToString = "uriComponentToString";
        public const string Xml = "xml";
        public const string FormatNumber = "formatNumber";

        // URI Parsing Functions
        public const string UriHost = "uriHost";
        public const string UriPath = "uriPath";
        public const string UriPathAndQuery = "uriPathAndQuery";
        public const string UriPort = "uriPort";
        public const string UriQuery = "uriQuery";
        public const string UriScheme = "uriScheme";

        // Memory
        public const string Accessor = "Accessor";
        public const string Element = "Element";
        public const string CreateArray = "createArray";

        // Misc
        public const string Constant = "Constant";
        public const string Lambda = "Lambda";
        public const string If = "if";
        public const string Rand = "rand";

        // Object manipulation and construction functions
        public const string Json = "json";
        public const string GetProperty = "getProperty";
        public const string AddProperty = "addProperty";
        public const string RemoveProperty = "removeProperty";
        public const string SetProperty = "setProperty";
        public const string Coalesce = "coalesce";
        public const string XPath = "xPath";
        public const string SetPathToValue = "setPathToValue";
        public const string JPath = "jPath";
        public const string Merge = "merge";

        // Regular expression
        public const string IsMatch = "isMatch";

        //Type Checking
        public const string IsInteger = "isInteger";
        public const string IsFloat = "isFloat";
        public const string IsString = "isString";
        public const string IsArray = "isArray";
        public const string IsObject = "isObject";
        public const string IsBoolean = "isBoolean";
        public const string IsDateTime = "isDateTime";

        // StringOrValue
        public const string StringOrValue = "stringOrValue";

        // trigger tree 

        /// <summary>
        /// Mark a sub-expression as optional.
        /// </summary>
        /// <remarks>
        /// When an expression is being processed, optional creates a disjunction where the expression is both included and not
        /// included with the rest of the expression.  This is a simple way to express this common relationship.  By generating
        /// both clauses then matching the expression can be more specific when the optional expression is true.
        /// </remarks>
        public const string Optional = "optional";

        /// <summary>
        /// Any predicate expression wrapped in this will be ignored for specialization.
        /// </summary>
        /// <remarks>
        /// This is useful for when you need to add expression to the trigger that are part of rule mechanics rather than of intent.
        /// For example, if you have a counter for how often a particular message is displayed, then that is part of the triggering condition, 
        /// but all such rules would be incomparable because they counter is per-rule. 
        /// </remarks>
        public const string Ignore = "ignore";
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
