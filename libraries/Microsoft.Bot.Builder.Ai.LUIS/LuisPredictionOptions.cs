// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Bot.Builder.Ai.Luis
{
    /// <summary>
    /// Optional parameters for a LUIS prediction request.
    /// </summary>
    public class LuisPredictionOptions
    {
        /// <summary>
        /// Gets or sets the Bing Spell Check subscription key.
        /// </summary>
        /// <value>
        /// The Bing Spell Check subscription key.
        /// </value>
        public string BingSpellCheckSubscriptionKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not instance data should be included in response.
        /// </summary>
        /// <value>
        /// A value indicating whether or not instance data should be included in response.
        /// </value>
        public bool? IncludeInstanceData { get; set; }

        /// <summary>
        /// Gets or sets if queries should be logged in LUIS.
        /// </summary>
        /// <value>
        /// If queries should be logged in LUIS.
        /// </value>
        public bool? Log { get; set; }

        /// <summary>
        /// Gets or sets whether to spell check queries.
        /// </summary>
        /// <value>
        /// Whether to spell check queries.
        /// </value>
        public bool? SpellCheck { get; set; }

        /// <summary>
        /// Gets or sets whether to use the staging endpoint.
        /// </summary>
        /// <value>
        /// Whether to use the staging endpoint.
        /// </value>
        public bool? Staging { get; set; }

        /// <summary>
        /// Gets or sets the time zone offset.
        /// </summary>
        /// <value>
        /// The time zone offset.
        /// </value>
        public double? TimezoneOffset { get; set; }

        /// <summary>
        /// Gets or sets the verbose flag which controls if all intents come back or only the top one.
        /// </summary>
        /// <value>
        /// The verbose flag.
        /// </value>
        public bool? Verbose { get; set; }
    }
}
