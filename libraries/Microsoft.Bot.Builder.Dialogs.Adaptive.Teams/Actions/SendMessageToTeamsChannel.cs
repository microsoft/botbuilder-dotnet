// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Teams.Actions;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Teams.Actions
{
    /// <summary>
    /// Calls TeamsInfo.SendMessageToTeamsChannel and sets the result to a memory property.
    /// </summary>
    public class SendMessageToTeamsChannel : Dialog
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Teams.SendMessageToTeamsChannel";

        /// <summary>
        /// Initializes a new instance of the <see cref="SendMessageToTeamsChannel"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        [JsonConstructor]
        public SendMessageToTeamsChannel([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
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
        /// Gets or sets property path to put the newly created activity's Conversation Reference.
        /// This can be used to later send messages to this same conversation.
        /// </summary>
        /// <value>
        /// Property path to put the newly created activity's conversation reference.
        /// </value>
        [JsonProperty("conversationReferenceProperty")]
        public StringExpression ConversationReferenceProperty { get; set; }

        /// <summary>
        /// Gets or sets property path to put the id of the activity sent.
        /// </summary>
        /// <value>
        /// Property path to put the id of the activity sent.
        /// </value>
        [JsonProperty("activityIdProperty")]
        public StringExpression ActivityIdProperty { get; set; }

        /// <summary>
        /// Gets or sets the expression to get the value to use for the teams channel id where the message should be sent. Default is turn.activity.channelData.channel.id.
        /// </summary>
        /// <value>
        /// The expression to get the value to use for teams channel id.
        /// </value>
        [JsonProperty("teamsChannelId")]
        public StringExpression TeamsChannelId { get; set; } = "=turn.activity.channelData.channel.id";

        /// <summary>
        /// Gets or sets template for the activity expression containing the activity to send.
        /// </summary>
        /// <value>
        /// Template for the activity.
        /// </value>
        [JsonProperty("activity")]
        public ITemplate<Activity> Activity { get; set; }

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
            
            if (this.Disabled != null && this.Disabled.GetValue(dc.State))
            {
                return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            if (dc.Context.Activity.ChannelId != Channels.Msteams)
            {
                throw new InvalidOperationException($"{Kind} works only on the Teams channel.");
            }

            IActivity activity = null;
            if (Activity != null)
            {
                activity = await Activity.BindAsync(dc, dc.State).ConfigureAwait(false);
            }

            string teamsChannelId = TeamsChannelId.GetValueOrNull(dc.State);
            if (string.IsNullOrEmpty(teamsChannelId))
            {
                teamsChannelId = dc.Context.Activity.TeamsGetChannelId();
            }

            if (!(dc.Context.Adapter is BotFrameworkAdapter))
            {
                throw new InvalidOperationException($"{Kind} is not supported by the current adapter.");
            }

            // TODO: this will NOT work with certificate app credentials

            // TeamsInfo.SendMessageToTeamsChannelAsync requires AppCredentials
            var credentials = dc.Context.TurnState.Get<IConnectorClient>()?.Credentials as MicrosoftAppCredentials;
            if (credentials == null)
            {
                throw new InvalidOperationException($"Missing credentials as {nameof(MicrosoftAppCredentials)} in {nameof(IConnectorClient)} from TurnState");
            }

            // The result comes back as a tuple, which is used to set the two properties (if present).
            var result = await TeamsInfo.SendMessageToTeamsChannelAsync(dc.Context, activity, teamsChannelId, credentials, cancellationToken: cancellationToken).ConfigureAwait(false);

            if (this.ConversationReferenceProperty != null)
            {
                dc.State.SetValue(this.ConversationReferenceProperty.GetValue(dc.State), result.Item1);
            }
            
            if (this.ActivityIdProperty != null)
            {
                dc.State.SetValue(this.ActivityIdProperty.GetValue(dc.State), result.Item2);
            }

            return await dc.EndDialogAsync(result, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Builds the compute Id for the dialog.
        /// </summary>
        /// <returns>A string representing the compute Id.</returns>
        protected override string OnComputeId()
        {
            return $"{this.GetType().Name}[{this.TeamsChannelId?.ToString() ?? string.Empty},{this.ActivityIdProperty?.ToString() ?? string.Empty},{this.ConversationReferenceProperty?.ToString() ?? string.Empty}]";
        }
    }
}
