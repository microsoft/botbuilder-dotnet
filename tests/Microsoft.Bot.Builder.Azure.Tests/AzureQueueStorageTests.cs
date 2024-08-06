// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.Bot.Builder.Azure.Queues;
using Microsoft.Bot.Schema;
using Moq;
using Xunit;

namespace Microsoft.Bot.Builder.Azure.Tests
{
    public class AzureQueueStorageTests
    {
        private const string ConnectionString = @"UseDevelopmentStorage=true";

        [Fact]
        public void ConstructorValidation()
        {
            // Should work.
            _ = new AzureQueueStorage(ConnectionString, "queueName");

            // No ConnectionString. Should throw.
            Assert.Throws<ArgumentNullException>(() => new AzureQueueStorage(null, "queueName"));

            // No QueueName. Should throw.
            Assert.Throws<ArgumentNullException>(() => new AzureQueueStorage(ConnectionString, null));
        }

        [Fact]
        public async Task QueueActivityAsync()
        {
            var client = new Mock<QueueClient>();
            var response = new Mock<Response<SendReceipt>>();

            var receipt = QueuesModelFactory.SendReceipt("messageId", DateTimeOffset.MinValue, DateTimeOffset.MinValue, "popReceipt", DateTimeOffset.MinValue);
            response.SetupGet(e => e.Value).Returns(receipt);
            client.Setup(e => e.CreateIfNotExistsAsync(It.IsAny<IDictionary<string, string>>(), It.IsAny<CancellationToken>()));
            client.Setup(e => e.SendMessageAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response.Object);

            var storage = new AzureQueueStorage(client.Object);
            var activity = Activity.CreateMessageActivity() as Activity;
            var result = await storage.QueueActivityAsync(activity, TimeSpan.Zero, TimeSpan.Zero, CancellationToken.None);

            Assert.NotNull(result);
            client.Verify(e => e.CreateIfNotExistsAsync(It.IsAny<IDictionary<string, string>>(), It.IsAny<CancellationToken>()), Times.Once());
            client.Verify(e => e.SendMessageAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once());
        }
    }
}
