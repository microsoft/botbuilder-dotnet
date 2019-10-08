using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Configuration;

namespace SkillHost.Controllers
{
    /// <summary>
    /// Handles InvokeActivity for ChannelAPI method calls coming from SkillHostController.
    /// </summary>
    public class ChannelAPIMiddleware : IMiddleware
    {
        public ChannelAPIMiddleware()
        {
        }

        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default)
        {
            if (turnContext.Activity.Type == ActivityTypes.Invoke && turnContext.Activity.Name == "ChannelAPI")
            {
                var invokeActivity = turnContext.Activity.AsInvokeActivity();
                var invokeArgs = invokeActivity.Value as ChannelAPIArgs;

                // TODO This needs to be more robust to get bot id
                await CallChannelAPI(turnContext, next, invokeArgs, cancellationToken);
                return;
            }
            else
            {
                // just pass through
                await next(cancellationToken);
                return;
            }
        }

        private async Task CallChannelAPI(ITurnContext turnContext, NextDelegate next, ChannelAPIArgs invokeArgs, CancellationToken cancellationToken)
        {
            switch (invokeArgs.Method)
            {
                /// <summary>
                /// Send activity(activity)
                /// </summary>
                case ChannelAPIMethod.SendToConversation:
                    {
                        Activity activityPayload = (Activity)invokeArgs.Args[0];
                        if (activityPayload.Type == ActivityTypes.EndOfConversation)
                        {
                            await this.ProcessEndOfConversation(turnContext, next, activityPayload, cancellationToken).ConfigureAwait(false);
                            invokeArgs.Result = new ResourceResponse(id: Guid.NewGuid().ToString("N"));
                            return;
                        }

                        invokeArgs.Result = await turnContext.SendActivityAsync(activityPayload, cancellationToken).ConfigureAwait(false);
                        return;
                    }

                /// <summary>
                /// Send activity(replyToId, activity)
                /// </summary>
                case ChannelAPIMethod.ReplyToActivity:
                    {
                        Activity activityPayload = (Activity)invokeArgs.Args[1];
                        activityPayload.ReplyToId = (string)invokeArgs.Args[0];

                        if (activityPayload.Type == ActivityTypes.EndOfConversation)
                        {
                            await this.ProcessEndOfConversation(turnContext, next, activityPayload, cancellationToken).ConfigureAwait(false);
                            invokeArgs.Result = new ResourceResponse(id: Guid.NewGuid().ToString("N"));
                            return;
                        }

                        invokeArgs.Result = await turnContext.SendActivityAsync(activityPayload, cancellationToken).ConfigureAwait(false);
                        return;
                    }

                /// <summary>
                /// UpdateActivity(activity)
                /// </summary>
                case ChannelAPIMethod.UpdateActivity:
                    invokeArgs.Result = await turnContext.UpdateActivityAsync((Activity)invokeArgs.Args[0], cancellationToken);
                    return;

                /// <summary>
                /// DeleteActivity(activityId)
                /// </summary>
                case ChannelAPIMethod.DeleteActivity:
                    await turnContext.DeleteActivityAsync((string)invokeArgs.Args[0], cancellationToken);
                    break;

                /// <summary>
                /// SendConversationHistory(history)
                /// </summary>
                case ChannelAPIMethod.SendConversationHistory:
                    invokeArgs.Result = await turnContext.Adapter.SendConversationHistoryAsync(turnContext, (Transcript)invokeArgs.Args[0]);
                    break;

                /// <summary>
                /// GetConversationMembers()
                /// </summary>
                case ChannelAPIMethod.GetConversationMembers:
                    invokeArgs.Result = await turnContext.Adapter.GetConversationMembersAsync(turnContext, cancellationToken);
                    break;

                /// <summary>
                /// GetConversationPageMembers((int)pageSize, continuationToken)
                /// </summary>
                case ChannelAPIMethod.GetConversationPagedMembers:
                    invokeArgs.Result = await turnContext.Adapter.GetConversationPagedMembersAsync(turnContext, (int)invokeArgs.Args[0], (string)invokeArgs.Args[1], cancellationToken);
                    break;

                /// <summary>
                /// DeleteConversationMember(memberId)
                /// </summary>
                case ChannelAPIMethod.DeleteConversationMember:
                    await turnContext.Adapter.DeleteConversationMemberAsync(turnContext, (string)invokeArgs.Args[0], cancellationToken);
                    break;

                /// <summary>
                /// GetActivityMembers(activityId)
                /// </summary>
                case ChannelAPIMethod.GetActivityMembers:
                    invokeArgs.Result = await turnContext.Adapter.GetActivityMembersAsync(turnContext, (string)invokeArgs.Args[0], cancellationToken);
                    break;

                /// <summary>
                /// UploadAttachment(attachmentData)
                /// </summary>
                case ChannelAPIMethod.UploadAttachment:
                    invokeArgs.Result = await turnContext.Adapter.UploadAttachment(turnContext, (AttachmentData)invokeArgs.Args[0], cancellationToken);
                    break;
            }

            return;
        }

        private async Task ProcessEndOfConversation(ITurnContext turnContext, NextDelegate next, Activity activityPayload, CancellationToken cancellationToken)
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
