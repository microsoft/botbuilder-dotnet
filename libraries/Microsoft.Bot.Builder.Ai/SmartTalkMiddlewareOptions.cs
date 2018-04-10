namespace Microsoft.Bot.Builder.Ai
{
    /// <summary>
    /// Options to alter the default behaviour of the SmartTalk Middleware
    /// </summary>
    public class SmartTalkMiddlewareOptions
    {
        /// <summary>
        /// Azure subscription key for accessing smart talk api. 
        /// </summary>
        public string SubscriptionKey { get; set; }

        /// <summary>
        /// Score threshold of scenario/intents matching to query. Range [0,1]
        /// </summary>
        public float ScoreThreshold { get; set; }

        /// <summary>
        /// The persona for the bot. Friendly,Professional,...
        /// </summary>
        public Persona BotPersona { get; set; }

        /// <summary>
        /// If true then routing of the activity will be stopped when an answer is 
        /// successfully returned by the SmartTalk Middleware
        /// </summary>
        public bool EndActivityRoutingOnAnswer { get; set; }

        /// <summary>
        /// If true, smart talk middleware will only respond 
        /// when query is classified as a chat query.
        /// </summary>
        public bool UseChatQuerySignal { get; set; }

        /// <summary>
        /// If set Default Message is shown when no response is returned.
        /// </summary>
        public string DefaultMessage { get; set; }

        public SmartTalkMiddlewareOptions()
        {
            // By default the middleware will continue routing of the activity
            EndActivityRoutingOnAnswer = false;

            // By default the BotPersona will be friendly.
            BotPersona = Persona.Friendly;

            ScoreThreshold = 0.35F;

            // By default, subscription key will be empty. Works without subscription.
            SubscriptionKey = string.Empty;

            // By default, smart talk will respond based on the intent match and won't consider if query is chat.
            UseChatQuerySignal = false;
        }
    }
}
