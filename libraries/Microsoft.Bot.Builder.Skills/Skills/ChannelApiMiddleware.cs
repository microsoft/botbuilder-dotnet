// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Skills
{
    /// <summary>
    /// Handles InvokeActivity for ChannelAPI methods calls coming from the skill adapter.
    /// </summary>
    internal class ChannelApiMiddleware : IMiddleware
    {
        public const string InvokeActivityName = "SkillEvents.ChannelApiInvoke";

        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default)
        {
            if (turnContext.Activity.Type == ActivityTypes.Invoke && turnContext.Activity.Name == InvokeActivityName)
            {
                // process skill invoke Activity 
                var invokeActivity = turnContext.Activity.AsInvokeActivity();
                var invokeArgs = invokeActivity.Value as ChannelApiArgs;
                await ProcessSkillActivityAsync(turnContext, next, invokeArgs, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                // just pass through
                await next(cancellationToken).ConfigureAwait(false);
            }
        }

        private static async Task ProcessEndOfConversationAsync(ITurnContext turnContext, NextDelegate next, Activity activityPayload, CancellationToken cancellationToken)
        {
            // transform the turnContext.Activity to be the EndOfConversation and pass up to the bot, we would set the Activity, but it only has a get;
            var endOfConversation = activityPayload.AsEndOfConversationActivity();
            turnContext.Activity.Type = endOfConversation.Type;
            turnContext.Activity.Text = endOfConversation.Text;
            turnContext.Activity.Code = endOfConversation.Code;

            turnContext.Activity.ReplyToId = endOfConversation.ReplyToId;
            turnContext.Activity.Value = activityPayload.Value;
            turnContext.Activity.Entities = endOfConversation.Entities;
            turnContext.Activity.LocalTimestamp = endOfConversation.LocalTimestamp;
            turnContext.Activity.Timestamp = endOfConversation.Timestamp;
            turnContext.Activity.ChannelData = endOfConversation.ChannelData;
            turnContext.Activity.Properties = ((Activity)endOfConversation).Properties;
            await next(cancellationToken).ConfigureAwait(false);
        }

        private static async Task ProcessEventAsync(ITurnContext turnContext, NextDelegate next, Activity activityPayload, CancellationToken cancellationToken)
        {
            // transform the turnContext.Activity to be the EventActivity and pass up to the bot, we would set the Activity, but it only has a get;
            var eventActivity = activityPayload.AsEventActivity();
            turnContext.Activity.Type = eventActivity.Type;
            turnContext.Activity.Name = eventActivity.Name;
            turnContext.Activity.Value = eventActivity.Value;
            turnContext.Activity.RelatesTo = eventActivity.RelatesTo;

            turnContext.Activity.ReplyToId = eventActivity.ReplyToId;
            turnContext.Activity.Value = activityPayload.Value;
            turnContext.Activity.Entities = eventActivity.Entities;
            turnContext.Activity.LocalTimestamp = eventActivity.LocalTimestamp;
            turnContext.Activity.Timestamp = eventActivity.Timestamp;
            turnContext.Activity.ChannelData = eventActivity.ChannelData;
            turnContext.Activity.Properties = ((Activity)eventActivity).Properties;
            await next(cancellationToken).ConfigureAwait(false);
        }

        private async Task ProcessSkillActivityAsync(ITurnContext turnContext, NextDelegate next, ChannelApiArgs invokeArgs, CancellationToken cancellationToken)
        {
            try
            {
                // TODO: this cast won't work for custom adapters
                var adapter = turnContext.Adapter as BotFrameworkAdapter;

                switch (invokeArgs.Method)
                {
                    case ChannelApiMethods.SendToConversation:
                    case ChannelApiMethods.ReplyToActivity:
                    {
                        var activityPayload = (Activity)invokeArgs.Args[0];
                        if (invokeArgs.Args.Length > 1)
                        {
                            // ReplyToActivity send a ReplyToId property.
                            activityPayload.ReplyToId = (string)invokeArgs.Args[1];
                        }

                        switch (activityPayload.Type)
                        {
                            case ActivityTypes.EndOfConversation:
                                await ProcessEndOfConversationAsync(turnContext, next, activityPayload, cancellationToken).ConfigureAwait(false);
                                invokeArgs.Result = new ResourceResponse(id: Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture));
                                return;
                            case ActivityTypes.Event:
                                await ProcessEventAsync(turnContext, next, activityPayload, cancellationToken).ConfigureAwait(false);
                                invokeArgs.Result = new ResourceResponse(id: Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture));
                                return;
                            default:
                                invokeArgs.Result = await turnContext.SendActivityAsync(activityPayload, cancellationToken).ConfigureAwait(false);
                                return;
                        }
                    }

                    case ChannelApiMethods.UpdateActivity:
                        invokeArgs.Result = await turnContext.UpdateActivityAsync((Activity)invokeArgs.Args[0], cancellationToken).ConfigureAwait(false);
                        return;

                    case ChannelApiMethods.DeleteActivity:
                        await turnContext.DeleteActivityAsync((string)invokeArgs.Args[0], cancellationToken).ConfigureAwait(false);
                        break;

                    case ChannelApiMethods.SendConversationHistory:
                        throw new NotImplementedException($"{ChannelApiMethods.SendConversationHistory} is not supported");

                    case ChannelApiMethods.GetConversationMembers:
                        if (adapter != null)
                        {
                            invokeArgs.Result = await adapter.GetConversationMembersAsync(turnContext, cancellationToken).ConfigureAwait(false);
                        }

                        break;

                    case ChannelApiMethods.GetConversationPagedMembers:
                        throw new NotImplementedException($"{ChannelApiMethods.SendConversationHistory} is not supported");

                    //if (adapter != null)
                    //{
                    //    invokeArgs.Result = await adapter.OnGetConversationsAsync((int)invokeArgs.Args[0], (string)invokeArgs.Args[1], cancellationToken).ConfigureAwait(false);
                    //}

                    case ChannelApiMethods.DeleteConversationMember:
                        if (adapter != null)
                        {
                            await adapter.DeleteConversationMemberAsync(turnContext, (string)invokeArgs.Args[0], cancellationToken).ConfigureAwait(false);
                        }

                        break;

                    case ChannelApiMethods.GetActivityMembers:
                        if (adapter != null)
                        {
                            invokeArgs.Result = await adapter.GetActivityMembersAsync(turnContext, (string)invokeArgs.Args[0], cancellationToken).ConfigureAwait(false);
                        }

                        break;

                    case ChannelApiMethods.UploadAttachment:
                        throw new NotImplementedException($"{ChannelApiMethods.UploadAttachment} is not supported");
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types (excluding, we use the general exception to store it in the inokeArgs).
            catch (Exception ex)
            {
                invokeArgs.Exception = ex;
            }
#pragma warning restore CA1031 // Do not catch general exception types
        }
    }
}
