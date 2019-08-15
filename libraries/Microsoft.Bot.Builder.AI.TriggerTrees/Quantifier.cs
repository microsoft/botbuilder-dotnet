using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.AI.TriggerTrees
{
    /// <summary>
    /// Type of quantifier for expanding trigger expressions.
    /// </summary>
    public enum QuantifierType {
        /// <summary>
        /// Within a clause, duplicate any predicate with variable for each possible binding.
        /// </summary>
        All,

        /// <summary>
        /// Create a new clause for each possible binding of variable.
        /// </summary>
        Any }
;

    /// <summary>
    /// Quantifier for allowing runtime expansion of expressions.
    /// </summary>
    public class Quantifier
    {
        /// <summary>
        /// Name of variable that will be replaced.
        /// </summary>
        public string Variable { get;  }

        /// <summary>
        /// Type of quantifier.
        /// </summary>
        public QuantifierType Type { get; }

        /// <summary>
        /// Possible bindings for quantifier.
        /// </summary>
        public IEnumerable<string> Bindings { get; }

        /// <summary>
        /// Create a quantifier.
        /// </summary>
        /// <param name="variable">Name of variable to replace.</param>
        /// <param name="type">Type of quantifier.</param>
        /// <param name="bindings">Possible bindings for variable.</param>
        public Quantifier(string variable, QuantifierType type, IEnumerable<string> bindings)
        {
            Variable = variable;
            Type = type;
            Bindings = bindings;
        }

        public override string ToString()
        {
            return $"{Type} {Variable} {Bindings.Count()}";
        }
    }
}
