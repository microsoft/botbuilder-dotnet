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
    /// Actions triggered when a Teams InvokeActivity is received with activity.name='task/fetch'.
    /// </summary>
    public class OnTeamsTaskModuleFetch : OnInvokeActivity
    {
        [JsonProperty("$kind")]
        public new const string Kind = "Teams.OnTaskModuleFetch";

        [JsonConstructor]
        public OnTeamsTaskModuleFetch(List<Dialog> actions = null, string condition = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(actions: actions, condition: condition, callerPath: callerPath, callerLine: callerLine)
        {
        }

        /// <inheritdoc/>
        protected override Expression CreateExpression()
        {
            // if name is 'task/fetch'
            return Expression.AndExpression(Expression.Parse($"{TurnPath.Activity}.ChannelId == '{Channels.Msteams}' && {TurnPath.Activity}.name == 'task/fetch'"), base.CreateExpression());
        }
    }
}
