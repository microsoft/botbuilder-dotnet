// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// General configuration settings for authentication.
    /// </summary>
    /// <remarks>
    /// Note that this is explicitly a class and not an interface,
    /// since interfaces don't support default values, after the initial release any change would break backwards compatibility.
    /// </remarks>
    public class AuthenticationConfiguration
    {
        public string[] RequiredEndorsements { get; set; } = new string[] { };
    }
}
