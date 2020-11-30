// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// A card representing a request to perform a sign in via OAuth.
    /// </summary>
    public partial class OAuthCard
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OAuthCard"/> class.
        /// </summary>
        public OAuthCard()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OAuthCard"/> class.
        /// </summary>
        /// <param name="text">Text for signin request.</param>
        /// <param name="connectionName">The name of the registered
        /// connection.</param>
        /// <param name="buttons">Action to use to perform signin.</param>
        public OAuthCard(string text = default(string), string connectionName = default(string), IList<CardAction> buttons = default(IList<CardAction>))
        {
            Text = text;
            ConnectionName = connectionName;
            Buttons = buttons;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets text for signin request.
        /// </summary>
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the name of the registered connection.
        /// </summary>
        [JsonProperty(PropertyName = "connectionName")]
        public string ConnectionName { get; set; }

        /// <summary>
        /// Gets or sets the resource to try to perform token exchange with.
        /// </summary>
        public TokenExchangeResource TokenExchangeResource { get; set; }

        /// <summary>
        /// Gets or sets action to use to perform signin.
        /// </summary>
        [JsonProperty(PropertyName = "buttons")]
        public IList<CardAction> Buttons { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
