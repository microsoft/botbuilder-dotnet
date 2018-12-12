using System.Collections.Generic;

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
        /// <summary>
        /// Token for operator
        /// </summary>
        public string Token { get; private set; }

        public int Power { get; private set; }

        /// <summary>
        /// Binding direction (left, free, right)
        /// </summary>
        public BindingDirection Direction { get; private set; }

        /// <summary>
        /// Min number of args (arrityMin)
        /// </summary>
        public int MinArgs { get; private set; }

        /// <summary>
        /// Max number of args to support (arrityMax)
        /// </summary>
        public int MaxArgs { get; private set; }

        public Eager Eager { get; private set; }

        /// <summary>
        /// Create OperatorEntry 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="power"></param>
        /// <param name="direction"></param>
        /// <param name="minArgs"></param>
        /// <param name="maxArgs"></param>
        /// <param name="eager"></param>
        /// <returns></returns>
        public static OperatorEntry From(string token, int power, BindingDirection direction, int minArgs, int maxArgs, Eager eager)
            => new OperatorEntry()
            {
                Token = token,
                Power = power,
                Direction = direction,
                MinArgs = maxArgs,
                MaxArgs = maxArgs,
                Eager = eager
            };


        public override string ToString() => Token;
    }
}
