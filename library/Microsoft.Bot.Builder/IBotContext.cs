using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    public interface IBotContext
    {
        /// <summary>
        /// Incoming request
        /// </summary>
        Activity Request { get; }

        /// <summary>
        /// Respones
        /// </summary>
        IList<Activity> Responses { get; set; }

        /// <summary>
        /// Conversation reference
        /// </summary>
        ConversationReference ConversationReference { get; }

        /// <summary>
        /// Bot state 
        /// </summary>
        BotState State { get; }

        /// <summary>
        /// Registered logger
        /// </summary>
        IBotLogger Logger { get; }

        /// <summary>
        /// registered storage
        /// </summary>
        IStorage Storage { get; set; }

        Intent TopIntent { get; set; }

        /// <summary>
        /// check to see if topIntent matches
        /// </summary>
        /// <param name="intentName"></param>
        /// <returns></returns>
        bool IfIntent(string intentName);

        /// <summary>
        /// Check to see if intent matches regex
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        bool IfIntent(Regex expression);

        /// <summary>
        /// Send a reply to the sender
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        BotContext Reply(string text);

        /// <summary>
        /// Send a reply using a templateId bound to data
        /// </summary>
        /// <param name="templateId">template Id</param>
        /// <param name="data">data object to bind to</param>
        /// <returns></returns>
        BotContext ReplyWith(string templateId, object data);

    }   

    public static partial class BotContextExtension
    {
        public static async Task Post(this BotContext context)
        {            
            await context.PostActivity(context, new List<Activity>()).ConfigureAwait(false);
        }  
        
        public static BotContext ToBotContext(this IBotContext context)
        {
            return (BotContext)context; 
        }
    }

    public class BotContext : FlexObject, IBotContext, IPostActivity
    {
        private readonly Bot _bot;
        private readonly Activity _request;        
        private readonly ConversationReference _conversationReference;
        private readonly BotState _state = new BotState();
        private readonly IList<ITemplateEngine> _templateEngines = new List<ITemplateEngine>();
        private IList<Activity> _responses = new List<Activity>();

        public BotContext(Bot bot, Activity request)
        {
            _bot = bot ?? throw new ArgumentNullException(nameof(bot));
            _request = request ?? throw new ArgumentNullException(nameof(request)); 

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
        

        public async Task PostActivity(BotContext context, IList<Activity> acitivties)
        {
            await _bot.PostActivity(context, acitivties).ConfigureAwait(false);
        }

        public Activity Request => _request;

        public Bot Bot => _bot;

        public IList<Activity> Responses { get => _responses; set => this._responses = value; }

        public IBotLogger Logger => _bot.Logger;

        public IStorage Storage { get; set; }

        public Intent TopIntent { get; set; }

        public TemplateManager TemplateManager { get; set; }

        public bool IfIntent(string intentName)
        {
            if (string.IsNullOrWhiteSpace(intentName))
                throw new ArgumentNullException(nameof(intentName)); 

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
                throw new ArgumentNullException(nameof(expression)); 

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

        public BotContext ReplyWith(string templateId, object data)
        {
            // queue template activity to be databound when sent
            var reply = (this.Request as Activity).CreateReply();
            reply.Type = "template";
            reply.Text = templateId;
            reply.Value = data;
            this.Responses.Add(reply);
            return this;
        }
    }
}
