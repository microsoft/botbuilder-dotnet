// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using Newtonsoft.Json;
    using System.Linq;

    /// <summary>
    /// O365 connector card action base
    /// </summary>
    public partial class O365ConnectorCardActionBase
    {
        /// <summary>
        /// Initializes a new instance of the O365ConnectorCardActionBase
        /// class.
        /// </summary>
        public O365ConnectorCardActionBase()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the O365ConnectorCardActionBase
        /// class.
        /// </summary>
        /// <param name="type">Type of the action. Possible values include:
        /// 'ViewAction', 'OpenUri', 'HttpPOST', 'ActionCard'</param>
        /// <param name="name">Name of the action that will be used as button
        /// title</param>
        /// <param name="id">Action Id</param>
        public O365ConnectorCardActionBase(string type = default(string), string name = default(string), string id = default(string))
        {
            Type = type;
            Name = name;
            Id = id;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets type of the action. Possible values include:
        /// 'ViewAction', 'OpenUri', 'HttpPOST', 'ActionCard'
        /// </summary>
        [JsonProperty(PropertyName = "@type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets name of the action that will be used as button title
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets action Id
        /// </summary>
        [JsonProperty(PropertyName = "@id")]
        public string Id { get; set; }

    }
}
