// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.Bot.Schema.SharePoint
{
    /// <summary>
    /// ACE invoke request payload.
    /// </summary>
    public class AceRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AceRequest"/> class.
        /// </summary>
        public AceRequest()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AceRequest"/> class.
        /// </summary>
        /// <param name="data">ACE request data.</param>
        /// <param name="customProperties">ACE properties data.</param>
        public AceRequest(object data = default, object customProperties = default)
        { 
            Data = data;
            CustomProperties = customProperties;
        }

        /// <summary>
        /// Gets or sets user ACE request data.
        /// </summary>
        /// <value>The ACE request data.</value>
        [JsonProperty(PropertyName = "data")]
        public object Data { get; set; }

        /// <summary>
        /// Gets or sets ACE properties data. Free payload with key-value pairs.
        /// </summary>
        /// <value>ACE Properties object.</value>
        [JsonProperty(PropertyName = "customProperties")]
        public object CustomProperties { get; set; }
    }
}
