// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Debugging.Identifiers
{
    internal sealed class IdentifierMutex<T> : IIdentifier<T>
    {
        private readonly object _gate = new object();
        private readonly IIdentifier<T> _inner;

        public IdentifierMutex(IIdentifier<T> inner)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        IEnumerable<T> IIdentifier<T>.Items
        {
            get
            {
                lock (_gate)
                {
                    return _inner.Items.ToList();
                }
            }
        }

        T IIdentifier<T>.this[ulong code]
        {
            get
            {
                lock (_gate)
                {
                    return _inner[code];
                }
            }
        }

        ulong IIdentifier<T>.this[T item]
        {
            get
            {
                lock (_gate)
                {
                    return _inner[item];
                }
            }
        }

        bool IIdentifier<T>.TryGetValue(ulong code, out T item)
        {
            lock (_gate)
            {
                return _inner.TryGetValue(code, out item);
            }
        }

        bool IIdentifier<T>.TryGetValue(T item, out ulong code)
        {
            lock (_gate)
            {
                return _inner.TryGetValue(item, out code);
            }
        }

        ulong IIdentifier<T>.Add(T item)
        {
            lock (_gate)
            {
                return _inner.Add(item);
            }
        }

        void IIdentifier<T>.Remove(T item)
        {
            lock (_gate)
            {
                _inner.Remove(item);
            }
        }

        void IIdentifier<T>.Clear()
        {
            lock (_gate)
            {
                _inner.Clear();
            }
        }

        IEnumerator<KeyValuePair<ulong, T>> IEnumerable<KeyValuePair<ulong, T>>.GetEnumerator()
        {
            lock (_gate)
            {
                return _inner.ToList().GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            lock (_gate)
            {
                return _inner.ToList().GetEnumerator();
            }
        }
    }
}
