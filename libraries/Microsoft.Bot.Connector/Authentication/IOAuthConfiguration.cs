using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// Configuration for OAuth client credential authentication.
    /// </summary>
    public interface IOAuthConfiguration
    {
        /// <summary>
        /// OAuth Authority for authentication.
        /// </summary>
        string Authority { get; set; }

        /// <summary>
        /// OAuth scope for authentication.
        /// </summary>
        string Scope { get; set; }
    }

    public class OAuthConfiguration : IOAuthConfiguration
    {
        public string Authority { get; set; }
        public string Scope { get; set; }
    }
}
