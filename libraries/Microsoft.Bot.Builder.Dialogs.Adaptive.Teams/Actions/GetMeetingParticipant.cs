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
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Teams.Actions
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
            RegisterSourceLocation(callerPath, callerLine);
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
        /// The expression to get the value to use for meeting id. Default value is turn.activity.channelData.meeting.id.
        /// </value>
        [JsonProperty("meetingId")]
        public StringExpression MeetingId { get; set; } = "=turn.activity.channelData.meeting.id";

        /// <summary>
        /// Gets or sets the expression to get the value to use for participant id.
        /// </summary>
        /// <value>
        /// The expression to get the value to use for participant id. Default value is turn.activity.from.aadObjectId.
        /// </value>
        [JsonProperty("participantId")]
        public StringExpression ParticipantId { get; set; } = "=turn.activity.from.aadObjectId";

        /// <summary>
        /// Gets or sets the expression to get the value to use for tenant id.
        /// </summary>
        /// <value>
        /// The expression to get the value to use for tenant id. Default value is turn.activity.channelData.meetingInfo.Id.
        /// </value>
        [JsonProperty("tenantId")]
        public StringExpression TenantId { get; set; } = "=turn.activity.channelData.tenant.id";

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

            if (dc.Context.Activity.ChannelId != Channels.Msteams)
            {
                throw new InvalidOperationException($"{Kind} works only on the Teams channel.");
            }

            string meetingId = MeetingId.GetValueOrNull(dc.State);
            string participantId = ParticipantId.GetValueOrNull(dc.State);
            string tenantId = TenantId.GetValueOrNull(dc.State);

            if (participantId == null)
            {
                // TeamsInfo.GetMeetingParticipantAsync will default to retrieving the current meeting's participant
                // if none is provided.  This could lead to unexpected results.  Therefore, GetMeetingParticipant action
                // throws an exception if the expression provided somehow maps to an invalid result.
                throw new InvalidOperationException($"{Kind} could not determine the participant id by expression value provided. {nameof(participantId)} is required.");
            }

            var result = await TeamsInfo.GetMeetingParticipantAsync(dc.Context, meetingId, participantId, tenantId, cancellationToken: cancellationToken).ConfigureAwait(false);

            if (Property != null)
            {
                dc.State.SetValue(Property.GetValue(dc.State), result);
            }

            return await dc.EndDialogAsync(result, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        protected override string OnComputeId()
        {
            return $"{GetType().Name}[{MeetingId?.ToString() ?? string.Empty},{ParticipantId?.ToString() ?? string.Empty},{TenantId?.ToString() ?? string.Empty},{Property?.ToString() ?? string.Empty}]";
        }
    }
}
