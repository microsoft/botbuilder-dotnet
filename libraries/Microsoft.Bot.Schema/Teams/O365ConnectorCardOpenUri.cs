// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// O365 connector card OpenUri action.
    /// </summary>
    public class O365ConnectorCardOpenUri : O365ConnectorCardActionBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="O365ConnectorCardOpenUri"/> class.
        /// </summary>
        /// <param name="type">Type of the action. Possible values include:
        /// 'ViewAction', 'OpenUri', 'HttpPOST', 'ActionCard'.</param>
        /// <param name="name">Name of the action that will be used as button
        /// title.</param>
        /// <param name="id">Action Id.</param>
        /// <param name="targets">Target os / urls.</param>
        public O365ConnectorCardOpenUri(string type = default, string name = default, string id = default, IList<O365ConnectorCardOpenUriTarget> targets = default)
            : base(type, name, id)
        {
            Targets = targets ?? new List<O365ConnectorCardOpenUriTarget>();
        }

        /// <summary>
        /// Gets target OS/URLs.
        /// </summary>
        /// <value>The target OS/URLs.</value>
        [JsonProperty(PropertyName = "targets")]
        public IList<O365ConnectorCardOpenUriTarget> Targets { get; private set; } = new List<O365ConnectorCardOpenUriTarget>();
    }
}
