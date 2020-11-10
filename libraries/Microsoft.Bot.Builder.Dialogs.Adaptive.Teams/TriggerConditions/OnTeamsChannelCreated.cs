// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AdaptiveExpressions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Teams
{
    /// <summary>
    /// Actions triggered when a Teams ConversationUpdateActivity with channelData.eventType == 'channelCreated'.
    /// </summary>
    /// <remarks>
    /// turn.activity.channelData.Teams has team data.
    /// </remarks>
    public class OnTeamsChannelCreated : OnConversationUpdateActivity
    {
        [JsonProperty("$kind")]
        public new const string Kind = "Teams.OnChannelCreated";

        [JsonConstructor]
        public OnTeamsChannelCreated(List<Dialog> actions = null, string condition = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(actions: actions, condition: condition, callerPath: callerPath, callerLine: callerLine)
        {
        }

        /// <inheritdoc/>
        protected override Expression CreateExpression()
        {
            // if teams channel and eventType == 'channelCreated'
            return Expression.AndExpression(Expression.Parse($"{TurnPath.Activity}.ChannelId == '{Channels.Msteams}' && {TurnPath.Activity}.channelData.eventType == 'channelCreated'"), base.CreateExpression());
        }
    }
}
