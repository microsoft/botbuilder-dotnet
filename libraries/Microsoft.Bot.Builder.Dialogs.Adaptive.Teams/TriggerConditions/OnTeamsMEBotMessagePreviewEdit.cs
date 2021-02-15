// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AdaptiveExpressions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Teams.Conditions
{
    /// <summary>
    /// Actions triggered when a Teams InvokeActivity is received with activity.name='composeExtension/submitAction'
    /// and activity.value.botMessagePreviewAction == 'edit'.
    /// </summary>
    public class OnTeamsMEBotMessagePreviewEdit : OnInvokeActivity
    {
        [JsonProperty("$kind")]
        public new const string Kind = "Teams.OnMEBotMessagePreviewEdit";

        [JsonConstructor]
        public OnTeamsMEBotMessagePreviewEdit(List<Dialog> actions = null, string condition = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(actions: actions, condition: condition, callerPath: callerPath, callerLine: callerLine)
        {
        }

        /// <inheritdoc/>
        protected override Expression CreateExpression()
        {
            // if name is 'composeExtension/submitAction'
            return Expression.AndExpression(Expression.Parse($"{TurnPath.Activity}.ChannelId == '{Channels.Msteams}' && {TurnPath.Activity}.name == 'composeExtension/submitAction' && {TurnPath.ACTIVITY}.value.botMessagePreviewAction == 'edit'"), base.CreateExpression());
        }
    }
}
