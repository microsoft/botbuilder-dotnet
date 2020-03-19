using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Bot.Builder.Dialogs.Debugging
{
    public sealed class IdentifierMutex<T> : IIdentifier<T>
    {
        private readonly IIdentifier<T> inner;
        private readonly object gate = new object();

        public IdentifierMutex(IIdentifier<T> inner)
        {
            this.inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        IEnumerable<T> IIdentifier<T>.Items
        {
            get
            {
                lock (this.gate)
                {
                    return this.inner.Items.ToList();
                }
            }
        }

        T IIdentifier<T>.this[ulong code]
        {
            get
            {
                lock (this.gate)
                {
                    return this.inner[code];
                }
            }
        }

        ulong IIdentifier<T>.this[T item]
        {
            get
            {
                lock (this.gate)
                {
                    return this.inner[item];
                }
            }
        }

        bool IIdentifier<T>.TryGetValue(ulong code, out T item)
        {
            lock (this.gate)
            {
                return this.inner.TryGetValue(code, out item);
            }
        }

        bool IIdentifier<T>.TryGetValue(T item, out ulong code)
        {
            lock (this.gate)
            {
                return this.inner.TryGetValue(item, out code);
            }
        }

        ulong IIdentifier<T>.Add(T item)
        {
            lock (this.gate)
            {
                return this.inner.Add(item);
            }
        }

        void IIdentifier<T>.Remove(T item)
        {
            lock (this.gate)
            {
                this.inner.Remove(item);
            }
        }

        void IIdentifier<T>.Clear()
        {
            lock (this.gate)
            {
                this.inner.Clear();
            }
        }

        IEnumerator<KeyValuePair<ulong, T>> IEnumerable<KeyValuePair<ulong, T>>.GetEnumerator()
        {
            lock (this.gate)
            {
                return this.inner.ToList().GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            lock (this.gate)
            {
                return this.inner.ToList().GetEnumerator();
            }
        }
    }
}
