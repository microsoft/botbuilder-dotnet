// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;

namespace AdaptiveExpressions.TriggerTrees
{
    /// <summary>
    /// Type of quantifier for expanding trigger expressions.
    /// </summary>
    public enum QuantifierType
    {
        /// <summary>
        /// Within a clause, duplicate any predicate with variable for each possible binding.
        /// </summary>
        All,

        /// <summary>
        /// Create a new clause for each possible binding of variable.
        /// </summary>
        Any
    }

    /// <summary>
    /// Quantifier for allowing runtime expansion of expressions.
    /// </summary>
    public class Quantifier
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Quantifier"/> class.
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

        /// <summary>
        /// Gets name of variable that will be replaced.
        /// </summary>
        /// <value>
        /// Name of variable that will be replaced.
        /// </value>
        public string Variable { get;  }

        /// <summary>
        /// Gets type of quantifier.
        /// </summary>
        /// <value>
        /// Type of quantifier.
        /// </value>
        public QuantifierType Type { get; }

        /// <summary>
        /// Gets possible bindings for quantifier.
        /// </summary>
        /// <value>
        /// Possible bindings for quantifier.
        /// </value>
        public IEnumerable<string> Bindings { get; }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string value.</returns>
        public override string ToString()
        {
            return $"{Type} {Variable} {Bindings.Count()}";
        }
    }
}
