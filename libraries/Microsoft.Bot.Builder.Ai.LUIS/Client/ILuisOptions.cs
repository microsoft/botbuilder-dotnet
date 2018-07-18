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
        /// Gets or sets a value indicating whether to log the query.
        /// </summary>
        /// <value>
        /// Indicates whether to log the query.
        /// </value>
        bool? Log { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to enable spell checking.
        /// </summary>
        /// <value>
        /// Indicates whether to enable spell checking.
        /// </value>
        bool? SpellCheck { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use the staging endpoint.
        /// </summary>
        /// <value>
        /// Indicates whether to use the staging endpoint.
        /// </value>
        bool? Staging { get; set; }

        /// <summary>
        /// Gets or sets the timezone offset for the location of the request in minutes.
        /// </summary>
        /// <value>
        /// The timezone offset for the location of the request in minutes.
        /// </value>
        double? TimezoneOffset { get; set; }

        /// <summary>
        /// Gets or sets whether to return all intents instead of just the topscoring intent.
        /// </summary>
        /// <value>
        /// Indicates whether to return all intents instead of just the topscoring intent.
        /// </value>
        bool? Verbose { get; set; }

        /// <summary>
        /// Gets or sets the subscription key to use when enabling bing spell check.
        /// </summary>
        /// <value>
        /// The subscription key to use when enabling bing spell check.
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
