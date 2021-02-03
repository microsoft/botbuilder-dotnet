// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// Teams channel account detailing user Azure Active Directory details.
    /// </summary>
    public partial class TeamsChannelAccount : ChannelAccount
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TeamsChannelAccount"/> class.
        /// </summary>
        public TeamsChannelAccount()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamsChannelAccount"/> class.
        /// </summary>
        /// <param name="id">Channel id for the user or bot on this channel.
        /// (Example: joe@smith.com, or @joesmith or 123456).</param>
        /// <param name="name">Display friendly name.</param>
        /// <param name="givenName">Given name part of the user name.</param>
        /// <param name="surname">Surname part of the user name.</param>
        /// <param name="email">Email Id of the user.</param>
        /// <param name="userPrincipalName">Unique user principal name.</param>
        /// <param name="tenantId">TenantId of the user.</param>
        /// <param name="userRole">UserRole of the user.</param>
        public TeamsChannelAccount(string id = default(string), string name = default(string), string givenName = default(string), string surname = default(string), string email = default(string), string userPrincipalName = default(string), string tenantId = default(string), string userRole = default(string))
            : base(id, name)
        {
            GivenName = givenName;
            Surname = surname;
            Email = email;
            UserPrincipalName = userPrincipalName;
            TenantId = tenantId;
            UserRole = userRole;
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamsChannelAccount"/> class.
        /// </summary>
        /// <param name="id">Channel id for the user or bot on this channel.
        /// (Example: joe@smith.com, or @joesmith or 123456).</param>
        /// <param name="name">Display friendly name.</param>
        /// <param name="givenName">Given name part of the user name.</param>
        /// <param name="surname">Surname part of the user name.</param>
        /// <param name="email">Email Id of the user.</param>
        /// <param name="userPrincipalName">Unique user principal name.</param>
        public TeamsChannelAccount(string id = default(string), string name = default(string), string givenName = default(string), string surname = default(string), string email = default(string), string userPrincipalName = default(string))
            : this(id, name, givenName, surname, email, userPrincipalName, string.Empty, string.Empty)
        {
        }

        /// <summary>
        /// Gets or sets given name part of the user name.
        /// </summary>
        /// <value>The given name part of the user name.</value>
        [JsonProperty(PropertyName = "givenName")]
        public string GivenName { get; set; }

        /// <summary>
        /// Gets or sets surname part of the user name.
        /// </summary>
        /// <value>The surname part of the user name.</value>
        [JsonProperty(PropertyName = "surname")]
        public string Surname { get; set; }

        /// <summary>
        /// Gets or sets email Id of the user.
        /// </summary>
        /// <value>The email ID of the user.</value>
        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets unique user principal name.
        /// </summary>
        /// <value>The unique user principal name.</value>
        [JsonProperty(PropertyName = "userPrincipalName")]
        public string UserPrincipalName { get; set; }

        /// <summary>
        /// Gets or sets the UserRole.
        /// </summary>
        /// <value>The user role.</value>
        [JsonProperty(PropertyName = "userRole")]
        public string UserRole { get; set; }

        /// <summary>
        /// Gets or sets the TenantId.
        /// </summary>
        /// <value>The tenant ID.</value>
        [JsonProperty(PropertyName = "tenantId")]
        public string TenantId { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}
