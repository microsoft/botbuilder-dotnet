// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
using System.Collections.Generic;

namespace Microsoft.Bot.Builder.AI.Luis
{
    /// <summary>
    /// Optional parameters for a LUIS prediction request.
    /// </summary>
    public class LuisPredictionOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether all intents come back or only the top one.
        /// </summary>
        /// <value>
        /// True for returning all intents.
        /// </value>
        public bool IncludeAllIntents { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether or not instance data should be included in response.
        /// </summary>
        /// <value>
        /// A value indicating whether or not instance data should be included in response.
        /// </value>
        public bool IncludeInstanceData { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether queries should be logged in LUIS.
        /// </summary>
        /// <value>
        /// If queries should be logged in LUIS in order to help build better models through active learning.
        /// </value>
        public bool Log { get; set; } = true;

        /// <summary>
        /// Gets or sets dynamic lists used to recognize entities for a particular query.
        /// </summary>
        /// <value>
        /// Dynamic lists of things like contact names to recognize at query time.
        /// </value>
        public IList<DynamicList> DynamicLists { get; set; }

        /// <summary>
        /// Gets or sets external entities recognized in the query.
        /// </summary>
        /// <value>
        /// External entities recognized in query.
        /// </value>
        public IList<ExternalEntity> ExternalEntities { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether external entities should override other means of recognizing entities.
        /// </summary>
        /// <value>
        /// Boolean for if external entities should be preferred to the results from LUIS models.
        /// </value>
        public bool PreferExternalEntities { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether production (false) or staging (true) slot should be used.
        /// </summary>
        /// <value>
        /// True for staging or false for production.
        /// </value>
        /// <remarks> 
        /// LUIS supports both a production slot and a staging slot and this controls which is used.
        /// If you specify a Version, then a private versioned slot is used instead.
        /// </remarks>
        public bool Staging { get; set; } = false;

        /// <summary>
        /// Gets or sets the specific version of the model to access.
        /// </summary>
        /// <value>
        /// Version to access.
        /// </value>
        /// <remarks>
        /// LUIS supports versions and this is the version name to use instead of a production/staging slots.
        /// If this is specified, then the <see cref="Staging"/> flag is ignored.
        /// </remarks>
        public string Version { get; set; }
    }
}
