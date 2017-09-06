using Microsoft.Bot.Connector;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Interface for creating BotContext
    /// </summary>
    public interface IBotContextFactory
    {
        /// <summary>
        /// Create a bot context from an activity from an activiy source such as a connector
        /// </summary>
        /// <param name="activity"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<BotContext> CreateBotContext(IActivity activity, CancellationToken token);

        /// <summary>
        /// Create a bot context from a conversationReference so you can proactively interact witha conversation
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<BotContext> CreateBotContext(ConversationReference reference, CancellationToken token);
    }

    public class BotContextFactory : IBotContextFactory
    {
        private readonly IDataContext _dataContext;
        private readonly IPostActivity _postToUser;
        private readonly IBotLogger _logger; 

        public BotContextFactory(IDataContext dataContext, IPostActivity postToUser, IBotLogger logger)
        {
            _dataContext = dataContext ?? throw new ArgumentNullException("dataContext");
            _postToUser = postToUser ?? throw new ArgumentNullException("postToUser");
            _logger = logger ?? throw new ArgumentNullException("logger");
        }
        
        public Task<BotContext> CreateBotContext(IActivity activity, CancellationToken token)
        {
            BotAssert.ActivityNotNull(activity);
            BotAssert.CancellationTokenNotNull(token);
            return Task.FromResult(new BotContext(activity, this._dataContext, this._postToUser, this._logger));
        }

        public async Task<BotContext> CreateBotContext(ConversationReference reference, CancellationToken token)
        {
            if (reference == null)
                throw new ArgumentNullException("reference");

            BotAssert.CancellationTokenNotNull(token);

            return await this.CreateBotContext(reference.GetPostToBotMessage(), token);
        }
    }
}
