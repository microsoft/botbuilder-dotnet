using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder
{
    public class ConversationState : StoreItem
    {
    }

    public class UserState : StoreItem
    {
    }
    
    public class BotState : FlexObject
    {       
        public ConversationState Conversation { get; set; } = new ConversationState(); 
        public UserState User { get; set; } = new UserState(); 
    }
    
    public class BotStateManagerSettings
    {
        public BotStateManagerSettings()
        {
            PersistConversationState = true;
            PersistUserState = true;
            WriteBeforePost = true;
            LastWriterWins = true;
        }

        public bool PersistUserState { get; set; }
        public bool PersistConversationState { get; set; }
        public bool WriteBeforePost { get; set; }
        public bool LastWriterWins { get; set; }
    }

    public class BotStateManager : IMiddleware, IContextCreated, IPostActivity, IContextDone
    {
        private BotStateManagerSettings _settings;
        public BotStateManager() : this(new BotStateManagerSettings())
        {
        }

        public BotStateManager(BotStateManagerSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException("settings");
        }

        public async Task ContextCreated(BotContext context, CancellationToken token)
        {
            await Read(context).ConfigureAwait(false);            
        }

        public async Task ContextDone(BotContext context, CancellationToken token)
        {
            await Write(context).ConfigureAwait(false);            
        }

        public async Task PostActivity(BotContext context, IList<Activity> activities, CancellationToken token)
        {
            if (_settings.WriteBeforePost)
            {
                await Write(context).ConfigureAwait(false); 
            }
        }

        protected async Task Read(BotContext context)
        {
            AssertStorage(context);
            List<String> keys = new List<string>(); 

            if (_settings.PersistUserState)
                keys.Add(UserKey(context));

            if (_settings.PersistConversationState)
                keys.Add(ConversationKey(context));            

            var items = await context.Storage.Read(keys.ToArray());

            string userKey = UserKey(context);
            string conversationKey = ConversationKey(context);

            context.State.User = items.Get<UserState>(userKey) ?? new UserState();
            context.State.Conversation = items.Get<ConversationState>(conversationKey) ?? new ConversationState();
        }

        protected async Task Write (BotContext context)
        {
            AssertStorage(context);

            StoreItems changes = new StoreItems();            
            
            if (this._settings.PersistUserState)
            {
                changes[this.UserKey(context)] = context.State.User ?? new UserState();
            }

            if (this._settings.PersistConversationState)
            {
                changes[this.ConversationKey(context)] = context.State.Conversation ?? new ConversationState();
            }

            if (this._settings.LastWriterWins)
            {
                foreach (var item in changes)
                {
                    ((StoreItem)changes[item.Key]).eTag = "*"; 
                }
            }

            await context.Storage.Write(changes).ConfigureAwait(false); 
        }

        private string UserKey(BotContext context)
        {
            var conversation = context.ConversationReference;
            return $"user/{conversation.ChannelId}/{conversation.User.Id}";            
        }

        private string ConversationKey(BotContext context)
        {
            var conversation = context.ConversationReference;
            return $"conversation/{conversation.ChannelId}/{conversation.Conversation.Id}";
        }

        private void AssertStorage(BotContext context)
        {
            if (context.Storage == null)
                throw new InvalidOperationException("BotStateManager: context.storage not found.");
        }
    }
}
