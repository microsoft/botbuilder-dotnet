using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Samples.Ai.QnA
{
    public class QnAMakerBot : IBot
    {
        public QnAMakerBot() { }

        public async Task OnTurn(ITurnContext context)
        {
            // At this point, the QnA Maker Middleware has already been run. If the incoming
            // Activity was a message, the Middleware called out to QnA Maker looking for 
            // an answer. If an answer was found, the Responded flag on the context will be set 
            // and we can do nothing here. If the Middlware did NOT find a match, then it's 
            // up to the Bot to send something to the user, in this case the "No Match" message. 
            switch (context.Activity.Type)
            {
                case ActivityTypes.Message:
                    if (context.Activity.Type == ActivityTypes.Message && context.Responded == false)
                    {
                        // add app logic when QnA Maker doesn't find an answer
                        await context.SendActivity("No good match found in the KB.");
                    }

                    break;
                case ActivityTypes.ConversationUpdate:
                    foreach (var newMember in context.Activity.MembersAdded)
                    {
                        if (newMember.Id != context.Activity.Recipient.Id)
                        {
                            await context.SendActivity("Hello and welcome to the QnA Maker Sample bot.");
                        }
                    }
                    break;
            }
        }
    }
}
