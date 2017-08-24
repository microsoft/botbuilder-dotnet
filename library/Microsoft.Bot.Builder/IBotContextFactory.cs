using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;

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
        private readonly IDataContext dataContext;
        private readonly IPostActivity postToUser;
        private readonly IBotLogger logger; 

        public BotContextFactory(IDataContext dataContext, IPostActivity postToUser, IBotLogger logger)
        {
            SetField.NotNull(out this.dataContext, nameof(dataContext), dataContext);
            SetField.NotNull(out this.postToUser, nameof(postToUser), postToUser);
            SetField.NotNull(out this.logger, nameof(logger), logger);
        }
        
        public Task<BotContext> CreateBotContext(IActivity activity, CancellationToken token)
        {
            return Task.FromResult(new BotContext(activity, this.dataContext, this.postToUser, this.logger));
        }

        public async Task<BotContext> CreateBotContext(ConversationReference reference, CancellationToken token)
        {
            return await this.CreateBotContext(reference.GetPostToBotMessage(), token);
        }
    }
}
