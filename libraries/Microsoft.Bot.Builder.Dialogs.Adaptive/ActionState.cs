// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Diagnostics;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    /// <summary>
    /// Contains state information about the action.
    /// </summary>
    [DebuggerDisplay("{DialogId}")]
    public class ActionState : DialogState
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActionState"/> class.
        /// </summary>
        public ActionState()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionState"/> class.
        /// </summary>
        /// <param name="dialogId">Optional, dialog identifier value.</param>
        /// <param name="options">Optional, dialog options.</param>
        public ActionState(string dialogId = null, object options = null)
        {
            DialogId = dialogId;
            Options = options;
        }

        /// <summary>
        /// Gets or sets DialogId value.
        /// </summary>
        /// <value>
        /// DialogId value.
        /// </value>
        [JsonProperty(PropertyName = "dialogId")]
        public string DialogId { get; set; }

        /// <summary>
        /// Gets or sets options value.
        /// </summary>
        /// <value>
        /// Options value.
        /// </value>
        [JsonProperty(PropertyName = "options")]
        public object Options { get; set; }
    }
}
