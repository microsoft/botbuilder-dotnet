using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// Configuration for OAuth client credential authentication.
    /// </summary>
    public class OAuthConfiguration
    {
        /// <summary>
        /// OAuth Authority for authentication.
        /// </summary>
        public string Authority { get; set; }

        /// <summary>
        /// OAuth scope for authentication.
        /// </summary>
        public string Scope { get; set; }
    }
}
