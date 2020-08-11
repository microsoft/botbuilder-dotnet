using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.TestBot.Json
{
    /// <summary>
    /// Support arbitrary event by hack.
    /// Message: {"event": "myevent", "value": "aaa"} => into event activity.
    /// </summary>
    public class ConvertMessageToEventMiddleware : IMiddleware
    {
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
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                var message = turnContext.Activity.Text;
                if (message.StartsWith("{") && message.EndsWith("}"))
                {
                    try
                    {
                        var jObj = JObject.Parse(message);
                        if (jObj["event"] != null)
                        {
                            turnContext.Activity.Type = ActivityTypes.Event;
                            turnContext.Activity.Name = jObj["event"].ToString();
                            turnContext.Activity.Value = jObj["value"];
                        }
                    }
                    catch
                    {
                        // do nothing.
                    }
                }
            }

            await nextTurn(cancellationToken).ConfigureAwait(false);
        }
    }
}
