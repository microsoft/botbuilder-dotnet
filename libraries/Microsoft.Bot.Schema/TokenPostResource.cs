// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// Response schema sent back from Bot Framework Token Service required to initiate a user token direct post.
    /// </summary>
    public partial class TokenPostResource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TokenPostResource"/> class.
        /// </summary>
        public TokenPostResource()
        {
            CustomInit();
        }

        /// <summary>
        /// Gets or sets the shared access signature url used to directly post a token to Bot Framework Token Service.
        /// </summary>
        /// <value>The URI.</value>
        [JsonProperty(PropertyName = "sasUrl")]
#pragma warning disable CA1056 // Uri properties should not be strings
        public string SasUrl { get; set; }
#pragma warning restore CA1056 // Uri properties should not be strings

        partial void CustomInit();
    }
}
