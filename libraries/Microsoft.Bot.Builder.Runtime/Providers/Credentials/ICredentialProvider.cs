// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Runtime.Providers.Credentials
{
    /// <summary>
    /// Defines an interface for an implementation of <see cref="IProvider"/> that is primarily responsible for
    /// registering a <see cref="ICredentialProvider"/> and associated dependencies with the application's service
    /// collection.
    /// </summary>
    public interface ICredentialProvider : IProvider
    {
    }
}
