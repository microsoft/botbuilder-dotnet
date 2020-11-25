// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
#pragma once

#include "ExpressionType.h"




// Math
const std::string ExpressionType::Add = "+";
const std::string ExpressionType::Subtract = "-";
const std::string ExpressionType::Multiply = "*";
const std::string ExpressionType::Divide = "/";
const std::string ExpressionType::Min = "min";
const std::string ExpressionType::Max = "max";
const std::string ExpressionType::Power = "^";
const std::string ExpressionType::Mod = "%";
const std::string ExpressionType::Average = "average";
const std::string ExpressionType::Sum = "sum";
const std::string ExpressionType::Range = "range";
const std::string ExpressionType::Floor = "floor";
const std::string ExpressionType::Ceiling = "ceiling";
const std::string ExpressionType::Round = "round";

// Comparisons
const std::string ExpressionType::LessThan = "<";
const std::string ExpressionType::LessThanOrEqual = "<=";
const std::string ExpressionType::Equal = "==";
const std::string ExpressionType::NotEqual = "!=";
const std::string ExpressionType::GreaterThan = ">";
const std::string ExpressionType::GreaterThanOrEqual = ">=";
const std::string ExpressionType::Exists = "exists";

// Logic
const std::string ExpressionType::And = "&&";
const std::string ExpressionType::Or = "||";
const std::string ExpressionType::Not = "!";

// String
const std::string ExpressionType::Concat = "concat";
const std::string ExpressionType::Length = "length";
const std::string ExpressionType::Replace = "replace";
const std::string ExpressionType::ReplaceIgnoreCase = "replaceIgnoreCase";
const std::string ExpressionType::Split = "split";
const std::string ExpressionType::Substring = "substring";
const std::string ExpressionType::ToLower = "toLower";
const std::string ExpressionType::ToUpper = "toUpper";
const std::string ExpressionType::Trim = "trim";
const std::string ExpressionType::EndsWith = "endsWith";
const std::string ExpressionType::StartsWith = "startsWith";
const std::string ExpressionType::CountWord = "countWord";
const std::string ExpressionType::AddOrdinal = "addOrdinal";
const std::string ExpressionType::NewGuid = "newGuid";
const std::string ExpressionType::IndexOf = "indexOf";
const std::string ExpressionType::LastIndexOf = "lastIndexOf";
const std::string ExpressionType::EOL = "EOL";
const std::string ExpressionType::SentenceCase = "sentenceCase";
const std::string ExpressionType::TitleCase = "titleCase";

// Collection
const std::string ExpressionType::Count = "count";
const std::string ExpressionType::Contains = "contains";
const std::string ExpressionType::Empty = "empty";
const std::string ExpressionType::Join = "join";
const std::string ExpressionType::First = "first";
const std::string ExpressionType::Last = "last";
const std::string ExpressionType::Foreach = "foreach";
const std::string ExpressionType::Select = "select";
const std::string ExpressionType::Where = "where";
const std::string ExpressionType::Union = "union";
const std::string ExpressionType::Intersection = "intersection";
const std::string ExpressionType::Skip = "skip";
const std::string ExpressionType::Take = "take";
const std::string ExpressionType::SubArray = "subArray";
const std::string ExpressionType::SortBy = "sortBy";
const std::string ExpressionType::SortByDescending = "sortByDescending";
const std::string ExpressionType::IndicesAndValues = "indicesAndValues";
const std::string ExpressionType::Flatten = "flatten";
const std::string ExpressionType::Unique = "unique";

// DateTime
const std::string ExpressionType::AddDays = "addDays";
const std::string ExpressionType::AddHours = "addHours";
const std::string ExpressionType::AddMinutes = "addMinutes";
const std::string ExpressionType::AddSeconds = "addSeconds";
const std::string ExpressionType::DayOfMonth = "dayOfMonth";
const std::string ExpressionType::DayOfWeek = "dayOfWeek";
const std::string ExpressionType::DayOfYear = "dayOfYear";
const std::string ExpressionType::Month = "month";
const std::string ExpressionType::Date = "date";
const std::string ExpressionType::Year = "year";
const std::string ExpressionType::UtcNow = "utcNow";
const std::string ExpressionType::FormatDateTime = "formatDateTime";
const std::string ExpressionType::FormatEpoch = "formatEpoch";
const std::string ExpressionType::FormatTicks = "formatTicks";
const std::string ExpressionType::SubtractFromTime = "subtractFromTime";
const std::string ExpressionType::DateReadBack = "dateReadBack";
const std::string ExpressionType::GetTimeOfDay = "getTimeOfDay";
const std::string ExpressionType::GetFutureTime = "getFutureTime";
const std::string ExpressionType::GetPastTime = "getPastTime";
const std::string ExpressionType::ConvertFromUtc = "convertFromUTC";
const std::string ExpressionType::ConvertToUtc = "convertToUTC";
const std::string ExpressionType::AddToTime = "addToTime";
const std::string ExpressionType::StartOfDay = "startOfDay";
const std::string ExpressionType::StartOfHour = "startOfHour";
const std::string ExpressionType::StartOfMonth = "startOfMonth";
const std::string ExpressionType::Ticks = "ticks";
const std::string ExpressionType::TicksToDays = "ticksToDays";
const std::string ExpressionType::TicksToHours = "ticksToHours";
const std::string ExpressionType::TicksToMinutes = "ticksToMinutes";
const std::string ExpressionType::DateTimeDiff = "dateTimeDiff";

