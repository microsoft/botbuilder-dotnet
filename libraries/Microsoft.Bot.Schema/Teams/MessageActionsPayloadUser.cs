﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a user entity.
    /// </summary>
    public class MessageActionsPayloadUser
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageActionsPayloadUser"/> class.
        /// </summary>
        /// <param name="userIdentityType">The identity type of the user.
        /// Possible values include: 'aadUser', 'onPremiseAadUser',
        /// 'anonymousGuest', 'federatedUser'.</param>
        /// <param name="id">The id of the user.</param>
        /// <param name="displayName">The plaintext display name of the
        /// user.</param>
        public MessageActionsPayloadUser(string userIdentityType = default, string id = default, string displayName = default)
        {
            UserIdentityType = userIdentityType;
            Id = id;
            DisplayName = displayName;
        }

        /// <summary>
        /// Gets or sets the identity type of the user. Possible values
        /// include: 'aadUser', 'onPremiseAadUser', 'anonymousGuest',
        /// 'federatedUser'.
        /// </summary>
        /// <value>The identity type of the user.</value>
        [JsonProperty(PropertyName = "userIdentityType")]
        public string UserIdentityType { get; set; }

        /// <summary>
        /// Gets or sets the id of the user.
        /// </summary>
        /// <value>The user ID.</value>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the plaintext display name of the user.
        /// </summary>
        /// <value>The plaintext display name of the user.</value>
        [JsonProperty(PropertyName = "displayName")]
        public string DisplayName { get; set; }
    }
}
