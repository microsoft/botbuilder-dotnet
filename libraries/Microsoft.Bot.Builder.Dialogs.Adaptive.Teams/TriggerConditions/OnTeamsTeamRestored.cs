// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AdaptiveExpressions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Teams.Conditions
{
    /// <summary>
    /// Actions triggered when a Teams ConversationUpdate with channelData.eventType == 'teamRestored'.
    /// </summary>
    /// <remarks>
    /// turn.activity.channelData.Teams has team data.
    /// </remarks>
    public class OnTeamsTeamRestored : OnConversationUpdateActivity
    {
        [JsonProperty("$kind")]
        public new const string Kind = "Teams.OnTeamRestored";

        [JsonConstructor]
        public OnTeamsTeamRestored(List<Dialog> actions = null, string condition = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(actions: actions, condition: condition, callerPath: callerPath, callerLine: callerLine)
        {
        }

        /// <inheritdoc/>
        protected override Expression CreateExpression()
        {
            // if teams channel and eventType == 'teamRestored'
            return Expression.AndExpression(Expression.Parse($"{TurnPath.Activity}.ChannelId == '{Channels.Msteams}' && {TurnPath.Activity}.channelData.eventType == 'teamRestored'"), base.CreateExpression());
        }
    }
}
