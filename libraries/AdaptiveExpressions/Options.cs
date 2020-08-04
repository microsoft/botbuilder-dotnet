using System;

namespace AdaptiveExpressions
{
    /// <summary>
    /// Options used to define evaluation behaviors.
    /// </summary>
    public class Options
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Options"/> class.
        /// </summary>
        public Options()
        {
            this.NullSubstitution = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Options"/> class.
        /// </summary>
        /// <param name="opt">An Options instance.</param>
        public Options(Options opt)
        {
            this.NullSubstitution = opt.NullSubstitution;
        }

        /// <summary>
        /// Gets or sets a value, a function that been called when there is null value hit in memory.
        /// </summary>
        /// <value>
        /// The function delegate.
        /// </value>
        public Func<string, object> NullSubstitution { get; set; } = null;
    }
}
