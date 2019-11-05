// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.3.0

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
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var activity = MessageFactory.Text("Hello and Welcome!");
            await turnContext.SendActivityAsync(activity, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.GetChannelData<FacebookMessage>().IsStandby)
            {
                /*request control al primero */
                if ((turnContext.Activity as Activity)?.Text == "OtherBot") 
                {
                    var activity = new Activity();
                    activity.Type = ActivityTypes.Event;

                    //Action
                    (activity as IEventActivity).Name = "request_thread_control";
                    await turnContext.SendActivityAsync(activity, cancellationToken);
                }
            }
            else if (turnContext.Activity.Attachments != null)
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
            else
            {
                IActivity activity;

                switch (turnContext.Activity.Text)
                {
                    case "Pass to primary":
                        activity = MessageFactory.Text("Redirecting to the primary bot...");
                        activity.Type = ActivityTypes.Event;
                        (activity as IEventActivity).Name = "pass_thread_control";
                        (activity as IEventActivity).Value = "<PRIMARY RECEIVER APP ID>";
                        break;
                    case "Redirected to the bot":
                        activity = MessageFactory.Text("Hello, I'm the secondary bot. How can I help you?");
                        break;
                    case "Little":
                        activity = MessageFactory.Text($"You have spoken the forbidden word!"); // escribe que el primero hizo take control
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
                var metadata = (turnContext.Activity.Value as FacebookThreadControl).Metadata;

                if (metadata.Equals("Pass thread control"))
                {
                    var activity = MessageFactory.Text("Hello Human, I'm the secondary bot to help you!");
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
