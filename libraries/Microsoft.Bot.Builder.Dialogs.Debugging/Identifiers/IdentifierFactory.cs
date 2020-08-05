// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Dialogs.Debugging.Identifiers
{
    internal static class IdentifierFactory
    {
        public static IIdentifier<T> WithCache<T>(this IIdentifier<T> identifier, int count)
            => new IdentifierCache<T>(identifier, count);

        public static IIdentifier<T> WithMutex<T>(this IIdentifier<T> identifier)
            => new IdentifierMutex<T>(identifier);
    }
}
