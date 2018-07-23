// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;

namespace Microsoft.Bot.Connector.Authentication
{
    /// <summary>
    /// ChannelProvider interface. This interface allows Bots to provide their own
    /// implementation for the configuration parameters to connect to a Bot 
    /// Framework channel service
    /// </summary>
    public interface IChannelProvider
    {
        /// <summary>
        /// Determines if the particular auth header is from this channel
        /// </summary>
        /// <param name="authHeader">The complete auth header value</param>
        /// <returns>True if this auth header is from this channel; false if it is not</returns>
        Task<bool> IsTokenFromChannel(string authHeader);

        /// <summary>
        /// Gets the issuer for this channel provider
        /// </summary>
        /// <returns>The issuer for the channel provider</returns>
        Task<string> GetIssuerAsync();
        
        /// <summary>
        /// Returns the Open ID Metadata Url for this channel provider
        /// </summary>
        /// <returns>The Open ID Metadata Url for this channel provider</returns>
        Task<string> GetOpenIdMetadataUrlAsync();
    }
}
