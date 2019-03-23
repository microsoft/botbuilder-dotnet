using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Expressions
{

    public static class BuiltInFunctions
    {
        public static (object value, string error) Apply(
            Func<IReadOnlyList<dynamic>, object> function,
            IReadOnlyList<Expression> parameters,
            Func<object, Expression, string> valid,
            IReadOnlyDictionary<string, object> state)
        {
            object value = null;
            string error = null;
            var args = new List<dynamic>();
            foreach (var child in parameters)
            {
                (value, error) = child.TryEvaluate(state);
                if (error != null)
                {
                    break;
                }
                error = valid(value, child);
                if (error != null)
                {
                    break;
                }
                args.Add(value);
            }
            if (error == null)
            {
                value = function(args);
            }
            return (value, error);
        }

        public static string VerifyNumber(object value, Expression expression)
        {
            string error = null;
            if (!value.IsNumber())
            {
                error = $"{expression} is not a number.";
            }
            return error;
        }

        public static string VerifyPlus(object value, Expression expression)
        {
            string error = null;
            if (!value.IsNumber() && !(value is string))
            {
                error = $"{expression} is not string or number.";
            }
            return error;
        }

        public static bool IsNumber(this object value)
        {
            return value is sbyte
                    || value is byte
                    || value is short
                    || value is ushort
                    || value is int
                    || value is uint
                    || value is long
                    || value is ulong
                    || value is float
                    || value is double
                    || value is decimal;
        }

        public static ExpressionEvaluator Add =
            (parameters, state) => Apply((args) => args[0] + args[1], parameters, VerifyPlus, state);

        /*
        public static ExpressionEvaluator Sub =
            operands =>
                Task.FromResult<dynamic>(operands[0] - operands[1]);

        public static ExpressionEvaluator Mul = operands =>
                Task.FromResult(
                    operands[0] is double double0 && operands[1] is double double1 ? double0 * double1 :
                    operands[0] is int int0 && operands[1] is int int1 ? (object)(int0 * int1) :
                    throw new ExpressionException(ExpressionType.Multiply, operands));

        public static ExpressionEvaluator Div = operands =>
                Task.FromResult(
                    operands[0] is double double0 && operands[1] is double double1 ? double0 / double1 :
                    operands[0] is int int0 && operands[1] is int int1 ? (object)(int0 / int1) :
                    throw new ExpressionException(ExpressionType.Divide, operands));

        public static ExpressionEvaluator Equal = operands =>
                Task.FromResult<object>(
                    operands[0] is IComparable operand0 && operands[1] is IComparable operand1 ?
                    operand0.CompareTo(operand1) == 0 : operands[0] == null && operands[1] == null ?
                    true : (operands[0] == null || operands[1] == null) ?
                        false :
                        throw new ExpressionException(ExpressionType.Equal, operands));

        public static ExpressionEvaluator NotEqual = operands =>
                Task.FromResult<object>(
                    operands[0] is IComparable operand0 && operands[1] is IComparable operand1 ?
                    operand0.CompareTo(operand1) != 0 :
                    throw new ExpressionException(ExpressionType.NotEqual, operands));

        public static ExpressionEvaluator Min = operands =>
                Task.FromResult<object>(
                    operands[0] is IComparable c0 && operands[1] is IComparable c1 ? (c0.CompareTo(c1) < 0 ? c0 : c1) :
                    throw new ExpressionException(ExpressionType.Min, operands));

        public static ExpressionEvaluator Max = operands =>
                Task.FromResult<object>(
                    operands[0] is IComparable c0 && operands[1] is IComparable c1 ? (c0.CompareTo(c1) > 0 ? c0 : c1) :
                    throw new ExpressionException(ExpressionType.Max, operands));

        public static ExpressionEvaluator LessThan = operands =>
                Task.FromResult<object>(
                    operands[0] is IComparable operand0 && operands[1] is IComparable operand1 ?
                    operand0.CompareTo(operand1) < 0 :
                    throw new ExpressionException(ExpressionType.LessThan, operands));

        public static ExpressionEvaluator LessThanOrEqual = operands =>
                Task.FromResult<object>(
                    operands[0] is IComparable operand0 && operands[1] is IComparable operand1 ?
                    operand0.CompareTo(operand1) <= 0 :
                    throw new ExpressionException(ExpressionType.LessThanOrEqual, operands));

        public static ExpressionEvaluator GreaterThan = operands =>
                Task.FromResult<object>(
                    operands[0] is IComparable operand0 && operands[1] is IComparable operand1 ?
                    operand0.CompareTo(operand1) > 0 :
                    throw new ExpressionException(ExpressionType.GreaterThan, operands));

        public static ExpressionEvaluator GreaterThanOrEqual = operands =>
                    Task.FromResult<object>(
                        operands[0] is IComparable operand0 && operands[1] is IComparable operand1 ?
                        operand0.CompareTo(operand1) >= 0 :
                        throw new ExpressionException(ExpressionType.GreaterThanOrEqual, operands));
*/
        // TODO: Logical expressions

        private static readonly Dictionary<string, ExpressionEvaluator> BinaryFunctions = new Dictionary<string, ExpressionEvaluator>
        {
            {"+", BuiltInFunctions.Add}
            /* ,
            {"/", BuiltInFunctions.Div},
            {"*", BuiltInFunctions.Mul},
            {"-", BuiltInFunctions.Sub},
            {"==", BuiltInFunctions.Equal},
            {"!=", BuiltInFunctions.NotEqual},
            {"max", BuiltInFunctions.Max},
            {"min", BuiltInFunctions.Min},
            {"<", BuiltInFunctions.LessThan},
            {"<=", BuiltInFunctions.LessThanOrEqual},
            {">", BuiltInFunctions.GreaterThan},
            {">=", BuiltInFunctions.GreaterThanOrEqual}, */
        };

        private static readonly Dictionary<string, ExpressionEvaluator> UnaryFunctions = new Dictionary<string, ExpressionEvaluator>
        {
            // TODO: Unary functions
        };

        private static readonly Dictionary<string, ExpressionEvaluator> NAryFunctions = new Dictionary<string, ExpressionEvaluator>
        {
            // TODO: NAry functions if any
        };

        public static ExpressionEvaluator GetUnaryEvaluator(string type)
        {
            if (!UnaryFunctions.TryGetValue(type, out ExpressionEvaluator eval))
            {
                throw new Exception($"{type} does not have a built-in unary evaluator.");
            }
            return eval;
        }

        public static ExpressionEvaluator GetBinaryEvaluator(string type)
        {
            if (!BinaryFunctions.TryGetValue(type, out ExpressionEvaluator eval))
            {
                throw new Exception($"{type} does not have a built-in binary evaluator.");
            }
            return eval;
        }

        public static ExpressionEvaluator GetNAryEvaluator(string type)
        {
            if (!NAryFunctions.TryGetValue(type, out ExpressionEvaluator eval))
            {
                throw new Exception($"{type} does not have a built-in nary evaluator.");
            }
            return eval;
        }
    }
}
