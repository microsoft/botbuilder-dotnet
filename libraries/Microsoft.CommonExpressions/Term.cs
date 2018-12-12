using System.Collections.Generic;

namespace Microsoft.Expressions
{
    /// <summary>
    /// Expression term from grammar analysis.
    /// </summary>
    public sealed class Term
    {
        public Token Token { get; private set; }

        public OperatorEntry Entry { get; private set; }

        public IReadOnlyList<Term> Terms { get; private set; }

        public static Term From(Token token, OperatorEntry entry, params Term[] terms) => new Term()
        {
            Token = token,
            Entry = entry,
            Terms = terms
        };

        public override string ToString() => Terms.Count > 0 ? $"{Token}({string.Join(",", Terms)})" : Token.Input;
    }
}
