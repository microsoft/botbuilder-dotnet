// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// O365 connector card ActionCard action.
    /// </summary>
    public class O365ConnectorCardActionCard : O365ConnectorCardActionBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="O365ConnectorCardActionCard"/> class.
        /// </summary>
        /// <param name="type">Type of the action. Possible values include:
        /// 'ViewAction', 'OpenUri', 'HttpPOST', 'ActionCard'.</param>
        /// <param name="name">Name of the action that will be used as button
        /// title.</param>
        /// <param name="id">Action Id.</param>
        /// <param name="inputs">Set of inputs contained in this ActionCard
        /// whose each item can be in any subtype of
        /// O365ConnectorCardInputBase.</param>
        /// <param name="actions">Set of actions contained in this ActionCard
        /// whose each item can be in any subtype of
        /// O365ConnectorCardActionBase except O365ConnectorCardActionCard, as
        /// nested ActionCard is forbidden.</param>
        public O365ConnectorCardActionCard(string type = default, string name = default, string id = default, IList<O365ConnectorCardInputBase> inputs = default, IList<O365ConnectorCardActionBase> actions = default)
            : base(type, name, id)
        {
            Inputs = inputs ?? new List<O365ConnectorCardInputBase>();
            Actions = actions ?? new List<O365ConnectorCardActionBase>();
        }

        /// <summary>
        /// Gets set of inputs contained in this ActionCard whose each
        /// item can be in any subtype of O365ConnectorCardInputBase.
        /// </summary>
        /// <value>The inputs contained in the ActionCard.</value>
        [JsonProperty(PropertyName = "inputs")]
        public IList<O365ConnectorCardInputBase> Inputs { get; private set; } = new List<O365ConnectorCardInputBase>();

        /// <summary>
        /// Gets set of actions contained in this ActionCard whose each
        /// item can be in any subtype of O365ConnectorCardActionBase except
        /// O365ConnectorCardActionCard, as nested ActionCard is forbidden.
        /// </summary>
        /// <value>The actions contained in this ActionCard.</value>
        [JsonProperty(PropertyName = "actions")]
        public IList<O365ConnectorCardActionBase> Actions { get; private set; } = new List<O365ConnectorCardActionBase>();
    }
}
