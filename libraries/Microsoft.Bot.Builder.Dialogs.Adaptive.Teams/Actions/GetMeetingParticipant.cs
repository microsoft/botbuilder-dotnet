// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    /// <summary>
    /// Calls TeamsInfo.GetMeetingParticipantAsync and sets the result to a memory property.
    /// </summary>
    public class GetMeetingParticipant : Dialog
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Teams.GetMeetingParticipant";

        /// <summary>
        /// Initializes a new instance of the <see cref="GetMeetingParticipant"/> class.
        /// </summary>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        [JsonConstructor]
        public GetMeetingParticipant([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
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
        /// Gets or sets the expression to get the value to use for meeting id.
        /// </summary>
        /// <value>
        /// The expression to get the value to use for meeting id. If this is missing, then the current turn Activity.TeamsChannelData.Meeting.Id will be used.
        /// </value>
        [JsonProperty("meetingId")]
        public StringExpression MeetingId { get; set; }

        /// <summary>
        /// Gets or sets the expression to get the value to use for participant id.
        /// </summary>
        /// <value>
        /// The expression to get the value to use for participant id. If this is missing, then the current turn Activity.From.AadObjectId will be used.
        /// </value>
        [JsonProperty("participantId")]
        public StringExpression ParticipantId { get; set; }

        /// <summary>
        /// Gets or sets the expression to get the value to use for tenant id.
        /// </summary>
        /// <value>
        /// The expression to get the value to use for tenant id. If this is missing, then the current turn Activity.TeamsChannelData.Tenant.Id will be used.
        /// </value>
        [JsonProperty("tenantId")]
        public StringExpression TenantId { get; set; }

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
            
            if (this.Disabled != null && this.Disabled.GetValue(dc.State) == true)
            {
                return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            if (dc.Context.Activity.ChannelId != Channels.Msteams)
            {
                throw new Exception("TeamsInfo.GetMeetingParticipantAsync() works only on the Teams channel.");
            }

            string meetingId = GetValueOrNull(dc, this.MeetingId) ?? dc.Context.Activity.TeamsGetMeetingInfo()?.Id;
            string participantId = GetValueOrNull(dc, this.ParticipantId) ?? dc.Context.Activity.From.AadObjectId;
            string tenantId = GetValueOrNull(dc, this.TenantId) ?? dc.Context.Activity.GetChannelData<TeamsChannelData>()?.Tenant?.Id;

            var result = await TeamsInfo.GetMeetingParticipantAsync(dc.Context, meetingId, participantId, tenantId, cancellationToken: cancellationToken).ConfigureAwait(false);

            dc.State.SetValue(this.Property.GetValue(dc.State), result);

            return await dc.EndDialogAsync(result, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Builds the compute Id for the dialog.
        /// </summary>
        /// <returns>A string representing the compute Id.</returns>
        protected override string OnComputeId()
        {
            return $"{this.GetType().Name}[{this.MeetingId?.ToString() ?? string.Empty},{this.ParticipantId?.ToString() ?? string.Empty},{this.TenantId?.ToString() ?? string.Empty},{this.Property?.ToString() ?? string.Empty}]";
        }

        private string GetValueOrNull(DialogContext dc, StringExpression stringExpression)
        {
            if (stringExpression != null)
            {
                var (value, valueError) = stringExpression.TryGetValue(dc.State);
                if (valueError != null)
                {
                    throw new Exception($"Expression evaluation resulted in an error. Expression: {stringExpression.ExpressionText}. Error: {valueError}");
                }

                return value as string;
            }

            return null;
        }
    }
}
