using System.Threading.Tasks;
using Microsoft.Bot.Schema;

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

        public bool Responded { get => _innerContext.Responded; set => _innerContext.Responded = value; }

        

        /// <summary>
        /// Get a value by a key.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <returns>The value.</returns>
        public object Get(string key)
        {
            return this._innerContext.Get(key);
        }

        public Task SendActivity(params Activity[] activities)
        {
            return _innerContext.SendActivity(activities); 
        }

        public Task UpdateActivity(Activity activity)
        {
            return _innerContext.UpdateActivity(activity);
        }

        public Task DeleteActivity(string activityId)
        {
            return _innerContext.DeleteActivity(activityId);
        }

        //public IBotContext Reply(string text, string speak = null)
        //{
        //    this._innerContext.Reply(text, speak);
        //    return this;
        //}

        //public IBotContext Reply(IActivity activity)
        //{
        //    this._innerContext.Reply(activity);
        //    return this;
        //}

        /// <summary>
        /// Set the value associated with a key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value to set.</param>
        public void Set(string key, object value)
        {
            this._innerContext.Set(key, value);
        }        
    }
}
