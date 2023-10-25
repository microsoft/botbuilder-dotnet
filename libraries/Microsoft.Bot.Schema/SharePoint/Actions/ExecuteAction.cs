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
    public class ExecuteAction : BaseAction, IAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExecuteAction"/> class.
        /// </summary>
        public ExecuteAction() 
            : base("Execute")
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
