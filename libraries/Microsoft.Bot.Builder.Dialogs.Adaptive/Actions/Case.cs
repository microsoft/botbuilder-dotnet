// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using Microsoft.Bot.Expressions;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    public class Case : ActionScope
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
