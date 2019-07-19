// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Handoff
{
    /// <summary>
    /// Extends ITurnContext to add InitiateHandoffAsync
    /// </summary>
    public static class TurnContextExtender
    {
        /// <summary>
        /// Initiate handoff to human agent.
        /// </summary>
        /// <param name="activities">Transcript of the activities that took place so far.</param>
        /// <param name="handoffContext">Additional channel-specific content.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>IHandoffRequest.</returns>
        public static Task<IHandoffRequest> InitiateHandoffAsync(this ITurnContext turnContext, IActivity[] activities, object handoffContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }
    }
}
