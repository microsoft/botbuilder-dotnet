// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.Streaming;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;

namespace Microsoft.Bot.Streaming.Tests.Utilities
{
    public class MockBot : IBot
    {
        private readonly BotFrameworkHttpAdapter _adapter;
        private readonly Func<Schema.Activity, Task<InvokeResponse>> _processActivityAsync;

        public MockBot(Func<Schema.Activity, Task<InvokeResponse>> processActivityAsync, string pipeName = null, BotFrameworkHttpAdapter adapter = null)
        {
            if (pipeName == null)
            {
                pipeName = Guid.NewGuid().ToString();
            }

            _processActivityAsync = processActivityAsync;
            _adapter = adapter ?? new BotFrameworkHttpAdapter();
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

            var serverResponse = await _adapter.ProcessStreamingActivityAsync(activity, OnTurnAsync, CancellationToken.None).ConfigureAwait(false);

            if (serverResponse.Status == (int)HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<Schema.ResourceResponse>(serverResponse.Body.ToString(), new JsonSerializerSettings { MaxDepth = null });
            }

            throw new Exception("SendActivityAsync failed");
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            await turnContext.SendActivityAsync(MessageFactory.Text($"Echo: {turnContext.Activity.Text}"), cancellationToken);
            return;
        }

        public Task<InvokeResponse> ProcessActivityAsync(Schema.Activity activity)
        {
            ReceivedActivities.Add(activity);

            return _processActivityAsync(activity);
        }
    }
}
