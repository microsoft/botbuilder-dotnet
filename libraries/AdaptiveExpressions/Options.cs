using System;
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
            this.RandomSeed = opt.RandomSeed;
            this.RandomValue = opt.RandomValue;
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
        /// Gets or sets random seed.
        /// </summary>
        /// <value>
        /// The random seed.
        /// </value>
        public int? RandomSeed { get; set; }

        /// <summary>
        /// Gets or sets random value.
        /// </summary>
        /// <value>
        /// The random value.
        /// </value>
        public int? RandomValue { get; set; }
    }
}
