using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters.Facebook.FacebookEvents.Handover;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Facebook.SecondaryTestBot.Bots
{
    public class SecondaryBot : ActivityHandler
    {
        /// <summary>
        /// Id value for the intended primary receiver app.
        /// </summary>
        private const string PrimaryReceiverAppId = "<PRIMARY RECEIVER APP ID>";

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
                    await turnContext.SendActivityAsync(activity, cancellationToken);
                }
            }
            else if (turnContext.Activity.GetChannelData<FacebookMessage>().IsStandby)
            {
                if ((turnContext.Activity as Activity)?.Text == "Other Bot")
                {
                    var activity = new Activity
                    {
                        Type = ActivityTypes.Event,
                    };

                    // Action
                    ((IEventActivity)activity).Name = HandoverConstants.RequestThreadControl;
                    await turnContext.SendActivityAsync(activity, cancellationToken);
                }
            }
            else
            {
                IActivity activity;

                switch (turnContext.Activity.Text)
                {
                    case "Pass to primary":
                        activity = MessageFactory.Text("Redirecting to the primary bot...");
                        activity.Type = ActivityTypes.Event;
                        ((IEventActivity)activity).Name = HandoverConstants.PassThreadControl;
                        ((IEventActivity)activity).Value = PrimaryReceiverAppId;
                        break;
                    case "Redirected to the bot":
                        activity = MessageFactory.Text("Hello, I'm the secondary bot. How can I help you?");
                        break;
                    case "Invoke a take":
                        activity = MessageFactory.Text($"The Primary bot will take back the control");
                        break;
                    default:
                        activity = MessageFactory.Text($"Echo Secondary: {turnContext.Activity.Text}");
                        break;
                }

                await turnContext.SendActivityAsync(activity, cancellationToken);
            }
        }

        protected override async Task OnEventActivityAsync(ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Value != null)
            {
                var metadata = ((FacebookThreadControl)turnContext.Activity.Value).Metadata;

                if (metadata.Equals(HandoverConstants.MetadataPassThreadControl))
                {
                    var activity = MessageFactory.Text("Hello, I'm the secondary bot. How can I help you?");
                    await turnContext.SendActivityAsync(activity, cancellationToken);
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
