// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AdaptiveExpressions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Teams
{
    /// <summary>
    /// Actions triggered when a Teams ConversationUpdateActivity with channelData.eventType == 'channelRestored'.
    /// </summary>
    /// <remarks>
    /// turn.activity.channelData.Teams has team data.
    /// </remarks>
    public class OnTeamsChannelRestored : OnConversationUpdateActivity
    {
        [JsonProperty("$kind")]
        public new const string Kind = "Teams.OnChannelRestored";

        [JsonConstructor]
        public OnTeamsChannelRestored(List<Dialog> actions = null, string condition = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(actions: actions, condition: condition, callerPath: callerPath, callerLine: callerLine)
        {
        }

        /// <inheritdoc/>
        protected override Expression CreateExpression()
        {
            // if teams channel and eventType == 'channelRestored'
            return Expression.AndExpression(Expression.Parse($"{TurnPath.Activity}.ChannelId == '{Channels.Msteams}' && {TurnPath.Activity}.channelData.eventType == 'channelRestored'"), base.CreateExpression());
        }
    }
}
