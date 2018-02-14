using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Middleware
{
    /// <summary>
    /// Binds outgoing activities to a particular conversation. 
    /// As messages are sent during the Send pipeline, they must be
    /// bound to the relvant ConversationReference object. This middleare
    /// runs at the start of the Sending Pipeline and binds all outgoing
    /// Activities to the ConversationRefernce on the Context. 
    /// </summary>
    /// <remarks>
    /// This Middleware component is automatically added to the Send Pipeline
    /// when constructing a bot. 
    /// 
    /// In terms of protocol level behavior, the binding of Actities to 
    /// a ConversationReference is similar to how the Node SDK applies the same
    /// set of rules on all outbound Activities.     
    /// </remarks>
    public class BindOutoingResponsesMiddlware : ISendActivity
    {        
        public BindOutoingResponsesMiddlware()
        {            
        }

        public async Task SendActivity(IBotContext context, IList<IActivity> activities, MiddlewareSet.NextDelegate next)
        {
            BotAssert.ContextNotNull(context);
            BotAssert.ActivityListNotNull(activities);

            foreach( var activity in activities)
            {
                if (string.IsNullOrWhiteSpace(activity.Type))
                {
                    activity.Type = ActivityTypes.Message;
                }

                ApplyConversationReference(activity, context.ConversationReference); 
            }

            await next().ConfigureAwait(false);            
        }

        /// <summary>
        /// Applies all relevant Conversation related identifies to an activity. This effectivly
        /// couples a blank Activity to a conversation. 
        /// </summary>
        /// <param name="activity">The activity to update. Existing values in the Activity will be overwritten.</param>
        /// <param name="reference">The ConversationReference from which to pull the relevant conversation information</param>
        /// <remarks>        
        /// This method applies the following values from ConversationReference:
        /// ChannelId
        /// ServiceUrl
        /// Conversation
        /// Bot (assigned to the .From property on the Activity)
        /// User (assigned to the .Recipient on the Activity)
        /// ActivityId (assigned as the ReplyToId on the Activity)                
        /// </remarks>
        public static void ApplyConversationReference(IActivity activity, ConversationReference reference)
        {
            BotAssert.ActivityNotNull(activity);
            BotAssert.ConversationReferenceNotNull(reference); 

            activity.ChannelId = reference.ChannelId;
            activity.ServiceUrl = reference.ServiceUrl;
            activity.Conversation = reference.Conversation;
            activity.From = reference.Bot;
            activity.Recipient = reference.User;
            activity.ReplyToId = reference.ActivityId;
        }
    }
    
}
