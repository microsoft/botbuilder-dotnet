// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AdaptiveExpressions;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions
{
    public class OnContinueConversation : OnEventActivity
    {
        [JsonProperty("$kind")]
        public new const string Kind = "Microsoft.OnContinueConversation";

        [JsonConstructor]
        public OnContinueConversation(List<Dialog> actions = null, string condition = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(actions, condition, callerPath, callerLine)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
        }

        public override Expression GetExpression()
        {
            // add constraints for activity type
            return Expression.AndExpression(Expression.Parse($"{TurnPath.Activity}.name == 'ContinueConversation'"), base.GetExpression());
        }
    }
}
