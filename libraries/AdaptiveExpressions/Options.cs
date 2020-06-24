using System;
using System.Globalization;

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
            this.Locale = null;
        }

        public Options(Options opt)
        {
            this.NullSubstitution = opt.NullSubstitution;
            this.Locale = opt.Locale;
        }

        /// <summary>
        /// Gets or sets a value, a function that been called when there is null value hit in memory.
        /// </summary>
        /// <value>
        /// The function delegate.
        /// </value>
        public Func<string, object> NullSubstitution { get; set; } = null;

        /// <summary>
        /// Gets or sets a value, a locale of CultureInfo.
        /// </summary>
        /// <value>
        /// The locale info.
        /// </value>
        public CultureInfo Locale { get; set; } = null;
    }
}
