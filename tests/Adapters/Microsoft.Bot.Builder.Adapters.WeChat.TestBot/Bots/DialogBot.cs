// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema.Requests;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema.Requests.Events;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.WeChat.TestBot
{
    // This IBot implementation can run any type of Dialog. The use of type parameterization is to allows multiple different bots
    // to be run at different endpoints within the same project. This can be achieved by defining distinct Controller types
    // each with dependency on distinct IBot types, this way ASP Dependency Injection can glue everything together without ambiguity.
    // The ConversationState is used by the Dialog system. The UserState isn't, however, it might have been used in a Dialog implementation,
    // and the requirement is that all BotState objects are saved at the end of a turn.
    public class DialogBot<T> : ActivityHandler
        where T : Dialog
    {
        private readonly BotState _conversationState;
        private readonly Dialog _dialog;
        private readonly ILogger _logger;
        private readonly BotState _userState;

        public DialogBot(ConversationState conversationState, UserState userState, T dialog, ILogger<DialogBot<T>> logger)
        {
            _conversationState = conversationState;
            _userState = userState;
            _dialog = dialog;
            _logger = logger;
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            var echo = turnContext.Activity.CreateReply();
            echo.Text = "Echo: " + turnContext.Activity.Text;
            await turnContext.SendActivityAsync(echo);
            var clickEvent = turnContext.Activity.ChannelData as ClickEvent;
            var baseEvent = turnContext.Activity.ChannelData as RequestEvent;
            var locationEvent = turnContext.Activity.ChannelData as LocationRequest;
            var imageRequest = turnContext.Activity.ChannelData as ImageRequest;
            var videoRquest = turnContext.Activity.ChannelData as VideoRequest;
            var voiceRquest = turnContext.Activity.ChannelData as VoiceRequest;
            if (clickEvent != null && clickEvent.EventKey == "V1001_TODAY_MUSIC")
            {
                var reply = turnContext.Activity.CreateReply();
                reply.Attachments.Add(Cards.GetAudioCard().ToAttachment());
                await turnContext.SendActivityAsync(reply);
            }
            else if (clickEvent != null && clickEvent.EventKey == "V1001_GOOD")
            {
                var reply = turnContext.Activity.CreateReply();
                reply.Text = "谢谢赞赏！";
                await turnContext.SendActivityAsync(reply);
            }
            else if (locationEvent != null)
            {
                var reply = turnContext.Activity.CreateReply();
                reply.Text = JsonConvert.SerializeObject(locationEvent);
                await turnContext.SendActivityAsync(reply);
            }
            else if (baseEvent != null)
            {
                var reply = turnContext.Activity.CreateReply();
                reply.Text = JsonConvert.SerializeObject(baseEvent);
                await turnContext.SendActivityAsync(reply);
            }
            else if (imageRequest != null)
            {
                var reply = turnContext.Activity.CreateReply();
                reply.Text = JsonConvert.SerializeObject(imageRequest);
                await turnContext.SendActivityAsync(reply);
            }
            else if (videoRquest != null)
            {
                var reply = turnContext.Activity.CreateReply();
                reply.Text = JsonConvert.SerializeObject(videoRquest);
                await turnContext.SendActivityAsync(reply);
            }
            else if (voiceRquest != null)
            {
                var reply = turnContext.Activity.CreateReply();
                reply.Text = JsonConvert.SerializeObject(voiceRquest);
                await turnContext.SendActivityAsync(reply);
            }
            else
            {
                await base.OnTurnAsync(turnContext, cancellationToken);
            }
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Running dialog with Message Activity.");

            // Run the Dialog with the new message Activity.
            await _dialog.Run(turnContext, _conversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
        }

        protected override async Task OnEventActivityAsync(ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken)
        {
            turnContext.Activity.TryGetChannelData(out string channelData);
            var echo = Activity.CreateMessageActivity();
            echo.Text = channelData;
            await turnContext.SendActivityAsync(turnContext.Activity);
        }
    }
}
