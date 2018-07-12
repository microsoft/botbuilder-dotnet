// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.Bot.Builder.Ai.LUIS
{
    /// <summary>
    /// A mockable interface for the LUIS model.
    /// </summary>
    public interface ILuisModel
    {
        /// <summary>
        /// Gets the GUID for the LUIS model.
        /// </summary>
        /// <value>
        /// The GUID for the LUIS model.
        /// </value>
        string ModelID { get; }

        /// <summary>
        /// Gets the subscription key for LUIS.
        /// </summary>
        /// <value>
        /// The subscription key for LUIS.
        /// </value>
        string SubscriptionKey { get; }

        /// <summary>
        /// Gets base URI for LUIS calls.
        /// </summary>
        /// <value>
        /// Base URI for LUIS calls.
        /// </value>
        Uri UriBase { get; }

        /// <summary>
        /// Gets version of the LUIS API to call.
        /// </summary>
        /// <value>
        /// Version of the LUIS API to call.
        /// </value>
        LuisApiVersion ApiVersion { get; }

        /// <summary>
        /// Gets threshold for top scoring intent.
        /// </summary>
        /// <value>
        /// Threshold for top scoring intent.
        /// </value>
        double Threshold { get; }

        /// <summary>
        /// Modify a Luis request to specify query parameters like spelling or logging.
        /// </summary>
        /// <param name="request">Request so far.</param>
        /// <returns>Modified request.</returns>
        LuisRequest ModifyRequest(LuisRequest request);
    }
}
