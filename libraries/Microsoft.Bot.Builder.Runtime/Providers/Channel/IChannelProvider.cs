// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Runtime.Providers.Channel
{
    /// <summary>
    /// Defines an interface for an implementation of <see cref="IProvider"/> that is primarily responsible for
    /// registering a <see cref="IChannelProvider"/> and associated dependencies with the application's service
    /// collection.
    /// </summary>
    internal interface IChannelProvider : IProvider
    {
    }
}
