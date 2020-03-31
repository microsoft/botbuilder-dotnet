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
        public delegate object GetValueDelegate(string path);

        /// <summary>
        /// Gets or sets a value, a delegate that been called when there is null value hit in memory.
        /// </summary>
        /// <value>
        /// The delegate.</placeholder>
        /// </value>
        public GetValueDelegate NullSubstitution { get; set; } = null;
    }
}
