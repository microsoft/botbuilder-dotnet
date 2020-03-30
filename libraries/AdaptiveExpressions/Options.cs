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
        /// Gets or sets a value indicating whether we allow the memory interface to substitue the value when value is null.
        /// </summary>
        /// <value>
        /// The flag.
        /// </value>
        public bool AllowSubstitution { get; set; } = true;
    }
}
