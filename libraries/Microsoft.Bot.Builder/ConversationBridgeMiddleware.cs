// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder
{
    public delegate Task<ConversationBridgeActions> ConversationBridgeFilter(ITurnContext turnContext);

    public delegate Task<InvokeResponse> ConversationBridgeCommand(ITurnContext turnContext, object args, CancellationToken cancellationToken);

    public enum ConversationBridgeActions
    {
        /// <summary>
        /// Rats.
        /// </summary>
        Block,

        /// <summary>
        /// Pigs.
        /// </summary>
        Forward,

        /// <summary>
        /// Cats.
        /// </summary>
        PassThrough,
    }

    public class ConversationBridgeMiddleware : IMiddleware
    {
        private static readonly ConversationBridgeFilter _defaultFilter = context =>
        {
            var activity = context.Activity;
            if (activity.RelatesTo != null && activity.Type != ActivityTypes.EndOfConversation)
            {
                return Task.FromResult(ConversationBridgeActions.Forward);
            }

            return Task.FromResult(ConversationBridgeActions.PassThrough);
        };

        private readonly Dictionary<string, ConversationBridgeCommand> _commands = new Dictionary<string, ConversationBridgeCommand>();
        private readonly ConversationBridgeFilter _filter;

        public ConversationBridgeMiddleware()
            : this(_defaultFilter)
        {
        }

        public ConversationBridgeMiddleware(ConversationBridgeFilter filter, bool allowAdapterCalls = true)
        {
            _filter = filter;

            // Add adapter commands
            if (allowAdapterCalls)
            {
                AddCommand("BotAdapter.sendActivities", async (context, args, cancellationToken) =>
                {
                    // TODO: find a better solution for (Dictionary<string, object>)args.
                    var argsDictionary = (Dictionary<string, object>)args;
                    var responses = await context.Adapter.SendActivitiesAsync(context, (Activity[])argsDictionary["activities"], cancellationToken).ConfigureAwait(false);
                    return new InvokeResponse()
                    {
                        Status = 200,
                        Body = responses,
                    };
                });
                AddCommand("BotAdapter.updateActivity", async (context, args, cancellationToken) =>
                {
                    var argsDictionary = (Dictionary<string, object>)args;
                    await context.Adapter.UpdateActivityAsync(context, (Activity)argsDictionary["activity"], cancellationToken).ConfigureAwait(false);
                    return new InvokeResponse
                    {
                        Status = 200,
                    };
                });
                AddCommand("BotAdapter.deleteActivity", async (context, args, cancellationToken) =>
                {
                    var argsDictionary = (Dictionary<string, object>)args;
                    await context.Adapter.DeleteActivityAsync(context, (ConversationReference)argsDictionary["reference"], cancellationToken).ConfigureAwait(false);
                    return new InvokeResponse
                    {
                        Status = 200,
                    };
                });
            }
        }

        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default)
        {
            // Filter incoming activity
            var conversationBridgeActions = await _filter(turnContext).ConfigureAwait(false);
            switch (conversationBridgeActions)
            {
                case ConversationBridgeActions.Forward:
                    if (turnContext.Activity.Type == ActivityTypes.Invoke)
                    {
                        // Invoke command
                        await OnInvokeCommand(turnContext, cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        // Forward activity
                        await OnForwardActivity(turnContext, cancellationToken).ConfigureAwait(false);
                    }

                    break;
                case ConversationBridgeActions.PassThrough:
                    await next(cancellationToken).ConfigureAwait(false);
                    break;
            }
        }

        private async Task OnForwardActivity(ITurnContext context, CancellationToken cancellationToken)
        {
            // Clone activity and re-address
            var clone = JsonConvert.DeserializeObject<Activity>(JsonConvert.SerializeObject(context.Activity));
            clone.ApplyConversationReference(clone.RelatesTo);
            clone.RelatesTo = null;

            // Forward to adapter for delivery
            await context.Adapter.SendActivitiesAsync(context, new[] { clone }, cancellationToken).ConfigureAwait(false);
        }

        private async Task OnInvokeCommand(ITurnContext context, CancellationToken cancellationToken)
        {
            // Lookup and invoke command
            // - Commands are always processed as 'invoke' activities
            var activity = context.Activity;
            var command = _commands[activity.Name];
            if (command != null)
            {
                var response = await command(context, activity.Value, cancellationToken).ConfigureAwait(false);
                await context.SendActivityAsync(new Activity(type: "invokeResponse", value: response), cancellationToken).ConfigureAwait(false);
            }
            else
            {
                await context.SendActivityAsync(new Activity(type: "invokeResponse", value: new InvokeResponse { Status = 404 }), cancellationToken).ConfigureAwait(false);
            }
        }

        private void AddCommand(string name, ConversationBridgeCommand command)
        {
            _commands.Add(name, command);
        }
    }
}
