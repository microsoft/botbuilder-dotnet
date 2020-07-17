// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using AdaptiveExpressions;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
#pragma warning disable CA1716 // Identifiers should not match keywords (by design and we can't change this without breaking binary compat)
    public class Case : ActionScope
#pragma warning restore CA1716 // Identifiers should not match keywords
    {
        public Case(string value = null, IEnumerable<Dialog> actions = null)
            : base(actions)
        {
            this.Value = value;
        }

        /// <summary>
        /// Gets or sets constant to be compared against condition.
        /// </summary>
        /// <value>
        /// Constant be compared against condition.
        /// </value>
        [JsonProperty("value")]
        public string Value { get; set; }
    }
}
