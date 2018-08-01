// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Ai.Luis;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace AspNetCore_LUIS_Bot
{
    public class MyLuisBot : IBot
    {
        private const double LUIS_INTENT_THRESHOLD = 0.2d;

        private readonly MyBotAccessors _stateAccessors;
        private readonly LuisRecognizer _luisRecognizer;        
        private readonly DialogSet _dialogSet;

        public MyLuisBot(MyBotAccessors accessors, LuisRecognizer recognizer)
        {
            _stateAccessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            _luisRecognizer = recognizer ?? throw new ArgumentNullException(nameof(recognizer));

            _dialogSet = new DialogSet();

            var reminderDialog = new ReminderDialog(_stateAccessors);            
            _dialogSet.Add("AlarmDialog", reminderDialog); 
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            if (turnContext.Activity.Type == ActivityTypes.ConversationUpdate && turnContext.Activity.MembersAdded.FirstOrDefault()?.Id == turnContext.Activity.Recipient.Id)
            {
                await turnContext.SendActivityAsync("Hi! I'm a simple reminder bot. I can add reminders and show them.");
            }
            else if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                var dialogState = await turnContext.GetStateAsync<Dictionary<string, object>>(_stateAccessors.UserDialogState);

                var dialogContext = _dialogSet.CreateContext(turnContext, dialogState);

                var utterance = turnContext.Activity.Text.ToLowerInvariant();
                if (utterance == "cancel")
                {
                    if (dialogContext.ActiveDialog != null)
                    {
                        await turnContext.SendActivityAsync("Ok... Cancelled");
                        dialogContext.EndAll();
                    }
                    else
                    {
                        await turnContext.SendActivityAsync("Nothing to cancel.");
                    }
                }

                if (!turnContext.Responded)
                {
                    await dialogContext.ContinueAsync();

                    if (!turnContext.Responded)
                    {
                        //TODO: This code will be updated with new Dialogs as the revised models land
                        // and the LUIS + Dialog examplar is formalized. 
                        var luisResult = await _luisRecognizer.RecognizeAsync(turnContext, CancellationToken.None);
                        var (intent, score) = luisResult.GetTopScoringIntent();
                        var intentResult = score > LUIS_INTENT_THRESHOLD ? intent : "None";
                        await dialogContext.BeginAsync("AlarmDialog");
                    }
                }
            }
        }
    }
}
