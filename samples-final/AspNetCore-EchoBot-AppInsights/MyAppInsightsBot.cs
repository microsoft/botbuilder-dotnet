// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using AspNetCore_EchoBot_With_AppInsights.AppInsights;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Ai.QnA;
using Microsoft.Bot.Schema;

namespace AspNetCore_EchoBot_With_AppInsights
{
    public class MyAppInsightsBot : IBot
    {
        private readonly MyAppInsightsBotAccessors _stateAccessors;
        private readonly MyAppInsightLuisRecognizer _luisRecognizer;
        private readonly MyAppInsightsQnaMaker _qnaMaker;

        public MyAppInsightsBot(MyAppInsightsBotAccessors accessors, MyAppInsightLuisRecognizer luisRecognizer, MyAppInsightsQnaMaker qnaMaker)
        {
            _stateAccessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            _luisRecognizer = luisRecognizer ?? throw new ArgumentNullException(nameof(luisRecognizer));
            _qnaMaker = qnaMaker;
        }
        /// <summary>
        /// Every Conversation turn for our EchoBot will call this method. In here
        /// the bot checks the Activty type to verify it's a message, bumps the 
        /// turn conversation 'Turn' count, and then echoes the users typing
        /// back to them. 
        /// </summary>
        /// <param name="context">Turn scoped context containing all the data needed
        /// for processing this conversation turn. </param>        
        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            // This bot is only handling Messages
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                // Get the conversation state from the turn context that holds a simple counter
                var state = await _stateAccessors.CounterState.GetAsync(turnContext, () => new CounterState());

                // Bump the turn count. 
                state.TurnCount++;

                // Echo back to the user whatever they typed.
                var responseMessage = $"Turn {state.TurnCount}: You sent '{turnContext.Activity.Text}'\n";


                // Try LUIS
                var recognizerResult = await _luisRecognizer.RecognizeAsync(turnContext, cancellationToken);
                var topIntent = recognizerResult?.GetTopScoringIntent();
                if (topIntent != null)
                {
                    responseMessage += $"==>Luis Top Scoring Intent: {topIntent.Value.intent}, Score: {topIntent.Value.score}\n";
                }

                // Try QNA
                QueryResult[] qnaResults = await _qnaMaker.GetAnswersAsync(turnContext);
                if (qnaResults != null && qnaResults.Length > 0)
                {
                    responseMessage += $"==>Qna Top Answer: {qnaResults[0].Answer}, Score: {qnaResults[0].Score}, Question: {string.Join(",", qnaResults[0]?.Questions)}\n";
                }
                else
                {
                    responseMessage += "==>No Matching Qna\n";
                }

                await turnContext.SendActivityAsync(responseMessage);
            }
        }
    }
}
