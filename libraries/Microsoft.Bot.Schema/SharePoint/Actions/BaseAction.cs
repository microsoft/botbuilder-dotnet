// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.Bot.Schema.SharePoint
{
    /// <summary>
    /// Base Action.
    /// </summary>
    public class BaseAction
    {
        [JsonProperty(PropertyName = "type")]
        private readonly string type;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseAction"/> class.
        /// </summary>
        /// <param name="actionType">Type of the action.</param>
        protected BaseAction(string actionType)
        {
            this.type = actionType;
        }
    }
}
