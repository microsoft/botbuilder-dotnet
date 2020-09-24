// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector.DirectLine;
using Microsoft.Net.Http.Headers;

namespace Microsoft.Bot.Streaming.Tests.Utilities
{
    public static class TestHelper
    {
        public static StreamingRequest GetStreamingRequestWithoutAttachments(string conversationId)
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

        public static StreamingRequest GetStreamingRequestWithAttachment(string conversationId)
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

        public static Func<Schema.Activity, Task<InvokeResponse>> ProcessActivityWithAttachments(MockBot mockBot, Conversation conversation)
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
    }
}
