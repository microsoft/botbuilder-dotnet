// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Skills
{
    /// <summary>
    /// Handles InvokeActivity for ChannelAPI method calls coming from SkillHostController.
    /// </summary>
    public class ChannelApiMiddleware : IMiddleware
    {
        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default)
        {
            if (turnContext.Activity.Type == ActivityTypes.Invoke && turnContext.Activity.Name == "ChannelAPI")
            {
                var invokeActivity = turnContext.Activity.AsInvokeActivity();
                var invokeArgs = invokeActivity.Value as ChannelApiArgs;

                // TODO This needs to be more robust to get bot id
                await CallChannelApiAsync(turnContext, next, invokeArgs, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                // just pass through
                await next(cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task CallChannelApiAsync(ITurnContext turnContext, NextDelegate next, ChannelApiArgs invokeArgs, CancellationToken cancellationToken)
        {
            switch (invokeArgs.Method)
            {
                // Send activity(activity)
                case ChannelApiMethod.SendToConversation:
                {
                    var activityPayload = (Activity)invokeArgs.Args[0];
                    if (activityPayload.Type == ActivityTypes.EndOfConversation)
                    {
                        await ProcessEndOfConversationAsync(turnContext, next, activityPayload, cancellationToken).ConfigureAwait(false);
                        invokeArgs.Result = new ResourceResponse(id: Guid.NewGuid().ToString("N"));
                        return;
                    }

                    invokeArgs.Result = await turnContext.SendActivityAsync(activityPayload, cancellationToken).ConfigureAwait(false);
                    return;
                }

                // Send activity(replyToId, activity)
                case ChannelApiMethod.ReplyToActivity:
                {
                    var activityPayload = (Activity)invokeArgs.Args[1];
                    activityPayload.ReplyToId = (string)invokeArgs.Args[0];

                    if (activityPayload.Type == ActivityTypes.EndOfConversation)
                    {
                        await ProcessEndOfConversationAsync(turnContext, next, activityPayload, cancellationToken).ConfigureAwait(false);
                        invokeArgs.Result = new ResourceResponse(id: Guid.NewGuid().ToString("N"));
                        return;
                    }

                    invokeArgs.Result = await turnContext.SendActivityAsync(activityPayload, cancellationToken).ConfigureAwait(false);
                    return;
                }

                // UpdateActivity(activity)
                case ChannelApiMethod.UpdateActivity:
                    invokeArgs.Result = await turnContext.UpdateActivityAsync((Activity)invokeArgs.Args[0], cancellationToken).ConfigureAwait(false);
                    return;

                // DeleteActivity(activityId)
                case ChannelApiMethod.DeleteActivity:
                    await turnContext.DeleteActivityAsync((string)invokeArgs.Args[0], cancellationToken).ConfigureAwait(false);
                    break;

                // SendConversationHistory(history)
                case ChannelApiMethod.SendConversationHistory:
                    invokeArgs.Result = await turnContext.Adapter.SendConversationHistoryAsync(turnContext, (Transcript)invokeArgs.Args[0]).ConfigureAwait(false);
                    break;

                // GetConversationMembers()
                case ChannelApiMethod.GetConversationMembers:
                    invokeArgs.Result = await turnContext.Adapter.GetConversationMembersAsync(turnContext, cancellationToken).ConfigureAwait(false);
                    break;

                // GetConversationPageMembers((int)pageSize, continuationToken)
                case ChannelApiMethod.GetConversationPagedMembers:
                    invokeArgs.Result = await turnContext.Adapter.GetConversationPagedMembersAsync(turnContext, (int)invokeArgs.Args[0], (string)invokeArgs.Args[1], cancellationToken).ConfigureAwait(false);
                    break;

                // DeleteConversationMember(memberId)
                case ChannelApiMethod.DeleteConversationMember:
                    await turnContext.Adapter.DeleteConversationMemberAsync(turnContext, (string)invokeArgs.Args[0], cancellationToken).ConfigureAwait(false);
                    break;

                // GetActivityMembers(activityId)
                case ChannelApiMethod.GetActivityMembers:
                    invokeArgs.Result = await turnContext.Adapter.GetActivityMembersAsync(turnContext, (string)invokeArgs.Args[0], cancellationToken).ConfigureAwait(false);
                    break;

                // UploadAttachment(attachmentData)
                case ChannelApiMethod.UploadAttachment:
                    invokeArgs.Result = await turnContext.Adapter.UploadAttachment(turnContext, (AttachmentData)invokeArgs.Args[0], cancellationToken).ConfigureAwait(false);
                    break;
            }
        }

        private async Task ProcessEndOfConversationAsync(ITurnContext turnContext, NextDelegate next, Activity activityPayload, CancellationToken cancellationToken)
        {
            // transform the turnContext.Activity to be the EndOfConversation and pass up to the bot, we would set the Activity, but it only has a get;
            var endOfConversation = activityPayload.AsEndOfConversationActivity();
            turnContext.Activity.Type = endOfConversation.Type;
            turnContext.Activity.Text = endOfConversation.Text;
            turnContext.Activity.Code = endOfConversation.Code;
            turnContext.Activity.Entities = endOfConversation.Entities;
            turnContext.Activity.LocalTimestamp = endOfConversation.LocalTimestamp;
            turnContext.Activity.Timestamp = endOfConversation.Timestamp;
            turnContext.Activity.ChannelData = endOfConversation.ChannelData;
            turnContext.Activity.Properties = ((Activity)endOfConversation).Properties;
            await next(cancellationToken).ConfigureAwait(false);
        }
    }
}
