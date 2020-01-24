using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters.Facebook.FacebookEvents.Handover;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Facebook.PrimaryTestBot.Bots
{
    public class PrimaryBot : ActivityHandler
    {
        /// <summary>
        /// Id value for the intended secondary receiver app.
        /// </summary>
        private const string SecondaryReceiverAppId = "<SECONDARY RECEIVER APP ID>";

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var activity = MessageFactory.Text("Hello and Welcome!");
            await turnContext.SendActivityAsync(activity, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Attachments != null)
            {
                foreach (var attachment in turnContext.Activity.Attachments)
                {
                    var activity = MessageFactory.Text($" I got {turnContext.Activity.Attachments.Count} attachments");

                    var image = new Attachment(
                        attachment.ContentType,
                        content: attachment.Content);

                    activity.Attachments.Add(image);

                    // Generate an activity with typing indicator, then send it along the other to display the indicator
                    // while the attachment is retrieved
                    var typingActivity = FacebookHelper.GenerateTypingActivity(turnContext.Activity.Conversation.Id);
                    await turnContext.SendActivitiesAsync(new[] { typingActivity, activity }, cancellationToken).ConfigureAwait(false);
                }
            }
            else if (turnContext.Activity.GetChannelData<FacebookMessage>().IsStandby)
            {
                if ((turnContext.Activity as Activity)?.Text == "Invoke a take")
                {
                    var activity = MessageFactory.Text("Hi! I'm the primary bot!");
                    activity.Type = ActivityTypes.Event;

                    //Action
                    ((IEventActivity)activity).Name = HandoverConstants.TakeThreadControl;
                    await turnContext.SendActivityAsync(activity, cancellationToken);
                }
            }
            else
            {
                IActivity activity;

                var messageText = turnContext.Activity.Text.ToLowerInvariant();

                switch (messageText)
                {
                    case "button template":
                        activity = MessageFactory.Attachment(CreateTemplateAttachment(Directory.GetCurrentDirectory() + @"/Resources/ButtonTemplatePayload.json"));
                        break;
                    case "media template":
                        activity = MessageFactory.Attachment(CreateTemplateAttachment(Directory.GetCurrentDirectory() + @"/Resources/MediaTemplatePayload.json"));
                        break;
                    case "generic template":
                        activity = MessageFactory.Attachment(CreateTemplateAttachment(Directory.GetCurrentDirectory() + @"/Resources/GenericTemplatePayload.json"));
                        break;
                    case "hello button":
                        activity = MessageFactory.Text("Hello Human!");
                        break;
                    case "goodbye button":
                        activity = MessageFactory.Text("Goodbye Human!");
                        break;
                    case "chatting":
                        activity = MessageFactory.Text("Hello! How can I help you?");
                        break;
                    case "handover template":
                        activity = MessageFactory.Attachment(CreateTemplateAttachment(Directory.GetCurrentDirectory() + @"/Resources/HandoverTemplatePayload.json"));
                        break;
                    case "handover":
                        activity = MessageFactory.Text("Redirecting...");
                        activity.Type = ActivityTypes.Event;
                        ((IEventActivity)activity).Name = HandoverConstants.PassThreadControl;
                        ((IEventActivity)activity).Value = "inbox";
                        break;
                    case "bot template":
                        activity = MessageFactory.Attachment(CreateTemplateAttachment(Directory.GetCurrentDirectory() + @"/Resources/HandoverBotsTemplatePayload.json"));
                        break;
                    case "secondaryBot":
                        activity = MessageFactory.Text("Redirecting to the secondary bot...");
                        activity.Type = ActivityTypes.Event;

                        //Action
                        ((IEventActivity)activity).Name = HandoverConstants.PassThreadControl;

                        //AppId
                        ((IEventActivity)activity).Value = SecondaryReceiverAppId;
                        break;
                    case "other Bot":
                        activity = MessageFactory.Text($"Secondary bot is requesting me the thread control. Passing thread control!");
                        break;
                    case "display typing indicator":
                        activity = FacebookHelper.GenerateTypingActivity(turnContext.Activity.Conversation.Id);
                        break;
                    default:
                        activity = MessageFactory.Text($"Echo: {turnContext.Activity.Text}");
                        break;
                }

                await turnContext.SendActivityAsync(activity, cancellationToken).ConfigureAwait(false);
            }
        }

        protected override async Task OnEventActivityAsync(ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Value != null)
            {
                var metadata = ((FacebookThreadControl)turnContext.Activity.Value).Metadata;
                var activity = new Activity();
                switch (metadata)
                {
                    case null:
                        var requester = ((FacebookRequestThreadControl)turnContext.Activity.Value).RequestedOwnerAppId;

                        if (requester == HandoverConstants.PageInboxId)
                        {
                            activity = MessageFactory.Text($"The Inbox is requesting me the thread control. Passing thread control!");
                            activity.Type = ActivityTypes.Event;
                            ((IEventActivity)activity).Name = HandoverConstants.PassThreadControl;
                            ((IEventActivity)activity).Value = "inbox";
                            await turnContext.SendActivityAsync(activity, cancellationToken);
                        }

                        break;

                    case HandoverConstants.MetadataRequestThreadControl:
                        activity.Type = ActivityTypes.Event;
                        ((IEventActivity)activity).Name = HandoverConstants.PassThreadControl;
                        ((IEventActivity)activity).Value = SecondaryReceiverAppId;
                        await turnContext.SendActivityAsync(activity, cancellationToken);
                        break;

                    case HandoverConstants.MetadataPassThreadControl:
                    case "Pass thread control from Page Inbox":
                        activity = MessageFactory.Text("Hello Again Human, I'm the bot");
                        await turnContext.SendActivityAsync(activity, cancellationToken);
                        break;
                }
            }
        }

        private static Attachment CreateTemplateAttachment(string filePath)
        {
            var templateAttachmentJson = File.ReadAllText(filePath);
            var templateAttachment = new Attachment()
            {
                ContentType = "template",
                Content = JsonConvert.DeserializeObject(templateAttachmentJson),
            };
            return templateAttachment;
        }
    }
}
