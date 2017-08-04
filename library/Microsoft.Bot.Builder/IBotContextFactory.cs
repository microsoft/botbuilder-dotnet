using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;

namespace Microsoft.Bot.Builder
{
    public interface IBotContextFactory
    {
        Task<BotContext> CreateBotContext(IActivity activity, CancellationToken token);
        Task<BotContext> CreateBotContext(ConversationReference reference, CancellationToken token);
    }

    public class BotContextFactory : IBotContextFactory
    {
        private readonly IDataContext dataContext;
        private readonly IPostToUser postToUser;
        private readonly IBotLogger logger; 

        public BotContextFactory(IDataContext dataContext, IPostToUser postToUser, IBotLogger logger)
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
