// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;

namespace Microsoft.Bot.Builder.AI.LuisV3
{
    /// <summary>
    /// Optional parameters for a LUIS prediction request.
    /// </summary>
    public class LuisPredictionOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LuisPredictionOptions"/> class.
        /// </summary>
        public LuisPredictionOptions()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LuisPredictionOptions"/> class from an existing instance.
        /// </summary>
        /// <param name="other">Source of values.</param>
        public LuisPredictionOptions(LuisPredictionOptions other)
        {
            IncludeAllIntents = other.IncludeAllIntents;
            IncludeAPIResults = other.IncludeAPIResults;
            IncludeInstanceData = other.IncludeInstanceData;
            Log = other.Log;
            DynamicLists = other.DynamicLists;
            ExternalEntities = other.ExternalEntities;
            PreferExternalEntities = other.PreferExternalEntities;
            Slot = other.Slot;
            Version = other.Version;
        }

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
        /// Gets or sets a value indicating whether API results should be included.
        /// </summary>
        /// <value>True to include API results.</value>
        /// <remarks>This is mainly useful for testing or getting access to LUIS features not yet in the SDK.</remarks>
        [Obsolete("Member is deprecated, please use LuisRecognizerOptionsV3 to set this value).")]
        public bool IncludeAPIResults { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether queries should be logged in LUIS.
        /// </summary>
        /// <value>
        /// If queries should be logged in LUIS in order to help build better models through active learning.
        /// </value>
        /// <remarks>The default is to log queries to LUIS in order to support active learning.  To default to the Luis setting set to null.</remarks>
        public bool? Log { get; set; } = true;

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
        /// Gets or sets datetimeV2 offset. The format for the datetimeReference is ISO 8601.
        /// </summary>
        /// <value>
        /// DateTimeReference.
        /// </value>
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
        public string Slot { get; set; } = LuisSlot.Production;

        /// <summary>
        /// Gets or sets the specific version of the application to access.
        /// </summary>
        /// <value>
        /// Version to access.
        /// </value>
        /// <remarks>
        /// LUIS supports versions and this is the version to use instead of a slot.
        /// If this is specified, then the <see cref="Slot"/> is ignored.
        /// </remarks>
        public string Version { get; set; }
    }
}
