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
    /// Actions triggered when a Teams ConversationUpdate with channelData.eventType == 'teamRenamed'.
    /// </summary>
    /// <remarks>
    /// turn.activity.channelData.Teams has team data.
    /// </remarks>
    public class OnTeamsTeamRenamed : OnConversationUpdateActivity
    {
        [JsonProperty("$kind")]
        public new const string Kind = "Teams.OnTeamRenamed";

        [JsonConstructor]
        public OnTeamsTeamRenamed(List<Dialog> actions = null, string condition = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(actions: actions, condition: condition, callerPath: callerPath, callerLine: callerLine)
        {
        }

        public override Expression GetExpression()
        {
            // if teams channel and eventType == 'teamRenamed'
            return Expression.AndExpression(Expression.Parse($"{TurnPath.Activity}.ChannelId == '{Channels.Msteams}' && {TurnPath.Activity}.channelData.eventType == 'teamRenamed'"), base.GetExpression());
        }
    }
}
