// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Dialogs.Debugging.Identifiers
{
    internal interface IIdentifier<T> : IEnumerable<KeyValuePair<ulong, T>>
    {
        IEnumerable<T> Items
        {
            get;
        }

        T this[ulong code]
        {
            get;
        }

        ulong this[T item]
        {
            get;
        }

        bool TryGetValue(ulong code, out T item);

        bool TryGetValue(T item, out ulong code);

        void Clear();

        ulong Add(T item);

        void Remove(T item);
    }
}
