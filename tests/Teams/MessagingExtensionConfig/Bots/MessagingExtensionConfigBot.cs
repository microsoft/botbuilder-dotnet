// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json.Linq;

namespace Microsoft.BotBuilderSamples.Bots
{
    public class MessagingExtensionConfigBot : TeamsActivityHandler
    {
        /*
         * After uploading the manifest you can click the dots in the extension menu at the bottom, or search for the
         * exntesion in the command bar. From the extension window or the command bar you can click on the 3 dots on the specific extension to trigger
         * the OnMessageActivityAsync function. If you click on the "Settings" tab you will fire the OnTeamsMessagingExtensionConfigurationSettingAsync 
         * function.
         */
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync(MessageFactory.Text($"echo: {turnContext.Activity.Text}"), cancellationToken);
        }

        protected override async Task<MessagingExtensionResponse> OnTeamsMessagingExtensionConfigurationQuerySettingUrlAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionQuery query, CancellationToken cancellationToken)
        {
            var messageExtensionResponse = new MessagingExtensionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    Type = "config",
                    SuggestedActions = new MessagingExtensionSuggestedAction
                    {
                        Actions = new List<CardAction>
                        {
                            new CardAction
                            {
                                Type = ActionTypes.OpenUrl,
                                Value = "https://teamssettingspagescenario.azurewebsites.net",
                            },
                        },
                    },
                },
            };

            return messageExtensionResponse;
        }

        /// <inheritdoc/>
        protected override async Task OnTeamsMessagingExtensionConfigurationSettingAsync(ITurnContext<IInvokeActivity> turnContext, JObject settings, CancellationToken cancellationToken)
        {
            // This event is fired when the settings page is submitted
            var reply = MessageFactory.Text($"OnTeamsMessagingExtensionConfigurationSettingAsync event fired with {settings}");
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }
    }
}
