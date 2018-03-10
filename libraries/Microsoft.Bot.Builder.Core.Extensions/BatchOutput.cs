// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Core.Extensions
{
    public class BatchOutputMiddleware : IMiddleware
    {
        public const string BatchOuputKey = "Extensions.Middleware.BatchOutput";

        public async Task OnProcessRequest(IBotContext context, MiddlewareSet.NextDelegate next)
        {
            BatchOutput batch = new BatchOutput();
            if (context.Has(BatchOuputKey))
                throw new InvalidOperationException("Batch Output already configured");

            // Store this in the context so that the Extensio method can 
            // find it to return when folks say "context.Batch().Typing()". 
            context.Set(BatchOuputKey, batch);

            await next().ConfigureAwait(false);
            await batch.Flush(context);
        }
    }

    public class BatchOutput
    {        
        private IList<Activity> _activities = new List<Activity>();

        public BatchOutput() { }

        /// <summary>
        /// Adds a delay to the batch.This can be used to pause after sending a typing indicator or
        /// after sending a card with image(s). 
        /// 
        /// Most chat clients download any images sent by the bot to a CDN which can delay the showing
        /// of the message to the user.  If a bot sends a message with only text immediately after
        /// sending a message with images, the messages could end up being shown to the user out of
        /// order. To help prevent this you can insert a delay of 2 seconds or so in between replies.     
        /// @param ms Number of milliseconds to pause before delivering the next activity in the batch. 
        /// </summary>
        /// <param name="milliseconds">Number of milliseconds to pause before delivering the next activity in the batch</param>
        public BatchOutput Delay(int milliseconds)
        {
            if (milliseconds < 0)
                throw new ArgumentOutOfRangeException(nameof(milliseconds));

            _activities.Add(new Activity
            {
                Type = ActivityTypesEx.Delay,
                Value = milliseconds
            });
            return this;
        }

        /// <summary>
        /// Adds an `endOfConversation` activity to the batch indicating that the bot has completed
        /// it's current task or skill.  For channels like Cortana this is used to tell Cortana that the
        /// skill has completed and the skills window should close.
        ///
        /// When used in conjunction with the `ConversationState` middleware, sending an `endOfConversation`
        /// activity will cause the bots conversation state to be automatically cleared.If you're 
        /// building a Cortana skill this helps ensure that the next time your skill is invoked it
        /// will be in a clean state given that you won't always get a new conversation ID in between
        /// invocations.
        ///
        /// Even for non-Cortana bots it's a good practice to send an `endOfConversation` anytime you 
        /// complete a task with the user as it will give your bot a chance to clear its conversation
        /// state and helps avoid your bot getting into a bad state for a conversation.
        /// </summary>
        /// <param name="endOfConversationCode">(Optional) code to indicate why the bot/skill is ending. Defaults to 
        /// `EndOfConversationCodes.CompletedSuccessfully`.
        /// </param>        
        public BatchOutput EndOfConversation(string endOfConversationCode = null)
        {
            if (string.IsNullOrWhiteSpace(endOfConversationCode))
                endOfConversationCode = EndOfConversationCodes.CompletedSuccessfully;

            _activities.Add(new Activity
            {
                Type = ActivityTypes.EndOfConversation,
                Code = endOfConversationCode
            });
            return this;
        }

        /// <summary>
        /// Adds an `event` activity to the batch. This is most useful for DirectLine and WebChat
        /// channels as a way for the bot to send a custom named event to the client.
        /// </summary>
        /// <param name="name">Name of the event being sent.</param>
        /// <param name="value">(Optional) value to include with the event.</param>
        /// <returns></returns>
        public BatchOutput Event(string name, object value = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            _activities.Add(new Activity
            {
                Type = ActivityTypes.Event,
                Name = name,
                Value = value
            });

            return this;
        }

        public async Task Flush(IBotContext context)
        {
            // ToDo: addin the ResourceResponses when this is plumbed through
            Activity[] toSend = _activities.ToArray();
            _activities.Clear();

            await context.SendActivity(toSend);            
        }

        /// <summary>
        /// Adds a Typing Activity to the Batch
        /// </summary>        
        public BatchOutput Typing()
        {
            _activities.Add(new Activity(ActivityTypes.Typing));
            return this;
        }

        public BatchOutput Activity(Activity activity)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));

            _activities.Add(activity);
            return this;
        }

        public BatchOutput Reply(IMessageActivity message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message)); 

            _activities.Add((Activity)message);
            return this;
        }
        public BatchOutput Reply(string text, string ssml = null, string inputHunt = null)
        {
            Activity a = MessageFactory.Text(text, ssml, inputHunt);
            _activities.Add(a); 
            return this;
        }
        public BatchOutput Reply(Activity activity, string ssml = null, string inputHint = null) 
        {
            BotAssert.ActivityNotNull(activity);

            // If the Type isn't set, just set it to message
            if (string.IsNullOrWhiteSpace(activity.Type))
                activity.Type = ActivityTypes.Message;      

            // apply any SSML if it's been passed in
            if (!string.IsNullOrWhiteSpace(ssml))
                activity.Speak = ssml;

            // apply the input hint if it's been passed in
            if (!string.IsNullOrWhiteSpace(inputHint))
                activity.InputHint = inputHint;

            _activities.Add(activity);
            return this;
        }
    }

    public static class BatchOutputExtensions
    {
        public static BatchOutput Batch(this IBotContext context)
        {
            BatchOutput bo = context.Get<BatchOutput>(BatchOutputMiddleware.BatchOuputKey);
            if (bo == null)
                throw new InvalidOperationException("BatchOutputMiddleware does not appear to be setup.");

            return bo;
        }
    }
}