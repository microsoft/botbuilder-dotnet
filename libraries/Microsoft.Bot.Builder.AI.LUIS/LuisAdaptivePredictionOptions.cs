// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.AI.LuisV3;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.Luis
{
    /// <summary>
    /// Optional parameters for a LUIS prediction request.
    /// </summary>
    public class LuisAdaptivePredictionOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LuisAdaptivePredictionOptions"/> class.
        /// </summary>
        public LuisAdaptivePredictionOptions()
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether all intents come back or only the top one.
        /// </summary>
        /// <value>
        /// True for returning all intents.
        /// </value>
        [JsonProperty("includeAllIntents")]
        public bool IncludeAllIntents { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not instance data should be included in response.
        /// </summary>
        /// <value>
        /// A value indicating whether or not instance data should be included in response.
        /// </value>
        [JsonProperty("includeInstanceData")]
        public bool IncludeInstanceData { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether API results should be included.
        /// </summary>
        /// <value>True to include API results.</value>
        /// <remarks>This is mainly useful for testing or getting access to LUIS features not yet in the SDK.</remarks>
        [JsonProperty("includeAPIResults")]
        public bool IncludeAPIResults { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether queries should be logged in LUIS.
        /// </summary>
        /// <value>
        /// If queries should be logged in LUIS in order to help build better models through active learning.
        /// </value>
        /// <remarks>The default is to log queries to LUIS in order to support active learning.  To default to the Luis setting set to null.</remarks>
        [JsonProperty("log")]
        public bool Log { get; set; } = true;

        /// <summary>
        /// Gets external entities recognized in the query.
        /// </summary>
        /// <value>
        /// External entities recognized in query.
        /// </value>
        [JsonProperty("externalEntities")]
#pragma warning disable CA1002 // Do not expose generic lists
        public List<ExternalEntity> ExternalEntities { get; } = new List<ExternalEntity>();
#pragma warning restore CA1002 // Do not expose generic lists

        /// <summary>
        /// Gets or sets a value indicating whether external entities should override other means of recognizing entities.
        /// </summary>
        /// <value>
        /// Boolean for if external entities should be preferred to the results from LUIS models.
        /// </value>
        public bool PreferExternalEntities { get; set; } = true;

        /// <summary>
        /// Gets or sets datetimeV2 offset. The format for the datetimeReference is ISO 8601.
        /// </summary>
        /// <value>
        /// DateTimeReference.
        /// </value>
        [JsonProperty("dateTimeReference")]
        public string DateTimeReference { get; set; }

        /// <summary>
        /// Gets or sets the LUIS slot to use for the application.
        /// </summary>
        /// <value>
        /// The LUIS slot to use for the application.
        /// </value>
        /// <remarks>
        /// By default this uses the production slot.  You can find other standard slots in <see cref="LuisSlot"/>.
        /// If you specify a Version, then a private version of the application is used instead of a slot.
        /// </remarks>
        [JsonProperty("slot")]
        public string Slot { get; set; } = LuisSlot.Production;
    }
}
