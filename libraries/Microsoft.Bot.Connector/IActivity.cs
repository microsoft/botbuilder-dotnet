using System;
using System.Linq;

namespace Microsoft.Bot.Connector
{
    /// <summary>
    /// Shared properties for all activities
    /// </summary>
    public interface IActivity
    {
        /// <summary>
        /// Activity type
        /// </summary>
        string Type { get; set; }

        /// <summary>
        /// Id for the activity
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// ServiceUrl
        /// </summary>
        string ServiceUrl { get; set; }

        /// <summary>
        /// UTC Time when message was sent
        /// </summary>
        DateTime? Timestamp { get; set; }

        /// <summary>
        /// Client Time when message was sent Ex: 2016-09-23T13:07:49.4714686-07:00
        /// </summary>
        DateTimeOffset? LocalTimestamp { get; set; }

        /// <summary>
        /// Channel this activity is associated with
        /// </summary>
        string ChannelId { get; set; }

        /// <summary>
        /// Sender address data 
        /// </summary>
        ChannelAccount From { get; set; }

        /// <summary>
        /// Address for the conversation that this activity is associated with
        /// </summary>
        ConversationAccount Conversation { get; set; }
        
        /// <summary>
        /// Address that received the message
        /// </summary>
        ChannelAccount Recipient { get; set; }

        /// <summary>
        /// The original id this message is a response to
        /// </summary>
        string ReplyToId { get; set; }

        /// <summary>
        /// Channel specific payload
        /// </summary>
        /// <remarks>
        /// Some channels will provide channel specific data.
        /// 
        /// For a message originating in the channel it might provide the original native schema object for the channel. 
        /// 
        /// For a message coming into the channel it might accept a payload allowing you to create a "native" response for the channel.
        /// 
        /// Example:
        /// * Email - The Email Channel will put the original Email metadata into the ChannelData object for outgoing messages, and will accep
        /// on incoming message a Subject property, and a HtmlBody which can contain Html.  
        /// 
        /// The channel data essentially allows a bot to have access to native functionality on a per channel basis.
        /// </remarks>
        dynamic ChannelData { get; set; }

        /// <summary>
        /// Return IMessageActivity if this is a message activity, null otherwise
        /// </summary>
        IMessageActivity AsMessageActivity();

        /// <summary>
        /// Return IContactRelationUpdateActivity if this is a contactRelationUpdate activity, null otherwise
        /// </summary>
        IContactRelationUpdateActivity AsContactRelationUpdateActivity();

        /// <summary>
        /// Return IInstallationUpdateActivity if this is a installationUpdate activity, null otherwise
        /// </summary>
        IInstallationUpdateActivity AsInstallationUpdateActivity();

        /// <summary>
        /// Return IConversationUpdateActivity if this is a conversationUpdate activity, null otherwise
        /// </summary>
        IConversationUpdateActivity AsConversationUpdateActivity();

        /// <summary>
        /// Return ITypingActivity if this is a typing activity, null otherwise
        /// </summary>
        ITypingActivity AsTypingActivity();

        /// <summary>
        /// Return IEndOfConversationActivity if this is an end-of-conversation activity, null otherwise
        /// </summary>
        IEndOfConversationActivity AsEndOfConversationActivity();

        /// <summary>
        /// Returns IEventActivity if this is an event activity, null otherwise
        /// </summary>
        IEventActivity AsEventActivity();

        /// <summary>
        /// Returns IInvokeActivity if this is an invoke activity, null otherwise
        /// </summary>
        IInvokeActivity AsInvokeActivity();
    }
}
