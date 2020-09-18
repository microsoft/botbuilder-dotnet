// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using AdaptiveExpressions;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    /// <summary>
    /// Cases of action scope.
    /// </summary>
#pragma warning disable CA1716 // Identifiers should not match keywords (by design and we can't change this without breaking binary compat)
    public class Case : ActionScope
#pragma warning restore CA1716 // Identifiers should not match keywords
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Case"/> class.
        /// </summary>
        /// <param name="value">Optional, case's string value.</param>
        /// <param name="actions">Optional, numerable list of dialog actions.</param>
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
