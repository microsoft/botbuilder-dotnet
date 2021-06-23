// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

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
    /// <summary>
    /// The TeamsActivityHandler is derived from ActivityHandler. It adds support for 
    /// the Microsoft Teams specific events and interactions.
    /// </summary>
    public class TeamsActivityHandler : ActivityHandler
    {
        /// <summary>
        /// Invoked when an invoke activity is received from the connector.
        /// Invoke activities can be used to communicate many different things.
        /// </summary>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>
        /// Invoke activities communicate programmatic commands from a client or channel to a bot.
        /// The meaning of an invoke activity is defined by the <see cref="IInvokeActivity.Name"/> property,
        /// which is meaningful within the scope of a channel.
        /// </remarks>
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
                        
                        case "tab/fetch":
                            return CreateInvokeResponse(await OnTeamsTabFetchAsync(turnContext, SafeCast<TabRequest>(turnContext.Activity.Value), cancellationToken).ConfigureAwait(false));
                        
                        case "tab/submit":
                            return CreateInvokeResponse(await OnTeamsTabSubmitAsync(turnContext, SafeCast<TabSubmit>(turnContext.Activity.Value), cancellationToken).ConfigureAwait(false));

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

        /// <summary>
        /// Invoked when an card action invoke activity is received from the connector.
        /// </summary>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected virtual Task<InvokeResponse> OnTeamsCardActionInvokeAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            throw new InvokeResponseException(HttpStatusCode.NotImplemented);
        }

        /// <summary>
        /// Invoked when a signIn invoke activity is received from the connector.
        /// </summary>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override Task OnSignInInvokeAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            return OnTeamsSigninVerifyStateAsync(turnContext, cancellationToken);
        }

        /// <summary>
        /// Invoked when a signIn verify state activity is received from the connector.
        /// </summary>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected virtual Task OnTeamsSigninVerifyStateAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            throw new InvokeResponseException(HttpStatusCode.NotImplemented);
        }

        /// <summary>
        /// Invoked when a file consent card activity is received from the connector.
        /// </summary>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="fileConsentCardResponse">The response representing the value of the invoke activity sent when the user acts on
        /// a file consent card.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>An InvokeResponse depending on the action of the file consent card.</returns>
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

        /// <summary>
        /// Invoked when a file consent card is accepted by the user.
        /// </summary>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="fileConsentCardResponse">The response representing the value of the invoke activity sent when the user accepts
        /// a file consent card.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected virtual Task OnTeamsFileConsentAcceptAsync(ITurnContext<IInvokeActivity> turnContext, FileConsentCardResponse fileConsentCardResponse, CancellationToken cancellationToken)
        {
            throw new InvokeResponseException(HttpStatusCode.NotImplemented);
        }

        /// <summary>
        /// Invoked when a file consent card is declined by the user.
        /// </summary>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="fileConsentCardResponse">The response representing the value of the invoke activity sent when the user declines
        /// a file consent card.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected virtual Task OnTeamsFileConsentDeclineAsync(ITurnContext<IInvokeActivity> turnContext, FileConsentCardResponse fileConsentCardResponse, CancellationToken cancellationToken)
        {
            throw new InvokeResponseException(HttpStatusCode.NotImplemented);
        }

        /// <summary>
        /// Invoked when a Messaging Extension Query activity is received from the connector.
        /// </summary>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="query">The query for the search command.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>The Messaging Extension Response for the query.</returns>
        protected virtual Task<MessagingExtensionResponse> OnTeamsMessagingExtensionQueryAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionQuery query, CancellationToken cancellationToken)
        {
            throw new InvokeResponseException(HttpStatusCode.NotImplemented);
        }

        /// <summary>
        /// Invoked when a O365 Connector Card Action activity is received from the connector.
        /// </summary>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="query">The O365 connector card HttpPOST invoke query.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected virtual Task OnTeamsO365ConnectorCardActionAsync(ITurnContext<IInvokeActivity> turnContext, O365ConnectorCardActionQuery query, CancellationToken cancellationToken)
        {
            throw new InvokeResponseException(HttpStatusCode.NotImplemented);
        }

        /// <summary>
        /// Invoked when an app based link query activity is received from the connector.
        /// </summary>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="query">The invoke request body type for app-based link query.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>The Messaging Extension Response for the query.</returns>
        protected virtual Task<MessagingExtensionResponse> OnTeamsAppBasedLinkQueryAsync(ITurnContext<IInvokeActivity> turnContext, AppBasedLinkQuery query, CancellationToken cancellationToken)
        {
            throw new InvokeResponseException(HttpStatusCode.NotImplemented);
        }

        /// <summary>
        /// Invoked when a messaging extension select item activity is received from the connector.
        /// </summary>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="query">The object representing the query.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>The Messaging Extension Response for the query.</returns>
        protected virtual Task<MessagingExtensionResponse> OnTeamsMessagingExtensionSelectItemAsync(ITurnContext<IInvokeActivity> turnContext, JObject query, CancellationToken cancellationToken)
        {
            throw new InvokeResponseException(HttpStatusCode.NotImplemented);
        }

        /// <summary>
        /// Invoked when a Messaging Extension Fetch activity is received from the connector.
        /// </summary>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="action">The messaging extension action.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>The Messaging Extension Action Response for the action.</returns>
        protected virtual Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionFetchTaskAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
        {
            throw new InvokeResponseException(HttpStatusCode.NotImplemented);
        }

        /// <summary>
        /// Invoked when a messaging extension submit action dispatch activity is received from the connector.
        /// </summary>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="action">The messaging extension action.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>The Messaging Extension Action Response for the action.</returns>
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

        /// <summary>
        /// Invoked when a messaging extension submit action activity is received from the connector.
        /// </summary>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="action">The messaging extension action.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>The Messaging Extension Action Response for the action.</returns>
        protected virtual Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionSubmitActionAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
        {
            throw new InvokeResponseException(HttpStatusCode.NotImplemented);
        }

        /// <summary>
        /// Invoked when a messaging extension bot message preview edit activity is received from the connector.
        /// </summary>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="action">The messaging extension action.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>The Messaging Extension Action Response for the action.</returns>
        protected virtual Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionBotMessagePreviewEditAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
        {
            throw new InvokeResponseException(HttpStatusCode.NotImplemented);
        }

        /// <summary>
        /// Invoked when a messaging extension bot message preview send activity is received from the connector.
        /// </summary>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="action">The messaging extension action.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>The Messaging Extension Action Response for the action.</returns>
        protected virtual Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionBotMessagePreviewSendAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
        {
            throw new InvokeResponseException(HttpStatusCode.NotImplemented);
        }

        /// <summary>
        /// Invoked when a messaging extension configuration query setting url activity is received from the connector.
        /// </summary>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="query">The Messaging extension query.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>The Messaging Extension Response for the query.</returns>
        protected virtual Task<MessagingExtensionResponse> OnTeamsMessagingExtensionConfigurationQuerySettingUrlAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionQuery query, CancellationToken cancellationToken)
        {
            throw new InvokeResponseException(HttpStatusCode.NotImplemented);
        }

        /// <summary>
        /// Override this in a derived class to provide logic for when a configuration is set for a messaging extension.
        /// </summary>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="settings">Object representing the configuration settings.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected virtual Task OnTeamsMessagingExtensionConfigurationSettingAsync(ITurnContext<IInvokeActivity> turnContext, JObject settings, CancellationToken cancellationToken)
        {
            throw new InvokeResponseException(HttpStatusCode.NotImplemented);
        }

        /// <summary>
        /// Override this in a derived class to provide logic for when a card button is clicked in a messaging extension.
        /// </summary>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="cardData">Object representing the card data.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected virtual Task OnTeamsMessagingExtensionCardButtonClickedAsync(ITurnContext<IInvokeActivity> turnContext, JObject cardData, CancellationToken cancellationToken)
        {
            throw new InvokeResponseException(HttpStatusCode.NotImplemented);
        }

        /// <summary>
        /// Override this in a derived class to provide logic for when a task module is fetched.
        /// </summary>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="taskModuleRequest">The task module invoke request value payload.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A Task Module Response for the request.</returns>
        protected virtual Task<TaskModuleResponse> OnTeamsTaskModuleFetchAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            throw new InvokeResponseException(HttpStatusCode.NotImplemented);
        }

        /// <summary>
        /// Override this in a derived class to provide logic for when a task module is submited.
        /// </summary>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="taskModuleRequest">The task module invoke request value payload.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A Task Module Response for the request.</returns>
        protected virtual Task<TaskModuleResponse> OnTeamsTaskModuleSubmitAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            throw new InvokeResponseException(HttpStatusCode.NotImplemented);
        }

        /// <summary>
        /// Override this in a derived class to provide logic for when a tab is fetched.
        /// </summary>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="tabRequest">The tab invoke request value payload.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A Tab Response for the request.</returns>
        protected virtual Task<TabResponse> OnTeamsTabFetchAsync(ITurnContext<IInvokeActivity> turnContext, TabRequest tabRequest, CancellationToken cancellationToken)
        {
            throw new InvokeResponseException(HttpStatusCode.NotImplemented);
        }

        /// <summary>
        /// Override this in a derived class to provide logic for when a tab is submitted.
        /// </summary>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="tabSubmit">The tab submit invoke request value payload.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A Tab Response for the request.</returns>
        protected virtual Task<TabResponse> OnTeamsTabSubmitAsync(ITurnContext<IInvokeActivity> turnContext, TabSubmit tabSubmit, CancellationToken cancellationToken)
        {
            throw new InvokeResponseException(HttpStatusCode.NotImplemented);
        }

        /// <summary>
        /// Invoked when a conversation update activity is received from the channel.
        /// Conversation update activities are useful when it comes to responding to users being added to or removed from the channel.
        /// For example, a bot could respond to a user being added by greeting the user.
        /// </summary>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>
        /// In a derived class, override this method to add logic that applies to all conversation update activities.
        /// </remarks>
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

                        case "channelRestored":
                            return OnTeamsChannelRestoredAsync(channelData.Channel, channelData.Team, turnContext, cancellationToken);

                        case "teamArchived":
                            return OnTeamsTeamArchivedAsync(channelData.Team, turnContext, cancellationToken);

                        case "teamDeleted":
                            return OnTeamsTeamDeletedAsync(channelData.Team, turnContext, cancellationToken);

                        case "teamHardDeleted":
                            return OnTeamsTeamHardDeletedAsync(channelData.Team, turnContext, cancellationToken);

                        case "teamRenamed":
                            return OnTeamsTeamRenamedAsync(channelData.Team, turnContext, cancellationToken);

                        case "teamRestored":
                            return OnTeamsTeamRestoredAsync(channelData.Team, turnContext, cancellationToken);

                        case "teamUnarchived":
                            return OnTeamsTeamUnarchivedAsync(channelData.Team, turnContext, cancellationToken);

                        default:
                            return base.OnConversationUpdateActivityAsync(turnContext, cancellationToken);
                    }
                }
            }

            return base.OnConversationUpdateActivityAsync(turnContext, cancellationToken);
        }

        /// <summary>
        /// Override this in a derived class to provide logic for when members other than the bot
        /// join the channel, such as your bot's welcome logic.
        /// UseIt will get the associated members with the provided accounts.
        /// </summary>
        /// <param name="membersAdded">A list of all the accounts added to the channel, as
        /// described by the conversation update activity.</param>
        /// <param name="teamInfo">The team info object representing the team.</param>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected virtual async Task OnTeamsMembersAddedDispatchAsync(IList<ChannelAccount> membersAdded, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var teamsMembersAdded = new List<TeamsChannelAccount>();
            foreach (var memberAdded in membersAdded)
            {
                if (memberAdded.Properties.HasValues || memberAdded.Id == turnContext.Activity?.Recipient?.Id)
                {
                    // when the ChannelAccount object is fully a TeamsChannelAccount, or the bot (when Teams changes the service to return the full details)
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

        /// <summary>
        /// Override this in a derived class to provide logic for when members other than the bot
        /// leave the channel, such as your bot's good-bye logic.
        /// It will get the associated members with the provided accounts.
        /// </summary>
        /// <param name="membersRemoved">A list of all the accounts removed from the channel, as
        /// described by the conversation update activity.</param>
        /// <param name="teamInfo">The team info object representing the team.</param>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected virtual Task OnTeamsMembersRemovedDispatchAsync(IList<ChannelAccount> membersRemoved, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var teamsMembersRemoved = new List<TeamsChannelAccount>();
            foreach (var memberRemoved in membersRemoved)
            {
                teamsMembersRemoved.Add(JObject.FromObject(memberRemoved).ToObject<TeamsChannelAccount>());
            }

            return OnTeamsMembersRemovedAsync(teamsMembersRemoved, teamInfo, turnContext, cancellationToken);
        }

        /// <summary>
        /// Override this in a derived class to provide logic for when members other than the bot
        /// join the channel, such as your bot's welcome logic.
        /// </summary>
        /// <param name="teamsMembersAdded">A list of all the members added to the channel, as
        /// described by the conversation update activity.</param>
        /// <param name="teamInfo">The team info object representing the team.</param>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected virtual Task OnTeamsMembersAddedAsync(IList<TeamsChannelAccount> teamsMembersAdded, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            return OnMembersAddedAsync(teamsMembersAdded.Cast<ChannelAccount>().ToList(), turnContext, cancellationToken);
        }

        /// <summary>
        /// Override this in a derived class to provide logic for when members other than the bot
        /// leave the channel, such as your bot's good-bye logic.
        /// </summary>
        /// <param name="teamsMembersRemoved">A list of all the members removed from the channel, as
        /// described by the conversation update activity.</param>
        /// <param name="teamInfo">The team info object representing the team.</param>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected virtual Task OnTeamsMembersRemovedAsync(IList<TeamsChannelAccount> teamsMembersRemoved, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            return OnMembersRemovedAsync(teamsMembersRemoved.Cast<ChannelAccount>().ToList(), turnContext, cancellationToken);
        }

        /// <summary>
        /// Invoked when a Channel Created event activity is received from the connector.
        /// Channel Created correspond to the user creating a new channel.
        /// </summary>
        /// <param name="channelInfo">The channel info object which describes the channel.</param>
        /// <param name="teamInfo">The team info object representing the team.</param>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected virtual Task OnTeamsChannelCreatedAsync(ChannelInfo channelInfo, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Invoked when a Channel Deleted event activity is received from the connector.
        /// Channel Deleted correspond to the user deleting an existing channel.
        /// </summary>
        /// <param name="channelInfo">The channel info object which describes the channel.</param>
        /// <param name="teamInfo">The team info object representing the team.</param>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected virtual Task OnTeamsChannelDeletedAsync(ChannelInfo channelInfo, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Invoked when a Channel Renamed event activity is received from the connector.
        /// Channel Renamed correspond to the user renaming an existing channel.
        /// </summary>
        /// <param name="channelInfo">The channel info object which describes the channel.</param>
        /// <param name="teamInfo">The team info object representing the team.</param>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected virtual Task OnTeamsChannelRenamedAsync(ChannelInfo channelInfo, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
        
        /// <summary>
        /// Invoked when a Channel Restored event activity is received from the connector.
        /// Channel Restored correspond to the user restoring a previously deleted channel.
        /// </summary>
        /// <param name="channelInfo">The channel info object which describes the channel.</param>
        /// <param name="teamInfo">The team info object representing the team.</param>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected virtual Task OnTeamsChannelRestoredAsync(ChannelInfo channelInfo, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Invoked when a Team Archived event activity is received from the connector.
        /// Team Archived correspond to the user archiving a team.
        /// </summary>
        /// <param name="teamInfo">The team info object representing the team.</param>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected virtual Task OnTeamsTeamArchivedAsync(TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Invoked when a Team Deleted event activity is received from the connector.
        /// Team Deleted corresponds to the user deleting a team.
        /// </summary>
        /// <param name="teamInfo">The team info object representing the team.</param>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected virtual Task OnTeamsTeamDeletedAsync(TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Invoked when a Team Hard Deleted event activity is received from the connector.
        /// Team Hard Deleted corresponds to the user hard deleting a team.
        /// </summary>
        /// <param name="teamInfo">The team info object representing the team.</param>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected virtual Task OnTeamsTeamHardDeletedAsync(TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Invoked when a Team Renamed event activity is received from the connector.
        /// Team Renamed correspond to the user renaming an existing team.
        /// </summary>
        /// <param name="teamInfo">The team info object representing the team.</param>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected virtual Task OnTeamsTeamRenamedAsync(TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Invoked when a Team Restored event activity is received from the connector.
        /// Team Restored corresponds to the user restoring a team.
        /// </summary>
        /// <param name="teamInfo">The team info object representing the team.</param>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected virtual Task OnTeamsTeamRestoredAsync(TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Invoked when a Team Unarchived event activity is received from the connector.
        /// Team Unarchived correspond to the user unarchiving a team.
        /// </summary>
        /// <param name="teamInfo">The team info object representing the team.</param>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected virtual Task OnTeamsTeamUnarchivedAsync(TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Invoked when an event activity is received from the channel.
        /// Event activities can be used to communicate many different things.
        /// </summary>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>
        /// In a derived class, override this method to add logic that applies to all event activities.
        /// </remarks>
        protected override Task OnEventActivityAsync(ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.ChannelId == Channels.Msteams)
            {
                switch (turnContext.Activity.Name)
                {
                    case "application/vnd.microsoft.meetingStart":
                        return OnTeamsMeetingStartAsync(JObject.FromObject(turnContext.Activity.Value).ToObject<MeetingStartEventDetails>(), turnContext, cancellationToken);
                    case "application/vnd.microsoft.meetingEnd":
                        return OnTeamsMeetingEndAsync(JObject.FromObject(turnContext.Activity.Value).ToObject<MeetingEndEventDetails>(), turnContext, cancellationToken);
                }
            }

            return base.OnEventActivityAsync(turnContext, cancellationToken);
        }

        /// <summary>
        /// Invoked when a Teams Meeting Start event activity is received from the connector.
        /// Override this in a derived class to provide logic for when a meeting is started.
        /// </summary>
        /// <param name="meeting">The details of the meeting.</param>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected virtual Task OnTeamsMeetingStartAsync(MeetingStartEventDetails meeting, ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Invoked when a Teams Meeting End event activity is received from the connector.
        /// Override this in a derived class to provide logic for when a meeting is ended.
        /// </summary>
        /// <param name="meeting">The details of the meeting.</param>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected virtual Task OnTeamsMeetingEndAsync(MeetingEndEventDetails meeting, ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Safely casts an object to an object of type <typeparamref name="T"/> .
        /// </summary>
        /// <param name="value">The object to be casted.</param>
        /// <returns>The object casted in the new type.</returns>
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
