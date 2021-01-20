// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using Newtonsoft.Json;
    
    /// <summary>
    /// Invoke ('tab/submit') request value payload.
    /// </summary>
    public partial class TabSubmit
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TabSubmit"/> class.
        /// </summary>
        public TabSubmit()
        {
            CustomInit();
        }

        /// <summary>
        /// Gets or sets current tab entity request context.
        /// </summary>
        /// <value>
        /// Tab context for this <see cref="TabSubmit"/>.
        /// </value>
        [JsonProperty(PropertyName = "tabContext")]
        public TabEntityContext TabEntityContext { get; set; }

        /// <summary>
        /// Gets or sets current user context, i.e., the current theme.
        /// </summary>
        /// <value>
        /// Current user context, i.e., the current theme.
        /// </value>
        [JsonProperty(PropertyName = "context")]
        public TabContext Context { get; set; }

        /// <summary>
        /// Gets or sets user input data. Free payload containing properties of key-value pairs.
        /// </summary>
        /// <value>
        /// User input data. Free payload containing properties of key-value pairs.
        /// </value>
        [JsonProperty(PropertyName = "data")]
        public TabSubmitData Data { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
