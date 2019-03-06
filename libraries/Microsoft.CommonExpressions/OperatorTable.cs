using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Expressions
{
    /// <summary>
    /// Table of operators in language.
    /// </summary>
    public static class OperatorTable
    {
        public static OperatorEntry Prefix(string input, int power, EvaluationDelegate evaluator) =>
            OperatorEntry.From(input, power, BindingDirection.Left, 1, 1, evaluator);

        public static OperatorEntry Infix(string input, int power, EvaluationDelegate evaluator) =>
            OperatorEntry.From(input, power, BindingDirection.Left, 2, 2, evaluator);

        public static OperatorEntry InfixFree(string input, int power, EvaluationDelegate evaluator) =>
            OperatorEntry.From(input, power, BindingDirection.Free, 2, int.MaxValue, evaluator);

        public static OperatorEntry InfixRight(string input, int power, EvaluationDelegate evaluator) =>
            OperatorEntry.From(input, power, BindingDirection.Right, 2, 2, evaluator);

        public static readonly IReadOnlyList<OperatorEntry> All = new[]
        {
            Infix(",", 0, null),
            Infix("]", 0, null),
            Infix(")", 0, null),

            InfixRight("or", 30, operands => operands[0] is bool bool0 && operands[1] is bool bool1 ? bool0 || bool1 : throw new Exception()),
            InfixRight("and", 40, operands => operands[0] is bool bool0 && operands[1] is bool bool1 ? bool0 && bool1 : throw new Exception()),
            Prefix("not", 50, operands => operands[0] is bool bool0 ? !bool0 : throw new Exception()),

            // TODO: delegate to C# IComparable semantics
            Infix("<", 60, operands => operands[0] is IComparable operand0 && operands[1] is IComparable operand1 ? operand0.CompareTo(operand1) < 0 : throw new Exception()),
            Infix("<=", 60, operands => operands[0] is IComparable operand0 && operands[1] is IComparable operand1 ? operand0.CompareTo(operand1) <= 0 : throw new Exception()),
            Infix(">", 60, operands => operands[0] is IComparable operand0 && operands[1] is IComparable operand1 ? operand0.CompareTo(operand1) > 0 : throw new Exception()),
            Infix(">=", 60, operands => operands[0] is IComparable operand0 && operands[1] is IComparable operand1 ? operand0.CompareTo(operand1) >= 0 : throw new Exception()),
            Infix("<>", 60, operands => operands[0] is IComparable operand0 && operands[1] is IComparable operand1 ? operand0.CompareTo(operand1) != 0 : throw new Exception()),
            Infix("!=", 60, operands => operands[0] is IComparable operand0 && operands[1] is IComparable operand1 ? operand0.CompareTo(operand1) != 0 : throw new Exception()),
            Infix("==", 60, operands => operands[0] is IComparable operand0 && operands[1] is IComparable operand1 ? operand0.CompareTo(operand1) == 0 : operands[0] == null && operands[1] == null ? true : (operands[0] == null || operands[1] == null) ? false : throw new Exception()),

            // TODO: delegate to C# double and int, might prefer coercion rules
            Infix("+", 110, operands =>
                operands[0] is double double0 && operands[1] is double double1 ? double0 + double1 :
                operands[0] is int int0 && operands[1] is int int1 ? (object)(int0 + int1) :
                operands[0] is string string0 && operands[1] is string string1 ? string0 + string1 :
                throw new Exception()
            ),

            Infix("-", 110, operands =>
                operands[0] is double double0 && operands[1] is double double1 ? double0 - double1 :
                operands[0] is int int0 && operands[1] is int int1 ? (object)(int0 - int1) :
                throw new Exception()
            ),

            Infix("*", 120, operands =>
                operands[0] is double double0 && operands[1] is double double1 ? double0 * double1 :
                operands[0] is int int0 && operands[1] is int int1 ? (object)(int0 * int1) :
                throw new Exception()
            ),

            Infix("/", 120, operands =>
                operands[0] is double double0 && operands[1] is double double1 ? double0 / double1 :
                operands[0] is int int0 && operands[1] is int int1 ? (object)(int0 / int1) :
                throw new Exception()
            ),

            Prefix("+", 130, operands =>
                operands[0] is double double0 ? +double0 :
                operands[0] is int int0 ? +int0 :
                throw new Exception()
            ),

            Prefix("-", 130, operands =>
                operands[0] is double double0 ? -double0 :
                operands[0] is int int0 ? -int0 :
                throw new Exception()
            ),

            InfixRight("^", 140, operands =>
                operands[0] is double double0 && operands[1] is double double1 ? Math.Pow(double0, double1) :
                operands[0] is int int0 && operands[1] is int int1 ? Math.Pow(int0, int1) :
                throw new Exception()
            ),

            // null means these operators require special evaluation handling
            Infix(".", 150, null),
            Infix("[", 150, null),
            Infix("(", 150, null),
        };

        public static readonly IReadOnlyDictionary<string, OperatorEntry> PrefixByToken = All.Where(e => e.MinArgs == 1).ToDictionary(e => e.Token);
        public static readonly IReadOnlyDictionary<string, OperatorEntry> InfixByToken = All.Where(e => e.MinArgs > 1).ToDictionary(e => e.Token);
    }
}
