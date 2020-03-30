// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Microsoft.BotBuilderSamples.DialogSkillBot
{
    public class SkillAdapterWithErrorHandler : BotFrameworkHttpAdapter
    {
        private readonly ConcurrentDictionary<string, string> _conversationMap = new ConcurrentDictionary<string, string>();

        public SkillAdapterWithErrorHandler(IConfiguration configuration, ICredentialProvider credentialProvider, AuthenticationConfiguration authConfig, ILogger<BotFrameworkHttpAdapter> logger, ConversationState conversationState = null)
            : base(configuration, credentialProvider, authConfig, logger: logger)
        {
            OnTurnError = async (turnContext, exception) =>
            {
                // Log any leaked exception from the application.
                logger.LogError(exception, $"[OnTurnError] unhandled error : {exception.Message}");

                // Send a message to the user.
                var errorMessageText = "The skill encountered an error or bug.";
                var errorMessage = MessageFactory.Text(errorMessageText, errorMessageText, InputHints.IgnoringInput);
                await turnContext.SendActivityAsync(errorMessage);

                errorMessageText = "To continue to run this bot, please fix the bot source code.";
                errorMessage = MessageFactory.Text(errorMessageText, errorMessageText, InputHints.ExpectingInput);
                await turnContext.SendActivityAsync(errorMessage);

                if (conversationState != null)
                {
                    try
                    {
                        // Delete the conversationState for the current conversation to prevent the
                        // bot from getting stuck in a error-loop caused by being in a bad state.
                        // ConversationState should be thought of as similar to "cookie-state" for a Web page.
                        await conversationState.DeleteAsync(turnContext);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"Exception caught on attempting to Delete ConversationState : {ex}");
                    }
                }

                // Send an EndOfConversation activity to the skill caller with the error to end the conversation,
                // and let the caller decide what to do.
                var endOfConversation = Activity.CreateEndOfConversationActivity();
                endOfConversation.Code = "SkillError";
                endOfConversation.Text = exception.Message;
                await turnContext.SendActivityAsync(endOfConversation);

                // Send a trace activity, which will be displayed in the Bot Framework Emulator.
                // Note: we return the entire exception in the value property to help the developer;
                // this should not be done in production.
                await turnContext.TraceActivityAsync("OnTurnError Trace", exception.ToString(), "https://www.botframework.com/schemas/error", "TurnError");
            };
        }

        public override async Task<InvokeResponse> ProcessActivityAsync(ClaimsIdentity claimsIdentity, Activity activity, BotCallbackHandler callback, CancellationToken cancellationToken)
        {
            if (claimsIdentity.Claims.All(x => x.Type != "azp"))
            {
                if (_conversationMap.TryGetValue(activity.Conversation.Id, out var appId))
                {
                    claimsIdentity.AddClaim(new Claim("azp", appId));
                    claimsIdentity.AddClaim(new Claim("ver", "2.0"));
                }
            }
            else
            {
                var appId = JwtTokenValidation.GetAppIdFromClaims(claimsIdentity.Claims);
                _conversationMap.AddOrUpdate(activity.Conversation.Id, appId, (id, a) => appId);
            }

            if (activity.Type == ActivityTypes.EndOfConversation)
            {
                _conversationMap.TryRemove(activity.Conversation.Id, out var conversation);
            }

            return await base.ProcessActivityAsync(claimsIdentity, activity, callback, cancellationToken).ConfigureAwait(false);
        }
    }
}
