using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Globalization;
using Newtonsoft.Json;
using System.Collections;
using Newtonsoft.Json.Linq;

namespace Microsoft.Expressions
{

    public static class BuildinFunctions
    {

        /// <summary>
        /// The default date time format string.
        /// </summary>
        private static readonly string DefaultDateTimeFormat = "o";

        public static EvaluationDelegate Add = operands =>
                operands[0] is double double0 && operands[1] is double double1 ? double0 + double1 :
                operands[0] is int int0 && operands[1] is int int1 ? (object)(int0 + int1) :
                operands[0] is string string0 && operands[1] is string string1 ? string0 + string1 :
                throw new ExpressionPropertyMissingException();

        public static EvaluationDelegate Sub = operands =>
                operands[0] is double double0 && operands[1] is double double1 ? double0 - double1 :
                operands[0] is int int0 && operands[1] is int int1 ? (object)(int0 - int1) :
                throw new ExpressionPropertyMissingException();

        public static EvaluationDelegate Mul = operands =>
                operands[0] is double double0 && operands[1] is double double1 ? double0 * double1 :
                operands[0] is int int0 && operands[1] is int int1 ? (object)(int0 * int1) :
                throw new ExpressionPropertyMissingException();


        public static EvaluationDelegate Div = operands =>
                operands[0] is double double0 && operands[1] is double double1 ? double0 / double1 :
                operands[0] is int int0 && operands[1] is int int1 ? (object)(int0 / int1) :
                throw new ExpressionPropertyMissingException();

        public static EvaluationDelegate Equal = operands =>
                operands[0] is IComparable operand0 && operands[1] is IComparable operand1 ?
                operand0.CompareTo(operand1) == 0 : operands[0] == null && operands[1] == null ?
                true : (operands[0] == null || operands[1] == null) ?
                false : throw new ExpressionPropertyMissingException();

        public static EvaluationDelegate NotEqual = operands =>
                operands[0] is IComparable operand0 && operands[1] is IComparable operand1 ?
                operand0.CompareTo(operand1) != 0 : throw new ExpressionPropertyMissingException();

        public static EvaluationDelegate Min = parameters =>
                         parameters[0] is IComparable c0 && parameters[1] is IComparable c1 ? (c0.CompareTo(c1) < 0 ? c0 : c1) :
                         throw new ExpressionPropertyMissingException();

        public static EvaluationDelegate Max = parameters =>
                         parameters[0] is IComparable c0 && parameters[1] is IComparable c1 ? (c0.CompareTo(c1) > 0 ? c0 : c1) :
                         throw new ExpressionPropertyMissingException();

        public static EvaluationDelegate LessThan = operands =>
                        operands[0] is IComparable operand0 && operands[1] is IComparable operand1 ?
                        operand0.CompareTo(operand1) < 0 : throw new ExpressionPropertyMissingException();

        public static EvaluationDelegate LessThanOrEqual = operands =>
                        operands[0] is IComparable operand0 && operands[1] is IComparable operand1 ?
                        operand0.CompareTo(operand1) <= 0 : throw new ExpressionPropertyMissingException();

        public static EvaluationDelegate GreaterThan = operands =>
                        operands[0] is IComparable operand0 && operands[1] is IComparable operand1 ?
                        operand0.CompareTo(operand1) > 0 : throw new ExpressionPropertyMissingException();

        public static EvaluationDelegate GreaterThanOrEqual = operands =>
                        operands[0] is IComparable operand0 && operands[1] is IComparable operand1 ?
                        operand0.CompareTo(operand1) >= 0 : throw new ExpressionPropertyMissingException();

        public static EvaluationDelegate Pow = operands =>
                        operands[0] is double double0 && operands[1] is double double1 ? Math.Pow(double0, double1) :
                        operands[0] is int int0 && operands[1] is int int1 ? (object)(int)Math.Pow(int0, int1) :
                        throw new ExpressionPropertyMissingException();

        public static EvaluationDelegate And = operands =>
                        operands[0] is bool bool0 && operands[1] is bool bool1 ? bool0 && bool1 : throw new ExpressionPropertyMissingException();

        public static EvaluationDelegate Or = operands =>
                        operands[0] is bool bool0 && operands[1] is bool bool1 ? bool0 || bool1 : throw new ExpressionPropertyMissingException();

        public static EvaluationDelegate Not = operands =>
                        operands[0] is bool bool0 ? !bool0 :
                        operands[0] is int int0 ? int0 == 0 : operands[0] == null;

