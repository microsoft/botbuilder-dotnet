// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Streaming
{
    /// <summary>
    /// Processes streaming activities.
    /// </summary>
    public interface IStreamingActivityProcessor
    {
        Task<InvokeResponse> ProcessStreamingActivityAsync(Activity activity, BotCallbackHandler botCallbackHandler, CancellationToken cancellationToken = default);
    }
}
