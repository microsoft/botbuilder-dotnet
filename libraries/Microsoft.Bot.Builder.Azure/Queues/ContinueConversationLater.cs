// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Azure.Storage.Queues;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Azure.Queues
{
    /// <summary>
    /// Action which schedules a conversation to be continued later by writing a EventActivity(Name=ContinueConversation) to a Azure Storage queue.
    /// </summary>
    /// <remarks>
    /// This class works by writing an EventActivity(Name=ConversationUpdate) to an azure storage queue with visibility policy to 
    /// make it visible at a future point in time. 
    /// 
    /// The queue needs a process (such as a webjob/azure function) monitoring incoming activities and processing them by either:
    ///   - posting the activity back to the bot itself via BotFrameworkHttpClient.PostActivityAsync(botId, botEndpoint, activity).
    /// OR
    ///   - processing the activity directly via adapter.ProcessActivity(activity, ...); 
    ///     NOTE: adapter.ProcessActivity() understands that EventActivity(Name=ConversationUpdate) should be processed as ContinueConversation() pipeline.
    /// 
    /// This dialog returns the receipt information for the queued activity as the result of the dialog.
    /// </remarks>
    public class ContinueConversationLater : Dialog
    {
        /// <summary>
        /// The Kind name for this dialog.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "AzureQueues.ContinueConversationLater";

        private static JsonSerializerSettings jsonSettings = new JsonSerializerSettings() { Formatting = Formatting.Indented, NullValueHandling = NullValueHandling.Ignore };

        /// <summary>
        /// Initializes a new instance of the <see cref="ContinueConversationLater"/> class.
        /// </summary>
        /// <param name="callerPath">The full path of the source file that contains this called.</param>
        /// <param name="callerLine">The line within the source file that contains this caller.</param>
        [JsonConstructor]
        public ContinueConversationLater([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// Gets or sets an optional expression which if is true will disable this action.
        /// </summary>
        /// <example>
        /// "user.age > 18".
        /// </example>
        /// <value>
        /// A boolean expression. 
        /// </value>
        [JsonProperty("disabled")]
        public BoolExpression Disabled { get; set; }

        /// <summary>
        /// Gets or sets the expression which resolves to the date/time to continue the conversation.
        /// </summary>
        /// <value>date/time string in ISO 8601 format to continue conversation.</value>
        /// <example>=addHour(utcNow(), 1).</example>
        [JsonProperty("date")]
        public StringExpression Date { get; set; }

        /// <summary>
        /// Gets or sets the value to pass in the EventActivity.Value payload. 
        /// </summary>
        /// <value>
        /// The value to use for the EventActivity.Value payload.
        /// </value>
        [JsonProperty("value")]
        public ValueExpression Value { get; set; }

        /// <summary>
        /// Gets or sets the connectionString for the azure storage queue to use.
        /// </summary>
        /// <value>
        /// The connectionString for the azure storage queue to use.
        /// </value>
        /// <example>'=settings.ConnectionString'.</example>
        [JsonProperty("connectionString")]
        public StringExpression ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the name of the queue to use. 
        /// </summary>
        /// <value>default is 'activities'.</value>
        [JsonProperty("queueName")]
        public StringExpression QueueName { get; set; } = "activities";

        /// <inheritdoc/>
        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }

            if (this.Disabled != null && this.Disabled.GetValue(dc.State) == true)
            {
                return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            var dateString = Date.GetValue(dc.State);
            DateTime date;
            if (!DateTime.TryParse(dateString, out date))
            {
                throw new ArgumentException($"{nameof(Date)} is invalid");
            }
            
            date = date.ToUniversalTime();
            if (date <= DateTime.UtcNow)
            {
                throw new ArgumentOutOfRangeException($"{nameof(Date)} must be in the future");
            }

            var visibility = date - DateTime.UtcNow;
            var ttl = visibility + TimeSpan.FromMinutes(2);

            var queueName = QueueName.GetValue(dc.State);
            var connectionString = ConnectionString.GetValue(dc.State);
            var value = Value.GetValue(dc.State);

            // create ContinuationActivity from the conversation reference.
            var activity = dc.Context.Activity.GetConversationReference().GetContinuationActivity();
            activity.Value = value;

            var message = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(activity, jsonSettings)));

            QueueClient queueClient = new QueueClient(connectionString, queueName);
            
            await queueClient.CreateIfNotExistsAsync().ConfigureAwait(false);

            // send ResumeConversation event, it will get posted back to us, giving us ability to process it and do the right thing.
            var reciept = await queueClient.SendMessageAsync(message, visibility, ttl, cancellationToken).ConfigureAwait(false);
            
            // return the receipt as the result.
            return await dc.EndDialogAsync(result: reciept.Value, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        protected override string OnComputeId()
        {
            return $"{this.GetType().Name}({Date?.ToString()}s)";
        }
    }
}
