// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core
{
    /// <summary>
    /// Handles InvokeActivity for ChannelAPI methods calls coming from the skill adapter.
    /// </summary>
    internal class ChannelApiMiddleware : IMiddleware
    {
        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default)
        {
            if (turnContext.Activity.Type == ActivityTypes.Invoke && turnContext.Activity.Name == SkillHandler.InvokeActivityName)
            {
                // process Invoke Activity 
                var invokeActivity = turnContext.Activity.AsInvokeActivity();
                var invokeArgs = invokeActivity.Value as ChannelApiArgs;

                // TODO This needs to be more robust to get bot id
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
            turnContext.Activity.Entities = endOfConversation.Entities;
            turnContext.Activity.LocalTimestamp = endOfConversation.LocalTimestamp;
            turnContext.Activity.Timestamp = endOfConversation.Timestamp;
            turnContext.Activity.Value = activityPayload.Value;
            turnContext.Activity.ChannelData = endOfConversation.ChannelData;
            turnContext.Activity.Properties = ((Activity)endOfConversation).Properties;
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
                    // Send activity(activity)
                    case ChannelApiMethods.SendToConversation:
                    {
                        var activityPayload = (Activity)invokeArgs.Args[0];
                        if (activityPayload.Type == ActivityTypes.EndOfConversation)
                        {
                            await ProcessEndOfConversationAsync(turnContext, next, activityPayload, cancellationToken).ConfigureAwait(false);
                            invokeArgs.Result = new ResourceResponse(id: Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture));
                            return;
                        }

                        invokeArgs.Result = await turnContext.SendActivityAsync(activityPayload, cancellationToken).ConfigureAwait(false);
                        return;
                    }

                    // Send activity(replyToId, activity)
                    case ChannelApiMethods.ReplyToActivity:
                    {
                        var activityPayload = (Activity)invokeArgs.Args[1];
                        activityPayload.ReplyToId = (string)invokeArgs.Args[0];

                        if (activityPayload.Type == ActivityTypes.EndOfConversation)
                        {
                            await ProcessEndOfConversationAsync(turnContext, next, activityPayload, cancellationToken).ConfigureAwait(false);
                            invokeArgs.Result = new ResourceResponse(id: Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture));
                            return;
                        }

                        invokeArgs.Result = await turnContext.SendActivityAsync(activityPayload, cancellationToken).ConfigureAwait(false);
                        return;
                    }

                    // UpdateActivity(activity)
                    case ChannelApiMethods.UpdateActivity:
                        invokeArgs.Result = await turnContext.UpdateActivityAsync((Activity)invokeArgs.Args[0], cancellationToken).ConfigureAwait(false);
                        return;

                    // DeleteActivity(activityId)
                    case ChannelApiMethods.DeleteActivity:
                        await turnContext.DeleteActivityAsync((string)invokeArgs.Args[0], cancellationToken).ConfigureAwait(false);
                        break;

                    // SendConversationHistory(history)
                    case ChannelApiMethods.SendConversationHistory:
                        throw new NotImplementedException($"{ChannelApiMethods.SendConversationHistory} is not supported");

                    // GetConversationMembers()
                    case ChannelApiMethods.GetConversationMembers:
                        if (adapter != null)
                        {
                            invokeArgs.Result = await adapter.GetConversationMembersAsync(turnContext, cancellationToken).ConfigureAwait(false);
                        }

                        break;

                    // GetConversationPageMembers((int)pageSize, continuationToken)
                    case ChannelApiMethods.GetConversationPagedMembers:
                        throw new NotImplementedException($"{ChannelApiMethods.SendConversationHistory} is not supported");

                    //if (adapter != null)
                    //{
                    //    invokeArgs.Result = await adapter.OnGetConversationsAsync((int)invokeArgs.Args[0], (string)invokeArgs.Args[1], cancellationToken).ConfigureAwait(false);
                    //}

                    // DeleteConversationMember(memberId)
                    case ChannelApiMethods.DeleteConversationMember:
                        if (adapter != null)
                        {
                            await adapter.DeleteConversationMemberAsync(turnContext, (string)invokeArgs.Args[0], cancellationToken).ConfigureAwait(false);
                        }

                        break;

                    // GetActivityMembers(activityId)
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
