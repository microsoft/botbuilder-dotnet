using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Microsoft.Bot.Builder
{    
    public interface IBotContext
    {
        Activity Request { get; }
        IList<Activity> Responses { get; set; }
        ConversationReference ConversationReference { get; }
        BotState State { get; }    
        IBotLogger Logger { get; }
        IStorage Storage { get; set; }
        Intent TopIntent { get; set; }

        bool IfIntent(string intentName);
        bool IfIntent(Regex expression);
        BotContext Reply(string text);
    }   

    public static partial class BotContextExtension
    {
        public static async Task Post(this BotContext context, CancellationToken token)
        {
            BotAssert.CancellationTokenNotNull(token);
            await context.PostActivity(context, new List<Activity>(), token).ConfigureAwait(false);
        }  
        
        public static BotContext ToBotContext(this IBotContext context)
        {
            return (BotContext)context; 
        }
    }

    public class BotContext : FlexObject, IBotContext, IPostActivity
    {
        private Bot _bot;
        private readonly Activity _request;
        private IList<Activity> _responses = new List<Activity>();
        private ConversationReference _conversationReference;
        private BotState _state = new BotState(); 

        public BotContext(Bot bot, Activity request)
        {
            _bot = bot ?? throw new ArgumentNullException("bot");
            _request = request ?? throw new ArgumentNullException("request");

            _conversationReference = new ConversationReference()
            {
                ActivityId = request.Id,
                User = request.From,
                Bot = request.Recipient,
                Conversation = request.Conversation,
                ChannelId = request.ChannelId,
                ServiceUrl = request.ServiceUrl
            };
        }
        

        public async Task PostActivity(BotContext context, IList<Activity> acitivties, CancellationToken token)
        {
            await _bot.PostActivity(context, acitivties, token).ConfigureAwait(false);
        }

        public Activity Request => _request;

        public Bot Bot => _bot;

        public IList<Activity> Responses { get => _responses; set => this._responses = value; }

        public IBotLogger Logger => _bot.Logger;

        public IStorage Storage { get; set; }

        public Intent TopIntent { get; set; }

        public bool IfIntent(string intentName)
        {
            if (string.IsNullOrWhiteSpace(intentName))
                throw new ArgumentNullException("intentName"); 

            if (this.TopIntent != null)
            {
                if (TopIntent.Name == intentName)
                {
                    return true;
                }
            }

            return false;
        }
        public bool IfIntent(Regex expression)
        {
            if (expression == null) 
                throw new ArgumentNullException("expression");

            if (this.TopIntent != null)
            {
                if (expression.IsMatch(this.TopIntent.Name))
                    return true;
            }

            return false;
        }


        public ConversationReference ConversationReference { get => _conversationReference; }

        public BotState State { get => _state; }

        public BotContext Reply(string text)
        {
            var reply = (this.Request as Activity).CreateReply();
            reply.Text = text;
            this.Responses.Add(reply);
            return this; 
        }
    }
}
