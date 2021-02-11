// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions
{
    /// <summary>
    /// Actions triggered when a MessageUpdateActivity is received.
    /// </summary>
    public class OnMessageUpdateActivity : OnActivity
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public new const string Kind = "Microsoft.OnMessageUpdateActivity";

        /// <summary>
        /// Initializes a new instance of the <see cref="OnMessageUpdateActivity"/> class.
        /// </summary>
        /// <param name="actions">Optional, list of <see cref="Dialog"/> actions.</param>
        /// <param name="condition">Optional, condition which needs to be met for the actions to be executed.</param>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        [JsonConstructor]
        public OnMessageUpdateActivity(List<Dialog> actions = null, string condition = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(type: ActivityTypes.MessageUpdate, actions: actions, condition: condition, callerPath: callerPath, callerLine: callerLine)
        {
        }
    }
}
