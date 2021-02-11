// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Middleware to enable debugging the state of a bot.
    /// </summary>
    public class InspectionMiddleware : InterceptionMiddleware
    {
        private const string Command = "/INSPECT";

        private readonly InspectionState _inspectionState;
        private readonly UserState _userState;
        private readonly ConversationState _conversationState;
        private readonly MicrosoftAppCredentials _credentials;
        private readonly Lazy<HttpClient> _httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="InspectionMiddleware"/> class.
        /// </summary>
        /// <param name="inspectionState">A state management object for inspection state.</param>
        /// <param name="userState">A state management object for user state.</param>
        /// <param name="conversationState">A state management object for conversation state.</param>
        /// <param name="credentials">The authentication credentials.</param>
        /// <param name="logger">A logger.</param>
        public InspectionMiddleware(InspectionState inspectionState, UserState userState = null, ConversationState conversationState = null, MicrosoftAppCredentials credentials = null, ILogger<InspectionMiddleware> logger = null)
            : base(logger)
        {
            _inspectionState = inspectionState ?? throw new ArgumentNullException(nameof(inspectionState));
            _userState = userState;
            _conversationState = conversationState;
            _credentials = credentials ?? MicrosoftAppCredentials.Empty;
            _httpClient = new Lazy<HttpClient>(() => new HttpClient());
        }

        /// <summary>
        /// Indentifies open and attach commands and calls the appropriate method.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>true if the command is open or attach, otherwise false.</returns>
        public async Task<bool> ProcessCommandAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext.Activity.Type == ActivityTypes.Message && turnContext.Activity.Text != null)
            {
                var originalText = turnContext.Activity.Text;
                turnContext.Activity.RemoveRecipientMention();

                var command = turnContext.Activity.Text.Trim().Split(' ');
                if (command.Length > 1 && command[0] == Command)
                {
                    if (command.Length == 2 && command[1] == "open")
                    {
                        await ProcessOpenCommandAsync(turnContext, cancellationToken).ConfigureAwait(false);
                        return true;
                    }

                    if (command.Length == 3 && command[1] == "attach")
                    {
                        await ProcessAttachCommandAsync(turnContext, command[2], cancellationToken).ConfigureAwait(false);
                        return true;
                    }
                }

                turnContext.Activity.Text = originalText;
            }

            return false;
        }

        /// <summary>
        /// Gets the HTTP client for the current object.
        /// </summary>
        /// <returns>The HTTP client for the current object.</returns>
        protected virtual HttpClient GetHttpClient()
        {
            return _httpClient.Value;
        }

        /// <summary>
        /// Processes inbound activities.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="traceActivity">The trace activity.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected override async Task<(bool shouldForwardToApplication, bool shouldIntercept)> InboundAsync(ITurnContext turnContext, Activity traceActivity, CancellationToken cancellationToken)
        {
            if (await ProcessCommandAsync(turnContext, cancellationToken).ConfigureAwait(false))
            {
                return (shouldForwardToApplication: false, shouldIntercept: false);
            }

            var session = await FindSessionAsync(turnContext, cancellationToken).ConfigureAwait(false);
            if (session != null)
            {
                if (await InvokeSendAsync(turnContext, session, traceActivity, cancellationToken).ConfigureAwait(false))
                {
                    return (shouldForwardToApplication: true, shouldIntercept: true);
                }
                else
                {
                    return (shouldForwardToApplication: true, shouldIntercept: false);
                }
            }
            else
            {
                return (shouldForwardToApplication: true, shouldIntercept: false);
            }
        }

        /// <summary>
        /// Processes outbound activities.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="traceActivities">A collection of trace activities.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected override async Task OutboundAsync(ITurnContext turnContext, IEnumerable<Activity> traceActivities, CancellationToken cancellationToken)
        {
            var session = await FindSessionAsync(turnContext, cancellationToken).ConfigureAwait(false);
            if (session != null)
            {
                foreach (var traceActivity in traceActivities)
                {
                    if (!await InvokeSendAsync(turnContext, session, traceActivity, cancellationToken).ConfigureAwait(false))
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Processes the state management object.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected override async Task TraceStateAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var session = await FindSessionAsync(turnContext, cancellationToken).ConfigureAwait(false);
            if (session != null && (_userState != null || _conversationState != null))
            {
                var task1 = _userState?.LoadAsync(turnContext, false, cancellationToken) ?? Task.CompletedTask;
                var task2 = _conversationState?.LoadAsync(turnContext, false, cancellationToken) ?? Task.CompletedTask;
                await Task.WhenAll(task1, task2).ConfigureAwait(false);

                var botState = new JObject();

                if (_userState != null)
                {
                    botState.Add("userState", _userState.Get(turnContext));
                }

                if (_conversationState != null)
                {
                    botState.Add("conversationState", _conversationState.Get(turnContext));
                }

                await InvokeSendAsync(turnContext, session, botState.TraceActivity(), cancellationToken).ConfigureAwait(false);
            }
        }

        private static string OpenCommand(InspectionSessionsByStatus sessions, ConversationReference conversationReference)
        {
            var sessionId = Guid.NewGuid().ToString();
            sessions.OpenedSessions.Add(sessionId, conversationReference);
            return sessionId;
        }

        private static bool AttachCommand(string attachId, InspectionSessionsByStatus sessions, string sessionId)
        {
            if (sessions.OpenedSessions.TryGetValue(sessionId, out var inspectionSessionState))
            {
                sessions.AttachedSessions[attachId] = inspectionSessionState;
                sessions.OpenedSessions.Remove(sessionId);
                return true;
            }

            return false;
        }

        private static string GetAttachId(Activity activity)
        {
            // If we are running in a Microsoft Teams Team the conversation Id will reflect a particular thread the bot is in.
            // So if we are in a Team then we will associate the "attach" with the Team Id rather than the more restrictive conversation Id.
            return activity.TeamsGetTeamInfo()?.Id ?? activity.Conversation.Id;
        }

        private async Task ProcessOpenCommandAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var accessor = _inspectionState.CreateProperty<InspectionSessionsByStatus>(nameof(InspectionSessionsByStatus));
            var sessions = await accessor.GetAsync(turnContext, () => new InspectionSessionsByStatus()).ConfigureAwait(false);
            var sessionId = OpenCommand(sessions, turnContext.Activity.GetConversationReference());
            await turnContext.SendActivityAsync($"{Command} attach {sessionId}".MakeCommandActivity()).ConfigureAwait(false);
            await _inspectionState.SaveChangesAsync(turnContext, false, cancellationToken).ConfigureAwait(false);
        }

        private async Task ProcessAttachCommandAsync(ITurnContext turnContext, string sessionId, CancellationToken cancellationToken)
        {
            var accessor = _inspectionState.CreateProperty<InspectionSessionsByStatus>(nameof(InspectionSessionsByStatus));
            var sessions = await accessor.GetAsync(turnContext, () => new InspectionSessionsByStatus()).ConfigureAwait(false);

            if (AttachCommand(GetAttachId(turnContext.Activity), sessions, sessionId))
            {
                if (turnContext.Activity.TeamsGetTeamInfo()?.Id == null)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Attached to session, all traffic is being replicated for inspection."), cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Attached to session, all traffic, directed to this bot, within this Team, is being replicated for inspection."), cancellationToken).ConfigureAwait(false);
                }
            }
            else
            {
                await turnContext.SendActivityAsync(MessageFactory.Text($"Open session with id {sessionId} does not exist."), cancellationToken).ConfigureAwait(false);
            }

            await _inspectionState.SaveChangesAsync(turnContext, false, cancellationToken).ConfigureAwait(false);
        }

        private async Task<InspectionSession> FindSessionAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var accessor = _inspectionState.CreateProperty<InspectionSessionsByStatus>(nameof(InspectionSessionsByStatus));
            var openSessions = await accessor.GetAsync(turnContext, () => new InspectionSessionsByStatus(), cancellationToken).ConfigureAwait(false);

            if (openSessions.AttachedSessions.TryGetValue(GetAttachId(turnContext.Activity), out var conversationReference))
            {
                return new InspectionSession(conversationReference, _credentials, GetHttpClient(), Logger);
            }

            return null;
        }

        private async Task<bool> InvokeSendAsync(ITurnContext turnContext, InspectionSession session, Activity activity, CancellationToken cancellationToken)
        {
            if (await session.SendAsync(activity, cancellationToken).ConfigureAwait(false))
            {
                return true;
            }
            else
            {
                await CleanUpSessionAsync(turnContext, cancellationToken).ConfigureAwait(false);
                return false;
            }
        }

        private async Task CleanUpSessionAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var accessor = _inspectionState.CreateProperty<InspectionSessionsByStatus>(nameof(InspectionSessionsByStatus));
            var openSessions = await accessor.GetAsync(turnContext, () => new InspectionSessionsByStatus(), cancellationToken).ConfigureAwait(false);
            openSessions.AttachedSessions.Remove(GetAttachId(turnContext.Activity));
            await _inspectionState.SaveChangesAsync(turnContext, false, cancellationToken).ConfigureAwait(false);
        }
    }
}
