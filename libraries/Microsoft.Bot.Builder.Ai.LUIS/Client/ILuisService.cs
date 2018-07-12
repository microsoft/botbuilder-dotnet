// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Cognitive.LUIS.Models;

namespace Microsoft.Bot.Builder.Ai.LUIS
{
    /// <summary>
    /// A mockable interface for the LUIS service.
    /// </summary>
    public interface ILuisService
    {
        /// <summary>
        /// Modify the incoming LUIS request.
        /// </summary>
        /// <param name="request">Request so far.</param>
        /// <returns>Modified request.</returns>
        LuisRequest ModifyRequest(LuisRequest request);

        /// <summary>
        /// Build the query uri for the <see cref="LuisRequest"/>.
        /// </summary>
        /// <param name="luisRequest">The luis request text.</param>
        /// <returns>The query uri.</returns>
        Uri BuildUri(LuisRequest luisRequest);

        /// <summary>
        /// Query the LUIS service using this uri.
        /// </summary>
        /// <param name="uri">The query uri.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>The LUIS result.</returns>
        Task<LuisResult> QueryAsync(Uri uri, CancellationToken token);
    }
}