        public static EvaluationDelegate Exist = operands =>
                        operands[0] != null;

        public static EvaluationDelegate Mod = operands =>
                        operands[0] is int int0 && operands[1] is int int1 ? int0 % int1 :
                        throw new ExpressionPropertyMissingException();

        public static EvaluationDelegate Concat = operands =>
        {
            var stringBuilder = new StringBuilder();
            foreach (var item in operands)
            {
                if (item is string str)
                {
                    stringBuilder.Append(str);
                }
                else throw new ExpressionPropertyMissingException();
            }
            return stringBuilder.ToString();
        };

        public static EvaluationDelegate Length = operands =>
                        operands[0] is string string0 ? string0.Length :
                        throw new ExpressionPropertyMissingException();

        public static EvaluationDelegate Replace = operands =>
                       operands[0] is string string0 && operands[1] is string string1 && operands[2] is string string2 ?
                       string0.Replace(string1, string2) : throw new ExpressionPropertyMissingException();

        public static EvaluationDelegate ReplaceIgnoreCase = operands =>
                       operands[0] is string string0 && operands[1] is string string1 && operands[2] is string string2 ?
                       Regex.Replace(string0, string1, string2, RegexOptions.IgnoreCase) : throw new ExpressionPropertyMissingException();

        public static EvaluationDelegate Split = operands =>
                      operands[0] is string string0 && operands[1] is string string1 && string1.Length == 1 ?
                      string0.Split(string1.ToCharArray()[0]) : throw new ExpressionPropertyMissingException();

        public static EvaluationDelegate SubString = operands =>
        {
            if (operands[0] is string string0 && operands[1] is int int0 && operands[2] is int int1)
            {
                if (int0 < 0 || int0 >= string0.Length)
                {
                    throw new ExpressionPropertyMissingException();
                }
                if (int1 >= string0.Length)
                {
                    return string0.Substring(int0);
                }
                return string0.Substring(int0, int1 - int0);
            }
            else throw new ExpressionPropertyMissingException();
        };

        public static EvaluationDelegate ToLower = operands =>
                       operands[0] is string string0 ? string0.ToLower() :
                       throw new ExpressionPropertyMissingException();

        public static EvaluationDelegate ToUpper = operands =>
                       operands[0] is string string0 ? string0.ToUpper() :
                       throw new ExpressionPropertyMissingException();

        public static EvaluationDelegate Trim = operands =>
                       operands[0] is string string0 ? string0.Trim() :
                       throw new ExpressionPropertyMissingException();

        public static EvaluationDelegate If = operands =>
                       operands[0] is bool bool0 ? (bool0 ? operands[1] : operands[2]) :
                       throw new ExpressionPropertyMissingException();

        public static EvaluationDelegate Rand = operands =>
                        operands[0] is int int0 && operands[1] is int int1 ? new Random().Next(int0, int1) :
                        throw new ExpressionPropertyMissingException();

        public static EvaluationDelegate Sum = operands =>
        {
            if (operands.All(u => (u is int)))
            {
                return operands.Sum(u => (int)u);
            }

            if (operands.All(u => ((u is int) || (u is double))))
            {
                return operands.Sum(u => Convert.ToDouble(u));
            }

            throw new ExpressionPropertyMissingException();
        };

        public static EvaluationDelegate Average = operands =>
                        operands.All(u => ((u is int) || (u is double))) ? operands.Average(u => Convert.ToDouble(u)) :
                        throw new ExpressionPropertyMissingException();

        //Date and time functions

        public static EvaluationDelegate AddDays = operands =>
        {
            if (operands[0] is string string0 && operands[1] is int int0)
            {
                var formatString = operands.Count == 3 && operands[2] is string string1
                    ? string1 : DefaultDateTimeFormat;

                var timestamp = ParseTimestamp(string0);
                return timestamp.AddDays(int0).ToString(formatString);
            }

            throw new ExpressionPropertyMissingException();
        };

        public static EvaluationDelegate AddHours = operands =>
        {
            if (operands[0] is string string0 && operands[1] is int int0)
            {
                var formatString = operands.Count == 3 && operands[2] is string string1
                    ? string1 : DefaultDateTimeFormat;

                var timestamp = ParseTimestamp(string0);
                return timestamp.AddHours(int0).ToString(formatString);
            }

            throw new ExpressionPropertyMissingException();
        };

