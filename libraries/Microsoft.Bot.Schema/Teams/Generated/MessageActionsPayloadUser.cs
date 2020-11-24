// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using Newtonsoft.Json;
    using System.Linq;

    /// <summary>
    /// Represents a user entity.
    /// </summary>
    public partial class MessageActionsPayloadUser
    {
        /// <summary>
        /// Initializes a new instance of the MessageActionsPayloadUser class.
        /// </summary>
        public MessageActionsPayloadUser()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the MessageActionsPayloadUser class.
        /// </summary>
        /// <param name="userIdentityType">The identity type of the user.
        /// Possible values include: 'aadUser', 'onPremiseAadUser',
        /// 'anonymousGuest', 'federatedUser'</param>
        /// <param name="id">The id of the user.</param>
        /// <param name="displayName">The plaintext display name of the
        /// user.</param>
        public MessageActionsPayloadUser(string userIdentityType = default(string), string id = default(string), string displayName = default(string))
        {
            UserIdentityType = userIdentityType;
            Id = id;
            DisplayName = displayName;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets the identity type of the user. Possible values
        /// include: 'aadUser', 'onPremiseAadUser', 'anonymousGuest',
        /// 'federatedUser'
        /// </summary>
        [JsonProperty(PropertyName = "userIdentityType")]
        public string UserIdentityType { get; set; }

        /// <summary>
        /// Gets or sets the id of the user.
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the plaintext display name of the user.
        /// </summary>
        [JsonProperty(PropertyName = "displayName")]
        public string DisplayName { get; set; }

    }
}
