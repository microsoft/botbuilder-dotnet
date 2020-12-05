// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// O365 connector card ActionCard action.
    /// </summary>
    public partial class O365ConnectorCardActionCard : O365ConnectorCardActionBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="O365ConnectorCardActionCard"/> class.
        /// </summary>
        public O365ConnectorCardActionCard()
        {
            CustomInit();
        }

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
        public O365ConnectorCardActionCard(string type = default(string), string name = default(string), string id = default(string), IList<O365ConnectorCardInputBase> inputs = default(IList<O365ConnectorCardInputBase>), IList<O365ConnectorCardActionBase> actions = default(IList<O365ConnectorCardActionBase>))
            : base(type, name, id)
        {
            Inputs = inputs;
            Actions = actions;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets set of inputs contained in this ActionCard whose each
        /// item can be in any subtype of O365ConnectorCardInputBase.
        /// </summary>
        /// <value>The inputs contained in the ActionCard.</value>
        [JsonProperty(PropertyName = "inputs")]
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking compat).
        public IList<O365ConnectorCardInputBase> Inputs { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// Gets or sets set of actions contained in this ActionCard whose each
        /// item can be in any subtype of O365ConnectorCardActionBase except
        /// O365ConnectorCardActionCard, as nested ActionCard is forbidden.
        /// </summary>
        /// <value>The actions contained in this ActionCard.</value>
        [JsonProperty(PropertyName = "actions")]
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking compat).
        public IList<O365ConnectorCardActionBase> Actions { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
