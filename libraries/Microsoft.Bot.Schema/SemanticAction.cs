// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a reference to a programmatic action.
    /// </summary>
    public partial class SemanticAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SemanticAction"/> class.
        /// </summary>
        public SemanticAction()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SemanticAction"/> class.
        /// </summary>
        /// <param name="id">ID of this action.</param>
        /// <param name="entities">Entities associated with this action.</param>
        public SemanticAction(string id = default, IDictionary<string, Entity> entities = default)
        {
            Id = id;
            Entities = entities ?? new Dictionary<string, Entity>();
            CustomInit();
        }

        /// <summary>
        /// Gets or sets ID of this action.
        /// </summary>
        /// <value>The ID of this action card.</value>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets entities associated with this action.
        /// </summary>
        /// <value>The entities associated with this action.</value>
        [JsonProperty(PropertyName = "entities")]
        public IDictionary<string, Entity> Entities { get; private set; } = new Dictionary<string, Entity>();

        /// <summary>
        /// Gets or sets state of this action. Allowed values: `start`,
        /// `continue`, `done`.
        /// </summary>
        /// <value>The state of this action.</value>
        [JsonProperty(PropertyName = "state")]
        public string State { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
