using System;
using System.Collections.Generic;
using System.Text;

namespace AdaptiveExpressions
{
    /// <summary>
    /// Options used to define evaluation behaviors.
    /// </summary>
    public class Options
    {
        /// <summary>
        /// Gets or sets a value, a function that been called when there is null value hit in memory.
        /// </summary>
        /// <value>
        /// The delegate.</placeholder>
        /// </value>
        public Func<string, object> NullSubstitution { get; set; } = null;
    }
}
