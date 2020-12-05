// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// O365 connector card action base.
    /// </summary>
    public partial class O365ConnectorCardActionBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="O365ConnectorCardActionBase"/> class.
        /// </summary>
        public O365ConnectorCardActionBase()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="O365ConnectorCardActionBase"/> class.
        /// </summary>
        /// <param name="type">Type of the action. Possible values include:
        /// 'ViewAction', 'OpenUri', 'HttpPOST', 'ActionCard'.</param>
        /// <param name="name">Name of the action that will be used as button
        /// title.</param>
        /// <param name="id">Action Id.</param>
        public O365ConnectorCardActionBase(string type = default(string), string name = default(string), string id = default(string))
        {
            Type = type;
            Name = name;
            Id = id;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets type of the action. Possible values include:
        /// 'ViewAction', 'OpenUri', 'HttpPOST', 'ActionCard'.
        /// </summary>
        /// <value>The type of the action.</value>
        [JsonProperty(PropertyName = "@type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets name of the action that will be used as button title.
        /// </summary>
        /// <value>The name of the action that will be used as a button title.</value>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets action ID.
        /// </summary>
        /// <value>The action ID.</value>
        [JsonProperty(PropertyName = "@id")]
        public string Id { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
