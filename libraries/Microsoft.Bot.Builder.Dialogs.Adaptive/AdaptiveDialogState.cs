// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    /// <summary>
    /// Defines adaptive dialog <see cref="ActionState"/> list.
    /// </summary>
    public class AdaptiveDialogState
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AdaptiveDialogState"/> class.
        /// </summary>
        public AdaptiveDialogState()
        {
        }

        /// <summary>
        /// Gets or sets <see cref="ActionState"/> list.
        /// </summary>
        /// <value>
        /// <see cref="ActionState"/> list.
        /// </value>
        [JsonProperty(PropertyName = "actions")]
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking binary compat)
        public List<ActionState> Actions { get; set; } = new List<ActionState>();
#pragma warning restore CA2227 // Collection properties should be read only
    }
}
