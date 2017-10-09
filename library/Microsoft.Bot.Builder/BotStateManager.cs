using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;

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
        private const string UserKeyRoot = @"user";
        private const string ConversationKeyRoot = @"conversation";


        public BotStateManager() : this(new BotStateManagerSettings())
        {
        }

        public BotStateManager(BotStateManagerSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException("settings");
        }

        public async Task ContextCreated(BotContext context, CancellationToken token)
        {
            await Read(context, new List<string>()).ConfigureAwait(false);            
        }

        public async Task ContextDone(BotContext context, CancellationToken token)
        {
            await Write(context, new StoreItems()).ConfigureAwait(false);            
        }

        public async Task PostActivity(BotContext context, IList<Activity> activities, CancellationToken token)
        {
            if (_settings.WriteBeforePost)
            {
                await Write(context, new StoreItems()).ConfigureAwait(false); 
            }
        }

        protected virtual async Task<StoreItems> Read(BotContext context, IList<String> keys)
        {
            AssertStorage(context);
            AssertValidKeys(keys);
            
            if (_settings.PersistUserState)
                keys.Add(UserKey(context));

            if (_settings.PersistConversationState)
                keys.Add(ConversationKey(context));            

            var items = await context.Storage.Read(keys.ToArray());

            string userKey = UserKey(context);
            string conversationKey = ConversationKey(context);

            context.State.User = items.Get<UserState>(userKey) ?? new UserState();
            context.State.Conversation = items.Get<ConversationState>(conversationKey) ?? new ConversationState();

            return items;
        }

        protected virtual async Task Write (BotContext context, StoreItems changes)
        {
            AssertStorage(context);
            if (changes == null)
                throw new ArgumentNullException("changes");            
                        
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

        private static void AssertValidKeys(IList<string> keys)
        {
            if (keys == null)
                throw new ArgumentNullException("keys");

            foreach (string key in keys)
            {
                if (key.StartsWith(UserKeyRoot, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException($"Keys starting with '{UserKeyRoot}' are reserved.");

                if (key.StartsWith(ConversationKeyRoot, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException($"Keys starting with '{ConversationKeyRoot}' are reserved.");
            }
        }
        private string UserKey(BotContext context)
        {
            var conversation = context.ConversationReference;
            return $"{UserKeyRoot}/{conversation.ChannelId}/{conversation.User.Id}";            
        }

        private string ConversationKey(BotContext context)
        {
            var conversation = context.ConversationReference;
            return $"{ConversationKeyRoot}/{conversation.ChannelId}/{conversation.Conversation.Id}";
        }

        private void AssertStorage(BotContext context)
        {
            if (context.Storage == null)
                throw new InvalidOperationException("BotStateManager: context.storage not found.");
        }
    }
}
