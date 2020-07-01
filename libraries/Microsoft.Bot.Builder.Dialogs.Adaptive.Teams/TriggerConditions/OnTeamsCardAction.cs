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
    /// Actions triggered when a Teams InvokeActivity is received with no activity.name.
    /// </summary>
    public class OnTeamsCardAction : OnInvokeActivity
    {
        [JsonProperty("$kind")]
        public new const string Kind = "Teams.OnCardAction";

        [JsonConstructor]
        public OnTeamsCardAction(List<Dialog> actions = null, string condition = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(actions: actions, condition: condition, callerPath: callerPath, callerLine: callerLine)
        {
        }

        public override Expression GetExpression()
        {
            // if name is null
            return Expression.AndExpression(Expression.Parse($"{TurnPath.Activity}.ChannelId == '{Channels.Msteams}' && {TurnPath.Activity}.name == null"), base.GetExpression());
        }
    }
}
