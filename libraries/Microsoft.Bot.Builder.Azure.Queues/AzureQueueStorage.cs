// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Azure.Queues
{
    /// <summary>
    /// Service used to add messages to an Azure.Storage.Queues.
    /// </summary>
    public class AzureQueueStorage : QueueStorage
    {
        private readonly JsonSerializerSettings _jsonSettings;
        private bool _createQueueIfNotExists = true;
        private readonly QueueClient _queueClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureQueueStorage"/> class.
        /// </summary>
        /// <param name="queuesStorageConnectionString">Azure Storage connection string.</param>
        /// <param name="queueName">Name of the storage queue where entities will be queued.</param>
        /// <param name="jsonSerializerSettings">If passing in custom JsonSerializerSettings, we 
        /// recommend the following settings:
        /// <para>jsonSerializer.TypeNameHandling = TypeNameHandling.None.</para>
        /// <para>jsonSerializer.NullValueHandling = NullValueHandling.Ignore.</para>
        /// </param>
        public AzureQueueStorage(string queuesStorageConnectionString, string queueName, JsonSerializerSettings jsonSerializerSettings = null)
        {
            if (string.IsNullOrEmpty(queuesStorageConnectionString))
            {
                throw new ArgumentNullException(nameof(queuesStorageConnectionString));
            }

            if (string.IsNullOrEmpty(queueName))
            {
                throw new ArgumentNullException(nameof(queueName));
            }

            _jsonSettings = jsonSerializerSettings ?? new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.None,
                NullValueHandling = NullValueHandling.Ignore
            };

            _queueClient = new QueueClient(queuesStorageConnectionString, queueName);
        }

        /// <summary>
        /// Queue an Activity to an Azure.Storage.Queues.QueueClient. The visibility timeout specifies how long the message should be invisible
        /// to Dequeue and Peek operations. The message content must be a UTF-8 encoded string that is up to 64KB in size.
        /// </summary>
        /// <param name="activity">This is expected to be an <see cref="Activity"/> retrieved from a call to 
        /// activity.GetConversationReference().GetContinuationActivity().  This enables restarting the conversation
        /// using BotAdapter.ContinueConversationAsync.</param>
        /// <param name="visibilityTimeout">Default value of 0. Cannot be larger than 7 days.</param>
        /// <param name="timeToLive">Specifies the time-to-live interval for the message.</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns><see cref="SendReceipt"/> as a Json string, from the QueueClient SendMessageAsync operation.</returns>
        public override async Task<string> QueueActivityAsync(Activity activity, TimeSpan? visibilityTimeout = null, TimeSpan? timeToLive = null, CancellationToken cancellationToken = default)
        {
            if (_createQueueIfNotExists)
            {
                // This is an optimization flag to check if the container creation call has been made.
                // It is okay if this is called more than once.
                _createQueueIfNotExists = false;
                await _queueClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            var message = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(activity, _jsonSettings)));
            var receipt = await _queueClient.SendMessageAsync(message, visibilityTimeout, timeToLive, cancellationToken).ConfigureAwait(false);

            return JsonConvert.SerializeObject(receipt.Value);
        }
    }
}
