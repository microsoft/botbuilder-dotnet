// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    public delegate Task BotCallbackHandler(ITurnContext context, CancellationToken cancellationToken);
}
