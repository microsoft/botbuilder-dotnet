// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder
{
    public class MultiAdapter : BotAdapter
    {
        private readonly Dictionary<string, BotAdapter> _adapterMap = new Dictionary<string, BotAdapter>();

        public BotAdapter AddAdapter(BotAdapter adapter)
        {
            return AddAdapter("*", adapter);
        }

        public BotAdapter AddAdapter(string adapterId, BotAdapter adapter)
        {
            // ensure unique and add
            if (_adapterMap.ContainsKey(adapterId))
            {
                throw new Exception($"MultiAdapter.AddAdapter(): an adapter with a channel id of '{adapterId}' has already been added.");
            }

            _adapterMap.Add(adapterId, adapter);

            // Chain middleware stacks together
            // TODO: we don't have MiddlewareHandler in C# (JS code from steve below
            // adapter.use(async (context: TurnContext, next: () => Promise<void>) => {
            //     await this.middleware.run(context, next)
            // });
            return this;
        }

        public BotAdapter GetAdapter(string channelId)
        {
            if (_adapterMap.ContainsKey(channelId))
            {
                return _adapterMap[channelId];
            }

            if (_adapterMap.ContainsKey("*"))
            {
                return _adapterMap[channelId];
            }

            throw new Exception($"MultiAdapter.GetAdapter(): an adapter with a channel id of '{channelId}' not found.");
        }

        public override async Task<ResourceResponse[]> SendActivitiesAsync(ITurnContext turnContext, Activity[] activities, CancellationToken cancellationToken)
        {
            var responses = new List<ResourceResponse>();
            foreach (var activity in activities)
            {
                // Route to appropriate adapter
                var adapter = GetAdapter(activity.ChannelId);
                var ar = await adapter.SendActivitiesAsync(turnContext, activities, cancellationToken).ConfigureAwait(false);

                // TODO: not sure if this is equivalent to the logic steve has in JS
                responses.AddRange(ar);
            }

            return responses.ToArray();
        }

        public override async Task<ResourceResponse> UpdateActivityAsync(ITurnContext turnContext, Activity activity, CancellationToken cancellationToken)
        {
            // Route to appropriate adapter
            var adapter = GetAdapter(activity.ChannelId);
            return await adapter.UpdateActivityAsync(turnContext, activity, cancellationToken).ConfigureAwait(false);
        }

        public override async Task DeleteActivityAsync(ITurnContext turnContext, ConversationReference reference, CancellationToken cancellationToken)
        {
            // Route to appropriate adapter
            var adapter = GetAdapter(reference.ChannelId);
            await adapter.DeleteActivityAsync(turnContext, reference, cancellationToken).ConfigureAwait(false);
        }

        public override async Task ContinueConversationAsync(string botId, ConversationReference reference, BotCallbackHandler callback, CancellationToken cancellationToken)
        {
            // Route to appropriate adapter
            var adapter = GetAdapter(reference.ChannelId);
            await adapter.ContinueConversationAsync(botId, reference, callback, cancellationToken).ConfigureAwait(false);
        }
    }
}
