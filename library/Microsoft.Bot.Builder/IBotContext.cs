using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;

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
            await context.Post(context, new List<IActivity>(), token);
        }

        public static Task Done(this IBotContext context, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }

    public class BotContext : FlexObject, IBotContext, IPostToUser
    {
        private readonly IActivity request;
        private IList<IActivity> responses;
        private IBotLogger logger;
        private IDataContext dataContext;
        private IPostToUser postToUser;
        private IStorage storage;

        public BotContext(IActivity request, IDataContext dataContext, IPostToUser postToUser, IBotLogger logger = null)
        {
            SetField.NotNull(out this.request, nameof(request), request);
            SetField.NotNull(out this.dataContext, nameof(dataContext), dataContext);
            SetField.NotNull(out this.postToUser, nameof(postToUser), postToUser);
            this.logger = logger;
            this.responses = new List<IActivity>();
        }
        
        public async Task Post(BotContext context, IList<IActivity> acitivties, CancellationToken token)
        {
            await this.postToUser.Post(context, acitivties, token);
        }

        public IActivity Request => request;

        public IList<IActivity> Responses { get => responses; set => this.responses = value; }

        public IUserContext User => dataContext.User;

        public IConversationContext Conversation => dataContext.Conversation;

        public IBotContextData Data => dataContext.Data;

        public IBotLogger Logger => this.logger;

        /// <summary>
        /// Key/Value storage provider
        /// </summary>
        public IStorage Storage { get; set; }
    }
}
