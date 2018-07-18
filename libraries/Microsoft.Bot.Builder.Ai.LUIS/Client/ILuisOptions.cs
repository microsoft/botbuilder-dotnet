// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Bot.Builder.Ai.LUIS
{
    /// <summary>
    /// Contains optional parameters for a LUIS request.
    /// </summary>
    public interface ILuisOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether to logging of queries to LUIS is allowed.
        /// </summary>
        /// <value>
        /// Indicates whether the logging of queries to LUIS is allowed.
        /// </value>
        bool? Log { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether spell checking is enabled.
        /// </summary>
        /// <value>
        /// Indicates whether spell checking is enabled.
        /// </value>
        bool? SpellCheck { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the staging endpoint is used.
        /// </summary>
        /// <value>
        /// Indicates whether the staging endpoint is used.
        /// </value>
        bool? Staging { get; set; }

        /// <summary>
        /// Gets or sets the time zone offset.
        /// </summary>
        /// <value>
        /// The time zone offset.
        /// </value>
        double? TimezoneOffset { get; set; }

        /// <summary>
        /// Gets or sets the verbose flag.
        /// </summary>
        /// <value>
        /// The verbose flag.
        /// </value>
        bool? Verbose { get; set; }

        /// <summary>
        /// Gets or sets the Bing Spell Check subscription key.
        /// </summary>
        /// <value>
        /// The Bing Spell Check subscription key.
        /// </value>
        string BingSpellCheckSubscriptionKey { get; set; }
    }

    /// <summary>
    /// LUIS extension methods.
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// Applies optional parameters from an existing <see cref="ILuisOptions"/>
        /// to another <see cref="ILuisOptions"/>.
        /// </summary>
        /// <param name="source">The object containg the options to copy.</param>
        /// <param name="target">The object to which to apply the options.</param>
        /// <remarks>For each option has a value in the <paramref name="source"/> object,
        /// the value overwrites the value in the <paramref name="target"/> object.</remarks>
        public static void Apply(this ILuisOptions source, ILuisOptions target)
        {
            if (source.Log.HasValue)
            {
                target.Log = source.Log.Value;
            }

            if (source.SpellCheck.HasValue)
            {
                target.SpellCheck = source.SpellCheck.Value;
            }

            if (source.Staging.HasValue)
            {
                target.Staging = source.Staging.Value;
            }

            if (source.TimezoneOffset.HasValue)
            {
                target.TimezoneOffset = source.TimezoneOffset.Value;
            }

            if (source.Verbose.HasValue)
            {
                target.Verbose = source.Verbose.Value;
            }

            if (!string.IsNullOrWhiteSpace(source.BingSpellCheckSubscriptionKey))
            {
                target.BingSpellCheckSubscriptionKey = source.BingSpellCheckSubscriptionKey;
            }
        }
    }
}
