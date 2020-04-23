// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Teams
{
    public class TeamsActivityHandler : ActivityHandler
    {
        protected override async Task<InvokeResponse> OnInvokeActivityAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            try
            {
                if (turnContext.Activity.Name == null && turnContext.Activity.ChannelId == Channels.Msteams)
                {
                    return await OnTeamsCardActionInvokeAsync(turnContext, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    switch (turnContext.Activity.Name)
                    {
                        case "fileConsent/invoke":
                            return await OnTeamsFileConsentAsync(turnContext, SafeCast<FileConsentCardResponse>(turnContext.Activity.Value), cancellationToken).ConfigureAwait(false);

                        case "actionableMessage/executeAction":
                            await OnTeamsO365ConnectorCardActionAsync(turnContext, SafeCast<O365ConnectorCardActionQuery>(turnContext.Activity.Value), cancellationToken).ConfigureAwait(false);
                            return CreateInvokeResponse();

                        case "composeExtension/queryLink":
                            return CreateInvokeResponse(await OnTeamsAppBasedLinkQueryAsync(turnContext, SafeCast<AppBasedLinkQuery>(turnContext.Activity.Value), cancellationToken).ConfigureAwait(false));

                        case "composeExtension/query":
                            return CreateInvokeResponse(await OnTeamsMessagingExtensionQueryAsync(turnContext, SafeCast<MessagingExtensionQuery>(turnContext.Activity.Value), cancellationToken).ConfigureAwait(false));

                        case "composeExtension/selectItem":
                            return CreateInvokeResponse(await OnTeamsMessagingExtensionSelectItemAsync(turnContext, turnContext.Activity.Value as JObject, cancellationToken).ConfigureAwait(false));

                        case "composeExtension/submitAction":
                            return CreateInvokeResponse(await OnTeamsMessagingExtensionSubmitActionDispatchAsync(turnContext, SafeCast<MessagingExtensionAction>(turnContext.Activity.Value), cancellationToken).ConfigureAwait(false));

                        case "composeExtension/fetchTask":
                            return CreateInvokeResponse(await OnTeamsMessagingExtensionFetchTaskAsync(turnContext, SafeCast<MessagingExtensionAction>(turnContext.Activity.Value), cancellationToken).ConfigureAwait(false));

                        case "composeExtension/querySettingUrl":
                            return CreateInvokeResponse(await OnTeamsMessagingExtensionConfigurationQuerySettingUrlAsync(turnContext, SafeCast<MessagingExtensionQuery>(turnContext.Activity.Value), cancellationToken).ConfigureAwait(false));

                        case "composeExtension/setting":
                            await OnTeamsMessagingExtensionConfigurationSettingAsync(turnContext, turnContext.Activity.Value as JObject, cancellationToken).ConfigureAwait(false);
                            return CreateInvokeResponse();

                        case "composeExtension/onCardButtonClicked":
                            await OnTeamsMessagingExtensionCardButtonClickedAsync(turnContext, turnContext.Activity.Value as JObject, cancellationToken).ConfigureAwait(false);
                            return CreateInvokeResponse();

                        case "task/fetch":
                            return CreateInvokeResponse(await OnTeamsTaskModuleFetchAsync(turnContext, SafeCast<TaskModuleRequest>(turnContext.Activity.Value), cancellationToken).ConfigureAwait(false));

                        case "task/submit":
                            return CreateInvokeResponse(await OnTeamsTaskModuleSubmitAsync(turnContext, SafeCast<TaskModuleRequest>(turnContext.Activity.Value), cancellationToken).ConfigureAwait(false));

                        default:
                            return await base.OnInvokeActivityAsync(turnContext, cancellationToken).ConfigureAwait(false);
                    }
                }
            }
            catch (InvokeResponseException e)
            {
                return e.CreateInvokeResponse();
            }
        }

        protected virtual Task<InvokeResponse> OnTeamsCardActionInvokeAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            throw new InvokeResponseException(HttpStatusCode.NotImplemented);
        }

        protected override Task OnSignInInvokeAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            return OnTeamsSigninVerifyStateAsync(turnContext, cancellationToken);
        }

        protected virtual Task OnTeamsSigninVerifyStateAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            throw new InvokeResponseException(HttpStatusCode.NotImplemented);
        }

        protected virtual async Task<InvokeResponse> OnTeamsFileConsentAsync(ITurnContext<IInvokeActivity> turnContext, FileConsentCardResponse fileConsentCardResponse, CancellationToken cancellationToken)
        {
            switch (fileConsentCardResponse.Action)
            {
                case "accept":
                    await OnTeamsFileConsentAcceptAsync(turnContext, fileConsentCardResponse, cancellationToken).ConfigureAwait(false);
                    return CreateInvokeResponse();

                case "decline":
                    await OnTeamsFileConsentDeclineAsync(turnContext, fileConsentCardResponse, cancellationToken).ConfigureAwait(false);
                    return CreateInvokeResponse();

                default:
                    throw new InvokeResponseException(HttpStatusCode.BadRequest, $"{fileConsentCardResponse.Action} is not a supported Action.");
            }
        }

        protected virtual Task OnTeamsFileConsentAcceptAsync(ITurnContext<IInvokeActivity> turnContext, FileConsentCardResponse fileConsentCardResponse, CancellationToken cancellationToken)
        {
            throw new InvokeResponseException(HttpStatusCode.NotImplemented);
        }

        protected virtual Task OnTeamsFileConsentDeclineAsync(ITurnContext<IInvokeActivity> turnContext, FileConsentCardResponse fileConsentCardResponse, CancellationToken cancellationToken)
        {
            throw new InvokeResponseException(HttpStatusCode.NotImplemented);
        }

        protected virtual Task<MessagingExtensionResponse> OnTeamsMessagingExtensionQueryAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionQuery query, CancellationToken cancellationToken)
        {
            throw new InvokeResponseException(HttpStatusCode.NotImplemented);
        }

        protected virtual Task OnTeamsO365ConnectorCardActionAsync(ITurnContext<IInvokeActivity> turnContext, O365ConnectorCardActionQuery query, CancellationToken cancellationToken)
        {
            throw new InvokeResponseException(HttpStatusCode.NotImplemented);
        }

        protected virtual Task<MessagingExtensionResponse> OnTeamsAppBasedLinkQueryAsync(ITurnContext<IInvokeActivity> turnContext, AppBasedLinkQuery query, CancellationToken cancellationToken)
        {
            throw new InvokeResponseException(HttpStatusCode.NotImplemented);
        }

        protected virtual Task<MessagingExtensionResponse> OnTeamsMessagingExtensionSelectItemAsync(ITurnContext<IInvokeActivity> turnContext, JObject query, CancellationToken cancellationToken)
        {
            throw new InvokeResponseException(HttpStatusCode.NotImplemented);
        }

        protected virtual Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionFetchTaskAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
        {
            throw new InvokeResponseException(HttpStatusCode.NotImplemented);
        }

        protected virtual async Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionSubmitActionDispatchAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(action.BotMessagePreviewAction))
            {
                switch (action.BotMessagePreviewAction)
                {
                    case "edit":
                        return await OnTeamsMessagingExtensionBotMessagePreviewEditAsync(turnContext, action, cancellationToken).ConfigureAwait(false);

                    case "send":
                        return await OnTeamsMessagingExtensionBotMessagePreviewSendAsync(turnContext, action, cancellationToken).ConfigureAwait(false);

                    default:
                        throw new InvokeResponseException(HttpStatusCode.BadRequest, $"{action.BotMessagePreviewAction} is not a supported BotMessagePreviewAction.");
                }
            }
            else
            {
                return await OnTeamsMessagingExtensionSubmitActionAsync(turnContext, action, cancellationToken).ConfigureAwait(false);
            }
        }

        protected virtual Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionSubmitActionAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
        {
            throw new InvokeResponseException(HttpStatusCode.NotImplemented);
        }

        protected virtual Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionBotMessagePreviewEditAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
        {
            throw new InvokeResponseException(HttpStatusCode.NotImplemented);
        }

        protected virtual Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionBotMessagePreviewSendAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
        {
            throw new InvokeResponseException(HttpStatusCode.NotImplemented);
        }

        protected virtual Task<MessagingExtensionResponse> OnTeamsMessagingExtensionConfigurationQuerySettingUrlAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionQuery query, CancellationToken cancellationToken)
        {
            throw new InvokeResponseException(HttpStatusCode.NotImplemented);
        }

        protected virtual Task OnTeamsMessagingExtensionConfigurationSettingAsync(ITurnContext<IInvokeActivity> turnContext, JObject settings, CancellationToken cancellationToken)
        {
            throw new InvokeResponseException(HttpStatusCode.NotImplemented);
        }

        protected virtual Task OnTeamsMessagingExtensionCardButtonClickedAsync(ITurnContext<IInvokeActivity> turnContext, JObject cardData, CancellationToken cancellationToken)
        {
            throw new InvokeResponseException(HttpStatusCode.NotImplemented);
        }

        protected virtual Task<TaskModuleResponse> OnTeamsTaskModuleFetchAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            throw new InvokeResponseException(HttpStatusCode.NotImplemented);
        }

        protected virtual Task<TaskModuleResponse> OnTeamsTaskModuleSubmitAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            throw new InvokeResponseException(HttpStatusCode.NotImplemented);
        }

        protected override Task OnConversationUpdateActivityAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.ChannelId == Channels.Msteams)
            {
                var channelData = turnContext.Activity.GetChannelData<TeamsChannelData>();

                if (turnContext.Activity.MembersAdded != null)
                {
                    return OnTeamsMembersAddedDispatchAsync(turnContext.Activity.MembersAdded, channelData?.Team, turnContext, cancellationToken);
                }

                if (turnContext.Activity.MembersRemoved != null)
                {
                    return OnTeamsMembersRemovedDispatchAsync(turnContext.Activity.MembersRemoved, channelData?.Team, turnContext, cancellationToken);
                }

                if (channelData != null)
                {
                    switch (channelData.EventType)
                    {
                        case "channelCreated":
                            return OnTeamsChannelCreatedAsync(channelData.Channel, channelData.Team, turnContext, cancellationToken);

                        case "channelDeleted":
                            return OnTeamsChannelDeletedAsync(channelData.Channel, channelData.Team, turnContext, cancellationToken);

                        case "channelRenamed":
                            return OnTeamsChannelRenamedAsync(channelData.Channel, channelData.Team, turnContext, cancellationToken);

                        case "teamRenamed":
                            return OnTeamsTeamRenamedAsync(channelData.Team, turnContext, cancellationToken);

                        default:
                            return base.OnConversationUpdateActivityAsync(turnContext, cancellationToken);
                    }
                }
            }

            return base.OnConversationUpdateActivityAsync(turnContext, cancellationToken);
        }

        protected virtual async Task OnTeamsMembersAddedDispatchAsync(IList<ChannelAccount> membersAdded, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var teamsMembersAdded = new List<TeamsChannelAccount>();
            foreach (var memberAdded in membersAdded)
            {
                if (memberAdded.Properties.HasValues)
                {
                    // when the ChannelAccount object is fully a TeamsChannelAccount (when Teams changes the service to return the full details)
                    teamsMembersAdded.Add(JObject.FromObject(memberAdded).ToObject<TeamsChannelAccount>());
                }
                else
                {
                    TeamsChannelAccount newMemberInfo = null;
                    try
                    {
                        newMemberInfo = await TeamsInfo.GetMemberAsync(turnContext, memberAdded.Id, cancellationToken).ConfigureAwait(false);
                    }
                    catch (ErrorResponseException ex)
                    {
                        if (ex.Body?.Error?.Code != "ConversationNotFound")
                        {
                            throw;
                        }

                        // unable to find the member added in ConversationUpdate Activity in the response from the GetMemberAsync call
                        newMemberInfo = new TeamsChannelAccount
                        {
                            Id = memberAdded.Id,
                            Name = memberAdded.Name,
                            AadObjectId = memberAdded.AadObjectId,
                            Role = memberAdded.Role,
                        };
                    }

                    teamsMembersAdded.Add(newMemberInfo);
                }
            }

            await OnTeamsMembersAddedAsync(teamsMembersAdded, teamInfo, turnContext, cancellationToken).ConfigureAwait(false);
        }

        protected virtual Task OnTeamsMembersRemovedDispatchAsync(IList<ChannelAccount> membersRemoved, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var teamsMembersRemoved = new List<TeamsChannelAccount>();
            foreach (var memberRemoved in membersRemoved)
            {
                teamsMembersRemoved.Add(JObject.FromObject(memberRemoved).ToObject<TeamsChannelAccount>());
            }

            return OnTeamsMembersRemovedAsync(teamsMembersRemoved, teamInfo, turnContext, cancellationToken);
        }

        protected virtual Task OnTeamsMembersAddedAsync(IList<TeamsChannelAccount> teamsMembersAdded, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            return OnMembersAddedAsync(teamsMembersAdded.Cast<ChannelAccount>().ToList(), turnContext, cancellationToken);
        }

        protected virtual Task OnTeamsMembersRemovedAsync(IList<TeamsChannelAccount> teamsMembersRemoved, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            return OnMembersRemovedAsync(teamsMembersRemoved.Cast<ChannelAccount>().ToList(), turnContext, cancellationToken);
        }

        protected virtual Task OnTeamsChannelCreatedAsync(ChannelInfo channelInfo, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnTeamsChannelDeletedAsync(ChannelInfo channelInfo, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnTeamsChannelRenamedAsync(ChannelInfo channelInfo, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnTeamsTeamRenamedAsync(TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private static T SafeCast<T>(object value)
        {
            var obj = value as JObject;
            if (obj == null)
            {
                throw new InvokeResponseException(HttpStatusCode.BadRequest, $"expected type '{value.GetType().Name}'");
            }

            return obj.ToObject<T>();
        }
    }
}
