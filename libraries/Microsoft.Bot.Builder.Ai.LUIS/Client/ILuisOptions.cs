// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Bot.Builder.Ai.LUIS
{
    /// <summary>
    /// Interface containing optional parameters for a LUIS request.
    /// </summary>
    public interface ILuisOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether if logging of queries to LUIS is allowed.
        /// </summary>
        /// <value>
        /// Indicates if logging of queries to LUIS is allowed.
        /// </value>
        bool? Log { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether if spell checking is enabled.
        /// </summary>
        /// <value>
        /// Indicates if spell checking is enabled.</placeholder>
        /// </value>
        bool? SpellCheck { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether if the staging endpoint is used..
        /// </summary>
        /// <value>
        /// Indicates if the staging endpoint is used.
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
