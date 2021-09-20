// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Streaming.Transport
{
    /// <summary>
    /// A factory that creates an instance of <see cref="IStreamingTransportServer"/>.
    /// </summary>
    public interface IStreamingTransportServerFactory
    {
        /// <summary>
        /// Creates an instance of <see cref="IStreamingTransportServer"/>.
        /// </summary>
        /// <param name="requestHandler">An instance of request handler to use.</param>
        /// <returns>An instance of <see cref="IStreamingTransportServer"/>.</returns>
        IStreamingTransportServer Create(RequestHandler requestHandler);
    }
}
