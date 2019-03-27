using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;

namespace Microsoft.Expressions
{

    public static class BuildinFunctions
    {
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
                        operands[0] is int int0 && operands[1] is int int1 ? int0%int1:
                        throw new ExpressionPropertyMissingException();

        public static EvaluationDelegate Concat = (IReadOnlyList<object> operands) =>
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
                        operands[0] is string string0 ? string0.Length:
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

        public static EvaluationDelegate SubString = (IReadOnlyList<object> operands) =>
        {
            if(operands[0] is string string0 && operands[1] is int int0 && operands[2] is int int1)
            {
                if(int0 < 0 || int0 >= string0.Length)
                {
                    throw new ExpressionPropertyMissingException();
                }
                if(int1 >= string0.Length)
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
                       operands[0] is bool bool0 ? (bool0 ? operands[1]:operands[2]):
                       throw new ExpressionPropertyMissingException();

        public static EvaluationDelegate Rand = operands =>
                        operands[0] is int int0 && operands[1] is int int1 ? new Random().Next(int0,int1) :
                        throw new ExpressionPropertyMissingException();

        public static EvaluationDelegate Sum = (IReadOnlyList<object> operands) =>
        {
            if(operands.All(u=>(u is int)))
            {
                return operands.Sum(u => (int)u);
            }

            if(operands.All(u => ((u is int) || (u is double))))
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
                       operands[0] is DateTime dateTime0 && operands[1] is int int0 ? dateTime0.AddDays(int0) :
                       throw new ExpressionPropertyMissingException();

        public static EvaluationDelegate AddHours = operands =>
                      operands[0] is DateTime dateTime0 && operands[1] is int int0 ? dateTime0.AddHours(int0) :
                      throw new ExpressionPropertyMissingException();

        public static EvaluationDelegate AddMinutes = operands =>
                      operands[0] is DateTime dateTime0 && operands[1] is int int0 ? dateTime0.AddMinutes(int0) :
                      throw new ExpressionPropertyMissingException();

        public static EvaluationDelegate AddSeconds = operands =>
                      operands[0] is DateTime dateTime0 && operands[1] is int int0 ? dateTime0.AddSeconds(int0) :
                      throw new ExpressionPropertyMissingException();

        public static EvaluationDelegate DayOfMonth = operands =>
                      operands[0] is DateTime dateTime0? dateTime0.Day :
                      throw new ExpressionPropertyMissingException();

        public static EvaluationDelegate DayOfWeek = operands =>
                      operands[0] is DateTime dateTime0 ? (int)dateTime0.DayOfWeek :
                      throw new ExpressionPropertyMissingException();

        public static EvaluationDelegate DayOfYear = operands =>
                      operands[0] is DateTime dateTime0 ? dateTime0.DayOfYear :
                      throw new ExpressionPropertyMissingException();

        public static EvaluationDelegate Month = operands =>
                      operands[0] is DateTime dateTime0 ? dateTime0.Month:
                      throw new ExpressionPropertyMissingException();

        public static EvaluationDelegate Date = operands =>
                      operands[0] is DateTime dateTime0 ? dateTime0.Date :
                      throw new ExpressionPropertyMissingException();

        public static EvaluationDelegate Year = operands =>
                      operands[0] is DateTime dateTime0 ? dateTime0.Year :
                      throw new ExpressionPropertyMissingException();

        public static EvaluationDelegate UtcNow = (operands) =>
                      DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}
