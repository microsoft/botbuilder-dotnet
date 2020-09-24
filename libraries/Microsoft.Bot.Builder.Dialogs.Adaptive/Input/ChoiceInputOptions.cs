// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Input
{
    /// <summary>
    /// Holds choice input option list.
    /// </summary>
    public class ChoiceInputOptions : InputDialogOptions
    {
        /// <summary>
        /// Gets or sets input option list.
        /// </summary>
        /// <value>
        /// Input option list.
        /// </value>
        [JsonProperty("choices")]
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking binary compat)
        public List<Choice> Choices { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only
    }
}
