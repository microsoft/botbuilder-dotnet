// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AdaptiveExpressions;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions
{
    /// <summary>
    /// Action triggered when a conversation continues.
    /// </summary>
    public class OnContinueConversation : OnEventActivity
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public new const string Kind = "Microsoft.OnContinueConversation";

        /// <summary>
        /// Initializes a new instance of the <see cref="OnContinueConversation"/> class.
        /// </summary>
        /// <param name="actions">Optional, actions to add to the plan when the rule constraints are met.</param>
        /// <param name="condition">Optional, condition which needs to be met for the actions to be executed.</param>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        [JsonConstructor]
        public OnContinueConversation(List<Dialog> actions = null, string condition = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(actions, condition, callerPath, callerLine)
        {
        }

        /// <inheritdoc/>
        protected override Expression CreateExpression()
        {
            // add constraints for activity type
            return Expression.AndExpression(Expression.Parse($"{TurnPath.Activity}.name == 'ContinueConversation'"), base.CreateExpression());
        }
    }
}