// Timex 
const std::string ExpressionType::IsDefinite = "isDefinite";
const std::string ExpressionType::IsTime = "isTime";
const std::string ExpressionType::IsDuration = "isDuration";
const std::string ExpressionType::IsDate = "isDate";
const std::string ExpressionType::IsTimeRange = "isTimeRange";
const std::string ExpressionType::IsDateRange = "isDateRange";
const std::string ExpressionType::IsPresent = "isPresent";
const std::string ExpressionType::GetNextViableDate = "getNextViableDate";
const std::string ExpressionType::GetPreviousViableDate = "getPreviousViableDate";
const std::string ExpressionType::GetNextViableTime = "getNextViableTime";
const std::string ExpressionType::GetPreviousViableTime = "getPreviousViableTime";

// Conversions
#pragma warning disable CA1720 // Identifier contains type name (by design and can't change this because of backward compat)
const std::string ExpressionType::Float = "float";
#pragma warning restore CA1720 // Identifier contains type name
#pragma warning disable CA1720 // Identifier contains type name (by design and can't change this because of backward compat)
const std::string ExpressionType::Int = "int";
#pragma warning restore CA1720 // Identifier contains type name
#pragma warning disable CA1720 // Identifier contains type name (by design and can't change this because of backward compat)
const std::string ExpressionType::String = "string";
#pragma warning restore CA1720 // Identifier contains type name
const std::string ExpressionType::JsonStringify = "jsonStringify";
const std::string ExpressionType::Bool = "bool";
const std::string ExpressionType::Binary = "binary";
const std::string ExpressionType::Base64 = "base64";
const std::string ExpressionType::Base64ToBinary = "base64ToBinary";
const std::string ExpressionType::Base64ToString = "base64ToString";
const std::string ExpressionType::DataUri = "dataUri";
const std::string ExpressionType::DataUriToBinary = "dataUriToBinary";
const std::string ExpressionType::DataUriToString = "dataUriToString";
const std::string ExpressionType::UriComponent = "uriComponent";
const std::string ExpressionType::UriComponentToString = "uriComponentToString";
const std::string ExpressionType::Xml = "xml";
const std::string ExpressionType::FormatNumber = "formatNumber";

// URI Parsing Functions
const std::string ExpressionType::UriHost = "uriHost";
const std::string ExpressionType::UriPath = "uriPath";
const std::string ExpressionType::UriPathAndQuery = "uriPathAndQuery";
const std::string ExpressionType::UriPort = "uriPort";
const std::string ExpressionType::UriQuery = "uriQuery";
const std::string ExpressionType::UriScheme = "uriScheme";

// Memory
const std::string ExpressionType::Accessor = "Accessor";
const std::string ExpressionType::Element = "Element";
const std::string ExpressionType::CreateArray = "createArray";

// Misc
const std::string ExpressionType::Constant = "Constant";
const std::string ExpressionType::Lambda = "Lambda";
const std::string ExpressionType::If = "if";
const std::string ExpressionType::Rand = "rand";

// Object manipulation and construction functions
const std::string ExpressionType::Json = "json";
const std::string ExpressionType::GetProperty = "getProperty";
const std::string ExpressionType::AddProperty = "addProperty";
const std::string ExpressionType::RemoveProperty = "removeProperty";
const std::string ExpressionType::SetProperty = "setProperty";
const std::string ExpressionType::Coalesce = "coalesce";
const std::string ExpressionType::XPath = "xPath";
const std::string ExpressionType::SetPathToValue = "setPathToValue";
const std::string ExpressionType::JPath = "jPath";
const std::string ExpressionType::Merge = "merge";

// Regular expression
const std::string ExpressionType::IsMatch = "isMatch";

//Type Checking
const std::string ExpressionType::IsInteger = "isInteger";
const std::string ExpressionType::IsFloat = "isFloat";
const std::string ExpressionType::IsString = "isString";
const std::string ExpressionType::IsArray = "isArray";
const std::string ExpressionType::IsObject = "isObject";
const std::string ExpressionType::IsBoolean = "isBoolean";
const std::string ExpressionType::IsDateTime = "isDateTime";

// trigger tree 

/// <summary>
/// Mark a sub-expression as optional.
/// </summary>
/// <remarks>
/// When an expression is being processed, optional creates a disjunction where the expression is both included and not
/// included with the rest of the expression.  This is a simple way to express this common relationship.  By generating
/// both clauses then matching the expression can be more specific when the optional expression is true.
/// </remarks>
const std::string ExpressionType::Optional = "optional";

/// <summary>
/// Any predicate expression wrapped in this will be ignored for specialization.
/// </summary>
/// <remarks>
/// This is useful for when you need to add expression to the trigger that are part of rule mechanics rather than of intent.
/// For example, if you have a counter for how often a particular message is displayed, then that is part of the triggering condition, 
/// but all such rules would be incomparable because they counter is per-rule. 
/// </remarks>
const std::string ExpressionType::Ignore = "ignore";
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
