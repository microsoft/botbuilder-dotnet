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
        /// Indicates if logging of queries to LUIS is allowed.
        /// </summary>
        bool? Log { get; set; }

        /// <summary>
        /// Turn on spell checking.
        /// </summary>
        bool? SpellCheck { get; set; }

        /// <summary>
        /// Use the staging endpoint.
        /// </summary>
        bool? Staging { get; set; }

        /// <summary>
        /// The time zone offset.
        /// </summary>
        double? TimezoneOffset { get; set; }

        /// <summary>
        /// The verbose flag.
        /// </summary>
        bool? Verbose { get; set; }

        /// <summary>
        /// The Bing Spell Check subscription key.
        /// </summary>
        string BingSpellCheckSubscriptionKey { get; set; }
    }

    public static partial class Extensions
    {
        public static void Apply(this ILuisOptions source, ILuisOptions target)
        {
            if (source.Log.HasValue) target.Log = source.Log.Value;
            if (source.SpellCheck.HasValue) target.SpellCheck = source.SpellCheck.Value;
            if (source.Staging.HasValue) target.Staging = source.Staging.Value;
            if (source.TimezoneOffset.HasValue) target.TimezoneOffset = source.TimezoneOffset.Value;
            if (source.Verbose.HasValue) target.Verbose = source.Verbose.Value;
            if (!string.IsNullOrWhiteSpace(source.BingSpellCheckSubscriptionKey)) target.BingSpellCheckSubscriptionKey = source.BingSpellCheckSubscriptionKey;
        }
    }
}

