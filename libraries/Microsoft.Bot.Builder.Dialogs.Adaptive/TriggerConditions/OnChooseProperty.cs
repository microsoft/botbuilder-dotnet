// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AdaptiveExpressions;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions
{
    /// <summary>
    /// Triggered to choose which property an entity goes to.
    /// </summary>
    public class OnChooseProperty : OnDialogEvent
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public new const string Kind = "Microsoft.OnChooseProperty";

        /// <summary>
        /// Initializes a new instance of the <see cref="OnChooseProperty"/> class.
        /// </summary>
        /// <param name="actions">Optional, actions to add to the plan when the rule constraints are met.</param>
        /// <param name="condition">Optional, condition which needs to be met for the actions to be executed.</param>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        [JsonConstructor]
        public OnChooseProperty(List<Dialog> actions = null, string condition = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(
                @event: AdaptiveEvents.ChooseProperty,
                actions: actions,
                condition: condition,
                callerPath: callerPath,
                callerLine: callerLine)
        {
        }

        /// <summary>
        /// Gets the identity for this rule's action.
        /// </summary>
        /// <returns>String with the identity.</returns>
        public override string GetIdentity()
            => $"{GetType().Name}()";
    }
}
