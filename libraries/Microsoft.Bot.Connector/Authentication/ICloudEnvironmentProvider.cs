// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// CloudEnvironmentProvider interface. This interface allows Bots to provide their own
    /// implementation for the authentication when using the Bot Framework Protocol.
    /// </summary>
    public interface ICloudEnvironmentProvider
    {
        /// <summary>
        /// Gets the cloud environment to be used.
        /// </summary>
        /// <returns>The cloud environment property.</returns>
        Task<ICloudEnvironment> GetCloudEnvironmentAsync();
    }
}
