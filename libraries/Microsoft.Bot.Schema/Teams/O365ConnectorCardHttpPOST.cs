// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using Newtonsoft.Json;

    /// <summary>
    /// O365 connector card HttpPOST action.
    /// </summary>
    public class O365ConnectorCardHttpPOST : O365ConnectorCardActionBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="O365ConnectorCardHttpPOST"/> class.
        /// </summary>
        /// <param name="type">Type of the action. Possible values include:
        /// 'ViewAction', 'OpenUri', 'HttpPOST', 'ActionCard'.</param>
        /// <param name="name">Name of the action that will be used as button title.</param>
        /// <param name="id">Action Id.</param>
        /// <param name="body">Content to be posted back to bots via invoke.</param>
        public O365ConnectorCardHttpPOST(string type = default, string name = default, string id = default, string body = default)
            : base(type, name, id)
        {
            Body = body;
        }

        /// <summary>
        /// Gets or sets content to be posted back to bots via invoke.
        /// </summary>
        /// <value>The content to be posted back to bots via invoke.</value>
        [JsonProperty(PropertyName = "body")]
        public string Body { get; set; }
    }
}
