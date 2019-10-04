using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Streaming;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;

namespace Microsoft.Bot.Streaming.Tests.Utilities
{
    public class MockBot : IBot
    {
        private readonly DirectLineAdapter _adapter;
        private readonly Func<Schema.Activity, Task<InvokeResponse>> _processActivityAsync;

        public MockBot(Func<Schema.Activity, Task<InvokeResponse>> processActivityAsync, string pipeName = null, DirectLineAdapter adapter = null)
        {
            if (pipeName == null)
            {
                pipeName = Guid.NewGuid().ToString();
            }

            _processActivityAsync = processActivityAsync;
            _adapter = adapter ?? new DirectLineAdapter(null, this, null);
            _adapter.AddNamedPipeConnection(pipeName, this);
        }

        public List<Schema.Activity> ReceivedActivities { get; private set; } = new List<Schema.Activity>();

        public List<Schema.Activity> SentActivities { get; private set; } = new List<Schema.Activity>();

        public async Task<Schema.ResourceResponse> SendActivityAsync(Schema.Activity activity, List<AttachmentStream> attachmentStreams = null)
        {
            SentActivities.Add(activity);

            var requestPath = $"/v3/conversations/{activity.Conversation?.Id}/activities/{activity.Id}";
            var request = StreamingRequest.CreatePost(requestPath);
            request.SetBody(activity);
            attachmentStreams?.ForEach(a =>
            {
                var streamContent = new StreamContent(a.ContentStream);
                streamContent.Headers.TryAddWithoutValidation(HeaderNames.ContentType, a.ContentType);
                request.AddStream(streamContent);
            });

            var serverResponse = await _adapter.ProcessActivityForStreamingChannelAsync(activity, CancellationToken.None).ConfigureAwait(false);

            if (serverResponse.Status == (int)HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<Schema.ResourceResponse>(serverResponse.Body.ToString());
            }

            throw new Exception("SendActivityAsync failed");
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            turnContext.SendActivityAsync(MessageFactory.Text($"Echo: {turnContext.Activity.Text}"), cancellationToken);

            return;

            // await SendActivityAsync(turnContext.Activity);
        }

        public Task<InvokeResponse> ProcessActivityAsync(Schema.Activity activity)
        {
            ReceivedActivities.Add(activity);

            return _processActivityAsync(activity);
        }
    }
}
