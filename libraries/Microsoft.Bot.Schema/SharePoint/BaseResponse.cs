// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.Bot.Schema.SharePoint
{
    /// <summary>
    /// Base SharePoint Response object.
    /// </summary>
    /// <typeparam name="T">Type for data field.</typeparam>
    public class BaseResponse<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseResponse{T}"/> class.
        /// </summary>
        /// <param name="schemaVersion">Schema version to be used.</param>
        public BaseResponse(string schemaVersion)
        {
            SchemaVersion = schemaVersion;
        }

        /// <summary>
        /// Gets or sets the Schema version of the response.
        /// </summary>
        /// <value>
        /// The Schema version of the response.
        /// </value>
        public string SchemaVersion { get; set; }

        /// <summary>
        /// Gets or Sets open-ended response Data.
        /// </summary>
        /// <value>This value is the data of the response.</value>
        [JsonProperty(PropertyName = "data")]
        public T Data { get; set; }
    }
}
