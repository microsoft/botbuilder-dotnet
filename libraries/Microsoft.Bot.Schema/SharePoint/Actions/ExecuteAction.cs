// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.Bot.Schema.SharePoint
{
    /// <summary>
    /// Action.Execute.
    /// </summary>
    public class ExecuteAction : IAction
    {
#pragma warning disable CA1823 // Avoid unused private fields
#pragma warning disable CS0414 // The field 'ExecuteAction.type' is assigned but its value is never used
        [JsonProperty(PropertyName = "type")]
        private string type = "Execute";
#pragma warning restore CS0414 // The field 'ExecuteAction.type' is assigned but its value is never used
#pragma warning restore CA1823 // Avoid unused private fields

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecuteAction"/> class.
        /// </summary>
        public ExecuteAction()
        {
            // Do nothing
        }

        /// <summary>
        /// Gets or Sets the action parameters of type <see cref="Dictionary{TKey, TValue}"/>.
        /// </summary>
        /// <value>This value is the parameters of the action.</value>
        [JsonProperty(PropertyName = "parameters")]
        #pragma warning disable CA2227
        public Dictionary<string, object> Parameters { get; set; }

        /// <summary>
        /// Gets or Sets the verb associated with this action of type <see cref="string"/>.
        /// </summary>
        /// <value>This value is the verb associated with the action.</value>
        [JsonProperty(PropertyName = "verb")]
        public string Verb { get; set; }
    }
}
