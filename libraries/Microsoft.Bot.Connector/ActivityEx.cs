using System;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Connector
{
    /// <summary>
    /// An Activity is the basic communication type for the Bot Framework 3.0 protocol
    /// </summary>
    /// <remarks>
    /// The Activity class contains all properties that individual, more specific activities
    /// could contain. It is a superset type.
    /// </remarks>
    public partial class Activity :
        IActivity,
        IConversationUpdateActivity,
        IContactRelationUpdateActivity,
        IInstallationUpdateActivity,
        IMessageActivity,
        ITypingActivity,
        IEndOfConversationActivity,
        IEventActivity,
        IInvokeActivity
    {
        /// <summary>
        /// Content-type for an Activity
        /// </summary>
        public const string ContentType = "application/vnd.microsoft.activity";

        /// <summary>
        /// Take a message and create a reply message for it with the routing information 
        /// set up to correctly route a reply to the source message
        /// </summary>
        /// <param name="text">text you want to reply with</param>
        /// <param name="locale">language of your reply</param>
        /// <returns>message set up to route back to the sender</returns>
        public Activity CreateReply(string text = null, string locale = null)
        {
            Activity reply = new Activity();
            reply.Type = ActivityTypes.Message;
            reply.Timestamp = DateTime.UtcNow;
            reply.From = new ChannelAccount(id: this.Recipient.Id, name: this.Recipient.Name);
            reply.Recipient = new ChannelAccount(id: this.From.Id, name: this.From.Name);
            reply.ReplyToId = this.Id;
            reply.ServiceUrl = this.ServiceUrl;
            reply.ChannelId = this.ChannelId;
            reply.Conversation = new ConversationAccount(isGroup: this.Conversation.IsGroup, id: this.Conversation.Id, name: this.Conversation.Name);
            reply.Text = text ?? String.Empty;
            reply.Locale = locale ?? this.Locale;
            return reply;
        }

        /// <summary>
        /// Extension data for overflow of properties
        /// </summary>
        [JsonExtensionData(ReadData = true, WriteData = true)]
        public JObject Properties { get; set; } = new JObject();


        /// <summary>
        /// Create an instance of the Activity class with IMessageActivity masking
        /// </summary>
        public static IMessageActivity CreateMessageActivity() { return new Activity(ActivityTypes.Message); }

        /// <summary>
        /// Create an instance of the Activity class with IContactRelationUpdateActivity masking
        /// </summary>
        public static IContactRelationUpdateActivity CreateContactRelationUpdateActivity() { return new Activity(ActivityTypes.ContactRelationUpdate); }

        /// <summary>
        /// Create an instance of the Activity class with IConversationUpdateActivity masking
        /// </summary>
        public static IConversationUpdateActivity CreateConversationUpdateActivity() { return new Activity(ActivityTypes.ConversationUpdate); }

        /// <summary>
        /// Create an instance of the Activity class with ITypingActivity masking
        /// </summary>
        public static ITypingActivity CreateTypingActivity() { return new Activity(ActivityTypes.Typing); }

        /// <summary>
        /// Create an instance of the Activity class with IActivity masking
        /// </summary>
        public static IActivity CreatePingActivity() { return new Activity(ActivityTypes.Ping); }

        /// <summary>
        /// Create an instance of the Activity class with IEndOfConversationActivity masking
        /// </summary>
        public static IEndOfConversationActivity CreateEndOfConversationActivity() { return new Activity(ActivityTypes.EndOfConversation); }

        /// <summary>
        /// Create an instance of the Activity class with an IEventActivity masking
        /// </summary>
        public static IEventActivity CreateEventActivity() { return new Activity(ActivityTypes.Event); }

        /// <summary>
        /// Create an instance of the Activity class with IInvokeActivity masking
        /// </summary>
        public static IInvokeActivity CreateInvokeActivity() { return new Activity(ActivityTypes.Invoke); }

        /// <summary>
        /// True if the Activity is of the specified activity type
        /// </summary>
        protected bool IsActivity(string activity) { return string.Compare(this.Type?.Split('/').First(), activity, true) == 0; }

        /// <summary>
        /// Return an IMessageActivity mask if this is a message activity
        /// </summary>
        public IMessageActivity AsMessageActivity() { return IsActivity(ActivityTypes.Message) ? this : null; }

        /// <summary>
        /// Return an IContactRelationUpdateActivity mask if this is a contact relation update activity
        /// </summary>
        public IContactRelationUpdateActivity AsContactRelationUpdateActivity() { return IsActivity(ActivityTypes.ContactRelationUpdate) ? this : null; }

        /// <summary>
        /// Return an IInstallationUpdateActivity mask if this is a installation update activity
        /// </summary>
        public IInstallationUpdateActivity AsInstallationUpdateActivity() { return IsActivity(ActivityTypes.InstallationUpdate) ? this : null; }

        /// <summary>
        /// Return an IConversationUpdateActivity mask if this is a conversation update activity
        /// </summary>
        public IConversationUpdateActivity AsConversationUpdateActivity() { return IsActivity(ActivityTypes.ConversationUpdate) ? this : null; }

        /// <summary>
        /// Return an ITypingActivity mask if this is a typing activity
        /// </summary>
        public ITypingActivity AsTypingActivity() { return IsActivity(ActivityTypes.Typing) ? this : null; }

        /// <summary>
        /// Return an IEndOfConversationActivity mask if this is an end of conversation activity
        /// </summary>
        public IEndOfConversationActivity AsEndOfConversationActivity() { return IsActivity(ActivityTypes.EndOfConversation) ? this : null; }

        /// <summary>
        /// Return an IEventActivity mask if this is an event activity
        /// </summary>
        public IEventActivity AsEventActivity() { return IsActivity(ActivityTypes.Event) ? this : null; }

        /// <summary>
        /// Return an IInvokeActivity mask if this is an invoke activity
        /// </summary>
        public IInvokeActivity AsInvokeActivity() { return IsActivity(ActivityTypes.Invoke) ? this : null; }

        /// <summary>
        /// Maps type to activity types 
        /// </summary>
        /// <param name="type"> The type.</param>
        /// <returns> The activity type.</returns>
        public static string GetActivityType(string type)
        {
            if (String.Equals(type, ActivityTypes.Message, StringComparison.OrdinalIgnoreCase))
                return ActivityTypes.Message;

            if (String.Equals(type, ActivityTypes.ContactRelationUpdate, StringComparison.OrdinalIgnoreCase))
                return ActivityTypes.ContactRelationUpdate;

            if (String.Equals(type, ActivityTypes.ConversationUpdate, StringComparison.OrdinalIgnoreCase))
                return ActivityTypes.ConversationUpdate;

            if (String.Equals(type, ActivityTypes.DeleteUserData, StringComparison.OrdinalIgnoreCase))
                return ActivityTypes.DeleteUserData;

            if (String.Equals(type, ActivityTypes.Typing, StringComparison.OrdinalIgnoreCase))
                return ActivityTypes.Typing;

            if (String.Equals(type, ActivityTypes.Ping, StringComparison.OrdinalIgnoreCase))
                return ActivityTypes.Ping;

            return $"{Char.ToLower(type[0])}{type.Substring(1)}";
        }

        /// <summary>
        /// Checks if this (message) activity has content.
        /// </summary>
        /// <returns>Returns true, if this message has any content to send. False otherwise.</returns>
        public bool HasContent()
        {
            if (!String.IsNullOrWhiteSpace(this.Text))
                return true;

            if (!String.IsNullOrWhiteSpace(this.Summary))
                return true;

            if (this.Attachments != null && this.Attachments.Any())
                return true;

            if (this.ChannelData != null)
                return true;

            return false;
        }

        /// <summary>
        /// Resolves the mentions from the entities of this (message) activity.
        /// </summary>
        /// <returns>The array of mentions or an empty array, if none found.</returns>
        public Mention[] GetMentions()
        {
            return this.Entities?.Where(entity => String.Compare(entity.Type, "mention", ignoreCase: true) == 0)
                .Select(e => e.Properties.ToObject<Mention>()).ToArray() ?? new Mention[0];
        }
    }

    public static class ActivityExtensions
    {
        /// <summary>
        /// Get StateClient appropriate for this activity
        /// </summary>
        /// <param name="credentials">credentials for bot to access state api</param>
        /// <param name="serviceUrl">alternate serviceurl to use for state service</param>
        /// <param name="handlers"></param>
        /// <param name="activity"></param>
        /// <returns></returns>
        public static StateClient GetStateClient(this IActivity activity, MicrosoftAppCredentials credentials, string serviceUrl = null, params DelegatingHandler[] handlers)
        {
            bool useServiceUrl = (activity.ChannelId == "emulator");
            if (useServiceUrl)
                return new StateClient(new Uri(activity.ServiceUrl), credentials: credentials, handlers: handlers);

            if (serviceUrl != null)
                return new StateClient(new Uri(serviceUrl), credentials: credentials, handlers: handlers);

            return new StateClient(credentials, true, handlers);
        }

        /// <summary>
        /// Get StateClient appropriate for this activity
        /// </summary>
        /// <param name="microsoftAppId"></param>
        /// <param name="microsoftAppPassword"></param>
        /// <param name="serviceUrl">alternate serviceurl to use for state service</param>
        /// <param name="handlers"></param>
        /// <param name="activity"></param>
        /// <returns></returns>
        public static StateClient GetStateClient(this IActivity activity, string microsoftAppId = null, string microsoftAppPassword = null, string serviceUrl = null, params DelegatingHandler[] handlers) => GetStateClient(activity, new MicrosoftAppCredentials(microsoftAppId, microsoftAppPassword), serviceUrl, handlers);

        /// <summary>
        /// Return the "major" portion of the activity
        /// </summary>
        /// <param name="activity"></param>
        /// <returns>normalized major portion of the activity, aka message/... will return "message"</returns>
        public static string GetActivityType(this IActivity activity)
        {
            var type = activity.Type.Split('/').First();
            return Activity.GetActivityType(type);
        }

        /// <summary>
        /// Get channeldata as typed structure
        /// </summary>
        /// <param name="activity"></param>
        /// <typeparam name="TypeT">type to use</typeparam>
        /// <returns>typed object or default(TypeT)</returns>
        public static TypeT GetChannelData<TypeT>(this IActivity activity)
        {
            if (activity.ChannelData == null)
                return default(TypeT);
            return ((JObject)activity.ChannelData).ToObject<TypeT>();
        }


        /// <summary>
        /// Get channeldata as typed structure
        /// </summary>
        /// <param name="activity"></param>
        /// <typeparam name="TypeT">type to use</typeparam>
        /// <param name="instance">The resulting instance, if possible</param>
        /// <returns>
        /// <c>true</c> if value of <seealso cref="IActivity.ChannelData"/> was coerceable to <typeparamref name="TypeT"/>, <c>false</c> otherwise.
        /// </returns>
        public static bool TryGetChannelData<TypeT>(this IActivity activity,
            out TypeT instance)
        {
            instance = default(TypeT);

            try
            {
                if (activity.ChannelData == null)
                {
                    return false;
                }

                instance = GetChannelData<TypeT>(activity);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Is there a mention of Id in the Text Property 
        /// </summary>
        /// <param name="id">ChannelAccount.Id</param>
        /// <param name="activity"></param>
        /// <returns>true if this id is mentioned in the text</returns>
        public static bool MentionsId(this IMessageActivity activity, string id)
        {
            return activity.GetMentions().Where(mention => mention.Mentioned.Id == id).Any();
        }

        /// <summary>
        /// Is there a mention of Recipient.Id in the Text Property 
        /// </summary>
        /// <param name="activity"></param>
        /// <returns>true if this id is mentioned in the text</returns>
        public static bool MentionsRecipient(this IMessageActivity activity)
        {
            return activity.GetMentions().Where(mention => mention.Mentioned.Id == activity.Recipient.Id).Any();
        }

        /// <summary>
        /// Remove recipient mention text from Text property
        /// </summary>
        /// <param name="activity"></param>
        /// <returns>new .Text property value</returns>
        public static string RemoveRecipientMention(this IMessageActivity activity)
        {
            return activity.RemoveMentionText(activity.Recipient.Id);
        }

        /// <summary>
        /// Replace any mention text for given id from Text property
        /// </summary>
        /// <param name="id">id to match</param>
        /// <param name="activity"></param>
        /// <returns>new .Text property value</returns>
        public static string RemoveMentionText(this IMessageActivity activity, string id)
        {
            foreach (var mention in activity.GetMentions().Where(mention => mention.Mentioned.Id == id))
            {
                activity.Text = Regex.Replace(activity.Text, mention.Text, "", RegexOptions.IgnoreCase);
            }
            return activity.Text;
        }
    }
}
