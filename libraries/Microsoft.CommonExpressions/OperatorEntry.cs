using System.Collections.Generic;

namespace Microsoft.Expressions
{

    /// <summary>
    /// Delegate which evaluates operators operands (aka the paramters) to the result
    /// </summary>
    public delegate object EvaluationDelegate(IReadOnlyList<object> parameters);

    /// <summary>
    /// Schema for entry in table of operators
    /// </summary>
    public sealed class OperatorEntry
    {
        /// <summary>
        /// Token for operator
        /// </summary>
        public string Token { get; private set; }

        /// <summary>
        /// Precedence order (lower numbers evaluated before higher)
        /// </summary>
        public int Power { get; private set; }

        /// <summary>
        /// Binding direction (left, free, right)
        /// </summary>
        public BindingDirection Direction { get; private set; }

        /// <summary>
        /// Min number of args (arityMin)
        /// </summary>
        public int MinArgs { get; private set; }

        /// <summary>
        /// Max number of args to support (arityMax)
        /// </summary>
        public int MaxArgs { get; private set; }

        /// <summary>
        /// Delegate to evaluates operator operands (aka the paramters) to the result
        /// </summary>
        public EvaluationDelegate Evaluate { get; private set; }

        /// <summary>
        /// Create OperatorEntry 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="power"></param>
        /// <param name="direction"></param>
        /// <param name="minArgs"></param>
        /// <param name="maxArgs"></param>
        /// <param name="evaluator">the delegate which evaluates the operands according to the operator semantics</param>
        /// <returns></returns>
        public static OperatorEntry From(string token, int power, BindingDirection direction, int minArgs, int maxArgs, EvaluationDelegate evaluator)
            => new OperatorEntry()
            {
                Token = token,
                Power = power,
                Direction = direction,
                MinArgs = maxArgs,
                MaxArgs = maxArgs,
                Evaluate = evaluator
            };


        public override string ToString() => Token;
    }
}
