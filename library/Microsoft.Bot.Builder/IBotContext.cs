using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    
    public interface IBotContext
    {
        IActivity Request { get; }
        IList<IActivity> Responses { get; set; }
        IBotLogger Logger { get; }
    }

    public static partial class BotContextExtension
    {
        public static async Task Post(this BotContext context, CancellationToken token)
        {
            BotAssert.CancellationTokenNotNull(token);
            await context.PostActivity(context, new List<IActivity>(), token);
        }        
    }

    public class BotContext : FlexObject, IBotContext, IPostActivity
    {
        private Bot _bot;
        private readonly IActivity _request;
        private IList<IActivity> _responses = new List<IActivity>();
        private ConversationReference _conversationReference;

        public BotContext(Bot bot, IActivity request)
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

        public async Task PostActivity(BotContext context, IList<IActivity> acitivties, CancellationToken token)
        {
            await _bot.PostActivity(context, acitivties, token).ConfigureAwait(false);
        }

        public IActivity Request => _request;

        public Bot Bot => _bot;

        public IList<IActivity> Responses { get => _responses; set => this._responses = value; }

        public IBotLogger Logger => _bot.Logger;

        public IStorage Storage { get; set; }

        // Note: These will come back a we interagte the storage layer next. 
        //public IUserContext User => throw new NotImplementedException();
        //public IConversationContext Conversation => throw new NotImplementedException();
        //public IBotContextData Data => throw new NotImplementedException();
    }
}
