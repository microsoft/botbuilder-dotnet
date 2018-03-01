using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Utility class to allow you to create custom BotContext wrapper which wraps someone elses BotContext 
    /// </summary>
    /// <remarks>
    /// Adapters create the IBotContext implementation which is then passed to the bot's logic handler
    /// This class allows you to create your own IBotContext which delegates to the BotContext passed to you
    /// It simply passes all IBotContext calls through to the inner IBotContext.
    /// </remarks>
    public class BotContextWrapper : IBotContext
    {
        private IBotContext _innerContext;

        public BotContextWrapper(IBotContext context)
        {
            this._innerContext = context;
        }

        public BotAdapter Adapter => this._innerContext.Adapter;

        public Activity Request => this._innerContext.Request;

        public IList<Activity> Responses { get => this._innerContext.Responses; set => this._innerContext.Responses = value; }

        public ConversationReference ConversationReference => this._innerContext.ConversationReference;

        public object Get(string serviceId)
        {
            return this._innerContext.Get(serviceId);
        }

        public IBotContext Reply(string text, string speak = null)
        {
            this._innerContext.Reply(text, speak);
            return this;
        }

        public IBotContext Reply(IActivity activity)
        {
            this._innerContext.Reply(activity);
            return this;
        }

        public void Set(string serviceId, object service)
        {
            this._innerContext.Set(serviceId, service);
        }
    }
}
