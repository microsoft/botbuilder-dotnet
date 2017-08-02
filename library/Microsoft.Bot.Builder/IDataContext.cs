using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Connector;

namespace Microsoft.Bot.Builder
{
    public interface IUserContext
    {
        IBotDataBag Data { get; }
    }

    public interface IConversationContext
    {
        ConversationReference reference { get; }
        IBotDataBag Data { get; }
    }

    public interface IBotContextData
    {
        IBotDataBag Data { get; }
    }

    public interface IDataContext
    {
        IUserContext User { get; }
        IConversationContext Conversation { get; }
        IBotContextData Data { get; }
    }

    public sealed class DataContext : IDataContext
    {
        private readonly IUserContext user;
        private readonly IConversationContext conversation;
        private readonly IBotContextData data;

        public DataContext(IUserContext user, IConversationContext conversation, IBotContextData data)
        {
            SetField.NotNull(out this.user, nameof(user), user);
            SetField.NotNull(out this.conversation, nameof(conversation), conversation);
            SetField.NotNull(out this.data, nameof(data), data);
        }

        public IUserContext User => this.user;

        public IConversationContext Conversation => this.conversation;

        public IBotContextData Data => this.data;
    }

    public sealed class NullDataContext : IDataContext
    {
        public NullDataContext()
        {
        }

        public IUserContext User => null;

        public IConversationContext Conversation => null;

        public IBotContextData Data => null;
    }

}
