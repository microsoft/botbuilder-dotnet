using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.Dialogs.Debugging
{
    public sealed class IdentifierCache<T> : IIdentifier<T>
    {
        private readonly IIdentifier<T> inner;
        private readonly int count;

        private readonly Queue<T> queue = new Queue<T>();

        public IdentifierCache(IIdentifier<T> inner, int count)
        {
            this.inner = inner ?? throw new ArgumentNullException(nameof(inner));
            this.count = count;
        }

        IEnumerable<T> IIdentifier<T>.Items => this.inner.Items;

        T IIdentifier<T>.this[ulong code] => this.inner[code];

        ulong IIdentifier<T>.this[T item] => this.inner[item];

        bool IIdentifier<T>.TryGetValue(ulong code, out T item) => this.inner.TryGetValue(code, out item);

        bool IIdentifier<T>.TryGetValue(T item, out ulong code) => this.inner.TryGetValue(item, out code);

        ulong IIdentifier<T>.Add(T item)
        {
            if (this.inner.TryGetValue(item, out var code))
            {
                return code;
            }

            while (this.queue.Count >= this.count)
            {
                var head = this.queue.Dequeue();
                this.inner.Remove(head);
            }

            return this.inner.Add(item);
        }

        void IIdentifier<T>.Remove(T item)
             => this.inner.Remove(item);

        IEnumerator<KeyValuePair<ulong, T>> IEnumerable<KeyValuePair<ulong, T>>.GetEnumerator() => this.inner.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.inner.GetEnumerator();
    }
}
