using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.DirectLine;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Streaming;
using Microsoft.Bot.Streaming.Transport.NamedPipes;
using Microsoft.Net.Http.Headers;
using Newtonsoft;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Bot.Builder.Streaming.Tests
{
    public class EndToEndTests
    {
        [Fact]
        public async Task EndToEnd_PostActivityToBot()
        {
            // Arrange
            object syncLock = new object();
            MockBot mockBot = null;
            string pipeName = Guid.NewGuid().ToString();
            var client = new NamedPipeClient(pipeName);
            var conversation = new Conversation(conversationId: Guid.NewGuid().ToString());
            var processActivity = ProcessActivityWithAttachments(mockBot, conversation);
            var adapter = new BotFrameworkHttpAdapter();
            mockBot = new MockBot(processActivity);
            var request = GetStreamingRequestWithoutAttachments(conversation.ConversationId);

            // Act
            adapter.UseNamedPipeAsync(pipeName, mockBot);
            await client.ConnectAsync();
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(200, response.StatusCode);
        }

        [Fact]
        public async Task ConnectToNamedPipe()
        {
            // Arrange
            Exception result = null;
            object syncLock = new object();
            string pipeName = Guid.NewGuid().ToString();
            MockBot mockBot = null;
            var client = new NamedPipeClient(pipeName);
            var conversation = new Conversation(conversationId: Guid.NewGuid().ToString());
            var processActivity = ProcessActivityWithAttachments(mockBot, conversation);
            BotFrameworkHttpAdapter adapter;
            mockBot = new MockBot(processActivity, pipeName);
            adapter = new BotFrameworkHttpAdapter();

            // Act
            await client.ConnectAsync();
            try
            {
                adapter.UseNamedPipeAsync(pipeName, mockBot);
            }
            catch (Exception ex)
            {
                result = ex;
            }

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task ConnectToMultiplePipes()
        {
            // Arrange
            Exception result = null;
            object syncLock = new object();
            string pipeNameA = Guid.NewGuid().ToString();
            string pipeNameB = Guid.NewGuid().ToString();
            MockBot mockBot = null;
            var client = new NamedPipeClient(pipeNameA);
            var conversation = new Conversation(conversationId: Guid.NewGuid().ToString());
            var processActivity = ProcessActivityWithAttachments(mockBot, conversation);
            BotFrameworkHttpAdapter adapter;
            mockBot = new MockBot(processActivity, pipeNameA);
            adapter = new BotFrameworkHttpAdapter();

            // Act
            await client.ConnectAsync();
            try
            {
                adapter.UseNamedPipeAsync(pipeNameA, mockBot);
                adapter.UseNamedPipeAsync(pipeNameB, mockBot);
            }
            catch (Exception ex)
            {
                result = ex;
            }

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task RespondsOnCorrectConnection()
        {
            // Arrange
            ReceiveResponse result = null;
            object syncLock = new object();
            string pipeNameA = Guid.NewGuid().ToString();
            string pipeNameB = Guid.NewGuid().ToString();
            MockBot mockBot = null;
            var client = new NamedPipeClient(pipeNameA);
            var conversation = new Conversation(conversationId: Guid.NewGuid().ToString());
            var processActivity = ProcessActivityWithAttachments(mockBot, conversation);
            BotFrameworkHttpAdapter adapter;
            mockBot = new MockBot(processActivity, pipeNameA);
            adapter = new BotFrameworkHttpAdapter();

            // Act
            await client.ConnectAsync();
            try
            {
                adapter.UseNamedPipeAsync(pipeNameA, mockBot);
                adapter.UseNamedPipeAsync(pipeNameB, mockBot);

                client.ConnectAsync();
                result = await client.SendAsync(GetStreamingRequestWithoutAttachments(Guid.NewGuid().ToString()));
            }
            catch (Exception ex)
            {
                Assert.Null(ex);
            }

            // Assert
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task EndToEnd_PostActivityWithAttachmentToBot()
        {
            // Arrange
            object syncLock = new object();
            MockBot mockBot = null;
            string pipeName = Guid.NewGuid().ToString();
            var client = new NamedPipeClient(pipeName);
            var conversation = new Conversation(conversationId: Guid.NewGuid().ToString());
            var processActivity = ProcessActivityWithAttachments(mockBot, conversation);
            var adapter = new BotFrameworkHttpAdapter();
            mockBot = new MockBot(processActivity);
            var requestWithAttachments = GetStreamingRequestWithAttachment(conversation.ConversationId);

            // Act
            adapter.UseNamedPipeAsync(pipeName, mockBot);
            await client.ConnectAsync();
            var response = await client.SendAsync(requestWithAttachments);

            // Assert
            Assert.Equal(200, response.StatusCode);
        }

        private static StreamingRequest GetStreamingRequestWithoutAttachments(string conversationId)
        {
            var conId = string.IsNullOrWhiteSpace(conversationId) ? Guid.NewGuid().ToString() : conversationId;

            var request = new StreamingRequest()
            {
                Verb = "POST",
                Path = $"/v3/directline/conversations/{conId}/activities",
            };

            var activity = new Schema.Activity()
            {
                Type = "message",
                Text = "hello",
                ServiceUrl = "urn:test:namedpipe:testPipes",
                From = new Schema.ChannelAccount()
                {
                    Id = "123",
                    Name = "Fred",
                },
                Conversation = new Schema.ConversationAccount(null, null, conId, null, null, null, null),
            };

            request.SetBody(activity);

            return request;
        }

        private static StreamingRequest GetStreamingRequestWithAttachment(string conversationId)
        {
            var conId = string.IsNullOrWhiteSpace(conversationId) ? Guid.NewGuid().ToString() : conversationId;
            var attachmentData = "blah blah i am a stream!";
            var streamContent = new MemoryStream(Encoding.UTF8.GetBytes(attachmentData));
            var attachmentStream = new AttachmentStream("botframework-stream", streamContent);

            var request = new StreamingRequest()
            {
                Verb = "POST",
                Path = $"/v3/directline/conversations/{conId}/activities",
            };
            var activity = new Schema.Activity()
            {
                Type = "message",
                Text = "hello",
                ServiceUrl = "urn:test:namedpipe:testPipes",
                From = new Schema.ChannelAccount()
                {
                    Id = "123",
                    Name = "Fred",
                },
                Conversation = new Schema.ConversationAccount(null, null, conId, null, null, null, null),
            };

            request.SetBody(activity);

            var contentStream = new StreamContent(attachmentStream.ContentStream);
            contentStream.Headers.TryAddWithoutValidation(HeaderNames.ContentType, attachmentStream.ContentType);
            request.AddStream(contentStream);

            return request;
        }

        private static Func<Schema.Activity, Task<InvokeResponse>> ProcessActivityWithAttachments(MockBot mockBot, Conversation conversation)
        {
            var attachmentStreamData = new List<string>();

            Func<Schema.Activity, Task<InvokeResponse>> processActivity = async (activity) =>
            {
                if (activity.Attachments != null)
                {
                    foreach (Schema.Attachment attachment in activity.Attachments)
                    {
                        if (attachment.ContentType.Contains("botframework-stream"))
                        {
                            var stream = attachment.Content as Stream;
                            using (var reader = new StreamReader(stream, Encoding.UTF8))
                            {
                                attachmentStreamData.Add(reader.ReadToEnd());
                            }

                            var testActivity = new Schema.Activity()
                            {
                                Type = "message",
                                Text = "received from bot",
                                From = new Schema.ChannelAccount()
                                {
                                    Id = "bot",
                                    Name = "bot",
                                },
                                Conversation = new Schema.ConversationAccount(null, conversation.ConversationId, null),
                            };
                            var attachmentData1 = "blah blah i am a stream!";
                            var streamContent1 = new MemoryStream(Encoding.UTF8.GetBytes(attachmentData1));
                            var attachmentData2 = "blah blah i am also a stream!";
                            var streamContent2 = new MemoryStream(Encoding.UTF8.GetBytes(attachmentData2));
                            await mockBot.SendActivityAsync(testActivity, new List<AttachmentStream>()
                            {
                                new AttachmentStream("bot-stream1", streamContent1),
                            });
                            await mockBot.SendActivityAsync(testActivity, new List<AttachmentStream>()
                            {
                                new AttachmentStream("bot-stream2", streamContent2),
                            });
                        }
                    }
                }

                return null;
            };
            return processActivity;
        }

        private class MockBot : IBot
        {
            private readonly BotFrameworkHttpAdapter _adapter;
            private readonly Func<Schema.Activity, Task<InvokeResponse>> _processActivityAsync;

            public MockBot(Func<Schema.Activity, Task<InvokeResponse>> processActivityAsync, string pipeName = "testPipes", BotFrameworkHttpAdapter adapter = null)
            {
                _processActivityAsync = processActivityAsync;
                _adapter = adapter ?? new BotFrameworkHttpAdapter();
                _adapter.UseNamedPipeAsync(pipeName, this);
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
                   return JsonConvert.DeserializeObject<Schema.ResourceResponse>(serverResponse.Body.ToString());
                }

                throw new Exception("SendActivityAsync failed");
        }

            public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
            {
                turnContext.SendActivityAsync(MessageFactory.Text($"Echo: {turnContext.Activity.Text}"), cancellationToken);
                return;
            }
        }

        private class AttachmentStream
        {
            public AttachmentStream(string contentType, Stream stream)
            {
                ContentType = contentType ?? throw new ArgumentNullException(nameof(contentType));
                ContentStream = stream ?? throw new ArgumentNullException(nameof(stream));
            }

            public string ContentType { get; }

            public Stream ContentStream { get; }
        }
    }
}
