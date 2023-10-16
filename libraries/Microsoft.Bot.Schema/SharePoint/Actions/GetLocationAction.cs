// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Schema.SharePoint
{
    /// <summary>
    /// SharePoint get location action.
    /// </summary>
    public class GetLocationAction : IAction, IOnCardSelectionAction
    {
#pragma warning disable CA1823 // Avoid unused private fields
#pragma warning disable CS0414 // The field 'GetLocationAction.type' is assigned but its value is never used
        [JsonProperty(PropertyName = "type")]
        private string type = "VivaAction.GetLocation";
#pragma warning restore CS0414 // The field 'GetLocationAction.type' is assigned but its value is never used
#pragma warning restore CA1823 // Avoid unused private fields

        /// <summary>
        /// Initializes a new instance of the <see cref="GetLocationAction"/> class.
        /// </summary>
        public GetLocationAction()
        {
            // Do nothing
        }
        
        /// <summary>
        /// Gets or Sets the action parameters of type <see cref="GetLocationActionParameters"/>.
        /// </summary>
        /// <value>This value is the parameters of the action.</value>
        [JsonProperty(PropertyName = "parameters")]
        public GetLocationActionParameters Parameters { get; set; }
    }
}
