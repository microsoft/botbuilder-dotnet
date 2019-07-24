// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Rest;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// ServiceClientCredentialProvider interface. This interface allows Bots to provide their own
    /// proof of service identity for the purpose of making service calls from the bot to 
    /// channels. The implementor should return ServiceClientCredentails from GetCredentials 
    /// method.
    /// </summary>
    public interface IServiceClientCredentialProvider : ICredentialProvider
    {
        /// <summary>
        /// Get the credential necessary to provide identity/authorization to use.
        /// </summary>
        /// <returns>ServiceClientCedentials.</returns>
        ServiceClientCredentials GetCredentials();
    }
}
