using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.Expressions
{

    /// <summary>
    /// Eagerly evaluate an expression term.
    /// </summary>
    public delegate object Eager(IReadOnlyList<object> parameters);

    /// <summary>
    /// Schema for entry in table of operators
    /// </summary>
    public sealed class OperatorEntry
    {
        public string Token { get; private set; }

        public int Power { get; private set; }

        public BindingDirection Direction { get; private set; }

        public int ArityMin { get; private set; }

        public int ArityMax { get; private set; }

        public Eager Eager { get; private set; }

        public static OperatorEntry From(string token, int power, BindingDirection direction, int arityMin, int arityMax, Eager eager)
            => new OperatorEntry() { Token = token, Power = power, Direction = direction, ArityMin = arityMax, ArityMax = arityMax, Eager = eager };
        public override string ToString() => Token;
    }
}
