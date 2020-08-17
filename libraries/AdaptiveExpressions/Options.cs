using System;
using System.Collections.Generic;
using System.Threading;

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
            this.Locale = Thread.CurrentThread.CurrentCulture.Name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Options"/> class.
        /// </summary>
        /// <param name="opt">An Options instance.</param>
        public Options(Options opt)
        {
            this.NullSubstitution = opt.NullSubstitution;
            this.Locale = opt.Locale;
            this.Properties = opt.Properties;
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
        public string Locale { get; set; }

        /// <summary>
        /// Gets or sets the additional properties.
        /// </summary>
        /// <value>
        /// Additional properties.
        /// </value>
#pragma warning disable CA2227 // Collection properties should be read only
        public IDictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
#pragma warning restore CA2227 // Collection properties should be read only

    }
}
