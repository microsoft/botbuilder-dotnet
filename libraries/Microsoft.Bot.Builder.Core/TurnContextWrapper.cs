using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Utility class to allow you to create custom TurnContext wrapper which wraps someone elses TurnContext 
    /// </summary>
    /// <remarks>
    /// Adapters create the ITurnContext implementation which is then passed to the bot's logic handler
    /// This class allows you to create your own ITurnContext which delegates to the BotContext passed to you
    /// It simply passes all ITurnContext calls through to the inner ITurnContext.
    /// </remarks>
    public class TurnContextWrapper : ITurnContext
    {
        private ITurnContext _innerContext;

        public TurnContextWrapper(ITurnContext context)
        {
            this._innerContext = context;
        }

        public BotAdapter Adapter => this._innerContext.Adapter;

        public Activity Activity => this._innerContext.Activity;

        public ITurnContextServiceCollection Services => this._innerContext.Services;

        public bool Responded { get => _innerContext.Responded; set => _innerContext.Responded = value; }
        
        public Task<ResourceResponse> SendActivity(string textRepliesToSend, string speak = null, string inputHint = null)
        {
            return _innerContext.SendActivity(textRepliesToSend, speak, inputHint);
        }
        
        public Task<ResourceResponse> SendActivity(IActivity activity)
        {
            return _innerContext.SendActivity(activity); 
        }

        public Task<ResourceResponse[]> SendActivities(params IActivity[] activities)
        {
            return _innerContext.SendActivities(activities);
        }

        public Task<ResourceResponse> UpdateActivity(IActivity activity)
        {
            return _innerContext.UpdateActivity(activity);
        }

        public Task DeleteActivity(string activityId)
        {
            return _innerContext.DeleteActivity(activityId);
        }

        public Task DeleteActivity(ConversationReference conversationReference)
        {
            return _innerContext.DeleteActivity(conversationReference);
        }

        public ITurnContext OnSendActivities(SendActivitiesHandler handler)
        {
            this._innerContext.OnSendActivities(handler);
            return this;
        }

        public ITurnContext OnUpdateActivity(UpdateActivityHandler handler)
        {
            this._innerContext.OnUpdateActivity(handler);
            return this;
        }

        public ITurnContext OnDeleteActivity(DeleteActivityHandler handler)
        {
            this._innerContext.OnDeleteActivity(handler);
            return this;
        }
    }
}