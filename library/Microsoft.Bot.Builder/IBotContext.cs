using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    public interface IBotContext : IDataContext
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

        public static Task Done(this IBotContext context, CancellationToken token)
        {
            BotAssert.CancellationTokenNotNull(token);
            throw new NotImplementedException();
        }
    }

    public class BotContext : FlexObject, IBotContext, IPostActivity
    {
        private readonly IActivity _request;
        private IList<IActivity> _responses = new List<IActivity>();
        private IBotLogger _logger;
        private IDataContext _dataContext;
        private IPostActivity _postToUser;
        private IStorage _storage = null;

        public BotContext(IActivity request, IDataContext dataContext, IPostActivity postToUser, IBotLogger logger = null)
        {
            _request = request ?? throw new ArgumentNullException("request");
            _dataContext = dataContext ?? throw new ArgumentNullException("dataContext");
            _postToUser = postToUser ?? throw new ArgumentNullException("postToUser");

            this._logger = logger ?? new NullLogger();
        }
        
        public async Task PostActivity(BotContext context, IList<IActivity> acitivties, CancellationToken token)
        {
            await this._postToUser.PostActivity(context, acitivties, token);
        }

        public IActivity Request => _request;

        public IList<IActivity> Responses { get => _responses; set => this._responses = value; }

        public IUserContext User => _dataContext.User;

        public IConversationContext Conversation => _dataContext.Conversation;

        public IBotContextData Data => _dataContext.Data;

        public IBotLogger Logger => this._logger;

        /// <summary>
        /// Key/Value storage provider
        /// </summary>
        public IStorage Storage { get; set; }
    }
}