        public static EvaluationDelegate AddMinutes = operands =>
        {
            if (operands[0] is string string0 && operands[1] is int int0)
            {
                var formatString = operands.Count == 3 && operands[2] is string string1
                    ? string1 : DefaultDateTimeFormat;

                var timestamp = ParseTimestamp(string0);
                return timestamp.AddMinutes(int0).ToString(formatString);
            }

            throw new ExpressionPropertyMissingException();
        };

        public static EvaluationDelegate AddSeconds = operands =>
        {
            if (operands[0] is string string0 && operands[1] is int int0)
            {
                var formatString = operands.Count == 3 && operands[2] is string string1
                    ? string1 : DefaultDateTimeFormat;

                var timestamp = ParseTimestamp(string0);
                return timestamp.AddSeconds(int0).ToString(formatString);
            }

            throw new ExpressionPropertyMissingException();
        };

        public static EvaluationDelegate DayOfMonth = operands =>
                      operands[0] is string string0 ? ParseTimestamp(string0).Day :
                      throw new ExpressionPropertyMissingException();

        public static EvaluationDelegate DayOfWeek = operands =>
                     operands[0] is string string0 ? (int)(ParseTimestamp(string0).DayOfWeek) :
                      throw new ExpressionPropertyMissingException();

        public static EvaluationDelegate DayOfYear = operands =>
                      operands[0] is string string0 ? ParseTimestamp(string0).DayOfYear :
                      throw new ExpressionPropertyMissingException();

        public static EvaluationDelegate Month = operands =>
                      operands[0] is string string0 ? ParseTimestamp(string0).Month :
                      throw new ExpressionPropertyMissingException();

        public static EvaluationDelegate Date = operands =>
                      operands[0] is string string0 ? ParseTimestamp(string0).Date.ToString("d") :
                      throw new ExpressionPropertyMissingException();

        public static EvaluationDelegate Year = operands =>
                       operands[0] is string string0 ? ParseTimestamp(string0).Year :
                      throw new ExpressionPropertyMissingException();

        public static EvaluationDelegate UtcNow = operands =>
        {
            var formatString = operands.Count == 1 && operands[0] is string string0
                    ? string0 : DefaultDateTimeFormat;

            return DateTime.UtcNow.ToString(formatString);
        };

        public static EvaluationDelegate FormatDateTime = operands =>
        {
            if (operands[0] is string string0)
            {
                var formatString = operands.Count == 2 && operands[1] is string string1
                    ? string1 : DefaultDateTimeFormat;

                var timestamp = ParseTimestamp(string0);
                return timestamp.ToString(formatString);
            }

            throw new ExpressionPropertyMissingException();
        };

        public static EvaluationDelegate SubtractFromTime = operands =>
        {
            if (operands[0] is string string0 &&
                operands[1] is int int0 &&
                operands[2] is string string1)
            {
                var timeSpan = GetTimeSpan(int0, string1);
                var formatString = operands.Count == 4 && operands[3] is string string2
                    ? string2 : DefaultDateTimeFormat;

                var timestamp = ParseTimestamp(string0);
                return timestamp.Subtract(timeSpan).ToString(formatString);
            }

            throw new ExpressionPropertyMissingException();
        };

        public static EvaluationDelegate DateReadBack = operands =>
        {
            if (operands[0] is string string0 &&
                operands[1] is string string1)
            {
                var timestamp1 = ParseTimestamp(string0).Date;
                var timestamp2 = ParseTimestamp(string1).Date;
                if (IsSameDay(timestamp1, timestamp2))
                {
                    return "Today";
                }
                if (IsSameDay(timestamp1.AddDays(1), timestamp2))
                {
                    return "Tomorrow";
                }
                else if (IsSameDay(timestamp1.AddDays(2), timestamp2))
                {
                    return "The day after tomorrow";
                }
                else if (IsSameDay(timestamp1.AddDays(-1), timestamp2))
                {
                    return "Yesterday";
                }
                else if (IsSameDay(timestamp1.AddDays(-2), timestamp2))
                {
                    return "The day before yesterday";
                }
            }

            throw new ExpressionPropertyMissingException();
        };

        public static EvaluationDelegate GetTimeOfDay = operands =>
        {
            if (operands[0] is string string0)
            {
                var timestamp = ParseTimestamp(string0);
                if (timestamp.Hour == 0 && timestamp.Minute == 0)
                    return "midnight";
                if (timestamp.Hour >= 0 && timestamp.Hour < 12)
                    return "morning";
                if (timestamp.Hour == 12 && timestamp.Minute == 0)
                    return "noon";
                if (timestamp.Hour < 18)
                    return "afternoon";
                if (timestamp.Hour < 22 || (timestamp.Hour == 22 && timestamp.Minute == 0))
                    return "evening";
                return "night";
            }

            throw new ExpressionPropertyMissingException();
        };

