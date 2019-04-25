// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Builder
{
    public class InspectionMiddleware : InterceptionMiddleware
    {
        private const string Command = "/INSPECT";

        private readonly InspectionState _inspectionState;
        private readonly UserState _userState;
        private readonly ConversationState _conversationState;
        private readonly MicrosoftAppCredentials _credentials;
        private readonly Lazy<HttpClient> _httpClient;

        public InspectionMiddleware(InspectionState inspectionState, UserState userState = null, ConversationState conversationState = null, MicrosoftAppCredentials credentials = null, ILogger<InspectionMiddleware> logger = null)
            : base(logger)
        {
            _inspectionState = inspectionState ?? throw new ArgumentNullException(nameof(inspectionState));
            _userState = userState;
            _conversationState = conversationState;
            _credentials = credentials ?? MicrosoftAppCredentials.Empty;
            _httpClient = new Lazy<HttpClient>(() => new HttpClient());
        }

        public async Task<bool> ProcessCommandAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                var command = turnContext.Activity.Text.Split(' ');
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
            }

            return false;
        }

        protected virtual HttpClient GetHttpClient()
        {
            return _httpClient.Value;
        }

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

        protected override async Task TraceStateAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var session = await FindSessionAsync(turnContext, cancellationToken).ConfigureAwait(false);
            if (session != null)
            {
                var task1 = _userState?.LoadAsync(turnContext, false, cancellationToken) ?? Task.CompletedTask;
                var task2 = _conversationState?.LoadAsync(turnContext, false, cancellationToken) ?? Task.CompletedTask;
                await Task.WhenAll(task1, task2).ConfigureAwait(false);

                if (_userState != null)
                {
                    await InvokeSendAsync(turnContext, session, _userState.TraceActivity(turnContext), cancellationToken).ConfigureAwait(false);
                }

                if (_conversationState != null)
                {
                    await InvokeSendAsync(turnContext, session, _conversationState.TraceActivity(turnContext), cancellationToken).ConfigureAwait(false);
                }
            }
        }

        private async Task ProcessOpenCommandAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var accessor = _inspectionState.CreateProperty<InspectionSessionsByStatus>(nameof(InspectionSessionsByStatus));
            var sessions = await accessor.GetAsync(turnContext, () => new InspectionSessionsByStatus()).ConfigureAwait(false);
            var sessionId = OpenCommand(sessions, turnContext.Activity.GetConversationReference());
            await turnContext.SendActivityAsync(MessageFactory.Text($"{Command} attach {sessionId}")).ConfigureAwait(false);
            await _inspectionState.SaveChangesAsync(turnContext, false, cancellationToken).ConfigureAwait(false);
        }

        private async Task ProcessAttachCommandAsync(ITurnContext turnContext, string sessionId, CancellationToken cancellationToken)
        {
            var accessor = _inspectionState.CreateProperty<InspectionSessionsByStatus>(nameof(InspectionSessionsByStatus));
            var sessions = await accessor.GetAsync(turnContext, () => new InspectionSessionsByStatus()).ConfigureAwait(false);

            if (AttachCommand(turnContext.Activity.Conversation.Id, sessions, sessionId))
            {
                await turnContext.SendActivityAsync(MessageFactory.Text($"Attached to session, all traffic is being relicated for inspection."), cancellationToken).ConfigureAwait(false);
            }
            else
            {
                await turnContext.SendActivityAsync(MessageFactory.Text($"Open session with id {sessionId} does not exist."), cancellationToken).ConfigureAwait(false);
            }

            await _inspectionState.SaveChangesAsync(turnContext, false, cancellationToken).ConfigureAwait(false);
        }

        private string OpenCommand(InspectionSessionsByStatus sessions, ConversationReference conversationReference)
        {
            var sessionId = Guid.NewGuid().ToString();
            sessions.OpenedSessions.Add(sessionId, conversationReference);
            return sessionId;
        }

        private bool AttachCommand(string conversationId, InspectionSessionsByStatus sessions, string sessionId)
        {
            if (sessions.OpenedSessions.TryGetValue(sessionId, out var inspectionSessionState))
            {
                sessions.AttachedSessions[conversationId] = inspectionSessionState;
                sessions.OpenedSessions.Remove(sessionId);
                return true;
            }

            return false;
        }

        private async Task<InspectionSession> FindSessionAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var accessor = _inspectionState.CreateProperty<InspectionSessionsByStatus>(nameof(InspectionSessionsByStatus));
            var openSessions = await accessor.GetAsync(turnContext, () => new InspectionSessionsByStatus(), cancellationToken).ConfigureAwait(false);

            if (openSessions.AttachedSessions.TryGetValue(turnContext.Activity.Conversation.Id, out var conversationReference))
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
            openSessions.AttachedSessions.Remove(turnContext.Activity.Conversation.Id);
            await _inspectionState.SaveChangesAsync(turnContext, false, cancellationToken).ConfigureAwait(false);
        }
    }
}
