// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
#pragma once

#include <string>

 class ExpressionType
{
public:

    // Math
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
     static const std::string Add;
     static const std::string Subtract;
     static const std::string Multiply;
     static const std::string Divide;
     static const std::string Min;
     static const std::string Max;
     static const std::string Power;
     static const std::string Mod;
     static const std::string Average;
     static const std::string Sum;
     static const std::string Range;
     static const std::string Floor;
     static const std::string Ceiling;
     static const std::string Round;

    // Comparisons
     static const std::string LessThan;
     static const std::string LessThanOrEqual;
     static const std::string Equal;
     static const std::string NotEqual;
     static const std::string GreaterThan;
     static const std::string GreaterThanOrEqual;
     static const std::string Exists;

    // Logic
     static const std::string And;
     static const std::string Or;
     static const std::string Not;

    // String
     static const std::string Concat;
     static const std::string Length;
     static const std::string Replace;
     static const std::string ReplaceIgnoreCase;
     static const std::string Split;
     static const std::string Substring;
     static const std::string ToLower;
     static const std::string ToUpper;
     static const std::string Trim;
     static const std::string EndsWith;
     static const std::string StartsWith;
     static const std::string CountWord;
     static const std::string AddOrdinal;
     static const std::string NewGuid;
     static const std::string IndexOf;
     static const std::string LastIndexOf;
     static const std::string EOL;
     static const std::string SentenceCase;
     static const std::string TitleCase;

    // Collection
     static const std::string Count;
     static const std::string Contains;
     static const std::string Empty;
     static const std::string Join;
     static const std::string First;
     static const std::string Last;
     static const std::string Foreach;
     static const std::string Select;
     static const std::string Where;
     static const std::string Union;
     static const std::string Intersection;
     static const std::string Skip;
     static const std::string Take;
     static const std::string SubArray;
     static const std::string SortBy;
     static const std::string SortByDescending;
     static const std::string IndicesAndValues;
     static const std::string Flatten;
     static const std::string Unique;

    // DateTime
     static const std::string AddDays;
     static const std::string AddHours;
     static const std::string AddMinutes;
     static const std::string AddSeconds;
     static const std::string DayOfMonth;
     static const std::string DayOfWeek;
     static const std::string DayOfYear;
     static const std::string Month;
     static const std::string Date;
     static const std::string Year;
     static const std::string UtcNow;
     static const std::string FormatDateTime;
     static const std::string FormatEpoch;
     static const std::string FormatTicks;
     static const std::string SubtractFromTime;
     static const std::string DateReadBack;
     static const std::string GetTimeOfDay;
     static const std::string GetFutureTime;
     static const std::string GetPastTime;
     static const std::string ConvertFromUtc;
     static const std::string ConvertToUtc;
     static const std::string AddToTime;
     static const std::string StartOfDay;
     static const std::string StartOfHour;
     static const std::string StartOfMonth;
     static const std::string Ticks;
     static const std::string TicksToDays;
     static const std::string TicksToHours;
     static const std::string TicksToMinutes;
     static const std::string DateTimeDiff;

    // Timex 
     static const std::string IsDefinite;
     static const std::string IsTime;
     static const std::string IsDuration;
     static const std::string IsDate;
     static const std::string IsTimeRange;
     static const std::string IsDateRange;
     static const std::string IsPresent;
     static const std::string GetNextViableDate;
     static const std::string GetPreviousViableDate;
     static const std::string GetNextViableTime;
     static const std::string GetPreviousViableTime;

    // Conversions
#pragma warning disable CA1720 // Identifier contains type name (by design and can't change this because of backward compat)
     static const std::string Float;
#pragma warning restore CA1720 // Identifier contains type name
#pragma warning disable CA1720 // Identifier contains type name (by design and can't change this because of backward compat)
     static const std::string Int;
#pragma warning restore CA1720 // Identifier contains type name
#pragma warning disable CA1720 // Identifier contains type name (by design and can't change this because of backward compat)
     static const std::string String;
#pragma warning restore CA1720 // Identifier contains type name
     static const std::string JsonStringify;
     static const std::string Bool;
     static const std::string Binary;
     static const std::string Base64;
     static const std::string Base64ToBinary;
     static const std::string Base64ToString;
     static const std::string DataUri;
     static const std::string DataUriToBinary;
     static const std::string DataUriToString;
     static const std::string UriComponent;
     static const std::string UriComponentToString;
     static const std::string Xml;
     static const std::string FormatNumber;

    // URI Parsing Functions
     static const std::string UriHost;
     static const std::string UriPath;
     static const std::string UriPathAndQuery;
     static const std::string UriPort;
     static const std::string UriQuery;
     static const std::string UriScheme;

    // Memory
     static const std::string Accessor;
     static const std::string Element;
     static const std::string CreateArray;

    // Misc
     static const std::string Constant;
     static const std::string Lambda;
     static const std::string If;
     static const std::string Rand;

    // Object manipulation and construction functions
     static const std::string Json;
     static const std::string GetProperty;
     static const std::string AddProperty;
     static const std::string RemoveProperty;
     static const std::string SetProperty;
     static const std::string Coalesce;
     static const std::string XPath;
     static const std::string SetPathToValue;
     static const std::string JPath;
     static const std::string Merge;

    // Regular expression
     static const std::string IsMatch;

    //Type Checking
     static const std::string IsInteger;
     static const std::string IsFloat;
     static const std::string IsString;
     static const std::string IsArray;
     static const std::string IsObject;
     static const std::string IsBoolean;
     static const std::string IsDateTime;

    // trigger tree 

    /// <summary>
    /// Mark a sub-expression as optional.
    /// </summary>
    /// <remarks>
    /// When an expression is being processed, optional creates a disjunction where the expression is both included and not
    /// included with the rest of the expression.  This is a simple way to express this common relationship.  By generating
    /// both clauses then matching the expression can be more specific when the optional expression is true.
    /// </remarks>
     static const std::string Optional;

    /// <summary>
    /// Any predicate expression wrapped in this will be ignored for specialization.
    /// </summary>
    /// <remarks>
    /// This is useful for when you need to add expression to the trigger that are part of rule mechanics rather than of intent.
    /// For example, if you have a counter for how often a particular message is displayed, then that is part of the triggering condition, 
    /// but all such rules would be incomparable because they counter is per-rule. 
    /// </remarks>
     static const std::string Ignore;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
};