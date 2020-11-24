// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using Newtonsoft.Json;
    using System.Linq;

    /// <summary>
    /// O365 connector card OpenUri target
    /// </summary>
    public partial class O365ConnectorCardOpenUriTarget
    {
        /// <summary>
        /// Initializes a new instance of the O365ConnectorCardOpenUriTarget
        /// class.
        /// </summary>
        public O365ConnectorCardOpenUriTarget()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the O365ConnectorCardOpenUriTarget
        /// class.
        /// </summary>
        /// <param name="os">Target operating system. Possible values include:
        /// 'default', 'iOS', 'android', 'windows'</param>
        /// <param name="uri">Target url</param>
        public O365ConnectorCardOpenUriTarget(string os = default(string), string uri = default(string))
        {
            Os = os;
            Uri = uri;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets target operating system. Possible values include:
        /// 'default', 'iOS', 'android', 'windows'
        /// </summary>
        [JsonProperty(PropertyName = "os")]
        public string Os { get; set; }

        /// <summary>
        /// Gets or sets target url
        /// </summary>
        [JsonProperty(PropertyName = "uri")]
        public string Uri { get; set; }

    }
}
