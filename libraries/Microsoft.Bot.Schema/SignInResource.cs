// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// A type containing information for single sign-on.
    /// </summary>
    public partial class SignInResource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SignInResource"/> class.
        /// </summary>
        public SignInResource()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SignInResource"/> class.
        /// </summary>
        /// <param name="signInLink">Sign-in link.</param>
        /// <param name="tokenExchangeResource">Token exchange resource.</param>
        public SignInResource(string signInLink = default(string), TokenExchangeResource tokenExchangeResource = default(TokenExchangeResource))
        {
            SignInLink = signInLink;
            TokenExchangeResource = tokenExchangeResource;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets the sign-in link.
        /// </summary>
        /// <value>The sign-in link.</value>
        [JsonProperty(PropertyName = "signInLink")]
        public string SignInLink { get; set; }

        /// <summary>
        /// Gets or sets additional properties that can be used for token exchange operations.
        /// </summary>
        /// <value>The additional properties can be used for token exchange operations.</value>
        [JsonProperty(PropertyName = "tokenExchangeResource")]
        public TokenExchangeResource TokenExchangeResource { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
