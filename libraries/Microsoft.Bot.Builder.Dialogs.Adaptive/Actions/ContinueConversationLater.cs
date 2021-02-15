// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    /// <summary>
    /// Action which schedules the current conversation to be continued at a later time..
    /// </summary>
    /// <remarks>
    /// This action works by writing an EventActivity(Name=ContinueConversation) to a StorageQueue with same routing information 
    /// as the current conversation reference, and with a visibility policy to make it visible at a future point in time. 
    /// 
    /// The queue needs a process (such as a webjob/azure function) pulling activites from the StorageQueue and processing them by 
    /// calling adapter.ProcessActivity(activity, ...); 
    /// 
    /// NOTE: In the case of multiple adapters this webjob/function should inspect the activity.channelId 
    /// to properly route the activity to the appropriate adapter. 
    /// 
    /// This dialog returns the receipt information for the queued activity as the result of the dialog.
    /// <seealso cref="ContinueConversation"/>
    /// </remarks>
    public class ContinueConversationLater : Dialog
    {
        /// <summary>
        /// The Kind name for this dialog.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.ContinueConversationLater";

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
        /// Gets or sets an optional value to use for EventActivity.Value.
        /// </summary>
        /// <value>
        /// The value to use for the EventActivity.Value payload.
        /// </value>
        [JsonProperty("value")]
        public ValueExpression Value { get; set; }

        /// <inheritdoc/>
        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }

            if (Disabled != null && Disabled.GetValue(dc.State))
            {
                return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            var dateString = Date.GetValue(dc.State);
            if (!DateTime.TryParse(dateString, out var date))
            {
                throw new ArgumentException($"{nameof(Date)} is invalid");
            }

            date = date.ToUniversalTime();
            if (date <= DateTime.UtcNow)
            {
                throw new ArgumentOutOfRangeException($"{nameof(Date)} must be in the future");
            }

            // create ContinuationActivity from the conversation reference.
            var activity = dc.Context.Activity.GetConversationReference().GetContinuationActivity();
            activity.Value = Value.GetValue(dc.State);

            var visibility = date - DateTime.UtcNow;
            var ttl = visibility + TimeSpan.FromMinutes(2);

            var queueStorage = dc.Context.TurnState.Get<QueueStorage>() ?? throw new NullReferenceException("Unable to locate QueueStorage in HostContext");
            var receipt = await queueStorage.QueueActivityAsync(activity, visibility, ttl, cancellationToken).ConfigureAwait(false);

            // return the receipt as the result.
            return await dc.EndDialogAsync(result: receipt, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        protected override string OnComputeId()
        {
            return $"{GetType().Name}({Date?.ToString()}s)";
        }
    }
}
