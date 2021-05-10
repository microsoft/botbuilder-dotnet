// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    /// <summary>
    /// Action which continues a conversation using a Conversation reference.
    /// </summary>
    /// <remarks>
    /// This action works by writing an EventActivity(Name=ContinueConversation) to a StorageQueue stamped with the 
    /// routing information from the provided ConversationReference. 
    /// 
    /// The queue needs a process (such as a webjob/azure function) pulling activites from the StorageQueue and processing them by 
    /// calling adapter.ProcessActivity(activity, ...); 
    /// 
    /// NOTE: In the case of multiple adapters this webjob/function should inspect the activity.channelId 
    /// to properly route the activity to the appropriate adapter. 
    /// 
    /// This dialog returns the receipt information for the queued activity as the result of the dialog.
    /// <seealso cref="ContinueConversationLater"/>
    /// </remarks>
    public class ContinueConversation : Dialog
    {
        /// <summary>
        /// The Kind name for this dialog.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.ContinueConversation";

        /// <summary>
        /// Initializes a new instance of the <see cref="ContinueConversation"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        [JsonConstructor]
        public ContinueConversation([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// Gets or sets an optional expression which if true will disable this action.
        /// </summary>
        /// <value>
        /// A boolean expression. 
        /// </value>
        [JsonProperty("disabled")]
        public BoolExpression Disabled { get; set; }

        /// <summary>
        /// Gets or sets the conversationReference for the target conversation.
        /// </summary>
        /// <value>
        /// The conversation reference data structure which is needed to switch to a conversation.
        /// </value>
        [JsonProperty("conversationReference")]
        public ObjectExpression<ConversationReference> ConversationReference { get; set; }

        /// <summary>
        /// Gets or sets an optional value to use for EventActivity.Value.
        /// </summary>
        /// <value>
        /// The value to use for the EventActivity.Value payload.
        /// </value>
        [JsonProperty("value")]
        public ValueExpression Value { get; set; }

        /// <inheritdoc/>
        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object opts = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (opts != null)
            {
                throw new NotSupportedException($"{nameof(opts)} is not supported by this action.");
            }

            if (Disabled != null && Disabled.GetValue(dc.State))
            {
                return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            var conversationReference = this.ConversationReference.GetValue(dc.State);
            var continuationActivity = conversationReference.GetContinuationActivity();
            continuationActivity.Value = Value.GetValue(dc.State);

            var queueStorage = dc.Context.TurnState.Get<QueueStorage>() ?? throw new NullReferenceException("Unable to locate QueueStorage in HostContext");
            var receipt = await queueStorage.QueueActivityAsync(continuationActivity, cancellationToken: cancellationToken).ConfigureAwait(false);

            // return the receipt as the result.
            return await dc.EndDialogAsync(result: receipt, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
    }
}
