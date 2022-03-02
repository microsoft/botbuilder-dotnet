// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Microsoft.Bot.Connector.Schema
{
    /// <summary>
    /// A type containing information for single sign-on.
    /// </summary>
    public class SignInResource
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
        public SignInResource(string signInLink = default, TokenExchangeResource tokenExchangeResource = default)
        {
            SignInLink = signInLink;
            TokenExchangeResource = tokenExchangeResource;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets the sign-in link.
        /// </summary>
        /// <value>The sign-in link.</value>
        [JsonPropertyName("signInLink")]
        public string SignInLink { get; set; }

        /// <summary>
        /// Gets or sets additional properties that can be used for token exchange operations.
        /// </summary>
        /// <value>The additional properties can be used for token exchange operations.</value>
        [JsonPropertyName("tokenExchangeResource")]
        public TokenExchangeResource TokenExchangeResource { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        private void CustomInit()
        {
            throw new System.NotImplementedException();
        }
    }
}
