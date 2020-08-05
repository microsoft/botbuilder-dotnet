// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Microsoft.BotBuilderSamples.TeamsSkillBot
{
    public class SkillAdapterWithErrorHandler : BotFrameworkHttpAdapter
    {
        private readonly IConfiguration _configuration;

        public SkillAdapterWithErrorHandler(IConfiguration configuration, ICredentialProvider credentialProvider, AuthenticationConfiguration authConfig, ILogger<BotFrameworkHttpAdapter> logger, ConversationState conversationState = null)
            : base(configuration, credentialProvider, authConfig, logger: logger)
        {
            _configuration = configuration;
            OnTurnError = async (turnContext, exception) =>
            {
                // Log any leaked exception from the application.
                logger.LogError(exception, $"[OnTurnError] unhandled error : {exception.Message}");

                // Send a message to the user
                var errorMessageText = "The skill encountered an error or bug.";
                var errorMessage = MessageFactory.Text(errorMessageText, errorMessageText, InputHints.IgnoringInput);
                await turnContext.SendActivityAsync(errorMessage);

                errorMessageText = "To continue to run this bot, please fix the bot source code.";
                errorMessage = MessageFactory.Text(errorMessageText, errorMessageText, InputHints.ExpectingInput);
                await turnContext.SendActivityAsync(errorMessage);

                // Send a trace activity, which will be displayed in the Bot Framework Emulator
                // Note: we return the entire exception in the value property to help the developer, this should not be done in prod.
                await turnContext.TraceActivityAsync("OnTurnError Trace", exception.ToString(), "https://www.botframework.com/schemas/error", "TurnError");

                // Send and EndOfConversation activity to the skill caller with the error to end the conversation
                // and let the caller decide what to do.
                var endOfConversation = Activity.CreateEndOfConversationActivity();
                endOfConversation.Code = "SkillError";
                endOfConversation.Text = exception.Message;
                await turnContext.SendActivityAsync(endOfConversation);

                if (conversationState != null)
                {
                    try
                    {
                        // Delete the conversationState for the current conversation to prevent the
                        // bot from getting stuck in a error-loop caused by being in a bad state.
                        // ConversationState should be thought of as similar to "cookie-state" in a Web pages.
                        await conversationState.DeleteAsync(turnContext);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"Exception caught on attempting to Delete ConversationState : {ex}");
                    }
                }
            };
        }

        public async Task<string> GetBotSkillToken(ITurnContext turnContext)
        {
            var appId = _configuration.GetSection(MicrosoftAppCredentials.MicrosoftAppIdKey)?.Value;
            var oAuthScope = turnContext.TurnState.Get<string>(OAuthScopeKey);
            var token = await (await GetAppCredentialsAsync(appId, oAuthScope).ConfigureAwait(false)).GetTokenAsync();
            return token;
        }

        private async Task<AppCredentials> GetAppCredentialsAsync(string appId, string oAuthScope, CancellationToken cancellationToken = default)
        {
            if (appId == null)
            {
                return MicrosoftAppCredentials.Empty;
            }

            var cacheKey = $"{appId}{oAuthScope}";
            if (AppCredentialMap.TryGetValue(cacheKey, out var appCredentials))
            {
                return appCredentials;
            }

            // Credentials not found in cache, build them
            appCredentials = await BuildCredentialsAsync(appId, oAuthScope).ConfigureAwait(false);

            // Cache the credentials for later use
            AppCredentialMap[cacheKey] = appCredentials;
            return appCredentials;
        }
    }
}
