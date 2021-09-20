﻿// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    /// <summary>
    /// Calls BotFrameworkAdapter.GetActivityMembers() and sets the result to a memory property.
    /// </summary>
    public class GetActivityMembers : Dialog
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.GetActivityMembers";

        /// <summary>
        /// Initializes a new instance of the <see cref="GetActivityMembers"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        [JsonConstructor]
        public GetActivityMembers([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base()
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
        /// Gets or sets property path to put the value in.
        /// </summary>
        /// <value>
        /// Property path to put the value in.
        /// </value>
        [JsonProperty("property")]
        public StringExpression Property { get; set; }

        /// <summary>
        /// Gets or sets the expression to get the value to put into property path.
        /// </summary>
        /// <value>
        /// The expression to get the value to put into property path. If this is missing, then the current turn Activity.id will be used.
        /// </value>
        [JsonProperty("activityId")]
        public StringExpression ActivityId { get; set; }

        /// <summary>
        /// Called when the dialog is started and pushed onto the dialog stack.
        /// </summary>
        /// <param name="dc">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <param name="options">Optional, initial information to pass to the dialog.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
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

            string id = dc.Context.Activity.Id;
            if (this.ActivityId != null)
            {
                var (value, valueError) = this.ActivityId.TryGetValue(dc.State);
                if (valueError != null)
                {
                    throw new InvalidOperationException($"Expression evaluation resulted in an error. Expression: \"{this.ActivityId}\". Error: {valueError}");
                }

                id = value as string;
            }

            var result = await GetActivityMembersAsync(dc.Context, id, cancellationToken).ConfigureAwait(false);

            dc.State.SetValue(this.Property.GetValue(dc.State), result);

            return await dc.EndDialogAsync(result, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        protected override string OnComputeId()
        {
            return $"{GetType().Name}[{this.ActivityId?.ToString() ?? string.Empty},{this.Property?.ToString() ?? string.Empty}]";
        }

        private async Task<IList<ChannelAccount>> GetActivityMembersAsync(ITurnContext turnContext, string activityId, CancellationToken cancellationToken)
        {
            // If no activity was passed in, use the current activity.
            if (activityId == null)
            {
                activityId = turnContext.Activity.Id;
            }

            if (turnContext.Activity.Conversation == null)
            {
                throw new ArgumentException($"{nameof(GetActivityMembers)}.{nameof(GetActivityMembersAsync)}(): missing conversation");
            }

            if (string.IsNullOrWhiteSpace(turnContext.Activity.Conversation.Id))
            {
                throw new ArgumentException($"{nameof(GetActivityMembers)}.{nameof(GetActivityMembersAsync)}(): missing conversation.id");
            }

            var connectorClient = turnContext.TurnState.Get<IConnectorClient>();
            var conversationId = turnContext.Activity.Conversation.Id;

            var accounts = await connectorClient.Conversations.GetActivityMembersAsync(conversationId, activityId, cancellationToken).ConfigureAwait(false);

            return accounts;
        }
    }
}
