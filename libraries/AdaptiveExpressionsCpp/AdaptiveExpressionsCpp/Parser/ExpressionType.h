// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
#pragma once

#include <string>

static class ExpressionType
{
public:

    // Math
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    static const std::string Add;
    static const std::string Subtract;
    const std::string Multiply = "*";
    const std::string Divide = "/";
    const std::string Min = "min";
    const std::string Max = "max";
    const std::string Power = "^";
    const std::string Mod = "%";
    const std::string Average = "average";
    const std::string Sum = "sum";
    const std::string Range = "range";
    const std::string Floor = "floor";
    const std::string Ceiling = "ceiling";
    const std::string Round = "round";

    // Comparisons
    const std::string LessThan = "<";
    const std::string LessThanOrEqual = "<=";
    const std::string Equal = "==";
    const std::string NotEqual = "!=";
    const std::string GreaterThan = ">";
    const std::string GreaterThanOrEqual = ">=";
    const std::string Exists = "exists";

    // Logic
    const std::string And = "&&";
    const std::string Or = "||";
    const std::string Not = "!";

    // String
    const std::string Concat = "concat";
    const std::string Length = "length";
    const std::string Replace = "replace";
    const std::string ReplaceIgnoreCase = "replaceIgnoreCase";
    const std::string Split = "split";
    const std::string Substring = "substring";
    const std::string ToLower = "toLower";
    const std::string ToUpper = "toUpper";
    const std::string Trim = "trim";
    const std::string EndsWith = "endsWith";
    const std::string StartsWith = "startsWith";
    const std::string CountWord = "countWord";
    const std::string AddOrdinal = "addOrdinal";
    const std::string NewGuid = "newGuid";
    const std::string IndexOf = "indexOf";
    const std::string LastIndexOf = "lastIndexOf";
    const std::string EOL = "EOL";
    const std::string SentenceCase = "sentenceCase";
    const std::string TitleCase = "titleCase";

    // Collection
    const std::string Count = "count";
    const std::string Contains = "contains";
    const std::string Empty = "empty";
    const std::string Join = "join";
    const std::string First = "first";
    const std::string Last = "last";
    const std::string Foreach = "foreach";
    const std::string Select = "select";
    const std::string Where = "where";
    const std::string Union = "union";
    const std::string Intersection = "intersection";
    const std::string Skip = "skip";
    const std::string Take = "take";
    const std::string SubArray = "subArray";
    const std::string SortBy = "sortBy";
    const std::string SortByDescending = "sortByDescending";
    const std::string IndicesAndValues = "indicesAndValues";
    const std::string Flatten = "flatten";
    const std::string Unique = "unique";

    // DateTime
    const std::string AddDays = "addDays";
    const std::string AddHours = "addHours";
    const std::string AddMinutes = "addMinutes";
    const std::string AddSeconds = "addSeconds";
    const std::string DayOfMonth = "dayOfMonth";
    const std::string DayOfWeek = "dayOfWeek";
    const std::string DayOfYear = "dayOfYear";
    const std::string Month = "month";
    const std::string Date = "date";
    const std::string Year = "year";
    const std::string UtcNow = "utcNow";
    const std::string FormatDateTime = "formatDateTime";
    const std::string FormatEpoch = "formatEpoch";
    const std::string FormatTicks = "formatTicks";
    const std::string SubtractFromTime = "subtractFromTime";
    const std::string DateReadBack = "dateReadBack";
    const std::string GetTimeOfDay = "getTimeOfDay";
    const std::string GetFutureTime = "getFutureTime";
    const std::string GetPastTime = "getPastTime";
    const std::string ConvertFromUtc = "convertFromUTC";
    const std::string ConvertToUtc = "convertToUTC";
    const std::string AddToTime = "addToTime";
    const std::string StartOfDay = "startOfDay";
    const std::string StartOfHour = "startOfHour";
    const std::string StartOfMonth = "startOfMonth";
    const std::string Ticks = "ticks";
    const std::string TicksToDays = "ticksToDays";
    const std::string TicksToHours = "ticksToHours";
    const std::string TicksToMinutes = "ticksToMinutes";
    const std::string DateTimeDiff = "dateTimeDiff";

