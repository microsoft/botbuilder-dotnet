// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// O365 connector card ViewAction action.
    /// </summary>
    public class O365ConnectorCardViewAction : O365ConnectorCardActionBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="O365ConnectorCardViewAction"/> class.
        /// </summary>
        /// <param name="type">Type of the action. Possible values include:
        /// 'ViewAction', 'OpenUri', 'HttpPOST', 'ActionCard'.</param>
        /// <param name="name">Name of the action that will be used as button title.</param>
        /// <param name="id">Action Id.</param>
        /// <param name="target">Target urls, only the first url effective for card button.</param>
        public O365ConnectorCardViewAction(string type = default, string name = default, string id = default, IList<string> target = default)
            : base(type, name, id)
        {
            Target = target ?? new List<string>();
        }

        /// <summary>
        /// Gets target urls, only the first url effective for card button.
        /// </summary>
        /// <value>The target URLs.</value>
        [JsonProperty(PropertyName = "target")]
        public IList<string> Target { get; private set; } = new List<string>();
    }
}
