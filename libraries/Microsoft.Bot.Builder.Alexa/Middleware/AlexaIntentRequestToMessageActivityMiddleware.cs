using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Alexa.Middleware
{
    public class AlexaIntentRequestToMessageActivityMiddleware : IMiddleware
    {
        private readonly RequestTransformPatterns _transformPattern;
        private readonly Func<ITurnContext, AlexaIntentRequest, string> _createMessageActivityText;

        public AlexaIntentRequestToMessageActivityMiddleware(RequestTransformPatterns transformPattern = RequestTransformPatterns.MessageActivityTextFromSinglePhraseSlotValue)
        {
            _transformPattern = transformPattern;
        }

        public AlexaIntentRequestToMessageActivityMiddleware(Func<ITurnContext, AlexaIntentRequest, string> createMessageActivityText)
        {
            _createMessageActivityText = createMessageActivityText;
        }

        public async Task OnTurnAsync(ITurnContext context, NextDelegate next, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (context.Activity.ChannelId == "alexa" && context.Activity.Type == AlexaRequestTypes.IntentRequest)
            {
                var alexaRequestBody = (AlexaRequestBody) context.Activity.ChannelData;
                var alexaIntentRequest = (AlexaIntentRequest) alexaRequestBody.Request;

                context.Activity.Type = ActivityTypes.Message;

                if (_createMessageActivityText != null)
                {
                    var messageActivityText = _createMessageActivityText(context, alexaIntentRequest);
                    context.Activity.Text = messageActivityText;
                }
                else switch (_transformPattern)
                {
                    case RequestTransformPatterns.MessageActivityTextFromSinglePhraseSlotValue:
                        if (alexaIntentRequest.Intent.Slots.ContainsKey("phrase"))
                        {
                            context.Activity.Text = alexaIntentRequest.Intent.Slots["phrase"].Value;
                        }
                        break;
                    case RequestTransformPatterns.MessageActivityTextFromIntentAndAllSlotValues:
                        var messageActivityText = $"Intent='{alexaIntentRequest.Intent.Name}'";

                        foreach (var intentSlot in alexaIntentRequest.Intent.Slots)
                        {
                            messageActivityText += $" {intentSlot.Key}='{intentSlot.Value.Value}'";
                        }

                        context.Activity.Text = messageActivityText;
                        break;
                }
            }

            await next(cancellationToken).ConfigureAwait(false);
        }
    }

    public enum RequestTransformPatterns
    {
        MessageActivityTextFromSinglePhraseSlotValue,
        MessageActivityTextFromIntentAndAllSlotValues
    }
}
