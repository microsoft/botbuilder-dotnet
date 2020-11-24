// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    using Newtonsoft.Json;
    using System.Linq;

    /// <summary>
    /// A type containing information for single sign-on.
    /// </summary>
    public partial class SignInResource
    {
        /// <summary>
        /// Initializes a new instance of the SignInUrlResponse class.
        /// </summary>
        public SignInResource()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the SignInUrlResponse class.
        /// </summary>
        public SignInResource(string signInLink = default(string), TokenExchangeResource tokenExchangeResource = default(TokenExchangeResource))
        {
            SignInLink = signInLink;
            TokenExchangeResource = tokenExchangeResource;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// The sign-in link
        /// </summary>
        [JsonProperty(PropertyName = "signInLink")]
        public string SignInLink { get; set; }

        /// <summary>
        /// Additional properties that cna be used for token exchange operations
        /// </summary>
        [JsonProperty(PropertyName = "tokenExchangeResource")]
        public TokenExchangeResource TokenExchangeResource { get; set; }

    }
}
