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
        public static OperatorEntry Prefix(string input, int power, Eager eager) =>
            OperatorEntry.From(input, power, BindingDirection.Left, 1, 1, eager);

        public static OperatorEntry Infix(string input, int power, Eager eager) =>
            OperatorEntry.From(input, power, BindingDirection.Left, 2, 2, eager);

        public static OperatorEntry InfixFree(string input, int power, Eager eager) =>
            OperatorEntry.From(input, power, BindingDirection.Free, 2, int.MaxValue, eager);

        public static OperatorEntry InfixRight(string input, int power, Eager eager) =>
            OperatorEntry.From(input, power, BindingDirection.Right, 2, 2, eager);

        public static readonly IReadOnlyList<OperatorEntry> All = new[]
        {
            Infix(",", 0, null),
            Infix("]", 0, null),
            Infix(")", 0, null),

            InfixRight("or", 30, p => p[0] is bool b0 && p[1] is bool b1 ? b0 || b1 : throw new Exception()),
            InfixRight("and", 40, p => p[0] is bool b0 && p[1] is bool b1 ? b0 && b1 : throw new Exception()),
            Prefix("not", 50, p => p[0] is bool b0 ? !b0 : throw new Exception()),

            // TODO: delegate to C# IComparable semantics
            Infix("<", 60, p => p[0] is IComparable c0 && p[1] is IComparable c1 ? c0.CompareTo(c1) < 0 : throw new Exception()),
            Infix("<=", 60, p => p[0] is IComparable c0 && p[1] is IComparable c1 ? c0.CompareTo(c1) <= 0 : throw new Exception()),
            Infix(">", 60, p => p[0] is IComparable c0 && p[1] is IComparable c1 ? c0.CompareTo(c1) > 0 : throw new Exception()),
            Infix(">=", 60, p => p[0] is IComparable c0 && p[1] is IComparable c1 ? c0.CompareTo(c1) >= 0 : throw new Exception()),
            Infix("<>", 60, p => p[0] is IComparable c0 && p[1] is IComparable c1 ? c0.CompareTo(c1) != 0 : throw new Exception()),
            Infix("!=", 60, p => p[0] is IComparable c0 && p[1] is IComparable c1 ? c0.CompareTo(c1) != 0 : throw new Exception()),
            Infix("==", 60, p => p[0] is IComparable c0 && p[1] is IComparable c1 ? c0.CompareTo(c1) == 0 : throw new Exception()),

            // TODO: delegate to C# double and int, might prefer coercion rules
            Infix("+", 110, p =>
                p[0] is double d0 && p[1] is double d1 ? d0 + d1 :
                p[0] is int i0 && p[1] is int i1 ? (object)(i0 + i1) :
                p[0] is string s0 && p[1] is string s1 ? s0 + s1 :
                throw new Exception()
            ),

            Infix("-", 110, p =>
                p[0] is double d0 && p[1] is double d1 ? d0 - d1 :
                p[0] is int i0 && p[1] is int i1 ? (object)(i0 - i1) :
                throw new Exception()
            ),

            Infix("*", 120, p =>
                p[0] is double d0 && p[1] is double d1 ? d0 * d1 :
                p[0] is int i0 && p[1] is int i1 ? (object)(i0 * i1) :
                throw new Exception()
            ),

            Infix("/", 120, p =>
                p[0] is double d0 && p[1] is double d1 ? d0 / d1 :
                p[0] is int i0 && p[1] is int i1 ? (object)(i0 / i1) :
                throw new Exception()
            ),

            Prefix("+", 130, p =>
                p[0] is double d0 ? +d0 :
                p[0] is int i0 ? +i0 :
                throw new Exception()
            ),

            Prefix("-", 130, p =>
                p[0] is double d0 ? -d0 :
                p[0] is int i0 ? -i0 :
                throw new Exception()
            ),

            InfixRight("^", 140, p =>
                p[0] is double d0 && p[1] is double d1 ? Math.Pow(d0, d1) :
                p[0] is int i0 && p[1] is int i1 ? Math.Pow(i0, i1) :
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
