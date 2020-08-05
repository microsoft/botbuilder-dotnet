// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Dialogs.Debugging.Identifiers
{
    internal sealed class IdentifierCache<T> : IIdentifier<T>
    {
        private readonly int _count;
        private readonly IIdentifier<T> _inner;

        private readonly Queue<T> _queue = new Queue<T>();

        public IdentifierCache(IIdentifier<T> inner, int count)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _count = count;
        }

        IEnumerable<T> IIdentifier<T>.Items => _inner.Items;

        T IIdentifier<T>.this[ulong code] => _inner[code];

        ulong IIdentifier<T>.this[T item] => _inner[item];

        bool IIdentifier<T>.TryGetValue(ulong code, out T item) => _inner.TryGetValue(code, out item);

        bool IIdentifier<T>.TryGetValue(T item, out ulong code) => _inner.TryGetValue(item, out code);

        void IIdentifier<T>.Clear()
        {
            _inner.Clear();
            _queue.Clear();
        }

        ulong IIdentifier<T>.Add(T item)
        {
            if (_inner.TryGetValue(item, out var code))
            {
                return code;
            }

            while (_queue.Count >= _count)
            {
                var head = _queue.Dequeue();
                _inner.Remove(head);
            }

            return _inner.Add(item);
        }

        void IIdentifier<T>.Remove(T item)
            => _inner.Remove(item);

        IEnumerator<KeyValuePair<ulong, T>> IEnumerable<KeyValuePair<ulong, T>>.GetEnumerator() => _inner.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _inner.GetEnumerator();
    }
}
