// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Expressions
{
    /// NOTE: If you add a built-in type here, you also need to make sure it can be evaluated as well either by adding to BuiltInFunctions or when you construct the expression.
    /// <summary>
    /// Built-in expression types.
    /// </summary>
    public class ExpressionType
    {
        // Math
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
        public const string Concat = "&";
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
        public const string SubtractFromTime = "subtractFromTime";
        public const string DateReadBack = "dateReadBack";
        public const string GetTimeOfDay = "getTimeOfDay";
        public const string GetFutureTime = "getFutureTime";
        public const string GetPastTime = "getPastTime";
        public const string ConvertFromUTC = "convertFromUTC";
        public const string ConvertToUTC = "convertToUTC";
        public const string AddToTime = "addToTime";
        public const string StartOfDay = "startOfDay";
        public const string StartOfHour = "startOfHour";
        public const string StartOfMonth = "startOfMonth";
        public const string Ticks = "ticks";

        // Conversions
        public const string Float = "float";
        public const string Int = "int";
        public const string String = "string";
        public const string Bool = "bool";
        public const string Array = "array";
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

        // Regular expression
        public const string IsMatch = "isMatch";

        // short hand functions
        public const string SimpleEntity = "simpleEntity";
        public const string CallstackScope = "callstackScope";
    }
}