    // Timex 
    const std::string IsDefinite = "isDefinite";
    const std::string IsTime = "isTime";
    const std::string IsDuration = "isDuration";
    const std::string IsDate = "isDate";
    const std::string IsTimeRange = "isTimeRange";
    const std::string IsDateRange = "isDateRange";
    const std::string IsPresent = "isPresent";
    const std::string GetNextViableDate = "getNextViableDate";
    const std::string GetPreviousViableDate = "getPreviousViableDate";
    const std::string GetNextViableTime = "getNextViableTime";
    const std::string GetPreviousViableTime = "getPreviousViableTime";

    // Conversions
#pragma warning disable CA1720 // Identifier contains type name (by design and can't change this because of backward compat)
    const std::string Float = "float";
#pragma warning restore CA1720 // Identifier contains type name
#pragma warning disable CA1720 // Identifier contains type name (by design and can't change this because of backward compat)
    const std::string Int = "int";
#pragma warning restore CA1720 // Identifier contains type name
#pragma warning disable CA1720 // Identifier contains type name (by design and can't change this because of backward compat)
    const std::string String = "string";
#pragma warning restore CA1720 // Identifier contains type name
    const std::string JsonStringify = "jsonStringify";
    const std::string Bool = "bool";
    const std::string Binary = "binary";
    const std::string Base64 = "base64";
    const std::string Base64ToBinary = "base64ToBinary";
    const std::string Base64ToString = "base64ToString";
    const std::string DataUri = "dataUri";
    const std::string DataUriToBinary = "dataUriToBinary";
    const std::string DataUriToString = "dataUriToString";
    const std::string UriComponent = "uriComponent";
    const std::string UriComponentToString = "uriComponentToString";
    const std::string Xml = "xml";
    const std::string FormatNumber = "formatNumber";

    // URI Parsing Functions
    const std::string UriHost = "uriHost";
    const std::string UriPath = "uriPath";
    const std::string UriPathAndQuery = "uriPathAndQuery";
    const std::string UriPort = "uriPort";
    const std::string UriQuery = "uriQuery";
    const std::string UriScheme = "uriScheme";

    // Memory
    const std::string Accessor = "Accessor";
    const std::string Element = "Element";
    const std::string CreateArray = "createArray";

    // Misc
    const std::string Constant = "Constant";
    const std::string Lambda = "Lambda";
    const std::string If = "if";
    const std::string Rand = "rand";

    // Object manipulation and construction functions
    const std::string Json = "json";
    const std::string GetProperty = "getProperty";
    const std::string AddProperty = "addProperty";
    const std::string RemoveProperty = "removeProperty";
    const std::string SetProperty = "setProperty";
    const std::string Coalesce = "coalesce";
    const std::string XPath = "xPath";
    const std::string SetPathToValue = "setPathToValue";
    const std::string JPath = "jPath";
    const std::string Merge = "merge";

    // Regular expression
    const std::string IsMatch = "isMatch";

    //Type Checking
    const std::string IsInteger = "isInteger";
    const std::string IsFloat = "isFloat";
    const std::string IsString = "isString";
    const std::string IsArray = "isArray";
    const std::string IsObject = "isObject";
    const std::string IsBoolean = "isBoolean";
    const std::string IsDateTime = "isDateTime";

    // trigger tree 

    /// <summary>
    /// Mark a sub-expression as optional.
    /// </summary>
    /// <remarks>
    /// When an expression is being processed, optional creates a disjunction where the expression is both included and not
    /// included with the rest of the expression.  This is a simple way to express this common relationship.  By generating
    /// both clauses then matching the expression can be more specific when the optional expression is true.
    /// </remarks>
    const std::string Optional = "optional";

    /// <summary>
    /// Any predicate expression wrapped in this will be ignored for specialization.
    /// </summary>
    /// <remarks>
    /// This is useful for when you need to add expression to the trigger that are part of rule mechanics rather than of intent.
    /// For example, if you have a counter for how often a particular message is displayed, then that is part of the triggering condition, 
    /// but all such rules would be incomparable because they counter is per-rule. 
    /// </remarks>
    const std::string Ignore = "ignore";
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
};