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
        private readonly IUserContext _user;
        private readonly IConversationContext _conversation;
        private readonly IBotContextData _data;

        public DataContext(IUserContext user, IConversationContext conversation, IBotContextData data)
        {
            _user = user ?? throw new ArgumentNullException("user");
            _conversation = conversation ?? throw new ArgumentNullException("conversation");
            _data = data ?? throw new ArgumentNullException("data");
        }

        public IUserContext User => this._user;

        public IConversationContext Conversation => this._conversation;

        public IBotContextData Data => this._data;
    }

    /// <summary>
    /// Note: This class is only here for DI to work. It'll be removed in then next pass. 
    /// </summary>
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
