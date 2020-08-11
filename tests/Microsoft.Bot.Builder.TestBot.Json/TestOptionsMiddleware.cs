using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.TestBot.Json
{
    public class TestOptionsMiddleware : IMiddleware
    {
        private static readonly string _setOptionsName = "Microsoft.SetTestOptions";
        private static readonly string _seedProperty = "seed";

        private ConversationState conversationState;

        public TestOptionsMiddleware(ConversationState conversationState)
        {
            this.conversationState = conversationState;
        }

        /// <summary>
        /// Adds the associated object or service to the current turn context.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="nextTurn">The delegate to call to continue the bot middleware pipeline.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <seealso cref="ITurnContext"/>
        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate nextTurn, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Type == ActivityTypes.Event &&
                turnContext.Activity.Name == _setOptionsName)
            {
                var testOptions = new TestOptions();
                var randomObject = turnContext.Activity.Value;
                if (randomObject != null)
                {
                    var randomJObj = JObject.FromObject(randomObject);
                    if (randomJObj[_seedProperty] != null)
                    {
                        var seedJOb = randomJObj[_seedProperty];
                        if (seedJOb is JValue seedJValue && seedJValue.Type == JTokenType.Integer)
                        {
                            var seed = seedJValue.ToObject<int>();
                            testOptions.Random = new Random(seed);

                            var accessor = conversationState.CreateProperty<TestOptions>(nameof(TestOptions));
                            await accessor.SetAsync(turnContext, testOptions, cancellationToken);

                            await conversationState.SaveChangesAsync(turnContext, true, cancellationToken);
                        }
                    }
                }
            }

            await nextTurn(cancellationToken).ConfigureAwait(false);
        }
    }
}