        public static EvaluationDelegate ConvertToFloat = operands =>
                       operands[0] is string string0 ? (float)Convert.ToDouble(string0) :
                      throw new ExpressionPropertyMissingException();

        public static EvaluationDelegate ConvertToInt = operands =>
                      operands[0] is string string0 ? Convert.ToInt32(string0) :
                     throw new ExpressionPropertyMissingException();

        public static EvaluationDelegate ConvertToString = operands =>
                    operands[0] is string string0? string0:
                    JsonConvert.SerializeObject(operands[0]);

        public static EvaluationDelegate ConvertToBool = operands => Convert.ToBoolean(operands[0]);

        public static EvaluationDelegate CreateArray = operands =>
                      new List<object>(operands) { };

        public static EvaluationDelegate CheckContains = operands =>
        {
            //string to find subString
            if (operands[0] is string string0 &&
                operands[1] is string string1)
            {
                if (string0.Contains(string1))
                    return true;
            }
            //list to find a value
            else if (operands[0] is IList list1)
            {
                if (list1.Contains(operands[1]))
                    return true;
            }
            //Dictionary contains key
            else if (operands[0] is IDictionary dict && operands[1] is string string2)
            {
                if (dict is Dictionary<string, object> realdict
                    && realdict.ContainsKey(string2))
                    return true;
            }
            else if(operands[1] is string string3)
            {
                var propInfo = operands[0].GetType().GetProperty(string3);
                if (propInfo != null)
                {
                    return true;
                }
            }
            return false;
        };


        public static EvaluationDelegate CheckEmpty = operands =>
        {
            if (operands[0] == null)
                return true;

            if (operands[0] is string string0)
                return string.IsNullOrEmpty(string0);

            if (operands[0] is IList list)
                return list.Count == 0;

            return operands[0].GetType().GetProperties().Length == 0;
        };

        public static EvaluationDelegate First = operands =>
        {
            if (operands[0] is string string0 && string0.Length > 0)
                return string0.First().ToString();

            if (operands[0] is IList list && list.Count > 0)
                return list[0];

            throw new ExpressionPropertyMissingException();
        };

        public static EvaluationDelegate Join = operands =>
                     operands[0] is IList list0 && operands[1] is string string0 ? string.Join(string0, list0.OfType<object>().Select(x => x.ToString())) :
                     throw new ExpressionPropertyMissingException();

        public static EvaluationDelegate Last = operands =>
        {
            if (operands[0] is string string0 && string0.Length > 0)
                return string0.Last().ToString();

            if (operands[0] is IList list && list.Count > 0)
                return list[list.Count - 1];

            throw new ExpressionPropertyMissingException();
        };

       


        public static EvaluationDelegate Parameters = operands =>
        {
            if (operands[0] is string string0 && string0.Length > 0)
                return string0.Last().ToString();

            if (operands[0] is IList list && list.Count > 0)
                return list[list.Count - 1];

            throw new ExpressionPropertyMissingException();
        };


        private static TimeSpan GetTimeSpan(long interval, string timeUnit)
        {
            switch (timeUnit)
            {
                //TODO support week and year
                case "Second":
                    return TimeSpan.FromSeconds(interval);
                case "Minute":
                    return TimeSpan.FromMinutes(interval);
                case "Hour":
                    return TimeSpan.FromHours(interval);
                case "Day":
                    return TimeSpan.FromDays(interval);
                case "Week":
                    return TimeSpan.FromDays(interval * 7);
                default:
                    throw new ExpressionPropertyMissingException();
            }
        }

        private static DateTime ParseTimestamp(string timeStamp)
        {
            if (!DateTime.TryParse(
              s: timeStamp,
              provider: CultureInfo.InvariantCulture,
              styles: DateTimeStyles.RoundtripKind,
              result: out var parsedTimestamp))
            {
                throw new ExpressionPropertyMissingException();
            }

            return parsedTimestamp;
        }

        private static bool IsSameDay(DateTime date1, DateTime date2)
        {
            return date1.Year == date2.Year && date1.Month == date2.Month && date1.Day == date2.Day;
        }
    }
}
