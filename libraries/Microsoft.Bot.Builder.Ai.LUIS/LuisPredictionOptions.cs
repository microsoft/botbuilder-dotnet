// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Bot.Builder.Ai.LUIS
{
    /// <summary>
    /// Optional parameters for a LUIS prediction request.
    /// </summary>
    public class LuisPredictionOptions
    {
        /// <summary>
        /// Gets or sets if logging of queries to LUIS is allowed.
        /// </summary>
        public bool? Log { get; set; }

        /// <summary>
        /// Gets or sets whether to spell check queries.
        /// </summary>
        public bool? SpellCheck { get; set; }

        /// <summary>
        /// Gets or sets whether to use the staging endpoint.
        /// </summary>
        public bool? Staging { get; set; }

        /// <summary>
        /// Gets or sets the time zone offset.
        /// </summary>
        public double? TimezoneOffset { get; set; }

        /// <summary>
        /// Gets or sets the verbose flag.
        /// </summary>
        public bool? Verbose { get; set; }

        /// <summary>
        /// Gets or sets the Bing Spell Check subscription key.
        /// </summary>
        public string BingSpellCheckSubscriptionKey { get; set; }
    }
}
