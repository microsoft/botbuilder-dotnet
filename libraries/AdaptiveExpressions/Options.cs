using System;

namespace AdaptiveExpressions
{
    /// <summary>
    /// Options used to define evaluation behaviors.
    /// </summary>
    public class Options
    {
        public Options()
        {
            this.NullSubstitution = null;
        }

        public Options(Options opt)
        {
            this.NullSubstitution = opt.NullSubstitution;
        }

        /// <summary>
        /// Gets or sets a value, a function that been called when there is null value hit in memory.
        /// </summary>
        /// <value>
        /// The delegate.</placeholder>
        /// </value>
        public Func<string, object> NullSubstitution { get; set; } = null;
    }
}
