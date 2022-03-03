﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using Newtonsoft.Json;

    /// <summary>
    /// O365 connector card OpenUri target.
    /// </summary>
    public class O365ConnectorCardOpenUriTarget
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="O365ConnectorCardOpenUriTarget"/> class.
        /// </summary>
        /// <param name="os">Target operating system. Possible values include:
        /// 'default', 'iOS', 'android', 'windows'.</param>
        /// <param name="uri">Target url.</param>
        public O365ConnectorCardOpenUriTarget(string os = default, string uri = default)
        {
            Os = os;
            Uri = uri;
        }

        /// <summary>
        /// Gets or sets target operating system. Possible values include:
        /// 'default', 'iOS', 'android', 'windows'.
        /// </summary>
        /// <value>The target operating system.</value>
        [JsonProperty(PropertyName = "os")]
        public string Os { get; set; }

        /// <summary>
        /// Gets or sets target URL.
        /// </summary>
        /// <value>The target URL.</value>
        [JsonProperty(PropertyName = "uri")]
#pragma warning disable CA1056 // Uri properties should not be strings
        public string Uri { get; set; }
#pragma warning restore CA1056 // Uri properties should not be strings
    }
}
