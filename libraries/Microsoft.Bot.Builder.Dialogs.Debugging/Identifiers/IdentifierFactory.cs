using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.Dialogs.Debugging
{
    public static class IdentifierFactory
    {
        public static IIdentifier<T> WithCache<T>(this IIdentifier<T> identifier, int count)
            => new IdentifierCache<T>(identifier, count);

        public static IIdentifier<T> WithMutex<T>(this IIdentifier<T> identifier)
            => new IdentifierMutex<T>(identifier);
    }
}
