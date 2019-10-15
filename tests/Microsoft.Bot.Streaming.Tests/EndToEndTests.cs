// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

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
using Microsoft.Bot.Streaming.Tests.Utilities;
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
        public async void EndToEnd_PostActivityToBot()
        {
            // Arrange
            object syncLock = new object();
            string pipeName = Guid.NewGuid().ToString();
            MockBot mockBot = null;
            var client = new NamedPipeClient(pipeName);
            var conversation = new Conversation(conversationId: Guid.NewGuid().ToString());
            var processActivity = TestHelper.ProcessActivityWithAttachments(mockBot, conversation);
            mockBot = new MockBot(processActivity, pipeName);
            var requestWithOutActivity = TestHelper.GetStreamingRequestWithoutAttachments(conversation.ConversationId);

            // Act
            await client.ConnectAsync();
            var response = await client.SendAsync(requestWithOutActivity);
            client.Dispose();

            // Assert
            Assert.Equal(200, response.StatusCode);
        }

        [Fact]
        public async void ConnectToNamedPipe()
        {
            // Arrange
            Exception result = null;
            object syncLock = new object();
            string pipeName = Guid.NewGuid().ToString();
            MockBot mockBot = null;
            var client = new NamedPipeClient(pipeName);
            var conversation = new Conversation(conversationId: Guid.NewGuid().ToString());
            var processActivity = TestHelper.ProcessActivityWithAttachments(mockBot, conversation);
            BotFrameworkHttpAdapter adapter;
            mockBot = new MockBot(processActivity, pipeName);
            adapter = new BotFrameworkHttpAdapter();

            // Act
            await client.ConnectAsync();
            try
            {
                // Not awaiting on purpose
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                adapter.UseNamedPipeAsync(pipeName, mockBot);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
            catch (Exception ex)
            {
                result = ex;
            }

            client.Dispose();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async void ConnectToMultiplePipes()
        {
            // Arrange
            Exception result = null;
            object syncLock = new object();
            string pipeNameA = Guid.NewGuid().ToString();
            string pipeNameB = Guid.NewGuid().ToString();
            MockBot mockBot = null;
            var client = new NamedPipeClient(pipeNameA);
            var conversation = new Conversation(conversationId: Guid.NewGuid().ToString());
            var processActivity = TestHelper.ProcessActivityWithAttachments(mockBot, conversation);
            BotFrameworkHttpAdapter adapter;
            mockBot = new MockBot(processActivity, pipeNameA);
            adapter = new BotFrameworkHttpAdapter();

            // Act
            await client.ConnectAsync();
            try
            {
                // Not awaiting on purpose
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                adapter.UseNamedPipeAsync(pipeNameA, mockBot);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                adapter.UseNamedPipeAsync(pipeNameB, mockBot);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
            catch (Exception ex)
            {
                result = ex;
            }

            client.Dispose();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async void RespondsOnCorrectConnection()
        {
            // Arrange
            ReceiveResponse result = null;
            object syncLock = new object();
            string pipeNameA = Guid.NewGuid().ToString();
            string pipeNameB = Guid.NewGuid().ToString();
            MockBot mockBot = null;
            var client = new NamedPipeClient(pipeNameA);
            var conversation = new Conversation(conversationId: Guid.NewGuid().ToString());
            var processActivity = TestHelper.ProcessActivityWithAttachments(mockBot, conversation);
            BotFrameworkHttpAdapter adapter;
            mockBot = new MockBot(processActivity, pipeNameA);
            adapter = new BotFrameworkHttpAdapter();

            // Act
            await client.ConnectAsync();
            try
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                adapter.UseNamedPipeAsync(pipeNameA, mockBot);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                adapter.UseNamedPipeAsync(pipeNameB, mockBot);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                client.ConnectAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                result = await client.SendAsync(TestHelper.GetStreamingRequestWithoutAttachments(conversation.ConversationId));
            }
            catch (Exception ex)
            {
                Assert.Null(ex);
            }

            client.Dispose();

            // Assert
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task EndToEnd_PostActivityWithAttachmentToBot()
        {
            // Arrange
            ReceiveResponse result = null;
            object syncLock = new object();
            string pipeNameA = Guid.NewGuid().ToString();
            string pipeNameB = Guid.NewGuid().ToString();
            MockBot mockBot = null;
            var client = new NamedPipeClient(pipeNameA);
            var conversation = new Conversation(conversationId: Guid.NewGuid().ToString());
            var processActivity = TestHelper.ProcessActivityWithAttachments(mockBot, conversation);
            BotFrameworkHttpAdapter adapter;
            mockBot = new MockBot(processActivity, pipeNameA);
            adapter = new BotFrameworkHttpAdapter();

            // Act
            await client.ConnectAsync();
            try
            {
                // Not awaiting on purpose
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                adapter.UseNamedPipeAsync(pipeNameA, mockBot);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                adapter.UseNamedPipeAsync(pipeNameB, mockBot);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                client.ConnectAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                result = await client.SendAsync(TestHelper.GetStreamingRequestWithAttachment(conversation.ConversationId));
            }
            catch (Exception ex)
            {
                Assert.Null(ex);
            }

            client.Dispose();

            // Assert
            Assert.Equal(200, result.StatusCode);
        }
    }
}
